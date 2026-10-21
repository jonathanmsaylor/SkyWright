using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;
namespace Skywright {
public sealed class AetherSystem {
 public static AetherSystem Current{get;private set;}
 public readonly AetherConfig config;public AetherState state;public string Prompt="";
 readonly IslandField field=Resources.Load<IslandField>("CompactIsland/Field");
 readonly SkywrightGame game;readonly Action changed;readonly Action<string> log;
 class Bubble {public AetherPoint definition;public Transform view;public Vector3 velocity;public bool pulling;public float gather,scale,lost;}
 readonly List<Bubble> bubbles=new();readonly LineRenderer[] currentLines=new LineRenderer[4];readonly Transform[] feeding=new Transform[18];readonly LineRenderer[] updraft=new LineRenderer[5];readonly LineRenderer[] gates=new LineRenderer[2];BuiltPiece fixedAnchor;float offering=1,spin;Vector3 feedStart,feedEnd;Transform root;float clock,expansion=1;Vector3 oldRadii,oldCenter;
 public AetherSystem(SkywrightGame game,Action changed,Action<string> log){this.game=game;this.changed=changed;this.log=log;config=Resources.Load<AetherConfig>("CompactIsland/Aether");if(!config)throw new IOException("Missing authored Aether catalog");Current=this;fixedAnchor=Kit.Create("wind-anchor",config.anchorPosition,Quaternion.identity,null);fixedAnchor.id=TutorialIsland.AnchorId;fixedAnchor.name="Fixed island wind anchor";}
 public static void Validate(AetherState s){
  if(s==null||s.version!=1||s.balance<0||s.balance>1000000||s.tier<0||s.tier>3||s.collected==null)throw new IOException("Invalid Aether progression; current world kept.");
  var config=Resources.Load<AetherConfig>("CompactIsland/Aether");if(!config)throw new IOException("Aether catalog missing.");
  var known=config.bubbles.Select(b=>b.id).ToHashSet();var found=new HashSet<string>();foreach(var id in s.collected)if(!known.Contains(id)||!found.Add(id))throw new IOException("Invalid collected Aether identity.");
  int spent=0;for(int i=1;i<=s.tier;i++)spent+=config.stages[i].aether;
  if(s.balance+spent!=s.collected.Count)throw new IOException("Aether accounting mismatch.");
 }
 public void Restore(AetherState saved){
  Validate(saved);state=JsonUtility.FromJson<AetherState>(JsonUtility.ToJson(saved));if(root){root.gameObject.SetActive(false);UnityEngine.Object.Destroy(root.gameObject);}bubbles.Clear();
  root=new GameObject("Aether island progression").transform;clock=0;expansion=offering=1;state.primaryAnchor=TutorialIsland.AnchorId;
  foreach(var d in config.bubbles){if(state.collected.Contains(d.id))continue;var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Aether bubble";g.layer=2;g.transform.SetParent(root,false);g.transform.position=d.position;g.transform.localScale=Vector3.one*d.size;var c=g.GetComponent<Collider>();c.enabled=false;UnityEngine.Object.Destroy(c);var r=g.GetComponent<Renderer>();r.sharedMaterial=config.bubble;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;bubbles.Add(new Bubble{definition=d,view=g.transform,scale=1});}
  for(int i=0;i<feeding.Length;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Aether offering";g.layer=2;g.transform.SetParent(root);var col=g.GetComponent<Collider>();col.enabled=false;UnityEngine.Object.Destroy(col);var mr=g.GetComponent<Renderer>();mr.sharedMaterial=config.bubble;mr.shadowCastingMode=ShadowCastingMode.Off;g.SetActive(false);feeding[i]=g.transform;}
  for(int i=0;i<currentLines.Length;i++){var g=new GameObject("Awakened sky current");g.layer=2;g.transform.SetParent(root);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=config.ribbon;l.positionCount=96;l.numCapVertices=4;l.widthCurve=new AnimationCurve(new Keyframe(0,0),new Keyframe(.2f,.8f),new Keyframe(.75f,1),new Keyframe(1,0));l.widthMultiplier=.7f;l.shadowCastingMode=ShadowCastingMode.Off;l.enabled=state.tier==3;currentLines[i]=l;}
  for(int i=0;i<gates.Length;i++){var g=new GameObject("Wind current entrance");g.layer=2;g.transform.SetParent(root);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=config.ribbon;l.positionCount=65;l.widthMultiplier=.10f;l.shadowCastingMode=ShadowCastingMode.Off;gates[i]=l;}
  for(int i=0;i<updraft.Length;i++){var g=new GameObject("Anchor upward wind");g.layer=2;g.transform.SetParent(root);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=config.ribbon;l.positionCount=96;l.widthCurve=new AnimationCurve(new Keyframe(0,0),new Keyframe(.15f,1),new Keyframe(.8f,.55f),new Keyframe(1,0));l.widthMultiplier=.25f;l.shadowCastingMode=ShadowCastingMode.Off;updraft[i]=l;}

 }
 public BuiltPiece Primary()=>fixedAnchor;
 public bool IsPrimary(string id){var p=Primary();return p&&p.id==id;}
 public AnchorStage Stage=>config.stages[state.tier];
 public Vector3 Radii=>config.islandRadii*((state.tier+1)/4f);
 public Vector3 Center(Vector3 anchor)=>config.islandCenter;
 public Vector3 EffectiveRadii=>Vector3.Lerp(oldRadii,Radii,Mathf.SmoothStep(0,1,expansion));
 public Vector3 EffectiveCenter(Vector3 anchor)=>Vector3.Lerp(oldCenter,Center(anchor),Mathf.SmoothStep(0,1,expansion));
 public bool Contains(Vector3 anchor,Vector3 player){var r=EffectiveRadii;var d=player-EffectiveCenter(anchor);return (field?field.LowerContains(d,r):d.y>=0)&&d.x*d.x/(r.x*r.x)+d.y*d.y/(r.y*r.y)+d.z*d.z/(r.z*r.z)<=1;}
 public BuiltPiece AimedAnchor(){
  var ray=game.cam.ViewportPointToRay(new Vector3(.5f,.5f,0));if(Physics.Raycast(ray,out var hit,4.5f,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore)){var b=hit.collider.GetComponentInParent<BuiltPiece>();if(b&&b.type=="wind-anchor")return b;}return null;
 }
 public string CostText {get{if(state.tier==3)return "Island attuned";var n=config.stages[state.tier+1];return n.aether+" Aether  •  "+n.wood+" Wood  •  "+n.stone+" Stone";}}
 public bool Upgrade(BuiltPiece anchor){
  if(!anchor||!IsPrimary(anchor.id)){game.Notify("This anchor is dormant. Your active anchor carries the island field.");return false;}
  if(offering<1||expansion<1){game.Notify("The anchor is still gathering its wind.");return false;}
  if(state.tier>=3){game.Notify("The island is attuned.");return false;}
  var cost=config.stages[state.tier+1];var earth=game.HarvestState;
  if(state.balance<cost.aether||earth.wood<cost.wood||earth.stone<cost.stone){game.Notify("More resources needed: "+CostText);log("ANCHOR_INSUFFICIENT tier="+state.tier);return false;}
  oldRadii=EffectiveRadii;oldCenter=EffectiveCenter(anchor.transform.position);
  state.balance-=cost.aether;earth.wood-=cost.wood;earth.stone-=cost.stone;state.tier++;expansion=0;offering=0;feedStart=game.cam.transform.position+game.cam.transform.forward*.6f;feedEnd=anchor.transform.position+Vector3.up*.9f;changed();game.PlayPickup();
  log("ANCHOR_UPGRADE tier="+state.tier+" aether="+state.balance+" wood="+earth.wood+" stone="+earth.stone);
  game.Notify(state.tier==3?"Island attuned. A distant current has awakened.":Stage.name+" restored");
  return true;
 }
 const int Mask=~((1<<2)|(1<<6));
 bool Visible(Vector3 from,Vector3 to)=>!Physics.Linecast(from,to,Mask,QueryTriggerInteraction.Ignore);
 public void Interrupt(){
  foreach(var b in bubbles)if(b.pulling){b.pulling=false;b.velocity=Vector3.zero;b.gather=0;b.lost=0;b.scale=0;b.view.position=b.definition.position;b.view.localScale=Vector3.zero;}
 }
 public AetherState Snapshot(){Interrupt();return JsonUtility.FromJson<AetherState>(JsonUtility.ToJson(state));}
 public void Tick(float dt,bool suction){
  clock+=dt;var primary=Primary();Prompt=AimedAnchor()?"E  Wind anchor":TravelReady&&Mathf.Min(Vector3.Distance(game.controller.transform.position,config.routeStart),Vector3.Distance(game.controller.transform.position,config.routeEnd))<12?"Enter the wind ring to travel":"";
  offering=Mathf.Min(1,offering+dt/config.offeringDuration);
  if(offering>=1)expansion=Mathf.Min(1,expansion+dt/config.expansionDuration);
  for(int i=0;i<feeding.Length;i++){
   float u=Mathf.Clamp01(offering*1.5f-i*.025f);var f=feeding[i];f.gameObject.SetActive(offering<1&&u>0&&u<1);
   float eased=Mathf.SmoothStep(0,1,u);f.position=Vector3.Lerp(feedStart,feedEnd,eased)+Vector3.up*Mathf.Sin(u*Mathf.PI)*(.6f+i*.018f)+new Vector3(Mathf.Sin(u*9+i),0,Mathf.Cos(u*9+i))*.25f*Mathf.Sin(u*Mathf.PI);
   f.localScale=Vector3.one*(.07f+.06f*Mathf.Sin(u*Mathf.PI));
  }
  spin+=dt*Mathf.Lerp(8,210,state.tier/3f);
  if(primary){var visuals=primary.transform.Find("Visuals");if(visuals)foreach(Transform part in visuals)if(part.name.Contains("ring")||part.name=="Crystal")part.localRotation=Quaternion.Euler(0,spin*(part.name=="Crystal"?1.1f:1),part.name=="Crystal"?0:20);}
  for(int i=0;i<updraft.Length;i++){
   var l=updraft[i];l.enabled=state.tier==3&&expansion>=1;if(!l.enabled)continue;
   for(int j=0;j<96;j++){float u=j/95f,a=u*15-clock*2+i*Mathf.PI*2/5;float radius=.5f+Mathf.Sin(u*Mathf.PI)*3;
    l.SetPosition(j,config.anchorPosition+new Vector3(Mathf.Cos(a)*radius,1+u*(config.islandRadii.y-1+config.islandCenter.y),Mathf.Sin(a)*radius));}
  }
  var origin=game.cam.transform.position;var forward=game.cam.transform.forward;var socket=origin+forward*.72f+game.cam.transform.right*.20f-game.cam.transform.up*.19f;int incoming=0;
  foreach(var b in bubbles){
   if(!b.view||!b.view.gameObject.activeSelf)continue;
   var delta=b.view.position-origin;float distance=delta.magnitude;float angle=Vector3.Angle(forward,delta);
   bool caught=suctionsafe(suction,distance,angle,b.pulling)&&Visible(origin,b.view.position);
   if(caught&&!b.pulling){b.pulling=true;b.gather=0;b.velocity=Vector3.zero;log("AETHER_CATCH "+b.definition.id);}
   if(b.pulling){
    incoming++;if(!caught){b.lost+=dt;if(b.lost>.65f){b.pulling=false;b.gather=0;b.velocity=Vector3.zero;b.scale=0;b.view.position=b.definition.position;continue;}}else b.lost=0;
    b.gather+=dt;var desired=socket-b.view.position;float remaining=desired.magnitude;
    if(b.gather>.12f&&remaining<1.05f&&Visible(b.view.position,socket)){state.collected.Add(b.definition.id);state.balance++;b.view.gameObject.SetActive(false);b.pulling=false;changed();game.PlayPickup();log("AETHER_COLLECT "+b.definition.id+" balance="+state.balance);continue;}
    float speed=Mathf.Min(config.bubbleSpeed,4+b.gather*18,Mathf.Max(5,remaining*8));
    var tangent=game.cam.transform.right*Mathf.Sin(clock*7+b.definition.position.x)*.23f+game.cam.transform.up*Mathf.Cos(clock*7+b.definition.position.x)*.23f;
    var direction=(desired+tangent*Mathf.Clamp01(remaining-1)).normalized;
    b.velocity=Vector3.MoveTowards(b.velocity,direction*speed,dt*45);var step=b.velocity*dt;
    if(step.magnitude>remaining)step=step.normalized*remaining;
    if(step.sqrMagnitude>0&&Physics.SphereCast(b.view.position,.10f,step.normalized,out var hit,step.magnitude+.015f,Mask,QueryTriggerInteraction.Ignore)){step=step.normalized*Mathf.Max(0,hit.distance-.015f);b.velocity=Vector3.zero;}
    b.view.position+=step;
   }else b.view.position=b.definition.position+Vector3.up*Mathf.Sin(clock*1.35f+b.definition.position.x)*.10f;
   b.view.GetComponent<Renderer>().SetPropertyBlock(ShimmerBlock(game.AirEquipped&&distance<=config.suctionRange&&angle<=config.suctionCone&&Visible(origin,b.view.position), distance,angle));
   b.scale=Mathf.MoveTowards(b.scale,1,dt*4);float stretch=b.pulling?1+Mathf.Sin(Mathf.Clamp01(b.gather/.16f)*Mathf.PI)*.3f:1;
   b.view.localScale=Vector3.Scale(new Vector3(1/stretch,stretch,1/stretch),Vector3.one*b.definition.size*b.scale);
  }
  for(int i=0;i<gates.Length;i++){var line=gates[i];line.enabled=TravelReady;if(!line.enabled)continue;var at=i==0?config.routeStart:config.routeEnd;var direction=(config.routeEnd-config.routeStart).normalized;var q=Quaternion.LookRotation(direction);for(int j=0;j<65;j++){float a=j*Mathf.PI*2/64;line.SetPosition(j,at+Vector3.up*.9f+q*new Vector3(Mathf.Cos(a)*1.8f,Mathf.Sin(a)*1.8f,Mathf.Sin(a*3-clock*2)*.1f));}}
  for(int i=0;i<currentLines.Length;i++){
   var line=currentLines[i];line.enabled=state.tier==3&&expansion>=1;if(!line.enabled)continue;
   for(int j=0;j<96;j++){float u=j/95f;var p=TutorialIsland.RoutePoint(config,u);p+=new Vector3(0,Mathf.Sin(u*10-clock*.7f+i*1.57f),Mathf.Cos(u*10-clock*.7f+i*1.57f))*(1+Mathf.Sin(u*Mathf.PI)*2);line.SetPosition(j,p);}
  }
 }
 readonly MaterialPropertyBlock bubbleBlock=new MaterialPropertyBlock();
 MaterialPropertyBlock ShimmerBlock(bool equipped,float distance,float angle){bubbleBlock.SetFloat("_Shimmer",equipped&&distance<=config.suctionRange&&angle<=config.suctionCone?1:0);return bubbleBlock;}
 internal void TraceWind(AirEffects fx){foreach(var b in bubbles)if(b.pulling&&b.view&&b.view.gameObject.activeSelf)fx.Affected(b.view.position,b.definition.size*.6f);}
 public bool TravelReady=>state.tier==3&&expansion>=1;
 bool suctionsafe(bool active,float distance,float angle,bool latched)=>active&&distance<=config.suctionRange+(latched?4:0)&&angle<=(latched?config.releaseCone:config.suctionCone);
 public AetherObservation Observe(){var o=new AetherObservation{state=state,aimedAnchor=AimedAnchor()?AimedAnchor().id:""};foreach(var b in bubbles)if(b.view&&b.view.gameObject.activeSelf){o.remaining.Add(new AetherPoint{id=b.definition.id,position=b.view.position,size=b.definition.size});if(b.pulling)o.inTransit++;}return o;}
}
}

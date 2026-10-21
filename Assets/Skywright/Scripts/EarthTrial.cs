using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Skywright {
public partial class EarthTrial {
 public static readonly Vector3[] Locations={new Vector3(-4,0,-3),new Vector3(4,0,-2),new Vector3(-8,0,3),new Vector3(0,0,4),new Vector3(8,0,4),new Vector3(-12,0,8),new Vector3(-5,0,10),new Vector3(4,0,11),new Vector3(11,0,10),new Vector3(14,0,-2),new Vector3(-1,0,-7),new Vector3(3,0,-8),new Vector3(-3,0,-10),new Vector3(7,0,-7),new Vector3(-10,0,-6)};
 public static string Kind(int i)=>i<10?"tree":i<13?"pile":"outcrop";
 public static string Id(int i)=>"earth-trial-"+Kind(i)+"-"+i;
 public EarthConfig config;public EarthState state;public bool equipped;
 public string hint="Tab  Equip Earthcasting",targetId="",phase="Idle";public bool targetValid;
 public bool MarkVisible=>marker&&marker.gameObject.activeSelf;
 enum StoneMode { Idle, Preparing, Prepared, Guided, Rebounding, Waiting, Striking, Returning, Catching, Blocked, Dismissing, KnockedAway }
 StoneMode mode;public bool Busy=>mode!=StoneMode.Idle&&mode!=StoneMode.Prepared;
 const int SpellMask=~((1<<2)|(1<<6));float StoneRadius=>config.stoneRadius;
 Vector3 previousSocket,socketVelocity,catchOffset,lastContactNormal,lastContactCenter;bool socketObserved,compactSummon,repeatArmed;Quaternion heldRotation=Quaternion.identity,catchRotation;
 Transform boundTransform;Collider boundCollider;AudioClip ding;
 Transform holdSocket;TrailRenderer trail;Vector3 velocity;float returnAge,returnTravel,holdClock;bool holdClear;
 Vector3 localNormal,waitPoint,strikeStart;float actionClock,blockedClock;int bounces;
 public string BoundTarget=>castNode?castNode.state.id:boundTransform?boundTransform.name:"";public int BounceCount=>bounces;public int StoneCount=>projectile?1:0;
 public bool externalInput;public bool managedCrates;
 public float collectDistance=-1;
 public readonly List<EarthNode> nodes=new List<EarthNode>();
 readonly SkywrightGame game;readonly Camera cam;readonly Transform player;readonly Action changed;readonly Action<string> log;
 Transform root,focus,projectile;EarthNode castNode;Vector3 source,liftPoint,impactPoint,castLocal,impactNormal;
 public Vector3 ProjectilePosition=>projectile?projectile.position:Vector3.zero;
 public float Travelled=>travelled;public float FlightAge=>flightAge;public float PlayerDistance=>projectile?Vector3.Distance(projectile.position,player.position):0;
 Vector3 flightDirection;float flightAge,travelled;
 public float PeakSeparation{get;private set;}public float PeakOutbound{get;private set;}
 public Vector3 ProjectileVelocity=>velocity;public float ReturnTravelled=>returnTravel;
 float castClock;EarthEffects effects;LineRenderer marker;
 public EarthTrial(SkywrightGame game,Action changed,Action<string> log){
  this.game=game;cam=game.cam;player=game.controller.transform;this.changed=changed;this.log=log;
  config=Resources.Load<EarthConfig>("Earthcasting/Tuning");if(!config)throw new Exception("Earthcasting assets missing. Run explicit authoring once.");
  if(TutorialIsland.Active){config=UnityEngine.Object.Instantiate(config);config.flightSpeed=26;config.maxPlayerDistance=13;config.maxTravelDistance=26;config.recallAcceleration=130;}
  ding=BubbleAudio.Ding();effects=new EarthEffects(config,cam);
  focus=UnityEngine.Object.Instantiate(config.focus,cam.transform).transform;focus.gameObject.SetActive(false);
  holdSocket=new GameObject("Earth stone hold socket").transform;holdSocket.SetParent(focus,false);holdSocket.localPosition=new Vector3(.17f,.39f,.52f);
  marker=effects.Line("Earthcasting impact mark",.014f,true);marker.positionCount=33;marker.gameObject.SetActive(false);
  Reset();
 }
 public void Equip(bool value){if(!value){if(SentinelCombat.Active){repeatArmed=false;if(mode==StoneMode.Guided||mode==StoneMode.Waiting||mode==StoneMode.Striking||mode==StoneMode.Rebounding)BeginReturn("tool change");}else CancelCast("tool change");}equipped=value;focus.gameObject.SetActive(value);marker.gameObject.SetActive(false);if(projectile&&mode==StoneMode.Prepared)projectile.gameObject.SetActive(value);}
 public void Reset(){var fresh=new EarthState();for(int i=0;i<Locations.Length;i++)fresh.nodes.Add(new EarthNodeState{id=Id(i),kind=Kind(i)});Restore(fresh);}
 public static void Validate(EarthState s){
  var definitions=SceneryResource.Definitions();
  int expected=s!=null&&s.version==3?Locations.Length+definitions.Length:Locations.Length;
  if(s==null||(s.version!=1&&s.version!=2&&s.version!=3)||s.stone<0||s.wood<0||s.stone>1000000||s.wood>1000000||s.nodes==null||s.nodes.Count!=expected)throw new IOException("Invalid harvesting state. Current world kept.");
  var kinds=new System.Collections.Generic.Dictionary<string,string>();
  for(int i=0;i<Locations.Length;i++)kinds.Add(Id(i),Kind(i));
  if(s.version==3)foreach(var d in definitions)kinds.Add(d.stableId,d.kind);
  var ids=new HashSet<string>();var tuning=Resources.Load<EarthConfig>("Earthcasting/Tuning");
  foreach(var n in s.nodes){
   if(n==null||!kinds.TryGetValue(n.id,out var kind)||!ids.Add(n.id)||n.kind!=kind||n.damage<0||!float.IsFinite(n.fracture.x)||!float.IsFinite(n.fracture.y)||!float.IsFinite(n.fracture.z)||n.fracture.sqrMagnitude>100||n.collected&&!n.depleted)throw new IOException("Invalid resource node. Current world kept.");
   int limit=n.kind=="tree"?(s.version==1?3:tuning.treeHits):n.kind=="outcrop"?(s.version==1?2:tuning.outcropHits):0;
   if(n.kind!="pile"&&(n.damage>limit||n.depleted!=(n.damage==limit))||n.kind=="pile"&&(n.damage!=0||n.collected!=n.depleted))throw new IOException("Inconsistent resource depletion. Current world kept.");
  }
 }
 public static EarthState Upgrade(EarthState data){
  Validate(data);if(data.version==3||data.version==2&&!NaturalPreview.Active)return data;
  var next=JsonUtility.FromJson<EarthState>(JsonUtility.ToJson(data));var tuning=Resources.Load<EarthConfig>("Earthcasting/Tuning");
  if(next.version==1)foreach(var n in next.nodes){if(n.kind=="pile")continue;int old=n.kind=="tree"?3:2,limit=n.kind=="tree"?tuning.treeHits:tuning.outcropHits;
   n.damage=n.depleted?limit:Mathf.Clamp(Mathf.RoundToInt(n.damage*limit/(float)old),0,limit-1);
  }
  next.version=2;
  if(NaturalPreview.Active){foreach(var d in SceneryResource.Definitions())next.nodes.Add(new EarthNodeState{id=d.stableId,kind=d.kind});next.version=3;}
  Validate(next);return next;
 }
 public void Restore(EarthState data){
  data=Upgrade(data);effects.Clear();socketObserved=false;heldRotation=Quaternion.identity;if(root){root.gameObject.SetActive(false);UnityEngine.Object.Destroy(root.gameObject);}nodes.Clear();
  if(projectile)UnityEngine.Object.Destroy(projectile.gameObject);projectile=null;trail=null;castNode=null;boundTransform=null;boundCollider=null;mode=StoneMode.Idle;phase="Idle";equipped=false;focus.gameObject.SetActive(false);marker.gameObject.SetActive(false);
  state=data;if(state.preparedStone){mode=StoneMode.Prepared;phase="Prepared";}root=new GameObject("Earthcasting trial resources").transform;
  for(int i=0;i<Locations.Length;i++){
   var s=state.nodes.Find(n=>n.id==Id(i));var prefab=s.kind=="tree"?config.tree:s.kind=="pile"?config.pile:config.outcrop;
   var n=UnityEngine.Object.Instantiate(prefab,Locations[i],Quaternion.Euler(0,(i*67)%360,0),root).GetComponent<EarthNode>();n.state=s;
   if(NaturalPreview.Active&&s.kind=="tree")NaturalPreview.Tree(n);
   if(NaturalPreview.Active&&s.kind=="outcrop")NaturalPreview.Outcrop(n);
   n.restingRotation=n.crown.localRotation;nodes.Add(n);
   if(s.damage>0&&!s.depleted)Wound(n,s.fracture);
   if(s.depleted){n.crown.gameObject.SetActive(false);n.harvestingCollider.enabled=false;if(!s.collected)MakeLoot(n);}
  }
  if(NaturalPreview.Active)foreach(var scenery in UnityEngine.Object.FindObjectsByType<SceneryResource>(FindObjectsInactive.Include)){
   var saved=state.nodes.Find(x=>x.id==scenery.stableId);
   if(saved==null)throw new IOException("Missing authored scenery resource "+scenery.stableId);
   var n=scenery.Restore(saved);nodes.Add(n);
   if(saved.damage>0&&!saved.depleted)Wound(n,saved.fracture);
   if(saved.depleted&&!saved.collected)MakeLoot(n);
  }
  if(!managedCrates)foreach(var p in new[]{new Vector3(8,0,-12),new Vector3(10,0,-12)})UnityEngine.Object.Instantiate(config.crate,p,Quaternion.Euler(0,15,0),root);
  Physics.SyncTransforms();
 }
 public string CostHint(string type)=>type=="floor"?config.deckCost+" wood  /  You have "+state.wood:"Free trial piece";
 public string CanBuild(string type)=>type=="floor"&&state.wood<config.deckCost?"Need "+config.deckCost+" wood — harvest a tree and approach its wood":"";
 public void Pay(BuiltPiece b){if(b.type=="floor"){state.wood-=config.deckCost;b.paidWood=config.deckCost;log("SPEND wood="+config.deckCost+" remaining="+state.wood);}changed();}
 public void Refund(BuiltPiece b){if(b.paidWood>0){state.wood+=b.paidWood;game.Notify("Recovered "+b.paidWood+" wood");log("REFUND wood="+b.paidWood+" total="+state.wood);}changed();}
 public void Hide(){repeatArmed=false;marker.gameObject.SetActive(false);focus.gameObject.SetActive(false);if(mode==StoneMode.Prepared&&projectile)projectile.gameObject.SetActive(false);}
 public void Tick(float dt){
  effects.Tick(dt);focus.gameObject.SetActive(equipped);targetId="";targetValid=false;collectDistance=-1;holdClock+=dt;
  focus.localPosition=new Vector3(.36f,-.33f,.62f);focus.localRotation=Quaternion.Euler(-10,-12,Mathf.Sin(holdClock*1.8f)*.6f);
  var socket=HoldPoint(out _);socketVelocity=socketObserved?Vector3.ClampMagnitude((socket-previousSocket)/Mathf.Max(.001f,dt),20):Vector3.zero;previousSocket=socket;socketObserved=true;
  if(SentinelCombat.Active&&equipped&&mode==StoneMode.Idle)ReadyCombatStone();
  if(mode==StoneMode.Prepared)UpdatePrepared(dt);
  UpdateNodes(dt);var m=Mouse.current;var before=mode;
  bool attackPressed=CombatAttackPress(m.leftButton.wasPressedThisFrame,m.leftButton.isPressed);
  if(!m.leftButton.isPressed)repeatArmed=false;
  if(equipped&&attackPressed)repeatArmed=true;
  if(equipped){
   if(m.rightButton.wasPressedThisFrame&&mode!=StoneMode.Idle&&mode!=StoneMode.Prepared&&mode!=StoneMode.Preparing&&mode!=StoneMode.Catching)BeginReturn("manual");
   else if(attackPressed||repeatArmed&&m.leftButton.isPressed&&mode==StoneMode.Waiting){
    if(mode==StoneMode.Prepared&&attackPressed)Launch();
    else if(mode==StoneMode.Waiting&&TargetAlive()){strikeStart=projectile.position;actionClock=0;velocity=Vector3.zero;SetMode(StoneMode.Striking);log("STRIKE_BEGIN "+BoundTarget);}
   }
  }
  if(Busy)AdvanceCast(dt);
  marker.gameObject.SetActive(false);hint=equipped?"LMB Summon stone":"Tab Earthcasting • B Building kit";
  var ray=cam.ViewportPointToRay(new Vector3(.5f,.5f,0));
  bool hit=Physics.Raycast(ray,out var h,config.range,SpellMask,QueryTriggerInteraction.Ignore);
  EarthNode node=hit?h.collider.GetComponentInParent<EarthNode>():null;
  bool breakable=Eligible(node);
  if(node&&!node.state.depleted){
   targetId=node.state.id;
   if(node.state.kind=="pile"){
    bool near=h.distance<=config.stonePickupRange;hint=near?"E Collect "+config.pileYield+" loose stones":"Loose stones — move closer";
    if(near&&!Busy&&!externalInput&&Keyboard.current.eKey.wasPressedThisFrame){node.state.depleted=node.state.collected=true;node.crown.gameObject.SetActive(false);node.harvestingCollider.enabled=false;state.stone+=config.pileYield;game.PlayPickup();changed();game.Notify("+"+config.pileYield+" stone");log("PICKUP "+targetId+" stone="+state.stone);}
   }
  }
  if(equipped){
   if(mode==StoneMode.Idle){
    string invalid=CheckPreparation(out var origin);targetValid=invalid=="";
    if(!node||node.state.kind!="pile")hint=SentinelCombat.Active?"Clear space around the wand to ready Earthstone":targetValid?"LMB Summon stone":invalid;
    if(!SentinelCombat.Active&&before==StoneMode.Idle&&m.leftButton.wasPressedThisFrame){if(targetValid)Prepare(origin);else Reject(invalid);}
   }else if(mode==StoneMode.Prepared){targetValid=holdClear;hint=combatFreshPressRequired?"Release LMB, then cast again":holdClear?"LMB Launch stone":"Clear space around the wand to launch";}
   else if(mode==StoneMode.Guided){targetValid=true;hint="Mouse Guide • RMB Recall";}
   else if(mode==StoneMode.Waiting){targetValid=true;hint="Hold LMB Strike bound target • RMB Recall";}
   else if(mode==StoneMode.Striking||mode==StoneMode.Rebounding){targetValid=true;hint="RMB Recall";}
   else if(mode==StoneMode.Preparing)hint="Summoning stone";
   else if(mode==StoneMode.KnockedAway)hint="Stone deflected — returning to your wand";
   else if(SentinelCombat.Active)hint="Earthstone returning • prepare your next cast";
   else hint="";
   if(TargetAlive()&&(mode==StoneMode.Waiting||mode==StoneMode.Striking||mode==StoneMode.Rebounding))DrawMark(boundTransform.TransformPoint(castLocal),boundTransform.TransformDirection(localNormal));
   else if(breakable&&targetValid&&(mode==StoneMode.Idle||mode==StoneMode.Prepared||mode==StoneMode.Guided))DrawMark(h.point,h.normal);
  }
 }
 void SetMode(StoneMode next){mode=next;phase=next.ToString();if(trail){trail.emitting=next==StoneMode.Guided||next==StoneMode.Striking||next==StoneMode.Returning;trail.time=next==StoneMode.Returning?.25f:.18f;trail.startWidth=.055f;}}
 Vector3 HoldPoint(out bool clear){
  var desired=holdSocket.position+cam.transform.up*(Mathf.Sin(holdClock*1.8f)*.006f);var from=cam.transform.position;var d=desired-from;
  clear=!Physics.CheckSphere(from,StoneRadius,SpellMask,QueryTriggerInteraction.Ignore)&&!Physics.SphereCast(from,StoneRadius,d.normalized,out _,d.magnitude,SpellMask,QueryTriggerInteraction.Ignore)&&!Physics.CheckSphere(desired,StoneRadius,SpellMask,QueryTriggerInteraction.Ignore);
  return desired;
 }
 void CreateStone(Vector3 p){if(projectile)throw new InvalidOperationException("Only one Earthstone may exist");projectile=UnityEngine.Object.Instantiate(config.rock,p,Quaternion.identity).transform;projectile.localScale=Vector3.one;effects.Trail(projectile);trail=projectile.GetComponent<TrailRenderer>();trail.emitting=false;}
 void UpdatePrepared(float dt){
  Vector3 p=HoldPoint(out holdClear);if(!equipped){if(projectile)projectile.gameObject.SetActive(false);return;}
  if(!projectile)CreateStone(p);projectile.gameObject.SetActive(holdClear);projectile.position=p;projectile.localScale=Vector3.one;heldRotation*=Quaternion.Euler(0,dt*17,dt*4);projectile.rotation=heldRotation;
 }
 void Reject(string reason){game.Notify(reason);log("CAST_REJECT "+reason+" stone="+state.stone);}
 string CheckPreparation(out Vector3 origin){
  origin=Vector3.zero;
  Vector3 held=HoldPoint(out bool clear);if(!clear)return "Clear space around the wand to summon a stone";
  // The first solid surface directly beneath the player must be authored earth.
  if(!Physics.Raycast(player.position+Vector3.up*.1f,Vector3.down,out var below,config.earthSearchDistance,SpellMask,QueryTriggerInteraction.Ignore)||!below.collider.GetComponent<EarthSurface>()||below.normal.y<.65f)return "Earth must be below you, within 30 m";
  compactSummon=player.position.y-below.point.y>2.5f;
  if(compactSummon){
   origin=below.point;Vector3 gather=held-cam.transform.right*.10f-cam.transform.up*.18f;
   if(Physics.CheckSphere(gather,StoneRadius,SpellMask,QueryTriggerInteraction.Ignore)||!PathClear(gather,held)||!PathClear(below.point+Vector3.up*(StoneRadius+.04f),gather))return "Clear the path between earth and wand";
   return "";
  }
  Vector3 f=Vector3.ProjectOnPlane(cam.transform.forward,Vector3.up).normalized,r=player.right;
  foreach(float side in new[]{1f,-1f,0f}){
   Vector3 p=player.position+f*1.2f+r*side*.8f;
   if(!Physics.Raycast(p+Vector3.up*.2f,Vector3.down,out var g,config.earthSearchDistance,SpellMask,QueryTriggerInteraction.Ignore)||!g.collider.GetComponent<EarthSurface>()||g.normal.y<.65f)continue;
   Vector3 low=g.point+Vector3.up*(StoneRadius+.03f),high=g.point+Vector3.up*1.25f;
   if(Physics.CheckSphere(low,StoneRadius,SpellMask,QueryTriggerInteraction.Ignore)||Physics.CheckSphere(high,StoneRadius,SpellMask,QueryTriggerInteraction.Ignore))continue;
   if(!PathClear(low,high)||!PathClear(high,held))continue;origin=g.point;return "";
  }
  return "No clear earth or summon path";
 }

 bool PathClear(Vector3 a,Vector3 b){var d=b-a;return d.sqrMagnitude<.000001f||!Physics.SphereCast(a,StoneRadius,d.normalized,out _,d.magnitude,SpellMask,QueryTriggerInteraction.Ignore);}
 void DrawMark(Vector3 p,Vector3 normal){
  marker.gameObject.SetActive(true);Quaternion q=Quaternion.FromToRotation(Vector3.forward,normal);
  for(int i=0;i<=32;i++){float a=i*Mathf.PI*2/32;marker.SetPosition(i,p+normal*.018f+q*new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*.16f);}
 }
 void Prepare(Vector3 origin){
  source=origin;liftPoint=origin+Vector3.up*1.25f;castClock=0;velocity=Vector3.zero;
  state.preparedStone=true;changed();Vector3 first=source+Vector3.up*(StoneRadius+.03f);if(compactSummon){first=HoldPoint(out _)-cam.transform.right*.10f-cam.transform.up*.18f;liftPoint=first;}CreateStone(first);SetMode(StoneMode.Preparing);
  effects.Ring(source+Vector3.up*.025f,Vector3.up,.65f,.48f);effects.Burst(source+Vector3.up*.12f,Vector3.up,config.stone,12,.045f,1.4f,.5f);
  effects.Sound(config.prepare,cam.transform.position,.25f);effects.Sound(config.lift,source,.35f);log("STONE_SUMMON stone="+state.stone+" compact="+compactSummon+" at="+Time.time);
 }
 void Prepared(){state.preparedStone=true;castNode=null;boundTransform=null;boundCollider=null;velocity=Vector3.zero;SetMode(StoneMode.Prepared);if(trail)trail.Clear();if(projectile)heldRotation=projectile.rotation;changed();log("STONE_READY stone="+state.stone+" at="+Time.time);}
 void Launch(){
  PeakSeparation=PeakOutbound=0;
  Vector3 p=HoldPoint(out bool clear);if(!clear||!projectile){Reject("Clear space around the wand to launch");return;}
  p=projectile.position;projectile.gameObject.SetActive(true);flightDirection=(AimPoint()-p).normalized;velocity=flightDirection*config.flightSpeed;
  flightAge=travelled=returnAge=returnTravel=actionClock=blockedClock=0;bounces=0;lastContactNormal=Vector3.zero;castNode=null;boundTransform=null;boundCollider=null;state.preparedStone=false;changed();SetMode(StoneMode.Guided);trail.Clear();
  effects.Sound(config.launch,p,.55f);log("STONE_LAUNCH stone="+state.stone);
 }
 void DestroyStone(){if(projectile){projectile.gameObject.SetActive(false);UnityEngine.Object.Destroy(projectile.gameObject);}projectile=null;trail=null;castNode=null;boundTransform=null;boundCollider=null;velocity=Vector3.zero;}
 public void CancelCast(string reason){repeatArmed=false;
  if(SentinelCombat.Active&&reason=="focus lost"){if(mode==StoneMode.Guided||mode==StoneMode.Waiting||mode==StoneMode.Striking||mode==StoneMode.Rebounding)BeginReturn(reason);return;}
  if(mode==StoneMode.Preparing){DestroyStone();state.preparedStone=true;SetMode(StoneMode.Prepared);changed();return;}
  if(mode==StoneMode.Prepared||mode==StoneMode.Idle)return;
  if(projectile)effects.Ring(projectile.position,Vector3.up,.35f,.3f);
  DestroyStone();state.preparedStone=false;SetMode(StoneMode.Idle);changed();
  log("CAST_DISMISSED "+reason+" travelled="+travelled.ToString("F2")+" return="+returnTravel.ToString("F2")+" stone="+state.stone);
 }
 void BeginReturn(string reason){
  repeatArmed=false;
  if(mode==StoneMode.Returning||mode==StoneMode.Catching||mode==StoneMode.Blocked||mode==StoneMode.Dismissing||mode==StoneMode.KnockedAway||!projectile)return;
  castNode=null;boundTransform=null;boundCollider=null;actionClock=blockedClock=0;SetMode(StoneMode.Returning);log("RETURN_BEGIN "+reason+" stone="+state.stone);effects.Ring(projectile.position,Vector3.up,.3f,.22f);
 }
 void Dismiss(string reason){if(mode==StoneMode.Dismissing)return;castNode=null;boundTransform=null;boundCollider=null;velocity=Vector3.zero;actionClock=0;SetMode(StoneMode.Dismissing);effects.Ring(projectile.position,Vector3.up,.4f,config.dismissTime);log("RETURN_FALLBACK "+reason);}
 bool Eligible(EarthNode n)=>n&&n.state!=null&&!n.state.depleted&&n.state.kind!="pile";
 bool CanBind(Collider c){
  var enemy=c.GetComponentInParent<SentinelEnemy>();if(enemy)return enemy.Alive;
  var node=c.GetComponentInParent<EarthNode>();if(node)return Eligible(node);
  if(!TutorialIsland.Active)return false;
  return c.GetComponentInParent<BuiltPiece>()||c.GetComponentInParent<AirCrate>()||c.GetComponentInParent<CargoHarvest>();
 }
 bool TargetAlive()=>boundTransform&&boundCollider&&boundCollider.enabled&&boundCollider.gameObject.activeInHierarchy&&(!castNode||Eligible(castNode));
 Vector3 AimPoint(){var ray=cam.ViewportPointToRay(new Vector3(.5f,.5f,0));return Physics.Raycast(ray,out var h,config.range,SpellMask,QueryTriggerInteraction.Ignore)?h.point:ray.GetPoint(config.range);}
 bool Sweep(Vector3 next,out RaycastHit hit){var d=next-projectile.position;hit=default;return d.sqrMagnitude>.0000001f&&Physics.SphereCast(projectile.position,StoneRadius,d.normalized,out hit,d.magnitude,SpellMask,QueryTriggerInteraction.Ignore);}
 void StopAt(RaycastHit h,Vector3 direction){projectile.position+=direction.normalized*Mathf.Max(0,h.distance-.008f);}
 void AdvanceCast(float dt){
  if(mode==StoneMode.Dismissing){actionClock+=dt;projectile.localScale=Vector3.one*(1-Mathf.SmoothStep(0,1,actionClock/config.dismissTime));if(actionClock>=config.dismissTime){
   if(SentinelCombat.Active){projectile.gameObject.SetActive(false);var socket=HoldPoint(out bool clear);if(clear){projectile.position=socket;projectile.localScale=Vector3.one;Prepared();UpdatePrepared(0);}}
   else CancelCast("fallback complete");
  }return;}
  if(mode==StoneMode.Preparing){
   castClock+=dt;Vector3 held=HoldPoint(out bool clear);if(!clear){CancelCast("summon obstructed");return;}
   float rise=compactSummon?0:config.emergeTime,settle=compactSummon?config.emergeTime+config.summonSettleTime:config.summonSettleTime;Vector3 next;
   if(castClock<rise){float t=castClock/rise;next=Vector3.Lerp(source+Vector3.up*(StoneRadius+.03f),liftPoint,Mathf.SmoothStep(0,1,t));}
   else {float t=Mathf.Clamp01((castClock-rise)/settle);next=Vector3.Lerp(liftPoint,held,Mathf.SmoothStep(0,1,t));}
   if(!PathClear(projectile.position,next)){CancelCast("summon path changed");return;}projectile.position=next;projectile.Rotate(0,dt*65,dt*12);
   if(castClock>=rise+settle)Prepared();return;
  }
  int steps=Mathf.Max(1,Mathf.CeilToInt(dt/.01f));float step=dt/steps;
  for(int i=0;i<steps&&projectile;i++){
   if(mode==StoneMode.KnockedAway){AdvanceKnockedStone(step);continue;}
   if(mode!=StoneMode.Prepared){float distance=Vector3.Distance(projectile.position,player.position);PeakSeparation=Mathf.Max(PeakSeparation,distance);if(mode==StoneMode.Guided)PeakOutbound=Mathf.Max(PeakOutbound,distance);}
   if(mode==StoneMode.Prepared||mode==StoneMode.Idle||mode==StoneMode.Dismissing)break;
   if(mode==StoneMode.Waiting||mode==StoneMode.Striking||mode==StoneMode.Rebounding){
    if(AdvanceSentinelAttachment(step))continue;
    if(!TargetAlive()){BeginReturn("target unavailable");continue;}
    if(Vector3.Distance(projectile.position,player.position)>config.maxPlayerDistance){BeginReturn("target out of range");continue;}
    var contact=boundTransform.TransformPoint(castLocal);var normal=boundTransform.TransformDirection(localNormal).normalized;
    waitPoint=contact+normal*(StoneRadius+config.waitOffset);
    Vector3 destination=waitPoint;float speed=config.reboundSpeed;actionClock+=step;
    if(mode==StoneMode.Waiting){
     if(Physics.CheckSphere(projectile.position,StoneRadius,SpellMask,QueryTriggerInteraction.Ignore)){BeginReturn("hover obstructed");continue;}
     var follow=Vector3.MoveTowards(projectile.position,waitPoint,config.reboundSpeed*step);if(!PathClear(projectile.position,follow)){BeginReturn("moving attachment blocked");continue;}projectile.position=follow;velocity=Vector3.zero;projectile.Rotate(0,step*15,step*5);continue;
    }
    if(mode==StoneMode.Striking){
     if(actionClock<config.strikeAnticipation)destination=strikeStart+normal*(.12f*Mathf.SmoothStep(0,1,actionClock/config.strikeAnticipation));
     else {destination=contact-normal*.06f;speed=config.strikeSpeed;}
     if(actionClock>1.2f){BeginReturn("strike contact unavailable");continue;}
    }
    velocity=Vector3.MoveTowards(velocity,(destination-projectile.position).normalized*speed,config.strikeAcceleration*step);
    var next=Vector3.MoveTowards(projectile.position,destination,velocity.magnitude*step);
    if(Sweep(next,out var strikeHit)){
     StopAt(strikeHit,next-projectile.position);
     if(mode==StoneMode.Striking&&actionClock>=config.strikeAnticipation&&(strikeHit.collider==boundCollider||castNode&&strikeHit.collider.GetComponentInParent<EarthNode>()==castNode)){HitTarget(strikeHit);}
     else {log("ATTACHMENT_BLOCKED "+phase+" collider="+strikeHit.collider.name);BeginReturn("strike/rebound obstacle");}
     continue;
    }
    projectile.position=next;projectile.Rotate(0,step*60,step*18);
    if(mode==StoneMode.Rebounding&&Vector3.Distance(next,waitPoint)<.012f){velocity=Vector3.zero;SetMode(StoneMode.Waiting);log("TARGET_WAIT "+BoundTarget+" at="+next);}
    continue;
   }
   if(mode==StoneMode.Catching){
    actionClock+=step;var socket=HoldPoint(out bool clear);float t=Mathf.SmoothStep(0,1,actionClock/config.catchTime);
    var next=socket+catchOffset*(1-t);
    if(!clear||Physics.CheckSphere(projectile.position,StoneRadius,SpellMask,QueryTriggerInteraction.Ignore)||!PathClear(projectile.position,next)){blockedClock=0;velocity=Vector3.zero;SetMode(StoneMode.Blocked);continue;}
    projectile.position=next;projectile.rotation=Quaternion.Slerp(catchRotation,heldRotation,t);
    if(actionClock>=config.catchTime){Prepared();log("RETURN_CAUGHT socketSpeed="+socketVelocity.magnitude+" peak="+PeakSeparation+" outbound="+PeakOutbound+" at="+Time.time);}continue;
   }
   bool returning=mode==StoneMode.Returning||mode==StoneMode.Blocked;
   if(returning){
    returnAge+=step;var destination=HoldPoint(out bool clear);var delta=destination-projectile.position;
    if(returnAge>=config.maxRecallTime||returnTravel>=config.maxRecallDistance){Dismiss("return budget");break;}
    if(mode==StoneMode.Blocked){
     blockedClock+=step;velocity=Vector3.zero;
     if(clear&&!Physics.CheckSphere(projectile.position,StoneRadius,SpellMask,QueryTriggerInteraction.Ignore)&&PathClear(projectile.position,destination)){SetMode(StoneMode.Returning);log("RETURN_UNBLOCKED");}
     else if(blockedClock>=config.blockedReturnTime){Dismiss("obstruction");break;}
     continue;
    }
    if(Physics.CheckSphere(projectile.position,StoneRadius,SpellMask,QueryTriggerInteraction.Ignore)){velocity=Vector3.zero;blockedClock=0;SetMode(StoneMode.Blocked);log("RETURN_BLOCKED overlap at="+projectile.position);continue;}
    if(clear&&delta.magnitude<config.catchDistance&&(velocity-socketVelocity).magnitude<3&&PathClear(projectile.position,destination)){catchOffset=projectile.position-destination;catchRotation=projectile.rotation;heldRotation=catchRotation*Quaternion.Euler(0,config.catchTime*17,0);actionClock=0;SetMode(StoneMode.Catching);continue;}
    float speed=Mathf.Min(config.recallSpeed,Mathf.Sqrt(2*config.recallAcceleration*delta.magnitude),delta.magnitude*10);
    velocity=Vector3.MoveTowards(velocity,Vector3.ClampMagnitude(socketVelocity+delta.normalized*speed,config.recallSpeed),config.recallAcceleration*step);
   }else{
    if(mode==StoneMode.Guided){var desired=AimPoint()-projectile.position;
     if(lastContactNormal!=Vector3.zero&&Vector3.Dot(projectile.position-lastContactCenter,lastContactNormal)>=StoneRadius+.08f)lastContactNormal=Vector3.zero;
     if(lastContactNormal!=Vector3.zero&&Vector3.Dot(projectile.position-lastContactCenter,lastContactNormal)<StoneRadius+.08f&&Vector3.Dot(desired,lastContactNormal)<0){desired=Vector3.ProjectOnPlane(desired,lastContactNormal)+lastContactNormal*.12f;}if(desired.sqrMagnitude>.01f)flightDirection=Vector3.RotateTowards(velocity.normalized,desired.normalized,config.steeringDegrees*Mathf.Deg2Rad*step,0).normalized;velocity=flightDirection*config.flightSpeed;}
    flightAge+=step;var radial=projectile.position-player.position;float outward=Mathf.Max(0,Vector3.Dot(velocity,radial.normalized));
    float braking=TutorialIsland.Active?outward*outward/(2*config.recallAcceleration)+config.flightSpeed*step:0;
    if(radial.magnitude+braking>=config.maxPlayerDistance){BeginReturn("distance braking");continue;}if(flightAge>=config.maxFlightTime||travelled>=config.maxTravelDistance){BeginReturn(flightAge>=config.maxFlightTime?"flight time limit":"path limit");continue;}
   }
   float remaining=returning?config.maxRecallDistance-returnTravel:config.maxTravelDistance-travelled;
   Vector3 movement=Vector3.ClampMagnitude(velocity*step,Mathf.Max(0,remaining));var nextPosition=projectile.position+movement;
   if(velocity.sqrMagnitude>.001f)flightDirection=velocity.normalized;
   if(Sweep(nextPosition,out var h)){
    float actual=Mathf.Max(0,h.distance-.008f);if(returning)returnTravel+=actual;else travelled+=actual;
    StopAt(h,movement);
    if(returning){velocity=Vector3.zero;blockedClock=0;SetMode(StoneMode.Blocked);log("RETURN_BLOCKED collider="+h.collider.name+" at="+projectile.position);continue;}
    if(CanBind(h.collider))HitTarget(h);else Bounce(h);
    continue;
   }
   if(returning)returnTravel+=movement.magnitude;else travelled+=movement.magnitude;
   projectile.position=nextPosition;projectile.Rotate(0,step*95,step*25);
  }
 }
 void HitTarget(RaycastHit h){
  castNode=h.collider.GetComponentInParent<EarthNode>();if(!Eligible(castNode))castNode=null;boundCollider=h.collider;boundTransform=castNode?castNode.transform:h.collider.transform;impactPoint=h.point;impactNormal=h.normal;castLocal=boundTransform.InverseTransformPoint(h.point);localNormal=boundTransform.InverseTransformDirection(h.normal);
  if(boundCollider.GetComponentInParent<SentinelEnemy>()){attachmentFrame=boundTransform.localToWorldMatrix;FinishSentinelImpact();return;}
  Impact();velocity=Vector3.zero;actionClock=0;
  if(!TargetAlive()){BeginReturn("target broken");return;}
  waitPoint=boundTransform.TransformPoint(castLocal)+h.normal*(StoneRadius+config.waitOffset);
  if(Physics.CheckSphere(waitPoint,StoneRadius,SpellMask,QueryTriggerInteraction.Ignore)||!PathClear(projectile.position,waitPoint)){BeginReturn("no clear waiting position");return;}
  SetMode(StoneMode.Rebounding);
 }
 void Bounce(RaycastHit h){
  bounces++;lastContactNormal=h.normal;lastContactCenter=projectile.position;
  effects.Sound(config.impact,h.point,.35f);effects.Burst(h.point,h.normal,config.stone,8,.035f,1.5f,.35f);effects.Ring(h.point+h.normal*.02f,h.normal,.25f,.18f);
  velocity=Vector3.Reflect(velocity,h.normal)*config.ricochetRetention;
  // The sweep stops short of contact; a swept separation prevents repeat-face jitter without crossing a corner.
  var separated=projectile.position+h.normal*.015f;if(PathClear(projectile.position,separated))projectile.position=separated;
  SetMode(StoneMode.Guided);log("RICOCHET count="+bounces+" normal="+h.normal+" velocity="+velocity+" at="+projectile.position);
  if(bounces>=config.maxRicochets||velocity.magnitude<2)BeginReturn("ricochet limit");
 }
 void Impact(){
  var enemy=boundCollider?boundCollider.GetComponentInParent<SentinelEnemy>():null;
  if(enemy){enemy.EarthImpact(impactPoint);effects.Sound(config.impact,impactPoint,.65f);effects.Burst(impactPoint,impactNormal,config.stone,12,.055f,2,.45f);effects.Ring(impactPoint,impactNormal,.3f,.2f);return;}
  var n=castNode;if(!n||n.state.depleted){
   var cargo=boundCollider?boundCollider.GetComponentInParent<CargoHarvest>():null;if(cargo){cargo.Hit(game);effects.Sound(config.impact,impactPoint,.55f);}else effects.Sound(ding,impactPoint,.42f);effects.Burst(impactPoint,impactNormal,config.stone,10,.04f,1,.35f);
   log("IMPACT_UNAFFECTED stone="+state.stone+" phase="+phase+" at="+impactPoint);return;
  }
  bool precise=n.state.kind=="tree"&&n.state.damage>0&&Vector3.Distance(castLocal,n.state.fracture)<.32f;
  int maximum=n.state.kind=="tree"?config.treeHits:config.outcropHits;n.state.damage=Mathf.Min(maximum,n.state.damage+1);
  if(n.state.damage==1)n.state.fracture=castLocal;
  effects.Sound(config.impact,impactPoint,.85f);effects.Burst(impactPoint,impactNormal,n.state.kind=="tree"?config.bark:config.stone,28,.09f,3,.7f);effects.Burst(impactPoint,impactNormal,config.stone,12,.06f,2,.6f);effects.Ring(impactPoint+impactNormal*.03f,impactNormal,.42f,.25f);
  n.reaction=.5f;Wound(n,n.state.fracture);changed();
  log("IMPACT "+n.state.id+" damage="+n.state.damage+" precise="+precise+" stone="+state.stone);
  if(n.state.damage>=maximum){
   n.state.depleted=true;foreach(var c in n.GetComponentsInChildren<Collider>(true))c.enabled=false;
   if(n.state.kind=="tree"){
    n.fallClock=0;Vector3 dir=Vector3.ProjectOnPlane(flightDirection,Vector3.up).normalized;if(dir.sqrMagnitude<.01f)dir=player.forward;
    // Choose a clear visual fall corridor; decorative motion has no damaging physics.
    n.fallDirection=SafeFall(n,dir);log("FALL_DIRECTION "+n.state.id+" direction="+n.fallDirection);effects.Sound(config.crack,n.transform.position,.9f);game.Notify("Tree felled — approach within "+config.collectionRadius+" m to collect wood");
   }else{n.crown.gameObject.SetActive(false);MakeLoot(n);effects.Sound(config.crack,n.transform.position,.6f);}
  }
 }
 Vector3 SafeFall(EarthNode n,Vector3 desired){
  Vector3 best=desired;int least=int.MaxValue;float length=5;var renderers=n.crown.GetComponentsInChildren<Renderer>();foreach(var r in renderers)length=Mathf.Max(length,r.bounds.max.y-n.transform.position.y);length=Mathf.Min(length,16);
  foreach(float angle in new[]{0f,45f,-45f,90f,-90f,135f,-135f,180f}){
   Vector3 dir=Quaternion.Euler(0,angle,0)*desired,center=n.transform.position+dir*(length*.5f)+Vector3.up*1.7f;
   int blockers=0;foreach(var c in Physics.OverlapBox(center,new Vector3(1,2,length*.5f),Quaternion.LookRotation(dir),~(1<<2),QueryTriggerInteraction.Ignore))if(c.GetComponentInParent<BuiltPiece>()||c==game.controller||c.GetComponentInParent<EarthNode>()!=n&&c.GetComponentInParent<EarthNode>())blockers++;
   if(blockers<least){least=blockers;best=dir;}if(blockers==0)return dir;
  }
  // Cosmetic fall takes the least occupied corridor; it never applies physical force.
  return best;
 }
 void Wound(EarthNode n,Vector3 local){
  if(n.state.kind!="tree")return;if(n.wound)UnityEngine.Object.Destroy(n.wound.gameObject);
  if(NaturalPreview.Active){var surface=n.GetComponentInChildren<TreeSurfacePresentation>();n.wound=surface?surface.FitScar(n.crown,n.transform.TransformPoint(local),n.state.damage):null;return;}
  var w=new GameObject("Exposed wood fracture");w.transform.SetParent(n.crown,false);w.transform.position=n.transform.TransformPoint(local);
  Vector3 outward=Vector3.ProjectOnPlane(w.transform.position-n.transform.position,Vector3.up).normalized;w.transform.rotation=Quaternion.LookRotation(outward);
  n.wound=w.transform;
  effects.Scar(w.transform,n.state.damage);
 }
 void MakeLoot(EarthNode n){
  if(n.loot||n.state.collected)return;
  n.lootGround=n.transform.position;
  if(n.GetComponent<SceneryResource>()){
   foreach(var h in Physics.RaycastAll(n.transform.position+Vector3.up*10,Vector3.down,30,SpellMask,QueryTriggerInteraction.Ignore).OrderBy(x=>x.distance))
    if(h.collider.GetComponent<EarthSurface>()){n.lootGround=h.point;break;}
  }
  n.loot=UnityEngine.Object.Instantiate(n.state.kind=="tree"?config.wood:config.pile,n.lootGround+Vector3.up*.18f,Quaternion.identity,root).transform;
  foreach(var c in n.loot.GetComponentsInChildren<Collider>())c.enabled=false;
  // This bundle is a collection display, not a second harvesting node.
  foreach(var c in n.loot.GetComponentsInChildren<EarthNode>())UnityEngine.Object.Destroy(c);
 }
 void FinishFall(EarthNode n){if(NaturalPreview.Active)NaturalPreview.TreeFelled(n);n.fallClock=-1;n.crown.gameObject.SetActive(false);if(n.wound)n.wound.gameObject.SetActive(false);MakeLoot(n);}
 void UpdateNodes(float dt){
  foreach(var n in nodes){
   if(n.fallClock>=0){n.fallClock+=dt;float t=Mathf.Clamp01(n.fallClock/config.fallTime);
    {Vector3 axis=Vector3.Cross(Vector3.up,n.fallDirection);float angle=82*Mathf.Pow(t,2.2f);n.crown.rotation=Quaternion.AngleAxis(angle,axis)*n.transform.rotation*n.restingRotation;}
    if(t>=1){effects.Sound(config.fall,n.transform.position,.85f);effects.Burst(n.transform.position+n.fallDirection*2+Vector3.up*.3f,Vector3.up,config.leaf,30,.09f,2,1);FinishFall(n);}
   }else if(!n.state.depleted&&n.state.kind=="tree"){
    n.reaction=Mathf.Max(0,n.reaction-dt);float sway=Mathf.Sin(Time.time*1.4f+n.transform.position.x)*.25f;
    n.crown.localRotation=n.restingRotation*Quaternion.Euler(0,0,sway+n.state.damage*.55f+Mathf.Sin(n.reaction*48)*n.reaction*2);
   }
   if(!n.state.depleted||n.state.collected||!n.loot)continue;
   float distance=Vector3.Distance(player.position,n.lootGround);collectDistance=collectDistance<0?distance:Mathf.Min(collectDistance,distance);
   if(n.collectClock<0){
    if(n.pull.Tick(game,n.loot,n.lootGround+Vector3.up*(.35f+Mathf.Sin(Time.time*2)*.055f),dt)){Award(n);continue;}
    bool occluded=Physics.Linecast(n.loot.position,player.position+Vector3.up, out var hit,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<EarthNode>()!=n;
    if(distance<=config.collectionRadius&&!occluded){n.collectClock=0;n.collectStart=n.loot.position;effects.Sound(config.prepare,n.transform.position,.2f);log("COLLECTION_BEGIN "+n.state.id+" distance="+distance.ToString("F2"));}
   }else {
    n.collectClock+=dt;float t=Mathf.Clamp01(n.collectClock/.7f);n.loot.position=Vector3.Lerp(n.collectStart,player.position+Vector3.up*1.25f,t*t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.65f;n.loot.localScale=Vector3.one*(1-t*.85f);
    if(t>=1)Award(n);
   }
  }
 }
 void Award(EarthNode n){
  if(n.state.collected)return;n.pull.Reset();n.state.collected=true;n.collectClock=-1;
  int amount=n.state.kind=="tree"?config.treeYield:config.outcropYield;
  if(n.state.kind=="tree")state.wood+=amount;else state.stone+=amount;
  if(n.loot)UnityEngine.Object.Destroy(n.loot.gameObject);n.loot=null;
  game.PlayPickup();game.Notify("+"+amount+(n.state.kind=="tree"?" wood":" stone"));changed();
  log("AWARD "+n.state.id+" amount="+amount+" wood="+state.wood+" stone="+state.stone);
 }
 // Unresolved flight cancels without invented hits. Resolved falls/collections settle once.
 public void InterruptDrops(){foreach(var n in nodes){n.pull.Reset();if(n.loot&&n.collectClock<0)n.loot.position=n.lootGround+Vector3.up*.35f;}}
 public EarthState Snapshot(){
  InterruptDrops();
  CancelCast("save");
  foreach(var n in nodes){if(n.fallClock>=0)FinishFall(n);if(n.collectClock>=0)Award(n);}
  return state;
 }
}
}

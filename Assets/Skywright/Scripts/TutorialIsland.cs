using UnityEngine;
namespace Skywright {
// Stable authored world identity; independent of player construction and catalog ordering.
public static class TutorialIsland {
 public const string Layout="tutorial-v1", AnchorId="tutorial-island-anchor";
 public static bool Active=>NaturalPreview.Active;
 public static int BuildableCount=>Kit.Ids.Length-(Active?1:0);
 public static bool Fixed(BuiltPiece b)=>Active&&b&&b.type=="wind-anchor";
 public static Vector3 RoutePoint(AetherConfig c,float t){
  t=Mathf.Clamp01(t);return Vector3.Lerp(c.routeStart,c.routeEnd,t)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*12);
 }
}
// A property-block shimmer preserves source materials and works with shared meshes.
public sealed class SuctionShimmer {
 MaterialPropertyBlock block;
 readonly System.Collections.Generic.Dictionary<Renderer,Color> originals=new();
 public void Clear(){foreach(var r in originals.Keys)if(r)r.SetPropertyBlock(null);originals.Clear();}
 public void Show(Transform root,float strength){
  if(block==null)block=new MaterialPropertyBlock();
  foreach(var r in root.GetComponentsInChildren<Renderer>()){
   if(r is LineRenderer||r is TrailRenderer||!r.sharedMaterial||!r.sharedMaterial.HasProperty("_BaseColor"))continue;
   if(!originals.TryGetValue(r,out var color)){color=r.sharedMaterial.GetColor("_BaseColor");originals.Add(r,color);}
   block.Clear();block.SetColor("_BaseColor",Color.Lerp(color,new Color(.78f,1,1,color.a),strength*(.32f+.12f*Mathf.Sin(Time.time*5))));r.SetPropertyBlock(block);
  }
 }
}
// A drop has one ledger owner. This class moves only its visual until physical capture.
public sealed class CollectiblePull {
 Vector3 velocity;bool latched;readonly SuctionShimmer shimmer=new SuctionShimmer();
 public void Reset(){latched=false;velocity=Vector3.zero;shimmer.Clear();}
 public bool Tick(SkywrightGame game,Transform loot,Vector3 home,float dt){
  if(!loot)return false;shimmer.Clear();var config=AetherSystem.Current?.config;if(config==null)return false;
  var camera=game.cam.transform;var delta=loot.position-camera.position;
  bool clear=!Physics.Linecast(camera.position,loot.position,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore);
  bool eligible=delta.magnitude<=config.suctionRange&&Vector3.Angle(camera.forward,delta)<=config.suctionCone&&clear;
  if(game.AirEquipped&&eligible)shimmer.Show(loot,1);
  bool retain=latched&&delta.magnitude<=config.suctionRange+4&&Vector3.Angle(camera.forward,delta)<=config.releaseCone&&clear;
  if(!game.SuctionActive||!(eligible||retain)){latched=false;velocity=Vector3.zero;loot.position=home;return false;}
  latched=true;var goal=camera.position+camera.forward*.6f-camera.up*.2f;var direction=goal-loot.position;
  if(direction.magnitude<1&&!Physics.Linecast(loot.position,goal,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore)){Reset();return true;}
  velocity=Vector3.MoveTowards(velocity,direction.normalized*Mathf.Min(12,Mathf.Max(5,direction.magnitude*6)),dt*40);
  var step=Vector3.ClampMagnitude(velocity*dt,direction.magnitude);
  if(step.sqrMagnitude>0&&Physics.SphereCast(loot.position,.12f,step.normalized,out var hit,step.magnitude,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore))step=step.normalized*Mathf.Max(0,hit.distance-.01f);
  loot.position+=step;return false;
 }
}
}

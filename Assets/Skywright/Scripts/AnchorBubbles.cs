using System.Collections.Generic;
using UnityEngine;
namespace Skywright {
public static class AnchorVolume {
 public static Vector3 Radii(AirConfig c)=>AetherSystem.Current!=null?AetherSystem.Current.EffectiveRadii:new Vector3(c.anchorRadius,(c.anchorHeight+2)*.5f,c.anchorRadius);
 public static Vector3 Center(AirConfig c,Vector3 anchor)=>AetherSystem.Current!=null?AetherSystem.Current.EffectiveCenter(anchor):anchor+Vector3.up*(c.anchorHeight-2)*.5f;
 public static bool Contains(AirConfig c,Vector3 anchor,Vector3 player){if(AetherSystem.Current!=null)return AetherSystem.Current.Contains(anchor,player);var d=player-Center(c,anchor);var r=Radii(c);return d.x*d.x/(r.x*r.x)+d.y*d.y/(r.y*r.y)+d.z*d.z/(r.z*r.z)<=1;}
}
public class AnchorBubbles {
 readonly MaterialPropertyBlock block=new MaterialPropertyBlock();readonly IslandField field=Resources.Load<IslandField>("CompactIsland/Field");
 readonly AirConfig config;readonly Dictionary<BuiltPiece,GameObject> views=new Dictionary<BuiltPiece,GameObject>();readonly List<BuiltPiece> stale=new List<BuiltPiece>();
 public AnchorBubbles(AirConfig config){this.config=config;}
 public void Refresh(Builder builder){
  stale.Clear();foreach(var pair in views)if(!pair.Key||pair.Key!=builder.ghost&&!builder.pieces.Contains(pair.Key)&&(AetherSystem.Current==null||pair.Key!=AetherSystem.Current.Primary()))stale.Add(pair.Key);
  foreach(var b in stale){Object.Destroy(views[b]);views.Remove(b);}
  foreach(var b in builder.pieces)if(b&&b.type=="wind-anchor")Update(b,AetherSystem.Current==null||AetherSystem.Current.IsPrimary(b.id));
  if(AetherSystem.Current!=null)Update(AetherSystem.Current.Primary(),true);
  if(builder.ghost&&builder.ghost.type=="wind-anchor")Update(builder.ghost,builder.enabled&&builder.ghost.gameObject.activeSelf&&(AetherSystem.Current==null||!AetherSystem.Current.Primary()));
 }
 void Update(BuiltPiece b,bool visible){
  if(!views.TryGetValue(b,out var sphere)){
   sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);sphere.name="Wind support bubble";sphere.layer=2;var c=sphere.GetComponent<Collider>();c.enabled=false;Object.Destroy(c);
   if(field&&field.shell)sphere.GetComponent<MeshFilter>().sharedMesh=field.shell;
   var mr=sphere.GetComponent<MeshRenderer>();mr.sharedMaterial=config.bubble;mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;mr.receiveShadows=false;views.Add(b,sphere);
  }
  block.SetFloat("_Hemisphere",TutorialIsland.Active?1:0);if(field){block.SetTexture("_Shoreline",field.shoreline);block.SetFloat("_FieldRadius",AnchorVolume.Radii(config).x);block.SetFloat("_FarthestShore",field.farthest);}sphere.GetComponent<Renderer>().SetPropertyBlock(block);sphere.SetActive(visible);sphere.transform.position=AnchorVolume.Center(config,b.transform.position);sphere.transform.localScale=AnchorVolume.Radii(config)*2;
 }
}
}

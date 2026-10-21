using System;
using System.Collections.Generic;
using UnityEngine;
namespace Skywright {
[Serializable] public class SnapAnchor {
 public string id,label; public Vector3 position; public float yaw;
 public SnapAnchor(string id,string label,Vector3 position,float yaw=0){this.id=id;this.label=label;this.position=position;this.yaw=yaw;}
}
// Connection coordinates are local to the stable gameplay root, never to a visual mesh.
public static class SnapRules {
 static readonly Dictionary<string,SnapAnchor[]> defaults=new Dictionary<string,SnapAnchor[]>();
 public static SnapAnchor[] Anchors(string type){
  var d=PieceCatalog.Find(type);return d!=null&&d.anchors!=null&&d.anchors.Length>0?d.anchors:Defaults(type);
 }
 public static SnapAnchor[] Defaults(string t){
  if(defaults.TryGetValue(t,out var cached))return cached;
  var a=new List<SnapAnchor>();
  void A(string id,string label,float x,float y,float z,float yaw=0)=>a.Add(new SnapAnchor(id,label,new Vector3(x,y,z),yaw));
  if(t=="floor"){
   for(int i=0;i<4;i++){var p=Quaternion.Euler(0,i*90,0)*Vector3.back*2;a.Add(new SnapAnchor("edge"+i,"Edge "+(i+1),p,i*90));}
   A("center","Top",0,0,0);
   for(int i=0;i<4;i++){var p=Quaternion.Euler(0,i*90,0)*new Vector3(1.85f,0,-1.85f);a.Add(new SnapAnchor("corner"+i,"Corner "+(i+1),p));}
  }else if(Kit.IsWall(t)||t=="railing"||t=="gable"){
   float h=t=="railing"?1.19f:t=="gable"?1.5f:3;
   A("bottom","Bottom",0,0,0);A("top","Top",0,h,0);A("left","Left edge",-2,0,0);A("right","Right edge",2,0,0);
  }else if(t=="roof"){
   A("eave","Eave / bottom",0,0,0);A("ridge","Ridge / top",0,1.5f,2);
   A("left","Left edge",-2,.75f,1);A("right","Right edge",2,.75f,1);
  }else if(t=="stairs"){
   A("bottom","Bottom landing",0,0,-4);A("top","Top landing",0,3,0);
  }else if(t=="post"){
   A("bottom","Bottom",0,0,0);A("top","Top",0,2.6f,0);
  }else if(t=="beam"){
   A("left","Left end",-2,0,0);A("right","Right end",2,0,0);A("bottom","Bottom centre",0,0,0);
   A("top","Top centre",0,.24f,0);A("topleft","Top left end",-1.88f,.24f,0);A("topright","Top right end",1.88f,.24f,0);
  }else A("bottom","Bottom",0,0,0);
  defaults[t]=a.ToArray();return defaults[t];
 }
 public static SnapAnchor[] Modes(string type){
  var a=new List<SnapAnchor>();
  foreach(var p in Anchors(type)){
   if(type=="floor"&&!p.id.StartsWith("edge"))continue;
   if(type=="gable"&&p.id!="bottom")continue;
   if(type=="railing"&&p.id=="top")continue;
   if(type=="beam"&&p.id.StartsWith("top"))continue;
   a.Add(p);
  }
  return a.ToArray();
 }
 // Returns a base yaw for compatible anchor pairs. User rotation is applied BEFORE solving position.
 public static bool Match(string source,SnapAnchor a,string target,SnapAnchor b,bool auto,out float yaw,out float preference){
  yaw=0;preference=0;
  bool edge=b.id.StartsWith("edge");
  if(target=="floor"){
   if(source=="floor"&&edge&&a.id.StartsWith("edge")){yaw=b.yaw-a.yaw+180;return !auto||a.id=="edge0";}
   if((Kit.IsWall(source)||source=="railing")&&edge&&a.id=="bottom"){yaw=b.yaw;return true;}
   if(source=="stairs"&&edge){yaw=b.yaw+(a.id=="bottom"?180:0);preference=a.id=="bottom"?.015f:0;return true;}
   if(source=="post"&&a.id=="bottom"&&(b.id.StartsWith("corner")||b.id=="center"))return true;
   if(Kit.IsSmall(source)&&source!="post"&&source!="beam"&&b.id=="center"&&a.id=="bottom")return true;
  }
  if(Kit.IsWall(target)){
   if(b.id=="top"){
    if((Kit.IsWall(source)||source=="gable"||source=="beam")&&a.id=="bottom")return true;
    if(source=="roof"&&a.id=="eave")return true;
    if(source=="floor"&&a.id.StartsWith("edge")){yaw=-a.yaw;return !auto||a.id=="edge0";}
   }
   if(Kit.IsWall(source)&&((a.id=="left"&&b.id=="right")||(a.id=="right"&&b.id=="left")))return true;
   if(Kit.IsWall(source)&&a.id=="top"&&b.id=="bottom")return true;
  }
  if(target=="post"){
   if(source=="beam"&&b.id=="top"&&(a.id=="left"||a.id=="right"||a.id=="bottom")){preference=a.id=="left"?0:a.id=="right"?.02f:.04f;return true;}
   if(source=="post"&&((a.id=="bottom"&&b.id=="top")||(a.id=="top"&&b.id=="bottom")))return true;
  }
  if(target=="beam"){
   if(source=="beam"&&((a.id=="left"&&b.id=="right")||(a.id=="right"&&b.id=="left")))return true;
   if(source=="post"&&a.id=="bottom"&&b.id.StartsWith("top"))return true;
   if(source=="roof"&&a.id=="eave"&&b.id=="top")return true;
  }
  if(target=="stairs"&&source=="floor"&&a.id.StartsWith("edge")){
   yaw=(b.id=="bottom"?180:0)-a.yaw;return !auto||a.id=="edge0";
  }
  if(target=="stairs"&&source=="stairs"&&((a.id=="bottom"&&b.id=="top")||(a.id=="top"&&b.id=="bottom")))return true;
  if(target=="roof"&&source=="roof"){
   if((a.id=="left"&&b.id=="right")||(a.id=="right"&&b.id=="left"))return true;
   if(a.id=="ridge"&&b.id=="ridge"){yaw=180;return true;}
   if(!auto&&((a.id=="eave"&&b.id=="ridge")||(a.id=="ridge"&&b.id=="eave")))return true;
  }
  if(target=="railing"&&source=="railing"&&((a.id=="left"&&b.id=="right")||(a.id=="right"&&b.id=="left")))return true;
  return false;
 }
 public static Vector3 Solve(Vector3 contact,Quaternion rotation,Vector3 localAnchor)=>contact-rotation*localAnchor;
}
}

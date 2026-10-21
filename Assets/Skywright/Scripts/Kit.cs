using System;
using System.Collections.Generic;
using UnityEngine;

namespace Skywright {
[Serializable] public class PieceRecord {
 public string id, type, parentId="home-island";
 public int paidWood;
 public Vector3 position; public Quaternion rotation=Quaternion.identity;
}
public static class Kit {
 public static readonly string[] Ids={"floor","wall","window","doorway","roof","gable","stairs","railing","post","beam","planter","lantern","bench","wind-anchor"};
 public static readonly string[] Names={"Deck / foundation","Timber wall","Window wall","Open doorway","Roof slope","Gable end","Stairway","Deck railing","Support post","Crossbeam","Garden planter","Warm lantern","Timber bench","Wind anchor"};
 public static readonly string[] Hints={"Four-metre deck. Extend from its edges.","Full-height wall. Snaps to deck edges.","Frame a view of the sky.","Walk-through opening. No door leaf in M1.","Two slopes meet over a four-metre room.","Closes the triangular end beneath a roof.","Three-metre rise. Approach from the low end.","A safe edge for your dock.","Visual support; no structural collapse.","A four-metre architectural beam.","A little green for your new home.","Warm light for a sheltered corner.","Make a place to enjoy the view.","R enables local building hover. Space rises; Ctrl descends. E inspects anchor growth."};
 public static Material wood, trim, metal, leaf, soil, roof, glow, good, bad, selected;
 public static void Init() {
  if(wood) return;
  for(int i=0;i<Ids.Length;i++){var definition=PieceCatalog.Find(Ids[i]);if(definition!=null){Names[i]=definition.displayName;Hints[i]=definition.hint;}}
  wood=Mat("Honey cedar",new Color(.48f,.28f,.13f)); trim=Mat("Dark endgrain",new Color(.22f,.14f,.09f));
  metal=Mat("Aged brass",new Color(.38f,.39f,.34f)); leaf=Mat("Sage foliage",new Color(.30f,.49f,.22f));
  soil=Mat("Soil",new Color(.16f,.12f,.09f)); roof=Mat("Deep teal shingles",new Color(.12f,.28f,.29f));
  glow=Mat("Lantern glass",new Color(1f,.76f,.33f)); glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(1,.57f,.12f)*2);
  good=Mat("Valid preview",new Color(.28f,.46f,1f)); bad=Mat("Blocked preview",new Color(.95f,.26f,.21f)); selected=Mat("Removal highlight",new Color(1,.65f,.25f));
  foreach(var m in new[]{good,bad,selected}) {m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",m.color*.4f);}
 }
 public static Material Mat(string name, Color c) { var m=new Material(Resources.Load<Material>("SkywrightLit"));m.name=name;m.color=c;m.SetFloat("_Smoothness",.16f); return m;}
 public static GameObject Box(Transform root,string name,Vector3 pos,Vector3 size,Material m,Quaternion? rot=null,bool collision=true) {
  var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root,false);g.transform.localPosition=pos;g.transform.localRotation=rot??Quaternion.identity;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;
  if(!collision) UnityEngine.Object.Destroy(g.GetComponent<Collider>());
  return g;
 }
 static void B(Transform r,string n,float x,float y,float z,float sx,float sy,float sz,Material m) {Box(r,n,new Vector3(x,y,z),new Vector3(sx,sy,sz),m);}
 public static BuiltPiece Create(string type,Vector3 p,Quaternion q,Transform parent,bool preview=false) {
  var definition=PieceCatalog.Find(type);
  if(definition==null||!definition.prefab)return CreateLegacy(type,p,q,parent,preview);
  Init();
  var bp=UnityEngine.Object.Instantiate(definition.prefab,parent);
  bp.transform.SetPositionAndRotation(p,q);bp.name=definition.displayName;
  bp.id=Guid.NewGuid().ToString("N");bp.type=definition.typeId;
  bp.boxes=bp.GetComponentsInChildren<BoxCollider>();
  if(preview){foreach(var c in bp.GetComponentsInChildren<Collider>())c.isTrigger=true;
   foreach(var tr in bp.GetComponentsInChildren<Transform>())tr.gameObject.layer=2;
   foreach(var l in bp.GetComponentsInChildren<Light>())l.enabled=false;
  }
  return bp;
 }
 public static BuiltPiece CreateLegacy(string type,Vector3 p,Quaternion q,Transform parent,bool preview=false) {
  Init(); var root=new GameObject(Names[Array.IndexOf(Ids,type)]);root.transform.SetParent(parent,false);root.transform.SetPositionAndRotation(p,q);
  var t=root.transform;
  switch(type) {
  case "floor":
   for(int i=0;i<8;i++) B(t,"Cedar plank",-.0f,-.12f,-1.75f+i*.5f,3.98f,.24f,.48f,wood);
   B(t,"Underbeam",0,-.32f,-1.65f,4,.18f,.22f,trim);B(t,"Underbeam",0,-.32f,1.65f,4,.18f,.22f,trim);break;
  case "wall": case "window": case "doorway":
   B(t,"Left upright",-1.88f,1.5f,0,.24f,3,.26f,trim);B(t,"Right upright",1.88f,1.5f,0,.24f,3,.26f,trim);
   B(t,"Lintel",0,2.88f,0,3.65f,.24f,.24f,trim);
   if(type=="wall") {for(int i=0;i<8;i++) B(t,"Wall board",-1.56f+i*.445f,1.4f,0,.43f,2.78f,.16f,wood);}
   else if(type=="window") {
    B(t,"Sill panel",0,.52f,0,3.55f,1.04f,.16f,wood); B(t,"Upper panel",0,2.56f,0,3.55f,.4f,.16f,wood);
    B(t,"Side panel",-1.4f,1.7f,0,.74f,1.35f,.16f,wood); B(t,"Side panel",1.4f,1.7f,0,.74f,1.35f,.16f,wood);
    B(t,"Window sill",0,1.08f,0,2.2f,.12f,.36f,trim);B(t,"Window top",0,2.32f,0,2.2f,.12f,.26f,trim);
    B(t,"Window mullion",0,1.7f,0,.08f,1.2f,.12f,trim);
   } else {
    B(t,"Door left",-1.35f,1.4f,0,.85f,2.8f,.16f,wood);B(t,"Door right",1.35f,1.4f,0,.85f,2.8f,.16f,wood);
    B(t,"Door header",0,2.65f,0,1.9f,.3f,.22f,trim);
   } break;
  case "roof":
   Box(t,"Roof plane",new Vector3(0,.75f,1),new Vector3(4,.16f,2.5f),roof,Quaternion.Euler(-36.8699f,0,0));
   for(int i=0;i<5;i++) Box(t,"Shingle seam",new Vector3(0,i*.3f,i*.4f),new Vector3(4.02f,.07f,.10f),trim,Quaternion.Euler(-36.8699f,0,0));
   break;
  case "gable":
   var mesh=new Mesh(); mesh.name="Gable prism";
   mesh.vertices=new[]{new Vector3(-2,0,-.08f),new Vector3(2,0,-.08f),new Vector3(0,1.5f,-.08f),new Vector3(-2,0,.08f),new Vector3(2,0,.08f),new Vector3(0,1.5f,.08f)};
   mesh.triangles=new[]{0,2,1,3,4,5,0,3,5,0,5,2,1,2,5,1,5,4,0,1,4,0,4,3};mesh.RecalculateNormals();mesh.RecalculateBounds();
   var g=new GameObject("Gable");g.transform.SetParent(t,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=wood;var mc=g.AddComponent<MeshCollider>();mc.sharedMesh=mesh;mc.convex=true;break;
  case "stairs":
   for(int i=0;i<12;i++) B(t,"Tread",0,(i+1)*.25f-.12f,-3.85f+i*.32f,2,.24f,.34f,wood);
   Box(t,"Left stringer",new Vector3(-.85f,1.45f,-2),new Vector3(.16f,.2f,5),trim,Quaternion.Euler(-36.87f,0,0));
   Box(t,"Right stringer",new Vector3(.85f,1.45f,-2),new Vector3(.16f,.2f,5),trim,Quaternion.Euler(-36.87f,0,0));break;
  case "railing":
   B(t,"Handrail",0,1.12f,0,4,.14f,.16f,trim);B(t,"Lower rail",0,.32f,0,4,.1f,.12f,trim);
   for(int i=0;i<5;i++) B(t,"Baluster",-1.9f+i*.95f,.58f,0,.12f,1.1f,.12f,wood);break;
  case "post": B(t,"Support",0,1.3f,0,.24f,2.6f,.24f,trim);break;
  case "beam": B(t,"Beam",0,.12f,0,4,.24f,.24f,trim);break;
  case "planter":
   B(t,"Planter base",0,.18f,0,1.2f,.36f,.65f,wood);B(t,"Earth",0,.38f,0,1.05f,.08f,.5f,soil);
   for(int i=0;i<5;i++) {
    B(t,"Stem",-.42f+i*.21f,.64f,0,.04f,.5f,.04f,trim);
    Box(t,"Leaf",new Vector3(-.42f+i*.21f,.76f+(i%2)*.13f,0),new Vector3(.28f,.18f,.36f),leaf,Quaternion.Euler(0,i*31,25));
   }break;
  case "lantern":
   B(t,"Foot",0,.05f,0,.42f,.1f,.42f,metal);B(t,"Glass",0,.37f,0,.25f,.5f,.25f,glow);B(t,"Cap",0,.66f,0,.42f,.1f,.42f,metal);
   foreach(float x in new[]{-.16f,.16f}) foreach(float z in new[]{-.16f,.16f}) B(t,"Cage",x,.37f,z,.04f,.52f,.04f,metal);
   if(!preview){var l=new GameObject("Lantern light");l.transform.SetParent(t,false);l.transform.localPosition=Vector3.up*.8f;var light=l.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.7f,.36f);light.intensity=3;light.range=6;light.shadows=LightShadows.None;}
   break;
  case "bench":
   B(t,"Seat",0,.52f,0,1.8f,.16f,.62f,wood);B(t,"Back",0,1.0f,.28f,1.8f,.62f,.12f,wood);
   B(t,"Leg",-.65f,.24f,0,.14f,.5f,.5f,trim);B(t,"Leg",.65f,.24f,0,.14f,.5f,.5f,trim);break;
  }
  var bp=root.AddComponent<BuiltPiece>(); bp.id=Guid.NewGuid().ToString("N");bp.type=type;bp.boxes=root.GetComponentsInChildren<BoxCollider>();
  if(preview) {foreach(var c in root.GetComponentsInChildren<Collider>()) c.isTrigger=true; foreach(var tr in root.GetComponentsInChildren<Transform>())tr.gameObject.layer=2;}
  return bp;
 }
 public static bool IsWall(string t)=>t=="wall"||t=="window"||t=="doorway";
 public static bool IsSmall(string t)=>t=="wind-anchor"||t=="planter"||t=="lantern"||t=="bench"||t=="post"||t=="beam";
}
}

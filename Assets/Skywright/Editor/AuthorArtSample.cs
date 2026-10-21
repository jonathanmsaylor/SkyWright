using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace Skywright.Editor {
public static class AuthorArtSample {
 const string Dir="Assets/Resources/SkywrightArt";
 static Material timber,white,aqua,glass,teal,dark,earth,green,lime,flower,gold,light,stone,grass;
 static int meshId;
 static System.Random random=new System.Random(718);
 static float R(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
 [MenuItem("Skywright/Author Checkpoint A sample (explicit regeneration)")]
 public static void Author(){
  Directory.CreateDirectory(Dir);AssetDatabase.Refresh();Kit.Init();meshId=0;
  timber=Material("Pale ash",new Color(.78f,.66f,.48f),.18f);
  white=Material("Pearl ceramic",new Color(.86f,.87f,.80f),.32f);
  aqua=Material("Aqua enamel",new Color(.07f,.42f,.43f),.35f);
  teal=Material("Teal roof",new Color(.07f,.32f,.36f),.28f);
  dark=Material("Shadow joints",new Color(.20f,.23f,.20f),.15f);
  earth=Material("Garden earth",new Color(.17f,.12f,.07f),.05f);
  green=Material("Leaf green",new Color(.20f,.40f,.065f),.14f);
  lime=Material("Sunlit leaf",new Color(.39f,.57f,.12f),.17f);
  flower=Material("Ivory petals",new Color(.95f,.91f,.68f),.2f);
  gold=Material("Brushed brass",new Color(.52f,.36f,.14f),.4f);gold.SetFloat("_Metallic",.4f);
  light=Material("Warm lamp",new Color(1,.73f,.32f),.3f);light.EnableKeyword("_EMISSION");light.SetColor("_EmissionColor",new Color(1,.48f,.12f)*1.5f);
  stone=Material("Pale limestone",new Color(.66f,.65f,.53f),.08f);
  grass=Material("Sample meadow",new Color(.25f,.43f,.085f),.08f);
  glass=Material("Blue green glass",new Color(.12f,.56f,.60f,.24f),.72f);
  glass.SetFloat("_Surface",1);glass.SetFloat("_ZWrite",0);glass.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);glass.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
  glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.SetOverrideTag("RenderType","Transparent");glass.renderQueue=3000;glass.SetFloat("_Cull",2); // Closed glazing boxes: avoid drawing their backfaces twice.
  foreach(var m in new[]{green,lime,flower})m.SetFloat("_Cull",0);
  Grain(timber);Grain(stone,true);
  var catalog=AssetDatabase.LoadAssetAtPath<PieceCatalog>("Assets/Resources/SkywrightCatalog.asset");
  if(!catalog){catalog=ScriptableObject.CreateInstance<PieceCatalog>();AssetDatabase.CreateAsset(catalog,"Assets/Resources/SkywrightCatalog.asset");}
  var defs=new List<PieceDefinition>();
  foreach(string type in new[]{"floor","wall","window","doorway","roof","gable","railing","lantern","planter"}){
   var bp=Kit.CreateLegacy(type,Vector3.zero,Quaternion.identity,null,false);
   var collision=new GameObject("Collision — M1 contract").transform;collision.SetParent(bp.transform,false);
   foreach(var tr in bp.transform.Cast<Transform>().ToArray())if(tr!=collision)tr.SetParent(collision,true);
   foreach(var l in collision.GetComponentsInChildren<Light>())UnityEngine.Object.DestroyImmediate(l.gameObject);
   foreach(var r in collision.GetComponentsInChildren<Renderer>())UnityEngine.Object.DestroyImmediate(r);
   foreach(var f in collision.GetComponentsInChildren<MeshFilter>())UnityEngine.Object.DestroyImmediate(f);
   foreach(var mc in collision.GetComponentsInChildren<MeshCollider>())if(!AssetDatabase.Contains(mc.sharedMesh)){mc.sharedMesh=Persist(UnityEngine.Object.Instantiate(mc.sharedMesh),"collision_"+type);}
   var v=new GameObject("Visuals — replace freely").transform;v.SetParent(bp.transform,false);
   Visual(type,v);
   Combine(v,type);
   if(type=="lantern"){
    var lamp=new GameObject("Warm light");lamp.transform.SetParent(v,false);lamp.transform.localPosition=new Vector3(0,.8f,0);
    var l=lamp.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.76f,.45f);l.intensity=2.4f;l.range=5;l.shadows=LightShadows.None;
   }
   bp.id="";bp.boxes=bp.GetComponentsInChildren<BoxCollider>();
   var prefab=PrefabUtility.SaveAsPrefabAsset(bp.gameObject,Dir+"/"+type+".prefab").GetComponent<BuiltPiece>();
   int i=Array.IndexOf(Kit.Ids,type);
   defs.Add(new PieceDefinition{typeId=type,displayName=Kit.Names[i],hint=Kit.Hints[i],prefab=prefab,dimensions=Dimensions(type),origin=type=="floor"?"Deck top centre":type=="roof"?"Eave centre; rises inward +Z":type=="gable"?"Base centre":"Base centre; M1 pivot retained",connections=Connections(type),anchors=SnapRules.Defaults(type)});
   UnityEngine.Object.DestroyImmediate(bp.gameObject);
  }
  catalog.pieces=defs.ToArray();EditorUtility.SetDirty(catalog);
  Garden();
  var sky=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/AeroSky.mat");
  if(!sky){sky=new Material(Shader.Find("Skybox/Procedural"));AssetDatabase.CreateAsset(sky,Dir+"/AeroSky.mat");}
  sky.shader=Shader.Find("Skywright/AeroSky");
  sky.SetColor("_Zenith",new Color(.045f,.28f,.65f));sky.SetColor("_Horizon",new Color(.40f,.75f,.87f));EditorUtility.SetDirty(sky);
  AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("ART_SAMPLE_AUTHORED nine piece prefabs + garden; M1 collision retained");
 }
 static Material Material(string n,Color c,float smooth){
  string p=Dir+"/"+n.Replace(" ","_")+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);
  if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}
  m.color=c;m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
 }
 static void Grain(Material m,bool rock=false){
  var tex=new Texture2D(256,512,TextureFormat.RGB24,true);tex.name=rock?"Limestone grain":"Ash long grain";
  for(int y=0;y<512;y++)for(int x=0;x<256;x++){
   float n=rock?Mathf.PerlinNoise(x*.08f,y*.08f):Mathf.PerlinNoise(x*.21f+Mathf.Sin(y*.008f)*.25f,y*.003f);
   float v=rock?.82f+n*.18f:.91f+n*.09f;tex.SetPixel(x,y,new Color(v,v,v));
  }tex.Apply();tex.wrapMode=TextureWrapMode.Repeat;
  string path=Dir+"/"+tex.name.Replace(" ","_")+".asset";
  var old=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(old){EditorUtility.CopySerialized(tex,old);UnityEngine.Object.DestroyImmediate(tex);tex=old;}else AssetDatabase.CreateAsset(tex,path);
  m.mainTexture=tex;m.mainTextureScale=rock?Vector2.one:new Vector2(1,1);EditorUtility.SetDirty(m);
 }
 static Mesh Persist(Mesh m,string name){
  string p=Dir+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(p);
  if(old){EditorUtility.CopySerialized(m,old);UnityEngine.Object.DestroyImmediate(m);return old;}AssetDatabase.CreateAsset(m,p);return m;
 }
 static void MeshObject(Transform t,string name,Mesh m,Material mat,Vector3 pos,Quaternion rot){
  var g=new GameObject(name);g.transform.SetParent(t,false);g.transform.localPosition=pos;g.transform.localRotation=rot;
  g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterial=mat;
 }
 // Rounded solid: six gridded faces projected around a rectangular inner core.
 static Mesh Rounded(Vector3 size,float radius){
  var v=new List<Vector3>();var norms=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();
  Vector3 h=size*.5f;float r=Mathf.Min(radius,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.9f);Vector3 inner=h-Vector3.one*r;
  for(int side=0;side<6;side++){
   Vector3 n=side==0?Vector3.right:side==1?Vector3.left:side==2?Vector3.up:side==3?Vector3.down:side==4?Vector3.forward:Vector3.back;
   Vector3 u=Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.Cross(Vector3.up,n);Vector3 w=Vector3.Cross(n,u);
   float hu=Vector3.Dot(h,new Vector3(Mathf.Abs(u.x),Mathf.Abs(u.y),Mathf.Abs(u.z)));
   float hw=Vector3.Dot(h,new Vector3(Mathf.Abs(w.x),Mathf.Abs(w.y),Mathf.Abs(w.z)));
   float hn=Vector3.Dot(h,new Vector3(Mathf.Abs(n.x),Mathf.Abs(n.y),Mathf.Abs(n.z)));
   float[] a={-hu,-hu+r*.293f,-hu+r,hu-r,hu-r*.293f,hu};float[] b={-hw,-hw+r*.293f,-hw+r,hw-r,hw-r*.293f,hw};
   int start=v.Count;
   for(int j=0;j<6;j++)for(int i=0;i<6;i++){
    Vector3 p=n*hn+u*a[i]+w*b[j];Vector3 c=new Vector3(Mathf.Clamp(p.x,-inner.x,inner.x),Mathf.Clamp(p.y,-inner.y,inner.y),Mathf.Clamp(p.z,-inner.z,inner.z));
    Vector3 normal=(p-c).normalized;v.Add(c+normal*r);norms.Add(normal);uv.Add(hu>hw?new Vector2((b[j]/hw+1)*.5f,(a[i]/hu+1)*.5f):new Vector2((a[i]/hu+1)*.5f,(b[j]/hw+1)*.5f));
   }
   for(int j=0;j<5;j++)for(int i=0;i<5;i++){int k=start+j*6+i;tri.AddRange(new[]{k,k+1,k+7,k,k+7,k+6});}
  }
  var mesh=new Mesh();mesh.SetVertices(v);mesh.SetNormals(norms);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);mesh.RecalculateBounds();return mesh;
 }
 static void Box(Transform t,string n,Vector3 p,Vector3 s,Material m,float r=.035f,Quaternion? q=null)=>MeshObject(t,n,Rounded(s,r),m,p,q??Quaternion.identity);
 static void Bar(Transform t,string n,Vector3 a,Vector3 b,float width,Material m){
  Vector3 d=b-a;Box(t,n,(a+b)*.5f,new Vector3(width,d.magnitude,width),m,width*.25f,Quaternion.FromToRotation(Vector3.up,d));
 }
 static void Visual(string type,Transform t){
  if(type=="floor"){
   for(int i=0;i<8;i++)Box(t,"Ash deck board",new Vector3(0,-.12f,-1.75f+i*.5f),new Vector3(3.98f,.24f,.48f),timber,.018f);
   foreach(float z in new[]{-1.65f,1.65f})Box(t,"White underside bearer",new Vector3(0,-.32f,z),new Vector3(4,.18f,.22f),white);
  }else if(Kit.IsWall(type)){
   foreach(float x in new[]{-1.88f,1.88f})Box(t,"Pearl upright",new Vector3(x,1.5f,0),new Vector3(.23f,3,.25f),white,.10f);
   Box(t,"Pearl lintel",new Vector3(0,2.88f,0),new Vector3(3.65f,.24f,.24f),white,.07f);
   if(type=="wall"){
    Box(t,"Board backing",new Vector3(0,1.4f,0),new Vector3(3.53f,2.74f,.08f),timber,.008f);
    for(int i=0;i<8;i++)Box(t,"Ash board",new Vector3(-1.56f+i*.445f,1.4f,0),new Vector3(.43f,2.78f,.16f),timber,.012f);
   }
   if(type=="window"){
    for(int i=0;i<8;i++){Box(t,"Sill boarding",new Vector3(-1.56f+i*.445f,.52f,0),new Vector3(.43f,1.04f,.16f),timber,.012f);Box(t,"Header boarding",new Vector3(-1.56f+i*.445f,2.56f,0),new Vector3(.43f,.4f,.16f),timber,.012f);}
    foreach(float x in new[]{-1.4f,1.4f})Box(t,"Side panel",new Vector3(x,1.7f,0),new Vector3(.74f,1.32f,.16f),timber);
    Box(t,"Window sill",new Vector3(0,1.08f,0),new Vector3(2.2f,.12f,.36f),white);
    Box(t,"Window crown",new Vector3(0,2.32f,0),new Vector3(2.2f,.12f,.26f),white);
    foreach(float x in new[]{-1.04f,1.04f})Box(t,"Aqua reveal",new Vector3(x,1.7f,0),new Vector3(.06f,1.16f,.18f),aqua,.025f);
    Box(t,"Mullion",new Vector3(0,1.7f,0),new Vector3(.08f,1.2f,.12f),white);
    foreach(float x in new[]{-.52f,.52f})Box(t,"Glazing",new Vector3(x,1.7f,0),new Vector3(.97f,1.1f,.025f),glass,.01f);
   }
   if(type=="doorway"){
    foreach(float x in new[]{-1.35f,1.35f}){Box(t,"Ash side",new Vector3(x,1.4f,0),new Vector3(.85f,2.78f,.16f),timber);
     Box(t,"Entry reveal",new Vector3(Mathf.Sign(x)*.94f,1.25f,.015f),new Vector3(.07f,2.5f,.19f),white,.03f);}
    Box(t,"Door crown",new Vector3(0,2.65f,0),new Vector3(1.9f,.3f,.22f),white,.075f);
   }
  }else if(type=="roof"){
   var q=Quaternion.Euler(-36.8699f,0,0);Vector3 center=new Vector3(0,.75f,1);
   Box(t,"Ash ceiling",center,new Vector3(3.86f,.16f,2.5f),timber,.018f,q);
   for(int i=0;i<8;i++)Box(t,"Teal standing seam panel",center+q*new Vector3(-1.75f+i*.5f,.085f,0),new Vector3(.485f,.025f,2.5f),teal,.008f,q);
   foreach(float x in new[]{-1.94f,1.94f})Box(t,"White verge",center+q*new Vector3(x,0,0),new Vector3(.12f,.18f,2.5f),white,.035f,q);
   Box(t,"Eave fascia",new Vector3(0,0,0),new Vector3(4.02f,.14f,.14f),white);
  }else if(type=="gable"){
   var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-2,0,-.075f),new Vector3(2,0,-.075f),new Vector3(0,1.5f,-.075f),new Vector3(-2,0,.075f),new Vector3(2,0,.075f),new Vector3(0,1.5f,.075f)};
   mesh.triangles=new[]{0,2,1,3,4,5,0,3,5,0,5,2,1,2,5,1,5,4,0,1,4,0,4,3};mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.zero,Vector2.right,Vector2.up};mesh.RecalculateNormals();
   MeshObject(t,"Ash gable",mesh,timber,Vector3.zero,Quaternion.identity);
   Bar(t,"Gable edge",new Vector3(-1.92f,.02f,0),new Vector3(0,1.46f,0),.11f,white);Bar(t,"Gable edge",new Vector3(0,1.46f,0),new Vector3(1.92f,.02f,0),.11f,white);
   Box(t,"Gable sill",new Vector3(0,.04f,0),new Vector3(3.9f,.10f,.18f),white);
  }else if(type=="railing"){
   Box(t,"Timber handrail",new Vector3(0,1.12f,0),new Vector3(4,.14f,.16f),timber);
   Box(t,"Pearl lower rail",new Vector3(0,.32f,0),new Vector3(4,.1f,.10f),white);
   for(int i=0;i<5;i++)Box(t,"Pearl baluster",new Vector3(-1.9f+i*.95f,.58f,0),new Vector3(.12f,1.1f,.12f),white,.04f);
   for(int i=0;i<4;i++)Box(t,"Aqua glass infill",new Vector3(-1.425f+i*.95f,.70f,0),new Vector3(.81f,.68f,.024f),glass,.01f);
  }else if(type=="planter"){
   Box(t,"Ceramic planter",new Vector3(0,.18f,0),new Vector3(1.2f,.36f,.65f),white,.09f);
   Box(t,"Aqua inset",new Vector3(0,.10f,.319f),new Vector3(1.02f,.045f,.014f),aqua,.006f);
   Box(t,"Earth",new Vector3(0,.38f,0),new Vector3(1.05f,.08f,.5f),earth);
   for(int i=0;i<5;i++){Vector3 p=new Vector3(-.42f+i*.21f,.42f,0);Fern(t,p,.52f+(i%2)*.13f,i*68);}
   for(int i=0;i<8;i++)Flower(t,new Vector3(R(-.48f,.48f),R(.65f,.85f),R(-.2f,.2f)),.09f);
  }else if(type=="lantern"){
   Box(t,"Brass foot",new Vector3(0,.05f,0),new Vector3(.42f,.1f,.42f),gold);
   Box(t,"Warm diffuser",new Vector3(0,.37f,0),new Vector3(.25f,.5f,.25f),light,.05f);
   Box(t,"Pearl cap",new Vector3(0,.66f,0),new Vector3(.42f,.1f,.42f),white);
   foreach(float x in new[]{-.16f,.16f})foreach(float z in new[]{-.16f,.16f})Box(t,"Brass cage",new Vector3(x,.37f,z),new Vector3(.04f,.52f,.04f),gold,.01f);
  }
 }
 static Vector3 Dimensions(string t)=>t=="floor"?new Vector3(4,.41f,4):Kit.IsWall(t)?new Vector3(4,3,.36f):t=="roof"?new Vector3(4,1.6f,2.1f):t=="gable"?new Vector3(4,1.5f,.16f):t=="railing"?new Vector3(4,1.19f,.16f):t=="planter"?new Vector3(1.2f,1,.65f):new Vector3(.42f,.71f,.42f);
 static PieceConnection[] Connections(string type){
  var s=new List<PieceConnection>();
  void Add(string accepts,Vector3 pos,Vector3 euler,float height=0){s.Add(new PieceConnection{accepts=accepts,position=pos,euler=euler,aimHeight=height});}
  if(type=="floor"){
   foreach(var p in new[]{Vector3.right*4,Vector3.left*4,Vector3.forward*4,Vector3.back*4})Add("floor",p,Vector3.zero);
   for(int j=0;j<4;j++){Vector3 e=new Vector3(0,j*90,0),p=Quaternion.Euler(e)*new Vector3(0,0,-2);Add("wall,window,doorway,railing",p,e);Add("stairs",p-Vector3.up*3,e,1.5f);}
   foreach(float x in new[]{-1.85f,1.85f})foreach(float z in new[]{-1.85f,1.85f})Add("post",new Vector3(x,0,z),Vector3.zero);
  }
  if(Kit.IsWall(type)){Add("roof,gable,beam,wall,window,doorway",Vector3.up*3,Vector3.zero);Add("floor",new Vector3(0,3,2),Vector3.zero);}
  return s.ToArray();
 }
 static void Combine(Transform root,string name){
  var fs=root.GetComponentsInChildren<MeshFilter>();
  foreach(var group in fs.GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial)){
   var combines=group.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray();
   var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(combines,true,true);mesh.name=name+" "+group.Key.name;
   MeshObject(root,group.Key.name,Persist(mesh,name+"_"+meshId++),group.Key,Vector3.zero,Quaternion.identity);
  }
  foreach(var f in fs){var mesh=f.sharedMesh;UnityEngine.Object.DestroyImmediate(f.gameObject);if(!AssetDatabase.Contains(mesh))UnityEngine.Object.DestroyImmediate(mesh);}
 }
 static void Leaf(Transform t,Vector3 p,Vector3 size,Quaternion rot,Material mat){
  var m=new Mesh();m.vertices=new[]{new Vector3(0,0,0),new Vector3(-.42f,.35f,0),new Vector3(-.3f,.73f,.08f),new Vector3(0,1,.02f),new Vector3(.3f,.73f,.08f),new Vector3(.42f,.35f,0),new Vector3(0,.5f,.14f)}.Select(v=>Vector3.Scale(v,size)).ToArray();
  m.triangles=new[]{0,1,6,1,2,6,2,3,6,3,4,6,4,5,6,5,0,6};m.RecalculateNormals();m.RecalculateBounds();MeshObject(t,"Leaf",m,mat,p,rot);
 }
 static void Fern(Transform t,Vector3 p,float size,float angle){
  for(int i=0;i<7;i++){var q=Quaternion.Euler(45,angle+i*51,0);
   for(int j=0;j<4;j++){Vector3 c=p+q*new Vector3(0,size*(j*.19f),0);
    Leaf(t,c,new Vector3(size*.28f,size*.45f,size),q*Quaternion.Euler(0,0,42),i%2==0?green:lime);
    Leaf(t,c,new Vector3(size*.28f,size*.45f,size),q*Quaternion.Euler(0,0,-42),green);
   }
  }
 }
 static void Flower(Transform t,Vector3 p,float s){
  for(int j=0;j<5;j++)Leaf(t,p,new Vector3(s,s*1.5f,s),Quaternion.Euler(75,j*72,0),flower);
 }
 static void Garden(){
  var g=new GameObject("Checkpoint A garden — visual only");var t=g.transform;
  // A small edge garden; the original island collider, clearing, and distant scenery remain intact.
  var m=new Mesh();int n=40;var v=new List<Vector3>{new Vector3(-7,.012f,-12)};var tri=new List<int>();
  for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;v.Add(new Vector3(-7+Mathf.Cos(a)*(4.5f+.25f*Mathf.Sin(i*2)),.012f,-12+Mathf.Sin(a)*4.5f));}
  for(int i=0;i<n;i++)tri.AddRange(new[]{0,1+(i+1)%n,1+i});m.SetVertices(v);m.SetTriangles(tri,0);m.RecalculateNormals();MeshObject(t,"Meadow sample",m,grass,Vector3.zero,Quaternion.identity);
  for(int i=0;i<46;i++){
   float a=R(0,6.283f),r=R(2,4.5f);Vector3 p=new Vector3(-7+Mathf.Cos(a)*r,0,-12+Mathf.Sin(a)*r);
   Fern(t,p,R(.18f,.42f),R(0,360));if(i%2==0)Flower(t,p+Vector3.up*.3f,.08f);
  }
  Tree(t,new Vector3(-8,0,-11),1);Tree(t,new Vector3(-11,0,-15),.65f);
  for(int i=0;i<11;i++){float a=i*.55f;Box(t,"Limestone border",new Vector3(-7+Mathf.Cos(a)*1.9f,.14f,-13+Mathf.Sin(a)*1.25f),new Vector3(.85f,.28f,.55f),stone,.12f,Quaternion.Euler(0,-a*Mathf.Rad2Deg,0));}
  Box(t,"Pool limestone basin",new Vector3(-7,.07f,-13),new Vector3(3.65f,.12f,2.4f),stone,.05f);
  Box(t,"Bounded turquoise pool",new Vector3(-7,.14f,-13),new Vector3(3.3f,.025f,2.1f),aqua,.01f);
  for(int i=0;i<10;i++)Box(t,"Water glint",new Vector3(-7+R(-1.4f,1.4f),.155f,-13+R(-.8f,.8f)),new Vector3(R(.1f,.5f),.005f,.018f),glass,.001f);
  // Decorative cliff accents below the south-west rim, outside the playable surface.
  for(int i=0;i<12;i++){Vector3 p=new Vector3(-10+i*.48f,-1.3f-i%3*.4f,-18.2f-i*.12f);
   Box(t,"Limestone strata",p,new Vector3(.7f,2.2f+i%3*.3f,.65f),stone,.15f,Quaternion.Euler(0,i*14,i%2*9));
   Bar(t,"Exposed root",p+Vector3.up*1.4f,p+Vector3.down*.7f+Vector3.forward*.5f,.075f,timber);
   for(int j=0;j<6;j++)Leaf(t,p+Vector3.down*(j*.22f)+Vector3.back*.4f,new Vector3(.3f,.4f,.2f),Quaternion.Euler(15,i*32+j*51,160),green);
  }
  Combine(t,"garden");
  PrefabUtility.SaveAsPrefabAsset(g,Dir+"/SampleGarden.prefab");UnityEngine.Object.DestroyImmediate(g);
 }
 static void Tree(Transform t,Vector3 p,float scale){
  Bar(t,"Ash tree trunk",p,p+new Vector3(.3f,3.4f,0)*scale,.28f*scale,timber);
  for(int k=0;k<14;k++){
   float a=k*2.399f;Vector3 c=p+new Vector3(Mathf.Cos(a)*(1.4f+k%3*.3f),3.6f+k%4*.45f,Mathf.Sin(a)*(1.4f+k%3*.3f))*scale;
   Bar(t,"Branch",p+Vector3.up*2.6f*scale,c,.08f*scale,timber);
   for(int j=0;j<90;j++){
    Vector3 offset=new Vector3(R(-.9f,.9f),R(-.55f,.55f),R(-.9f,.9f))*scale;
    Leaf(t,c+offset,new Vector3(.45f,.62f,.35f)*scale,Quaternion.Euler(R(20,160),R(0,360),R(0,360)),j%3==0?lime:green);
   }
  }
 }
}
}
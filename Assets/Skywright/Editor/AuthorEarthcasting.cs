using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace Skywright.Editor {
public static class AuthorEarthcasting {
 const string Dir="Assets/Resources/Earthcasting";
 static Material timber,white,aqua,dark,earth,green,lime,flower,stone,magic;
 static int meshId;static System.Random random=new System.Random(916);
 static float R(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
 [MenuItem("Skywright/Author Earthcasting assets (explicit regeneration)")]
 public static void Author(){
  Directory.CreateDirectory(Dir);AssetDatabase.Refresh();meshId=0;random=new System.Random(916);
  timber=Material("Exposed ash",new Color(.71f,.52f,.28f),.12f);white=Material("Pearl focus",new Color(.83f,.87f,.81f),.3f);
  aqua=Material("Aqua fittings",new Color(.08f,.40f,.40f),.35f);dark=Material("Ash bark",new Color(.34f,.30f,.22f),.08f);
  earth=Material("Natural earth",new Color(.20f,.15f,.09f),.06f);stone=Material("Limestone",new Color(.65f,.64f,.51f),.1f);
  green=Material("Ash leaves",new Color(.16f,.34f,.055f),.12f);lime=Material("Ash sun leaves",new Color(.36f,.52f,.09f),.12f);
  flower=timber;magic=Material("Earth magic",new Color(.20f,.70f,.69f),.4f);magic.EnableKeyword("_EMISSION");magic.SetColor("_EmissionColor",new Color(.08f,.42f,.40f)*1.4f);
  green.SetFloat("_Cull",0);lime.SetFloat("_Cull",0);Grain(timber);Grain(dark);Grain(stone,true);
  var c=AssetDatabase.LoadAssetAtPath<EarthConfig>(Dir+"/Tuning.asset");if(!c){c=ScriptableObject.CreateInstance<EarthConfig>();AssetDatabase.CreateAsset(c,Dir+"/Tuning.asset");}
  c.tree=Tree();c.outcrop=Deposit(false);c.pile=Deposit(true);c.rock=Rock();c.wood=Wood();c.focus=Focus();c.crate=Crate();
  c.magic=magic;c.fracture=timber;c.bark=dark;c.stone=stone;c.leaf=green;
  c.prepare=Sound("01_prepare",.48f,0);c.lift=Sound("02_lift",.58f,1);c.launch=Sound("03_launch",.24f,2);
  c.impact=Sound("04_wood_impact",.45f,3);c.crack=Sound("05_trunk_crack",.75f,4);c.fall=Sound("06_fall",1.0f,5);c.collect=Sound("07_collect",.6f,6);
  EditorUtility.SetDirty(c);AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("EARTH_ASSETS_AUTHORED original tree, focus, resources, crates and seven sound layers");
 }
 static GameObject Save(GameObject g,string name){var p=PrefabUtility.SaveAsPrefabAsset(g,Dir+"/"+name+".prefab");UnityEngine.Object.DestroyImmediate(g);return p;}
 static Transform Child(Transform parent,string name){var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
 static void Tube(Transform root,string name,Vector3 a,Vector3 b,float bottom,float top,Material mat){
  const int n=12;var v=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();var q=Quaternion.FromToRotation(Vector3.up,b-a);
  for(int y=0;y<2;y++)for(int i=0;i<n;i++){float angle=i*Mathf.PI*2/n;Vector3 p=new Vector3(Mathf.Cos(angle)*(y==0?bottom:top),y*(b-a).magnitude,Mathf.Sin(angle)*(y==0?bottom:top));v.Add(a+q*p);uv.Add(new Vector2(i/(float)n,y));}
  for(int i=0;i<n;i++){int j=(i+1)%n;tri.AddRange(new[]{i,n+i,n+j,i,n+j,j});}
  v.Add(a);uv.Add(Vector2.zero);v.Add(b);uv.Add(Vector2.one);for(int i=0;i<n;i++){int j=(i+1)%n;tri.AddRange(new[]{24,i,j,25,n+j,n+i});}
  var mesh=new Mesh();mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();MeshObject(root,name,mesh,mat,Vector3.zero,Quaternion.identity);
 }
 static GameObject Tree(){
  var g=new GameObject("Harvestable ash");var n=g.AddComponent<EarthNode>();var crown=Child(g.transform,"Crown — replaceable visual");n.crown=crown;
  Tube(crown,"Living ash trunk",new Vector3(0,.25f,0),new Vector3(.08f,3.7f,.06f),.31f,.16f,dark);
  for(int k=0;k<12;k++){float a=k*2.399f;var c=new Vector3(Mathf.Cos(a)*(1.1f+k%3*.3f),3.5f+k%4*.48f,Mathf.Sin(a)*(1.1f+k%3*.3f));
   Tube(crown,"Branch",new Vector3(.05f,2.4f+k%3*.3f,0),c,.09f,.025f,dark);
   for(int j=0;j<55;j++)Leaf(crown,c+new Vector3(R(-.7f,.7f),R(-.45f,.45f),R(-.7f,.7f)),new Vector3(.38f,.57f,.3f),Quaternion.Euler(R(20,160),R(0,360),R(0,360)),j%3==0?lime:green);
  }
  Combine(crown,"tree");var stump=Child(g.transform,"Stump");Tube(stump,"Persistent stump",Vector3.zero,Vector3.up*.3f,.34f,.31f,dark);Tube(stump,"Cut heart",Vector3.up*.301f,Vector3.up*.31f,.26f,.26f,timber);Combine(stump,"stump");
  var collider=Child(g.transform,"Trunk targeting and collision").gameObject.AddComponent<CapsuleCollider>();collider.center=new Vector3(.03f,1.8f,0);collider.radius=.30f;collider.height=3.6f;n.harvestingCollider=collider;
  return Save(g,"AshTree");
 }
 static Mesh Boulder(){
  int sides=12,rings=7;var v=new List<Vector3>();var ts=new List<int>();var uv=new List<Vector2>();
  for(int y=0;y<=rings;y++)for(int i=0;i<=sides;i++){float a=i*Mathf.PI*2/sides,b=y*Mathf.PI/rings;float r=.5f*(1+.12f*Mathf.Sin(i*3.7f+y*2.1f));v.Add(new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b)*.8f,Mathf.Sin(b)*Mathf.Sin(a))*r);uv.Add(new Vector2(i/(float)sides,y/(float)rings));}
  for(int y=0;y<rings;y++)for(int i=0;i<sides;i++){int k=y*(sides+1)+i;ts.AddRange(new[]{k,k+1,k+sides+1,k+1,k+sides+2,k+sides+1});}
  var m=new Mesh();m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(ts,0);m.RecalculateNormals();return m;
 }
 static GameObject Rock(){var g=new GameObject("Lifted limestone and earth");MeshObject(g.transform,"Weathered limestone",Boulder(),stone,Vector3.zero,Quaternion.identity);for(int i=0;i<5;i++)Box(g.transform,"Earth clod",new Vector3(R(-.22f,.22f),R(-.25f,-.15f),R(-.22f,.22f)),Vector3.one*R(.12f,.22f),earth,.04f);Combine(g.transform,"projectile");foreach(var t in g.GetComponentsInChildren<Transform>())t.gameObject.layer=2;return Save(g,"EarthChunk");}
 static GameObject Deposit(bool pile){
  var g=new GameObject(pile?"Loose stones":"Limestone outcrop");var n=g.AddComponent<EarthNode>();n.crown=Child(g.transform,"Visuals");
  for(int i=0;i<(pile?7:4);i++){var t=Child(n.crown,"Stone");t.localPosition=new Vector3(R(-.35f,.35f),pile?.13f:.3f+i*.12f,R(-.3f,.3f));t.localScale=Vector3.one*(pile?R(.25f,.45f):R(1,1.5f));MeshObject(t,"Limestone",Boulder(),stone,Vector3.zero,Quaternion.Euler(R(0,45),R(0,360),0));}
  Combine(n.crown,pile?"pile":"outcrop");var bc=g.AddComponent<BoxCollider>();bc.center=Vector3.up*(pile?.16f:.5f);bc.size=pile?new Vector3(.85f,.35f,.8f):new Vector3(1.4f,1.2f,1.3f);n.harvestingCollider=bc;return Save(g,pile?"LooseStone":"Outcrop");
 }
 static GameObject Wood(){var g=new GameObject("Wood yield — 12 wood");for(int i=0;i<3;i++){Vector3 a=new Vector3((i-1)*.2f,0,-.4f),b=a+Vector3.forward*.8f;Tube(g.transform,"Ash log",a,b,.12f,.11f,dark);Tube(g.transform,"Visible endgrain",b,b+Vector3.forward*.01f,.095f,.095f,timber);}Combine(g.transform,"wood");foreach(var t in g.GetComponentsInChildren<Transform>())t.gameObject.layer=2;return Save(g,"WoodBundle");}
 static GameObject Focus(){
  var g=new GameObject("Earthcasting focus");Tube(g.transform,"Ash grip",new Vector3(0,-.2f,0),new Vector3(0,.05f,0),.055f,.045f,timber);
  Box(g.transform,"Pearl palm rest",Vector3.zero,new Vector3(.20f,.07f,.15f),white,.025f);
  for(int i=0;i<4;i++){float a=i*Mathf.PI/2;var p=new Vector3(Mathf.Cos(a)*.09f,.15f,Mathf.Sin(a)*.09f);Bar(g.transform,"Pearl focus arm",new Vector3(p.x*.8f,.02f,p.z*.8f),p,.035f,white);Box(g.transform,"Aqua inlay",p,new Vector3(.04f,.05f,.04f),aqua,.012f);}
  var t=Child(g.transform,"Suspended focus stone");t.localPosition=Vector3.up*.14f;t.localScale=Vector3.one*.13f;MeshObject(t,"Bound stone",Boulder(),magic,Vector3.zero,Quaternion.identity);Combine(g.transform,"focus");foreach(var tr in g.GetComponentsInChildren<Transform>())tr.gameObject.layer=2;return Save(g,"CastingFocus");
 }
 static GameObject Crate(){
  var g=new GameObject("Cargo crate — future air-magic test prop");
  Box(g.transform,"Ash cargo box",new Vector3(0,.5f,0),new Vector3(.92f,.94f,.92f),timber,.05f);
  foreach(float x in new[]{-.47f,.47f})foreach(float z in new[]{-.47f,.47f})Box(g.transform,"Pearl corner",new Vector3(x,.5f,z),new Vector3(.085f,1,.085f),white,.025f);
  foreach(float y in new[]{.08f,.9f}){Box(g.transform,"Front binding",new Vector3(0,y,-.48f),new Vector3(1,.09f,.04f),aqua,.015f);Box(g.transform,"Rear binding",new Vector3(0,y,.48f),new Vector3(1,.09f,.04f),aqua,.015f);}
  Combine(g.transform,"crate");var bc=g.AddComponent<BoxCollider>();bc.center=Vector3.up*.5f;bc.size=Vector3.one;return Save(g,"CargoCrate");
 }
 static AudioClip Sound(string name,float seconds,int kind){
  const int rate=22050;int count=(int)(seconds*rate);var rng=new System.Random(812+kind);float filtered=0;
  string path=Dir+"/"+name+".wav";
  using(var writer=new BinaryWriter(File.Create(path))){writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
   for(int i=0;i<count;i++){float t=i/(float)rate,u=t/seconds,noise=(float)rng.NextDouble()*2-1;filtered=Mathf.Lerp(filtered,noise,.13f);float s=0;
    if(kind==0)s=(Mathf.Sin(2*Mathf.PI*(170*t+110*t*t))*.35f+Mathf.Sin(2*Mathf.PI*380*t)*.1f)*Mathf.Sin(u*Mathf.PI);
    if(kind==1)s=(filtered*.9f+Mathf.Sin(2*Mathf.PI*58*t)*.25f)*Mathf.Sin(u*Mathf.PI)*.8f+noise*.15f*Mathf.Pow(1-u,3);
    if(kind==2)s=(noise-filtered)*Mathf.Sin(u*Mathf.PI)*.5f;
    if(kind==3)s=(Mathf.Sin(2*Mathf.PI*115*t)*.6f+Mathf.Sin(2*Mathf.PI*237*t)*.24f+noise*.45f)*Mathf.Exp(-t*15);
    if(kind==4){foreach(float onset in new[]{0f,.11f,.19f,.34f})if(t>onset)s+=(noise*.65f+Mathf.Sin(2*Mathf.PI*83*t)*.2f)*Mathf.Exp(-(t-onset)*35);}
    if(kind==5)s=(filtered*.9f+Mathf.Sin(2*Mathf.PI*47*t)*.42f)*Mathf.Exp(-t*5)+noise*.1f*Mathf.Sin(u*Mathf.PI);
    if(kind==6){int note=Mathf.Min(3,(int)(t/.12f));float hz=new[]{440f,554.37f,659.25f,880f}[note];s=Mathf.Sin(2*Mathf.PI*hz*t)*Mathf.Exp(-(t% .12f)*22)*Mathf.Sin(u*Mathf.PI)*.32f;}
    s*=Mathf.Clamp01(t*200)*Mathf.Clamp01((seconds-t)*100);writer.Write((short)(Mathf.Clamp(s,-.95f,.95f)*32767));
   }
  }
  AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
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

}
}

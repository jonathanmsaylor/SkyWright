using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace Skywright.Editor {
public static class NaturalAeroAuthor {
 const string D="Assets/Resources/NaturalAero",T="Assets/NaturalAero/Textures";
 static System.Random rng=new System.Random(1807);static float R(float a,float b)=>Mathf.Lerp(a,b,(float)rng.NextDouble());
 static Material wood,white,leaf,bark,rock,fern,grass,petal;static int serial;
 static TAsset Persist<TAsset>(TAsset obj,string name) where TAsset:UnityEngine.Object {
  string path=D+"/"+name;var old=AssetDatabase.LoadAssetAtPath<TAsset>(path);if(old){EditorUtility.CopySerialized(obj,old);UnityEngine.Object.DestroyImmediate(obj);return old;}AssetDatabase.CreateAsset(obj,path);return obj;
 }
 static Texture2D Tex(string n,bool normal=false,bool linear=false){
  string path=T+"/"+n;var im=AssetImporter.GetAtPath(path) as TextureImporter;if(im==null)throw new Exception("Missing texture "+path);
  im.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;im.sRGBTexture=!linear&&!normal;im.maxTextureSize=n.EndsWith(".hdr")?4096:2048;im.mipmapEnabled=true;im.anisoLevel=8;im.textureCompression=TextureImporterCompression.CompressedHQ;im.alphaIsTransparency=n.Contains("RGBA");im.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
 }
 static Material Mat(string name,Color color,float smooth=.15f){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;return Persist(m,name+".mat");}
 static void Maps(Material m,string prefix,float normal=.4f){m.SetTexture("_BaseMap",Tex(prefix+"_Color.jpg"));m.SetTexture("_BumpMap",Tex(prefix+"_NormalGL.jpg",true));m.SetFloat("_BumpScale",normal);m.EnableKeyword("_NORMALMAP");m.SetTexture("_MetallicGlossMap",Tex(prefix+"_Smooth.png",false,true));m.EnableKeyword("_METALLICSPECGLOSSMAP");m.SetFloat("_Smoothness",1);EditorUtility.SetDirty(m);}
 static void Cutout(Material m,string rgba){m.SetTexture("_BaseMap",Tex(rgba));m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.45f);m.SetFloat("_Cull",0);m.EnableKeyword("_ALPHATEST_ON");m.renderQueue=2450;m.SetOverrideTag("RenderType","TransparentCutout");EditorUtility.SetDirty(m);}
 [MenuItem("Skywright/M1.8 Author visual sample (explicit regeneration)")]
 public static void Author(){
  Directory.CreateDirectory(D);AssetDatabase.Refresh();rng=new System.Random(1807);serial=0;
  wood=Mat("Birch",new Color(1,.97f,.90f));Maps(wood,"Wood091A",.22f);
  white=Mat("Pearl",new Color(.87f,.91f,.88f),.30f);
  rock=Mat("Limestone",new Color(.88f,.89f,.80f));Maps(rock,"Rock030",.65f);
  leaf=Mat("Leaf",new Color(1.5f,1.75f,1.1f),.18f);Cutout(leaf,"Leaf_RGBA.png");
  fern=Mat("Fern",new Color(1.15f,1.35f,.92f),.12f);Cutout(fern,"Fern_RGBA.png");fern.SetTexture("_BumpMap",Tex("Fern_NormalGL.jpg",true));fern.EnableKeyword("_NORMALMAP");fern.SetFloat("_BumpScale",.35f);
  bark=Mat("Bark",new Color(.82f,.79f,.69f),.10f);bark.SetTexture("_BaseMap",Tex("Bark_Color.png"));bark.SetTexture("_BumpMap",Tex("Bark_NormalGL.png",true));bark.EnableKeyword("_NORMALMAP");bark.SetFloat("_BumpScale",.55f);
  grass=Mat("Blades",new Color(.31f,.48f,.12f),.04f);grass.SetFloat("_Cull",0);
  petal=Mat("Petals",new Color(.94f,.93f,.72f),.13f);petal.SetFloat("_Cull",0);
  Catalog();Tree();Garden();
  var sky=new Material(Shader.Find("Skybox/Panoramic"));sky.SetTexture("_MainTex",Tex("Sky4k.hdr"));sky.SetFloat("_Exposure",1.12f);sky.SetColor("_Tint",new Color(.44f,.55f,.61f));sky.SetFloat("_Rotation",105);Persist(sky,"Sky.mat");
  var meadow=new Material(Shader.Find("Skywright/NaturalMeadow"));meadow.SetTexture("_BaseMap",Tex("Grass004_Color.jpg"));Persist(meadow,"Meadow.mat");
  Demo();AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("NATURAL_AERO_AUTHORED isolated catalog, one tree design, limited garden, unchanged contracts");
 }
 static void Catalog(){
  var original=Resources.Load<PieceCatalog>("SkywrightCatalog");var cat=UnityEngine.Object.Instantiate(original);
  foreach(var def in cat.pieces){
   var g=UnityEngine.Object.Instantiate(def.prefab).gameObject;
   foreach(var r in g.GetComponentsInChildren<MeshRenderer>()){
    var m=r.sharedMaterial;string n=m.name;
    if(n=="Pale ash"){
     r.sharedMaterial=wood;var f=r.GetComponent<MeshFilter>();var mesh=UnityEngine.Object.Instantiate(f.sharedMesh);var v=mesh.vertices;var norms=mesh.normals;var uv=new Vector2[v.Length];
     // World-scale grain follows the long axis of each functional board class.
     for(int i=0;i<v.Length;i++){
      Vector3 a=v[i],normal=norms[i];
      if(def.typeId=="floor"||def.typeId=="railing")uv[i]=Mathf.Abs(normal.y)>.5f?new Vector2(a.z/1.4f,a.x/1.4f):new Vector2(a.y/1.4f,a.x/1.4f);
      else if(def.typeId=="roof")uv[i]=new Vector2(a.x/1.4f,(a.z*.8f+a.y*.6f)/1.4f);
      else uv[i]=Mathf.Abs(normal.z)>.5f?new Vector2(a.x/1.4f,a.y/1.4f):new Vector2(a.z/1.4f,a.y/1.4f);
     }mesh.uv=uv;mesh.RecalculateTangents();f.sharedMesh=Persist(mesh,def.typeId+"_birch.asset");
    } else if(n=="Pearl ceramic")r.sharedMaterial=white;
   }
   if(def.typeId=="lantern")foreach(var light in g.GetComponentsInChildren<Light>()){light.intensity=.9f;light.range=3.8f;}
   def.prefab=PrefabUtility.SaveAsPrefabAsset(g,D+"/"+def.typeId+".prefab").GetComponent<BuiltPiece>();UnityEngine.Object.DestroyImmediate(g);
  }Persist(cat,"Catalog.asset");
 }
 static Transform Child(Transform root,string name){var t=new GameObject(name).transform;t.SetParent(root,false);return t;}
 static void MeshObject(Transform t,Mesh mesh,Material mat,string name){var g=Child(t,name);g.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;}
 static Mesh Finish(List<Vector3> v,List<int> tri,List<Vector2> uv,string name){var m=new Mesh{indexFormat=IndexFormat.UInt32,name=name};m.SetVertices(v);m.SetTriangles(tri,0);m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();return m;}
 static void Tube(Transform root,Vector3 a,Vector3 b,float r0,float r1,Material mat){
  var v=new List<Vector3>();var tri=new List<int>();var uv=new List<Vector2>();const int sides=12,rings=6;var q=Quaternion.FromToRotation(Vector3.up,b-a);
  for(int k=0;k<=rings;k++)for(int j=0;j<=sides;j++){float f=k/(float)rings,ang=j*2*Mathf.PI/sides,r=Mathf.Lerp(r0,r1,f)*(1+.06f*Mathf.Sin(j*2.3f+k));
   v.Add(Vector3.Lerp(a,b,f)+q*new Vector3(Mathf.Cos(ang)*r,0,Mathf.Sin(ang)*r));uv.Add(new Vector2(j/(float)sides*2,(b-a).magnitude*f/1.4f));
  }
  for(int k=0;k<rings;k++)for(int j=0;j<sides;j++){int i=k*(sides+1)+j;tri.AddRange(new[]{i,i+sides+1,i+1,i+1,i+sides+1,i+sides+2});}
  // End caps prevent hollow branches and stump tops.
  for(int end=0;end<2;end++){int c=v.Count;v.Add(end==0?a:b);uv.Add(new Vector2(.5f,.5f));int start=end*rings*(sides+1);for(int j=0;j<sides;j++){if(end==0)tri.AddRange(new[]{c,start+j,start+j+1});else tri.AddRange(new[]{c,start+j+1,start+j});}}
  MeshObject(root,Finish(v,tri,uv,"Tapered branch"),mat,"Tapered branch");
 }
 static void LeafCard(List<Vector3> v,List<int> tr,List<Vector2> uv,Vector3 p,Quaternion q,float size){
  int a=v.Count;v.Add(p+q*new Vector3(-size*.43f,0,0));v.Add(p+q*new Vector3(size*.43f,0,0));v.Add(p+q*new Vector3(size*.43f,size,0));v.Add(p+q*new Vector3(-size*.43f,size,0));
  uv.AddRange(new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)});tr.AddRange(new[]{a,a+1,a+2,a,a+2,a+3});
 }
 static void Combine(Transform root,string key){
  var fs=root.GetComponentsInChildren<MeshFilter>();foreach(var group in fs.GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial)){
   var m=new Mesh{indexFormat=IndexFormat.UInt32};m.CombineMeshes(group.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray());m.RecalculateTangents();MeshObject(root,Persist(m,key+"_"+serial+++".asset"),group.Key,group.Key.name);
  }foreach(var f in fs)UnityEngine.Object.DestroyImmediate(f.gameObject);
 }
 static GameObject Save(GameObject g,string name){var p=PrefabUtility.SaveAsPrefabAsset(g,D+"/"+name+".prefab");UnityEngine.Object.DestroyImmediate(g);return p;}
 static void Tree(){
  var g=new GameObject("Natural broadleaf crown");var t=g.transform;
  Tube(t,new Vector3(0,.27f,0),new Vector3(.08f,3.7f,.06f),.30f,.105f,bark);
  var v=new List<Vector3>();var tr=new List<int>();var uv=new List<Vector2>();
  for(int k=0;k<22;k++){
   float ang=k*2.399f;var start=new Vector3(.05f,1.9f+k%5*.24f,.03f);var tip=new Vector3(Mathf.Cos(ang)*(1.1f+k%4*.23f),3.0f+k%5*.40f,Mathf.Sin(ang)*(1.1f+k%4*.23f));
   var mid=Vector3.Lerp(start,tip,.55f)+Vector3.up*.2f;Tube(t,start,mid,.095f,.045f,bark);Tube(t,mid,tip,.045f,.008f,bark);
   for(int j=0;j<120;j++){
    float a=R(0,Mathf.PI*2),y=R(-1,1),rad=Mathf.Sqrt(1-y*y)*Mathf.Pow(R(0,1),.333f);
    var p=tip+new Vector3(Mathf.Cos(a)*rad*.85f,y*.65f,Mathf.Sin(a)*rad*.85f);
    LeafCard(v,tr,uv,p,Quaternion.Euler(R(20,150),R(0,360),R(-65,65)),R(.23f,.39f));
   }
  }MeshObject(t,Finish(v,tr,uv,"Leaf clusters"),leaf,"Leaf clusters");Combine(t,"tree");
  var renderers=g.GetComponentsInChildren<Renderer>();
  var near=renderers.First(r=>r.sharedMaterial==leaf).GetComponent<MeshFilter>();var low=UnityEngine.Object.Instantiate(near.sharedMesh);var indices=low.triangles;var reduced=new List<int>();for(int i=0;i+5<indices.Length;i+=12)for(int j=0;j<6;j++)reduced.Add(indices[i+j]);low.triangles=reduced.ToArray();
  var distant=Child(t,"Distant leaf clusters");distant.gameObject.AddComponent<MeshFilter>().sharedMesh=Persist(low,"tree_distant.asset");var dr=distant.gameObject.AddComponent<MeshRenderer>();dr.sharedMaterial=leaf;
  var far=renderers.Where(r=>r.sharedMaterial!=leaf).Concat(new Renderer[]{dr}).ToArray();
  var lod=g.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.10f,renderers),new LOD(.012f,far)});lod.RecalculateBounds();Save(g,"SampleTree");
  g=new GameObject("Natural persistent stump");Tube(g.transform,Vector3.zero,Vector3.up*.30f,.335f,.30f,bark);Tube(g.transform,Vector3.up*.301f,Vector3.up*.31f,.26f,.26f,wood);Combine(g.transform,"stump");Save(g,"Stump");
 }
 static void Boulder(Transform t,Vector3 p,Vector3 scale,int seed){
  var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);var m=UnityEngine.Object.Instantiate(sphere.GetComponent<MeshFilter>().sharedMesh);UnityEngine.Object.DestroyImmediate(sphere);
  var v=m.vertices;for(int i=0;i<v.Length;i++){float n=Mathf.PerlinNoise(v[i].x*4+seed,v[i].y*4+v[i].z*3+seed);v[i]=Vector3.Scale(v[i]*(.86f+n*.28f),scale)+p;}m.vertices=v;m.RecalculateNormals();m.RecalculateTangents();MeshObject(t,m,rock,"Weathered limestone");
 }
 static void Garden(){
  var g=new GameObject("M1.8 garden corner");var t=g.transform;
  var tree=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("NaturalAero/SampleTree"),t);tree.transform.localPosition=new Vector3(-8,0,-11);tree.transform.localRotation=Quaternion.Euler(0,45,0);
  var trunk=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("NaturalAero/Stump"),t);trunk.transform.localPosition=tree.transform.localPosition;
  var fernModel=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NaturalAero/Fern02.fbx");if(!fernModel)throw new Exception("Fern FBX missing");
  for(int i=0;i<9;i++){
   float ang=i*2.399f;var p=new Vector3(-8+Mathf.Cos(ang)*(1.1f+i%3*.45f),.015f,-11+Mathf.Sin(ang)*(1.1f+i%3*.45f));
   var f=UnityEngine.Object.Instantiate(fernModel,t);f.transform.localPosition=p;f.transform.localScale=Vector3.one*.60f;f.transform.localRotation=Quaternion.Euler(0,i*37,0);
   foreach(var r in f.GetComponentsInChildren<Renderer>())r.sharedMaterial=fern;
   var rs=f.GetComponentsInChildren<Renderer>();var lod=f.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.018f,rs)});lod.RecalculateBounds();
  }
  // Ground vegetation mesh is deliberately sparse along the deck and walking route.
  var v=new List<Vector3>();var tr=new List<int>();var uv=new List<Vector2>();
  for(int i=0;i<900;i++){
   var p=new Vector3(R(-13,-3),.01f,R(-18,-5));if(Vector2.Distance(new Vector2(p.x+7,p.z+13),Vector2.zero)<2.3f||p.x>-6.3f&&p.z< -13.5f)continue;
   for(int j=0;j<4;j++){int a=v.Count;float h=R(.14f,.33f),w=.025f;var q=Quaternion.Euler(0,R(0,360),0);v.Add(p+q*new Vector3(-w,0,0));v.Add(p+q*new Vector3(w,0,0));v.Add(p+q*new Vector3(R(-.06f,.06f),h,.07f));uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.up});tr.AddRange(new[]{a,a+1,a+2});}
  }MeshObject(t,Finish(v,tr,uv,"Meadow blades"),grass,"Meadow blades");
  for(int i=0;i<18;i++){
   float a=(220+i*3)*Mathf.Deg2Rad;Vector3 p=new Vector3(Mathf.Cos(a)*22.4f,-1.5f-i%3*.8f,Mathf.Sin(a)*19.2f);
   if(p.x> -7.2f)continue; // Keep decorative cliff geometry outside the deck footprint.
   Boulder(t,p,new Vector3(2.3f,4.5f+i%3,2.2f),i+10);
   Tube(t,p+Vector3.up*1.9f,p+new Vector3(.3f,-2,.2f),.12f,.02f,bark);
  }
  // Low boulders and flowers frame a shallow decorative basin without adding blockers.
  for(int i=0;i<16;i++){float a=i*Mathf.PI/8;Boulder(t,new Vector3(-7+Mathf.Cos(a)*1.9f,.11f,-13+Mathf.Sin(a)*1.35f),new Vector3(.65f,.23f,.50f),40+i);}
  var water=Mat("Turquoise water",new Color(.09f,.64f,.64f,.62f),.85f);
  water.SetFloat("_Surface",1);water.SetFloat("_ZWrite",0);water.SetFloat("_SrcBlend",5);water.SetFloat("_DstBlend",10);water.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");water.renderQueue=3000;water.SetOverrideTag("RenderType","Transparent");
  var pool=GameObject.CreatePrimitive(PrimitiveType.Cylinder);UnityEngine.Object.DestroyImmediate(pool.GetComponent<Collider>());pool.transform.SetParent(t,false);pool.transform.localPosition=new Vector3(-7,.05f,-13);pool.transform.localScale=new Vector3(3.5f,.035f,2.4f);pool.GetComponent<Renderer>().sharedMaterial=water;
  for(int i=0;i<28;i++){var p=new Vector3(R(-12,-4),R(.20f,.35f),R(-17,-6));v=new List<Vector3>();tr=new List<int>();uv=new List<Vector2>();for(int j=0;j<5;j++)LeafCard(v,tr,uv,p,Quaternion.Euler(70,j*72,0),.07f);MeshObject(t,Finish(v,tr,uv,"Flower"),petal,"Flower");}
  // Combine only non-LOD meshes. Tree and fern LOD renderers stay owned by their LOD groups.
  var dressing=Child(t,"Combined ground dressing");foreach(var c in t.Cast<Transform>().ToArray())if(c!=dressing&&c.GetComponent<LODGroup>()==null&&c!=tree.transform&&c!=trunk.transform)c.SetParent(dressing,true);
  Combine(dressing,"garden");Save(g,"Garden");
 }
 static void Demo(){
  var d=new SaveData{player=new Vector3(0,.3f,-10),yaw=180,pitch=8,earth=new EarthState(),air=AirTrial.NewState()};
  for(int i=0;i<EarthTrial.Locations.Length;i++)d.earth.nodes.Add(new EarthNodeState{id=EarthTrial.Id(i),kind=EarthTrial.Kind(i)});
  int n=0;void Add(string type,float x,float y,float z,float yaw=0){d.pieces.Add(new PieceRecord{id="natural-aero-sample-"+n++,type=type,position=new Vector3(x,y,z),rotation=Quaternion.Euler(0,yaw,0)});}
  Add("floor",0,.2f,-16);Add("floor",0,.2f,-20);Add("floor",-4,.2f,-20);Add("floor",4,.2f,-20);
  Add("doorway",0,.2f,-14,180);Add("window",-2,.2f,-16,90);Add("window",2,.2f,-16,270);Add("doorway",0,.2f,-18);
  Add("roof",0,3.2f,-18);Add("roof",0,3.2f,-14,180);Add("gable",-2,3.2f,-16,90);Add("gable",2,3.2f,-16,270);
  Add("railing",-6,.2f,-20,90);Add("railing",6,.2f,-20,270);
  foreach(float x in new[]{-4f,0f,4f})Add("railing",x,.2f,-22);
  Add("planter",-5.1f,.2f,-21);Add("planter",5.1f,.2f,-21);
  Add("lantern",-1.4f,.2f,-17);Add("lantern",1.4f,.2f,-17);Add("wind-anchor",6,0,-16);
  // Leave an open side route between the lawn, entrance and south deck.
  Directory.CreateDirectory("Assets/StreamingAssets");Persistence.Write("Assets/StreamingAssets/natural-sample.json",d);File.Delete("Assets/StreamingAssets/natural-sample.json.bak");
 }
}
}

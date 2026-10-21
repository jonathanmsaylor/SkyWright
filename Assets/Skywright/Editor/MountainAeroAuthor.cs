using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
namespace Skywright.Editor {
// Explicit offline authoring of a fixed island layout. Output is ordinary mesh/prefab assets.
public static class MountainAeroAuthor {
 const string D="Assets/Resources/MountainAero",T="Assets/MountainAero/Textures";
 static System.Random rng;static float R(float a,float b)=>Mathf.Lerp(a,b,(float)rng.NextDouble());
 static Material ground,stone,blades;static GameObject tree,rock,fern;static int chunk;
 static readonly Vector2[] outline={new(-650,-210),new(-800,0),new(-690,270),new(-430,500),new(-130,480),new(140,340),new(350,120),new(260,-60),new(100,-100),new(32,-38),new(-20,-42),new(-160,-170),new(-400,-280)};
 static readonly Vector2[] route={new(-15,10),new(-35,23),new(-55,35),new(-75,52),new(-91,74),new(-110,98)};
 static TAsset Persist<TAsset>(TAsset obj,string file)where TAsset:UnityEngine.Object{
  string p=D+"/"+file;var old=AssetDatabase.LoadAssetAtPath<TAsset>(p);if(old){EditorUtility.CopySerialized(obj,old);UnityEngine.Object.DestroyImmediate(obj);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(obj,p);return obj;
 }
 static Transform Child(Transform t,string n){var g=new GameObject(n);g.transform.SetParent(t,false);return g.transform;}
 static Texture2D Tex(string p,bool normal=false){var im=(TextureImporter)AssetImporter.GetAtPath(p);if(im==null)throw new Exception(p);im.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;im.sRGBTexture=!normal;im.maxTextureSize=2048;im.mipmapEnabled=true;im.anisoLevel=8;im.alphaIsTransparency=p.Contains("RGBA");im.textureCompression=TextureImporterCompression.CompressedHQ;im.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(p);}
 static Material Lit(string name,string diffuse,string normal,Color color,bool cutout=false){
  var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;m.enableInstancing=true;m.SetFloat("_Smoothness",.13f);
  if(diffuse!=null)m.SetTexture("_BaseMap",Tex(diffuse));if(normal!=null){m.SetTexture("_BumpMap",Tex(normal,true));m.SetFloat("_BumpScale",.5f);m.EnableKeyword("_NORMALMAP");}
  if(cutout){m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.42f);m.SetFloat("_Cull",0);m.EnableKeyword("_ALPHATEST_ON");m.SetOverrideTag("RenderType","TransparentCutout");m.renderQueue=2450;}
  return Persist(m,name+".mat");
 }
 static GameObject Model(string label,Func<string,Material> material){
  var root=new GameObject(label);var lods=new List<LOD>();float[] threshold=label=="Broadleaf"?new[]{.42f,.14f,.012f}:new[]{.20f,.065f,.008f};
  for(int i=0;i<3;i++){
   string p="Assets/MountainAero/Models/"+label+"_LOD"+i+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(p);importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.isReadable=false;importer.importCameras=false;importer.importLights=false;importer.SaveAndReimport();
   var child=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(p),root.transform);child.name="LOD "+i;
   var rs=child.GetComponentsInChildren<Renderer>();foreach(var r in rs){r.sharedMaterials=r.sharedMaterials.Select(m=>material(m?m.name:"")).ToArray();r.shadowCastingMode=ShadowCastingMode.On;}
   lods.Add(new LOD(threshold[i],rs));
  }
  var group=root.AddComponent<LODGroup>();group.SetLODs(lods.ToArray());group.RecalculateBounds();var result=PrefabUtility.SaveAsPrefabAsset(root,D+"/"+label+".prefab");UnityEngine.Object.DestroyImmediate(root);return result;
 }
 [MenuItem("Skywright/M1.8.1 Author Mountain Island")]
 public static void Author(){
  Directory.CreateDirectory(D);AssetDatabase.Refresh();rng=new System.Random(181031);chunk=0;
  var trunk=Lit("TreeBark",T+"/Tree_Diffuse.jpg",T+"/Tree_nor_gl.jpg",new Color(.90f,.91f,.86f));
  var branch=Lit("TreeBranches",T+"/Tree_branch_diff.jpg",T+"/Tree_branch_nor_gl.jpg",Color.white);
  var leaves=Lit("TreeLeaves",T+"/Tree_Leaves_RGBA.png",T+"/Tree_leaves_nor_gl.jpg",new Color(.88f,1.04f,.79f),true);
  stone=Lit("LimestoneScan",T+"/Coast_Diffuse.jpg",T+"/Coast_nor_gl.jpg",new Color(.93f,.95f,.91f));
  tree=Model("Broadleaf",n=>n.Contains("leaves")?leaves:n.Contains("branches")?branch:trunk);rock=Model("Limestone",n=>stone);
  var fm=Lit("Fern","Assets/NaturalAero/Textures/Fern_RGBA.png","Assets/NaturalAero/Textures/Fern_NormalGL.jpg",new Color(.90f,1.05f,.82f),true);
  var fg=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NaturalAero/Fern02.fbx"));foreach(var r in fg.GetComponentsInChildren<Renderer>())r.sharedMaterial=fm;
  fg.AddComponent<LODGroup>().SetLODs(new[]{new LOD(.025f,fg.GetComponentsInChildren<Renderer>())});fg.GetComponent<LODGroup>().RecalculateBounds();fern=PrefabUtility.SaveAsPrefabAsset(fg,D+"/Fern.prefab");UnityEngine.Object.DestroyImmediate(fg);
  ground=Persist(new Material(Shader.Find("Skywright/MountainGround")),"Ground.mat");
  ground.SetTexture("_Grass",Tex("Assets/NaturalAero/Textures/Grass004_Color.jpg"));ground.SetTexture("_GrassNormal",Tex("Assets/NaturalAero/Textures/Grass004_NormalGL.jpg",true));ground.SetTexture("_Rock",Tex("Assets/NaturalAero/Textures/Rock030_Color.jpg"));ground.SetTexture("_RockNormal",Tex("Assets/NaturalAero/Textures/Rock030_NormalGL.jpg",true));EditorUtility.SetDirty(ground);
  blades=Lit("MeadowBlades",null,null,new Color(.40f,.56f,.23f));blades.SetFloat("_Cull",0);EditorUtility.SetDirty(blades);
  var root=new GameObject("Authored Mountain Home Island");
  Terrain(root.transform);Dressing(root.transform);Neighbors(root.transform);
  PrefabUtility.SaveAsPrefabAsset(root,D+"/Island.prefab");UnityEngine.Object.DestroyImmediate(root);
  Lighting();AssetDatabase.SaveAssets();AssetDatabase.Refresh();
  Directory.CreateDirectory("Evidence/MountainAero");File.WriteAllText("Evidence/MountainAero/authoring.txt","Fixed outline bounds: x -800..350, z -280..500 (1150 x 780 m). Home r<=38 m retained at y=0. Terrain chunks: "+chunk+"\nPeak sampled: "+maxHeight+" m. Route length: "+RouteLength()+" m.\n");
  Debug.Log("MOUNTAIN_AUTHORED peak="+maxHeight+" chunks="+chunk);
 }
 public static void AuthorAndBuild(){Author();Demo();Build();}
 public static void DemoAndBuild(){Demo();Build();}
 static void Demo(){
  var d=new SaveData{player=new Vector3(14,.3f,-31),yaw=315,pitch=-5,earth=new EarthState(),air=AirTrial.NewState()};
  for(int i=0;i<EarthTrial.Locations.Length;i++)d.earth.nodes.Add(new EarthNodeState{id=EarthTrial.Id(i),kind=EarthTrial.Kind(i)});
  int n=0;void Add(string type,float x,float y,float z,float yaw=0){d.pieces.Add(new PieceRecord{id="mountain-sample-"+n++,type=type,position=new Vector3(x,y,z),rotation=Quaternion.Euler(0,yaw,0)});}
  foreach(float x in new[]{-2f,2f}){Add("floor",x,.2f,-16);Add("floor",x,.2f,-20);Add("window",x,.2f,-14,180);Add("roof",x,3.2f,-18);Add("roof",x,3.2f,-14,180);Add("railing",x,.2f,-22);}
  Add("floor",6,.2f,-16);Add("floor",6,.2f,-20);
  Add("doorway",-2,.2f,-18);Add("window",2,.2f,-18);
  Add("window",-4,.2f,-16,90);Add("doorway",4,.2f,-16,270);
  Add("gable",-4,3.2f,-16,90);Add("gable",4,3.2f,-16,270);
  Add("railing",8,.2f,-20,270);Add("railing",8,.2f,-16,270);Add("railing",6,.2f,-22);
  Add("planter",-3.3f,.2f,-21);Add("planter",7,.2f,-21);Add("planter",7,.2f,-15);
  Add("lantern",-3.2f,.2f,-15);Add("lantern",3.2f,.2f,-15);Add("bench",1,.2f,-15,180);Add("wind-anchor",18,0,-5);
  d.savedUtc=DateTime.UtcNow.ToString("O");d.harvestingTrial=true;d.elementalTrial=true;
  const string temp="Evidence/MountainAero/authored-sample.json";File.WriteAllText(temp,JsonUtility.ToJson(d,true));Persistence.Read(temp);File.Copy(temp,"Assets/StreamingAssets/natural-sample.json",true);AssetDatabase.Refresh();
 }

 public static void Build(){
  var graphics=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;
  try{var p=Resources.Load<UniversalRenderPipelineAsset>("MountainAero/MountainPipeline");GraphicsSettings.defaultRenderPipeline=p;QualitySettings.renderPipeline=p;BuildNaturalPreview.Build();}
  finally{GraphicsSettings.defaultRenderPipeline=graphics;QualitySettings.renderPipeline=quality;AssetDatabase.SaveAssets();}
 }
 static float RouteLength(){float n=0;for(int i=1;i<route.Length;i++)n+=Vector2.Distance(route[i-1],route[i]);return n;}
 static float N(float x,float z,float scale,float seed)=>Mathf.PerlinNoise(x*scale+seed,z*scale+seed*.7f);
 static float Ridge(float x,float z,float cx,float cz,float angle,float sx,float sz,float height){
  float a=angle*Mathf.Deg2Rad,dx=x-cx,dz=z-cz,u=(dx*Mathf.Cos(a)+dz*Mathf.Sin(a))/sx,v=(-dx*Mathf.Sin(a)+dz*Mathf.Cos(a))/sz;
  float r=Mathf.Sqrt(u*u+v*v);return height*Mathf.Pow(Mathf.Max(0,1-r),.90f);
 }
 static readonly Vector3[] crest={new(-80,115,230),new(-170,212,202),new(-233,166,255),new(-350,244,277),new(-433,183,215),new(-550,207,150),new(-640,116,72)};
 static float Spine(float x,float z,Vector3 a,Vector3 b,float width){
  var p=new Vector2(x,z);var start=new Vector2(a.x,a.z);var d=new Vector2(b.x-a.x,b.z-a.z);
  float t=Mathf.Clamp01(Vector2.Dot(p-start,d)/d.sqrMagnitude);float distance=Vector2.Distance(p,start+d*t);
  float height=Mathf.Lerp(a.y,b.y,t);return height*Mathf.Pow(Mathf.Max(0,1-distance/width),1.28f);
 }
 public static float SurfaceHeight(float x,float z)=>Height(x,z);
 public static float RouteDistance(float x,float z)=>PathDistance(x,z);
 static float Height(float x,float z){
  float h=RawHeight(x,z);if(x*x+z*z<6400||PathDistance(x,z)<12)return h;
  float smooth=(h*4+RawHeight(x-7,z)+RawHeight(x+7,z)+RawHeight(x,z-7)+RawHeight(x,z+7)+RawHeight(x-5,z-5)+RawHeight(x+5,z+5)+RawHeight(x-5,z+5)+RawHeight(x+5,z-5))/12;
  return Mathf.Lerp(h,smooth,Mathf.SmoothStep(0,1,(Mathf.Sqrt(x*x+z*z)-80)/30));
 }
 static float RawHeight(float x,float z){
  float distance=Mathf.Sqrt(x*x+z*z);if(distance<=38)return 0;
  float wx=x+(N(x,z,.017f,2)-.5f)*32,wz=z+(N(x,z,.017f,9)-.5f)*26;
  float ridges=0;for(int i=1;i<crest.Length;i++)ridges=Mathf.Max(ridges,Spine(wx,wz,crest[i-1],crest[i],125));
  // Unequal branching buttresses connect the crest to foothills rather than isolated cones.
  ridges=Mathf.Max(ridges,Spine(wx,wz,crest[1],new Vector3(-132,33,65),66));
  ridges=Mathf.Max(ridges,Spine(wx,wz,crest[3],new Vector3(-281,37,50),86));
  ridges=Mathf.Max(ridges,Spine(wx,wz,crest[4],new Vector3(-463,32,-65),78));
  ridges=Mathf.Max(ridges,Spine(wx,wz,crest[5],new Vector3(-646,30,-55),70));
  float rolling=4+N(x,z,.010f,6)*13+N(x,z,.034f,4)*3;
  float h=Mathf.Max(rolling,ridges),mountain=Mathf.SmoothStep(0,1,(h-22)/85);
  float strata=Mathf.Sin(h*.10f+N(x,z,.012f,3)*3)*1.1f;
  float erosion=(N(x,z,.024f,5)-.5f)*11+(N(x,z,.065f,1)-.5f)*1.6f;
  h+=(erosion+strata)*mountain;
  h=Mathf.Max(0,h*Mathf.SmoothStep(0,1,(distance-38)/45));
  float nearest=9999,pathHeight=0;float[] elevations={0,0,4,12,22,36};
  var point=new Vector2(x,z);
  for(int i=1;i<route.Length;i++){var a=route[i-1];var delta=route[i]-a;float t=Mathf.Clamp01(Vector2.Dot(point-a,delta)/delta.sqrMagnitude);float pd=Vector2.Distance(point,a+delta*t);if(pd<nearest){nearest=pd;pathHeight=Mathf.Lerp(elevations[i-1],elevations[i],t);}}
  return Mathf.Lerp(pathHeight,h,Mathf.SmoothStep(0,1,(nearest-2.2f)/30));

 }
 static Vector3 Normal(float x,float z)=>new Vector3(Height(x-.5f,z)-Height(x+.5f,z),1,Height(x,z-.5f)-Height(x,z+.5f)).normalized;
 static float PathDistance(float x,float z){
  var p=new Vector2(x,z);float min=9999;
  for(int i=1;i<route.Length;i++){var a=route[i-1];var d=route[i]-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);min=Mathf.Min(min,Vector2.Distance(p,a+d*t));}return min;
 }
 static bool Inside(float x,float z){bool v=false;for(int i=0,j=outline.Length-1;i<outline.Length;j=i++){var a=outline[i];var b=outline[j];if((a.y>z)!=(b.y>z)&&x<(b.x-a.x)*(z-a.y)/(b.y-a.y)+a.x)v=!v;}return v;}
 static float maxHeight;
 static Mesh Mesh(List<Vector3> v,List<int> tr,List<Vector3> normals,List<Color> colors,string name){
  var m=new Mesh{name=name,indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(tr,0);
  if(normals!=null)m.SetNormals(normals);else m.RecalculateNormals();
  if(colors!=null)m.SetColors(colors);m.SetUVs(0,v.Select(p=>new Vector2(p.x*.1f,p.z*.1f)).ToList());m.RecalculateTangents();m.RecalculateBounds();return m;
 }
 static GameObject MeshChild(Transform t,Mesh mesh,Material mat,string name,bool collision){
  var g=Child(t,name).gameObject;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;
  if(collision){g.AddComponent<MeshCollider>().sharedMesh=mesh;g.AddComponent<EarthSurface>();}return g;
 }
 static void Terrain(Transform root){
  maxHeight=0;const int step=3,tiles=28,size=tiles*step;var edges=new Dictionary<string,(Vector3,Vector3)>();
  // One shared mathematical surface gives identical positions and normals on chunk boundaries.
  for(int oz=-300;oz<540;oz+=size)for(int ox=-840;ox<420;ox+=size){
   var v=new List<Vector3>();var n=new List<Vector3>();var colors=new List<Color>();var tr=new List<int>();
   for(int j=0;j<=tiles;j++)for(int i=0;i<=tiles;i++){float x=ox+i*step,z=oz+j*step,h=Height(x,z);v.Add(new Vector3(x,h,z));n.Add(Normal(x,z));colors.Add(new Color(1-Mathf.SmoothStep(0,1,(PathDistance(x,z)-1)/2),0,0,1));}
   for(int j=0;j<tiles;j++)for(int i=0;i<tiles;i++){
    float x=ox+(i+.5f)*step,z=oz+(j+.5f)*step;if(!Inside(x,z))continue;
    int a=j*(tiles+1)+i,b=a+1,c=a+tiles+1,d=c+1;
    tr.AddRange(new[]{a,c,b,b,c,d});maxHeight=Mathf.Max(maxHeight,v[a].y,v[b].y,v[c].y,v[d].y);
    foreach(var pair in new[]{(a,c),(c,d),(d,b),(b,a)}){
     Vector3 p=v[pair.Item1],q=v[pair.Item2];string key=p.x+","+p.z+":"+q.x+","+q.z,reverse=q.x+","+q.z+":"+p.x+","+p.z;
     if(edges.ContainsKey(reverse))edges.Remove(reverse);else edges[key]=(p,q);
    }
   }
   if(tr.Count==0)continue;
   MeshChild(root,Persist(Mesh(v,tr,n,colors,"Meadow and ridge"),"Land_"+chunk+".asset"),ground,"Land "+chunk,true);chunk++;
  }
  // Project the sampled boundary onto the authored outline: no visible grid staircase.
  var projected=new Dictionary<Vector2,Vector3>();
  Vector3 EdgePoint(Vector3 p){
   var key=new Vector2(p.x,p.z);if(projected.TryGetValue(key,out var existing))return existing;
   float best=float.MaxValue;var nearest=key;
   for(int i=0;i<outline.Length;i++){var a=outline[i];var d=outline[(i+1)%outline.Length]-a;float t=Mathf.Clamp01(Vector2.Dot(key-a,d)/d.sqrMagnitude);var q=a+d*t;float ds=(q-key).sqrMagnitude;if(ds<best){best=ds;nearest=q;}}
   var outward=(nearest-new Vector2(-230,90)).normalized;nearest+=outward*((N(nearest.x,nearest.y,.035f,6)-.5f)*4);
   var result=new Vector3(nearest.x,Height(nearest.x,nearest.y),nearest.y);projected[key]=result;return result;
  }
  foreach(var e in edges.Values){EdgePoint(e.Item1);EdgePoint(e.Item2);}
  foreach(var filter in root.GetComponentsInChildren<MeshFilter>()){
   var m=filter.sharedMesh;var verts=m.vertices;var ns=m.normals;
   for(int i=0;i<verts.Length;i++)if(projected.TryGetValue(new Vector2(verts[i].x,verts[i].z),out var p)){verts[i]=p;ns[i]=Normal(p.x,p.z);}
   m.vertices=verts;m.normals=ns;m.RecalculateBounds();m.RecalculateTangents();EditorUtility.SetDirty(m);
   var collider=filter.GetComponent<MeshCollider>();if(collider){collider.sharedMesh=null;collider.sharedMesh=m;}
  }
  var sv=new List<Vector3>();var st=new List<int>();var sc=new List<Color>();var center=new Vector3(-230,0,90);
  Vector3 Layer(Vector3 p,int k){
   p=EdgePoint(p);if(k==0)return p;
   float noise=N(p.x,p.z,.019f,3)-.5f;
   float[] factors={1,1.002f,.996f,1.003f,.990f,.984f,.974f,.966f,.90f,.76f,.56f,.30f,0};
   float[] depths={0,-2,-5,-8,-12,-18,-26,-38,-65,-100,-145,-185,-215};
   float f=factors[k]+(k==12?0:noise*(k<5?.008f:.035f));
   var q=center+(p-center)*f;q.y=depths[k]+(k==12?0:noise*19)+p.y*(k<3?.3f:0);return q;
  }
  foreach(var e in edges.Values)for(int k=0;k<12;k++){
   int a=sv.Count;sv.AddRange(new[]{Layer(e.Item1,k),Layer(e.Item2,k),Layer(e.Item1,k+1),Layer(e.Item2,k+1)});st.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});for(int c=0;c<4;c++)sc.Add(new Color(0,1,0,1));
  }
  MeshChild(root,Persist(Mesh(sv,st,null,sc,"Layered floating limestone underside"),"Cliffs.asset"),ground,"Floating cliff underside",true);
 }
 static GameObject Place(GameObject prefab,Transform root,Vector3 p,Vector3 scale,float yaw){
  var obj=(GameObject)PrefabUtility.InstantiatePrefab(prefab);obj.transform.SetParent(root,false);obj.transform.localPosition=p;obj.transform.localRotation=Quaternion.Euler(0,yaw,0);obj.transform.localScale=scale;return obj;
 }
 static bool Clear(float x,float z)=>Mathf.Sqrt(x*x+z*z)<24||PathDistance(x,z)<4;
 static void TreeAt(Transform t,float x,float z,float size,float yaw){
  var g=Place(tree,t,new Vector3(x,Height(x,z),z),Vector3.one*size,yaw);
  var col=g.AddComponent<CapsuleCollider>();col.center=new Vector3(0,1.4f,0);col.height=2.8f;col.radius=.16f;
 }
 static void RockAt(Transform t,float x,float z,float size,float yaw){
  var g=Place(rock,t,new Vector3(x,Height(x,z)-.12f,z),Vector3.one*size,yaw);
  g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,Normal(x,z))*Quaternion.Euler(0,yaw,0);
  var filter=g.transform.Find("LOD 2").GetComponentInChildren<MeshFilter>();var collider=filter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=filter.sharedMesh;
 }
 static void Dressing(Transform root){
  var trees=Child(root,"Authored woodland");var rocks=Child(root,"Limestone outcrops");var plants=Child(root,"Ground plants");
  // Framing clusters leave all established harvesting nodes and the home footprint clear.
  foreach(var p in new[]{new Vector2(-27,-12),new Vector2(-32,-6),new Vector2(-29,5),new Vector2(26,-7),new Vector2(29,8),new Vector2(34,13),new Vector2(-40,45),new Vector2(-55,53),new Vector2(-73,37),new Vector2(-90,63)})
   TreeAt(trees,p.x,p.y,R(1.8f,2.5f),R(0,360));
  TreeAt(trees,-24,-24,2.75f,25);TreeAt(trees,35,-21,2.45f,145);
  for(int i=0;i<110;i++){
   float x=R(-590,180),z=R(25,330);if(!Inside(x,z)||Clear(x,z)||Height(x,z)>115||Normal(x,z).y<.87f)continue;
   TreeAt(trees,x,z,R(1.6f,3.1f),R(0,360));
  }
  foreach(var p in new[]{new Vector2(-23,-20),new Vector2(22,-29),new Vector2(-36,18),new Vector2(-50,40),new Vector2(-77,46),new Vector2(-96,85),new Vector2(-120,106)})RockAt(rocks,p.x,p.y,R(1.0f,2.4f),R(0,360));
  for(int i=0;i<45;i++){float x=R(-160,65),z=R(-30,140);if(!Clear(x,z)&&Normal(x,z).y>.75f)RockAt(rocks,x,z,R(.65f,2.4f),R(0,360));}
  for(int i=0;i<95;i++){
   float x=R(-115,36),z=R(-28,105);if(Clear(x,z)||Normal(x,z).y<.88f)continue;Place(fern,plants,new Vector3(x,Height(x,z)+.015f,z),Vector3.one*R(.5f,1.0f),R(0,360));
  }
  foreach(var center in new[]{new Vector2(-13,-25),new Vector2(16,-28),new Vector2(-19,-6),new Vector2(16,16),new Vector2(-32,18)})
   for(int i=0;i<9;i++){float a=i*2.399f,rad=.5f+i*.16f;float x=center.x+Mathf.Cos(a)*rad,z=center.y+Mathf.Sin(a)*rad;Place(fern,plants,new Vector3(x,Height(x,z),z),Vector3.one*R(.55f,.95f),R(0,360));}
  // Tapered grass blades in bounded patches, with clear building and walking space.
  for(int tz=-40;tz<120;tz+=20)for(int tx=-140;tx<60;tx+=20){
   var v=new List<Vector3>();var tr=new List<int>();var colors=new List<Color>();
   int density=(tx>=-60&&tz<=60)?1800:650;
   for(int j=0;j<density;j++){
    float x=R(tx,tx+20),z=R(tz,tz+20);if(!Inside(x,z)||Mathf.Abs(x)<10&&z<0&&z>-25||PathDistance(x,z)<2.8f||Normal(x,z).y<.87f)continue;
    if(N(x,z,.17f,7)<.36f)continue;
    Vector3 p=new(x,Height(x,z)+.012f,z);float h=R(.13f,.33f);var q=Quaternion.Euler(0,R(0,360),0);
    for(int k=0;k<3;k++){int a=v.Count;float w=R(.012f,.026f);var rot=q*Quaternion.Euler(0,k*60,0);v.Add(p+rot*new Vector3(-w,0,0));v.Add(p+rot*new Vector3(w,0,0));v.Add(p+rot*new Vector3(.05f,h,.03f));tr.AddRange(new[]{a,a+1,a+2});}
   }
   if(v.Count==0)continue;var g=MeshChild(plants,Persist(Mesh(v,tr,null,null,"Meadow patch"),"Grass_"+tx+"_"+tz+".asset"),blades,"Meadow patch",false);var lod=g.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.06f,g.GetComponents<Renderer>())});lod.RecalculateBounds();
  }
 }
 static void Neighbors(Transform root){
  // Static low-detail neighboring landmasses; no extra playable destinations.
  for(int island=0;island<3;island++){
   var v=new List<Vector3>();var tr=new List<int>();var c=new List<Color>();const int around=100,rings=12;
   Vector3 center=island==0?new Vector3(820,-30,100):island==1?new Vector3(-170,-80,-900):new Vector3(-1000,20,650);
   for(int r=0;r<=rings;r++)for(int i=0;i<around;i++){
    float a=i*Mathf.PI*2/around,f=r/(float)rings,rad=220*(1+.10f*Mathf.Sin(a*3+island)+.07f*Mathf.Cos(a*7));
    float x=Mathf.Cos(a)*rad*f,z=Mathf.Sin(a)*rad*.68f*f,h=(1-f)*65+N(x,z,.03f,9)*9;v.Add(center+new Vector3(x,h,z));c.Add(new Color(0,Mathf.SmoothStep(0,1,(h-30)/35),0,1));
   }
   for(int r=0;r<rings;r++)for(int i=0;i<around;i++){int a=r*around+i,b=r*around+(i+1)%around,d=(r+1)*around+i,e=(r+1)*around+(i+1)%around;tr.AddRange(new[]{a,b,d,b,e,d});}
   int baseIndex=v.Count;
   for(int r=0;r<5;r++)for(int i=0;i<around;i++){
    var edge=v[rings*around+i];float f=1-r*.22f;var q=center+(edge-center)*f;q.y=center.y-10-r*40+Mathf.Sin(i*.6f)*5;v.Add(q);c.Add(new Color(0,1,0,1));
   }
   for(int r=0;r<4;r++)for(int i=0;i<around;i++){int a=baseIndex+r*around+i,b=baseIndex+r*around+(i+1)%around,d=a+around,e=b+around;tr.AddRange(new[]{a,d,b,b,d,e});}
   // Connect top rim to the underside.
   for(int i=0;i<around;i++){int a=rings*around+i,b=rings*around+(i+1)%around,d=baseIndex+i,e=baseIndex+(i+1)%around;tr.AddRange(new[]{a,d,b,b,d,e});}
   int bottom=v.Count;v.Add(center+Vector3.down*190);c.Add(new Color(0,1,0,1));for(int i=0;i<around;i++)tr.AddRange(new[]{bottom,baseIndex+4*around+(i+1)%around,baseIndex+4*around+i});
   MeshChild(root,Persist(Mesh(v,tr,null,c,"Distant limestone island"),"Neighbor_"+island+".asset"),ground,"Distant island "+island,false);
  }
 }
 static void Lighting(){
  var sky=new Material(Shader.Find("Skywright/AeroSky"));sky.SetColor("_Zenith",new Color(.035f,.22f,.55f));sky.SetColor("_Horizon",new Color(.48f,.76f,.87f));Persist(sky,"Sky.mat");
  string source=AssetDatabase.GUIDToAssetPath("4b83569d67af61e458304325a23e5dfd");Debug.Log("PREVIEW_PIPELINE_SOURCE "+source);
  if(!AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(D+"/MountainPipeline.asset"))AssetDatabase.CopyAsset(source,D+"/MountainPipeline.asset");
  if(!AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(D+"/MountainRenderer.asset"))AssetDatabase.CopyAsset("Assets/Settings/PC_Renderer.asset",D+"/MountainRenderer.asset");
  var renderer=AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(D+"/MountainRenderer.asset");
  var rso=new SerializedObject(renderer);var features=rso.FindProperty("m_RendererFeatures");
  for(int i=0;i<features.arraySize;i++){var feature=features.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableRendererFeature;if(!feature||!feature.GetType().Name.Contains("Ambient"))continue;feature.SetActive(true);var so=new SerializedObject(feature);so.FindProperty("m_Settings.Source").intValue=0;so.FindProperty("m_Settings.Downsample").boolValue=true;so.FindProperty("m_Settings.Intensity").floatValue=.28f;so.FindProperty("m_Settings.Radius").floatValue=.35f;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(feature);}
  var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(D+"/MountainPipeline.asset");var ps=new SerializedObject(pipeline);ps.FindProperty("m_RendererDataList").GetArrayElementAtIndex(0).objectReferenceValue=renderer;ps.FindProperty("m_MainLightShadowmapResolution").intValue=4096;ps.FindProperty("m_ShadowDistance").floatValue=220;ps.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(pipeline);
  var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile=Persist(profile,"Daylight.asset");
  var tone=profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.Neutral);AssetDatabase.AddObjectToAsset(tone,profile);
  var color=profile.Add<ColorAdjustments>();color.postExposure.Override(.05f);color.contrast.Override(5);color.saturation.Override(3);AssetDatabase.AddObjectToAsset(color,profile);
  var bloom=profile.Add<Bloom>();bloom.intensity.Override(.08f);bloom.threshold.Override(1.25f);AssetDatabase.AddObjectToAsset(bloom,profile);EditorUtility.SetDirty(profile);
 }
}
}
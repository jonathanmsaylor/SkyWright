using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;using UnityEngine.Rendering.Universal;using UnityEditor;
namespace Skywright.Editor {
// Explicit authoring command, never runtime generation. Existing resource GUIDs are retained.
public static class CompactIslandAuthor {
 const string D="Assets/Resources/CompactIsland",Island="Assets/Resources/MountainAero/Island.prefab";
 static System.Random random;static float R(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
 static Material ground;static int meshIndex;
 static T Save<T>(T value,string name)where T:UnityEngine.Object{string path=D+"/"+name;var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(value,path);return value;}
 static Transform Child(Transform p,string name){var g=new GameObject(name);g.transform.SetParent(p,false);return g.transform;}
 static float N(float x,float z,float scale,int seed)=>Mathf.PerlinNoise(x*scale+seed,z*scale+seed*.73f);
 public static float Height(float x,float z){
  float radius=new Vector2(x,z).magnitude;if(radius<=38)return 0;
  float hill=5*Mathf.Exp(-Mathf.Pow((x+39)/18,2)-Mathf.Pow((z-21)/15,2));
  return (1.2f+hill+N(x,z,.055f,3)*1.3f)*Mathf.SmoothStep(0,1,(radius-38)/10);

 }
 public static bool Inside(float x,float z,float margin=0){float u=x/65f,v=z/52f;float angle=Mathf.Atan2(v,u);float outline=1+.035f*Mathf.Sin(angle*3+1)+.022f*Mathf.Sin(angle*7-.4f);return Mathf.Sqrt(u*u+v*v)<outline-margin/52f;}
 static Vector3 Normal(float x,float z)=>new Vector3(Height(x-.3f,z)-Height(x+.3f,z),.6f,Height(x,z-.3f)-Height(x,z+.3f)).normalized;
 static float Path(float x,float z){var a=new Vector2(14,-31);var b=new Vector2(30,32);var p=new Vector2(x,z);float t=Mathf.Clamp01(Vector2.Dot(p-a,b-a)/(b-a).sqrMagnitude);float d=Vector2.Distance(p,Vector2.Lerp(a,b,t));return Mathf.Min(d,Mathf.Abs(z-26+Mathf.Sin(x*.035f)*6));}
 static bool Clear(float x,float z)=>x> -12&&x<14&&z> -27&&z< -10||Path(x,z)<2.4f||EarthTrial.Locations.Any(p=>Vector2.Distance(new Vector2(x,z),new Vector2(p.x,p.z))<1.3f);
 static GameObject MeshObject(Transform root,Mesh m,Material material,string name,bool collision){m.RecalculateBounds();var g=Child(root,name).gameObject;g.AddComponent<MeshFilter>().sharedMesh=Save(m,"Mesh_"+meshIndex+++".asset");g.AddComponent<MeshRenderer>().sharedMaterial=material;if(collision){g.AddComponent<MeshCollider>().sharedMesh=g.GetComponent<MeshFilter>().sharedMesh;g.AddComponent<EarthSurface>();}return g;}
 static Vector2 Map(float u,float v,int seed){
  float dx=u*Mathf.Sqrt(1-v*v*.5f),dz=v*Mathf.Sqrt(1-u*u*.5f);float angle=Mathf.Atan2(dz,dx),rad=Mathf.Sqrt(dx*dx+dz*dz);
  float outline=1+rad*rad*(.035f*Mathf.Sin(angle*3+seed)+.022f*Mathf.Sin(angle*7-seed*.4f));
  return new Vector2(dx*outline,dz*outline);
 }
 static float NeighborHeight(float u,float v,float x,float z,float peak,int seed){
  if(seed==10)return 3;
  float raw=peak*Mathf.Pow(Mathf.Max(0,1-Mathf.Abs(u+.1f)*.9f-Mathf.Abs(v-.08f)*1.5f),.65f);
  return 3+raw+(N(x,z,.065f,seed)-.5f)*4+(N(x,z,.18f,seed+1)-.5f)*1.2f;
 }
 static void Land(Transform root,bool home,int seed,Vector3 center,float width,float depth,float peak){
  int n=home?160:56;var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();var rim=new List<int>();
  for(int layer=0;layer<2;layer++)for(int j=0;j<=n;j++)for(int i=0;i<=n;i++){
   float u=i*2f/n-1,v=j*2f/n-1;var p=Map(u,v,seed);float x=p.x*width,z=p.y*depth;float r=Mathf.Sqrt(p.x*p.x+p.y*p.y);float wx=x+center.x,wz=z+center.z;
   float y=home?Height(wx,wz):NeighborHeight(p.x,p.y,x,z,peak,seed);
   if(layer==1){
    // A continuous broad, asymmetric rock keel: bottom grid avoids an apex/fan.
    float shelf=18+28*Mathf.Exp(-Mathf.Pow((p.x+.22f)/.52f,2)-Mathf.Pow(p.y/.8f,2));
    shelf+=17*Mathf.Exp(-Mathf.Pow((p.x-.49f)/.28f,2)-Mathf.Pow((p.y+.23f)/.40f,2));
    y=-shelf+(N(x,z,.055f,seed)-.5f)*8+(N(x,z,.16f,seed+2)-.5f)*2.2f;
    float strata=y/5.5f;float fract=strata-Mathf.Floor(strata);y=Mathf.Lerp(y,(Mathf.Floor(strata)+Mathf.SmoothStep(0,1,(fract-.1f)/.8f))*5.5f,.65f);
   }
   vertices.Add(new Vector3(wx,y+center.y,wz));colors.Add(layer==0?new Color(home?1-Mathf.SmoothStep(0,1,(Path(wx,wz)-.8f)/2):0,Mathf.SmoothStep(0,1,(y-13)/13),0,1):new Color(0,1,0,1));
  }
  int count=(n+1)*(n+1);
  for(int j=0;j<n;j++)for(int i=0;i<n;i++){int a=j*(n+1)+i,b=a+1,c=a+n+1,d=c+1;triangles.AddRange(new[]{a,c,b,b,c,d});triangles.AddRange(new[]{a+count,b+count,c+count,b+count,d+count,c+count});}
  for(int i=0;i<=n;i++)rim.Add(i);for(int j=1;j<=n;j++)rim.Add(j*(n+1)+n);for(int i=n-1;i>=0;i--)rim.Add(n*(n+1)+i);for(int j=n-1;j>0;j--)rim.Add(j*(n+1));
  int edgeStart=vertices.Count;const int rings=7;
  for(int k=0;k<=rings;k++)foreach(int index in rim){
   var top=vertices[index];var bottom=vertices[index+count];float t=k/(float)rings;var p=Vector3.Lerp(top,bottom,t);
   var radial=new Vector3(top.x-center.x,0,top.z-center.z).normalized;
   float shelf=Mathf.Sin(t*Mathf.PI)*(Mathf.Sin(t*14+N(top.x,top.z,.08f,seed)*3)*1.8f+(N(top.x,top.z,.045f,seed)-.6f)*10);
   p.y+=Mathf.Sin(t*Mathf.PI)*(N(top.x,top.z,.09f,seed)-.5f)*5;
   p+=radial*shelf;vertices.Add(p);colors.Add(new Color(0,1,0,1));
  }
  for(int k=0;k<rings;k++)for(int j=0;j<rim.Count;j++){int a=edgeStart+k*rim.Count+j,b=edgeStart+k*rim.Count+(j+1)%rim.Count,c=a+rim.Count,d=b+rim.Count;triangles.AddRange(new[]{a,b,c,b,d,c});}
  var mesh=new Mesh{name=home?"Compact limestone island":"Neighbor "+seed,indexFormat=IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.RecalculateNormals();mesh.SetUVs(0,vertices.Select(p=>new Vector2(p.x*.1f,p.z*.1f)).ToList());mesh.RecalculateTangents();MeshObject(root,mesh,ground,home?"Home landform":"Scenic landform",home||seed==10);
 }
 static GameObject Place(GameObject prefab,Transform parent,Vector3 position,float scale,float yaw){var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=Vector3.one*scale;g.transform.localRotation=Quaternion.Euler(0,yaw,0);return g;}
 static void StructuralRock(Transform parent,float x,float z,float scale,float yaw){
  var g=Place(Resources.Load<GameObject>("MountainAero/Limestone"),parent,new Vector3(x,Height(x,z)-scale*.65f,z),scale,yaw);
  // Original scan is deeply embedded; only weathered ledges emerge, no floating fragments.
  var f=g.transform.Find("LOD 2").GetComponentInChildren<MeshFilter>();f.gameObject.AddComponent<MeshCollider>().sharedMesh=f.sharedMesh;f.gameObject.AddComponent<EarthSurface>();
 }
 static void SeatRock(GameObject rock){
  // Fit the scan's lower surface to the local terrain, accounting for yaw, scale and slope.
  var points=new List<Vector3>();foreach(var mf in rock.GetComponentsInChildren<MeshFilter>())if(mf.sharedMesh)foreach(var v in mf.sharedMesh.vertices)points.Add(mf.transform.TransformPoint(v));
  if(points.Count==0)return;float min=points.Min(p=>p.y),max=points.Max(p=>p.y);
  var basePoints=points.Where(p=>p.y<min+(max-min)*.18f).ToArray();
  float shift=basePoints.Min(p=>Height(p.x,p.z)-p.y)-.12f;
  rock.transform.position+=Vector3.up*shift;
 }
 public static void Author(){
  Directory.CreateDirectory(D);Directory.CreateDirectory("Evidence/CompactAether");AssetDatabase.Refresh();random=new System.Random(190019);meshIndex=0;ground=Resources.Load<Material>("MountainAero/Ground");
  var old=PrefabUtility.LoadPrefabContents(Island);var root=new GameObject("Compact home island");
  try{
   // Keep every original resource component and explicit identity, move only incompatible remote roots.
   var resources=old.GetComponentsInChildren<SceneryResource>(true).OrderBy(r=>r.stableId).ToArray();var group=Child(root.transform,"Persistent scenery resources");var relocations=new List<string>();var occupied=new List<Vector2>();
   foreach(var resource in resources){
    var original=resource.transform.position;float x=original.x,z=original.z;bool relocated=!Inside(x,z,6)||new Vector2(x,z).magnitude>22;
    if(relocated){int tries=0;do{x=R(-59,59);z=R(-46,46);tries++;}while((!Inside(x,z,5)||Clear(x,z)||Normal(x,z).y<.84f||(resource.kind=="tree"&&Height(x,z)>25)||new Vector2(x,z).magnitude<22||occupied.Any(q=>Vector2.Distance(q,new Vector2(x,z))<3.8f))&&tries<3000);}
    if(!Inside(x,z,3))throw new Exception("Could not fit resource "+resource.stableId);
    occupied.Add(new Vector2(x,z));resource.transform.SetParent(group,true);resource.transform.position=new Vector3(x,Height(x,z)-.06f,z);
    if(resource.kind=="outcrop")SeatRock(resource.gameObject);
    relocations.Add(resource.stableId+" | "+original.ToString("F3")+" -> "+resource.transform.position.ToString("F3"));
   }
   Land(root.transform,true,1,Vector3.zero,65,52,0);
   // Clearings stay open; vegetation groups follow woodland roots and traversable slopes.
   var cover=Child(root.transform,"Island understory");var plants=new[]{"ForestBush01","ForestBush02","GrassPlant03","GrassPlant04","FlowerGrass01","FlowerGrass03"};
   for(int i=0;i<450;i++){float x=R(-64,64),z=R(-51,51);if(!Inside(x,z,7)||Clear(x,z)||Normal(x,z).y<.84f||new Vector2(x,z).magnitude<17)continue;Place(Resources.Load<GameObject>("FinishedMeadow/"+plants[i%plants.Length]),cover,new Vector3(x,Height(x,z)-.02f,z),i%6<2?R(.28f,.55f):R(.3f,.65f),R(0,360));}
   Grass(cover);
   var neighborPositions=new[]{new Vector3(160,14,0),new Vector3(-220,26,-15),new Vector3(190,30,245),new Vector3(-280,5,240),new Vector3(10,8,-265)};
   float[] widths={26,43,83,57,70},depths={22,32,52,40,37},peaks={12,27,7,25,10};
   for(int i=0;i<5;i++){var g=Child(root.transform,"Neighbor "+i);Land(g,false,10+i,neighborPositions[i],widths[i],depths[i],peaks[i]);
    for(int j=0;j<(i==1?3:18);j++){float u=R(-.75f,.75f),v=R(-.65f,.65f);if(u*u+v*v>.6f)continue;float x=u*widths[i],z=v*depths[i];if(i==0&&z<6&&z>-6)continue;float y=NeighborHeight(u,v,x,z,peaks[i],10+i);
     Place(Resources.Load<GameObject>("FinishedMeadow/"+(j%2==0?"MapleSpringTree03":"ForestTree03")),g,neighborPositions[i]+new Vector3(x,y-.1f,z),R(.35f,.6f),R(0,360));}
   }
   PrefabUtility.SaveAsPrefabAsset(root,Island);
   File.WriteAllLines("Evidence/CompactAether/resource-relocations.txt",relocations);
   File.WriteAllText("Evidence/CompactAether/world-dimensions.txt","Tutorial island approximately 135 x 108 m: quarter of M1.9 area. Home r<=38 unchanged at y=0. Peak sampled: "+Enumerable.Range(0,130).SelectMany(x=>Enumerable.Range(0,104).Select(z=>Height(x-65,z-52))).Max()+" m. Resource IDs retained: "+resources.Length);
  }finally{UnityEngine.Object.DestroyImmediate(root);PrefabUtility.UnloadPrefabContents(old);}
  FinishedMeadowAuthor.HarvestTree();
  var pipeline=Resources.Load<UniversalRenderPipelineAsset>("MountainAero/MountainPipeline");pipeline.shadowDistance=110;pipeline.shadowCascadeCount=4;EditorUtility.SetDirty(pipeline);
  var profile=Resources.Load<VolumeProfile>("MountainAero/Daylight");if(profile.TryGet<ColorAdjustments>(out var color)){color.postExposure.Override(.1f);color.contrast.Override(4);color.saturation.Override(1);EditorUtility.SetDirty(color);}
  AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("COMPACT_ISLAND_AUTHORED");
 }
 static void Grass(Transform parent){
  var mat=Resources.Load<Material>("FinishedMeadow/MeadowGrass");
  for(int tz=-55;tz<55;tz+=10)for(int tx=-65;tx<65;tx+=10){
   if(!Inside(tx+5,tz+5,5))continue;var v=new List<Vector3>();var ns=new List<Vector3>();var uv=new List<Vector2>();var cs=new List<Color>();var tr=new List<int>();
   for(int i=0;i<170;i++){float x=R(tx,tx+10),z=R(tz,tz+10);if(!Inside(x,z,3)||Clear(x,z)||Normal(x,z).y<.88f||N(x,z,.15f,5)<.38f)continue;float h=R(.14f,.28f),w=R(.35f,.6f),angle=R(0,360);var p=new Vector3(x,Height(x,z)-.015f,z);
    for(int c=0;c<3;c++){int a=v.Count;var q=Quaternion.Euler(0,angle+c*62,0);for(int row=0;row<3;row++)for(int side=0;side<2;side++){float t=row*.5f;v.Add(p+q*new Vector3((side-.5f)*w,t*h,t*t*.05f));ns.Add((q*new Vector3(0,.95f,.31f)).normalized);uv.Add(new Vector2(side,t));cs.Add(Color.Lerp(new Color(.82f,.94f,.76f),Color.white,R(0,1)));}
     tr.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3,a+2,a+4,a+3,a+3,a+4,a+5});}
   }
   if(v.Count==0)continue;var m=new Mesh{name="Compact meadow tile",indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetNormals(ns);m.SetUVs(0,uv);m.SetColors(cs);m.SetTriangles(tr,0);var obj=MeshObject(parent,m,mat,"Grass "+tx+" "+tz,false);obj.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
  }
 }
 public static void AuthorAndBuild(){Author();AetherAuthor.Author();AetherSaveChecks.VerifyAndBuild();}
}
}

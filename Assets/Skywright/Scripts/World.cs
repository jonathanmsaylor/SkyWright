using UnityEngine;
using UnityEngine.Rendering;
namespace Skywright {
public static class World {
 public static Vector3 Spawn=new Vector3(0,.3f,-10);
 public static void Create(Transform parent) {
  Kit.Init();
  if(NaturalPreview.Active){MountainEnvironment.Create(parent);return;}
  var grass=Kit.Mat("Sunlit moss",new Color(.40f,.48f,.29f));var rock=Kit.Mat("Warm limestone",new Color(.40f,.43f,.40f));var edge=Kit.Mat("Cliff strata",new Color(.31f,.34f,.33f));
  Texture2D grain=new Texture2D(64,128);var rng=new System.Random(82);
  for(int x=0;x<64;x++) {float stripe=(float)rng.NextDouble()*.12f;for(int y=0;y<128;y++){float v=.83f+stripe+Mathf.Sin(y*.12f+x*.4f)*.025f;grain.SetPixel(x,y,new Color(v,v,v));}}grain.Apply();grain.wrapMode=TextureWrapMode.Repeat;Kit.wood.mainTexture=grain;
  Island(parent,Vector3.zero,23,18,grass,rock,edge,true,31);
  // Authored dressing stays outside the central construction clearing.
  Tree(parent,new Vector3(-16,0,6),1.25f);Tree(parent,new Vector3(15,0,5),1.45f);Tree(parent,new Vector3(-10,0,14),1f);
  Tree(parent,new Vector3(12,0,13),.9f);Tree(parent,new Vector3(-17,0,-3),.7f);
  for(int i=0;i<24;i++){
   float a=i*2.399f;Vector3 p=new Vector3(Mathf.Sin(a)*18,0,Mathf.Cos(a)*15);if(p.z< -10&&Mathf.Abs(p.x)<9)continue;
   var b=GameObject.CreatePrimitive(PrimitiveType.Sphere);b.name="Meadow stone";b.transform.SetParent(parent);b.transform.position=p+Vector3.up*.22f;b.transform.localScale=new Vector3(.7f+(i%3)*.2f,.55f,.65f);b.GetComponent<Renderer>().sharedMaterial=rock;
  }
  // Distant scenery has no gameplay or collision.
  Island(parent,new Vector3(-105,-14,140),29,45,grass,rock,edge,false,48);
  Island(parent,new Vector3(100,12,190),20,58,grass,rock,edge,false,80);
  Island(parent,new Vector3(180,-42,-70),48,70,grass,rock,edge,false,12);
  var cloud=Kit.Mat("Cloud pearl",new Color(.87f,.91f,.89f));
  for(int i=0;i<18;i++){
   float a=i*2.399f;Vector3 center=new Vector3(Mathf.Sin(a)*(145+i*6),-48-(i%4)*10,Mathf.Cos(a)*(145+i*6));
   for(int j=0;j<6;j++){var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Cloud bank";g.transform.SetParent(parent);
    g.transform.position=center+new Vector3(Mathf.Sin(j*2.1f)*18,j%3*4,Mathf.Cos(j*2.1f)*12);
    g.transform.localScale=new Vector3(26+j%3*8,18+j%2*8,24+j%3*5);g.GetComponent<Renderer>().sharedMaterial=cloud;Object.Destroy(g.GetComponent<Collider>());
   }
  }
  Island(parent,new Vector3(-32,-4,-125),24,46,grass,rock,edge,false,94);
  Tree(parent,new Vector3(-40,-4,-124),1.4f);Tree(parent,new Vector3(-23,-4,-130),1.1f);
  var patch=Kit.Mat("Meadow blades",new Color(.46f,.54f,.30f));
  for(int i=0;i<105;i++){
   float a=i*2.399f;Vector3 p=new Vector3(Mathf.Sin(a)*(15+i%5),0,Mathf.Cos(a)*(12+i%5));
   if(p.z< -9&&Mathf.Abs(p.x)<11)continue;
   for(int j=0;j<3;j++)Kit.Box(parent,"Meadow tuft",p+new Vector3(j*.11f,.14f,0),new Vector3(.035f,.28f,.12f),patch,Quaternion.Euler(0,i*17,(-1+j)*20),false);
  }
  var sun=new GameObject("Afternoon sun");sun.transform.SetParent(parent);sun.transform.rotation=Quaternion.Euler(38,-35,0);
  var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(1,.87f,.68f);light.intensity=2.1f;light.shadows=LightShadows.Soft;light.shadowBias=.04f;
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.61f,.75f,.86f);RenderSettings.ambientEquatorColor=new Color(.5f,.56f,.55f);RenderSettings.ambientGroundColor=new Color(.27f,.28f,.23f);
  RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0023f;RenderSettings.fogColor=new Color(.64f,.78f,.85f);
  var sky=new Material(Resources.Load<Material>("SkywrightSky"));sky.SetColor("_SkyTint",new Color(.50f,.64f,.76f));sky.SetColor("_GroundColor",new Color(.59f,.71f,.75f));sky.SetFloat("_AtmosphereThickness",.8f);sky.SetFloat("_Exposure",1.2f);RenderSettings.skybox=sky;RenderSettings.sun=light;
 }
 static void Tree(Transform parent,Vector3 p,float s) {
  var t=new GameObject("Wind-shaped pine");t.transform.SetParent(parent);t.transform.position=p;t.transform.localScale=Vector3.one*s;
  Kit.Box(t.transform,"Trunk",Vector3.up*2,new Vector3(.45f,4,.45f),Kit.trim);
  for(int i=0;i<4;i++){
   var g=new GameObject("Faceted pine crown");g.transform.SetParent(t.transform,false);g.transform.localPosition=new Vector3(.12f*i,2.5f+i*.85f,0);
   var vs=new Vector3[24];var ts=new int[24];float r=1.8f-i*.32f;
   for(int j=0;j<8;j++){float a=j*Mathf.PI/4,b=(j+1)*Mathf.PI/4;vs[j*3]=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);vs[j*3+1]=new Vector3(.12f,2.0f,0);vs[j*3+2]=new Vector3(Mathf.Cos(b)*r,0,Mathf.Sin(b)*r);ts[j*3]=j*3;ts[j*3+1]=j*3+1;ts[j*3+2]=j*3+2;}
   var mesh=new Mesh();mesh.vertices=vs;mesh.triangles=ts;mesh.RecalculateNormals();g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=Kit.leaf;
  }
 }
 static void Island(Transform parent,Vector3 pos,float radius,float depth,Material grass,Material rock,Material edge,bool collide,int seed) {
  int n=32;var rng=new System.Random(seed);var verts=new Vector3[1+n*3];verts[0]=Vector3.zero;
  for(int i=0;i<n;i++){
   float a=i*Mathf.PI*2/n;float r=radius*(.94f+(float)rng.NextDouble()*.1f);
   verts[1+i]=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r*.86f);
   verts[1+n+i]=new Vector3(Mathf.Cos(a)*r*.83f,-depth*.35f-(float)rng.NextDouble()*2,Mathf.Sin(a)*r*.71f);
   verts[1+n*2+i]=new Vector3(Mathf.Cos(a)*r*.22f,-depth+(float)rng.NextDouble()*3,Mathf.Sin(a)*r*.18f);
  }
  var top=new int[n*3];var side=new int[n*12];for(int i=0;i<n;i++){int j=(i+1)%n;top[i*3]=0;top[i*3+1]=1+j;top[i*3+2]=1+i;
   for(int row=0;row<2;row++){int a=1+row*n+i,b=1+row*n+j,c=a+n,d=b+n,k=i*12+row*6;side[k]=a;side[k+1]=b;side[k+2]=c;side[k+3]=b;side[k+4]=d;side[k+5]=c;}}
  var m=new Mesh();m.name="Hand-shaped island";m.vertices=verts;m.subMeshCount=2;m.SetTriangles(top,0);m.SetTriangles(side,1);m.RecalculateNormals();m.RecalculateBounds();
  var g=new GameObject(collide?"Home island":"Unreachable horizon island");g.transform.SetParent(parent);g.transform.position=pos;g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterials=new[]{grass,rock};
  if(collide){g.AddComponent<MeshCollider>().sharedMesh=m;g.AddComponent<EarthSurface>();}
 }
}
}

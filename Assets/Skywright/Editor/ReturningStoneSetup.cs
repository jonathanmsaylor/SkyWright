using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Skywright.Editor {
// Explicit, deterministic authoring of an original continuous mesh. Ordinary builds do not regenerate art.
public static class ReturningStoneSetup {
 const string Dir="Assets/Resources/ReturningEarthstone";
 static float Noise(Vector3 p)=> (Mathf.PerlinNoise(p.x+13.7f,p.y+5.3f)+Mathf.PerlinNoise(p.y+21.1f,p.z+11.9f)+Mathf.PerlinNoise(p.z+2.7f,p.x+31.2f))/3;
 static Vector3 Sphere(float u,float v){float a=u*Mathf.PI*2,b=v*Mathf.PI;return new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a));}
 static Vector3 Shape(Vector3 d){
  // Broad eroded facets, asymmetric mass, fine geometry kept subordinate to the silhouette.
  float radius=1+.40f*(Noise(d*2.2f)-.5f)+.055f*(Noise(d*17)-.5f);
  var p=Vector3.Scale(d*radius,new Vector3(.31f,.255f,.28f));
  var planes=new[]{new Vector3(.4f,1,.2f).normalized,new Vector3(-.7f,-1,.3f).normalized,new Vector3(1,.2f,-.5f).normalized,new Vector3(-.3f,.2f,1).normalized,new Vector3(-1,.3f,-.4f).normalized,new Vector3(.3f,.1f,-1).normalized};
  float[] caps={.182f,.207f,.245f,.226f,.238f,.247f};
  for(int k=0;k<planes.Length;k++){var n=planes[k];float excess=Vector3.Dot(p,n)-caps[k];if(excess>0)p-=n*(excess*.97f);}
  p.x+=.016f*d.y*d.z;p.z+=.012f*d.y;return p;
 }
 static T Save<T>(T value,string file) where T:UnityEngine.Object {
  string p=Dir+"/"+file;var old=AssetDatabase.LoadAssetAtPath<T>(p);if(old){EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);return old;}AssetDatabase.CreateAsset(value,p);return value;
 }
 static string Audit(GameObject prefab){
  string report="Inspected previous projectile prefab before replacement:\n";
  foreach(var f in prefab.GetComponentsInChildren<MeshFilter>()){
   var m=f.sharedMesh;var v=m.vertices;var t=m.triangles;int degenerate=0,inward=0;
   for(int i=0;i<t.Length;i+=3){var a=v[t[i]];var b=v[t[i+1]];var c=v[t[i+2]];var n=Vector3.Cross(b-a,c-a);if(n.sqrMagnitude<1e-12f)degenerate++;else if(Vector3.Dot(n,(a+b+c)/3)<0)inward++;}
   report+=f.name+": vertices="+v.Length+" triangles="+t.Length/3+" UV="+m.uv.Length+" normals="+m.normals.Length+" degenerate="+degenerate+" inward relative to root="+inward+" bounds="+m.bounds+"\n";
  }return report;
 }
 public static void Build(){Author();BuildElemental.Build();}
 [MenuItem("Skywright/Author Returning Earthstone (explicit)")]
 public static void Author(){
  Directory.CreateDirectory(Dir);Directory.CreateDirectory("Evidence/ReturningEarthstone");AssetDatabase.Refresh();
  var config=Resources.Load<EarthConfig>("Earthcasting/Tuning");string audit=Audit(config.rock);
  const int nx=64,ny=32;var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
  for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++){float u=x/(float)nx,v=y/(float)ny;vertices.Add(Shape(Sphere(u,v)));uv.Add(new Vector2(u,v));}
  for(int y=0;y<ny;y++)for(int x=0;x<nx;x++){int k=y*(nx+1)+x;if(y>0)triangles.AddRange(new[]{k,k+1,k+nx+1});if(y<ny-1)triangles.AddRange(new[]{k+1,k+nx+2,k+nx+1});}
  float rawRadius=0;foreach(var v in vertices)rawRadius=Mathf.Max(rawRadius,v.magnitude);for(int i=0;i<vertices.Count;i++)vertices[i]*=.30f/rawRadius;
  var mesh=new Mesh{name="Weathered limestone — continuous surface"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();
  var normals=mesh.normals;
  for(int y=1;y<ny;y++){int a=y*(nx+1),b=a+nx;var n=(normals[a]+normals[b]).normalized;normals[a]=normals[b]=n;}
  for(int y=0;y<=ny;y+=ny){var n=Vector3.zero;for(int x=0;x<=nx;x++)n+=normals[y*(nx+1)+x];n.Normalize();for(int x=0;x<=nx;x++)normals[y*(nx+1)+x]=n;}
  mesh.normals=normals;mesh.RecalculateTangents();mesh.RecalculateBounds();
  double volume=0;float radius=0;for(int i=0;i<vertices.Count;i++)radius=Mathf.Max(radius,vertices[i].magnitude);
  for(int i=0;i<triangles.Count;i+=3){var a=vertices[triangles[i]];var b=vertices[triangles[i+1]];var c=vertices[triangles[i+2]];var n=Vector3.Cross(b-a,c-a);if(n.sqrMagnitude<1e-14f||Vector3.Dot(n,(a+b+c)/3)<=0)throw new Exception("Invalid stone face "+i);volume+=Vector3.Dot(a,Vector3.Cross(b,c))/6;}
  if(radius>.31f||volume<=0)throw new Exception("Rock exceeds deliberate collision sphere");mesh=Save(mesh,"Limestone.mesh.asset");
  const int width=1024,height=512;var color=new Texture2D(width,height,TextureFormat.RGBA32,true);var normal=new Texture2D(width,height,TextureFormat.RGBA32,true,true);
  Func<float,float,float> relief=(u,v)=>{var d=Sphere(u,v);return Noise(d*30)*.38f+Noise(d*170)*.12f+Noise(d*8)*.5f;};
  for(int y=0;y<height;y++)for(int x=0;x<width;x++){
   float u=x/(float)(width-1),v=y/(float)(height-1);var d=Sphere(u,v);float broad=Noise(d*4),grain=Noise(d*95),vein=Mathf.Pow(Mathf.Clamp01(1-Mathf.Abs(Noise(d*11)-.51f)*30),5);
   Color c=Color.Lerp(new Color(.47f,.49f,.44f),new Color(.76f,.76f,.65f),Mathf.Clamp01(.45f+(broad-.5f)*2.4f+(grain-.5f)*.14f));c=Color.Lerp(c,new Color(.80f,.79f,.70f),vein*.14f);
   c.a=.09f+(1-grain)*.08f;color.SetPixel(x,y,c);
   float dx=(relief(u+1f/width,v)-relief(u-1f/width,v))*1.3f,dy=(relief(u,v+1f/height)-relief(u,v-1f/height))*1.3f;
   var nn=new Vector3(-dx,-dy,1).normalized;normal.SetPixel(x,y,new Color(nn.x*.5f+.5f,nn.y*.5f+.5f,nn.z*.5f+.5f,1));
  }
  color.Apply();normal.Apply();File.WriteAllBytes(Dir+"/Limestone-color.png",color.EncodeToPNG());File.WriteAllBytes(Dir+"/Limestone-normal.png",normal.EncodeToPNG());UnityEngine.Object.DestroyImmediate(color);UnityEngine.Object.DestroyImmediate(normal);AssetDatabase.Refresh();
  foreach(string name in new[]{"Limestone-color.png","Limestone-normal.png"}){var importer=(TextureImporter)AssetImporter.GetAtPath(Dir+"/"+name);importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.textureType=name.Contains("normal")?TextureImporterType.NormalMap:TextureImporterType.Default;importer.SaveAndReimport();}
  var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Weathered pale limestone"};mat.color=Color.white;mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Dir+"/Limestone-color.png");mat.SetFloat("_Smoothness",.13f);mat.SetFloat("_Metallic",0);mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Dir+"/Limestone-normal.png"));mat.SetFloat("_BumpScale",.4f);mat.EnableKeyword("_NORMALMAP");mat=Save(mat,"Limestone.mat");
  var root=new GameObject("Returning Earthstone");root.layer=2;var visual=new GameObject("Visuals — replaceable limestone");visual.layer=2;visual.transform.SetParent(root.transform,false);visual.AddComponent<MeshFilter>().sharedMesh=mesh;visual.AddComponent<MeshRenderer>().sharedMaterial=mat;
  // Collision is a swept sphere owned by EarthTrial, never the detailed rendered mesh.
  config.rock=PrefabUtility.SaveAsPrefabAsset(root,Dir+"/Earthstone.prefab");UnityEngine.Object.DestroyImmediate(root);
  config.stoneRadius=.31f;config.waitOffset=.55f;config.strikeAnticipation=.16f;config.strikeSpeed=12;config.strikeAcceleration=90;config.reboundSpeed=4;config.maxRicochets=3;config.ricochetRetention=.88f;config.blockedReturnTime=.8f;config.dismissTime=.3f;
  EditorUtility.SetDirty(config);AssetDatabase.SaveAssets();
  audit+="Replacement: "+vertices.Count+" vertices, "+triangles.Count/3+" nondegenerate outward triangles; positive signed volume="+volume+"; radial max="+radius+"; bounds="+mesh.bounds+". Seam/pole normals averaged, continuous positional noise. Original mesh/color/normal artwork; no third-party assets. Sphere sweep radius=.31m, identity scale at rest and in flight.\n";
  File.WriteAllText("Evidence/ReturningEarthstone/mesh-audit.txt",audit);Debug.Log("RETURNING_STONE_AUTHORED "+audit);
 }
}
}
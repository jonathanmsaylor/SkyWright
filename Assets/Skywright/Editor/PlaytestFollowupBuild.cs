using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Skywright.Editor {
public static class PlaytestFollowupBuild {
 public static void ProfileBaseline(){PlayerSettings.enableFrameTimingStats=true;TutorialWindBuild.VerifyAndBuild();}
 public static void AuthorAndBuild(){BakeField();RockFlashBuild.SeatRocks();VerifyAndBuild();}
 public static void VerifyAndBuild(){PlayerSettings.enableFrameTimingStats=true;var ribbon=Resources.Load<Material>("AirMagic/WindRibbon");ribbon.shader=Shader.Find("Skywright/SoftWindRibbon");ribbon.SetFloat("_NearStart",.4f);ribbon.SetFloat("_NearEnd",1.6f);ribbon.SetFloat("_MaxPixelWidth",24);EditorUtility.SetDirty(ribbon);AssetDatabase.SaveAssets();VerifyField();TutorialWindBuild.VerifyAndBuild();}
 static void BakeField(){
  var island=PrefabUtility.LoadPrefabContents("Assets/Resources/MountainAero/Island.prefab");
  try{
   var home=island.GetComponentsInChildren<MeshFilter>().Single(m=>m.name=="Home landform");var collider=home.GetComponent<MeshCollider>();
   var c=Resources.Load<AetherConfig>("CompactIsland/Aether");var points=home.sharedMesh.vertices.Select(home.transform.TransformPoint).ToArray();
   var field=Resources.Load<IslandField>("CompactIsland/Field");if(!field){field=ScriptableObject.CreateInstance<IslandField>();AssetDatabase.CreateAsset(field,"Assets/Resources/CompactIsland/Field.asset");}
   const int count=256;field.distances=new float[count];field.farthest=points.Max(p=>new Vector2(p.x-c.islandCenter.x,p.z-c.islandCenter.z).magnitude);field.lowest=points.Min(p=>p.y);
   // Sample the actual authored collision footprint, including overhanging cliffs.
   for(int i=0;i<count;i++){float a=(i+.5f)*Mathf.PI*2/count;Vector3 direction=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));float far=0;
    for(float r=0;r<=field.farthest+1;r+=.25f){var origin=c.islandCenter+direction*r;origin.y=100;if(collider.Raycast(new Ray(origin,Vector3.down),out _,220))far=r;}
    field.distances[i]=far;
   }
   float radius=Mathf.Max(c.islandRadii.x,Mathf.Ceil(field.farthest+7));float vertical=c.islandRadii.y;
   foreach(var p in points){var d=p-c.islandCenter;float horizontal=(d.x*d.x+d.z*d.z)/(radius*radius);vertical=Mathf.Max(vertical,(Mathf.Abs(d.y)+2)/Mathf.Sqrt(Mathf.Max(.001f,1-horizontal)));}
   c.islandRadii=new Vector3(radius,Mathf.Ceil(vertical),radius);for(int i=0;i<4;i++)c.stages[i].radius=radius*(i+1)/4f;EditorUtility.SetDirty(c);
   if(!field.shoreline){field.shoreline=new Texture2D(count,1,TextureFormat.RFloat,false,true);field.shoreline.name="Home shoreline distance";AssetDatabase.AddObjectToAsset(field.shoreline,field);}
   if(!field.shell){field.shell=new Mesh{name="Smooth field shell"};AssetDatabase.AddObjectToAsset(field.shell,field);}
   var vertices=new List<Vector3>();var normals=new List<Vector3>();var indices=new List<int>();const int latitudes=48,longitudes=96;
   for(int y=0;y<=latitudes;y++)for(int x=0;x<=longitudes;x++){float lat=y*Mathf.PI/latitudes,lon=x*Mathf.PI*2/longitudes;var n=new Vector3(Mathf.Sin(lat)*Mathf.Cos(lon),Mathf.Cos(lat),Mathf.Sin(lat)*Mathf.Sin(lon));vertices.Add(n*.5f);normals.Add(n);}
   for(int y=0;y<latitudes;y++)for(int x=0;x<longitudes;x++){int a=y*(longitudes+1)+x,b=a+longitudes+1;indices.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
   field.shell.Clear();field.shell.SetVertices(vertices);field.shell.SetNormals(normals);field.shell.SetTriangles(indices,0);field.shell.RecalculateBounds();EditorUtility.SetDirty(field.shell);
   field.shoreline.wrapMode=TextureWrapMode.Repeat;field.shoreline.filterMode=FilterMode.Bilinear;
   for(int i=0;i<count;i++)field.shoreline.SetPixel(i,0,new Color(field.distances[i],0,0,1));field.shoreline.Apply();EditorUtility.SetDirty(field.shoreline);EditorUtility.SetDirty(field);AssetDatabase.SaveAssets();
   File.WriteAllText("Evidence/PlaytestFollowup/field.txt","Farthest shoreline="+field.farthest+" lowest="+field.lowest+" radii="+c.islandRadii+"\n");
  }finally{PrefabUtility.UnloadPrefabContents(island);}
 }
 static void VerifyField(){
  var field=Resources.Load<IslandField>("CompactIsland/Field");if(!field||field.distances.Length!=256)throw new Exception("Field missing");
  var c=Resources.Load<AetherConfig>("CompactIsland/Aether");var island=Resources.Load<GameObject>("MountainAero/Island");var home=island.GetComponentsInChildren<MeshFilter>().Single(m=>m.name=="Home landform");float worst=0;
  foreach(var v in home.sharedMesh.vertices){var d=home.transform.TransformPoint(v)-c.islandCenter;float ratio=d.x*d.x/(c.islandRadii.x*c.islandRadii.x)+d.y*d.y/(c.islandRadii.y*c.islandRadii.y)+d.z*d.z/(c.islandRadii.z*c.islandRadii.z);worst=Mathf.Max(worst,ratio);if(ratio>=1||!field.LowerContains(d,c.islandRadii))throw new Exception("Uncovered home vertex");}
  if(c.islandCenter.x+c.islandRadii.x>=134)throw new Exception("Starter field reaches neighbor");
  File.AppendAllText("Evidence/PlaytestFollowup/field.txt","All home mesh vertices enclosed; worst="+worst+". Neighbor excluded.\n");
 }
}
}

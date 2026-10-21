using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Skywright.Editor {
public static class RockFlashBuild {
 const string Evidence="Evidence/PlaytestFollowup/RockFlash/";
 [Serializable] class Geometry {public string name;public Vector3[] vertices;public int[] triangles;}
 public static void AuthorAndBuild(){SeatRocks();PlaytestFollowupBuild.VerifyAndBuild();}
 // The scan includes a shallow sheet of photographed ground below the solid boulder.
 // Its lowest vertex is not the boulder's foot. Bury that sheet, and remove the
 // old mountain slope from the presentation without changing saved resource roots.
 const float Shoulder=.38f;
 static float Ground(MeshCollider terrain,Vector3 p){
  if(!terrain.Raycast(new Ray(new Vector3(p.x,100,p.z),Vector3.down),out var hit,200))throw new Exception("Rock outside home terrain: "+p);
  return hit.point.y;
 }
 static void Seat(Transform root,MeshCollider terrain,List<string> report,string id){
  var source=Resources.Load<GameObject>("MountainAero/Limestone");
  var original=source.GetComponentsInChildren<MeshFilter>();
  var filters=root.GetComponentsInChildren<MeshFilter>();
  if(filters.Length!=3)throw new Exception("Expected three scan LODs for "+id);
  var scale=root.lossyScale;if(Mathf.Abs(scale.x-scale.y)>.001f||Mathf.Abs(scale.x-scale.z)>.001f)throw new Exception("Nonuniform rock root "+id);
  Vector3 p=root.position;float y=Ground(terrain,p);
  var normal=new Vector3(Ground(terrain,p-Vector3.right*.3f)-Ground(terrain,p+Vector3.right*.3f),.6f,Ground(terrain,p-Vector3.forward*.3f)-Ground(terrain,p+Vector3.forward*.3f)).normalized;
  var rotation=Quaternion.FromToRotation(Vector3.up,normal)*Quaternion.Euler(0,root.eulerAngles.y,0);
  foreach(var mf in filters){var src=original.Single(f=>f.name==mf.name);mf.transform.position=new Vector3(p.x,y,p.z);mf.transform.rotation=rotation*src.transform.localRotation;mf.transform.localScale=src.transform.localScale;}
  float shift=float.PositiveInfinity;
  // Check every skirt vertex in every LOD against actual triangulated terrain.
  // All of the scanned sheet must be below ground, including on uneven slopes.
  foreach(var mf in filters){var src=original.Single(f=>f.name==mf.name);foreach(var v in mf.sharedMesh.vertices){
   if(src.transform.TransformPoint(v).y>Shoulder)continue;
   var world=mf.transform.TransformPoint(v);shift=Mathf.Min(shift,Ground(terrain,world)-world.y-.045f);
  }}
  foreach(var mf in filters)mf.transform.position+=Vector3.up*shift;
  float maxSkirt=float.NegativeInfinity,maxVisible=float.NegativeInfinity;
  foreach(var mf in filters){var src=original.Single(f=>f.name==mf.name);foreach(var v in mf.sharedMesh.vertices){var world=mf.transform.TransformPoint(v);float clearance=world.y-Ground(terrain,world);maxVisible=Mathf.Max(maxVisible,clearance);if(src.transform.TransformPoint(v).y<=Shoulder)maxSkirt=Mathf.Max(maxSkirt,clearance);}}
  if(maxSkirt>-.04f||maxVisible<scale.x*.15f)throw new Exception("Invalid rock contact/visibility: "+id);
  var group=root.GetComponent<LODGroup>();if(group)group.RecalculateBounds();
  report.Add(id+" normal="+normal+" sheetClearance="+maxSkirt+" exposedHeight="+maxVisible+" shift="+shift);
 }
 public static void SeatRocks(){
  Directory.CreateDirectory(Evidence);var report=new List<string>();
  var island=PrefabUtility.LoadPrefabContents("Assets/Resources/MountainAero/Island.prefab");
  try{
   var terrain=island.GetComponentsInChildren<MeshFilter>().Single(f=>f.name=="Home landform").GetComponent<MeshCollider>();
   var resources=island.GetComponentsInChildren<SceneryResource>(true);var ids=resources.Select(r=>r.stableId).ToArray();
   foreach(var r in resources.Where(r=>r.kind=="outcrop")){var p=r.transform.position;var q=r.transform.rotation;var s=r.transform.localScale;Seat(r.transform,terrain,report,r.stableId);if(p!=r.transform.position||q!=r.transform.rotation||s!=r.transform.localScale)throw new Exception("Resource root moved");}
   if(!ids.SequenceEqual(island.GetComponentsInChildren<SceneryResource>(true).Select(r=>r.stableId)))throw new Exception("Resource identity changed");
   PrefabUtility.SaveAsPrefabAsset(island,"Assets/Resources/MountainAero/Island.prefab");
   for(int i=13;i<=14;i++){
    var node=UnityEngine.Object.Instantiate(Resources.Load<EarthConfig>("Earthcasting/Tuning").outcrop,EarthTrial.Locations[i],Quaternion.Euler(0,i*67%360,0)).GetComponent<EarthNode>();
    try{NaturalPreview.Outcrop(node,false);var visual=node.crown.GetChild(node.crown.childCount-1);
     // The historic fit used the entire ground sheet, leaving only a 15 cm
     // stone after burial. Give the exposed body enough height for Earthstone
     // swept collision to reach it before the surrounding terrain.
     visual.localScale*=3;Seat(visual,terrain,report,"earth-trial-outcrop-"+i);
     var clone=UnityEngine.Object.Instantiate(visual.gameObject);try{clone.transform.position=node.transform.InverseTransformPoint(visual.position);clone.transform.rotation=Quaternion.Inverse(node.transform.rotation)*visual.rotation;PrefabUtility.SaveAsPrefabAsset(clone,"Assets/Resources/CompactIsland/TrialOutcrop"+i+".prefab");}finally{UnityEngine.Object.DestroyImmediate(clone);}
    }finally{UnityEngine.Object.DestroyImmediate(node.gameObject);}
   }
  }finally{PrefabUtility.UnloadPrefabContents(island);}
  File.WriteAllLines(Evidence+"seating.txt",report);AssetDatabase.SaveAssets();
 }
 public static void Audit(){
  Directory.CreateDirectory(Evidence);var lines=new List<string>();
  var source=Resources.Load<GameObject>("MountainAero/Limestone");
  foreach(var f in source.GetComponentsInChildren<MeshFilter>()){
   var points=f.sharedMesh.vertices.Select(f.transform.TransformPoint).ToArray();
   File.WriteAllText(Evidence+f.name.Replace(" ","")+".json",JsonUtility.ToJson(new Geometry{name=f.name,vertices=points,triangles=f.sharedMesh.triangles}));
   lines.Add(f.name+" mesh="+f.sharedMesh.name+" transform="+f.transform.localRotation.eulerAngles+" vertices="+points.Length+" bounds="+f.GetComponent<Renderer>().bounds);
  }
  var island=Resources.Load<GameObject>("MountainAero/Island");
  foreach(var r in island.GetComponentsInChildren<SceneryResource>().Where(r=>r.kind=="outcrop")){
   lines.Add(r.stableId+" root="+r.transform.position.ToString("F3")+" rotation="+r.transform.rotation.eulerAngles.ToString("F3")+" scale="+r.transform.lossyScale);
   foreach(var f in r.GetComponentsInChildren<MeshFilter>())lines.Add("  "+f.name+" p="+f.transform.position+" rotation="+f.transform.rotation.eulerAngles+" bounds="+f.GetComponent<Renderer>().bounds);
  }
  File.WriteAllLines(Evidence+"geometry-audit.txt",lines);
 }
}
}

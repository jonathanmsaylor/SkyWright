using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Skywright.Editor {
public static class BuildNaturalPreview {
 [MenuItem("Skywright/Build Natural Aero visual preview")]
 public static void Build(){
  EarthSaveChecks.Run();AirSaveChecks.Run();var original=Resources.Load<PieceCatalog>("SkywrightCatalog");var next=Resources.Load<PieceCatalog>("NaturalAero/Catalog");
  if(!next||next.pieces.Length!=original.pieces.Length)throw new Exception("Incomplete sample catalog");
  string report="";
  foreach(var a in original.pieces){
   var b=next.pieces.Single(x=>x.typeId==a.typeId);
   if(a.dimensions!=b.dimensions||a.origin!=b.origin||JsonUtility.ToJson(new Anchors{items=a.anchors})!=JsonUtility.ToJson(new Anchors{items=b.anchors}))throw new Exception(a.typeId+" metadata changed");
   var x=UnityEngine.Object.Instantiate(a.prefab);var y=UnityEngine.Object.Instantiate(b.prefab);
   var cs=x.GetComponentsInChildren<Collider>();var ds=y.GetComponentsInChildren<Collider>();
   if(cs.Length!=ds.Length)throw new Exception("Collider count changed");
   for(int i=0;i<cs.Length;i++){
    if(cs[i].GetType()!=ds[i].GetType()||cs[i].transform.position!=ds[i].transform.position||Quaternion.Angle(cs[i].transform.rotation,ds[i].transform.rotation)>.001f||cs[i].transform.lossyScale!=ds[i].transform.lossyScale)throw new Exception(a.typeId+" collision transform changed");
    if(cs[i] is BoxCollider c&&ds[i] is BoxCollider d&&(c.center!=d.center||c.size!=d.size))throw new Exception(a.typeId+" box changed");
    if(cs[i] is MeshCollider m&&ds[i] is MeshCollider n&&(!m.sharedMesh.vertices.SequenceEqual(n.sharedMesh.vertices)||!m.sharedMesh.triangles.SequenceEqual(n.sharedMesh.triangles)))throw new Exception(a.typeId+" mesh collider changed");
   }
   report+=a.typeId+": identity, dimensions, origin, anchors and "+cs.Length+" colliders identical\n";UnityEngine.Object.DestroyImmediate(x.gameObject);UnityEngine.Object.DestroyImmediate(y.gameObject);
  }
  var tuning=Resources.Load<EarthConfig>("Earthcasting/Tuning");
  if(tuning.flightSpeed!=18||tuning.maxPlayerDistance!=20||tuning.maxTravelDistance!=40||tuning.maxFlightTime!=3||tuning.treeHits!=5||tuning.outcropHits!=8)throw new Exception("Earth tuning drift");
  Persistence.Read("Assets/StreamingAssets/natural-sample.json");
  Directory.CreateDirectory("Evidence/NaturalAero");File.WriteAllText("Evidence/NaturalAero/contracts.txt",report);
  AssetDatabase.SaveAssets();Directory.CreateDirectory("Builds/Visual_Preview");
  var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/FirstHome.unity"},locationPathName="Builds/Visual_Preview/Skywright.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  Debug.Log("NATURAL_VISUAL_BUILD "+r.summary.result+" errors="+r.summary.totalErrors);
  if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Visual build failed");
  File.WriteAllText("Builds/Visual_Preview/visual.preview","Isolated Skywright visual sample; uses visual-preview.json.");
 }
 [Serializable] class Anchors{public SnapAnchor[] items;}
 public static void AuthorAndBuild(){NaturalAeroAuthor.Author();Build();}
}
}

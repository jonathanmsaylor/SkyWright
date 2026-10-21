using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Skywright.Editor {
public static class BuildElemental {
 [MenuItem("Skywright/Build Elemental preview")]
 public static void Build(){
  EarthSaveChecks.Run();AirSaveChecks.Run();
  const string scene="Assets/Scenes/FirstHome.unity";
  if(!File.Exists(scene))throw new Exception("Existing FirstHome scene missing.");
  var catalog=Resources.Load<PieceCatalog>("SkywrightCatalog");
  if(!catalog||catalog.pieces.Length!=9)throw new Exception("Author nine sample prefabs first.");
  string report="M1 collision compatibility\n";
  foreach(var def in catalog.pieces){
   var before=Kit.CreateLegacy(def.typeId,Vector3.zero,Quaternion.identity,null);
   var after=UnityEngine.Object.Instantiate(def.prefab);
   var a=before.GetComponentsInChildren<Collider>();var b=after.GetComponentsInChildren<Collider>();
   if(a.Length!=b.Length)throw new Exception(def.typeId+" collider count changed");
   for(int i=0;i<a.Length;i++){
    if(a[i].GetType()!=b[i].GetType()||Vector3.Distance(a[i].transform.position,b[i].transform.position)>.00001f||Quaternion.Angle(a[i].transform.rotation,b[i].transform.rotation)>.001f||Vector3.Distance(a[i].transform.lossyScale,b[i].transform.lossyScale)>.00001f)throw new Exception(def.typeId+" collider transform changed");
    if(a[i] is BoxCollider ac&&b[i] is BoxCollider bc&&(ac.center!=bc.center||ac.size!=bc.size))throw new Exception(def.typeId+" box changed");
    if(a[i] is MeshCollider am&&b[i] is MeshCollider bm&&(!am.sharedMesh.vertices.SequenceEqual(bm.sharedMesh.vertices)||!am.sharedMesh.triangles.SequenceEqual(bm.sharedMesh.triangles)))throw new Exception(def.typeId+" mesh collision changed");
   }
   report+=def.typeId+": "+a.Length+" collider contracts identical\n";
   UnityEngine.Object.DestroyImmediate(before.gameObject);UnityEngine.Object.DestroyImmediate(after.gameObject);
  }
  Directory.CreateDirectory("Evidence/AirMagic");File.WriteAllText("Evidence/AirMagic/collider-compatibility.txt",report);
  if(!Resources.Load<EarthConfig>("Earthcasting/Tuning"))throw new Exception("Earthcasting tuning missing.");
  AssetDatabase.SaveAssets();
  Directory.CreateDirectory("Builds/Elemental_Preview");
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scene},locationPathName="Builds/Elemental_Preview/Skywright.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  Debug.Log("SKYWRIGHT_ELEMENTAL_BUILD "+result.summary.result+" errors="+result.summary.totalErrors);
  if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Preview build failed");
  File.WriteAllText("Builds/Elemental_Preview/elemental.preview","This executable defaults to the isolated Elemental trial.");
 }
 public static void AuthorAndBuild(){AuthorAirMagic.Author();Build();}
}
}

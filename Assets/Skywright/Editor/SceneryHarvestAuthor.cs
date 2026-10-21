using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Skywright.Editor {
public static class SceneryHarvestAuthor {
 // Position is used only to reconcile a deterministic re-authoring pass against existing authored GUIDs.
 // Runtime saves always use the explicit serialized GUID.
 static string Key(string kind,Vector3 p)=>kind+":"+p.x.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+":"+p.y.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+":"+p.z.ToString("R",System.Globalization.CultureInfo.InvariantCulture);
 public static Dictionary<string,string> Existing(){
  return SceneryResource.Definitions().ToDictionary(x=>Key(x.kind,x.transform.position),x=>x.stableId);
 }
 public static void Assign(Transform root,Dictionary<string,string> prior=null){
  var chosen=new Dictionary<GameObject,string>();
  foreach(Transform t in root)if(t.name=="Woodland tree")chosen[t.gameObject]="tree";
  var wood=root.Find("Finished woodland edge");
  if(wood)foreach(Transform t in wood)if(t.GetComponent<CapsuleCollider>())chosen[t.gameObject]="tree";
  foreach(var name in new[]{"Limestone outcrops","Composed limestone groups"}){
   var group=root.Find(name);if(group)foreach(Transform t in group)if(t.GetComponentInChildren<MeshFilter>())chosen[t.gameObject]="outcrop";
  }
  foreach(var pair in chosen){
   var g=pair.Key;var marker=g.GetComponent<SceneryResource>();
   if(!marker)marker=g.AddComponent<SceneryResource>();marker.kind=pair.Value;
   if(string.IsNullOrEmpty(marker.stableId))marker.stableId=prior!=null&&prior.TryGetValue(Key(marker.kind,g.transform.position),out var id)?id:"meadow-"+Guid.NewGuid().ToString("N");
   if(!g.GetComponentInChildren<Collider>()){
    var f=g.transform.Find("LOD 2")?.GetComponentInChildren<MeshFilter>()??g.GetComponentInChildren<MeshFilter>();
    if(f)f.gameObject.AddComponent<MeshCollider>().sharedMesh=f.sharedMesh;
   }
  }
  var all=root.GetComponentsInChildren<SceneryResource>(true);
  if(all.Any(x=>string.IsNullOrEmpty(x.stableId))||all.Select(x=>x.stableId).Distinct().Count()!=all.Length)throw new Exception("Duplicate scenery resource identity");
  Directory.CreateDirectory("Evidence/FinishedMeadow");
  File.WriteAllLines("Evidence/FinishedMeadow/scenery-resource-catalog.txt",all.Select(x=>x.stableId+" "+x.kind+" "+x.transform.position+" "+x.name));
  Debug.Log("SCENERY_HARVEST trees="+all.Count(x=>x.kind=="tree")+" rocks="+all.Count(x=>x.kind=="outcrop"));
 }
 public static void ConvertAndBuild(){
  var path="Assets/Resources/MountainAero/Island.prefab";var g=PrefabUtility.LoadPrefabContents(path);
  try{Assign(g.transform);PrefabUtility.SaveAsPrefabAsset(g,path);}finally{PrefabUtility.UnloadPrefabContents(g);}
  AssetDatabase.SaveAssets();MountainAeroAuthor.Build();
 }
}
}

using System;
using System.Linq;
using UnityEngine;
namespace Skywright {
// Authored GUID is the save identity. Names, hierarchy order and meshes are presentation only.
public class SceneryResource : MonoBehaviour {
 public string stableId,kind;
 EarthNode runtimeNode;
 public EarthNode Restore(EarthNodeState saved) {
  if(!runtimeNode) {
   runtimeNode=gameObject.AddComponent<EarthNode>();
   var children=transform.Cast<Transform>().ToArray();
   var crown=new GameObject("Harvestable visual").transform;crown.SetParent(transform,false);
   foreach(var child in children)child.SetParent(crown,false);
   runtimeNode.crown=crown;
   runtimeNode.harvestingCollider=GetComponentsInChildren<Collider>(true).FirstOrDefault();
   if(kind=="tree"){
    var source=Resources.Load<GameObject>("NaturalAero/Stump");
    if(source){var stump=Instantiate(source,transform,false);stump.name="Felled tree stump";stump.transform.localScale=Vector3.one*(.25f/Mathf.Max(.1f,transform.lossyScale.x));}
   }
  }
  var n=runtimeNode;if(n.wound)Destroy(n.wound.gameObject);if(n.loot)Destroy(n.loot.gameObject);
  n.wound=n.loot=null;n.state=saved;n.fallClock=n.collectClock=-1;n.reaction=0;
  n.crown.localRotation=Quaternion.identity;n.crown.localScale=Vector3.one;n.restingRotation=Quaternion.identity;
  n.crown.gameObject.SetActive(!saved.depleted);
  foreach(var c in n.GetComponentsInChildren<Collider>(true))c.enabled=!saved.depleted;
  var stumpVisual=transform.Find("Felled tree stump");if(stumpVisual)stumpVisual.gameObject.SetActive(saved.depleted);
  return n;
 }
 public static SceneryResource[] Definitions() {
  var island=Resources.Load<GameObject>("MountainAero/Island");
  return island?island.GetComponentsInChildren<SceneryResource>(true):Array.Empty<SceneryResource>();
 }
}
}

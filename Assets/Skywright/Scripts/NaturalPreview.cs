using System;
using System.IO;
using UnityEngine;
namespace Skywright {
public static class NaturalPreview {
 public static readonly bool Active=Array.IndexOf(System.Environment.GetCommandLineArgs(),"-visualpreview")>=0||Array.IndexOf(System.Environment.GetCommandLineArgs(),"-visualqa")>=0||File.Exists(Path.Combine(Application.dataPath,"../visual.preview"));
 public static readonly bool QA=Array.IndexOf(System.Environment.GetCommandLineArgs(),"-visualqa")>=0;
 public static void Environment(Transform root){MountainEnvironment.Lighting(root);}
 public static void Tree(EarthNode node){
  var source=Resources.Load<GameObject>("FinishedMeadow/HarvestTree")??Resources.Load<GameObject>("MountainAero/Broadleaf");if(!source)return;
  // Functional crown pivot, node identity, damage collider and fall rules stay unchanged.
  foreach(var r in node.GetComponentsInChildren<Renderer>())r.enabled=false;
  var visual=UnityEngine.Object.Instantiate(source,node.crown,false);visual.transform.localPosition=-node.crown.localPosition;
  // Natural tapered trunk replaces the old forced 0.3 m cylinder. Root identity is unchanged.
  if(node.harvestingCollider is CapsuleCollider capsule){capsule.radius=.17f;capsule.height=3.3f;capsule.center=new Vector3(.015f,1.65f,0);}
  var stump=Resources.Load<GameObject>("NaturalAero/Stump");if(stump){var baseVisual=UnityEngine.Object.Instantiate(stump,node.transform,false);baseVisual.name="Felled tree stump";baseVisual.transform.localScale=new Vector3(.38f,.25f,.38f);baseVisual.SetActive(node.state.depleted);}
 }
 public static void Outcrop(EarthNode node,bool baked=true){
  var authored=baked?Resources.Load<GameObject>("CompactIsland/TrialOutcrop"+(node.transform.position.x>0?13:14)):null;
  if(authored){foreach(var r in node.GetComponentsInChildren<Renderer>())r.enabled=false;node.harvestingCollider.enabled=false;var rock=UnityEngine.Object.Instantiate(authored,node.crown,false);rock.transform.localPosition-=node.crown.localPosition;node.harvestingCollider=rock.GetComponentInChildren<MeshCollider>();return;}
  var source=Resources.Load<GameObject>("MountainAero/Limestone");if(!source)return;
  foreach(var r in node.GetComponentsInChildren<Renderer>())r.enabled=false;
  var visual=UnityEngine.Object.Instantiate(source,node.crown,false);
  visual.transform.localPosition=-node.crown.localPosition;
  // Fit the scanned silhouette to the established collider footprint without moving the saved root.
  var bounds=new Bounds();bool first=true;
  foreach(var r in visual.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
  var target=node.harvestingCollider.bounds;
  float scale=Mathf.Min(target.size.x/Mathf.Max(.01f,bounds.size.x),target.size.y/Mathf.Max(.01f,bounds.size.y),target.size.z/Mathf.Max(.01f,bounds.size.z));
  visual.transform.localScale*=scale;
  bounds=new Bounds();first=true;
  foreach(var r in visual.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
  visual.transform.position+=target.center-bounds.center;
  // The scan is shallower than the old cluster. Ground its actual underside instead of suspending it at the old sphere center.
  visual.transform.position+=Vector3.up*(node.transform.position.y-.035f-(target.center.y-bounds.size.y*.5f));
  var low=visual.transform.Find("LOD 2").GetComponentInChildren<MeshFilter>();
  var collision=low.gameObject.AddComponent<MeshCollider>();collision.sharedMesh=low.sharedMesh;
  node.harvestingCollider.enabled=false;node.harvestingCollider=collision;
 }
 public static void TreeFelled(EarthNode node){var stump=node.transform.Find("Felled tree stump");if(stump)stump.gameObject.SetActive(true);}
}
}

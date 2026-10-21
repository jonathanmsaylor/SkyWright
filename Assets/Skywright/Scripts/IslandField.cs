using UnityEngine;
namespace Skywright {
// Baked from the home landform, not scenery or player construction.
public sealed class IslandField : ScriptableObject {
 public Texture2D shoreline;public Mesh shell;public float[] distances;public float farthest;public float lowest;
 public float Reveal(Vector3 offset,Vector3 radii){
  float u=Mathf.Repeat(Mathf.Atan2(offset.z,offset.x)/(2*Mathf.PI)-.5f/distances.Length,1)*distances.Length;
  int i=Mathf.FloorToInt(u);float shore=Mathf.Lerp(distances[i],distances[(i+1)%distances.Length],u-i);
  var unit=new Vector3(offset.x/radii.x,offset.y/radii.y,offset.z/radii.z).normalized;
  // At the pole all meridians meet: blend to the last shoreline to avoid a pinwheel seam.
  shore=Mathf.Lerp(shore,farthest,Mathf.SmoothStep(0,1,Mathf.Clamp01((-unit.y-.85f)/.15f)));
  return Mathf.SmoothStep(0,1,Mathf.Clamp01((radii.x-shore)/6));
 }
 public bool LowerContains(Vector3 offset,Vector3 radii){
  if(offset.y>=0)return true;float reveal=Reveal(offset,radii);
  // Evaluate the direction's shell latitude, matching the rendered reveal mask.
  var unit=new Vector3(offset.x/radii.x,offset.y/radii.y,offset.z/radii.z).normalized;
  return reveal>.02f&&unit.y>=-reveal*1.1f;
 }
}
}

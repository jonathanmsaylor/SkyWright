using UnityEngine;
using UnityEngine.Rendering;
namespace Skywright {
public static class ArtEnvironment {
 public static void Apply(Transform root){
  var garden=Resources.Load<GameObject>("SkywrightArt/SampleGarden");
  if(garden)Object.Instantiate(garden,root);
  if(RenderSettings.sun){
   RenderSettings.sun.color=new Color(1,.96f,.86f);
   RenderSettings.sun.intensity=1.65f;
   RenderSettings.sun.transform.rotation=Quaternion.Euler(42,135,0);
  }
  RenderSettings.ambientSkyColor=new Color(.57f,.72f,.86f);
  RenderSettings.ambientEquatorColor=new Color(.63f,.69f,.68f);
  RenderSettings.ambientGroundColor=new Color(.40f,.43f,.35f);
  RenderSettings.fogColor=new Color(.63f,.83f,.92f);
  RenderSettings.fogDensity=.0015f;
  var sky=Resources.Load<Material>("SkywrightArt/AeroSky");
  if(sky)RenderSettings.skybox=sky;
 }
}
}

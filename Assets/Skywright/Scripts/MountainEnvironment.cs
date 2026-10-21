using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Skywright {
// Runtime only instantiates authored assets. No world generation or save changes.
public static class MountainEnvironment {
 public static void Create(Transform parent){
  var source=Resources.Load<GameObject>("MountainAero/Island");
  if(!source)throw new System.InvalidOperationException("Mountain island authoring assets missing.");
  Object.Instantiate(source,parent,false);
  var sun=new GameObject("Mountain daylight").AddComponent<Light>();sun.transform.SetParent(parent);sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(36,-48,0);sun.intensity=1.35f;sun.color=new Color(1,.97f,.90f);sun.shadows=LightShadows.Soft;sun.shadowBias=.035f;sun.shadowNormalBias=.12f;RenderSettings.sun=sun;
 }
 public static void Lighting(Transform parent){
  var pipeline=Resources.Load<UniversalRenderPipelineAsset>("MountainAero/MountainPipeline");if(pipeline)QualitySettings.renderPipeline=pipeline;
  RenderSettings.skybox=Resources.Load<Material>("MountainAero/Sky");
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.40f,.57f,.75f);RenderSettings.ambientEquatorColor=new Color(.39f,.46f,.48f);RenderSettings.ambientGroundColor=new Color(.21f,.25f,.20f);
  RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.00115f;RenderSettings.fogColor=new Color(.58f,.77f,.87f);RenderSettings.reflectionIntensity=.8f;
  var volume=new GameObject("Mountain daylight grading").AddComponent<Volume>();volume.transform.SetParent(parent);volume.isGlobal=true;
  volume.sharedProfile=Resources.Load<VolumeProfile>("MountainAero/Daylight");
  DynamicGI.UpdateEnvironment();
 }
 public static void Camera(Camera camera){camera.farClipPlane=1600;var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;data.antialiasingQuality=AntialiasingQuality.High;}
}
}

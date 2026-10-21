using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Skywright {
// Opt-in isolated visual diagnosis. Only rendering changes; never writes game state.
public sealed class PreviewRenderCheck : MonoBehaviour {
 int mode;Renderer[] effects;Camera cam;Material sky;bool post;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Install(){if(NaturalPreview.QA&&Array.IndexOf(Environment.GetCommandLineArgs(),"-rendercheck")>=0)new GameObject("Render check").AddComponent<PreviewRenderCheck>();}
 void Start(){cam=Camera.main;sky=RenderSettings.skybox;post=cam.GetUniversalAdditionalCameraData().renderPostProcessing;
  Debug.Log("RENDER_CHECK sky="+(sky?sky.shader.name:"none")+" sun="+(RenderSettings.sun?RenderSettings.sun.name:"none")+" cameras="+Camera.allCamerasCount+" lights="+FindObjectsByType<Light>(FindObjectsSortMode.None).Length+" flares="+FindObjectsByType<LensFlareComponentSRP>(FindObjectsSortMode.None).Length+" post="+post);
 }
 void Update(){if(Keyboard.current==null||!Keyboard.current.f6Key.wasPressedThisFrame)return;
  if(effects!=null)foreach(var r in effects)if(r)r.forceRenderingOff=false;
  cam.GetUniversalAdditionalCameraData().renderPostProcessing=post;cam.clearFlags=CameraClearFlags.Skybox;RenderSettings.skybox=sky;
  mode=(mode+1)%4;
  if(mode==1){effects=FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.gameObject.layer==2).ToArray();foreach(var r in effects)r.forceRenderingOff=true;}
  if(mode==2)cam.GetUniversalAdditionalCameraData().renderPostProcessing=false;
  if(mode==3){cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.3f,.6f,.8f);}
  Debug.Log("RENDER_CHECK mode="+mode+" 0=normal 1=effects-hidden 2=post-off 3=flat-sky");
 }
 void OnGUI(){GUI.Label(new Rect(20,Screen.height/2,450,30),"Render check: F6 / "+new[]{"Normal","Effects hidden","Post off","Flat sky"}[mode]);}
}
}

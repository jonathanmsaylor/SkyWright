using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Skywright.Editor {
public static class BuildSkywright {
 public static void Build(){
  PlayerSettings.companyName="Skywright";PlayerSettings.productName="Skywright";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
  PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
  string[] ids=AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");UniversalRenderPipelineAsset pipeline=null;
  foreach(string id in ids){var p=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(id));if(p){pipeline=p;if(p.name.Contains("PC"))break;}}
  if(!pipeline)throw new Exception("URP template pipeline missing");
  pipeline.renderScale=1;pipeline.msaaSampleCount=4;pipeline.shadowDistance=100;
  GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
  Directory.CreateDirectory("Assets/Resources");AssetDatabase.Refresh();
  if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/SkywrightLit.mat"))AssetDatabase.CreateAsset(new Material(Shader.Find("Universal Render Pipeline/Lit")),"Assets/Resources/SkywrightLit.mat");
  if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/SkywrightSky.mat"))AssetDatabase.CreateAsset(new Material(Shader.Find("Skybox/Procedural")),"Assets/Resources/SkywrightSky.mat");
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  new GameObject("Skywright").AddComponent<SkywrightGame>();
  const string scenePath="Assets/Scenes/FirstHome.unity";EditorSceneManager.SaveScene(scene,scenePath);
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};AssetDatabase.SaveAssets();
  Directory.CreateDirectory("Builds/Windows");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scenePath},locationPathName="Builds/Windows/Skywright.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  Debug.Log("SKYWRIGHT_BUILD "+report.summary.result+" errors="+report.summary.totalErrors);
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
 }
}
}

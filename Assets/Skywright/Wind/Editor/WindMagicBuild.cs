using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Skywright.Editor {
public static class WindMagicBuild {
 public static void Build(){
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  var shader=Shader.Find("Skywright/AeroFlow");if(!shader)throw new Exception("Airflow shader missing");
  const string folder="Assets/Resources/WindMagic/";
  var mat=AssetDatabase.LoadAssetAtPath<Material>(folder+"AeroFlow.mat");
  if(!mat){mat=new Material(shader);AssetDatabase.CreateAsset(mat,folder+"AeroFlow.mat");}
  foreach(bool inward in new[]{true,false}){
   string name=inward?"Suction":"Push";string path=folder+name+".prefab";
   // Existing authored tuning remains authoritative on rebuild.
   if(AssetDatabase.LoadAssetAtPath<GameObject>(path))continue;
   var go=new GameObject(name);var flow=go.AddComponent<WindFlow>();flow.material=mat;flow.inward=inward;
   flow.ribbons=inward?12:10;flow.motes=inward?14:18;flow.curvature=inward?1.2f:.7f;flow.ribbonWidth=inward?.085f:.095f;
   PrefabUtility.SaveAsPrefabAsset(go,path);UnityEngine.Object.DestroyImmediate(go);
  }
  EarthSaveChecks.Run();AirSaveChecks.Run();
  var graphics=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;bool timing=PlayerSettings.enableFrameTimingStats;
  try{
   var pipeline=Resources.Load<UniversalRenderPipelineAsset>("MountainAero/MountainPipeline");GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;PlayerSettings.enableFrameTimingStats=true;AssetDatabase.SaveAssets();
   BuildOne("Builds/WindMagic_Preview","Skywright","Assets/Scenes/FirstHome.unity");
   BuildOne("Builds/WindMagic_Combat","StoneSentinelCombat","Assets/StoneSentinelCombat/SentinelCombat.unity");
  }finally{GraphicsSettings.defaultRenderPipeline=graphics;QualitySettings.renderPipeline=quality;PlayerSettings.enableFrameTimingStats=timing;AssetDatabase.SaveAssets();}
 }
 static void BuildOne(string folder,string exe,string scene){
  Directory.CreateDirectory(folder);File.WriteAllText(folder+"/visual.preview","Wind magic review\n");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scene},locationPathName=folder+"/"+exe+".exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  Debug.Log("WIND_BUILD "+folder+" "+report.summary.result+" errors="+report.summary.totalErrors);
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Wind build failed");
 }
}
}

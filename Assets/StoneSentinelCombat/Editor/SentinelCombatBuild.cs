using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Skywright.Editor {
public static class SentinelCombatBuild {
 const string Folder="Assets/StoneSentinelCombat";
 public static void SaveContracts(){EarthSaveChecks.Run();AirSaveChecks.Run();Debug.Log("SENTINEL_EXISTING_SAVE_CONTRACTS_PASSED");}
 public static void Build(){
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  string path=Folder+"/Models/SentinelCombat.fbx";
  var importer=(ModelImporter)AssetImporter.GetAtPath(path);
  importer.animationType=ModelImporterAnimationType.Legacy;importer.importAnimation=true;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importCameras=false;importer.importLights=false;
  var clips=importer.defaultClipAnimations;
  foreach(var clip in clips){string name=clip.name.Split('|').Last();clip.name=name;clip.wrapMode=name=="Idle"||name=="Walk"?WrapMode.Loop:WrapMode.ClampForever;clip.loopTime=name=="Idle"||name=="Walk";}
  importer.clipAnimations=clips;importer.SaveAndReimport();
  var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);var root=new GameObject("Stone Sentinel combat");
  var visual=(GameObject)PrefabUtility.InstantiatePrefab(model);visual.transform.SetParent(root.transform,false);
  var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/StoneSentinelTrial/Sentinel.mat");
  string corePath=Folder+"/InnerStone.mat";var core=AssetDatabase.LoadAssetAtPath<Material>(corePath);
  if(!core){core=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(core,corePath);}
  core.CopyPropertiesFromMaterial(mat);core.SetColor("_BaseColor",new Color(.42f,.48f,.45f));core.SetFloat("_Smoothness",.2f);core.SetFloat("_Metallic",0);core.EnableKeyword("_EMISSION");core.SetColor("_EmissionColor",Color.white*.18f);EditorUtility.SetDirty(core);
  foreach(var r in visual.GetComponentsInChildren<Renderer>()){var m=r.sharedMaterials;for(int i=0;i<m.Length;i++)m[i]=r.name.StartsWith("Core_")?core:mat;r.sharedMaterials=m;if(r is SkinnedMeshRenderer skin){skin.updateWhenOffscreen=true;skin.localBounds=new Bounds(Vector3.up*1.2f,new Vector3(4,4,4));}}
  var animation=visual.GetComponent<Animation>();if(!animation)animation=visual.AddComponent<Animation>();
  var actual=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview")).ToArray();
  foreach(var c in actual){string name=c.name.Split('|').Last();animation.AddClip(c,name);}
  foreach(string name in new[]{"Idle","Walk","Windup","Slam","Recover","Stagger","Defeat","Swat","Dormant","Awaken","LeapWindup","Leap","LeapRecover"})if(animation[name]==null)throw new Exception("Missing clip "+name+"; got "+string.Join(",",actual.Select(c=>c.name)));
  animation.playAutomatically=false;animation.cullingType=AnimationCullingType.AlwaysAnimate;
  var prefab=PrefabUtility.SaveAsPrefabAsset(root,Folder+"/StoneSentinelCombat.prefab");UnityEngine.Object.DestroyImmediate(root);
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  string warningPath=Folder+"/SlamWarning.mat";var warning=AssetDatabase.LoadAssetAtPath<Material>(warningPath);
  if(!warning){warning=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(warning,warningPath);}warning.color=new Color(1,.58f,.18f);
  var chipImport=(ModelImporter)AssetImporter.GetAtPath(Folder+"/Models/LimestoneDebris.fbx");chipImport.importAnimation=false;chipImport.isReadable=true;chipImport.materialImportMode=ModelImporterMaterialImportMode.None;chipImport.SaveAndReimport();
  var chips=AssetDatabase.LoadAllAssetsAtPath(Folder+"/Models/LimestoneDebris.fbx").OfType<Mesh>().OrderBy(m=>m.name).ToArray();if(chips.Length!=3)throw new Exception("Expected three polished limestone meshes");
  for(int i=0;i<chips.Length;i++){
   var baked=UnityEngine.Object.Instantiate(chips[i]);baked.name="Limestone chip "+(i+1);var b=baked.bounds;float extent=Mathf.Max(b.size.x,b.size.y,b.size.z);var vertices=baked.vertices;
   for(int j=0;j<vertices.Length;j++)vertices[j]=(vertices[j]-b.center)/extent;baked.vertices=vertices;baked.RecalculateBounds();
   string meshPath=Folder+"/Models/LimestoneChip"+(i+1)+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(existing){EditorUtility.CopySerialized(baked,existing);UnityEngine.Object.DestroyImmediate(baked);chips[i]=existing;}else{AssetDatabase.CreateAsset(baked,meshPath);chips[i]=baked;}
  }
  string chipPath=Folder+"/LimestoneDebris.mat";var chipMat=AssetDatabase.LoadAssetAtPath<Material>(chipPath);if(!chipMat){chipMat=new Material(mat);AssetDatabase.CreateAsset(chipMat,chipPath);}chipMat.CopyPropertiesFromMaterial(Resources.Load<Material>("NaturalAero/Limestone"));chipMat.SetColor("_BaseColor",new Color(1,.97f,.86f));chipMat.SetColor("_EmissionColor",Color.black);chipMat.SetFloat("_Smoothness",.17f);chipMat.SetFloat("_BumpScale",.65f);EditorUtility.SetDirty(chipMat);
  var owner=new GameObject("Sentinel combat trial");var encounter=owner.AddComponent<SentinelCombat>();encounter.creaturePrefab=prefab;encounter.warningMaterial=warning;encounter.limestoneChips=chips;encounter.limestoneMaterial=chipMat;owner.AddComponent<SkywrightGame>();
  string scenePath=Folder+"/SentinelCombat.unity";EditorSceneManager.SaveScene(scene,scenePath);
  var graphics=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;
  try{
   var pipeline=Resources.Load<UniversalRenderPipelineAsset>("MountainAero/MountainPipeline");GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;AssetDatabase.SaveAssets();
   string output="Builds/StoneSentinel_Combat";Directory.CreateDirectory(output);File.WriteAllText(output+"/visual.preview","Sentinel session-only combat\n");
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scenePath},locationPathName=output+"/StoneSentinelCombat.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
   Debug.Log("SENTINEL_COMBAT_BUILD "+report.summary.result+" errors="+report.summary.totalErrors);
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Combat build failed");
  }finally{GraphicsSettings.defaultRenderPipeline=graphics;QualitySettings.renderPipeline=quality;AssetDatabase.SaveAssets();}
 }
}
}

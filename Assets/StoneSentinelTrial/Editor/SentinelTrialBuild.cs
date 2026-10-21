using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skywright.SentinelReview.Editor {
public static class SentinelTrialBuild {
 const string Folder="Assets/StoneSentinelTrial";
 public static void Build(){
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  ConfigureTextures();
  var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Sentinel limestone, moss and aether"};
  mat.SetTexture("_BaseMap",Load("base-color"));mat.SetColor("_BaseColor",Color.white);
  mat.SetTexture("_BumpMap",Load("normal"));mat.SetFloat("_BumpScale",1);mat.EnableKeyword("_NORMALMAP");
  mat.SetTexture("_MetallicGlossMap",Load("metallic-smoothness"));mat.EnableKeyword("_METALLICSPECGLOSSMAP");mat.SetFloat("_Smoothness",1);
  mat.SetTexture("_EmissionMap",Load("emission"));mat.SetColor("_EmissionColor",Color.white*.8f);mat.EnableKeyword("_EMISSION");
  mat.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;
  mat=SaveMaterial(mat,Folder+"/Sentinel.mat");
  var prefabRoot=new GameObject("Stone Sentinel");var lods=new LOD[3];
  for(int i=0;i<3;i++){
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Models/Sentinel_LOD"+i+".fbx");
   if(!asset)throw new Exception("Sentinel model missing: LOD"+i);
   var g=(GameObject)PrefabUtility.InstantiatePrefab(asset);g.name="LOD "+i;g.transform.SetParent(prefabRoot.transform,false);
   foreach(var r in g.GetComponentsInChildren<Renderer>()){
    var materials=r.sharedMaterials;for(int j=0;j<materials.Length;j++)materials[j]=mat;r.sharedMaterials=materials;
   }
   lods[i]=new LOD(i==0?.24f:i==1?.10f:.025f,g.GetComponentsInChildren<Renderer>());
  }
  var group=prefabRoot.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
  var bounds=lods[0].renderers[0].bounds;foreach(var renderer in lods[0].renderers)bounds.Encapsulate(renderer.bounds);
  if(Mathf.Abs(bounds.size.y-2.4f)>.025f||Mathf.Abs(bounds.min.y)>.025f)throw new Exception("Sentinel scale or ground pivot invalid: "+bounds);
  Directory.CreateDirectory("Evidence/PlaytestFollowup/StoneSentinel");File.WriteAllText("Evidence/PlaytestFollowup/StoneSentinel/unity-asset-audit.txt","LOD0 world bounds: "+bounds+"\nThree LOD levels; one shared material; no gameplay collider, rig or save owner.\n");
  var prefab=PrefabUtility.SaveAsPrefabAsset(prefabRoot,Folder+"/StoneSentinel.prefab");UnityEngine.Object.DestroyImmediate(prefabRoot);
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var trial=new GameObject("Stone Sentinel visual review").AddComponent<SentinelTrial>();trial.sentinelPrefab=prefab;
  string scenePath=Folder+"/SentinelReview.unity";EditorSceneManager.SaveScene(scene,scenePath);
  var graphics=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;
  try{
   var pipeline=Resources.Load<UniversalRenderPipelineAsset>("MountainAero/MountainPipeline");
   GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;AssetDatabase.SaveAssets();
   Directory.CreateDirectory("Builds/StoneSentinel_Trial");
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scenePath},locationPathName="Builds/StoneSentinel_Trial/StoneSentinel.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
   Debug.Log("SENTINEL_BUILD "+report.summary.result+" errors="+report.summary.totalErrors);
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Sentinel trial build failed");
  }finally{GraphicsSettings.defaultRenderPipeline=graphics;QualitySettings.renderPipeline=quality;AssetDatabase.SaveAssets();}
 }
 static Texture2D Load(string name){var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/Textures/"+name+".png");if(!tex)throw new Exception("Missing "+name);return tex;}
 static Material SaveMaterial(Material mat,string path){var old=AssetDatabase.LoadAssetAtPath<Material>(path);if(old){EditorUtility.CopySerialized(mat,old);UnityEngine.Object.DestroyImmediate(mat);return old;}AssetDatabase.CreateAsset(mat,path);return mat;}
 static void ConfigureTextures(){
  foreach(string name in new[]{"base-color","normal","metallic-smoothness","emission"}){
   var imp=(TextureImporter)AssetImporter.GetAtPath(Folder+"/Textures/"+name+".png");if(!imp)throw new Exception("Missing texture "+name);
   imp.textureType=name=="normal"?TextureImporterType.NormalMap:TextureImporterType.Default;
   imp.sRGBTexture=name=="base-color"||name=="emission";imp.maxTextureSize=4096;imp.mipmapEnabled=true;
   imp.alphaSource=name=="metallic-smoothness"?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;
   imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.anisoLevel=4;imp.SaveAndReimport();
  }
  for(int i=0;i<3;i++){
   var imp=(ModelImporter)AssetImporter.GetAtPath(Folder+"/Models/Sentinel_LOD"+i+".fbx");if(!imp)throw new Exception("Missing FBX");
   imp.materialImportMode=ModelImporterMaterialImportMode.None;imp.importAnimation=false;imp.isReadable=false;imp.importCameras=false;imp.importLights=false;imp.SaveAndReimport();
  }
 }
}
}

using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;
namespace Skywright.Editor {
public static class AetherAuthor {
 const string D="Assets/Resources/CompactIsland";
 public static void Author(){
  Directory.CreateDirectory(D);AssetDatabase.Refresh();
  var config=AssetDatabase.LoadAssetAtPath<AetherConfig>(D+"/Aether.asset");
  if(!config){config=ScriptableObject.CreateInstance<AetherConfig>();AssetDatabase.CreateAsset(config,D+"/Aether.asset");}
  config.anchorPosition=new Vector3(12,0,-20);config.islandCenter=config.anchorPosition+Vector3.down*.5f;
  float radius=0;for(int z=-54;z<=54;z++)for(int x=-68;x<=68;x++)if(CompactIslandAuthor.Inside(x,z))radius=Mathf.Max(radius,Vector2.Distance(new Vector2(x,z),new Vector2(config.anchorPosition.x,config.anchorPosition.z)));
  radius=Mathf.Ceil(radius+8);config.islandRadii=new Vector3(radius,radius*.8f,radius);
  config.suctionRange=8;config.suctionCone=18;config.releaseCone=65;config.bubbleSpeed=14;config.offeringDuration=2.4f;config.expansionDuration=3.2f;
  config.routeStart=new Vector3(58,4,0);config.routeEnd=new Vector3(151,17.12f,0);
  config.stages=new[]{
   new AnchorStage{name="Workshop field",radius=radius*.25f,height=radius*.2f},
   new AnchorStage{name="Garden field",radius=radius*.5f,height=radius*.4f,aether=6,wood=6,stone=4},
   new AnchorStage{name="Settlement field",radius=radius*.75f,height=radius*.6f,aether=12,wood=12,stone=8},
   new AnchorStage{name="Island field",radius=radius,height=radius*.8f,aether=18,wood=18,stone=12}
  };
  var centers=new[]{new Vector3(15,2,-9),new Vector3(12,3,6),new Vector3(-10,2,0),new Vector3(-25,2,-12),new Vector3(-30,3,5),new Vector3(22,3,15),new Vector3(-35,2,26),new Vector3(-45,3,8),new Vector3(36,3,23),new Vector3(18,3,34),new Vector3(-46,2,-14),new Vector3(-25,3,36),new Vector3(0,3,40),new Vector3(38,3,-20)};
  var points=new List<AetherPoint>();
  for(int cluster=0;cluster<centers.Length;cluster++)for(int j=0;j<4;j++){
   var p=centers[cluster]+new Vector3((j%2)*.85f,(j/2)*.7f,(j%2)*.3f);p.y+=CompactIslandAuthor.Height(p.x,p.z);
   int index=points.Count;string id=config.bubbles!=null&&index<config.bubbles.Length?config.bubbles[index].id:"aether-"+Guid.NewGuid().ToString("N");
   points.Add(new AetherPoint{id=id,position=p,size=.32f+(j%3)*.045f});
  }
  config.bubbles=points.ToArray();
  var material=AssetDatabase.LoadAssetAtPath<Material>(D+"/AetherBubble.mat");if(!material){material=new Material(Shader.Find("Skywright/AetherBubble"));AssetDatabase.CreateAsset(material,D+"/AetherBubble.mat");}config.bubble=material;
  config.ribbon=Resources.Load<AirConfig>("AirMagic/Tuning").ribbon;
  EditorUtility.SetDirty(config);AssetDatabase.SaveAssets();
  File.WriteAllText("Evidence/CompactAether/aether-catalog.json",JsonUtility.ToJson(config,true));
  Debug.Log("AETHER_AUTHORED bubbles="+points.Count+" costs=36Aether/36Wood/24Stone");
 }
 public static void AuthorAndBuild(){Author();MountainAeroAuthor.Build();}
}
}

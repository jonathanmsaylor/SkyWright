using System;using System.Collections.Generic;using UnityEngine;
namespace Skywright {
[Serializable] public class AetherPoint {public string id;public Vector3 position;public float size=.34f;}
[Serializable] public class AnchorStage {public string name;public float radius,height;public int aether,wood,stone;}
[CreateAssetMenu(menuName="Skywright/Aether progression")]
public class AetherConfig:ScriptableObject {
 public float suctionRange=8,suctionCone=18,releaseCone=65,bubbleSpeed=14,propSpeed=3,propAcceleration=9;
 public Vector3 anchorPosition=new Vector3(4,0,-17);
 public float offeringDuration=2.4f,expansionDuration=3.2f;
 public Vector3 islandCenter=new Vector3(-32,30,58),islandRadii=new Vector3(145,82,121);
 public Vector3 routeStart=new Vector3(68,17,-4),routeEnd=new Vector3(215,39,-70);
 public AnchorStage[] stages;public AetherPoint[] bubbles;public Material bubble,ribbon;
}
[Serializable] public class AetherState {
 public int version=1,balance,tier;public string primaryAnchor="";
 public List<string> collected=new List<string>();
}
[Serializable] public class AetherObservation {public AetherState state;public int inTransit;public string aimedAnchor;public List<AetherPoint> remaining=new List<AetherPoint>();}
}

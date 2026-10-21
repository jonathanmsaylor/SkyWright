using UnityEngine;
namespace Skywright {
[CreateAssetMenu(menuName="Skywright/Earthcasting tuning")]
public class EarthConfig : ScriptableObject {
 public float flightSpeed=18,steeringDegrees=150,maxFlightTime=3,maxTravelDistance=40,maxPlayerDistance=20;
 public float gravity=9.81f,recallSpeed=26,recallAcceleration=65,maxRecallTime=12,maxRecallDistance=280;
 public float stoneRadius=.31f,waitOffset=.55f,strikeAnticipation=.16f,strikeSpeed=12,strikeAcceleration=90,reboundSpeed=4;
 public int maxRicochets=3;
 public float ricochetRetention=.88f,blockedReturnTime=.8f,dismissTime=.3f;
 public float range=240,collectionRadius=3.5f,stonePickupRange=2.5f;
 public float emergeTime=.10f,suspendTime=.16f,flightTime=.22f,recoveryTime=.28f,fallTime=1.65f;
 public float summonSettleTime=.20f,earthSearchDistance=30,catchTime=.12f,catchDistance=.30f;
 public int treeHits=5,outcropHits=8;
 public int deckCost=6,treeYield=12,pileYield=18,outcropYield=12;
 public GameObject tree,outcrop,pile,rock,wood,focus,crate;
 public Material magic,fracture,bark,stone,leaf;
 public AudioClip prepare,lift,launch,impact,crack,fall,collect;
}
}
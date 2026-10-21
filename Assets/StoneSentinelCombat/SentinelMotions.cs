using UnityEngine;
namespace Skywright {
public sealed partial class SentinelEnemy {
 public float wakeRange=8,leapMinRange=8,leapMaxRange=16,leapCooldownSeconds=7;
 public bool allowLeap=true;
 public bool Awakened {get;private set;}
 public int Leaps {get;private set;}
 public Vector3 LeapTarget=>leapEnd;
 public float LeapCooldown=>leapCooldown;
 const float AwakeDuration=3.1f,LeapWindupDuration=.65f,LeapDuration=1,LeapRecoveryDuration=1.15f;
 const int WorldMask=~((1<<2)|(1<<6));
 float leapCooldown;Vector3 leapStart,leapEnd;bool leapBlocked;
 Transform[] disguise;Vector3[] disguiseScale;
 bool LeapState=>State=="LeapWindup"||State=="Leap"||State=="LeapRecover";
 void InitializeMotions(){
  disguise=new Transform[7];disguiseScale=new Vector3[7];
  for(int i=0;i<disguise.Length;i++){
   var g=new GameObject("Sentinel resting limestone");g.layer=2;
   g.AddComponent<MeshFilter>().sharedMesh=combat.LimestoneChip(i);g.AddComponent<MeshRenderer>().sharedMaterial=combat.limestoneMaterial;
   float angle=i*Mathf.PI*2/disguise.Length;
   g.transform.position=home+new Vector3(Mathf.Cos(angle)*1.05f,-.13f,Mathf.Sin(angle)*.85f);
   g.transform.rotation=Quaternion.Euler(i*21,i*67,12);g.transform.localScale=new Vector3(.85f,.6f,.7f);
   disguise[i]=g.transform;disguiseScale[i]=g.transform.localScale;
  }
 }
 void ResetMotions(bool awake){
  Awakened=awake;Leaps=0;leapCooldown=2;leapBlocked=false;
  for(int i=0;i<disguise.Length;i++)if(disguise[i]){disguise[i].gameObject.SetActive(!awake);disguise[i].localScale=disguiseScale[i];}
  SetBodyHeight(awake?2.4f:1.3f);
  Enter(awake?"Guard":"Dormant");
  if(!awake&&animationRig&&animationRig["Dormant"]!=null){animationRig.Play("Dormant");animationRig["Dormant"].time=0;animationRig.Sample();}
 }
 void SetBodyHeight(float height){body.radius=height<1.64f?.65f:.82f;body.height=height;body.center=Vector3.up*(height*.5f);}
 void Wake(){
  if(State!="Dormant")return;
  engaged=true;lostContact=0;WindCharge=windThisFrame=0;
  Enter("Awaken");combat.ImpactSound(home);combat.Game.Notify("The limestone stirs…");
 }
 bool TickArrival(float dt){
  leapCooldown=Mathf.Max(0,leapCooldown-dt);
  if(State=="Dormant"){
   var p=combat.Game.controller.transform.position;
   if(windThisFrame>0||(Vector3.Distance(p,transform.position)<=wakeRange&&SentinelCombat.ClearLine(transform.position+Vector3.up*.7f,p+Vector3.up*1.35f,transform)))Wake();
   windThisFrame=0;return true;
  }
  if(State!="Awaken")return false;
  windThisFrame=WindCharge=0;
  float t=Mathf.Clamp01(StateTime/AwakeDuration);SetBodyHeight(Mathf.Lerp(1.3f,2.4f,t));
  for(int i=0;i<disguise.Length;i++)if(disguise[i]){float s=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(StateTime/1.1f));disguise[i].localScale=disguiseScale[i]*s;disguise[i].gameObject.SetActive(s>.001f);}
  if(StateTime>=AwakeDuration){Awakened=true;SetBodyHeight(2.4f);leapCooldown=2;Enter("Guard");}
  return true;
 }
 bool GroundAt(Vector3 point,out Vector3 ground){
  ground=point;float closest=float.PositiveInfinity;
  foreach(var hit in Physics.RaycastAll(point+Vector3.up*3,Vector3.down,6,WorldMask,QueryTriggerInteraction.Ignore)){
   if(hit.transform==transform||hit.transform.IsChildOf(transform)||hit.normal.y<.75f)continue;
   if(hit.distance<closest){closest=hit.distance;ground=hit.point+Vector3.up*.04f;}
  }
  return closest<float.PositiveInfinity;
 }
 internal bool PlanLeap(Vector3 player){
  var delta=player-transform.position;delta.y=0;float distance=delta.magnitude;
  if(!allowLeap||leapCooldown>0||distance<leapMinRange||distance>leapMaxRange||Mathf.Abs(player.y-transform.position.y)>2)return false;
  var candidate=player-delta.normalized*1.3f;
  if(!GroundAt(candidate,out var landing)||Mathf.Abs(landing.y-transform.position.y)>1.5f||Vector3.Distance(landing,home)>homeLeash-1)return false;
  // A supported footprint keeps the leap away from cliffs and narrow ledges.
  foreach(var offset in new[]{Vector3.right*.7f,Vector3.left*.7f,Vector3.forward*.7f,Vector3.back*.7f})
   if(!GroundAt(landing+offset,out var support)||Mathf.Abs(support.y-landing.y)>.3f)return false;
  foreach(var c in Physics.OverlapCapsule(landing+Vector3.up*.84f,landing+Vector3.up*1.56f,.78f,WorldMask,QueryTriggerInteraction.Ignore))
   if(c.transform!=transform&&!c.transform.IsChildOf(transform))return false;
  var previous=transform.position;
  for(int i=1;i<=16;i++){
   var next=LeapPoint(transform.position,landing,i/16f);
   var step=next-previous;
   foreach(var hit in Physics.CapsuleCastAll(previous+Vector3.up*.84f,previous+Vector3.up*1.56f,.76f,step.normalized,step.magnitude,WorldMask,QueryTriggerInteraction.Ignore))
    if(hit.transform!=transform&&!hit.transform.IsChildOf(transform))return false;
   previous=next;
  }
  leapStart=transform.position;leapEnd=landing;leapBlocked=false;leapCooldown=leapCooldownSeconds;
  transform.rotation=Quaternion.LookRotation(delta);slamCenter=landing;Enter("LeapWindup");return true;
 }
 static Vector3 LeapPoint(Vector3 start,Vector3 end,float t)=>Vector3.Lerp(start,end,t)+Vector3.up*(4*2.3f*t*(1-t));
 void TickLeap(float dt){
  if(State=="LeapWindup"){
   DrawWarning();if(StateTime>=LeapWindupDuration){Leaps++;Enter("Leap");}return;
  }
  if(State=="Leap"){
   DrawWarning();
   var destination=LeapPoint(leapStart,leapEnd,Mathf.Clamp01(StateTime/LeapDuration));
   body.Move(destination-transform.position);
   if(Vector3.Distance(transform.position,destination)>.24f){leapBlocked=true;Enter("LeapRecover");return;}
   if(StateTime>=LeapDuration){
    if(!leapBlocked)combat.Slam(leapEnd,2.5f);
    if(!combat.PlayerDown)Enter("LeapRecover");
   }
  }else if(State=="LeapRecover"){
   if(leapBlocked)body.Move(Vector3.down*12*dt);
   if(StateTime>=LeapRecoveryDuration&&(!leapBlocked||body.isGrounded))Enter("Guard");
  }
 }
 void DisposeMotions(){if(disguise!=null)foreach(var p in disguise)if(p)Destroy(p.gameObject);}
}
}

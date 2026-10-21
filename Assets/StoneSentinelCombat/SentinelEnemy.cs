using UnityEngine;
namespace Skywright {
public sealed partial class SentinelEnemy : MonoBehaviour {
 public const int MaxHealth=24;
 public int Health {get;private set;}=MaxHealth;
 public bool Alive=>Health>0;
 public string State {get;private set;}="Guard";
 public float StateTime {get;private set;}
 public float WindCharge {get;private set;}
 public float WindResistance=>windCooldown;
 public int Swats {get;private set;}
 public int HitsSinceSwat=>hitsSinceSwat;
 public int hitsBeforeSwat=3;
 public float noticeRange=12,pursuitRange=20,homeLeash=22,forgetAfter=3;
 public bool Aggroed=>engaged;
 public SentinelArmor Armor {get;private set;}
 public Collider HitCollider=>body;
 public bool Exposed=>Alive&&(State=="Stagger"||State=="Recover"||State=="LeapRecover");
 public string Status=>State=="Dormant"?"Quiet limestone...":State=="Awaken"?"The Sentinel awakens":LeapState?"LEAP - leave the landing circle":!Alive?"Quieted":State=="Return"?"Returning to the clearing":State=="Swat"||swatPending?"STONE SWAT — prepare to cast again":State=="Windup"?"SLAM INCOMING — move or stagger":Exposed?"Core exposed • double Earthstone damage":windCooldown>0?"Wind resistance: "+windCooldown.ToString("F1")+" s":"Wind ready • hold pull or push to stagger";
 SentinelCombat combat;Vector3 home,slamCenter;CharacterController body;Animation animationRig;
 LineRenderer ring;Material ringMaterial;Renderer[] renderers;MaterialPropertyBlock properties;
 float windCooldown,hitCooldown,flash,windThisFrame;bool ceased,swatPending,swatFired;int hitsSinceSwat,comboHit;
 static readonly float[] ComboBeats={.25f,1.25f,2.25f};
 bool engaged;float lostContact;
 public void Initialize(SentinelCombat owner,Vector3 origin){
  combat=owner;home=origin;body=gameObject.AddComponent<CharacterController>();body.height=2.4f;body.radius=.82f;body.center=Vector3.up*1.2f;body.skinWidth=.04f;body.stepOffset=.22f;body.slopeLimit=35;
  animationRig=GetComponentInChildren<Animation>();renderers=GetComponentsInChildren<Renderer>();properties=new MaterialPropertyBlock();
  var marker=new GameObject("Slam warning");marker.transform.SetParent(transform,false);ring=marker.AddComponent<LineRenderer>();
  ringMaterial=new Material(combat.warningMaterial);ring.sharedMaterial=ringMaterial;
  ring.positionCount=65;ring.useWorldSpace=true;ring.loop=false;ring.widthMultiplier=.045f;ring.enabled=false;ring.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
  Armor=gameObject.AddComponent<SentinelArmor>();Armor.Initialize(combat);
  InitializeMotions();ResetEncounter();
 }
 public void ResetEncounter(bool awake=false){
  body.enabled=false;transform.SetPositionAndRotation(home,Quaternion.Euler(0,180,0));body.enabled=true;
  Health=MaxHealth;windCooldown=0;hitCooldown=0;WindCharge=0;ceased=false;engaged=false;lostContact=0;flash=0;
  hitsSinceSwat=Swats=0;swatPending=swatFired=false;windThisFrame=0;Armor.ResetArmor();ResetMotions(awake);
  foreach(var r in renderers){properties.SetColor("_EmissionColor",Color.white*(r.name.StartsWith("Core_")?.18f:.8f)*(Awakened?1:State=="Awaken"?Mathf.Clamp01(StateTime/3.1f):0));r.SetPropertyBlock(properties);}
 }
 public void SetPaused(bool paused){if(paused)windThisFrame=0;if(animationRig)foreach(AnimationState s in animationRig)s.speed=paused?0:1;}
 public void Cease(){ceased=true;engaged=false;ring.enabled=false;Enter("Guard");}
 public void Wind(float dt){windThisFrame+=dt;}
 public void EarthImpact(Vector3 point){
  if(!Alive||hitCooldown>0||combat.Paused||swatPending||State=="Swat")return;
  if(State=="Dormant")Wake();
  int amount=Exposed?2:1;Health=Mathf.Max(0,Health-amount);hitCooldown=.12f;flash=.18f;
  if(State!="Return"&&!LeapState){engaged=true;lostContact=0;}
  Debug.Log("COMBAT EARTH_HIT damage="+amount+" health="+Health+" state="+State);
  Armor.SetDamage(Health);
  if(!Alive){swatPending=false;body.enabled=false;Enter("Defeat");combat.Game.Notify("Sentinel quieted");}
  else if(combat.Game.CombatStoneBoundTo(this)&&++hitsSinceSwat>=hitsBeforeSwat)swatPending=true;
 }
 public void Tick(float dt){
  StateTime+=dt;hitCooldown=Mathf.Max(0,hitCooldown-dt);windCooldown=Mathf.Max(0,windCooldown-dt);flash=Mathf.Max(0,flash-dt);
  Armor.Tick(dt);
  foreach(var r in renderers){var tint=Color.white*(r.name.StartsWith("Core_")?.18f:.8f);properties.SetColor("_EmissionColor",Alive?tint*(Exposed?2.5f:1)*(Awakened?1:State=="Awaken"?Mathf.Clamp01(StateTime/3.1f):0)+tint*flash*4:Color.black);r.SetPropertyBlock(properties);}
  if(!Alive){if(StateTime>=.4f)Armor.BreakApart();windThisFrame=0;return;}
  if(ceased){windThisFrame=0;return;}
  if(TickArrival(dt))return;
  if(windCooldown<=0&&State!="Stagger"&&State!="Leap"){
   WindCharge=windThisFrame>0?WindCharge+windThisFrame:Mathf.MoveTowards(WindCharge,0,dt*2);
   if(WindCharge>=(State=="Windup"||State=="LeapWindup"?.25f:.4f)){WindCharge=0;windCooldown=5;engaged=true;lostContact=0;Enter("Stagger");combat.ImpactSound(transform.position);Debug.Log("COMBAT WIND_STAGGER");}
  }else WindCharge=0;
  windThisFrame=0;
  if(!combat.Game.CombatStoneBoundTo(this)&&State!="Swat"){hitsSinceSwat=0;swatPending=false;}
  if(swatPending&&State!="Swat"&&State!="Stagger"&&State!="Slam"&&State!="Recover"&&!LeapState)Enter("Swat");
  Vector3 player=combat.Game.controller.transform.position;Vector3 delta=player-transform.position;delta.y=0;
  float distance=Vector3.Distance(player,transform.position);
  bool visible=SentinelCombat.ClearLine(transform.position+Vector3.up*1.35f,player+Vector3.up*1.35f,transform);
  if(State!="Return"&&!LeapState){
   if(!engaged&&distance<=noticeRange&&visible){engaged=true;lostContact=0;Debug.Log("COMBAT AGGRO acquired");}
   if(engaged){
    lostContact=distance>pursuitRange||!visible?lostContact+dt:0;
    if(Vector3.Distance(transform.position,home)>homeLeash||lostContact>=forgetAfter){engaged=false;lostContact=0;swatPending=false;Enter("Return");Debug.Log("COMBAT AGGRO returning");}
   }
  }
  if(State=="Guard"||State=="Walk"){
   if(engaged&&delta.magnitude<3.6f&&Mathf.Abs(player.y-transform.position.y)<2&&visible){Face(delta,dt);slamCenter=transform.position+transform.forward*1.65f;Enter("Windup");}
   else if(engaged&&visible&&PlanLeap(player)){}
   else if(engaged){Face(delta,dt);Move(delta.normalized*1.65f,dt);if(State!="Walk")Enter("Walk");}
   else if(State!="Guard")Enter("Guard");
  }else if(State=="Return"){
   var toHome=home-transform.position;toHome.y=0;Face(toHome,dt);Move(toHome.normalized*1.65f,dt);if(toHome.magnitude<.25f)Enter("Guard");
  }else if(State=="Windup"){
   if(StateTime<.35f){Face(delta,dt);slamCenter=transform.position+transform.forward*1.65f;}
   DrawWarning();if(StateTime>=.9f)Enter("Slam");
  }else if(State=="Slam"){
   DrawWarning();
   while(comboHit<ComboBeats.Length&&StateTime>=ComboBeats[comboHit]&&!combat.PlayerDown){combat.Slam(slamCenter,2.35f,comboHit==2?9:8);comboHit++;}
   if(StateTime>=2.65f&&!combat.PlayerDown)Enter("Recover");
  }else if(LeapState){TickLeap(dt);
  }else if(State=="Swat"){
   if(!swatFired&&StateTime>=.48f){swatFired=true;if(combat.Game.DeflectCombatStone(this))Swats++;swatPending=false;hitsSinceSwat=0;}
   if(StateTime>=.9f)Enter("Guard");
  }else if(State=="Recover"&&StateTime>=.8f||State=="Stagger"&&StateTime>=2)Enter("Guard");
  if(body.enabled&&State!="Leap")body.Move(Vector3.down*3*dt);
 }
 void Move(Vector3 velocity,float dt){
  // Do not chase over the tutorial island's edge; CharacterController handles props and slopes.
  var next=transform.position+velocity*dt+velocity.normalized*.8f+Vector3.up*.5f;
  if(Physics.Raycast(next,Vector3.down,1.5f,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore))body.Move(velocity*dt);
 }
 void Face(Vector3 direction,float dt){if(direction.sqrMagnitude>.001f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(direction),95*dt);}
 void Enter(string state){State=state;StateTime=0;ring.enabled=state=="Windup"||state=="Slam"||state=="LeapWindup"||state=="Leap";string clip=state=="Guard"?"Idle":state=="Return"?"Walk":state;
  if(state=="Slam")comboHit=0;
  if(state=="Swat"){swatFired=false;combat.ImpactSound(transform.position+Vector3.up);}
  if(animationRig&&animationRig[clip]!=null){animationRig[clip].time=0;animationRig.CrossFade(clip,state=="Slam"?.045f:.15f);}
  Debug.Log("COMBAT STATE "+state);
 }
 void DrawWarning(){
  ring.widthMultiplier=.045f+Mathf.Clamp01(StateTime/.9f)*.055f;
  for(int i=0;i<65;i++){float a=i*Mathf.PI*2/64;Vector3 p=slamCenter+new Vector3(Mathf.Cos(a)*(LeapState?2.5f:2.35f),.08f,Mathf.Sin(a)*(LeapState?2.5f:2.35f));ring.SetPosition(i,p);}
 }
 void OnDestroy(){DisposeMotions();if(ringMaterial)Destroy(ringMaterial);}
}
}

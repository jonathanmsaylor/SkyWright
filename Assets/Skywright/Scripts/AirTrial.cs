using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Skywright {
[Serializable] public class AirCrateState {public string id;public int damage;public bool depleted,collected;public Vector3 position,safe;public Quaternion rotation=Quaternion.identity;}
[Serializable] public class AirState {public int version=3;public AetherState aether=new AetherState();public bool liftUsed,descending;public string hoverAnchor="";public List<AirCrateState> crates=new List<AirCrateState>();}
public class AirTrial {
 public readonly AirConfig config;public readonly AetherSystem aether;readonly AirGesture gesture=new AirGesture();bool pullActive;AirCrate pulled;Vector3 pullVelocity;public bool equipped;public string hint="Q Earth / Air • Tab Equip magic",target="",motion="Grounded";
 public readonly List<AirCrate> crates=new List<AirCrate>();public AirCrate held;public bool hovering,descending,liftUsed;public string anchorId="";
 public bool Pulling=>pullActive;public bool Pushing=>windActive&&!pullActive;public AirGesture.Mode Gesture=>gesture.Current;internal AirEffects Presentation=>fx;public bool Travelling{get;private set;}public float Rush{get;private set;}float routeT,travelSpeed,travelCooldown;bool routeReverse,routeArmed;Vector3 travelExpected;readonly SuctionShimmer shimmer=new SuctionShimmer();
 public float WindGain=>fx.WindGain;public int WindSample=>fx.WindSample;
 public float holdDistance=3,holdHeight;public float gustClock=-1;
 readonly SkywrightGame game;readonly Action dirty;readonly Action<string> log;readonly AirEffects fx;
 Transform root;Vector3 carryVelocity,moveVelocity;float liftRemaining,liftCeiling,clock;string cancelledAnchor="";bool validateCrates,restoreFlight,windActive;int lastWindCount=-1;readonly AnchorBubbles bubbles;
 Vector3 gustOrigin,gustDirection;
 public static readonly Vector3[] Homes={new Vector3(8,.04f,-12),new Vector3(10,.04f,-12)};
 public AirTrial(SkywrightGame game,Action dirty,Action<string> log){this.game=game;this.dirty=dirty;this.log=log;
  config=Resources.Load<AirConfig>("AirMagic/Tuning");if(!config)throw new Exception("Air assets missing");fx=new AirEffects(config,game.cam);bubbles=new AnchorBubbles(config);if(NaturalPreview.Active)aether=new AetherSystem(game,dirty,log);Reset();
 }
 public static string Id(int i)=>"air-trial-crate-"+i;
 public static AirState NewState(){var s=new AirState();for(int i=0;i<2;i++)s.crates.Add(new AirCrateState{id=Id(i),position=Homes[i],safe=Homes[i],rotation=Quaternion.Euler(0,15,0)});return s;}
 static bool Finite(Vector3 p)=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.sqrMagnitude<1000000;
 public static void Validate(AirState s){
  if(s==null||(s.version<1||s.version>3)||s.crates==null||s.crates.Count!=2)throw new IOException("Invalid air trial state");
  if(s.version>=2&&NaturalPreview.Active)AetherSystem.Validate(s.aether);
  var ids=new HashSet<string>();foreach(var c in s.crates){if(c!=null&&(c.damage<0||c.damage>3||c.depleted!=(c.damage==3)||c.collected&&!c.depleted))throw new IOException("Invalid crate depletion");if(c==null||(c.id!=Id(0)&&c.id!=Id(1))||!ids.Add(c.id)||!Finite(c.position)||!Finite(c.safe)||!float.IsFinite(c.rotation.x)||!float.IsFinite(c.rotation.y)||!float.IsFinite(c.rotation.z)||!float.IsFinite(c.rotation.w)||Mathf.Abs(Quaternion.Dot(c.rotation,c.rotation)-1)>.02f||Vector3.Dot(c.rotation*Vector3.up,Vector3.up)<.99f)throw new IOException("Invalid crate identity or transform");}
 }
 public void Reset()=>Restore(NewState());
 public void Restore(AirState s){Validate(s);if(aether!=null)aether.Restore(s.version==1?new AetherState():s.aether);held=null;Travelling=false;routeArmed=false;Rush=0;fx.Clear();if(root){root.gameObject.SetActive(false);UnityEngine.Object.Destroy(root.gameObject);}crates.Clear();root=new GameObject("Air trial props").transform;
  for(int i=0;i<2;i++){var r=s.crates.Find(c=>c.id==Id(i));var c=UnityEngine.Object.Instantiate(config.crate,r.position,r.rotation,root).GetComponent<AirCrate>();c.id=r.id;c.home=Homes[i];c.safe=r.safe;if(TutorialIsland.Active){var harvest=c.gameObject.AddComponent<CargoHarvest>();harvest.Restore(JsonUtility.FromJson<AirCrateState>(JsonUtility.ToJson(r)),game.EarthTuning);}crates.Add(c);}
  validateCrates=true;cancelledAnchor="";restoreFlight=false;hovering=false;restoreFlight=!string.IsNullOrEmpty(s.hoverAnchor);anchorId=TutorialIsland.Active&&!string.IsNullOrEmpty(s.hoverAnchor)?TutorialIsland.AnchorId:s.hoverAnchor??"";descending=s.descending||anchorId!="";liftUsed=s.liftUsed;liftRemaining=0;moveVelocity=Vector3.zero;gustClock=-1;Equip(false);Physics.SyncTransforms();
 }
 public void Equip(bool value){if(!value){Release();StopWind();gesture.Cancel();fx.Cancel();}else if(Mouse.current!=null&&!Mouse.current.leftButton.isPressed&&!Mouse.current.rightButton.isPressed)gesture.Tick(false,false,false,false,false,true,false);equipped=value;fx.Focus(value);}
 public void Pause(){Release();gesture.Cancel();shimmer.Clear();game.InterruptDrops();foreach(var c in crates)if(c)c.GetComponent<CargoHarvest>()?.Interrupt();Rush=Mathf.MoveTowards(Rush,0,Time.unscaledDeltaTime*4);fx.Rush(0);aether?.Interrupt();StopWind();RefreshBubbles();moveVelocity=Vector3.zero;carryVelocity=Vector3.zero;fx.Focus(false);fx.Pause();}
 public void RecoverPlayer(){Travelling=false;travelCooldown=2;Rush=0;fx.Rush(0);cancelledAnchor="";restoreFlight=false;if(held)Release();hovering=false;descending=false;anchorId="";liftRemaining=0;moveVelocity=Vector3.zero;liftUsed=true;}
 public void RecoverCrates(){if(held)Release();foreach(var c in crates)if(c&&!c.furniture&&!Broken(c))Recover(c);game.Notify("Crates returned to clear ground");}
 void Recover(AirCrate c){
  Vector3 chosen=c.safe;bool found=SafeGround(c,chosen);
  if(!found){for(int z=-16;z<=-8&&!found;z+=2)for(int x=6;x<=14&&!found;x+=2){var p=new Vector3(x,.04f,z);if(SafeGround(c,p)){chosen=p;found=true;}}}
  if(!found){c.velocity=Vector3.zero;game.Notify("Clear a space on the ground, then recover crates");return;}
  c.transform.position=chosen;c.velocity=Vector3.zero;c.motionTime=0;Physics.SyncTransforms();dirty();log("CRATE_RECOVER "+c.id+" "+chosen);
 }
 bool SafeGround(AirCrate c,Vector3 at){
  if(c.Occupied(at)||at.y<0||new Vector2(at.x,at.z).magnitude>180)return false;
  foreach(var h in Physics.RaycastAll(at+Vector3.up*.1f,Vector3.down,.3f,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore))if(!h.collider.transform.IsChildOf(c.transform)&&h.normal.y>.65f)return true;
  return false;
 }
 bool StandingOn(AirCrate c)=>Physics.Raycast(game.controller.transform.position+Vector3.up*.1f,Vector3.down,out var h,.4f,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore)&&h.collider.GetComponentInParent<AirCrate>()==c;
 public void RefreshBubbles()=>bubbles.Refresh(game.builder);
 void SyncFurniture(){
  crates.RemoveAll(c=>!c||!c.gameObject.activeInHierarchy);
  foreach(var b in game.builder.pieces){if(!b||!AirCrate.Eligible(b.type))continue;var c=b.GetComponent<AirCrate>();if(!c){c=b.gameObject.AddComponent<AirCrate>();c.Configure(b);}if(!crates.Contains(c))crates.Add(c);}
 }
 void StopWind(){aether?.Interrupt();pulled=null;pullVelocity=Vector3.zero;pullActive=false;if(windActive)log("WIND_STOP");windActive=false;gustClock=-1;fx.Wind(false);}
 public void Tick(float dt){
  SyncFurniture();shimmer.Clear();
  if(validateCrates){validateCrates=false;Physics.SyncTransforms();foreach(var c in crates)if(!c.furniture&&!Broken(c)&&c.Occupied(c.transform.position))Recover(c);}
  clock+=dt;var k=Keyboard.current;var m=Mouse.current;target="";
  fx.Focus(equipped);hint=held?"Shift+LMB Release • Wheel Distance • Shift+wheel Height":"Hold LMB Suction • Hold RMB Push • Shift+LMB Grab";
  var ray=game.cam.ViewportPointToRay(new Vector3(.5f,.5f,0));
  AirCrate aimed=null;if(Physics.Raycast(ray,out var h,config.grabRange,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore))aimed=h.collider.GetComponentInParent<AirCrate>();
  if(Broken(aimed))aimed=null;if(aimed)target=aimed.id;
  bool shift=k.leftShiftKey.isPressed||k.rightShiftKey.isPressed;
  gesture.Tick(m.leftButton.isPressed,m.rightButton.isPressed,m.leftButton.wasPressedThisFrame,m.rightButton.wasPressedThisFrame,shift,equipped,held!=null);
  if(equipped){
   if(gesture.Toggle){if(held)Release();else if(aimed){if(StandingOn(aimed))game.Notify("Step off the object before lifting it");else {held=aimed;held.simulating=true;held.velocity=Vector3.zero;holdDistance=Mathf.Clamp(Vector3.Distance(game.cam.transform.position,held.Center),1.8f,5);holdHeight=.45f;carryVelocity=Vector3.zero;fx.Cue(config.gather,held.Center,.5f);dirty();log("AIR_GRAB "+held.id);}}else game.Notify("Aim at nearby furniture or a crate");}
   if(held){float scroll=m.scroll.ReadValue().y;if(Mathf.Abs(scroll)>.001f){int steps=Mathf.Abs(scroll)>=120?Mathf.RoundToInt(scroll/120):(int)Mathf.Sign(scroll);steps=Mathf.Clamp(steps,-8,8);if(k.leftShiftKey.isPressed||k.rightShiftKey.isPressed)holdHeight=Mathf.Clamp(holdHeight+steps*.25f,-1,2);else holdDistance=Mathf.Clamp(holdDistance+steps*.25f,1.8f,5);log("AIR_ADJUST distance="+holdDistance+" height="+holdHeight);}}
  }
  bool wasPulling=pullActive;pullActive=equipped&&!held&&gesture.Current==AirGesture.Mode.Pull;
  if(wasPulling&&!pullActive){aether?.Interrupt();game.InterruptDrops();}
  bool wantsPush=equipped&&!held&&gesture.Current==AirGesture.Mode.Push;
  bool wantsWind=pullActive||wantsPush;
  if(wantsWind&&!windActive){windActive=true;gustClock=0;lastWindCount=-1;log("WIND_BEGIN");}
  if(!wantsWind&&windActive)StopWind();
  gustOrigin=game.cam.transform.position;gustDirection=game.cam.transform.forward;
  fx.BeginFlow();
  if(windActive){gustClock+=dt;if(pullActive)Suction(dt);else {pulled=null;pullVelocity=Vector3.zero;Gust(dt);}}
  if(SentinelCombat.Active)SentinelCombat.Current.ApplyWind(wantsWind,gustOrigin,gustDirection,dt);
  aether?.Tick(dt,pullActive);if(pullActive)aether?.TraceWind(fx);fx.Pulling=pullActive;
  fx.FlowRange=pullActive?(aether!=null?aether.config.suctionRange:8):Mathf.Max(config.gustRange,SentinelCombat.Active?8:0);
  foreach(var c in crates){if(equipped&&PullEligible(c,false))shimmer.Show(c.transform,1);c.GetComponent<CargoHarvest>()?.Tick(game,dt);}
  if(Broken(held))held=null;if(Broken(pulled))pulled=null;
  fx.Wind(windActive);
  if(held){
   if(StandingOn(held)||Vector3.Distance(game.cam.transform.position,held.Center)>config.grabRange+1){Release();}
   else{Vector3 desired=game.cam.transform.position+game.cam.transform.forward*holdDistance+Vector3.up*(holdHeight-.5f);
    Vector3 desiredVelocity=Vector3.ClampMagnitude((desired-held.Center)*4,config.carrySpeed);carryVelocity=Vector3.MoveTowards(carryVelocity,desiredVelocity,dt*12);
    held.Sweep(carryVelocity*dt);held.motionTime+=dt;dirty();}
  }
  foreach(var c in crates){if(Broken(c)||c==held||c==pulled||!c.simulating)continue;
   bool ground=c.Grounded();if(ground&&c.velocity.y<=0){if(!c.resting&&c.velocity.y<-.5f)fx.Cue(config.settle,c.Center,.35f);c.velocity.y=0;c.velocity.x=Mathf.MoveTowards(c.velocity.x,0,dt*3);c.velocity.z=Mathf.MoveTowards(c.velocity.z,0,dt*3);if(c.velocity.sqrMagnitude<.01f&&new Vector2(c.transform.position.x,c.transform.position.z).magnitude<180)c.safe=c.transform.position;}
   else c.velocity.y=Mathf.Max(c.velocity.y-12*dt,-8);
   // Separate horizontal/downward sweeps allow sliding over support without penetration.
   Vector3 step=c.Sweep(new Vector3(c.velocity.x,0,c.velocity.z)*dt);string blocker=c.blocked;
   step+=c.Sweep(Vector3.up*c.velocity.y*dt);if(blocker!="")c.blocked=blocker;
   if(step.sqrMagnitude>0.000001f)dirty();
   c.resting=ground;
   if(c.transform.position.y< -15||new Vector2(c.transform.position.x,c.transform.position.z).magnitude>220)Recover(c);
  }
  fx.Tick(dt,held,hovering||descending,game.controller.transform.position,hovering?Anchor():null,gustClock,gustOrigin,gustDirection);
 }
 bool PullEligible(AirCrate c,bool retained){
  if(!c||Broken(c)||c==held||StandingOn(c))return false;var delta=c.Center-game.cam.transform.position;float range=aether!=null?aether.config.suctionRange:8;
  if(delta.magnitude>range+(retained?1:0)||Vector3.Angle(game.cam.transform.forward,delta)>(retained?30:18))return false;
  return !Physics.Linecast(game.cam.transform.position,c.Center,out var h,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore)||h.collider.GetComponentInParent<AirCrate>()==c;
 }
 void Suction(float dt){
  if(!PullEligible(pulled,true)){pulled=null;pullVelocity=Vector3.zero;float best=float.MaxValue;foreach(var c in crates)if(PullEligible(c,false)){var d=c.Center-game.cam.transform.position;float score=Vector3.Angle(game.cam.transform.forward,d)+d.magnitude*.08f;if(score<best){best=score;pulled=c;}}}
  if(!pulled)return;
  float brake=2.2f+pulled.Half.magnitude;var desired=game.cam.transform.position+game.cam.transform.forward*brake;
  float weight=Mathf.Max(1,pulled.Half.magnitude);float speed=(aether!=null?aether.config.propSpeed:3)/weight;
  var targetVelocity=Vector3.ClampMagnitude((desired-pulled.Center)*3,speed);
  pullVelocity=Vector3.MoveTowards(pullVelocity,targetVelocity,dt*(aether!=null?aether.config.propAcceleration:9));
  pulled.simulating=true;pulled.velocity=Vector3.zero;pulled.Sweep(pullVelocity*dt);fx.Affected(pulled.Center,pulled.Half.magnitude);dirty();
 }
 void Gust(float dt){
  int count=0;foreach(var c in crates){if(Broken(c)||c==held||StandingOn(c))continue;Vector3 delta=c.Center-gustOrigin;float along=Vector3.Dot(delta,gustDirection);
   if(along<0||along>config.gustRange||Vector3.Distance(c.Center,gustOrigin+gustDirection*along)>.5f+along*.10f+c.Half.magnitude*.4f)continue;
   if(Physics.Linecast(gustOrigin,c.Center,out var h,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore)&&h.collider.GetComponentInParent<AirCrate>()!=c)continue;
   Vector3 direction=Vector3.ProjectOnPlane(gustDirection,Vector3.up).normalized;
   Vector3 horizontal=Vector3.MoveTowards(new Vector3(c.velocity.x,0,c.velocity.z),direction*config.gustSpeed,config.gustAcceleration*dt);
   c.velocity=new Vector3(horizontal.x,c.velocity.y,horizontal.z);c.simulating=true;fx.Affected(c.Center,c.Half.magnitude);dirty();count++;
  }
  if(count!=lastWindCount){log("WIND_TARGETS count="+count+" heldExcluded="+(held!=null));lastWindCount=count;}
 }
 public void Release(){if(!held)return;log("AIR_RELEASE "+held.id+" position="+held.transform.position);held.velocity=Vector3.zero;held.resting=false;fx.Cue(config.settle,held.Center,.2f);held=null;carryVelocity=Vector3.zero;dirty();}
 static bool Broken(AirCrate c)=>c&&c.GetComponent<CargoHarvest>()&&c.GetComponent<CargoHarvest>().state.depleted;
 BuiltPiece Anchor(){
  if(aether!=null){var fixedAnchor=aether.Primary();return fixedAnchor&&AnchorVolume.Contains(config,fixedAnchor.transform.position,game.controller.transform.position)?fixedAnchor:null;}
  var p=game.controller.transform.position;BuiltPiece best=null;float distance=float.MaxValue;
  foreach(var b in game.builder.pieces){if(!b||b.type!="wind-anchor"||aether!=null&&!aether.IsPrimary(b.id))continue;Vector3 d=p-b.transform.position;float horizontal=new Vector2(d.x,d.z).magnitude;
   if(!AnchorVolume.Contains(config,b.transform.position,p))continue;
   if(b.id==anchorId)return b;if(horizontal<distance){best=b;distance=horizontal;}
  }return best;
 }
 public bool Move(Vector2 input,float dt,out Vector3 velocity){
  var k=Keyboard.current;var p=game.controller.transform.position;bool ground=game.controller.isGrounded;
  travelCooldown=Mathf.Max(0,travelCooldown-dt);
  if(aether!=null&&Vector3.Distance(p,aether.config.routeStart)>3.3f&&Vector3.Distance(p,aether.config.routeEnd)>3.3f)routeArmed=true;
  if(aether!=null&&aether.TravelReady&&!Travelling&&routeArmed&&travelCooldown<=0){
   bool start=Vector3.Distance(p,aether.config.routeStart)<2.7f,end=Vector3.Distance(p,aether.config.routeEnd)<2.7f;
   if(start||end){Travelling=true;routeReverse=end;routeT=end?1:0;travelSpeed=0;travelExpected=p;hovering=false;descending=false;Release();game.StowForJourney();log("CURRENT_ENTER "+(end?"return":"outbound"));}
  }
  if(Travelling){
   if(Vector3.Distance(p,travelExpected)>1.2f){Travelling=false;game.RecoverToHome();velocity=Vector3.zero;return true;}
   float direction=routeReverse?-1:1;float remaining=Vector3.Distance(p,routeReverse?aether.config.routeStart:aether.config.routeEnd);
   float goal=Mathf.Min(14,Mathf.Sqrt(Mathf.Max(1,remaining)*24));travelSpeed=Mathf.MoveTowards(travelSpeed,goal,dt*18);
   float lo=Mathf.Clamp01(routeT-.001f),hi=Mathf.Clamp01(routeT+.001f);
   float tangent=Vector3.Distance(TutorialIsland.RoutePoint(aether.config,lo),TutorialIsland.RoutePoint(aether.config,hi))/Mathf.Max(.0001f,hi-lo);
   routeT=Mathf.Clamp01(routeT+direction*travelSpeed*dt/tangent);var next=TutorialIsland.RoutePoint(aether.config,routeT);
   // Smoothly converge on the path on entry; collision remains owned by CharacterController.
   next=Vector3.MoveTowards(p,next,Mathf.Max(travelSpeed,3)*dt);travelExpected=next;velocity=(next-p)/Mathf.Max(.001f,dt);
   Rush=Mathf.MoveTowards(Rush,1,dt*2);fx.Rush(Rush);motion="Wind current • arriving at "+(routeReverse?"home":"neighbor");
   if((routeReverse?routeT<=0:routeT>=1)&&Vector3.Distance(next,routeReverse?aether.config.routeStart:aether.config.routeEnd)<.1f){Travelling=false;routeArmed=false;travelCooldown=4;descending=true;liftUsed=true;moveVelocity=Vector3.zero;log("CURRENT_ARRIVED "+(routeReverse?"home":"neighbor"));game.Notify("Arrived • movement restored");}
   return true;
  }
  bool stableGround=ground&&Physics.Raycast(p+Vector3.up*.15f,Vector3.down,out var groundHit,.5f,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore)&&groundHit.normal.y>.65f&&!groundHit.collider.GetComponentInParent<AirCrate>();
  if(stableGround&&!hovering&&liftRemaining<=0){liftUsed=false;descending=false;restoreFlight=false;}
  var anchor=Anchor();if(hovering&&anchor&&anchor.id!=anchorId){anchorId=anchor.id;dirty();log("HOVER_SUPPORT_TRANSFER "+anchorId);}if(anchor==null||anchor.id!=cancelledAnchor)cancelledAnchor="";
  if(hovering&&!anchor){hovering=false;anchorId="";descending=true;restoreFlight=false;liftRemaining=0;log("HOVER_SUPPORT_LOST");}
  // Only a saved explicit flight may restore support; crossing a field never captures a glide.
  if(restoreFlight){restoreFlight=false;if(anchor&&anchor.id==anchorId&&!stableGround){hovering=true;descending=false;liftRemaining=0;}else anchorId="";}
  bool doubleJump=k.spaceKey.wasPressedThisFrame&&!ground&&!hovering;
  if(k.rKey.wasPressedThisFrame||doubleJump){
   if(doubleJump)log("DOUBLE_JUMP");
   if(!hovering)moveVelocity=game.controller.velocity;
   if(hovering){cancelledAnchor=anchorId;hovering=false;descending=true;restoreFlight=false;liftRemaining=0;anchorId="";log("HOVER_CANCEL");}
   else if(anchor&&!doubleJump){cancelledAnchor="";hovering=true;descending=false;restoreFlight=false;anchorId=anchor.id;
    if(!liftUsed){liftRemaining=.85f;liftCeiling=p.y+config.liftHeight;}liftUsed=true;
    fx.Cue(config.hover,p,.45f);log("HOVER_BEGIN "+anchorId);}
   else if(!liftUsed){liftRemaining=.85f;liftCeiling=p.y+config.liftHeight;liftUsed=true;descending=true;restoreFlight=false;fx.Cue(config.gather,p,.45f);log("PERSONAL_LIFT");}
   dirty();
  }
  bool active=hovering||descending||liftRemaining>0;velocity=Vector3.zero;
  if(active){
   bool down=k.leftCtrlKey.isPressed||k.rightCtrlKey.isPressed;
   float up=hovering?((k.spaceKey.isPressed?1:0)-(down?1:0))*2.8f:-config.descentSpeed;
   if(liftRemaining>0){liftRemaining=Mathf.Max(0,liftRemaining-dt);up=Mathf.Min(2.7f,Mathf.Max(0,(liftCeiling-p.y)/Mathf.Max(.001f,dt)));}
   bool sprint=k.leftShiftKey.isPressed||k.rightShiftKey.isPressed;bool fast=hovering&&aether!=null&&aether.state.tier==3&&sprint;
   float speed=hovering?(fast?10.5f:config.hoverSpeed):(sprint?7:4);
   var desired=game.controller.transform.TransformDirection(new Vector3(input.x,0,input.y))*speed+Vector3.up*(fast?up*2:up);
   Rush=Mathf.MoveTowards(Rush,fast&&(input.sqrMagnitude>.01f||Mathf.Abs(up)>.1f)?1:0,dt*2.5f);fx.Rush(Rush);
   moveVelocity=Vector3.MoveTowards(moveVelocity,desired,dt*12);velocity=moveVelocity;motion=hovering?"Anchor hover":liftRemaining>0?"Short lift":"Descending";
   if(ground&&velocity.y<0){descending=false;restoreFlight=false;liftRemaining=0;if(hovering&&down){cancelledAnchor=anchorId;hovering=false;anchorId="";}moveVelocity=Vector3.zero;}
  }else{moveVelocity=Vector3.zero;motion="Grounded";Rush=Mathf.MoveTowards(Rush,0,dt*4);fx.Rush(Rush);}
  return active;
 }
 public void PrepareSave(){
  if(!Travelling)return;Travelling=false;routeArmed=false;travelCooldown=3;hovering=false;descending=true;liftUsed=true;
  game.controller.enabled=false;game.controller.transform.position=aether.config.routeStart;game.controller.enabled=true;moveVelocity=Vector3.zero;Rush=0;fx.Rush(0);game.Notify("Saved safely at the home current entrance");log("CURRENT_SAVE_SETTLED");
 }
 AirCrateState RecordCrate(AirCrate c){var h=c.GetComponent<CargoHarvest>();return new AirCrateState{id=c.id,position=c.transform.position,rotation=c.transform.rotation,safe=c.safe,damage=h?h.state.damage:0,depleted=h&&h.state.depleted,collected=h&&h.state.collected};}
 public AirState Snapshot(){
  Release();StopWind();gesture.Cancel();fx.Cancel();
  // Settle crate motion with swept collision; never teleport through an occupied floor.
  foreach(var c in crates){if(!c)continue;var harvest=c.GetComponent<CargoHarvest>();if(harvest){harvest.Settle(game.EarthTuning);harvest.Interrupt();}if(Broken(c)||!c.simulating)continue;c.velocity=Vector3.zero;c.Sweep(Vector3.down*40);if(c.transform.position.y< -1||!c.Grounded())Recover(c);if(c.Grounded())c.safe=c.transform.position;}
  var s=new AirState{aether=aether!=null?aether.Snapshot():new AetherState(),liftUsed=liftUsed,descending=descending||liftRemaining>0,hoverAnchor=hovering?anchorId:""};
  foreach(var c in crates)if(c&&!c.furniture)s.crates.Add(RecordCrate(c));return s;
 }
 public List<AirCrateState> ObserveMovables(){var a=new List<AirCrateState>();foreach(var c in crates)if(c)a.Add(RecordCrate(c));return a;}
 public AirState Observe(){var s=new AirState{aether=aether!=null?aether.state:new AetherState(),liftUsed=liftUsed,descending=descending,hoverAnchor=anchorId};foreach(var c in crates)if(c&&!c.furniture)s.crates.Add(RecordCrate(c));return s;}
}
}

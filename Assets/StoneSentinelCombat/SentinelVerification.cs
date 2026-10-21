using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Skywright {
// Explicit command-line verification only. Never attached in normal play.
public sealed class SentinelVerification : MonoBehaviour {
 readonly List<string> checks=new List<string>();SentinelCombat combat;SkywrightGame game;SentinelEnemy enemy;
 Mouse testMouse;Keyboard testKeyboard;
 void Input(bool left=false,bool right=false,bool push=false){InputSystem.QueueStateEvent(testMouse,new MouseState{buttons=(ushort)((left?1:0)|(right?2:0))});InputSystem.QueueStateEvent(testKeyboard,push?new KeyboardState(Key.LeftShift):new KeyboardState());InputSystem.Update();testMouse.MakeCurrent();testKeyboard.MakeCurrent();}
 void ReleaseTestDevices(){if(testMouse!=null){InputSystem.RemoveDevice(testMouse);testMouse=null;}if(testKeyboard!=null){InputSystem.RemoveDevice(testKeyboard);testKeyboard=null;}}
 void Frame(bool held=false,bool air=false,bool push=false,bool right=false){Input(air&&push?false:held,right||air&&push,false);game.cam.transform.LookAt(enemy.transform.position+Vector3.up*1.35f);Physics.SyncTransforms();game.CombatEarth.Tick(1f/60);if(air)game.CombatAir.Tick(1f/60);enemy.Tick(1f/60);Physics.SyncTransforms();}
 void ResetReady(){combat.Restart();enemy.ResetEncounter(true);enemy.allowLeap=false;}
 void Fresh(){Input();ResetReady();game.CombatVerificationPause(false);game.PlaceCombatPlayer(new Vector3(0,.12f,-7));game.CombatAir.Equip(false);game.CombatEarth.Equip(true);Frame();}
 void Check(bool condition,string label){if(!condition)throw new Exception("COMBAT VERIFY FAIL: "+label);checks.Add(label);Debug.Log("COMBAT VERIFY PASS: "+label);}
 void StepWind(int count,Vector3 origin,Vector3 direction){for(int i=0;i<count;i++){combat.ApplyWind(true,origin,direction,.05f);enemy.Tick(.05f);}}
 IEnumerator Start(){
  yield return null;combat=SentinelCombat.Current;game=combat.Game;enemy=combat.Enemy;
  combat.enabled=false;game.enabled=false;game.CombatVerificationPause(false);Physics.SyncTransforms();
  var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-combatverify");string folder=Path.GetFullPath(args[index+1]);Directory.CreateDirectory(folder);
  // Hidden verification players must receive their virtual events independently of desktop focus.
  Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
  testMouse=InputSystem.AddDevice<Mouse>();testKeyboard=InputSystem.AddDevice<Keyboard>();InputSystem.EnableDevice(testMouse);InputSystem.EnableDevice(testKeyboard);testMouse.MakeCurrent();testKeyboard.MakeCurrent();Input();
  try{Verify();VerifyRevision();VerifyAggro();VerifyMotions();File.WriteAllText(Path.Combine(folder,"checks.txt"),string.Join("\n",checks));}
  catch(Exception e){File.WriteAllText(Path.Combine(folder,"FAILED.txt"),e.ToString());Debug.LogException(e);ReleaseTestDevices();Application.Quit(2);yield break;}
  ReleaseTestDevices();
  ResetReady();enemy.SetPaused(true);game.CombatVerificationPause(true);game.HideCombatCaptureTools();
  foreach(var r in game.cam.GetComponentsInChildren<Renderer>())r.enabled=false;
  game.cam.fieldOfView=45;game.cam.transform.position=new Vector3(3,1.8f,-5);game.cam.transform.LookAt(enemy.transform.position+Vector3.up*1.15f);
  var animation=enemy.GetComponentInChildren<Animation>();
  var capturePosition=game.cam.transform.position;var captureRotation=game.cam.transform.rotation;
  enemy.ResetEncounter();PositionPlayer(new Vector3(0,.12f,-10));enemy.Tick(.01f);enemy.SetPaused(true);
  game.cam.transform.SetPositionAndRotation(capturePosition,captureRotation);
  yield return null;Capture(game.cam,Path.Combine(folder,"Encounter_Dormant.tga"));
  PositionPlayer(new Vector3(0,.12f,-7));enemy.Tick(.01f);for(int i=0;i<60;i++)enemy.Tick(.025f);
  animation.Stop();animation.Play("Awaken");animation["Awaken"].speed=0;animation["Awaken"].time=1.5f;animation.Sample();
  game.cam.transform.SetPositionAndRotation(capturePosition,captureRotation);
  yield return null;Capture(game.cam,Path.Combine(folder,"Encounter_Rising.tga"));
  ResetReady();game.CombatVerificationPause(true);game.HideCombatCaptureTools();enemy.SetPaused(true);
  game.cam.transform.SetPositionAndRotation(capturePosition,captureRotation);
  string[] names={"Idle","Walk","Windup","Slam","Stagger","Swat","Dormant","Awaken","LeapWindup","Leap","LeapRecover"};float[] times={.6f,.375f,.8f,1.25f,.4f,.48f,0,1.6f,.5f,.5f,.3f};
  yield return new WaitForSecondsRealtime(2);
  for(int i=0;i<names.Length;i++){
   animation.Stop();animation.Play(names[i]);animation[names[i]].speed=0;animation[names[i]].time=times[i];animation.Sample();
   yield return null;Capture(game.cam,Path.Combine(folder,names[i]+".tga"));
  }
  animation.Stop();animation.Play("Idle");animation["Idle"].speed=0;animation["Idle"].time=.6f;animation.Sample();
  for(int stage=1;stage<=3;stage++){enemy.Armor.SetDamage(24-stage*6);enemy.Armor.Tick(.1f);yield return null;Capture(game.cam,Path.Combine(folder,"Armor"+stage+".tga"));}
  enemy.Cease();game.CombatVerificationPause(false);for(int i=0;i<24;i++){enemy.EarthImpact(enemy.transform.position);enemy.Tick(.13f);}game.CombatVerificationPause(true);
  animation.Stop();animation.Play("Defeat");animation["Defeat"].speed=0;animation["Defeat"].time=.4f;animation.Sample();enemy.Armor.BreakApart();
  for(int i=0;i<90;i++){enemy.Armor.Tick(1f/60);combat.AdvanceCaptureEffects(1f/60);}yield return null;File.WriteAllText(Path.Combine(folder,"debris-bounds.txt"),enemy.Armor.DescribeDebris());Capture(game.cam,Path.Combine(folder,"BrokenApart.tga"));
  enemy.Armor.ResetArmor();foreach(var skin in enemy.GetComponentsInChildren<SkinnedMeshRenderer>())skin.enabled=false;
  for(int i=0;i<3;i++){var g=new GameObject("Limestone drop review "+i);g.AddComponent<MeshFilter>().sharedMesh=combat.LimestoneChip(i);g.AddComponent<MeshRenderer>().sharedMaterial=combat.limestoneMaterial;g.transform.position=new Vector3((i-1)*.7f,.08f,0);g.transform.rotation=Quaternion.Euler(12+i*17,i*63,9);g.transform.localScale=Vector3.one*.6f;g.transform.position+=Vector3.up*(.012f-SentinelArmor.LowestPoint(g.transform,combat.LimestoneChip(i).vertices));}
  game.cam.transform.position=new Vector3(1.5f,1.1f,-2.8f);game.cam.transform.LookAt(new Vector3(0,.3f,0));yield return null;Capture(game.cam,Path.Combine(folder,"LimestoneDrops.tga"));
  Debug.Log("SENTINEL_VERIFICATION_COMPLETE checks="+checks.Count);Application.Quit();
 }
 void Verify(){
  ResetReady();game.CombatVerificationPause(false);Physics.SyncTransforms();
  enemy.EarthImpact(enemy.transform.position);Check(enemy.Health==SentinelEnemy.MaxHealth-1,"Armor takes one damage");
  enemy.EarthImpact(enemy.transform.position);Check(enemy.Health==SentinelEnemy.MaxHealth-1,"Duplicate contact is gated");
  game.CombatVerificationPause(true);enemy.EarthImpact(enemy.transform.position);Check(enemy.Health==SentinelEnemy.MaxHealth-1,"Paused impact cannot damage");game.CombatVerificationPause(false);
  ResetReady();game.PlaceCombatPlayer(new Vector3(0,.04f,-7));Physics.SyncTransforms();
  StepWind(12,new Vector3(0,1.4f,-12),Vector3.forward);Check(enemy.State!="Stagger","Wind outside eight metres does not stagger");
  ResetReady();game.PlaceCombatPlayer(new Vector3(0,.04f,-7));Physics.SyncTransforms();
  StepWind(12,new Vector3(0,1.4f,-6),Vector3.back);Check(enemy.State!="Stagger","Wind aimed away does not stagger");
  ResetReady();game.PlaceCombatPlayer(new Vector3(0,.04f,-7));
  var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.transform.position=new Vector3(0,1.5f,-3);blocker.transform.localScale=new Vector3(3,3,.25f);Physics.SyncTransforms();
  StepWind(12,new Vector3(0,1.4f,-6),Vector3.forward);Check(enemy.State!="Stagger","Solid cover blocks wind");
  blocker.SetActive(false);Destroy(blocker);ResetReady();game.PlaceCombatPlayer(new Vector3(0,.04f,-7));Physics.SyncTransforms();
  StepWind(10,new Vector3(0,1.4f,-6),Vector3.forward);Check(enemy.State=="Stagger"&&enemy.Exposed,"Focused wind staggers and exposes core");
  enemy.EarthImpact(enemy.transform.position);Check(enemy.Health==SentinelEnemy.MaxHealth-2,"Exposed core takes double damage");
  StepWind(50,new Vector3(0,1.4f,-6),Vector3.forward);Check(enemy.State!="Stagger","Held wind cannot permanently stun");
  ResetReady();game.PlaceCombatPlayer(new Vector3(0,.04f,-3));Physics.SyncTransforms();enemy.Tick(.01f);Check(enemy.State=="Windup","Nearby player triggers telegraphed windup");
  StepWind(6,new Vector3(0,1.4f,-3),Vector3.forward);Check(enemy.State=="Stagger","Wind interrupts a pending slam");Check(combat.Health==100,"Interrupted slam does not hit player");
  ResetReady();game.PlaceCombatPlayer(new Vector3(0,.04f,-3));Physics.SyncTransforms();
  for(int i=0;i<73;i++)enemy.Tick(.05f);Check(combat.Health==75,"Completed combo deals three timed hits totaling 25 damage");
  for(int i=0;i<10;i++)enemy.Tick(.05f);Check(combat.Health==75,"Recovery does not repeat slam damage");
  ResetReady();game.PlaceCombatPlayer(new Vector3(0,1.3f,-3));Physics.SyncTransforms();combat.Slam(new Vector3(0,.04f,-3),2.35f);Check(combat.Health==100,"Airborne leap clears ground shock");
  game.PlaceCombatPlayer(new Vector3(5,.04f,-3));Physics.SyncTransforms();combat.Slam(new Vector3(0,.04f,-3),2.35f);Check(combat.Health==100,"Moving outside marker dodges slam");
  ResetReady();game.PlaceCombatPlayer(new Vector3(0,.04f,-3));Physics.SyncTransforms();
  for(int i=0;i<4;i++)combat.Slam(new Vector3(0,.04f,-3),2.35f);Check(combat.PlayerDown&&combat.Health==0,"Four hits enter recovery state");
  ResetReady();Check(combat.Health==100&&enemy.Health==SentinelEnemy.MaxHealth,"Retry restores both combatants");
  for(int i=0;i<SentinelEnemy.MaxHealth;i++){enemy.EarthImpact(enemy.transform.position);enemy.Tick(.13f);}
  Check(!enemy.Alive&&!enemy.GetComponent<CharacterController>().enabled,"Defeat disables spell attachment collider");
  enemy.EarthImpact(enemy.transform.position);Check(enemy.Health==0,"Defeated enemy takes no further damage");
  var animation=enemy.GetComponentInChildren<Animation>();
  foreach(string name in new[]{"Idle","Walk","Windup","Slam","Recover","Stagger","Defeat","Swat","Dormant","Awaken","LeapWindup","Leap","LeapRecover"})Check(animation[name]!=null&&animation[name].length>0,"Skeletal clip present: "+name);
 }
 void VerifyRevision(){
  Check(combat.limestoneChips.Length==3&&combat.limestoneMaterial.GetTexture("_BaseMap")!=null&&combat.limestoneMaterial.GetTexture("_BumpMap")!=null,"Three sculpted limestone drops retain detailed color and normal textures");
  foreach(var chip in combat.limestoneChips)Check(chip.bounds.size.magnitude>.5f&&chip.bounds.size.magnitude<1.8f&&chip.bounds.center.magnitude<.001f,"Limestone mesh uses normalized runtime units: "+chip.name);
  Fresh();var earth=game.CombatEarth;int identity=earth.CombatStoneInstance;
  Check(earth.phase=="Prepared"&&earth.StoneCount==1&&enemy.Health==24,"Earth auto-readies without clicking or damaging");
  for(int i=0;i<12;i++){earth.Equip(false);game.CombatAir.Equip(true);Frame();game.CombatAir.Equip(false);earth.Equip(true);Frame();earth.Equip(false);Frame();earth.Equip(true);Frame();}
  Check(earth.phase=="Prepared"&&earth.CombatStoneInstance==identity&&enemy.Health==24,"Repeated Air/Earth and stow/equip retain one ready stone without attacks");
  earth.ResetCombatStone();var block=GameObject.CreatePrimitive(PrimitiveType.Cube);block.transform.position=game.cam.transform.position+game.cam.transform.forward*.5f;block.transform.localScale=Vector3.one*2;Physics.SyncTransforms();Frame();
  Check(earth.StoneCount==0&&enemy.Health==24,"Obstructed equip cannot summon or deal damage");block.SetActive(false);Destroy(block);Physics.SyncTransforms();Frame();
  Check(earth.StoneCount==1&&earth.phase=="Prepared","Clearing summon space readies automatically");
  Fresh();identity=earth.CombatStoneInstance;Frame(true);Check(earth.phase=="Guided","First LMB launches the automatically readied stone");
  for(int i=0;i<100&&!earth.BoundTo(enemy);i++)Frame(true);
  Check(earth.BoundTo(enemy)&&enemy.Health<24,"Cast reaches and damages Sentinel through collision");
  Vector3 prior=earth.ProjectilePosition;var cc=enemy.GetComponent<CharacterController>();cc.enabled=false;enemy.transform.position+=Vector3.right*.4f;enemy.transform.Rotate(0,15,0);cc.enabled=true;Frame(true);
  Check(earth.BoundTo(enemy)&&Vector3.Distance(prior,earth.ProjectilePosition)>.1f,"Attached strike follows target translation and rotation without false obstruction");
  for(int i=0;i<500&&enemy.Swats==0;i++)Frame(true);
  Check(enemy.Swats==1&&enemy.Health>=18&&enemy.Health<24,"Three successful held strikes trigger a real swat before lethal damage");
  Check(earth.phase=="KnockedAway"&&earth.CombatFreshPressRequired&&!earth.BoundTo(enemy),"Swat detaches and knocks away stone with fresh-press gate");
  earth.CancelCast("focus lost");Check(earth.phase=="KnockedAway"&&earth.CombatStoneInstance==identity,"Focus loss preserves knocked-away stone");
  earth.Equip(false);game.CombatAir.Equip(true);Frame(true);game.CombatAir.Equip(false);earth.Equip(true);Frame(true);
  Check(earth.phase=="KnockedAway"&&earth.CombatStoneInstance==identity,"Swapping during knockback preserves stone and recovery");
  for(int i=0;i<22;i++)Frame(true);earth.Equip(false);Frame(true);earth.Equip(true);
  Check(earth.CombatStoneInstance==identity&&earth.CombatFreshPressRequired&&earth.phase!="Guided","Stow/equip during return cannot bypass swat");
  for(int i=0;i<900&&earth.phase!="Prepared";i++)Frame(true);
  Check(earth.phase=="Prepared"&&earth.CombatStoneInstance==identity&&earth.CombatFreshPressRequired,"Knocked stone returns ready as the same instance while LMB remains held");
  int hp=enemy.Health;for(int i=0;i<30;i++)Frame(true);
  Check(earth.phase=="Prepared"&&enemy.Health==hp,"Held LMB cannot relaunch after swat");Frame();Frame(true);
  Check(earth.phase=="Guided"&&!earth.CombatFreshPressRequired,"Release then fresh LMB permits the next cast");
  Frame(false,false,false,true);Check(earth.phase=="Returning"||earth.phase=="Catching"||earth.phase=="Blocked","RMB recalls the existing cast");
  for(int i=0;i<900&&earth.phase!="Prepared";i++)Frame();Check(earth.phase=="Prepared"&&earth.CombatStoneInstance==identity,"Manual recall returns the same reusable stone");
  Fresh();identity=earth.CombatStoneInstance;Frame(true);for(int i=0;i<100&&!earth.BoundTo(enemy);i++)Frame(true);Frame(false,false,false,true);
  block=GameObject.CreatePrimitive(PrimitiveType.Cube);block.transform.position=earth.ProjectilePosition;block.transform.localScale=Vector3.one*1.5f;Physics.SyncTransforms();
  for(int i=0;i<900&&earth.phase!="Prepared";i++){Input();earth.Tick(1f/60);}
  Check(earth.phase=="Prepared"&&earth.CombatStoneInstance==identity,"Obstructed recall fades back without creating another stone");block.SetActive(false);Destroy(block);Physics.SyncTransforms();
  Fresh();earth.Equip(false);game.CombatAir.Equip(true);
  for(int i=0;i<70&&enemy.State!="Stagger";i++)Frame(true,true);
  Check(enemy.State=="Stagger","Air pull input staggers");float resistance=enemy.WindResistance;Frame(false,true);Frame(true,true);
  Check(enemy.WindResistance<resistance&&enemy.State=="Stagger","Quick wind release/repress does not reset resistance");
  for(int i=0;i<200;i++)Frame(false,true);Check(enemy.State!="Stagger"&&enemy.WindResistance>0,"Stagger expires before temporary wind resistance");
  for(int i=0;i<160;i++)Frame(false,true);
  for(int i=0;i<70&&enemy.State!="Stagger";i++)Frame(true,true,true);
  Check(enemy.State=="Stagger","RMB wind push staggers again after resistance");
  for(int i=0;i<340;i++)Frame(false,true);
  for(int i=0;i<70&&enemy.State!="Stagger";i++)Frame(true,true);
  Check(enemy.State=="Stagger","Third stagger works after push-to-pull reuse");
  Fresh();for(int stage=1;stage<=3;stage++){
   while(enemy.Health>24-stage*6){enemy.EarthImpact(enemy.transform.position);enemy.Tick(.13f);}
   Check(enemy.Armor.Stage==stage&&enemy.Armor.DebrisCount==stage*2,"Damage sheds real armor at stage "+stage);
  }
  while(enemy.Alive){enemy.EarthImpact(enemy.transform.position);enemy.Tick(.13f);}enemy.Tick(.5f);
  Check(enemy.Armor.BrokenApart&&enemy.Armor.DebrisCount<=32&&enemy.Armor.DebrisCount>=24,"Death replaces body with bounded stone fragments");
  bool sized=true,near=true;foreach(var piece in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(piece.name.StartsWith("Detached ")){sized&=piece.bounds.size.magnitude<4;near&=Vector3.Distance(piece.bounds.center,enemy.transform.position)<8;if(piece.bounds.size.magnitude>=4)Debug.LogError("DEBRIS_SIZE "+piece.name+" "+piece.bounds);}
  Check(sized&&near,"Detached mesh dimensions and locations remain at creature scale");
  foreach(var skin in enemy.GetComponentsInChildren<SkinnedMeshRenderer>())Check(!skin.enabled,"Defeated section hidden: "+skin.name);
  for(int i=0;i<900;i++)enemy.Tick(1f/60);Check(enemy.Armor.DebrisCount==0,"Death debris cleans up after lifetime");
  Fresh();Check(enemy.Armor.Stage==0&&!enemy.Armor.BrokenApart&&enemy.Armor.DebrisCount==0,"Retry restores armor and clears debris");
  while(enemy.Health>1){enemy.EarthImpact(enemy.transform.position);enemy.Tick(.13f);}
  Frame(true);for(int i=0;i<180&&enemy.Alive;i++)Frame(true);
  Check(!enemy.Alive&&!earth.BoundTo(enemy),"Lethal attached impact releases stone");
  for(int i=0;i<900&&earth.phase!="Prepared";i++)Frame(true);Check(earth.phase=="Prepared","Stone recovers safely after Sentinel death");Input();
 }
 void PositionPlayer(Vector3 p){game.controller.enabled=false;game.controller.transform.position=p;game.controller.enabled=true;Physics.SyncTransforms();}
 void VerifyAggro(){
  Fresh();enemy.ResetEncounter(true);PositionPlayer(new Vector3(0,.12f,-13));enemy.Tick(.05f);Check(!enemy.Aggroed,"Unaware Sentinel does not acquire beyond notice range");
  PositionPlayer(new Vector3(0,.12f,-11));enemy.Tick(.05f);Check(enemy.Aggroed&&enemy.State=="Walk","Visible player inside notice range acquires aggro");
  PositionPlayer(new Vector3(0,.12f,-14));for(int i=0;i<20;i++)enemy.Tick(.05f);Check(enemy.Aggroed&&enemy.State=="Walk","Pursuit persists beyond the old twelve-metre cutoff");
  PositionPlayer(new Vector3(0,.12f,-18));for(int i=0;i<20;i++)enemy.Tick(.05f);Check(enemy.Aggroed&&enemy.State=="Walk","Player passing the old sixteen-metre home cutoff does not reset pursuit");
  int hp=enemy.Health;PositionPlayer(new Vector3(0,.12f,-40));for(int i=0;i<20;i++)enemy.Tick(.05f);Check(enemy.Aggroed,"Brief separation retains aggro during grace period");
  for(int i=0;i<44;i++)enemy.Tick(.05f);Check(!enemy.Aggroed&&enemy.State=="Return"&&enemy.Health==hp,"Sustained separation returns home without resetting health");
  Fresh();enemy.ResetEncounter(true);PositionPlayer(new Vector3(0,30,-3));enemy.Tick(.05f);Check(!enemy.Aggroed,"Detection uses vertical distance as well as horizontal range");
  PositionPlayer(new Vector3(0,.12f,-10));var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(0,2,-5);wall.transform.localScale=new Vector3(8,4,.3f);Physics.SyncTransforms();enemy.Tick(.05f);Check(!enemy.Aggroed,"Solid cover blocks initial detection");
  wall.SetActive(false);Physics.SyncTransforms();enemy.Tick(.05f);wall.SetActive(true);Physics.SyncTransforms();for(int i=0;i<20;i++)enemy.Tick(.05f);Check(enemy.Aggroed,"Brief cover does not abruptly end pursuit");
  wall.SetActive(false);Destroy(wall);Physics.SyncTransforms();enemy.Tick(.05f);Check(enemy.Aggroed,"Regaining sight resumes pursuit without reset");
  var cc=enemy.GetComponent<CharacterController>();cc.enabled=false;enemy.transform.position=new Vector3(23,.05f,0);cc.enabled=true;PositionPlayer(new Vector3(23,.12f,-5));enemy.Tick(.05f);Check(!enemy.Aggroed&&enemy.State=="Return","Home leash still bounds pursuit");
 }
 void ReadyLeap(){
  ResetReady();enemy.allowLeap=true;game.CombatVerificationPause(false);
  PositionPlayer(new Vector3(0,.12f,-40));enemy.Tick(2.1f);
  PositionPlayer(new Vector3(0,.12f,-11.5f));enemy.Tick(.01f);
 }
 void VerifyMotions(){
  combat.Restart();game.CombatVerificationPause(false);enemy.allowLeap=true;
  PositionPlayer(new Vector3(0,.12f,-10));enemy.Tick(.1f);
  Check(enemy.State=="Dormant"&&!enemy.Awakened&&!enemy.Aggroed,"Sentinel initially rests as unlit terrain stone");
  PositionPlayer(new Vector3(0,.12f,-7));
  var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(0,2,-4);wall.transform.localScale=new Vector3(8,4,.3f);Physics.SyncTransforms();enemy.Tick(.1f);
  Check(enemy.State=="Dormant","Cover blocks proximity awakening");wall.SetActive(false);Destroy(wall);Physics.SyncTransforms();enemy.Tick(.1f);
  Check(enemy.State=="Awaken"&&!enemy.Awakened,"Approaching inside eight metres starts stand-up animation");
  for(int i=0;i<60;i++)enemy.Tick(.05f);
  Check(combat.Health==100&&enemy.State=="Awaken","Sentinel cannot attack during its rise");
  for(int i=0;i<5;i++)enemy.Tick(.05f);
  Check(enemy.Awakened&&enemy.State!="Dormant"&&enemy.State!="Awaken","Rise completes into active encounter");
  PositionPlayer(new Vector3(0,.12f,-40));for(int i=0;i<200;i++)enemy.Tick(.05f);
  Check(enemy.Awakened&&enemy.State!="Dormant","Disengaging does not replay first awakening");
  combat.Restart();enemy.EarthImpact(enemy.transform.position);Check(enemy.State=="Awaken"&&enemy.Health==23,"Earth impact disturbs sleeping Sentinel");
  combat.Restart();enemy.Wind(.1f);enemy.Tick(.05f);Check(enemy.State=="Awaken","Wind disturbs sleeping Sentinel");
  ReadyLeap();Check(enemy.State=="LeapWindup","Out-of-melee target triggers telegraphed leap");
  Vector3 landing=enemy.LeapTarget;
  PositionPlayer(new Vector3(6,.12f,-12));for(int i=0;i<14;i++)enemy.Tick(.05f);
  Check(enemy.State=="Leap"&&enemy.Leaps==1,"Windup launches one leap");
  enemy.Tick(.45f);Check(enemy.transform.position.y>1.5f,"Leap physically lifts Sentinel above ground");
  Check(enemy.LeapTarget==landing,"Leap landing locks before launch and does not track dodge");
  for(int i=0;i<14;i++)enemy.Tick(.05f);
  Check(enemy.State=="LeapRecover"&&Vector3.Distance(enemy.transform.position,landing)<.3f,"Leap lands at the marked position and enters recovery");
  Check(combat.Health==100,"Leaving the fixed landing circle dodges leap damage");
  Check(enemy.LeapCooldown>0&&!enemy.PlanLeap(new Vector3(0,.12f,-20)),"Cooldown prevents chained leaps");
  ReadyLeap();for(int i=0;i<6;i++){enemy.Wind(.05f);enemy.Tick(.05f);}
  Check(enemy.State=="Stagger"&&enemy.Leaps==0,"Focused wind interrupts leap before takeoff");
  ReadyLeap();for(int i=0;i<40;i++)enemy.Tick(.05f);
  Check(combat.Health==75&&enemy.Leaps==1,"Landing delivers exactly one 25-point hit");
  int health=combat.Health;for(int i=0;i<8;i++)enemy.Tick(.05f);Check(combat.Health==health,"Leap recovery cannot repeat landing damage");
  ResetReady();enemy.allowLeap=true;PositionPlayer(new Vector3(0,.12f,-40));enemy.Tick(2.1f);
  Check(!enemy.PlanLeap(new Vector3(0,.12f,-4))&&!enemy.PlanLeap(new Vector3(0,.12f,-18))&&!enemy.PlanLeap(new Vector3(0,5,-12)),"Leap rejects close, distant and high airborne targets");
  wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(0,3,-5);wall.transform.localScale=new Vector3(8,6,.5f);Physics.SyncTransforms();
  Check(!enemy.PlanLeap(new Vector3(0,.12f,-12)),"Solid obstruction blocks the planned leap arc");wall.SetActive(false);Destroy(wall);Physics.SyncTransforms();
  combat.Restart();Check(enemy.State=="Dormant"&&!enemy.Awakened&&enemy.Leaps==0,"Retry restores the dormant reveal and clears leap state");
 }
 static void Capture(Camera view,string path){
  const int width=1600,height=900;var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);target.Create();
  RenderPipeline.SubmitRenderRequest(view,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
  var texture=new Texture2D(width,height,TextureFormat.RGB24,false);var old=RenderTexture.active;RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();RenderTexture.active=old;
  using(var writer=new BinaryWriter(File.Create(path))){byte[] h=new byte[18];h[2]=2;h[12]=(byte)(width&255);h[13]=(byte)(width>>8);h[14]=(byte)(height&255);h[15]=(byte)(height>>8);h[16]=24;writer.Write(h);foreach(var p in texture.GetPixels32()){writer.Write(p.b);writer.Write(p.g);writer.Write(p.r);}}
  Destroy(texture);target.Release();Destroy(target);
 }
}
}

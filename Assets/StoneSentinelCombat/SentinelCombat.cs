using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Skywright {
[DefaultExecutionOrder(-1000)]
public sealed class SentinelCombat : MonoBehaviour {
 public static SentinelCombat Current {get;private set;}
 public static bool Active=>Current!=null;
 public GameObject creaturePrefab;
 public Material warningMaterial;
 public Mesh[] limestoneChips;
 public Material limestoneMaterial;
 public Mesh LimestoneChip(int index)=>limestoneChips!=null&&limestoneChips.Length>0?limestoneChips[index%limestoneChips.Length]:null;
 public SentinelEnemy Enemy {get;private set;}
 public SkywrightGame Game {get;private set;}
 public int Health {get;private set;}=100;
 public float HurtFlash {get;private set;}
 public bool PlayerDown=>Health<=0;
 public bool Paused=>Game==null||Game.CombatPaused;
 public readonly Vector3 Spawn=new Vector3(0,.12f,-10),Home=new Vector3(0,.05f,0);
 float recovery,qaNext;EarthEffects effects;string qaFile;
 readonly bool qa=Array.IndexOf(Environment.GetCommandLineArgs(),"-combatqa")>=0;
 void Awake(){Current=this;}
 void Start(){
  Game=GetComponent<SkywrightGame>();Game.BeginSentinelCombat(Spawn);
  effects=new EarthEffects(Game.EarthTuning,Game.cam);
  var instance=Instantiate(creaturePrefab,Home,Quaternion.Euler(0,180,0));
  instance.name="Stone Sentinel";Enemy=instance.AddComponent<SentinelEnemy>();Enemy.Initialize(this,Home);
  qaFile=Path.GetFullPath(Path.Combine(Application.dataPath,"../combat-state.json"));
  Game.PauseCombat();
  if(Array.IndexOf(Environment.GetCommandLineArgs(),"-combatverify")>=0)gameObject.AddComponent<SentinelVerification>();
  Debug.Log("SENTINEL_COMBAT_READY session-only; no save load/write; one enemy");
 }
 void LateUpdate(){
  if(Game==null||Enemy==null)return;
  float dt=Mathf.Min(Time.deltaTime,.05f);
  Enemy.SetPaused(Paused);
  if(!Paused){
   HurtFlash=Mathf.MoveTowards(HurtFlash,0,dt*2);effects.Tick(dt);
   if(PlayerDown){recovery-=dt;if(recovery<=0){Restart();Game.Notify("Recovered — no supplies lost");}}
   else {Enemy.Tick(dt);if(!Enemy.Alive&&Keyboard.current!=null&&Keyboard.current.enterKey.wasPressedThisFrame)Restart();}
  }
  if(qa&&Time.unscaledTime>=qaNext){qaNext=Time.unscaledTime+.1f;WriteQA();}
 }
 public void Restart(){Health=100;HurtFlash=0;recovery=0;effects?.Clear();Game.PlaceCombatPlayer(Spawn);Enemy.ResetEncounter();}
 public void ApplyWind(bool active,Vector3 origin,Vector3 direction,float dt){
  if(Enemy==null||Paused||PlayerDown||!Enemy.Alive)return;
  Vector3 center=Enemy.transform.position+Vector3.up*1.35f;
  float along=Mathf.Max(0,Vector3.Dot(center-origin,direction));
  Vector3 target=Enemy.HitCollider.ClosestPoint(origin+direction*along);var delta=target-origin;
  bool aimed=active&&delta.magnitude<=8&&Vector3.Angle(direction,delta)<18&&ClearLine(origin,target,Enemy.transform);
  Enemy.Wind(aimed?dt:0);
 }
 public static bool ClearLine(Vector3 from,Vector3 to,Transform allowed){
  var delta=to-from;
  foreach(var hit in Physics.RaycastAll(from,delta.normalized,delta.magnitude,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore))
   if(!hit.transform.IsChildOf(allowed)&&hit.transform!=allowed)return false;
  return true;
 }
 public void Slam(Vector3 center,float radius,int damage=25){
  effects.Sound(Game.EarthTuning.impact,center,.7f);effects.Burst(center+Vector3.up*.1f,Vector3.up,Game.EarthTuning.stone,20,.07f,3,.55f);
  effects.Ring(center+Vector3.up*.04f,Vector3.up,radius,.45f);
  Vector3 p=Game.controller.transform.position;
  var planar=p-center;planar.y=0;
  if(planar.magnitude<=radius+Game.controller.radius&&Mathf.Abs(p.y-center.y)<.75f&&ClearLine(center+Vector3.up*.35f,p+Vector3.up*.35f,Enemy.transform)){
   Health=Mathf.Max(0,Health-damage);HurtFlash=1;Debug.Log("COMBAT PLAYER_HIT health="+Health);
   if(PlayerDown){recovery=2.5f;Game.InterruptCombatSpells();Enemy.Cease();}
  }
 }
 public void ImpactSound(Vector3 p){effects.Sound(Game.EarthTuning.impact,p,.45f);}
 internal void AdvanceCaptureEffects(float dt){effects.Tick(dt);}
 public void BreakEffect(Vector3 p){effects.Sound(Game.EarthTuning.crack,p,.8f);effects.Burst(p+Vector3.up,Vector3.up,Game.EarthTuning.stone,18,.055f,2,.7f);effects.Ring(p+Vector3.up*.1f,Vector3.up,1.8f,.5f);}
 [Serializable] class Snapshot {public Vector3 player,enemy,stone;public float yaw,pitch,wind,enemyTime,resistance;public int health,enemyHealth,stoneCount,stoneInstance,swats,leaps,armorStage,debris,hits;public string state,stonePhase,bound;public bool paused,exposed,down,freshPressRequired,brokenApart,aggroed,awakened;}
 void WriteQA(){
  var s=new Snapshot{player=Game.controller.transform.position,enemy=Enemy.transform.position,stone=Game.CombatStonePosition,yaw=Game.yaw,pitch=Game.pitch,health=Health,enemyHealth=Enemy.Health,state=Enemy.State,enemyTime=Enemy.StateTime,wind=Enemy.WindCharge,stoneCount=Game.CombatStoneCount,stonePhase=Game.CombatStonePhase,bound=Game.CombatBoundTarget,paused=Paused,exposed=Enemy.Exposed,down=PlayerDown,resistance=Enemy.WindResistance,stoneInstance=Game.CombatEarth.CombatStoneInstance,freshPressRequired=Game.CombatEarth.CombatFreshPressRequired,swats=Enemy.Swats,leaps=Enemy.Leaps,awakened=Enemy.Awakened,armorStage=Enemy.Armor.Stage,debris=Enemy.Armor.DebrisCount,brokenApart=Enemy.Armor.BrokenApart,hits=Enemy.HitsSinceSwat,aggroed=Enemy.Aggroed};
  try{File.WriteAllText(qaFile,JsonUtility.ToJson(s,true));}catch(IOException){}
 }
 void OnDestroy(){if(Current==this)Current=null;}
}
}

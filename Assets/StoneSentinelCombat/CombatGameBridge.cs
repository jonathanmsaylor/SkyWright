using UnityEngine;
namespace Skywright {
public partial class SkywrightGame {
 public bool CombatPaused=>!focused||menu||palette||help||aetherPanel||!started||Time.unscaledTime<inputDelay;
 public void BeginSentinelCombat(Vector3 at){
  builder.Clear();builder.enabled=false;earth.Reset();air.Reset();
  PlaceCombatPlayer(at);airSelected=false;earth.Equip(true);air.Equip(false);dirty=false;Resume();
 }
 public void PlaceCombatPlayer(Vector3 at){
  earth?.ResetCombatStone();air?.RecoverPlayer();
  controller.enabled=false;controller.transform.position=at;controller.enabled=true;
  vertical=0;yaw=0;pitch=3;Look();
 }
 public void InterruptCombatSpells(){earth?.CancelCast("combat interrupted");air?.Pause();}
 public bool CombatStoneBoundTo(SentinelEnemy enemy)=>earth!=null&&earth.BoundTo(enemy);
 public bool DeflectCombatStone(SentinelEnemy enemy)=>earth!=null&&earth.DeflectFrom(enemy);
 internal EarthTrial CombatEarth=>earth;
 internal AirTrial CombatAir=>air;
 public void PauseCombat(){menu=true;CursorState();}
 internal void CombatVerificationPause(bool value){focused=true;menu=value;started=true;inputDelay=0;}
 internal void HideCombatCaptureTools(){earth?.Hide();air?.Pause();}
 public string CombatStonePhase=>earth?.phase??"";
 public int CombatStoneCount=>earth?.StoneCount??0;
 public Vector3 CombatStonePosition=>earth?.ProjectilePosition??Vector3.zero;
 public string CombatBoundTarget=>earth?.BoundTarget??"";
 void DrawSentinelCombat(){
  var combat=SentinelCombat.Current;var enemy=combat.Enemy;
  if(menu){
   Panel(new Rect(0,0,1600,900),new Color(.025f,.055f,.07f,.48f));Panel(new Rect(490,170,620,550),ink);
   Text(530,205,540,60,"STONE SENTINEL",title);Text(532,280,530,60,"Disturb the sleeping limestone",sub);
   Text(532,335,530,90,"Approach the rocks to awaken the Sentinel.\nEarth: cast and hold; recast after its three-hit swat.\nAir: stagger. Dodge combo strikes and marked leaps.");
   if(Button(530,448,540,52,"Resume encounter"))Resume();
   if(Button(530,514,540,52,"Retry encounter")){combat.Restart();Resume();}
   if(Button(530,580,540,52,"Exit combat preview"))Application.Quit();
   Text(532,654,530,35,"Session-only encounter • personal saves are untouched",small);return;
  }
  Panel(new Rect(26,24,330,94),ink);Text(45,37,295,26,"SKYWRIGHT / COMBAT",sub);
  Text(45,76,295,25,"Health  "+combat.Health+" / 100",label);
  if(enemy){
   Panel(new Rect(1080,24,494,125),ink);Text(1100,36,455,30,enemy.State=="Dormant"?"LIMESTONE OUTCROP":"STONE SENTINEL",sub);
   Text(1100,72,455,25,enemy.Status,label);
   var old=GUI.color;GUI.color=new Color(.12f,.25f,.27f,.35f);GUI.DrawTexture(new Rect(1100,112,450,10),Texture2D.whiteTexture);
   GUI.color=new Color(.18f,.72f,.69f);GUI.DrawTexture(new Rect(1100,112,450*enemy.Health/SentinelEnemy.MaxHealth,10),Texture2D.whiteTexture);GUI.color=old;
  }
  if(combat.HurtFlash>0){var old=GUI.color;GUI.color=new Color(.65f,.15f,.12f,combat.HurtFlash*.18f);GUI.DrawTexture(new Rect(0,0,1600,900),Texture2D.whiteTexture);GUI.color=old;}
  Text(790,436,30,30,"+",hudText);
  string primary=AirEquipped?"Hold LMB Suction • Hold RMB Push • Shift+LMB Grab / Release":earth!=null&&earth.equipped?earth.hint:"Tab: equip magic";
  string extra=combat.PlayerDown?"Recovering at the clearing…":enemy&&!enemy.Alive?"Sentinel quieted  •  Enter: retry encounter":enemy&&enemy.Exposed?"CORE EXPOSED — Earthstone deals double damage":"Focus wind within 8 m to stagger. Dodge the marked slam.";
  BottomGlass(primary,"Q Earth / Air • Shift sprint • Double jump leap • Esc pause",extra);
  if(Time.unscaledTime<noticeUntil)Text(450,145,700,45,notice,hudText);
 }
}
}

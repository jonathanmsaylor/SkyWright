using UnityEngine;
namespace Skywright {
public partial class EarthTrial {
 bool combatFreshPressRequired;Matrix4x4 attachmentFrame;Collider knockedSource;float knockClock;
 public bool CombatFreshPressRequired=>combatFreshPressRequired;
 public int CombatStoneInstance=>projectile?projectile.GetInstanceID():0;
 public bool BoundTo(SentinelEnemy enemy)=>boundCollider&&boundCollider.GetComponentInParent<SentinelEnemy>()==enemy;
 public void ResetCombatStone(){DestroyStone();effects.Clear();SetMode(StoneMode.Idle);repeatArmed=false;combatFreshPressRequired=false;knockedSource=null;state.preparedStone=false;}
 void ReadyCombatStone(){
  var at=HoldPoint(out bool clear);if(!clear)return;
  if(!projectile)CreateStone(at);Prepared();UpdatePrepared(0);
  effects.Sound(config.prepare,at,.18f);log("COMBAT AUTO_READY");
 }
 bool CombatAttackPress(bool pressed,bool held){
  if(!SentinelCombat.Active)return pressed;
  if(combatFreshPressRequired){repeatArmed=false;if(mode==StoneMode.Prepared&&!held)combatFreshPressRequired=false;return false;}
  return pressed;
 }
 public bool DeflectFrom(SentinelEnemy enemy){
  if(!BoundTo(enemy)||!projectile)return false;
  knockedSource=boundCollider;var normal=boundTransform.TransformDirection(localNormal).normalized;
  var contact=boundTransform.TransformPoint(castLocal);
  projectile.position=contact+normal*(StoneRadius+.08f);
  velocity=(normal*.7f+enemy.transform.right*.9f+Vector3.up*.65f).normalized*10;
  castNode=null;boundTransform=null;boundCollider=null;repeatArmed=false;combatFreshPressRequired=true;knockClock=0;
  returnAge=returnTravel=0;SetMode(StoneMode.KnockedAway);if(trail){trail.Clear();trail.emitting=true;}
  effects.Sound(config.crack,contact,.8f);effects.Ring(contact,normal,.7f,.3f);log("COMBAT STONE_DEFLECTED");return true;
 }
 void AdvanceKnockedStone(float dt){
  knockClock+=dt;velocity+=Vector3.down*5*dt;var next=projectile.position+velocity*dt;
  if(CombatSweep(projectile.position,next,knockedSource,out var hit)){StopAt(hit,next-projectile.position);knockClock=.4f;}
  else projectile.position=next;
  projectile.Rotate(0,dt*220,dt*85);
  if(knockClock>=.35f){knockedSource=null;SetMode(StoneMode.Returning);actionClock=blockedClock=0;log("COMBAT DEFLECTION_RETURN");}
 }
 bool CombatSweep(Vector3 from,Vector3 to,Collider ignore,out RaycastHit nearest){
  nearest=default;var d=to-from;if(d.sqrMagnitude<.0000001f)return false;float closest=float.MaxValue;
  foreach(var hit in Physics.SphereCastAll(from,StoneRadius,d.normalized,d.magnitude,SpellMask,QueryTriggerInteraction.Ignore))
   if(hit.collider!=ignore&&hit.distance<closest){nearest=hit;closest=hit.distance;}
  return closest<float.MaxValue;
 }
 bool CombatObstructed(Vector3 p,Collider ignore){foreach(var c in Physics.OverlapSphere(p,StoneRadius*.97f,SpellMask,QueryTriggerInteraction.Ignore))if(c!=ignore)return true;return false;}
 bool AdvanceSentinelAttachment(float dt){
  var enemy=boundCollider?boundCollider.GetComponentInParent<SentinelEnemy>():null;if(!enemy)return false;
  if(!TargetAlive()||!enemy.Alive){BeginReturn("Sentinel defeated");return true;}
  if(Vector3.Distance(projectile.position,player.position)>config.maxPlayerDistance){BeginReturn("Sentinel out of range");return true;}
  // Carry the orbit/strike with the moving target before advancing the attack itself.
  var current=boundTransform.localToWorldMatrix;var carried=current.MultiplyPoint3x4(attachmentFrame.inverse.MultiplyPoint3x4(projectile.position));
  strikeStart=current.MultiplyPoint3x4(attachmentFrame.inverse.MultiplyPoint3x4(strikeStart));attachmentFrame=current;
  if(CombatSweep(projectile.position,carried,boundCollider,out _)||CombatObstructed(carried,boundCollider)){BeginReturn("attachment blocked by scenery");return true;}
  projectile.position=carried;
  var contact=boundTransform.TransformPoint(castLocal);var normal=boundTransform.TransformDirection(localNormal).normalized;
  waitPoint=contact+normal*(StoneRadius+config.waitOffset);actionClock+=dt;
  if(mode==StoneMode.Waiting){projectile.position=waitPoint;velocity=Vector3.zero;return true;}
  bool striking=mode==StoneMode.Striking&&actionClock>=config.strikeAnticipation;
  Vector3 destination=mode==StoneMode.Striking?(striking?contact-normal*.05f:strikeStart+normal*.12f):waitPoint;
  var next=Vector3.MoveTowards(projectile.position,destination,(striking?config.strikeSpeed:config.reboundSpeed)*dt);
  if(CombatSweep(projectile.position,next,striking?null:boundCollider,out var hit)){
   if(striking&&hit.collider==boundCollider){projectile.position=hit.point+hit.normal*(StoneRadius+.025f);impactPoint=hit.point;impactNormal=hit.normal;FinishSentinelImpact();}
   else BeginReturn("strike blocked by scenery");return true;
  }
  projectile.position=next;
  if(striking&&Vector3.Distance(boundCollider.ClosestPoint(next),next)<=StoneRadius+.03f){impactPoint=contact;impactNormal=normal;FinishSentinelImpact();return true;}
  if(mode==StoneMode.Rebounding&&Vector3.Distance(next,waitPoint)<.015f){velocity=Vector3.zero;SetMode(StoneMode.Waiting);log("COMBAT TARGET_WAIT");}
  if(mode==StoneMode.Striking&&actionClock>1.2f)BeginReturn("strike lost contact");
  return true;
 }
 void FinishSentinelImpact(){
  var normal=boundTransform.TransformDirection(localNormal).normalized;
  projectile.position=boundTransform.TransformPoint(castLocal)+normal*(StoneRadius+.025f);
  Impact();velocity=Vector3.zero;actionClock=0;
  if(!TargetAlive()){BeginReturn("Sentinel defeated");return;}
  SetMode(StoneMode.Rebounding);
 }
}
}

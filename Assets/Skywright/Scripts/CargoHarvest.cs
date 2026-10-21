using UnityEngine;
namespace Skywright {
// Only the two explicitly identified tutorial cargo crates own this component.
public sealed class CargoHarvest : MonoBehaviour {
 public AirCrateState state;public Transform loot;float breakTime=-1;Vector3 lootHome;
 readonly CollectiblePull pull=new CollectiblePull();
 Transform[] fragments;Vector3[] starts;Quaternion[] turns;
 public void Restore(AirCrateState record,EarthConfig config){
  state=record;if(state.depleted){foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;foreach(var c in GetComponentsInChildren<Collider>())c.enabled=false;if(!state.collected)MakeLoot(config);}
 }
 void MakeLoot(EarthConfig config){
  if(loot||state.collected)return;lootHome=transform.position+Vector3.up*.3f;
  if(Physics.Raycast(transform.position+Vector3.up*1.2f,Vector3.down,out var hit,100,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore))lootHome=hit.point+Vector3.up*.3f;
  loot=Instantiate(config.wood,lootHome,Quaternion.identity,transform.parent).transform;
  foreach(var c in loot.GetComponentsInChildren<Collider>())c.enabled=false;
 }
 public void Hit(SkywrightGame game){
  if(state.depleted)return;state.damage++;game.MarkChanged();game.LogEvent("CRATE_HIT "+state.id+" damage="+state.damage);
  if(state.damage<3)return;state.depleted=true;breakTime=0;
  foreach(var c in GetComponentsInChildren<Collider>())c.enabled=false;
  var renderers=GetComponentsInChildren<MeshRenderer>();fragments=new Transform[renderers.Length];starts=new Vector3[renderers.Length];turns=new Quaternion[renderers.Length];
  for(int i=0;i<renderers.Length;i++){fragments[i]=renderers[i].transform;starts[i]=fragments[i].localPosition;turns[i]=fragments[i].localRotation;}
  game.PlayWorldSound(game.EarthTuning.crack,transform.position,.6f);game.Notify("Crate broken — collect its wood");
 }
 public void Tick(SkywrightGame game,float dt){
  if(breakTime>=0){breakTime+=dt;float t=Mathf.Clamp01(breakTime/.8f);
   for(int i=0;i<fragments.Length;i++){var f=fragments[i];float a=i*2.399f;f.localPosition=starts[i]+new Vector3(Mathf.Cos(a)*.6f*t,Mathf.Sin(t*Mathf.PI)*.35f-t*.18f,Mathf.Sin(a)*.6f*t);f.localRotation=turns[i]*Quaternion.Euler(t*55,t*(i%2==0?35:-35),t*30);}
   if(t>=1)Settle(game.EarthTuning);
  }
  if(!loot||state.collected)return;
  bool near=Vector3.Distance(game.controller.transform.position,lootHome)<game.EarthTuning.collectionRadius&&!Physics.Linecast(loot.position,game.cam.transform.position,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore);
  if(pull.Tick(game,loot,lootHome+Vector3.up*Mathf.Sin(Time.time*2)*.04f,dt)||near){state.collected=true;game.HarvestState.wood+=6;Destroy(loot.gameObject);loot=null;game.PlayPickup();game.MarkChanged();game.Notify("+6 wood");game.LogEvent("CRATE_AWARD "+state.id);}
 }
 public void Interrupt(){pull.Reset();if(loot)loot.position=lootHome;}
 public void Settle(EarthConfig config){if(!state.depleted)return;breakTime=-1;foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;MakeLoot(config);Interrupt();}
 void OnDestroy(){if(loot)Destroy(loot.gameObject);}
}
}

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Skywright {
public partial class SkywrightGame : MonoBehaviour {
 public Builder builder;public Camera cam;public CharacterController controller;
 public float yaw,pitch=12,sensitivity=.10f;float vertical,noticeUntil,inputDelay,qaNext;string notice="";
 readonly bool profile120=Array.IndexOf(Environment.GetCommandLineArgs(),"-profile120")>=0,noQAOutput=Array.IndexOf(Environment.GetCommandLineArgs(),"-noqaoutput")>=0;
 bool menu=true,palette,started,help,confirmNew,dirty,qa,artPreview;string savePath,qaPath;
 Transform piecesRoot;AudioSource sound;AudioClip click,pickup;float pickupNext,placementNext;AudioSource[] placementVoices;int placementVoice;bool focused=true;
 public EarthState HarvestState=>earth.state;public EarthConfig EarthTuning=>earth.config;
 public bool AirEquipped=>air!=null&&air.equipped;public bool SuctionActive=>air!=null&&air.Pulling;
 public void MarkChanged()=>dirty=true;public void LogEvent(string message)=>Record(message);
 public void PlayWorldSound(AudioClip clip,Vector3 position,float volume){sound.PlayOneShot(clip,volume);}
 public void InterruptDrops()=>earth?.InterruptDrops();
 public void RecoverToHome()=>Recover();
 public void StowForJourney(){earth?.Equip(false);air?.Equip(false);builder.enabled=false;builder.Hide();}
public AirConfig AirTuning=>air!=null?air.config:Resources.Load<AirConfig>("AirMagic/Tuning");
 bool aetherPanel;BuiltPiece inspectedAnchor;
 EarthTrial earth;bool earthMode;AirTrial air;bool airMode,airSelected;
 void Awake(){
  Application.SetStackTraceLogType(LogType.Log,StackTraceLogType.None);
  Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
  bool snapQA=Array.IndexOf(Environment.GetCommandLineArgs(),"-snapqa")>=0;
  bool earthQA=Array.IndexOf(Environment.GetCommandLineArgs(),"-earthqa")>=0;
  bool airQA=Array.IndexOf(Environment.GetCommandLineArgs(),"-airqa")>=0;
  bool visual=NaturalPreview.Active;
  qa=NaturalPreview.QA||airQA||earthQA||snapQA||Array.IndexOf(Environment.GetCommandLineArgs(),"-qa")>=0;
  artPreview=Array.IndexOf(Environment.GetCommandLineArgs(),"-artpreview")>=0;
  airMode=visual||airQA||Array.IndexOf(Environment.GetCommandLineArgs(),"-elemental")>=0||(!earthQA&&!snapQA&&!artPreview&&File.Exists(Path.Combine(Application.dataPath,"../elemental.preview")));
  earthMode=airMode||earthQA||Array.IndexOf(Environment.GetCommandLineArgs(),"-earthcasting")>=0||(!snapQA&&!artPreview&&File.Exists(Path.Combine(Application.dataPath,"../earthcasting.preview")));
  if(earthMode)artPreview=true;
  savePath=Path.Combine(Application.persistentDataPath,visual?(NaturalPreview.QA?"verification-visual.json":"visual-preview.json"):airMode?(airQA?"verification-elemental.json":"elemental-trial.json"):earthMode?(earthQA?"verification-earthcasting.json":"earthcasting-trial.json"):snapQA?"verification-snapping.json":qa?"verification-m15.json":artPreview?"art-preview.json":"home.json");
  qaPath=Path.Combine(Application.persistentDataPath,"qa-state.json");
  if(SentinelCombat.Active){qa=false;savePath=Path.Combine(Application.persistentDataPath,"sentinel-combat-unused.json");}
  sensitivity=PlayerPrefs.GetFloat("sensitivity",.10f);
  var world=new GameObject("Archipelago");World.Create(world.transform);if(visual)NaturalPreview.Environment(world.transform);else if(artPreview)ArtEnvironment.Apply(world.transform);
  piecesRoot=new GameObject("Player construction").transform;
  var p=new GameObject("First person");p.layer=6;p.transform.position=World.Spawn;controller=p.AddComponent<CharacterController>();
  controller.height=1.8f;controller.radius=.32f;controller.center=Vector3.up*.9f;controller.stepOffset=.32f;controller.skinWidth=.035f;controller.slopeLimit=50;
  var c=new GameObject("Player camera");c.transform.SetParent(p.transform,false);c.transform.localPosition=Vector3.up*1.65f;cam=c.AddComponent<Camera>();cam.tag="MainCamera";cam.fieldOfView=75;cam.nearClipPlane=.06f;cam.farClipPlane=800;if(NaturalPreview.Active)MountainEnvironment.Camera(cam);
  c.AddComponent<AudioListener>();sound=c.AddComponent<AudioSource>();
  click=BubbleAudio.Placement();pickup=BubbleAudio.Pickup();placementVoices=new AudioSource[3];for(int i=0;i<placementVoices.Length;i++)placementVoices[i]=c.AddComponent<AudioSource>();
  builder=new Builder(piecesRoot,cam,controller);builder.Notify=Notify;builder.Log=Record;
  if(earthMode){earth=new EarthTrial(this,()=>dirty=true,Record);builder.CanPay=earth.CanBuild;builder.Placed=earth.Pay;builder.Removing=earth.Refund;}
  if(airMode){earth.managedCrates=true;earth.Reset();air=new AirTrial(this,()=>dirty=true,Record);}
  Look();CursorState();Debug.Log("SKYWRIGHT SESSION "+DateTime.UtcNow.ToString("O")+" slot="+savePath);
 }
 float perfTime;int perfFrames;float perfWorst;
 void LateUpdate(){
  if(!qa||menu||palette||!started)return;
  perfTime+=Time.unscaledDeltaTime;perfFrames++;perfWorst=Mathf.Max(perfWorst,Time.unscaledDeltaTime);
  if(perfTime>=10){Debug.Log("PERF frames="+perfFrames+" seconds="+perfTime.ToString("F3")+" avgMs="+(perfTime*1000/perfFrames).ToString("F2")+" worstMs="+(perfWorst*1000).ToString("F2"));perfTime=0;perfFrames=0;perfWorst=0;}
 }
 void Update(){
  // Avoid rendering a full-rate island behind menus or while another app has focus.
  int cap=!focused?15:menu||palette||help||aetherPanel||!started?30:60;
  if(profile120)cap=120;
  if(Application.targetFrameRate!=cap)Application.targetFrameRate=cap;
  var k=Keyboard.current;if(k==null||Mouse.current==null)return;
  if(k.escapeKey.wasPressedThisFrame){
   if(aetherPanel)aetherPanel=false;else if(palette)palette=false;else if(help)help=false;else {menu=!menu;confirmNew=false;}
   CursorState();inputDelay=Time.unscaledTime+.15f;
  }
  if(!SentinelCombat.Active&&!menu&&!help&&!aetherPanel&&started&&(air==null||!air.Travelling)&&k.bKey.wasPressedThisFrame){palette=!palette;CursorState();inputDelay=Time.unscaledTime+.15f;}
  if(focused&&palette&&!menu&&!help&&started&&Time.unscaledTime>=inputDelay&&Mouse.current.rightButton.wasPressedThisFrame){
   earth?.Equip(false);air?.Equip(false);builder.enabled=true;palette=false;CursorState();inputDelay=Time.unscaledTime+.2f;
   builder.Hide();air?.Pause();Record("PALETTE_RMB_BUILD");WriteQA();return;
  }
  if(!focused||menu||palette||help||aetherPanel||!started||Time.unscaledTime<inputDelay){builder.Hide();earth?.Hide();air?.Pause();WriteQA();return;}
  if(SentinelCombat.Active&&SentinelCombat.Current.PlayerDown){earth?.Hide();air?.Pause();return;}
  if(!SentinelCombat.Active&&air?.aether!=null&&k.eKey.wasPressedThisFrame){inspectedAnchor=air.aether.AimedAnchor();if(inspectedAnchor){aetherPanel=true;air.Pause();earth?.Hide();builder.Hide();CursorState();inputDelay=Time.unscaledTime+.2f;WriteQA();return;}}
  Vector2 delta=Mouse.current.delta.ReadValue();yaw+=delta.x*sensitivity;pitch=Mathf.Clamp(pitch-delta.y*sensitivity,-85,85);Look();
  Vector2 move=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
  move=Vector2.ClampMagnitude(move,1);float speed=k.leftShiftKey.isPressed?7:4;
  if(air!=null&&air.Move(move,Time.deltaTime,out var windVelocity)){vertical=0;controller.Move(windVelocity*Time.deltaTime);}
  else {
   if(controller.isGrounded&&vertical<0)vertical=-2;
   if(controller.isGrounded&&k.spaceKey.wasPressedThisFrame){vertical=6;Record("JUMP");}
   vertical-=18*Time.deltaTime;
   controller.Move((controller.transform.TransformDirection(new Vector3(move.x,0,move.y))*speed+Vector3.up*vertical)*Time.deltaTime);
  }
  if(controller.transform.position.y< -25&&!(TutorialIsland.Active&&air!=null&&air.hovering))Recover();
  if(air!=null&&air.Travelling){if(k.f5Key.wasPressedThisFrame)Save();earth?.Tick(Time.deltaTime);air.Tick(Time.deltaTime);air.RefreshBubbles();WriteQA();return;}
  bool picking=(k.leftShiftKey.isPressed||k.rightShiftKey.isPressed)&&Mouse.current.middleButton.wasPressedThisFrame;
  if(earth!=null){
   if(air!=null&&k.qKey.wasPressedThisFrame){bool wasEquipped=earth.equipped||air.equipped;earth.Equip(false);air.Equip(false);airSelected=!airSelected;if(wasEquipped){if(airSelected)air.Equip(true);else earth.Equip(true);}Record("MAGIC_SELECT "+(airSelected?"air":"earth"));}
   if(k.tabKey.wasPressedThisFrame&&!k.leftShiftKey.isPressed&&!k.rightShiftKey.isPressed){bool value=!(earth.equipped||air!=null&&air.equipped);earth.Equip(value&&!airSelected);air?.Equip(value&&airSelected);if(value){builder.enabled=false;builder.Hide();}Record("MAGIC_EQUIP "+value);}
   if(SentinelCombat.Active){earth.externalInput=air!=null&&air.equipped;earth.Tick(Time.deltaTime);air?.Tick(Time.deltaTime);builder.Hide();return;}
   bool buildInput=k.fKey.wasPressedThisFrame||k.leftBracketKey.wasPressedThisFrame||k.rightBracketKey.wasPressedThisFrame||picking;
   for(int i=0;i<9;i++)if(k[(Key)((int)Key.Digit1+i)].wasPressedThisFrame)buildInput=true;
   if(buildInput){earth.Equip(false);air?.Equip(false);}
  }
  {
  if(k.fKey.wasPressedThisFrame){builder.enabled=!builder.enabled;Record("BUILD_MODE "+builder.enabled);}
  if(k.leftBracketKey.wasPressedThisFrame)builder.Select(builder.index-1);
  if(k.rightBracketKey.wasPressedThisFrame)builder.Select(builder.index+1);
  for(int i=0;i<9;i++)if(k[(Key)((int)Key.Digit1+i)].wasPressedThisFrame){builder.Select(i);builder.enabled=true;}
  }
  if(builder.enabled&&(earth==null||!earth.equipped)&&(air==null||!air.equipped)&&Mouse.current.rightButton.wasPressedThisFrame){palette=true;CursorState();inputDelay=Time.unscaledTime+.15f;builder.Hide();air?.Pause();WriteQA();return;}
  if(k.f5Key.wasPressedThisFrame){Save();WriteQA();return;}
  if(k.f9Key.wasPressedThisFrame){menu=true;CursorState();inputDelay=Time.unscaledTime+.15f;builder.Hide();earth?.Hide();air?.Pause();WriteQA();return;}
  if(earth!=null)earth.externalInput=air!=null&&air.equipped;
  earth?.Tick(Time.deltaTime);air?.Tick(Time.deltaTime);
  if((earth==null||!earth.equipped&&!earth.Busy)&&(air==null||!air.equipped))builder.Tick();else builder.Hide();
  air?.RefreshBubbles();WriteQA();
 }
 void Look(){controller.transform.rotation=Quaternion.Euler(0,yaw,0);cam.transform.localRotation=Quaternion.Euler(pitch,0,0);}
 void CursorState(){bool open=menu||palette||help||aetherPanel||!started;Cursor.lockState=open?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=open;}
 public void Notify(string s){if(s.EndsWith("placed")){if(Time.unscaledTime>=placementNext){placementNext=Time.unscaledTime+.07f;var voice=placementVoices[placementVoice++%placementVoices.Length];voice.Stop();voice.clip=click;voice.pitch=UnityEngine.Random.Range(.988f,1.012f);voice.volume=BubbleAudio.PlacementVolume;voice.Play();Record("SOUND placement-pop");}return;}notice=s;noticeUntil=Time.unscaledTime+3;}
 public void PlayPickup(){if(Time.unscaledTime<pickupNext)return;pickupNext=Time.unscaledTime+.09f;sound.pitch=UnityEngine.Random.Range(1-BubbleAudio.PitchVariation,1+BubbleAudio.PitchVariation);sound.PlayOneShot(pickup,BubbleAudio.PickupVolume);Record("SOUND pickup-pop");}
 void OnApplicationFocus(bool value){focused=value;if(!value){earth?.CancelCast("focus lost");air?.Pause();}inputDelay=Time.unscaledTime+.15f;}
 void Record(string s){Debug.Log("SKY "+s);if(s.StartsWith("PLACE")||s.StartsWith("REMOVE"))dirty=true;}
 void Resume(){aetherPanel=false;menu=false;palette=false;help=false;started=true;inputDelay=Time.unscaledTime+.2f;CursorState();}
 void Fresh(){
  builder.Clear();dirty=false;controller.enabled=false;controller.transform.position=World.Spawn;controller.enabled=true;yaw=0;pitch=12;vertical=0;Look();builder.enabled=false;if(earth!=null){earth.Reset();foreach(float z in new[]{-16f,-20f})builder.pieces.Add(Kit.Create("floor",new Vector3(0,.2f,z),Quaternion.identity,piecesRoot));builder.pieces.Add(Kit.Create("lantern",new Vector3(1.6f,.2f,-15),Quaternion.identity,piecesRoot));}
  if(air!=null){air.Reset();SeedAir();}
  if(NaturalPreview.Active){Load(Path.Combine(Application.streamingAssetsPath,"natural-sample.json"));return;}
  Resume();Notify(air!=null?"Elemental trial • Q selects Earth / Air • Tab equips • R hover":earth!=null?"Earthcasting trial: equip with Tab, summon with LMB":"Your island. Start with B to choose a piece.");Record("NEW_SESSION");
 }
 void SeedAir(bool workArea=true){
  // Stable authored demo identities, distinct from normal newly placed instance GUIDs.
  void Add(string type,Vector3 p,float angle,string id){if(builder.pieces.Exists(b=>b.id==id))return;var b=Kit.Create(type,p,Quaternion.Euler(0,angle,0),piecesRoot);b.id=id;builder.pieces.Add(b);}
  if(!TutorialIsland.Active)Add("wind-anchor",new Vector3(4,0,-17),0,"air-trial-start-anchor");
  if(workArea){
   Add("floor",new Vector3(0,.2f,-24),0,"air-trial-work-deck");
   Add("doorway",new Vector3(0,.2f,-22),180,"air-trial-work-doorway");
   Add("window",new Vector3(-2,.2f,-24),90,"air-trial-work-window");
   Add("roof",new Vector3(-2,3.2f,-24),90,"air-trial-work-roof");
  }
  Physics.SyncTransforms();
 }
 void Recover(){
  air?.RecoverPlayer();
  controller.enabled=false;Vector3 safe=World.Spawn;
  if(Physics.CheckCapsule(safe+Vector3.up*.4f,safe+Vector3.up*1.4f,.34f,~(1<<2))) {
   bool found=false;for(int z=-14;z<=14&&!found;z+=2)for(int x=-14;x<=14&&!found;x+=2){
    Vector3 trial=new Vector3(x,.3f,z);
    if(!Physics.CheckCapsule(trial+Vector3.up*.4f,trial+Vector3.up*1.4f,.34f,~(1<<2))){safe=trial;found=true;}
   }
  }
  controller.transform.position=safe;controller.enabled=true;vertical=0;Notify("Returned safely to the island");Record("RECOVER "+safe);
 }
 void Save(){
  if(SentinelCombat.Active){Notify("Combat trial is session-only; your island saves stay separate.");return;}
  try{
   air?.PrepareSave();var data=new SaveData{worldLayout=NaturalPreview.Active?TutorialIsland.Layout:"",player=controller.transform.position,yaw=yaw,pitch=pitch,earth=earth?.Snapshot(),air=air?.Snapshot()};
   foreach(var b in builder.pieces)data.pieces.Add(new PieceRecord{id=b.id,type=b.type,paidWood=b.paidWood,position=b.transform.localPosition,rotation=b.transform.localRotation});
   Persistence.Write(savePath,data);dirty=false;Notify("Saved "+data.pieces.Count+" pieces");Record("SAVE "+data.pieces.Count);
  }catch(Exception e){Notify("Save failed: "+e.Message);Debug.LogWarning("Save failed: "+e.Message);}
 }
 void Load(string path,bool importEarth=false){
  if(SentinelCombat.Active){Notify("Island loading is disabled in the combat trial.");return;}
  GameObject staging=null;
  try{
   SaveData data=Persistence.Read(path);
   if(NaturalPreview.Active&&data.worldLayout!=TutorialIsland.Layout&&data.pieces.Exists(r=>r.type!="wind-anchor"&&new Vector2(r.position.x,r.position.z).magnitude>38))throw new IOException("This older layout extends beyond the preserved home clearing. Original save kept. Use the M1.9 backup for that world, or explore the new tutorial sample.");
   if(NaturalPreview.Active&&data.worldLayout!=TutorialIsland.Layout&&data.worldLayout!=""&&data.worldLayout!="compact-v1")throw new IOException("Unknown island layout; source kept.");
   if(earthMode!=(data.earth!=null))throw new IOException("This save belongs to a different trial. Current world kept.");
   if(airMode){if(!importEarth&&!data.elementalTrial)throw new IOException("Use Import Earthcasting save — copy for an Earth trial.");if(importEarth&&data.elementalTrial)throw new IOException("Select an unchanged Earthcasting trial.");}
   else if(data.elementalTrial)throw new IOException("Open this save in the Elemental preview.");
   staging=new GameObject("Validated load");staging.SetActive(false);
   var loaded=new List<BuiltPiece>();foreach(var r in data.pieces){if(TutorialIsland.Active&&r.type=="wind-anchor")continue;var b=Kit.Create(r.type,r.position,r.rotation,staging.transform);b.id=r.id;b.paidWood=r.paidWood;loaded.Add(b);}
   if(earth!=null)earth.Restore(data.earth);
   if(air!=null)air.Restore(importEarth?AirTrial.NewState():data.air);
   foreach(var b in builder.pieces)b.gameObject.SetActive(false);
   builder.Clear();
   foreach(var b in loaded){b.transform.SetParent(piecesRoot,true);builder.pieces.Add(b);}
   Destroy(staging);staging=null;Physics.SyncTransforms();
   if(importEarth)SeedAir(false);
   controller.enabled=false;controller.transform.position=data.player;controller.enabled=true;yaw=data.yaw;pitch=data.pitch;vertical=0;Look();
   controller.enabled=false;
   bool unsafePosition=data.player.y< -1||(air==null||!air.descending)&&!Physics.Raycast(data.player+Vector3.up*.2f,Vector3.down,6,~(1<<2))||Physics.CheckCapsule(data.player+Vector3.up*.4f,data.player+Vector3.up*1.4f,.3f,~(1<<2));
   controller.enabled=true;if(unsafePosition)Recover();
   dirty=importEarth;Resume();Notify(importEarth?"Earthcasting imported as a copy • F5 saves the Elemental slot":"Loaded "+builder.pieces.Count+" pieces");Record("LOAD "+data.pieces.Count);
   if(qa){var audit=new SaveData{savedUtc=data.savedUtc,player=controller.transform.position,yaw=yaw,pitch=pitch};
    foreach(var b in builder.pieces)audit.pieces.Add(new PieceRecord{id=b.id,type=b.type,paidWood=b.paidWood,position=b.transform.localPosition,rotation=b.transform.localRotation});
    File.WriteAllText(Path.Combine(Application.persistentDataPath,"m15-loaded-audit.json"),JsonUtility.ToJson(audit,true));
   }
  }catch(Exception e){if(staging)Destroy(staging);Notify("Load failed: "+e.Message);Debug.LogWarning("Load failed: "+e.Message);}
 }
 [Serializable] class QAState {public Vector3 position,look,ghost;public string piece,reason,snapMode,snapLabel,target;public float anchorError;public Vector3 snapPoint;public bool valid,menu,palette;public int count,rotation;public float reach;public bool snap,free;public Vector3 ghostEuler;public EarthState earth;public string earthTarget,earthPhase,earthHint,boundEarthTarget;public int earthStoneCount,earthBounces;public bool earthEquipped,earthValid,earthMark;public float collectionDistance;public AirState air;public bool airEquipped;public string airHeld,airTarget,airMotion,crateBlockers;public float holdDistance,holdHeight,gustClock;public Vector3 projectile,projectileVelocity;public float earthTravelled,returnTravelled,windGain,earthFlightAge,earthPlayerDistance;public int windSample;public List<AirCrateState> movables;public AetherObservation aether;public bool aetherPanel,travelling;public float rush;}
 void WriteQA(){
  if(!qa||noQAOutput||Time.unscaledTime<qaNext)return;qaNext=Time.unscaledTime+.4f;
  try{File.WriteAllText(qaPath,JsonUtility.ToJson(new QAState{travelling=air!=null&&air.Travelling,rush=air!=null?air.Rush:0,aether=air?.aether?.Observe(),aetherPanel=aetherPanel,earthFlightAge=earth!=null?earth.FlightAge:0,earthPlayerDistance=earth!=null?earth.PlayerDistance:0,boundEarthTarget=earth?.BoundTarget,earthStoneCount=earth!=null?earth.StoneCount:0,earthBounces=earth!=null?earth.BounceCount:0,projectileVelocity=earth!=null?earth.ProjectileVelocity:Vector3.zero,returnTravelled=earth!=null?earth.ReturnTravelled:0,windGain=air!=null?air.WindGain:0,windSample=air!=null?air.WindSample:0,projectile=earth!=null?earth.ProjectilePosition:Vector3.zero,earthTravelled=earth!=null?earth.Travelled:0,movables=air?.ObserveMovables(),air=air?.Observe(),airEquipped=air!=null&&air.equipped,airHeld=air!=null&&air.held?air.held.id:"",airTarget=air?.target,airMotion=air?.motion,crateBlockers=air!=null?string.Join(" / ",air.crates.ConvertAll(c=>c.id+":"+c.blocked)):"",holdDistance=air!=null?air.holdDistance:0,holdHeight=air!=null?air.holdHeight:0,gustClock=air!=null?air.gustClock:-1,earth=earth?.state,earthTarget=earth?.targetId,earthPhase=earth?.phase,earthHint=earth?.hint,earthEquipped=earth!=null&&earth.equipped,earthMark=earth!=null&&earth.MarkVisible,earthValid=earth!=null&&earth.targetValid,collectionDistance=earth!=null?earth.collectDistance:-1,position=controller.transform.position,look=new Vector3(pitch,yaw,0),ghost=builder.ghost?builder.ghost.transform.position:Vector3.zero,piece=Kit.Ids[builder.index],reason=builder.reason,snapMode=builder.SnapMode,snapLabel=builder.snapLabel,target=builder.target?builder.target.type:"",anchorError=builder.anchorError,snapPoint=builder.snapPoint,valid=builder.valid,menu=menu,palette=palette,count=builder.pieces.Count,reach=builder.reach,snap=builder.snapped,rotation=builder.rotationSteps*45,free=builder.free,ghostEuler=builder.ghost?builder.ghost.transform.eulerAngles:Vector3.zero}));}catch(IOException){}
 }
 void OnApplicationQuit(){PlayerPrefs.SetFloat("sensitivity",sensitivity);Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
}
}

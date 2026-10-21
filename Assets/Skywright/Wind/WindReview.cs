using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Unity.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Skywright {
// Opt-in isolated review only. Normal previews do not run this harness.
public sealed class WindReview : MonoBehaviour {
 SkywrightGame game;AirTrial air;Mouse mouse;Keyboard keyboard;string folder;readonly List<string> checks=new();
 readonly Stopwatch watch=new();readonly FrameTiming[] timing=new FrameTiming[1];
 Vector3 eye=new Vector3(8,1.7f,-17),look=new Vector3(8,.65f,-12);int frames;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){
  var args=Environment.GetCommandLineArgs();foreach(string flag in new[]{"-windverify","-windperf","-windtour","-windqa"})if(Array.IndexOf(args,flag)>=0){new GameObject("Explicit wind review").AddComponent<WindReview>();break;}
 }
 void Input(bool left=false,bool right=false,bool shift=false){InputSystem.QueueStateEvent(mouse,new MouseState{buttons=(ushort)((left?1:0)|(right?2:0))});InputSystem.QueueStateEvent(keyboard,shift?new KeyboardState(Key.LeftShift):new KeyboardState());InputSystem.Update();mouse.MakeCurrent();keyboard.MakeCurrent();}
 void Step(bool left=false,bool right=false,bool shift=false,float dt=1f/60){Input(left,right,shift);Physics.SyncTransforms();air.Tick(dt);}
 void Check(bool value,string label){if(!value)throw new Exception("WIND FAIL "+label);checks.Add(label);UnityEngine.Debug.Log("WIND PASS "+label);}
 void Reset(){air.Reset();game.CombatEarth.Equip(false);air.Equip(true);Position(eye,look);Step();}
 void MoreProps(){
  for(int i=0;i<3;i++){var c=Instantiate(air.config.crate,new Vector3(6.8f+i*1.2f,.04f,-9.5f),Quaternion.identity,air.crates[0].transform.parent).GetComponent<AirCrate>();c.id="wind-review-"+i;c.home=c.safe=c.transform.position;air.crates.Add(c);}
 }
 void Position(Vector3 from,Vector3 to){game.controller.enabled=false;game.controller.transform.position=from-Vector3.up*1.65f;game.controller.enabled=true;game.cam.transform.position=from;game.cam.transform.LookAt(to);Physics.SyncTransforms();}
 IEnumerator Start(){
  yield return null;yield return null;game=FindAnyObjectByType<SkywrightGame>();air=game.CombatAir;
  var args=Environment.GetCommandLineArgs();string mode="";foreach(string f in new[]{"-windverify","-windperf","-windtour","-windqa"})if(Array.IndexOf(args,f)>=0){mode=f;folder=Path.GetFullPath(args[Array.IndexOf(args,f)+1]);break;}
  Directory.CreateDirectory(folder);Application.runInBackground=true;
  long allocationStart=GC.GetAllocatedBytesForCurrentThread();var allocationProbe=new byte[8192];long allocationDelta=GC.GetAllocatedBytesForCurrentThread()-allocationStart;GC.KeepAlive(allocationProbe);
  File.WriteAllText(Path.Combine(folder,"allocation-counter.txt"),"8192-byte calibration allocation measured: "+allocationDelta+" bytes. A zero result means this Mono counter is unavailable, not allocation-free execution.");
  if(mode=="-windqa"){StartCoroutine(NativeQA());yield break;}
  InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
  game.enabled=false;if(SentinelCombat.Active){SentinelCombat.Current.enabled=false;SentinelCombat.Current.Enemy.SetPaused(true);}
  game.CombatVerificationPause(false);game.CombatEarth.Equip(false);mouse=InputSystem.AddDevice<Mouse>();keyboard=InputSystem.AddDevice<Keyboard>();Input();Reset();
  Application.targetFrameRate=60;yield return new WaitForSecondsRealtime(2);
  if(mode=="-windverify"){
   try{Verify();File.WriteAllText(Path.Combine(folder,"checks.txt"),string.Join("\n",checks));}
   catch(Exception e){File.WriteAllText(Path.Combine(folder,"FAILED.txt"),e.ToString());UnityEngine.Debug.LogException(e);Application.Quit(2);yield break;}
  }else if(mode=="-windperf")yield return Performance();
  else yield return Tour();
  InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(keyboard);UnityEngine.Debug.Log("WIND_REVIEW_COMPLETE "+mode+" checks="+checks.Count);Application.Quit();
 }
 void Verify(){
  Reset();Step(true);Check(air.Pulling&&!air.Pushing,"LMB starts suction");
  Step(true,false,true);Check(air.Pulling&&!air.held,"Shift pressed during suction preserves suction, never grabs");
  Step(true);Check(air.Pulling,"Shift release during suction preserves gesture");
  Step(true,true);Check(air.Pulling&&!air.Pushing,"LMB-first owns simultaneous buttons");
  Step(false,true);Check(!air.Pulling&&!air.Pushing,"Releasing owner with other button held requires neutral");
  Step();Step(false,true);Check(air.Pushing&&!air.Pulling,"RMB starts continuous push");
  Step(true,true);Check(air.Pushing&&!air.Pulling,"RMB-first owns simultaneous buttons");
  Step();Step(true,true);Check(air.Pushing&&!air.Pulling,"Simultaneous fresh buttons resolve to push");
  Reset();Step(true,false,true);var held=air.held;Check(held&&!air.Pulling&&!air.Pushing,"Shift LMB grabs eligible crate and consumes suction");
  for(int i=0;i<30;i++)Step(true,false,true);Check(air.held==held,"Holding grab chord does not toggle repeatedly");
  Step(true);Check(air.held==held&&!air.Pulling,"Releasing Shift leaves object held without suction");
  Step();Step(false,true);Check(air.held==held&&!air.Pushing,"RMB cannot launch or push controlled object");
  Step();Step(true);Check(air.held==held&&!air.Pulling,"Plain LMB suppressed while holding");
  Step();Step(true,false,true);Check(!air.held&&!air.Pulling&&!air.Pushing,"Second grab click safely releases without casting");
  Step(true);Check(!air.Pulling,"Consumed release click remains consumed when Shift lifted");
  Reset();Step(true,false,true);air.Pause();Check(!air.held&&!air.Pulling&&air.Presentation.SuctionFlow.LiveCount==0,"Pause releases object and clears visual state");
  Step(true,false,true);Check(!air.held&&!air.Pulling,"Pause requires neutral before another grab or cast");
  Step();Step(true,false,true);Check(air.held,"Fresh click rearms after pause");air.Equip(false);Check(!air.held,"Stowing or switching safely releases");air.Equip(true);Step(true);Check(!air.Pulling,"Reequip with held input remains blocked");Step();Step(true);Check(air.Pulling,"Fresh attack after equip works");
  air.Pause();Step(true);Check(!air.Pulling,"Held input cannot resume after cancellation");Step();Step(false,true);Check(air.Pushing,"RMB rearms after cancellation");
  Reset();Position(eye,eye+Vector3.down*4);Step(true,false,true);Check(!air.held,"Terrain is not grabbable");
  Reset();Position(eye,air.aether.Primary().transform.position+Vector3.up);Step(true,false,true);Check(!air.held,"Fixed anchor is not grabbable");
  Reset();var crate=air.crates[0];var start=crate.Center;for(int i=0;i<60;i++)Step(true);Check(Vector3.Distance(crate.Center,eye)<Vector3.Distance(start,eye),"Suction moves eligible crate toward player");
  Reset();crate=air.crates[0];start=crate.Center;for(int i=0;i<60;i++)Step(false,true);Check(Vector3.Distance(crate.Center,eye)>Vector3.Distance(start,eye),"Push moves eligible crate away");
  Reset();var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(8,1,-14.5f);wall.transform.localScale=new Vector3(4,3,.25f);Physics.SyncTransforms();Step(true,false,true);Check(!air.held,"Cover blocks grabbing");Step();start=air.crates[0].Center;for(int i=0;i<60;i++)Step(true);Check(Vector3.Distance(start,air.crates[0].Center)<.1f,"Cover blocks suction");wall.SetActive(false);Destroy(wall);
  Reset();for(int i=0;i<120;i++)Step(true);Check(air.Presentation.SuctionFlow.LiveCount>8&&air.Presentation.PushFlow.LiveCount==0,"Suction has bounded layered geometry and push is inactive");
  Step();for(int i=0;i<60;i++)Step();Check(air.Presentation.SuctionFlow.LiveCount==0,"Suction cosmetic tail fully expires");
  for(int i=0;i<120;i++)Step(false,true);Check(air.Presentation.PushFlow.LiveCount>8&&air.Presentation.SuctionFlow.LiveCount==0,"Push has bounded distinct geometry");
  air.Pause();Check(air.Presentation.PushFlow.LiveCount==0,"Cancellation immediately clears push");
  Reset();var bubble=air.aether.config.bubbles[0].position;Position(bubble+Vector3.back*3,bubble);for(int i=0;i<150;i++)Step(true);Check(air.aether.state.balance>0,"Suction still collects authored Aether through its existing ledger");
  Reset();int wood=game.HarvestState.wood;var cargo=air.crates[0].GetComponent<CargoHarvest>();for(int i=0;i<3;i++)cargo.Hit(game);for(int i=0;i<60;i++)Step();Position(eye,cargo.loot.position);for(int i=0;i<180;i++)Step(true);Check(cargo.state.collected&&game.HarvestState.wood==wood+6,"Crate wood drops are suction-collected exactly once beyond auto-pickup distance");for(int i=0;i<30;i++)Step(true);Check(game.HarvestState.wood==wood+6,"Sustained suction cannot duplicate wood awards");
  Reset();var earth=game.CombatEarth;var saved=JsonUtility.FromJson<EarthState>(JsonUtility.ToJson(earth.state));var record=saved.nodes.Find(n=>n.kind=="outcrop");record.damage=earth.config.outcropHits;record.depleted=true;record.collected=false;earth.Restore(saved);earth.Equip(false);var node=earth.nodes.Find(n=>n.state.id==record.id);int stone=earth.state.stone;Position(node.lootGround+Vector3.up*1.65f+Vector3.back*5,node.loot.position);for(int i=0;i<180;i++){Step(true);earth.externalInput=true;earth.Tick(1f/60);}Check(node.state.collected&&earth.state.stone==stone+earth.config.outcropYield,"Limestone drops retain suction collection and exact stone yield");
  Reset();MoreProps();for(int i=0;i<120;i++)Step(false,true);Check(air.crates.Count==5,"Several eligible objects remain bounded during sustained push");
  Reset();var bench=Kit.Create("bench",new Vector3(8,.04f,-12),Quaternion.identity,null);air.crates[0].gameObject.SetActive(false);game.builder.pieces.Add(bench);Step();Position(eye,bench.transform.position+Vector3.up*.45f);Step(true,false,true);Check(air.held&&air.held.furniture==bench,"Existing furniture eligibility and control remain intact");air.Release();game.builder.pieces.Remove(bench);bench.gameObject.SetActive(false);Destroy(bench.gameObject);
  Check(air.Presentation.PushFlow.VertexBudget==2560&&air.Presentation.SuctionFlow.VertexBudget==2560,"Each spell has fixed 2560 vertex storage");
 }
 IEnumerator Performance(){
  // Warm scene/shaders first. No captures or QA-file output during sampling.
  var rows=new List<string>{"mode,frame,wall_ms,cpu_ms,gpu_ms,air_tick_ms,air_alloc_bytes,flow_ms,flow_alloc_bytes,live_wisps"};
  Application.targetFrameRate=-1;AirEffects.MeasureFlow=true;
  File.WriteAllText(Path.Combine(folder,"hardware.txt"),SystemInfo.processorType+"\n"+SystemInfo.graphicsDeviceName+"\n"+Screen.width+"x"+Screen.height+" vSync="+QualitySettings.vSyncCount+" uncapped; unchanged rendering quality");
  foreach(string mode in new[]{"disabled","legacy-suction","legacy-push","suction","push"}){
   Reset();MoreProps();bool push=mode.Contains("push");AirEffects.ReviewView=mode=="disabled"?AirEffects.FlowView.Disabled:mode.StartsWith("legacy")?AirEffects.FlowView.Legacy:AirEffects.FlowView.Upgraded;
   double start=Time.realtimeSinceStartupAsDouble;int count=0;double duration=mode=="suction"||mode=="push"?65:25;
   while(Time.realtimeSinceStartupAsDouble-start<duration){
    // Matched static camera. Reset both props to identical positions each frame.
    for(int i=0;i<air.crates.Count;i++){air.crates[i].transform.position=i<2?AirTrial.Homes[i]:air.crates[i].home;air.crates[i].velocity=Vector3.zero;}
    Input(!push,push);Physics.SyncTransforms();long before=GC.GetAllocatedBytesForCurrentThread();watch.Restart();air.Tick(Mathf.Min(Time.unscaledDeltaTime,.05f));watch.Stop();long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
    FrameTimingManager.CaptureFrameTimings();FrameTimingManager.GetLatestTimings(1,timing);
    if(Time.realtimeSinceStartupAsDouble-start>5)rows.Add(string.Join(",",mode,count,Time.unscaledDeltaTime*1000,timing[0].cpuFrameTime,timing[0].gpuFrameTime,watch.Elapsed.TotalMilliseconds,allocated,air.Presentation.FlowMilliseconds,air.Presentation.FlowAllocated,air.Presentation.SuctionFlow.LiveCount+air.Presentation.PushFlow.LiveCount));
    count++;yield return null;
   }
   UnityEngine.Debug.Log("WIND_PERF_PHASE "+mode+" frames="+count);
  }
  File.WriteAllLines(Path.Combine(folder,"frames.csv"),rows);AirEffects.MeasureFlow=false;AirEffects.ReviewView=AirEffects.FlowView.Upgraded;Application.targetFrameRate=60;
 }
 IEnumerator Tour(){
  Application.targetFrameRate=60;
  foreach(string mode in Array.IndexOf(Environment.GetCommandLineArgs(),"-surfaceonly")>=0?new[]{"surface"}:Array.IndexOf(Environment.GetCommandLineArgs(),"-finalflow")>=0?new[]{"suction","push","grab","switch","surface"}:new[]{"legacy-suction","suction","legacy-push","push","grab"}){
   Reset();bool push=mode.Contains("push"),grab=mode=="grab";AirEffects.ReviewView=mode.StartsWith("legacy")?AirEffects.FlowView.Legacy:AirEffects.FlowView.Upgraded;
   GameObject wall=null;if(mode=="surface"){wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("NaturalAero/Limestone");wall.transform.position=new Vector3(8,1.5f,-13);wall.transform.localScale=new Vector3(4,3,.25f);Physics.SyncTransforms();}
   string dest=Path.Combine(folder,mode);Directory.CreateDirectory(dest);frames=0;
   // Capture actual rendered frames at 15 fps; explicit scripted input, not native input.
   float time=0,next=0;while(time<12){
    float dt=1f/60;time+=dt;Vector3 at=eye;Vector3 to=look;
    if(time>4&&time<8){float t=time-4;at+=new Vector3(Mathf.Sin(t*2)*.6f,Mathf.Sin(t)*.18f,t*.3f);to+=new Vector3(Mathf.Sin(t*3)*3,Mathf.Sin(t*2)*1.7f,0);}
    Position(at,to);
    if(grab){bool click=time>.4f&&time<.44f||time>8&&time<8.04f;Step(click,false,click,dt);}
    else if(mode=="switch")Step(time>.5f&&time<4||time>8.3f&&time<10,time>4.3f&&time<8,false,dt);
    else Step(time>.5f&&time<9&&!push&&mode!="surface",time>.5f&&time<9&&(push||mode=="surface"),false,dt);
    yield return null;
    if(time>=next){Capture(Path.Combine(dest,(frames++).ToString("D4")+".tga"));next+=1f/15;}
   }
   if(wall){wall.SetActive(false);Destroy(wall);}
   UnityEngine.Debug.Log("WIND_TOUR "+mode+" frames="+frames);
  }
  AirEffects.ReviewView=AirEffects.FlowView.Upgraded;
 }
 void Capture(string path){
  var rt=RenderTexture.GetTemporary(1280,720,24,RenderTextureFormat.ARGB32);var prior=game.cam.targetTexture;var active=RenderTexture.active;
  RenderPipeline.SubmitRenderRequest(game.cam,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();using(var output=new BinaryWriter(File.Create(path))){byte[] h=new byte[18];h[2]=2;h[12]=0;h[13]=5;h[14]=208;h[15]=2;h[16]=24;output.Write(h);var pixels=image.GetPixels32();var bytes=new byte[pixels.Length*3];for(int i=0;i<pixels.Length;i++){bytes[i*3]=pixels[i].b;bytes[i*3+1]=pixels[i].g;bytes[i*3+2]=pixels[i].r;}output.Write(bytes);}Destroy(image);game.cam.targetTexture=prior;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);
 }
 IEnumerator NativeQA(){
  // Read-only diagnostics plus a reproducible initial pose. All subsequent controls are native input.
  game.CombatVerificationPause(false);game.WindReviewSelectAir();
  var state=new AetherState{tier=3,balance=0};for(int i=0;i<36;i++)state.collected.Add(air.aether.config.bubbles[i].id);air.aether.Restore(state);
  Position(eye,look);game.yaw=game.cam.transform.eulerAngles.y;game.pitch=game.cam.transform.eulerAngles.x;
  while(true){
   File.WriteAllText(Path.Combine(folder,"native-state.json"),JsonUtility.ToJson(new NativeState{pull=air.Pulling,push=air.Pushing,held=air.held?air.held.id:"",gesture=air.Gesture.ToString(),position=game.controller.transform.position,paused=game.CombatPaused,equipped=air.equipped,windGain=air.WindGain,hovering=air.hovering,rush=air.Rush,yaw=game.yaw,pitch=game.pitch,holdDistance=air.holdDistance,holdHeight=air.holdHeight}));yield return new WaitForSecondsRealtime(.1f);
  }
 }
 [Serializable] class NativeState{public bool pull,push,paused,equipped,hovering;public string held,gesture;public Vector3 position;public float windGain,rush,yaw,pitch,holdDistance,holdHeight;}
}
}

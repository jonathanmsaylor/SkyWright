using System;
using System.Globalization;
using System.IO;
using Unity.Profiling;
using UnityEngine;
namespace Skywright {
// Opt-in observation only. No game state or input commands are accepted.
public sealed class PreviewPerformance : MonoBehaviour {
 readonly FrameTiming[] timing=new FrameTiming[1];
 ProfilerRecorder allocations,draws,triangles;StreamWriter output;int frames;double elapsed,cpu,render,gpu,bytes;long maxBytes,lastAllocated;float worst;int timingFrames;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void StartProbe(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-profilepreview")>=0)new GameObject("Preview performance observer").AddComponent<PreviewPerformance>();}
 void Start(){
  allocations=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame");
  draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Batches Count");triangles=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Triangles Count");lastAllocated=GC.GetAllocatedBytesForCurrentThread();
  output=new StreamWriter(Path.Combine(Application.dataPath,"../preview-performance.csv"),false);output.AutoFlush=true;
  output.WriteLine("time,width,height,focused,cap,frames,frameMs,worstMs,cpuMainMs,cpuRenderMs,gpuMs,allocBytesPerFrame,maxAllocBytes,draws,triangles,timingSamples");
  Debug.Log("PROFILE hardware="+SystemInfo.processorType+" GPU="+SystemInfo.graphicsDeviceName+" timings="+FrameTimingManager.IsFeatureEnabled()+" threadAllocationCounter="+(lastAllocated>0));
 }
 void LateUpdate(){
  FrameTimingManager.CaptureFrameTimings();if(FrameTimingManager.GetLatestTimings(1,timing)>0){cpu+=timing[0].cpuMainThreadFrameTime;render+=timing[0].cpuRenderThreadFrameTime;gpu+=timing[0].gpuFrameTime;timingFrames++;}
  frames++;elapsed+=Time.unscaledDeltaTime;worst=Mathf.Max(worst,Time.unscaledDeltaTime);long allocated=GC.GetAllocatedBytesForCurrentThread();long b=Math.Max(0,allocated-lastAllocated);lastAllocated=allocated;bytes+=b;maxBytes=Math.Max(maxBytes,b);
  if(elapsed<2)return;double n=Math.Max(1,timingFrames);
  output.WriteLine(string.Format(CultureInfo.InvariantCulture,"{0:F2},{1},{2},{3},{4},{5},{6:F3},{7:F3},{8:F3},{9:F3},{10:F3},{11:F0},{12},{13},{14},{15}",Time.realtimeSinceStartup,Screen.width,Screen.height,Application.isFocused,Application.targetFrameRate,frames,elapsed*1000/frames,worst*1000,cpu/n,render/n,gpu/n,lastAllocated>0?bytes/frames:-1,lastAllocated>0?maxBytes:-1,draws.Valid?draws.LastValue:-1,triangles.Valid?triangles.LastValue:-1,timingFrames));
  frames=timingFrames=0;elapsed=cpu=render=gpu=bytes=0;worst=0;maxBytes=0;
 }
 void OnDestroy(){allocations.Dispose();draws.Dispose();triangles.Dispose();output?.Dispose();}
}
}

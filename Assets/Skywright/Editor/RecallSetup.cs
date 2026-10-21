using System;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Skywright.Editor {
public static class RecallSetup {
 public static void Build(){
  const string dir="Evidence/EarthRecall";Directory.CreateDirectory(dir);
  var e=Resources.Load<EarthConfig>("Earthcasting/Tuning");e.gravity=9.81f;e.recallSpeed=26;e.recallAcceleration=65;e.maxRecallTime=12;e.maxRecallDistance=280;EditorUtility.SetDirty(e);
  var a=Resources.Load<AirConfig>("AirMagic/Tuning");a.windVolume=.35f;EditorUtility.SetDirty(a);
  string report="Original runtime synthesis exports; PCM16 WAV, no third-party source.\n";
  foreach(var clip in new[]{BubbleAudio.Pickup(),BubbleAudio.Placement(),SoftWindAudio.CreateLoop()}){
   string name=clip.name.Contains("pickup")?"pickup-bubble":clip.name.Contains("placement")?"placement-bubble":"continuous-wind";
   var samples=new float[clip.samples];clip.GetData(samples,0);double energy=0;float peak=0,maxStep=0;
   for(int i=0;i<samples.Length;i++){if(!float.IsFinite(samples[i]))throw new Exception("Nonfinite audio");energy+=samples[i]*samples[i];peak=Mathf.Max(peak,Mathf.Abs(samples[i]));if(i>0)maxStep=Mathf.Max(maxStep,Mathf.Abs(samples[i]-samples[i-1]));}
   if(peak>=.99f)throw new Exception("Audio clipping");
   float seam=Mathf.Abs(samples[0]-samples[samples.Length-1]);
   if(name=="continuous-wind"&&seam>maxStep)throw new Exception("Wind seam larger than ordinary sample changes");
   report+=name+": duration="+clip.length+" RMS="+Math.Sqrt(energy/samples.Length).ToString("F6")+" peak="+peak.ToString("F6")+" seam="+seam.ToString("F6")+" maxAdjacent="+maxStep.ToString("F6")+"\n";
   using(var w=new BinaryWriter(File.Create(dir+"/"+name+".wav"))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+samples.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(clip.frequency);w.Write(clip.frequency*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(samples.Length*2);foreach(float x in samples)w.Write((short)Mathf.RoundToInt(x*32767));}
   UnityEngine.Object.DestroyImmediate(clip);
  }
  File.WriteAllText(dir+"/audio-analysis.txt",report);AssetDatabase.SaveAssets();BuildElemental.Build();
 }
}
}

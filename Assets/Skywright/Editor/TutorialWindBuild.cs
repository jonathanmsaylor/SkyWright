using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Skywright.Editor {
public static class TutorialWindBuild {
 public static void RefreshAnchorAndBuild(){AetherAuthor.Author();VerifyAndBuild();}
 public static void AuthorAndBuild(){CompactIslandAuthor.Author();AetherAuthor.Author();VerifyAndBuild();}
 public static void VerifyAndBuild(){
  Directory.CreateDirectory("Evidence/TutorialWind");
  AetherSaveChecks.Run();AirSaveChecks.Run();EarthSaveChecks.Run();
  var c=Resources.Load<AetherConfig>("CompactIsland/Aether");
  if(c.suctionRange!=8||c.bubbles.Length!=56)throw new Exception("Tutorial tuning drift");
  for(int i=0;i<4;i++)if(Mathf.Abs(c.stages[i].radius-c.islandRadii.x*(i+1)/4f)>.001f)throw new Exception("Dome stages must be island relative");
  int inside=0,oldInside=0;float worst=0;
  for(int z=-50;z<166;z++)for(int x=-170;x<104;x++){
   float u=(x+32)/130f,v=(z-58)/104f,a=Mathf.Atan2(v,u);if(Mathf.Sqrt(u*u+v*v)<1+.035f*Mathf.Sin(a*3+1)+.022f*Mathf.Sin(a*7-.4f))oldInside++;
   if(!CompactIslandAuthor.Inside(x,z))continue;inside++;
   var d=new Vector3(x,CompactIslandAuthor.Height(x,z)+2,z)-c.islandCenter;
   float ratio=d.x*d.x/(c.islandRadii.x*c.islandRadii.x)+d.y*d.y/(c.islandRadii.y*c.islandRadii.y)+d.z*d.z/(c.islandRadii.z*c.islandRadii.z);worst=Mathf.Max(worst,ratio);
  }
  float areaRatio=inside/(float)oldInside;if(areaRatio<.23f||areaRatio>.27f||worst>=1)throw new Exception("Island area or final dome coverage invalid");
  var state=AirTrial.NewState();state.crates[0].damage=3;state.crates[0].depleted=true;state.crates[0].collected=true;AirTrial.Validate(state);
  var copy=JsonUtility.FromJson<AirState>(JsonUtility.ToJson(state));if(!copy.crates[0].depleted||!copy.crates[0].collected)throw new Exception("Crate depletion lost");
  state.crates[1].collected=true;bool rejected=false;try{AirTrial.Validate(state);}catch(IOException){rejected=true;}if(!rejected)throw new Exception("Impossible crate reward accepted");
  var island=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("MountainAero/Island"));
  try{
   Physics.SyncTransforms();
   if(!Physics.Raycast(c.routeEnd+Vector3.up*2,Vector3.down,out var ground,4)||ground.normal.y<.9f)throw new Exception("No safe destination landing");
   for(int i=0;i<128;i++){
    var from=TutorialIsland.RoutePoint(c,i/128f)+Vector3.up*.9f;var to=TutorialIsland.RoutePoint(c,(i+1)/128f)+Vector3.up*.9f;var delta=to-from;
    if(Physics.CapsuleCast(from+Vector3.up*.55f,from-Vector3.up*.55f,.33f,delta.normalized,delta.magnitude,~(1<<2),QueryTriggerInteraction.Ignore))throw new Exception("Travel corridor obstructed at "+i);
   }
  }finally{UnityEngine.Object.DestroyImmediate(island);}
  File.WriteAllText("Evidence/TutorialWind/contracts.txt","Area ratio to M1.9: "+areaRatio+"\nFinal dome radius: "+c.islandRadii+"\nWorst terrain + player coverage ratio: "+worst+"\nQuarter-radius stages, unique preserved IDs, ledgers, crate depletion roundtrip/rejection, landing and swept travel corridor pass.\n");
  Export(BubbleAudio.Pickup(),"pickup-pop.wav");Export(BubbleAudio.Ding(),"solid-ding.wav");Export(SoftWindAudio.CreateLoop(),"flight-wind.wav");
  MountainAeroAuthor.Build();
 }
 static void Export(AudioClip clip,string file){
  var samples=new float[clip.samples*clip.channels];clip.GetData(samples,0);using(var w=new BinaryWriter(File.Create("Evidence/TutorialWind/"+file))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+samples.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)clip.channels);w.Write(clip.frequency);w.Write(clip.frequency*clip.channels*2);w.Write((short)(clip.channels*2));w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(samples.Length*2);foreach(var s in samples)w.Write((short)(Mathf.Clamp(s,-1,1)*32767));}
 }
}
}

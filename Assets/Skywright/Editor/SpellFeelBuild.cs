using System;
using System.IO;
using UnityEngine;
namespace Skywright.Editor {
public static class SpellFeelBuild {
 public static void Build(){
  const string dir="Evidence/SpellFeelUI";Directory.CreateDirectory(dir);
  var old=new EarthState{version=1,stone=17,wood=23,preparedStone=true};
  for(int i=0;i<EarthTrial.Locations.Length;i++)old.nodes.Add(new EarthNodeState{id=EarthTrial.Id(i),kind=EarthTrial.Kind(i)});
  old.nodes[0].damage=1;old.nodes[1].damage=2;old.nodes[2].damage=3;old.nodes[2].depleted=true;old.nodes[2].collected=true;
  old.nodes[13].damage=1;old.nodes[14].damage=2;old.nodes[14].depleted=true;
  string before=JsonUtility.ToJson(old);var upgraded=EarthTrial.Upgrade(old);
  if(upgraded.version!=2||upgraded.nodes[0].damage!=2||upgraded.nodes[1].damage!=3||upgraded.nodes[2].damage!=5||upgraded.nodes[13].damage!=4||upgraded.nodes[14].damage!=8||!upgraded.nodes[2].collected||upgraded.nodes[14].collected||upgraded.stone!=17||upgraded.wood!=23||!upgraded.preparedStone||JsonUtility.ToJson(old)!=before)throw new Exception("Earth migration contract failed");
  if(JsonUtility.ToJson(EarthTrial.Upgrade(upgraded))!=JsonUtility.ToJson(upgraded))throw new Exception("Migration is not idempotent");
  old.nodes[0].damage=3;bool rejected=false;try{EarthTrial.Upgrade(old);}catch(IOException){rejected=true;}if(!rejected)throw new Exception("Invalid legacy depletion accepted");
  File.WriteAllText(dir+"/migration-contracts.txt","PASS v1 0/1/2/depleted mapping, collected retention, inventory/ready preservation, source unchanged, v2 idempotence, invalid depletion rejection. Supported Earth schemas: v1 and v2; creative saves have no Earth state.");
  Export(BubbleAudio.Placement(),dir+"/placement-bubble.wav");Export(BubbleAudio.Pickup(),dir+"/pickup-unchanged.wav");
  BuildElemental.Build();
 }
 static void Export(AudioClip clip,string path){
  var samples=new float[clip.samples*clip.channels];clip.GetData(samples,0);
  using(var w=new BinaryWriter(File.Create(path))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+samples.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)clip.channels);w.Write(clip.frequency);w.Write(clip.frequency*clip.channels*2);w.Write((short)(clip.channels*2));w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(samples.Length*2);foreach(float f in samples)w.Write((short)Mathf.RoundToInt(Mathf.Clamp(f,-1,1)*32767));}
  UnityEngine.Object.DestroyImmediate(clip);
 }
}
}

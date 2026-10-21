using System;
using System.IO;
using UnityEngine;
namespace Skywright.Editor {
public static class AirSaveChecks {
 public static void Run(){
  string dir="Evidence/AirMagic/serializer";Directory.CreateDirectory(dir);
  var s=new SaveData{player=World.Spawn,earth=new EarthState(),air=AirTrial.NewState()};
  for(int i=0;i<EarthTrial.Locations.Length;i++)s.earth.nodes.Add(new EarthNodeState{id=EarthTrial.Id(i),kind=EarthTrial.Kind(i)});
  string path=dir+"/roundtrip.json";Persistence.Write(path,s);var a=Persistence.Read(path);
  if(!a.elementalTrial||a.air.crates.Count!=2||a.earth==null)throw new Exception("Air roundtrip failed");
  string before=File.ReadAllText(path);s.air.crates[1].id=s.air.crates[0].id;bool rejected=false;try{Persistence.Write(path,s);}catch(IOException){rejected=true;}
  if(!rejected||before!=File.ReadAllText(path))throw new Exception("Duplicate crate validation failed");
  var creative=new SaveData{player=World.Spawn};Persistence.Write(dir+"/creative.json",creative);if(Persistence.Read(dir+"/creative.json").air!=null)throw new Exception("Legacy air discriminator failed");
  Debug.Log("AIR_SAVE_CONTRACTS passed");
 }
}
}

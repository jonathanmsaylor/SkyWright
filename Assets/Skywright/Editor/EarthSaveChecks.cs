using System;
using System.IO;
using UnityEngine;
namespace Skywright.Editor {
public static class EarthSaveChecks {
 public static void Run(){
  string dir="Evidence/Earthcasting/serializer-checks";Directory.CreateDirectory(dir);
  string legacy="Evidence/Earthcasting/saves-before/verification.json";
  if(File.Exists(legacy)){var old=Persistence.Read(legacy);if(old.earth!=null||old.harvestingTrial)throw new Exception("Legacy save misclassified");}
  var creative=new SaveData{player=World.Spawn};creative.pieces.Add(new PieceRecord{id="contract-creative",type="floor",position=Vector3.zero});
  Persistence.Write(dir+"/creative.json",creative);var loaded=Persistence.Read(dir+"/creative.json");
  if(loaded.earth!=null||loaded.harvestingTrial||loaded.pieces[0].paidWood!=0)throw new Exception("Creative round trip failed");
  var earth=new EarthState{wood=12,stone=17,preparedStone=true};for(int i=0;i<EarthTrial.Locations.Length;i++)earth.nodes.Add(new EarthNodeState{id=EarthTrial.Id(i),kind=EarthTrial.Kind(i)});
  earth.nodes[0].damage=1;earth.nodes[0].fracture=new Vector3(.2f,1,-.2f);
  var trial=new SaveData{player=World.Spawn,earth=earth};trial.pieces.Add(new PieceRecord{id="contract-paid",type="floor",paidWood=6});
  Persistence.Write(dir+"/trial.json",trial);loaded=Persistence.Read(dir+"/trial.json");
  if(!loaded.harvestingTrial||!loaded.earth.preparedStone||loaded.earth.stone!=17||loaded.earth.wood!=12||loaded.earth.nodes[0].damage!=1||loaded.pieces[0].paidWood!=6)throw new Exception("Trial round trip failed");
  string before=File.ReadAllText(dir+"/trial.json");
  File.WriteAllText(dir+"/old-trial.json",before.Replace("\"preparedStone\": true,", ""));
  if(Persistence.Read(dir+"/old-trial.json").earth.preparedStone)throw new Exception("Missing prepared field must default false");trial.earth.wood=-1;
  bool rejected=false;try{Persistence.Write(dir+"/trial.json",trial);}catch(IOException){rejected=true;}
  if(!rejected||File.ReadAllText(dir+"/trial.json")!=before)throw new Exception("Invalid write replaced valid trial");
  trial.earth.wood=12;trial.earth.nodes[1].id=trial.earth.nodes[0].id;rejected=false;
  try{Persistence.Write(dir+"/trial.json",trial);}catch(IOException){rejected=true;}
  if(!rejected||File.ReadAllText(dir+"/trial.json")!=before)throw new Exception("Duplicate resource identity accepted");
  File.WriteAllText(dir+"/result.txt","PASS legacy absent-state handling; creative and trial round trips; paid resource retention; negative resource and duplicate ID rejection; failed writes retain prior file.\n");
  Debug.Log("EARTH_SAVE_CONTRACTS passed");
 }
}
}

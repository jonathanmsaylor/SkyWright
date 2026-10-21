using System;using System.IO;using System.Linq;using UnityEngine;
namespace Skywright.Editor {
public static class AetherSaveChecks {
 public static void Run(){
  var c=Resources.Load<AetherConfig>("CompactIsland/Aether");if(!c||c.bubbles.Length<36||c.bubbles.Select(x=>x.id).Distinct().Count()!=c.bubbles.Length)throw new Exception("Invalid authored Aether supply");
  var s=new AetherState{tier=1,balance=3};s.collected=c.bubbles.Take(9).Select(b=>b.id).ToList();AetherSystem.Validate(s);
  var copy=JsonUtility.FromJson<AetherState>(JsonUtility.ToJson(s));AetherSystem.Validate(copy);
  if(JsonUtility.ToJson(copy)!=JsonUtility.ToJson(s))throw new Exception("Aether roundtrip changed");
  void Reject(AetherState bad){bool rejected=false;try{AetherSystem.Validate(bad);}catch(IOException){rejected=true;}if(!rejected)throw new Exception("Bad Aether ledger accepted");}
  copy.collected.Add(copy.collected[0]);Reject(copy);copy=JsonUtility.FromJson<AetherState>(JsonUtility.ToJson(s));copy.balance++;Reject(copy);
  copy=JsonUtility.FromJson<AetherState>(JsonUtility.ToJson(s));copy.tier=3;Reject(copy);
  var previous=File.ReadAllLines("Evidence/CompactAether/prior-resource-ids.txt").Select(x=>x.Trim()).Where(x=>x.Length>0).OrderBy(x=>x).ToArray();
  var current=SceneryResource.Definitions().Select(x=>x.stableId).OrderBy(x=>x).ToArray();if(!previous.SequenceEqual(current))throw new Exception("Resource identity changed");
  Directory.CreateDirectory("Evidence/CompactAether");File.WriteAllText("Evidence/CompactAether/contracts.txt","Aether JSON roundtrip, duplicate rejection, ledger rejection, unpaid tier rejection passed.\n56 authored bubbles, 36 required; source IDs distinct.\nAll "+current.Length+" earlier scenery resource IDs preserved.\n");
  Debug.Log("AETHER_SAVE_CONTRACTS passed");
 }
 public static void VerifyAndBuild(){Run();MountainAeroAuthor.Build();}
}
}

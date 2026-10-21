using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Skywright {
[Serializable] public class SaveData {
 public int version=1; public string worldLayout="";public string savedUtc; public List<PieceRecord> pieces=new List<PieceRecord>();
 public Vector3 player;public float yaw,pitch;public EarthState earth;public bool harvestingTrial;public AirState air;public bool elementalTrial;
}
public static class Persistence {
 public static void Write(string path,SaveData data){
  Directory.CreateDirectory(Path.GetDirectoryName(path));data.savedUtc=DateTime.UtcNow.ToString("O");data.harvestingTrial=data.earth!=null;data.elementalTrial=data.air!=null;
  string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(data,true));
  Read(temp); // Validate before replacing the prior save.
  if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
 }
 public static SaveData Read(string path){
  if(!File.Exists(path))throw new IOException("No save exists in this slot yet.");
  var data=JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
  if(data==null||data.version!=1||data.pieces==null||data.pieces.Count>15000||string.IsNullOrEmpty(data.savedUtc)||!DateTime.TryParse(data.savedUtc,out _))throw new IOException("Unsupported or damaged save. Current build kept.");
  HashSet<string> ids=new HashSet<string>();
  foreach(var r in data.pieces) {
   if(r==null||string.IsNullOrEmpty(r.id)||!ids.Add(r.id)||Array.IndexOf(Kit.Ids,r.type)<0||r.parentId!="home-island"||!Finite(r.position)||!Finite(r.rotation)||r.paidWood<0||r.paidWood>1000000||r.paidWood>0&&r.type!="floor")throw new IOException("Invalid piece data. Current build kept.");
  }
  if(!Finite(data.player)||!float.IsFinite(data.yaw)||!float.IsFinite(data.pitch))throw new IOException("Invalid player data. Current build kept.");
  // JsonUtility materializes absent/null inline classes. An explicit marker distinguishes old saves.
  if(data.harvestingTrial)data.earth=EarthTrial.Upgrade(data.earth);else data.earth=null;
  if(data.elementalTrial){if(!data.harvestingTrial)throw new IOException("Air trial lacks harvesting state");AirTrial.Validate(data.air);if(!string.IsNullOrEmpty(data.air.hoverAnchor)&&!(TutorialIsland.Active&&data.air.hoverAnchor==TutorialIsland.AnchorId)&&!data.pieces.Exists(p=>p.id==data.air.hoverAnchor&&p.type=="wind-anchor"))throw new IOException("Invalid hover support");}else data.air=null;
  return data;
 }
 static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)&&v.sqrMagnitude<100000000;
 static bool Finite(Quaternion q)=>float.IsFinite(q.x)&&float.IsFinite(q.y)&&float.IsFinite(q.z)&&float.IsFinite(q.w)&&Mathf.Abs(q.x*q.x+q.y*q.y+q.z*q.z+q.w*q.w-1)<.02f;
}
}

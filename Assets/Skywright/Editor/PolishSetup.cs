using UnityEditor;
using UnityEngine;
namespace Skywright.Editor {
public static class PolishSetup {
 public static void Build(){
  var e=Resources.Load<EarthConfig>("Earthcasting/Tuning");e.range=e.maxTravelDistance=240;e.flightSpeed=22;e.steeringDegrees=150;e.maxFlightTime=12;EditorUtility.SetDirty(e);
  var a=Resources.Load<AirConfig>("AirMagic/Tuning");a.descentSpeed=3;a.gustAcceleration=10;
  const string path="Assets/Resources/AirMagic/AnchorBubble.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!m){m=new Material(Shader.Find("Skywright/AnchorBubble"));AssetDatabase.CreateAsset(m,path);}a.bubble=m;EditorUtility.SetDirty(a);
  var catalog=Resources.Load<PieceCatalog>("AirMagic/Catalog");catalog.pieces[0].hint="R to lift nearby. Space rises; Ctrl descends.";EditorUtility.SetDirty(catalog);
  AssetDatabase.SaveAssets();BuildElemental.Build();
 }
}
}

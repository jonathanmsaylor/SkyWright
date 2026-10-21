using System;
using UnityEngine;
namespace Skywright {
[Serializable] public class PieceConnection {
 public string accepts; public Vector3 position; public Vector3 euler; public float aimHeight;
 public bool Accepts(string type)=>Array.IndexOf(accepts.Split(','),type)>=0;
}
[Serializable] public class PieceDefinition {
 public string typeId, displayName, hint, origin;
 public Vector3 dimensions;
 public BuiltPiece prefab;
 public PieceConnection[] connections; // Legacy authored data; superseded by named anchors.
 public SnapAnchor[] anchors;
}
[CreateAssetMenu(menuName="Skywright/Piece catalog")]
public class PieceCatalog : ScriptableObject {
 public PieceDefinition[] pieces;
 static PieceCatalog cached,airCatalog,naturalCatalog;
 public static PieceDefinition Find(string type) {
  if(NaturalPreview.Active){if(!naturalCatalog)naturalCatalog=Resources.Load<PieceCatalog>("NaturalAero/Catalog");if(naturalCatalog)foreach(var p in naturalCatalog.pieces)if(p.typeId==type)return p;}
  if(!cached)cached=Resources.Load<PieceCatalog>("SkywrightCatalog");
  if(cached&&cached.pieces!=null)foreach(var p in cached.pieces)if(p.typeId==type)return p;
  if(!airCatalog)airCatalog=Resources.Load<PieceCatalog>("AirMagic/Catalog");
  if(airCatalog&&airCatalog.pieces!=null)foreach(var p in airCatalog.pieces)if(p.typeId==type)return p;
  return null;
 }
}
}

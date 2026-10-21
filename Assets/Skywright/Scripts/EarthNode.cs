using UnityEngine;
namespace Skywright {
// Identity lives in the explicit trial layout, never in a prefab or hierarchy name.
public class EarthNode : MonoBehaviour {
 public Transform crown;public Collider harvestingCollider;
 [System.NonSerialized] public EarthNodeState state;
 [System.NonSerialized] public Transform wound,loot;
 [System.NonSerialized] public Quaternion restingRotation;
 [System.NonSerialized] public float fallClock=-1,collectClock=-1,reaction;
 [System.NonSerialized] public Vector3 fallDirection,collectStart,lootGround;
 [System.NonSerialized] public CollectiblePull pull=new CollectiblePull();
}
[System.Serializable] public class EarthNodeState {
 public string id,kind;public int damage;public bool depleted,collected;
 public Vector3 fracture;
}
[System.Serializable] public class EarthState {
 public int version=2,stone,wood;public bool preparedStone;
 public System.Collections.Generic.List<EarthNodeState> nodes=new System.Collections.Generic.List<EarthNodeState>();
}
}

using UnityEngine;
namespace Skywright {
// Explicit movable adapter. Furniture keeps its existing BuiltPiece identity and colliders.
public class AirCrate : MonoBehaviour {
 public string id;public Vector3 home,safe,velocity;public BoxCollider box;public Transform visual;
 public string blocked="";public bool resting,simulating=true;public float motionTime;public BuiltPiece furniture;
 Vector3 localCenter,half;Bounds visualBounds;bool ready;
 public static bool Eligible(string type)=>type=="bench"||type=="planter"||type=="lantern";
 public void Configure(BuiltPiece piece){furniture=piece;id=piece.id;safe=home=transform.position;simulating=false;Measure();}
 void Measure(){
  Bounds b=new Bounds();bool first=true;
  foreach(var c in GetComponentsInChildren<Collider>()){
   if(c is BoxCollider bc){Encapsulate(ref b,ref first,bc.transform,bc.center,bc.size*.5f);}
   else if(c is MeshCollider mc&&mc.sharedMesh)Encapsulate(ref b,ref first,mc.transform,mc.sharedMesh.bounds.center,mc.sharedMesh.bounds.extents);
  }
  localCenter=b.center;half=Vector3.Max(b.extents,Vector3.one*.03f);
  visualBounds=b;first=true;foreach(var f in GetComponentsInChildren<MeshFilter>())if(f.sharedMesh)Encapsulate(ref visualBounds,ref first,f.transform,f.sharedMesh.bounds.center,f.sharedMesh.bounds.extents);
  ready=true;
 }
 void Encapsulate(ref Bounds b,ref bool first,Transform tr,Vector3 center,Vector3 ext){
  for(int i=0;i<8;i++){var q=center+Vector3.Scale(ext,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));q=transform.InverseTransformPoint(tr.TransformPoint(q));if(first){b=new Bounds(q,Vector3.zero);first=false;}else b.Encapsulate(q);}
 }
 public Vector3 Center{get{if(!ready)Measure();return transform.TransformPoint(localCenter);}}
 public Vector3 Half{get{if(!ready)Measure();return Vector3.Scale(half,transform.lossyScale);}}
 public Bounds VisualBounds{get{if(!ready)Measure();return visualBounds;}}
 public Vector3 Sweep(Vector3 delta){
  blocked="";if(delta.sqrMagnitude<.00000001f)return Vector3.zero;
  float distance=delta.magnitude,allowed=distance;
  foreach(var h in Physics.BoxCastAll(Center,Vector3.Max(Half-Vector3.one*.012f,Vector3.one*.01f),delta/distance,transform.rotation,distance+.018f,~(1<<2),QueryTriggerInteraction.Ignore)){
   if(h.collider.transform.IsChildOf(transform))continue;
   if(h.distance-.018f<allowed){allowed=Mathf.Max(0,h.distance-.018f);blocked=h.collider.name;}
  }
  Vector3 step=delta/distance*allowed;transform.position+=step;Physics.SyncTransforms();return step;
 }
 public bool Occupied(Vector3 at){
  Vector3 center=Center+at-transform.position;
  foreach(var c in Physics.OverlapBox(center,Vector3.Max(Half-Vector3.one*.02f,Vector3.one*.01f),transform.rotation,~(1<<2),QueryTriggerInteraction.Ignore))if(!c.transform.IsChildOf(transform))return true;
  return false;
 }
 public bool Grounded(){
  foreach(var h in Physics.BoxCastAll(Center,Vector3.Max(Half-Vector3.one*.025f,Vector3.one*.01f),Vector3.down,transform.rotation,.07f,~(1<<2),QueryTriggerInteraction.Ignore))
   if(!h.collider.transform.IsChildOf(transform)&&h.normal.y>.65f)return true;
  return false;
 }
}
}

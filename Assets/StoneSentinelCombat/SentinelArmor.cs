using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Skywright {
// Debris is presentation-only: no colliders, inventory identities, or damage callbacks.
public sealed class SentinelArmor : MonoBehaviour {
 public int Stage {get;private set;}
 public int DebrisCount=>debris.Count;
 public bool BrokenApart {get;private set;}
 internal string DescribeDebris()=>string.Join("\n",debris.Select(d=>d.obj.name+" p="+d.obj.transform.position+" bounds="+d.renderer.bounds+" scale="+d.obj.transform.localScale+" resting="+d.resting));
 SkinnedMeshRenderer[] sections;SentinelCombat combat;MaterialPropertyBlock dark;
 class Fragment {public GameObject obj;public Mesh mesh;public Renderer renderer;public Vector3[] vertices;public Vector3 velocity,spin,scale;public float age;public bool resting,ownsMesh;}
 readonly Dictionary<Mesh,Vector3[]> chipVertices=new Dictionary<Mesh,Vector3[]>();
 internal static float LowestPoint(Transform pose,Vector3[] vertices){float lowest=float.PositiveInfinity;var matrix=pose.localToWorldMatrix;foreach(var v in vertices)lowest=Mathf.Min(lowest,matrix.MultiplyPoint3x4(v).y);return lowest;}
 readonly List<Fragment> debris=new List<Fragment>();
 public void Initialize(SentinelCombat owner){combat=owner;sections=GetComponentsInChildren<SkinnedMeshRenderer>();dark=new MaterialPropertyBlock();ResetArmor();}
 public void ResetArmor(){
  foreach(var d in debris){if(d.obj)Destroy(d.obj);if(d.ownsMesh&&d.mesh)Destroy(d.mesh);}debris.Clear();Stage=0;BrokenApart=false;
  foreach(var s in sections)s.enabled=!s.name.StartsWith("Core_");
 }
 public void SetDamage(int health){
  int next=health<=6?3:health<=12?2:health<=18?1:0;
  while(Stage<next){Stage++;string prefix="Armor_"+Stage+"_";
   foreach(var s in sections.Where(s=>s.name.StartsWith(prefix)&&s.enabled))Detach(s,false);
   foreach(var s in sections.Where(s=>s.name.StartsWith("Core_"+Stage+"_")))s.enabled=true;
   combat.ImpactSound(transform.position+Vector3.up);Debug.Log("COMBAT ARMOR_SHED stage="+Stage);
  }
 }
 public void BreakApart(){
  if(BrokenApart)return;BrokenApart=true;
  foreach(var s in sections)if(s.enabled)Detach(s,true);
  combat.BreakEffect(transform.position);Debug.Log("COMBAT BREAK_APART debris="+debris.Count);
 }
 void Detach(SkinnedMeshRenderer skin,bool death){
  if(debris.Count>=32){skin.enabled=false;return;}
  // Imported FBX meshes carry scale in their transforms; compensate when baking.
  var mesh=new Mesh{name="Detached "+skin.name};skin.BakeMesh(mesh,true);
  Vector3 center=mesh.bounds.center;var vertices=mesh.vertices;for(int i=0;i<vertices.Length;i++)vertices[i]-=center;mesh.vertices=vertices;mesh.RecalculateBounds();
  var g=new GameObject(mesh.name);g.layer=2;g.transform.SetPositionAndRotation(skin.transform.TransformPoint(center),skin.transform.rotation);g.transform.localScale=skin.transform.lossyScale;
  // Collapse into solid, source-sculpted limestone rather than hollow clusters of body surfaces.
  bool ownsMesh=true;
  if(combat.LimestoneChip(debris.Count)){
   float diameter=Mathf.Clamp(Vector3.Scale(mesh.bounds.size,skin.transform.lossyScale).magnitude*.62f,.22f,.85f);
   Destroy(mesh);mesh=combat.LimestoneChip(debris.Count);ownsMesh=false;g.transform.localScale=Vector3.one*diameter;
  }
  g.AddComponent<MeshFilter>().sharedMesh=mesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterials=skin.sharedMaterials;
  if(combat.limestoneMaterial)r.sharedMaterial=combat.limestoneMaterial;
  dark.SetColor("_EmissionColor",Color.black);r.SetPropertyBlock(dark);
  var outward=Vector3.ProjectOnPlane(g.transform.position-transform.position,Vector3.up).normalized;
  int index=debris.Count;float spread=death?1.15f:1.6f;
  if(!chipVertices.TryGetValue(mesh,out var points)){points=mesh.vertices;if(!ownsMesh)chipVertices.Add(mesh,points);}
  debris.Add(new Fragment{obj=g,mesh=mesh,ownsMesh=ownsMesh,vertices=points,renderer=r,velocity=outward*spread+Vector3.up*(death?.45f:1.2f),spin=new Vector3(31+index*7,41-index*3,25+index*5),scale=g.transform.localScale});skin.enabled=false;
 }
 public void Tick(float dt){
  for(int i=debris.Count-1;i>=0;i--){var d=debris[i];d.age+=dt;
   if(d.age>=14){Destroy(d.obj);if(d.ownsMesh)Destroy(d.mesh);debris.RemoveAt(i);continue;}
   if(!d.resting){
    var before=d.obj.transform.position;d.velocity+=Vector3.down*9.8f*dt;
    Vector3 step=d.velocity*dt;
    if(step.sqrMagnitude>.000001f)foreach(var hit in Physics.RaycastAll(before,step.normalized,step.magnitude+.08f,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore)){
     if(hit.transform==transform||hit.transform.IsChildOf(transform))continue;
     if(hit.normal.y<.6f)d.velocity=Vector3.Reflect(d.velocity,hit.normal)*.2f;
    }
    d.obj.transform.position+=d.velocity*dt;d.obj.transform.Rotate(d.spin*dt,Space.World);
    RaycastHit? ground=null;foreach(var hit in Physics.RaycastAll(d.obj.transform.position+Vector3.up*2,Vector3.down,5,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore)){
     if(hit.transform==transform||hit.transform.IsChildOf(transform)||hit.normal.y<.6f)continue;
     if(!ground.HasValue||hit.point.y>ground.Value.point.y)ground=hit;
    }
    if(ground.HasValue&&d.renderer.bounds.min.y<=ground.Value.point.y+.012f){
     float bottom=LowestPoint(d.obj.transform,d.vertices);
     if(bottom<=ground.Value.point.y+.012f){
      d.obj.transform.position+=Vector3.up*(ground.Value.point.y+.012f-bottom);d.resting=true;d.velocity=Vector3.zero;
      if(i%4==0)combat.ImpactSound(d.obj.transform.position);
     }
    }
   }
   if(d.age>12)d.obj.transform.localScale=d.scale*Mathf.Clamp01((14-d.age)/2);
  }
 }
 void OnDestroy(){foreach(var d in debris){if(d.obj)Destroy(d.obj);if(d.ownsMesh&&d.mesh)Destroy(d.mesh);}}
}
}

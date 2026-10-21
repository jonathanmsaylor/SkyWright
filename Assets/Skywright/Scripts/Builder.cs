using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skywright {
public class Builder {
 public readonly List<BuiltPiece> pieces=new List<BuiltPiece>();
 public Transform root; public Camera cam; public CharacterController player;
 public int index,rotationSteps,cycle; public float reach=8;
 public bool enabled,free; public BuiltPiece ghost,target; public string reason="",snapLabel="";
 public bool valid,snapped; public Vector3 snapPoint;public float anchorError;
 public string SnapMode {get {var modes=SnapRules.Modes(Kit.Ids[index]);return cycle==0?"Auto snap":modes[(cycle-1)%modes.Length].label;}}
 public Action<string> Notify;public Action<string> Log;
 public Func<string,string> CanPay;public Action<BuiltPiece> Placed,Removing;
 struct Candidate {public Vector3 p,contact,source;public Quaternion q;public float score;public string label,key;}
 readonly List<Candidate> candidates=new List<Candidate>();
 readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
 string activeKey;Vector3 aimPoint;
 public Builder(Transform r,Camera c,CharacterController pl){root=r;cam=c;player=pl;}
 public void Select(int i){index=(i+TutorialIsland.BuildableCount)%TutorialIsland.BuildableCount;rotationSteps=0;cycle=0;activeKey=null;if(ghost)UnityEngine.Object.Destroy(ghost.gameObject);ghost=null;}
 public void Hide(){if(ghost)ghost.gameObject.SetActive(false);ClearHighlight();}
 public void Tick(){
  var k=Keyboard.current;var m=Mouse.current;if(k==null||m==null)return;
  free=k.leftAltKey.isPressed||k.rightAltKey.isPressed;
  bool shift=k.leftShiftKey.isPressed||k.rightShiftKey.isPressed;
  ClearHighlight();
  Ray ray=cam.ViewportPointToRay(new Vector3(.5f,.5f,0));
  bool hit=Physics.Raycast(ray,out RaycastHit h,reach,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore);
  if(hit)target=h.collider.GetComponentInParent<BuiltPiece>();if(TutorialIsland.Fixed(target))target=null;
  // Selection only: no object duplication or save ID reuse. The UI owns input before Tick.
  if(shift&&m.middleButton.wasPressedThisFrame){
   if(target){Select(Array.IndexOf(Kit.Ids,target.type));enabled=true;Notify?.Invoke("Selected "+Kit.Names[index]);Log?.Invoke("PICK "+target.type);}
   else Notify?.Invoke("Aim at a built piece to select it");
  }
  if(!enabled){if(ghost)ghost.gameObject.SetActive(false);return;}
  if(shift&&k.tabKey.wasPressedThisFrame){cycle=(cycle+1)%(SnapRules.Modes(Kit.Ids[index]).Length+1);activeKey=null;Notify?.Invoke("Snap mode: "+SnapMode);Log?.Invoke("SNAP_MODE "+SnapMode);}
  float scroll=m.scroll.ReadValue().y;bool rotating=false;
  if(Mathf.Abs(scroll)>.001f){
   int steps=Mathf.Abs(scroll)>=120?Mathf.RoundToInt(scroll/120f):(int)Mathf.Sign(scroll);steps=Mathf.Clamp(steps,-8,8);
   if(shift){reach=Mathf.Clamp(reach+steps*.5f,2,12);Log?.Invoke("REACH "+reach.ToString("F1"));}
   else {rotationSteps=(rotationSteps+steps+16)%8;rotating=true;Log?.Invoke("ROTATE "+rotationSteps*45);}
  }
  if(!ghost)ghost=Kit.Create(Kit.Ids[index],Vector3.zero,Quaternion.identity,null,true);
  ghost.gameObject.SetActive(true);string type=Kit.Ids[index];aimPoint=hit?h.point:ray.GetPoint(reach);
  Quaternion q=Quaternion.Euler(0,rotationSteps*45,0);Vector3 pos=aimPoint;snapped=false;snapLabel="Surface placement";anchorError=0;
  if(!free){
   FindCandidates(type,aimPoint,ray,hit,rotating);
   if(candidates.Count>0){
    candidates.Sort((a,b)=>{int n=a.score.CompareTo(b.score);return n!=0?n:string.CompareOrdinal(a.key,b.key);});
    Candidate chosen=candidates[0];string firstReason=null;bool found=false;
    // Auto chooses a usable attachment at the aimed contact; it must not jump to a distant socket.
    for(int i=0;i<candidates.Count;i++){
     var c=candidates[i];if(i>0&&(cycle!=0||c.score>candidates[0].score+.08f))break;
     ghost.transform.SetPositionAndRotation(c.p,c.q);Physics.SyncTransforms();snapPoint=c.contact;
     string failure=Validate(type,c.p,c.q,hit,true);
     if(i==0)firstReason=failure;
     if(failure==""){chosen=c;found=true;break;}
    }
    pos=chosen.p;q=chosen.q;snapPoint=chosen.contact;snapped=true;snapLabel=chosen.label;activeKey=chosen.key;
    anchorError=Vector3.Distance(pos+q*chosen.source,snapPoint);reason=found?"":firstReason;
   }else {
    activeKey=null;
    if(hit){if(type=="floor")pos=new Vector3(Mathf.Round(aimPoint.x/4)*4,Mathf.Max(.2f,aimPoint.y+.2f),Mathf.Round(aimPoint.z/4)*4);
     else pos=new Vector3(Mathf.Round(aimPoint.x*4)/4,aimPoint.y,Mathf.Round(aimPoint.z*4)/4);}
   }
  }else {activeKey=null;snapLabel="Free placement";if(type=="floor")pos.y+=.2f;}
  // The stair root is beneath its upper end. Surface placement uses the FOOT, not that root.
  if(!snapped&&type=="stairs")pos-=q*new Vector3(0,0,-4);
  ghost.transform.SetPositionAndRotation(pos,q);Physics.SyncTransforms();
  if(!snapped)reason=Validate(type,pos,q,hit,false);
  if(reason==""&&CanPay!=null)reason=CanPay(type);
  valid=reason=="";
  foreach(var r in ghost.GetComponentsInChildren<Renderer>())r.sharedMaterial=valid?Kit.good:Kit.bad;
  if(target){block.SetColor("_BaseColor",new Color(1,.65f,.22f));foreach(var r in target.GetComponentsInChildren<Renderer>())r.SetPropertyBlock(block);}
  if(m.leftButton.wasPressedThisFrame){
   if(valid){var b=Kit.Create(type,pos,q,root);pieces.Add(b);Placed?.Invoke(b);Log?.Invoke("PLACE "+type+" "+pos.ToString("F3")+" "+q.eulerAngles.ToString("F1")+" "+SnapMode+" "+snapLabel+" anchorError="+anchorError.ToString("F6"));Notify?.Invoke(Kit.Names[index]+" placed");}
   else {Notify?.Invoke(reason);Log?.Invoke("REJECT "+type+" "+reason);}
  }
  if(m.middleButton.wasPressedThisFrame&&!shift&&target){var b=target;Removing?.Invoke(b);pieces.Remove(b);target=null;Log?.Invoke("REMOVE "+b.type+" "+b.id);UnityEngine.Object.Destroy(b.gameObject);Notify?.Invoke("Piece removed");}
 }
 void FindCandidates(string type,Vector3 aim,Ray ray,bool hit,bool rotating){
  candidates.Clear();var modes=SnapRules.Modes(type);
  foreach(var b in pieces)foreach(var socket in SnapRules.Anchors(b.type)){
   Vector3 contact=b.transform.TransformPoint(socket.position);
   float along=Vector3.Dot(contact-ray.origin,ray.direction);if(along<.2f||Vector3.Distance(cam.transform.position,contact)>reach+.15f)continue;
   float off=Vector3.Distance(contact,ray.GetPoint(along)),distance=Vector3.Distance(contact,aim);
   // A directly aimed piece owns the snap. Terrain aiming only attracts very nearby contacts.
   if(hit?(target?b!=target||distance>(cycle==0?2.85f:6f):distance>1.05f):off>.65f)continue;
   float score=hit?distance:off+.15f*Mathf.Abs(along-reach);
   foreach(var source in modes){
    if(cycle!=0&&source.id!=modes[(cycle-1)%modes.Length].id)continue;
    if(!SnapRules.Match(type,source,b.type,socket,cycle==0,out float yaw,out float preference))continue;
    Quaternion q=b.transform.rotation*Quaternion.Euler(0,yaw+rotationSteps*45,0);
    var key=b.id+"/"+socket.id+"/"+source.id;
    candidates.Add(new Candidate{p=SnapRules.Solve(contact,q,source.position),q=q,contact=contact,source=source.position,
     score=score+preference-(rotating&&key==activeKey?.06f:0),label=source.label+" → "+Kit.Names[Array.IndexOf(Kit.Ids,b.type)]+" / "+socket.label,key=key});
   }
  }
 }
 string Validate(string type,Vector3 pos,Quaternion q,bool hit,bool snap){
  if(!hit&&!snap)return "Aim at the island or an existing piece";
  if(Vector3.Distance(cam.transform.position,snap?snapPoint:aimPoint)>reach+.2f)return "Outside building reach";
  if(pos.y<-.5f&&type!="post"&&type!="stairs")return "Below the buildable surface";
  foreach(var b in pieces){
   bool sameShape=b.type==type||(Kit.IsWall(type)&&Kit.IsWall(b.type));
   if(!sameShape||Vector3.Distance(pos,b.transform.position)>=.12f)continue;
   float angle=Quaternion.Angle(q,b.transform.rotation);
   bool symmetry=type=="floor"?Mathf.Abs(Mathf.DeltaAngle(angle,Mathf.Round(angle/90)*90))<5:
    (Kit.IsWall(type)||type=="beam"||type=="railing")?Mathf.Abs(angle-180)<5:type=="post";
   if(angle<5||symmetry)return "A piece already occupies this connection";
  }
  if(type=="stairs"){
   foreach(var local in new[]{new Vector3(0,0,-4),new Vector3(0,1.5f,-2),new Vector3(0,3,0)}){
    var point=pos+q*local;
    foreach(var ground in Physics.RaycastAll(point+Vector3.up*8,Vector3.down,16,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore))
     if(ground.collider.GetComponent<EarthSurface>()!=null&&point.y<ground.point.y-.12f)return "Stair bottom would be underground — use Bottom landing";
   }
  }
  foreach(var box in ghost.GetComponentsInChildren<Collider>()){
   Vector3 center,half;
   if(box is BoxCollider bc){center=bc.transform.TransformPoint(bc.center);half=Vector3.Scale(bc.size,bc.transform.lossyScale)*.5f;}
   else if(box is MeshCollider mc){center=mc.transform.TransformPoint(mc.sharedMesh.bounds.center);half=Vector3.Scale(mc.sharedMesh.bounds.extents,mc.transform.lossyScale);}
   else continue;
   var hits=Physics.OverlapBox(center,Vector3.Max(half-Vector3.one*.025f,Vector3.one*.01f),box.transform.rotation,~(1<<2),QueryTriggerInteraction.Ignore);
   foreach(var c in hits){
    if(c==player)return "Too close to you — step back";
    var other=c.GetComponentInParent<BuiltPiece>();
    if(other){
     float tolerance=(type=="floor"&&other.type=="stairs"||type=="stairs"&&other.type=="floor")?.20f:snap?.145f:.025f;
     if(Physics.ComputePenetration(box,box.transform.position,box.transform.rotation,c,c.transform.position,c.transform.rotation,out var dir,out float depth)&&depth>tolerance)
      return "Overlaps "+Kit.Names[Array.IndexOf(Kit.Ids,other.type)].ToLowerInvariant();
    }else{
     if(c.GetComponent<EarthSurface>()!=null&&(type=="floor"||type=="post"))continue;
     // Existing stair stringer ends extend 13 cm beneath the nominal foot: preserve that collider contract.
     float tolerance=type=="stairs"&&c.GetComponent<EarthSurface>()!=null?.18f:.12f;
     if(Physics.ComputePenetration(box,box.transform.position,box.transform.rotation,c,c.transform.position,c.transform.rotation,out var dir,out float depth)&&depth>tolerance)return "Blocked by terrain or scenery";
    }
   }
  }
  if(type=="gable"&&Vector3.Distance(player.transform.position+Vector3.up,pos+Vector3.up*.6f)<1.1f)return "Too close to you — step back";
  return "";
 }
 void ClearHighlight(){if(target)foreach(var r in target.GetComponentsInChildren<Renderer>())r.SetPropertyBlock(null);target=null;}
 public void Clear(){Hide();foreach(var b in pieces)if(b)UnityEngine.Object.Destroy(b.gameObject);pieces.Clear();}
}
}

using System.Collections.Generic;
using UnityEngine;
namespace Skywright {
// Bounded presentation only. Resource ownership never lives in a particle or audio callback.
public class EarthEffects {
 readonly EarthConfig config;readonly Transform root;readonly AudioSource[] audio=new AudioSource[8];int voice;
 class Shard {public Transform t;public Vector3 velocity,spin;public float age,life,size;}
 class Ripple {public LineRenderer line;public Vector3 center;public Quaternion rotation;public float age,life,radius;}
 readonly List<Shard> shards=new List<Shard>();readonly List<Ripple> rings=new List<Ripple>();
 readonly Stack<Transform> pool=new Stack<Transform>();
 public EarthEffects(EarthConfig c,Camera camera){config=c;root=new GameObject("Bounded Earthcasting effects").transform;
  for(int i=0;i<audio.Length;i++){var g=new GameObject("Earth voice "+i);g.transform.SetParent(root);audio[i]=g.AddComponent<AudioSource>();audio[i].spatialBlend=.65f;audio[i].minDistance=2;audio[i].maxDistance=22;audio[i].rolloffMode=AudioRolloffMode.Linear;}
 }
 public void Sound(AudioClip clip,Vector3 at,float volume){if(!clip)return;var a=audio[voice++%audio.Length];a.transform.position=at;a.pitch=Random.Range(.94f,1.06f);a.PlayOneShot(clip,volume);}
 public LineRenderer Line(string name,float width,bool loop){var g=new GameObject(name);g.layer=2;g.transform.SetParent(root);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=config.magic;l.startWidth=l.endWidth=width;l.loop=loop;l.numCornerVertices=3;l.numCapVertices=3;l.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return l;}
 public void Ring(Vector3 point,Vector3 normal,float radius,float life){if(rings.Count>=12)return;var l=Line("Earth pressure ripple",.025f,true);l.positionCount=40;rings.Add(new Ripple{line=l,center=point,rotation=Quaternion.FromToRotation(Vector3.forward,normal),radius=radius,life=life});}
 public void Trail(Transform target){var tr=target.gameObject.AddComponent<TrailRenderer>();tr.sharedMaterial=config.magic;tr.time=.35f;tr.startWidth=.10f;tr.endWidth=0;tr.minVertexDistance=.05f;tr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
 public void Burst(Vector3 p,Vector3 direction,Material material,int count,float size,float speed,float life){
  for(int i=0;i<count&&shards.Count<160;i++){
   Transform t;if(pool.Count>0)t=pool.Pop();else {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Spell fragment (visual only)";g.layer=2;g.GetComponent<Collider>().enabled=false;Object.Destroy(g.GetComponent<Collider>());g.transform.SetParent(root);t=g.transform;}
   t.gameObject.SetActive(true);t.GetComponent<Renderer>().sharedMaterial=material;t.position=p+Random.insideUnitSphere*.1f;t.rotation=Random.rotation;
   if(SentinelCombat.Active&&material==config.stone&&SentinelCombat.Current.LimestoneChip(i)){t.GetComponent<MeshFilter>().sharedMesh=SentinelCombat.Current.LimestoneChip(i);t.GetComponent<Renderer>().sharedMaterial=SentinelCombat.Current.limestoneMaterial;}
   float shardSize=size*Random.Range(.5f,1.4f);t.localScale=Vector3.one*shardSize;
   shards.Add(new Shard{t=t,velocity=(direction.normalized*.65f+Random.insideUnitSphere)*speed,spin=Random.insideUnitSphere*380,size=shardSize,life=life*Random.Range(.7f,1.3f)});
  }
 }
 public void Scar(Transform root,int damage){
  for(int i=0;i<6;i++){
   var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Bark fracture";g.layer=2;g.GetComponent<Collider>().enabled=false;Object.Destroy(g.GetComponent<Collider>());g.transform.SetParent(root,false);
   g.transform.localPosition=new Vector3((i%2==0?-1:1)*.04f,(i-2.5f)*.05f,.018f);
   g.transform.localScale=new Vector3(.045f+damage*.018f,.11f,.018f);g.transform.localRotation=Quaternion.Euler(0,0,i%2==0?28:-28);g.GetComponent<Renderer>().sharedMaterial=config.fracture;
  }
 }
 public void Tick(float dt){
  for(int i=shards.Count-1;i>=0;i--){var s=shards[i];s.age+=dt;if(s.age>=s.life){s.t.gameObject.SetActive(false);pool.Push(s.t);shards.RemoveAt(i);continue;}
   s.velocity+=Vector3.down*8*dt;s.t.position+=s.velocity*dt;if(s.t.position.y<.04f){var p=s.t.position;p.y=.04f;s.t.position=p;s.velocity*=.2f;s.velocity.y=Mathf.Abs(s.velocity.y);}
   s.t.Rotate(s.spin*dt);s.t.localScale=Vector3.one*s.size*Mathf.Min(1,(s.life-s.age)*5);
  }
  for(int i=rings.Count-1;i>=0;i--){var r=rings[i];r.age+=dt;if(r.age>=r.life){Object.Destroy(r.line.gameObject);rings.RemoveAt(i);continue;}
   float t=r.age/r.life,scale=r.radius*(.2f+.8f*(1-Mathf.Pow(1-t,2)));r.line.startWidth=r.line.endWidth=.025f*(1-t);
   for(int j=0;j<40;j++){float a=j*Mathf.PI*2/40;r.line.SetPosition(j,r.center+r.rotation*new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*scale);}
  }
 }
 public void Clear(){foreach(var s in shards){s.t.gameObject.SetActive(false);pool.Push(s.t);}shards.Clear();foreach(var r in rings)Object.Destroy(r.line.gameObject);rings.Clear();foreach(var a in audio)a.Stop();}
}
}

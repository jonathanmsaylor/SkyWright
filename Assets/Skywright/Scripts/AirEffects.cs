using UnityEngine;
namespace Skywright {
// Small fixed presentation budget. Ribbons and leaves never determine physical motion.
public class AirEffects {
 public enum FlowView { Upgraded, Legacy, Disabled }
 public static FlowView ReviewView=FlowView.Upgraded;
 public static bool MeasureFlow;public long FlowAllocated;public double FlowMilliseconds;
 readonly WindFlow suction,push;
 public float FlowRange=8;
 public void BeginFlow(){suction.BeginTargets();push.BeginTargets();}
 public void Affected(Vector3 center,float radius){suction.Target(center,radius);push.Target(center,radius);}
 public void Cancel(){suction.Clear();push.Clear();foreach(var l in ribbons)l.enabled=false;foreach(var l in leaves)l.gameObject.SetActive(false);}
 public WindFlow SuctionFlow=>suction;public WindFlow PushFlow=>push;
 public bool Pulling;float rush;public void Rush(float value){rush=value;windEnvelope.SetLevel(Mathf.Max(blowing?config.windVolume:0,rush*.24f));}
 readonly AirConfig config;readonly Camera camera;readonly Transform root,focus;readonly LineRenderer[] ribbons=new LineRenderer[10];readonly Transform[] leaves=new Transform[14];
 readonly AudioSource[] voices=new AudioSource[4];int voice;float clock,holdBlend,hoverBlend;Vector3 holdCenter;Bounds holdBounds;Quaternion holdRotation=Quaternion.identity;Vector3 holdPosition;float windBlend;bool blowing;readonly AudioSource wind;readonly SoftWindAudio windEnvelope;readonly Vector3[] points=new Vector3[48];
 public AirEffects(AirConfig c,Camera cam){config=c;camera=cam;root=new GameObject("Air presentation").transform;
  suction=Object.Instantiate(Resources.Load<WindFlow>("WindMagic/Suction"),root);push=Object.Instantiate(Resources.Load<WindFlow>("WindMagic/Push"),root);suction.Initialize(cam);push.Initialize(cam);
  focus=Object.Instantiate(c.focus,cam.transform).transform;focus.gameObject.SetActive(false);
  for(int i=0;i<ribbons.Length;i++){var g=new GameObject("Tapered wind ribbon");g.layer=2;g.transform.SetParent(root);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=c.ribbon;l.positionCount=48;l.numCapVertices=3;l.widthCurve=new AnimationCurve(new Keyframe(0,0),new Keyframe(.3f,.7f),new Keyframe(.65f,1),new Keyframe(1,0));l.widthMultiplier=.035f;l.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;l.receiveShadows=false;l.enabled=false;ribbons[i]=l;}
  var mesh=new Mesh();mesh.vertices=new[]{new Vector3(0,0,-.10f),new Vector3(-.045f,0,0),new Vector3(0,.015f,.11f),new Vector3(.045f,0,0)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateNormals();
  for(int i=0;i<leaves.Length;i++){var g=new GameObject("Wind-carried leaf");g.layer=2;g.transform.SetParent(root);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=c.leaf;leaves[i]=g.transform;g.SetActive(false);}
  var wg=new GameObject("Continuous wind sound");wg.transform.SetParent(camera.transform,false);wind=wg.AddComponent<AudioSource>();wind.loop=true;wind.spatialBlend=0;
  wind.clip=SoftWindAudio.CreateLoop();wind.volume=1;windEnvelope=wg.AddComponent<SoftWindAudio>();wind.Play();
  for(int i=0;i<voices.Length;i++){var g=new GameObject("Air sound");g.transform.SetParent(root);var a=g.AddComponent<AudioSource>();a.spatialBlend=.65f;a.minDistance=2;a.maxDistance=18;a.rolloffMode=AudioRolloffMode.Linear;voices[i]=a;}
 }
 public void Wind(bool active){blowing=active;windEnvelope.SetLevel(Mathf.Max(active?config.windVolume:0,rush*.24f));}
 public float WindGain=>windEnvelope.Gain;public int WindSample=>windEnvelope.SamplePosition;
 public void Focus(bool visible){focus.gameObject.SetActive(visible);}
 public void Cue(AudioClip clip,Vector3 p,float volume){if(!clip)return;var a=voices[voice++%voices.Length];a.transform.position=p;a.pitch=Random.Range(.96f,1.04f);a.PlayOneShot(clip,volume);}
 public void Pause(){Cancel();blowing=false;windEnvelope.SetLevel(0);windBlend=0;foreach(var a in voices)if(a.isPlaying)a.Pause();}
 public void Clear(){Cancel();blowing=false;windEnvelope.SetLevel(0);windBlend=0;foreach(var l in ribbons)l.enabled=false;foreach(var l in leaves)l.gameObject.SetActive(false);foreach(var a in voices)a.Stop();holdBlend=hoverBlend=0;}
 public void Tick(float dt,AirCrate held,bool floating,Vector3 player,BuiltPiece anchor,float gust,Vector3 gustOrigin,Vector3 gustDirection){
  long allocated=MeasureFlow?System.GC.GetAllocatedBytesForCurrentThread():0;long stamp=MeasureFlow?System.Diagnostics.Stopwatch.GetTimestamp():0;
  bool upgraded=ReviewView==FlowView.Upgraded;
  suction.Tick(dt,upgraded&&blowing&&Pulling,gustOrigin,gustDirection,FlowRange);
  push.Tick(dt,upgraded&&blowing&&!Pulling,gustOrigin,gustDirection,FlowRange);
  if(MeasureFlow){FlowAllocated=System.GC.GetAllocatedBytesForCurrentThread()-allocated;FlowMilliseconds=(System.Diagnostics.Stopwatch.GetTimestamp()-stamp)*1000.0/System.Diagnostics.Stopwatch.Frequency;}
  clock+=dt;foreach(var a in voices)a.UnPause();holdBlend=Mathf.MoveTowards(holdBlend,held?1:0,dt*4);hoverBlend=Mathf.MoveTowards(hoverBlend,floating?1:0,dt*3);
  windBlend=Mathf.MoveTowards(windBlend,blowing?1:0,dt*(blowing?6:8));
  if(held){holdCenter=held.Center;holdPosition=held.transform.position;holdRotation=held.transform.rotation;holdBounds=held.VisualBounds;}
  float gesture=windBlend*(.7f+Mathf.Sin(clock*5)*.06f)+holdBlend*.3f;
  focus.localPosition=new Vector3(.34f,-.32f+gesture*.09f,.62f-gesture*.09f);focus.localRotation=Quaternion.Euler(-12-gesture*20,0,Mathf.Sin(clock*1.7f)*2-gesture*12);
  for(int i=0;i<10;i++){
   bool carry=i<3,hover=i>=3&&i<6;float strength=carry?holdBlend:hover?hoverBlend:ReviewView==FlowView.Legacy?windBlend:0;
   var line=ribbons[i];line.enabled=strength>.01f;if(!line.enabled)continue;
   line.widthMultiplier=(carry?.028f:hover?.024f:.018f)*strength;
   for(int j=0;j<48;j++){float u=j/47f,a=u*Mathf.PI*2*(carry?.9f:.7f)+clock*(carry?2.2f:1.8f)+i*2.1f;
    if(carry){points[j]=Orbit(a,(u-.5f)*1.6f); }
    else if(hover){float radius=.65f+u*.2f;points[j]=player+new Vector3(Mathf.Cos(a)*radius,.12f+u*.8f,Mathf.Sin(a)*radius);}
    else {float travel=.8f+Mathf.Repeat((Pulling?-clock:clock)*8+i*1.7f,(Pulling?8:config.gustRange)-.8f);var q=Quaternion.LookRotation(gustDirection);float radius=.06f+u*.20f;points[j]=gustOrigin+q*new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,Mathf.Max(.75f,travel-u*1.3f));}
   }line.SetPositions(points);
  }
  for(int i=0;i<leaves.Length;i++){var l=leaves[i];bool gusting=ReviewView==FlowView.Legacy&&windBlend>.05f;bool active=gusting&&i<8||holdBlend>.1f&&i<4;l.gameObject.SetActive(active);if(!active)continue;float a=clock*2+i*2.399f;
   if(gusting){var q=Quaternion.LookRotation(gustDirection);l.position=gustOrigin+q*new Vector3(Mathf.Sin(a)*(.15f+i*.015f),Mathf.Cos(a*.7f)*.2f,.75f+Mathf.Repeat((Pulling?-clock:clock)*(5+i*.15f)+i*.3f,(Pulling?8:config.gustRange)-.75f));}
   else l.position=Orbit(a,Mathf.Sin(a*.8f)*.7f);
   l.rotation=Quaternion.Euler(clock*160+i*30,a*57,clock*85);l.localScale=Vector3.one*.65f*(gusting?windBlend:holdBlend);
  }
  if(anchor&&AetherSystem.Current==null){var crystal=anchor.transform.Find("Visuals/Crystal");if(crystal)crystal.localRotation=Quaternion.Euler(0,clock*35,0);}
 }
 Vector3 Orbit(float angle,float height){
  Vector3 ext=holdBounds.extents+Vector3.one*.09f;
  // Ellipse circumscribes every XZ corner of the visual bounds, never crossing the object.
  Vector3 local=holdBounds.center+new Vector3(Mathf.Cos(angle)*ext.x*1.45f,height*ext.y,Mathf.Sin(angle)*ext.z*1.45f);
  return holdPosition+holdRotation*local;
 }

}
}

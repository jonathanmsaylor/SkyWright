using UnityEngine;
using UnityEngine.Rendering;
namespace Skywright {
// Bounded world-space wisps, combined into one dynamic mesh per spell.
// No particles, forces, target selection, or runtime material instantiation.
public sealed class WindFlow : MonoBehaviour {
 public bool inward;
 public Material material;
 [Range(0,1)] public float intensity=.72f;
 public Color pearl=new Color(.87f,.98f,1),aqua=new Color(.36f,.86f,.92f);
 [Range(4,18)] public int ribbons=12;
 [Range(0,24)] public int motes=14;
 [Range(.01f,.2f)] public float ribbonWidth=.085f;
 [Range(0,1)] public float turbulence=.24f;
 [Range(.2f,3)] public float curvature=1.3f;
 public float radiusScale=1,lengthScale=1,startup=.22f,fade=.4f;
 [Range(.4f,1)] public float quality=1;
 const int Strips=40,Segments=32,Mask=~((1<<2)|(1<<6));
 struct Wisp {public Vector3 origin;public Quaternion rotation;public float age,life,phase,radial,span;public bool live;}
 readonly Wisp[] wisps=new Wisp[Strips];
 readonly Vector3[] vertices=new Vector3[Strips*Segments*2];
 readonly Color[] colors=new Color[Strips*Segments*2];
 readonly Vector2[] uv=new Vector2[Strips*Segments*2];
 readonly int[] triangles=new int[Strips*(Segments-1)*6];
 readonly Vector3[] curve=new Vector3[Segments];
 readonly Vector3[] targets=new Vector3[4];readonly float[] targetRadii=new float[4];int targetCount;
 Camera camera;Mesh mesh;MeshRenderer renderer;float blend,clock,currentLength=8;int serial;bool previous;
 public int LiveCount{get;private set;}public float Blend=>blend;
 public int VertexBudget=>vertices.Length;
 public void Initialize(Camera cam){
  camera=cam;gameObject.layer=2;transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
  mesh=new Mesh{name=inward?"Suction airflow":"Push airflow"};mesh.MarkDynamic();
  for(int s=0;s<Strips;s++)for(int j=0;j<Segments;j++){
   int v=(s*Segments+j)*2;float u=j/(float)(Segments-1);uv[v]=new Vector2(u,0);uv[v+1]=new Vector2(u,1);
   if(j==Segments-1)continue;int t=(s*(Segments-1)+j)*6;
   triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v+2;triangles[t+4]=v+1;triangles[t+5]=v+3;
  }
  mesh.vertices=vertices;mesh.colors=colors;mesh.uv=uv;mesh.triangles=triangles;
  gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
  renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;renderer.enabled=false;
 }
 public void BeginTargets(){targetCount=0;}
 public void Target(Vector3 at,float radius){if(targetCount>=targets.Length)return;targets[targetCount]=at;targetRadii[targetCount++]=radius;}
 public void Clear(){blend=0;previous=false;LiveCount=0;for(int i=0;i<Strips;i++)wisps[i].live=false;if(renderer)renderer.enabled=false;}
 static float Hash(int n)=>Mathf.Repeat(Mathf.Sin(n*127.1f+311.7f)*43758.5453f,1);
 void Spawn(int i,Vector3 origin,Quaternion rotation,bool fill){
  int n=++serial+i*91;float life=inward?1.25f:1.05f;
  wisps[i]=new Wisp{origin=origin,rotation=rotation,age=fill?Hash(i+15)*life:0,life=life*(.8f+Hash(n)*.4f),phase=Hash(n+1)*Mathf.PI*2,radial=.32f+Hash(n+2)*.65f,span=i<ribbons?.30f+Hash(n+3)*.22f:.035f+Hash(n+3)*.08f,live=true};
 }
 public void Tick(float dt,bool active,Vector3 origin,Vector3 forward,float gameplayLength){
  if(!camera||!renderer)return;
  dt=Mathf.Min(dt,.05f);clock+=dt;blend=Mathf.MoveTowards(blend,active?1:0,dt/Mathf.Max(.01f,active?startup:fade));
  if(blend<.001f&&!active){Clear();return;}
  renderer.enabled=true;int count=Mathf.Min(Strips-8,Mathf.RoundToInt((ribbons+motes)*quality));
  if(active)currentLength=Mathf.Max(.85f,gameplayLength*Mathf.Min(1,lengthScale));float length=currentLength;Quaternion aim=Quaternion.LookRotation(forward,camera.transform.up);
  LiveCount=0;
  for(int i=0;i<Strips;i++){
   if(i>=count){Hide(i);continue;}
   if(!wisps[i].live&&active)Spawn(i,origin,aim,!previous);
   var w=wisps[i];if(!w.live){Hide(i);continue;}
   w.age+=dt;
   if(w.age>=w.life){if(active){Spawn(i,origin,aim,false);w=wisps[i];}else{wisps[i].live=false;Hide(i);continue;}}
   float alignment=Mathf.Clamp01((Vector3.Dot(w.rotation*Vector3.forward,forward)-.45f)/.55f);
   float age=w.age/w.life,head=inward?1-age:age;
   float visibility=Mathf.SmoothStep(0,1,age*7)*Mathf.SmoothStep(0,1,(1-age)*5)*blend*alignment;
   // Retain world-space history on turns. Close wisps softly follow the hand socket.
   Vector3 streamOrigin=Vector3.Lerp(w.origin,origin,.35f);
   Vector3 last=streamOrigin;bool occluded=false;
   for(int j=0;j<Segments;j++){
    float u=j/(float)(Segments-1);
    float axial=Mathf.Clamp01(head+(u-.5f)*w.span),z=.62f+axial*(length-.62f);
    float cone=inward?Mathf.Tan(18*Mathf.Deg2Rad)*z:(.5f+z*.10f);
    float r=cone*w.radial*radiusScale*Mathf.SmoothStep(0,1,axial*3.2f)*.83f;
    float angle=w.phase+(inward?-1:1)*axial*curvature*6+clock*.6f;
    float curl=Mathf.Sin(axial*11+clock*1.7f+w.phase)*turbulence;
    Vector3 p=streamOrigin+w.rotation*new Vector3(.34f*(1-axial)+Mathf.Cos(angle+curl)*r,-.25f*(1-axial)+Mathf.Sin(angle)*r*.78f,z);
    // Cosmetic occlusion only: do not let a sampled ribbon arc cross a solid surface.
    if(!occluded&&Physics.Linecast(j==0?streamOrigin:last,p,out var hit,Mask,QueryTriggerInteraction.Ignore)){p=hit.point;occluded=true;}
    curve[j]=p;last=p;
    if(occluded)for(int k=j+1;k<Segments;k++)curve[k]=p;
    if(occluded)break;
   }
   float width=i<ribbons?ribbonWidth*(.6f+Hash(i+38)*.8f):ribbonWidth*.18f;
   Draw(i,width,visibility*(i<ribbons?.58f:.83f),i*.17f);
   wisps[i]=w;LiveCount++;
  }
  // Short airy curls at the hand: low opacity and a strict projected-width ceiling.
  for(int k=0;k<2;k++){
   for(int j=0;j<Segments;j++){float u=j/(float)(Segments-1),a=clock*(inward?-4:4)+u*4+k*3.1f;curve[j]=origin+aim*new Vector3(.34f+Mathf.Cos(a)*(.035f+u*.04f),-.25f+Mathf.Sin(a)*.05f,.65f+u*.38f);}
   Draw(32+k,.014f,blend*.38f,k);
  }
  if(!inward){
   float pulse=Mathf.Repeat(clock*.65f,1),distance=.9f+pulse*(length-1);
   // One broken pressure crescent, widening and dissolving as it travels outwards.
   for(int j=0;j<Segments;j++){float u=j/(float)(Segments-1),a=u*3.8f+clock*.4f;float r=(.5f+distance*.1f)*.65f;curve[j]=origin+aim*new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,distance+Mathf.Sin(u*6)*.15f);}
   if(!Physics.Raycast(origin,forward,distance,Mask,QueryTriggerInteraction.Ignore))Draw(34,.045f,blend*.22f*Mathf.Sin(pulse*Mathf.PI),2);
   // Surface contact reads from the same casting ray; it never applies gameplay force.
   if(active&&Physics.Raycast(origin,forward,out var impact,length,Mask,QueryTriggerInteraction.Ignore)){
    var plane=Quaternion.LookRotation(impact.normal);float r=.2f+Mathf.Repeat(clock*.9f,1)*.25f;
    for(int j=0;j<Segments;j++){float u=j/(float)(Segments-1),a=u*4.6f+clock*1.2f;curve[j]=impact.point+plane*new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,.035f+u*.12f);}
    Draw(35,.028f,blend*.48f,2);
   }
  }
  // Sparse short arcs only on props the gameplay system actually affected this tick.
  for(int i=0;i<targetCount;i++){
   int slot=Strips-4+i;float phase=clock*(inward?-3:4)+i*2;
   for(int j=0;j<Segments;j++){float u=j/(float)(Segments-1),a=phase+u*2.8f;curve[j]=targets[i]+new Vector3(Mathf.Cos(a),.25f*Mathf.Sin(a*1.4f),Mathf.Sin(a))*targetRadii[i]*1.08f;}
   Draw(slot,.035f,blend*.4f,i);LiveCount++;
  }
  mesh.vertices=vertices;mesh.colors=colors;mesh.bounds=new Bounds(origin,Vector3.one*(length*4+12));previous=active;
 }
 void Draw(int strip,float width,float alpha,float hue){
  for(int j=0;j<Segments;j++){
   float u=j/(float)(Segments-1);Vector3 tangent=curve[Mathf.Min(j+1,Segments-1)]-curve[Mathf.Max(0,j-1)];
   Vector3 across=Vector3.Cross(tangent,camera.transform.position-curve[j]).normalized;
   float taper=Mathf.Pow(Mathf.Max(0,Mathf.Sin(u*Mathf.PI)),.8f);
   float depth=Vector3.Dot(curve[j]-camera.transform.position,camera.transform.forward);
   // World width and projected-width ceilings both apply, before the shader near fade.
   float bounded=Mathf.Min(width*taper,Mathf.Max(0,depth)*.018f);
   int v=(strip*Segments+j)*2;vertices[v]=curve[j]-across*bounded;vertices[v+1]=curve[j]+across*bounded;
   var color=Color.Lerp(aqua,pearl,.5f+.45f*Mathf.Sin(u*5+hue+clock*.4f));color.a=alpha*intensity*taper;
   colors[v]=colors[v+1]=color;
  }
 }
 void Hide(int strip){for(int j=strip*Segments*2;j<(strip+1)*Segments*2;j++)colors[j]=Color.clear;}
 void OnDestroy(){if(mesh)Destroy(mesh);}
}
}

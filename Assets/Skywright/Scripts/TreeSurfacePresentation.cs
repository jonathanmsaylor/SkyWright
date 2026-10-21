using UnityEngine;
namespace Skywright {
// Render-only trunk surface. Never changes harvesting colliders, roots, or saved fracture coordinates.
public class TreeSurfacePresentation:MonoBehaviour {
 public Mesh surface;public Material scarMaterial;
 public Transform FitScar(Transform parent,Vector3 worldPoint,int damage){
  if(!surface||!surface.isReadable)return null;
  var local=transform.InverseTransformPoint(worldPoint);local.y=Mathf.Clamp(local.y,.25f,3.25f);var radial=new Vector3(local.x-.03f,0,local.z).normalized;if(radial.sqrMagnitude<.1f)radial=Vector3.forward;
  var tangent=Vector3.Cross(Vector3.up,radial);var verts=surface.vertices;var triangles=surface.triangles;var points=new Vector3[15];var uv=new Vector2[15];var indices=new int[48];int t=0;
  for(int y=0;y<5;y++)for(int x=0;x<3;x++){
   var target=new Vector3(.03f,local.y+(y-2)*.045f,0)+tangent*((x-1)*(.038f+damage*.004f));var origin=target+radial*1.4f;float best=float.MaxValue;Vector3 normal=radial;
   for(int i=0;i<triangles.Length;i+=3){var a=verts[triangles[i]];var e1=verts[triangles[i+1]]-a;var e2=verts[triangles[i+2]]-a;var h=Vector3.Cross(-radial,e2);float det=Vector3.Dot(e1,h);if(Mathf.Abs(det)<.000001f)continue;float inv=1/det;var s=origin-a;float u=Vector3.Dot(s,h)*inv;if(u<0||u>1)continue;var q=Vector3.Cross(s,e1);float v=Vector3.Dot(-radial,q)*inv;if(v<0||u+v>1)continue;float hit=Vector3.Dot(e2,q)*inv;if(hit>0&&hit<best){best=hit;normal=Vector3.Cross(e1,e2).normalized;if(Vector3.Dot(normal,radial)<0)normal=-normal;}}
   if(best==float.MaxValue)return null;points[y*3+x]=parent.InverseTransformPoint(transform.TransformPoint(origin-radial*best+normal*.003f));uv[y*3+x]=new Vector2(x/2f,y/4f);
   if(x<2&&y<4){int a=y*3+x;indices[t++]=a;indices[t++]=a+3;indices[t++]=a+1;indices[t++]=a+1;indices[t++]=a+3;indices[t++]=a+4;}
  }
  var obj=new GameObject("Trunk fitted bark scar");obj.layer=2;obj.transform.SetParent(parent,false);var mesh=new Mesh{name="Conforming scar"};mesh.vertices=points;mesh.uv=uv;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=scarMaterial;obj.AddComponent<TransientScarMesh>();return obj.transform;
 }
}
class TransientScarMesh:MonoBehaviour {void OnDestroy(){var f=GetComponent<MeshFilter>();if(f&&f.sharedMesh)Destroy(f.sharedMesh);}}
}

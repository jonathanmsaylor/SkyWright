using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace Skywright.Editor {
public static class AuthorAirMagic {
 const string Dir="Assets/Resources/AirMagic";
 static Material pearl,timber,aqua,ribbon;
 static GameObject Child(Transform parent,string name){var g=new GameObject(name);g.transform.SetParent(parent,false);return g;}
 static void Shape(Transform parent,string name,PrimitiveType kind,Vector3 p,Vector3 scale,Material mat){var g=GameObject.CreatePrimitive(kind);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;}
 static Material Mat(string name,Color color,bool transparent=false){string path=Dir+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(transparent?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.3f);
  if(transparent){m.SetFloat("_Surface",1);m.SetFloat("_Blend",0);m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);m.SetFloat("_ZWrite",0);m.SetFloat("_Cull",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;m.SetOverrideTag("RenderType","Transparent");}EditorUtility.SetDirty(m);return m;}
 static Mesh Torus(){var m=new Mesh();var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<=64;i++)for(int j=0;j<=10;j++){float a=i*Mathf.PI*2/64,b=j*Mathf.PI*2/10;v.Add(new Vector3(Mathf.Cos(a)*(.46f+.032f*Mathf.Cos(b)),Mathf.Sin(a)*(.46f+.032f*Mathf.Cos(b)),Mathf.Sin(b)*.032f));}for(int i=0;i<64;i++)for(int j=0;j<10;j++){int k=i*11+j;t.AddRange(new[]{k,k+11,k+1,k+1,k+11,k+12});}m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();string path=Dir+"/SculpturalRing.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(m,existing);UnityEngine.Object.DestroyImmediate(m);return existing;}AssetDatabase.CreateAsset(m,path);return m;}
 static void Ring(Transform root,Mesh mesh,Vector3 p,float size,Quaternion q){var g=Child(root,"Pearl open ring");g.transform.localPosition=p;g.transform.localScale=Vector3.one*size;g.transform.localRotation=q;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=pearl;}
 static GameObject Save(GameObject g,string name){var p=PrefabUtility.SaveAsPrefabAsset(g,Dir+"/"+name+".prefab");UnityEngine.Object.DestroyImmediate(g);return p;}
 [MenuItem("Skywright/Author Air assets (explicit regeneration)")]
 public static void Author(){
  Directory.CreateDirectory(Dir);AssetDatabase.Refresh();pearl=Mat("Pearl",new Color(.86f,.90f,.85f));timber=Mat("PaleTimber",new Color(.71f,.56f,.35f));aqua=Mat("Aqua",new Color(.10f,.61f,.65f));ribbon=Mat("WindRibbon",new Color(.72f,.96f,1,.44f),true);
  Mesh ring=Torus();var tuning=AssetDatabase.LoadAssetAtPath<AirConfig>(Dir+"/Tuning.asset");if(!tuning){tuning=ScriptableObject.CreateInstance<AirConfig>();AssetDatabase.CreateAsset(tuning,Dir+"/Tuning.asset");}
  var e=Resources.Load<EarthConfig>("Earthcasting/Tuning");
  var crate=UnityEngine.Object.Instantiate(e.crate);crate.name="Liftable empty cargo crate";var prop=crate.AddComponent<AirCrate>();prop.box=crate.GetComponent<BoxCollider>();prop.visual=crate.transform;tuning.crate=Save(crate,"LiftableCrate");
  var anchor=new GameObject("Wind anchor");var bp=anchor.AddComponent<BuiltPiece>();bp.type="wind-anchor";var collision=Child(anchor.transform,"Collision").AddComponent<BoxCollider>();collision.center=new Vector3(0,.75f,0);collision.size=new Vector3(1,1.5f,1);bp.boxes=new[]{collision};
  var v=Child(anchor.transform,"Visuals").transform;
  Shape(v,"Pearl foot",PrimitiveType.Cylinder,new Vector3(0,.09f,0),new Vector3(.9f,.09f,.9f),pearl);
  Shape(v,"Timber pedestal",PrimitiveType.Cylinder,new Vector3(0,.43f,0),new Vector3(.34f,.28f,.34f),timber);
  Shape(v,"Satin collar",PrimitiveType.Cylinder,new Vector3(0,.67f,0),new Vector3(.50f,.045f,.50f),pearl);
  Ring(v,ring,new Vector3(0,1.03f,0),1,Quaternion.identity);Ring(v,ring,new Vector3(0,1.03f,0),.8f,Quaternion.Euler(0,70,0));
  Shape(v,"Crystal",PrimitiveType.Sphere,new Vector3(0,1.03f,0),new Vector3(.16f,.33f,.16f),aqua);
  var prefab=Save(anchor,"WindAnchor").GetComponent<BuiltPiece>();
  var catalog=AssetDatabase.LoadAssetAtPath<PieceCatalog>(Dir+"/Catalog.asset");if(!catalog){catalog=ScriptableObject.CreateInstance<PieceCatalog>();AssetDatabase.CreateAsset(catalog,Dir+"/Catalog.asset");}
  catalog.pieces=new[]{new PieceDefinition{typeId="wind-anchor",displayName="Wind anchor",hint="R to hover nearby. Space rises; Ctrl descends.",origin="Ground centre; collision independent of visuals",dimensions=new Vector3(1,1.5f,1),prefab=prefab,anchors=new[]{new SnapAnchor("bottom","Bottom",Vector3.zero)},connections=new PieceConnection[0]}};EditorUtility.SetDirty(catalog);
  var focus=new GameObject("Air focus");Shape(focus.transform,"Pale grip",PrimitiveType.Capsule,new Vector3(0,-.12f,0),new Vector3(.06f,.12f,.06f),timber);Ring(focus.transform,ring,new Vector3(0,.08f,0),.25f,Quaternion.identity);Shape(focus.transform,"Aqua centre",PrimitiveType.Sphere,new Vector3(0,.08f,0),Vector3.one*.045f,aqua);foreach(var tr in focus.GetComponentsInChildren<Transform>())tr.gameObject.layer=2;tuning.focus=Save(focus,"AirFocus");
  tuning.ribbon=ribbon;tuning.leaf=e.leaf;tuning.gather=Sound("Gather",.6f,0);tuning.release=Sound("Gust",.7f,1);tuning.settle=Sound("Settle",.5f,2);tuning.hover=Sound("Rise",1,3);EditorUtility.SetDirty(tuning);AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("AIR_ASSETS_AUTHORED");
 }
 static AudioClip Sound(string name,float duration,int kind){
  const int rate=22050;int n=(int)(rate*duration);string path=Dir+"/"+name+".wav";var random=new System.Random(743+kind);float smooth=0;
  using(var w=new BinaryWriter(File.Create(path))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(n*2);
   for(int i=0;i<n;i++){float t=i/(float)rate,u=t/duration;float noise=(float)random.NextDouble()*2-1;smooth=Mathf.Lerp(smooth,noise,.08f);float envelope=Mathf.Pow(Mathf.Sin(u*Mathf.PI),kind==1?.7f:1.5f);float note=Mathf.Sin(2*Mathf.PI*(kind==2?330:440)*t)*.055f+Mathf.Sin(2*Mathf.PI*660*t)*.03f;float sample=(smooth*(kind==1?1.7f:.9f)+note)*envelope*.7f;w.Write((short)(Mathf.Clamp(sample,-.95f,.95f)*32767));}}
  AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
 }
}
}

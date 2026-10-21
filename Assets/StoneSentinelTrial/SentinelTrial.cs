using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skywright.SentinelReview {
// Dedicated review scene: deliberately has no SkywrightGame, inventory or persistence owner.
public sealed class SentinelTrial : MonoBehaviour {
 public GameObject sentinelPrefab;
 Camera view; RenderTexture captureTarget; Transform subject; float yaw=15, elevation=7, distance=5.1f;
 bool rotate, hideUI, capturing; string viewName="Three-quarter";
 readonly Vector3 placement=new Vector3(3,0,-9);
 GUIStyle title, text; Texture2D panel;
 void Awake(){
  Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
  var world=new GameObject("Existing Skywright island - display only");
  MountainEnvironment.Create(world.transform);MountainEnvironment.Lighting(world.transform);
  subject=Instantiate(sentinelPrefab,placement,Quaternion.identity).transform;
  subject.name="Stone Sentinel - visual trial";
  view=new GameObject("Review camera").AddComponent<Camera>();view.tag="MainCamera";
  view.nearClipPlane=.04f;view.fieldOfView=42;MountainEnvironment.Camera(view);
  view.gameObject.AddComponent<AudioListener>();
  Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
  SetView(0);PositionCamera();
  Debug.Log("SENTINEL_REVIEW_READY: isolated scene; existing island lighting; no saves loaded or written.");
 }
 IEnumerator Start(){
  var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-sentinelcapture");
  if(i<0||i+1>=args.Length)yield break;
  capturing=true;var folder=Path.GetFullPath(args[i+1]);Directory.CreateDirectory(folder);
  captureTarget=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);captureTarget.Create();
  yield return new WaitForSecondsRealtime(4);
  string[] names={"01-three-quarter","02-front","03-side","04-back","05-detail","06-player-distance"};
  for(int j=0;j<names.Length;j++){
   SetView(j);PositionCamera();yield return new WaitForSecondsRealtime(1);
   yield return new WaitForEndOfFrame();
   CaptureTga(Path.Combine(folder,names[j]+".tga"));
   yield return new WaitForSecondsRealtime(.5f);
  }
  view.targetTexture=null;captureTarget.Release();Destroy(captureTarget);
  Debug.Log("SENTINEL_CAPTURE_COMPLETE "+folder);
  yield return new WaitForSecondsRealtime(1);Application.Quit();
 }
 void CaptureTga(string path){
  var request=new UniversalRenderPipeline.SingleCameraRequest{destination=captureTarget};
  if(!RenderPipeline.SupportsRenderRequest(view,request))throw new InvalidOperationException("URP camera capture unsupported");
  RenderPipeline.SubmitRenderRequest(view,request);
  var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
  var previous=RenderTexture.active;RenderTexture.active=captureTarget;
  image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
  RenderTexture.active=previous;
  var pixels=image.GetPixels32();
  using(var writer=new BinaryWriter(File.Create(path))){
   byte[] header=new byte[18];header[2]=2;header[12]=(byte)Screen.width;header[13]=(byte)(Screen.width>>8);header[14]=(byte)Screen.height;header[15]=(byte)(Screen.height>>8);header[16]=24;
   writer.Write(header);foreach(var p in pixels){writer.Write(p.b);writer.Write(p.g);writer.Write(p.r);}
  }
  Destroy(image);
 }
 void Update(){
  if(capturing)return;
  var k=Keyboard.current;var m=Mouse.current;
  if(k!=null){
   if(k.escapeKey.wasPressedThisFrame)Application.Quit();
   if(k.digit1Key.wasPressedThisFrame)SetView(0);
   if(k.digit2Key.wasPressedThisFrame)SetView(1);
   if(k.digit3Key.wasPressedThisFrame)SetView(2);
   if(k.digit4Key.wasPressedThisFrame)SetView(3);
   if(k.digit5Key.wasPressedThisFrame)SetView(4);
   if(k.digit6Key.wasPressedThisFrame)SetView(5);
   if(k.spaceKey.wasPressedThisFrame)rotate=!rotate;
   if(k.hKey.wasPressedThisFrame)hideUI=!hideUI;
  }
  if(m!=null){
   if(m.rightButton.isPressed){var delta=m.delta.ReadValue();yaw+=delta.x*.2f;elevation=Mathf.Clamp(elevation-delta.y*.15f,-8,65);viewName="Orbit";}
   distance=Mathf.Clamp(distance-m.scroll.ReadValue().y*.002f,1.5f,12);
  }
  if(rotate){yaw+=Time.unscaledDeltaTime*16;viewName="Turntable";}
  PositionCamera();
 }
 void OnApplicationFocus(bool focus){Application.targetFrameRate=focus?60:15;}
 void SetView(int n){
  rotate=false;elevation=7;distance=5.1f;
  switch(n){
   case 1:yaw=0;viewName="Front";break;
   case 2:yaw=90;viewName="Side";break;
   case 3:yaw=180;viewName="Back";break;
   case 4:yaw=8;distance=2.7f;elevation=11;viewName="Material detail";break;
   case 5:yaw=15;distance=8;elevation=3;viewName="Player distance";break;
   default:yaw=25;viewName="Three-quarter";break;
  }
 }
 void PositionCamera(){
  var target=placement+Vector3.up*(viewName=="Material detail"?1.52f:1.18f);
  view.transform.position=target+Quaternion.Euler(-elevation,yaw,0)*Vector3.forward*distance;
  view.transform.LookAt(target);
 }
 void OnGUI(){
  if(hideUI)return;
  if(title==null){
   title=new GUIStyle(GUI.skin.label){fontSize=25,fontStyle=FontStyle.Bold};title.normal.textColor=new Color(.1f,.23f,.25f);
   text=new GUIStyle(GUI.skin.label){fontSize=16};text.normal.textColor=new Color(.12f,.23f,.25f);
   panel=new Texture2D(1,1);panel.SetPixel(0,0,new Color(.88f,1,1,.86f));panel.Apply();
  }
  GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1600f,Screen.height/900f,1));
  GUI.DrawTexture(new Rect(24,24,440,92),panel);
  GUI.Label(new Rect(43,35,410,36),"STONE SENTINEL",title);
  GUI.Label(new Rect(44,73,410,28),"Visual trial  /  Skywright daylight  /  "+viewName,text);
  GUI.DrawTexture(new Rect(290,826,1020,51),panel);
  GUI.Label(new Rect(310,838,990,28),"RMB drag: orbit   •   Wheel: zoom   •   1–6: views   •   Space: turntable   •   H: hide UI   •   Esc: exit",text);
 }
}
}

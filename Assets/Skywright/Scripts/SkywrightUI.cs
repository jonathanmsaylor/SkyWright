using System;
using System.IO;
using UnityEngine;
namespace Skywright {
public partial class SkywrightGame {
 GUIStyle title,sub,label,small,button,card,panelStyle,glassStyle,hudText,hudSmall;Texture2D[] icons;
 static readonly Vector2[] shadowOffsets={new Vector2(-1,0),new Vector2(1,0),new Vector2(0,-1),new Vector2(0,1)};
 readonly Color ink=new Color(.83f,.96f,.95f,.89f),cream=new Color(.055f,.18f,.22f),mint=new Color(.49f,.87f,.85f);
 void Styles(){
  if(title!=null)return;
  title=new GUIStyle(GUI.skin.label){fontSize=44,fontStyle=FontStyle.Bold,padding=new RectOffset(0,0,0,0)};title.normal.textColor=cream;
  sub=new GUIStyle(GUI.skin.label){fontSize=22,fontStyle=FontStyle.Normal,padding=new RectOffset(0,0,0,0)};sub.normal.textColor=cream;
  label=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true,padding=new RectOffset(0,0,0,0)};label.normal.textColor=cream;
  small=new GUIStyle(label){fontSize=14};small.normal.textColor=new Color(.12f,.29f,.32f);
  button=new GUIStyle(GUI.skin.button){fontSize=19,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(12,12,8,8)};
  button.normal.background=Rounded(new Color(.94f,1,.99f,.83f));button.normal.textColor=cream;
  button.hover.background=Rounded(new Color(.65f,.92f,.91f,.98f));button.hover.textColor=cream;
  button.active.background=Rounded(new Color(.43f,.79f,.80f,.98f));button.active.textColor=cream;button.focused=button.hover;button.border=new RectOffset(18,18,18,18);
  panelStyle=new GUIStyle{border=new RectOffset(18,18,18,18)};panelStyle.normal.background=Rounded(Color.white);
  glassStyle=new GUIStyle{border=new RectOffset(18,18,18,18)};glassStyle.normal.background=Rounded(new Color(.79f,.96f,.97f,.18f),.14f);
  hudText=new GUIStyle(label){fontSize=18,alignment=TextAnchor.MiddleCenter};hudText.normal.textColor=Color.white;
  hudSmall=new GUIStyle(hudText){fontSize=16};
  card=new GUIStyle(button){alignment=TextAnchor.LowerCenter,fontSize=16,padding=new RectOffset(8,8,8,12)};
  icons=new Texture2D[Kit.Ids.Length];for(int i=0;i<icons.Length;i++)icons[i]=KitIcons.Make(Kit.Ids[i]);
 }
 static Texture2D Solid(Color c){var t=new Texture2D(1,1);t.SetPixel(0,0,c);t.Apply();return t;}
 static Texture2D Rounded(Color tint,float rim=0){
  const int n=64;var t=new Texture2D(n,n,TextureFormat.RGBA32,false);t.wrapMode=TextureWrapMode.Clamp;
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){
   Vector2 q=new Vector2(Mathf.Abs(x-31.5f)-15.5f,Mathf.Abs(y-31.5f)-15.5f);
   float d=new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-16;
   float alpha=Mathf.Clamp01(.5f-d),edge=Mathf.Clamp01(1-Mathf.Abs(d+1));Color c=Color.Lerp(tint,Color.white,edge*.46f+(y/63f)*.04f);c.a=(tint.a+edge*rim)*alpha;t.SetPixel(x,y,c);
  }t.Apply();return t;
 }
 void Panel(Rect r,Color c){Color old=GUI.color;GUI.color=c;if(r.width>=1600)GUI.DrawTexture(r,Texture2D.whiteTexture);else GUI.Box(r,GUIContent.none,panelStyle);GUI.color=old;}
 void Text(float x,float y,float w,float h,string s,GUIStyle style=null){var st=style??label;GUI.Label(new Rect(x,y,w,Mathf.Max(h,st.CalcHeight(new GUIContent(s),w)+3)),s,st);}
 bool Button(float x,float y,float w,float h,string s){return GUI.Button(new Rect(x,y,w,h),s,button);}
 void GlassText(Rect r,string text,GUIStyle style){
  Color old=style.normal.textColor;style.normal.textColor=new Color(.025f,.08f,.10f,.9f);
  foreach(var offset in shadowOffsets)GUI.Label(new Rect(r.position+offset,r.size),text,style);
  style.normal.textColor=old;GUI.Label(r,text,style);
 }
 void BottomGlass(string primary,string secondary,string feedback=""){
  const float width=600,padding=12,gap=3;float inner=width-padding*2;
  float first=Mathf.Max(22,hudText.CalcHeight(new GUIContent(primary),inner));
  float second=Mathf.Max(20,hudSmall.CalcHeight(new GUIContent(secondary),inner));
  float third=string.IsNullOrEmpty(feedback)?0:hudSmall.CalcHeight(new GUIContent(feedback),inner)+gap;
  float height=Mathf.Max(60,first+second+third+gap+16),x=(1600-width)/2,y=900-16-height;
  GUI.Box(new Rect(x,y,width,height),GUIContent.none,glassStyle);
  GlassText(new Rect(x+padding,y+8,inner,first),primary,hudText);
  GlassText(new Rect(x+padding,y+8+first+gap,inner,second),secondary,hudSmall);
  if(third>0)GlassText(new Rect(x+padding,y+8+first+gap+second+gap,inner,third-gap),feedback,hudSmall);
 }
 void DrawRushingWind(float strength){
  if(strength<=.01f||Event.current.type!=EventType.Repaint)return;var matrix=GUI.matrix;var color=GUI.color;
  for(int i=0;i<24;i++){float a=i*Mathf.PI*2/24;float phase=Mathf.Repeat(Time.time*.8f+i*.618f,1);var direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a));var p=new Vector2(800,450)+Vector2.Scale(direction,new Vector2(690+phase*170,370+phase*110));
   GUI.color=new Color(.82f,.97f,1,strength*.22f*Mathf.Sin(phase*Mathf.PI));GUIUtility.RotateAroundPivot(a*Mathf.Rad2Deg,p);GUI.DrawTexture(new Rect(p.x,p.y,28+phase*55,1.5f),Texture2D.whiteTexture);GUI.matrix=matrix;
  }GUI.color=color;
 }
 void OnGUI(){
  // IMGUI buttons otherwise accept middle/right clicks and could close the palette mid-shortcut.
  if((menu||palette||help||aetherPanel)&&(Event.current.type==EventType.MouseDown||Event.current.type==EventType.MouseUp)&&Event.current.button!=0)Event.current.Use();
  if(builder==null)return;Styles();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1600f,Screen.height/900f,1));
  if(SentinelCombat.Active){DrawSentinelCombat();return;}
  if(menu){
   Panel(new Rect(0,0,1600,900),new Color(.025f,.055f,.07f,.43f));Panel(new Rect(80,70,570,760),ink);
   Text(120,105,480,70,"SKYWRIGHT",title);Text(123,178,470,35,NaturalPreview.Active?"ISLAND & AETHER / VISUAL PREVIEW":airMode?"ELEMENTAL TRIAL":earthMode?"EARTHCASTING TRIAL":"FIRST HOME",sub);
   Text(123,228,465,65,NaturalPreview.Active?"Explore your island. Gather Aether.\nRestore its wind and build toward the sky.":airMode?"Lift a crate. Shape the wind.\nBuild a home with room to breathe.":earthMode?"Summon stone. Fracture an ash tree.\nBring its wood back to your dock.":"An island to call your own.\nBuild toward the horizon.");
   Text(123,304,470,26,NaturalPreview.Active?(NaturalPreview.QA?"VISUAL VERIFICATION SLOT":"ISOLATED VISUAL PREVIEW SLOT"):airMode?"ELEMENTAL SLOT  •  Earlier saves stay separate":earthMode?"EARTH TRIAL SLOT  •  Creative saves stay separate":qa?"VERIFICATION SLOT  •  Separate from your home":artPreview?"ART PREVIEW SLOT  •  Separate from your home":"CREATIVE PROTOTYPE  •  Unlimited materials",small);
   float y=350;
   if(started){if(Button(120,y,490,48,"Return to island"))Resume();y+=60;}
   if(Button(120,y,490,48,NaturalPreview.Active?(started?"Reset visual sample":"Explore visual sample"):airMode?(started?"Reset elemental trial":"Begin elemental trial"):earthMode?(started?"Reset harvesting trial":"Begin harvesting trial"):started?"Start a fresh island":"Begin building")){if(started&&builder.pieces.Count>0)confirmNew=true;else Fresh();}y+=60;
   GUI.enabled=File.Exists(savePath);if(Button(120,y,490,48,NaturalPreview.Active?"Continue saved visual preview":airMode?"Continue elemental trial":earthMode?"Continue harvesting trial":artPreview?"Continue saved preview":"Continue saved home")){if(dirty){notice="Save your changes before loading, or start fresh to discard.";noticeUntil=Time.unscaledTime+5;}else Load(savePath);}GUI.enabled=true;y+=60;
   if(started){if(Button(120,y,238,45,earthMode?"Save trial":artPreview?"Save preview":"Save home"))Save();if(Button(372,y,238,45,"Recover position")){Recover();Resume();}y+=57;}
   string demo=Path.Combine(Application.streamingAssetsPath,artPreview?"art-sample.json":"demonstration.json");
   if(airMode){string old=Path.Combine(Application.persistentDataPath,qa?"import-earthcasting.json":"earthcasting-trial.json");GUI.enabled=File.Exists(old);if(Button(120,y,490,44,"Import Earthcasting save — copy")){if(dirty)Notify("Save changes or start fresh before importing.");else Load(old,true);}GUI.enabled=true;y+=50;
    if(started&&Button(120,y,490,36,"Recover demonstration crates")){air.RecoverCrates();Resume();}if(started)y+=43;
   }else {GUI.enabled=!earthMode&&File.Exists(demo);if(Button(120,y,490,44,earthMode?"Separate trial — demo disabled":artPreview?"Explore art sample":"Explore demonstration")){if(dirty)Notify("Save changes before loading the demonstration.");else Load(demo);}GUI.enabled=true;y+=57;}
   if(Button(120,y,238,42,"Controls")){help=true;menu=false;CursorState();}
   if(Button(372,y,238,42,"Quit")){if(dirty){Notify("Unsaved changes. Save before quitting, or use Quit without saving.");confirmNew=false;}else Application.Quit();}y+=52;
   if(dirty&&Button(120,y,490,34,"Quit without saving"))Application.Quit();
   Panel(new Rect(700,600,700,230),ink);
   Text(725,620,660,45,airMode?"EARTH, AIR, POSSIBILITY":earthMode?"EARTH, WOOD, POSSIBILITY":"YOUR FIRST CREATION",sub);
   Text(725,670,640,90,airMode?"Q selects Earth or Air. Tab equips magic.\nHold LMB suction; hold RMB push; Shift+LMB grab.\nDouble jump to glide; R toggles field flight.":earthMode?"E collects nearby loose stones. Tab equips Earthcasting.\nTrees take five strikes; limestone takes eight.\nApproach the wood, then spend it extending your dock.":"Choose a deck. Add walls and a roof.\nExtend a dock over the clouds.\nMake a corner of the sky yours.");
   Text(725,786,650,30,NaturalPreview.Active?"M1.10.2  /  Rock contact & dome polish":airMode?"M1.7.5  /  A clearer view":earthMode?"M1.6  /  Harvesting experiment  /  No air abilities yet":"M1.5 A  /  Art sample  /  Awaiting your visual review",small);
   if(confirmNew){
    Panel(new Rect(680,280,650,220),ink);Text(710,310,590,60,"Start fresh? Unsaved changes will be discarded.");
    if(Button(710,400,270,48,"Start fresh")){confirmNew=false;Fresh();}
    if(Button(1000,400,290,48,"Keep building")){confirmNew=false;Resume();}
   }
  } else if(aetherPanel&&air?.aether!=null){
   var a=air.aether;var state=a.state;bool active=inspectedAnchor&&a.IsPrimary(inspectedAnchor.id);
   Panel(new Rect(450,230,700,430),new Color(.83f,.96f,.96f,.88f));
   Text(486,262,620,50,"Wind anchor",title);
   Text(488,326,620,32,a.Stage.name+(active?"":" • Dormant"),sub);
   Text(488,370,620,34,"Aether "+state.balance+"   •   Wood "+earth.state.wood+"   •   Stone "+earth.state.stone);
   if(state.tier<3){
    var next=a.config.stages[state.tier+1];
    Text(488,416,620,52,"Next: "+next.name+"\n"+a.CostText);
    Text(488,476,620,44,state.tier==2?"Attune the home island and awaken its distant current.":"Expand the field to "+next.radius+" m for hovering and construction.",small);
    GUI.enabled=active;
    if(Button(488,540,380,48,"Feed the anchor")){if(a.Upgrade(inspectedAnchor)){aetherPanel=false;Resume();}}
    GUI.enabled=true;
   }else Text(488,420,620,90,"The home island is attuned.\nA sky current has awakened toward the next island.\nEnter the current to visit the neighboring island.");
   if(Button(890,540,222,48,"Return")){aetherPanel=false;Resume();}
   if(!active)Text(488,603,620,38,"One anchor sustains the field. Paid upgrades remain with the island.",small);
  } else if(help){
   Panel(new Rect(240,80,1120,740),ink);Text(290,110,900,55,"Make yourself at home",title);
   Text(290,190,1000,390,
    "W A S D   Move     •     Mouse   Look     •     Shift   Sprint     •     Space   Jump\n\n"+
    "B   Open the building kit     •     F   Put away / equip construction\n"+
    "1–9   Select structural pieces     •     [ / ]   Previous / next piece\n\n"+
    "Left click   Place     •     Right click   Open / close kit     •     Middle click   Remove\n"+
    "Wheel   Rotate 45°     •     Shift + wheel   Adjust reach\n"+
    "Hold Alt   Free placement     •     Shift + Tab   Cycle named attachment points (Auto, edges, top / bottom)\n\n"+
    "Shift + middle click   Select the built piece you are looking at\n\n"+
    "F5   Save     •     Escape   Menu     •     Recover position   Return safely\n\n"+
    (airMode?"Q Earth / Air • Tab Magic • Hold LMB Suction • Hold RMB Push • Shift+LMB Grab\nAir: Wheel distance • Shift+wheel height • Double jump Glide / R Flight • Space/Ctrl Up/Down • E Anchor\nEarth: E collect loose stones • LMB summon / launch / hold to strike • RMB recall":earthMode?"Tab   Earthcasting     •     E   Collect nearby stones     •     LMB Summon / launch / strike • RMB Recall":"Docks may extend from other decks. Supports are visual; pieces never collapse."));
   Text(290,600,300,30,"Mouse sensitivity");
   sensitivity=GUI.HorizontalSlider(new Rect(610,614,440,25),sensitivity,.025f,.25f);
   Text(1080,600,160,30,sensitivity.ToString("F3"));
   if(Button(290,710,970,50,"Back")){help=false;menu=true;CursorState();}
  } else if(palette){
   Panel(new Rect(90,60,1420,780),ink);Text(130,90,1000,55,"The building kit",title);Text(132,155,1000,30,earthMode?"Deck: "+earth.config.deckCost+" wood. Other pieces are free trial pieces. Wood: "+earth.state.wood:"Unlimited materials. Choose a piece, then make it yours.",label);
   for(int i=0;i<TutorialIsland.BuildableCount;i++){
    int col=i%5,row=i/5;Rect r=new Rect(130+col*270,215+row*180,250,162);
    Color old=GUI.backgroundColor;GUI.backgroundColor=i==builder.index?mint:Color.white;
    if(GUI.Button(r,Kit.Names[i],card)){earth?.Equip(false);air?.Equip(false);builder.Select(i);builder.enabled=true;palette=false;inputDelay=Time.unscaledTime+.2f;CursorState();}GUI.backgroundColor=old;
    GUI.DrawTexture(new Rect(r.x+69,r.y+6,112,112),icons[i],ScaleMode.ScaleToFit,true);
   }
   Text(132,780,1050,28,"RMB   Build selected piece     •     B / Escape   Close",small);
  } else {
   Panel(new Rect(30,24,330,78),ink);Text(50,34,280,28,"SKYWRIGHT",sub);
   Text(50,68,290,24,earth!=null?"Stone "+earth.state.stone+" • Wood "+earth.state.wood+(air?.aether!=null?" • Aether "+air.aether.state.balance:""):"First Home  •  Creative",small);
   if(air?.aether!=null){var a=air.aether;Panel(new Rect(1110,24,460,92),ink);
    Text(1130,34,420,28,a.state.tier==3?"Island attuned • current open":"Next: "+a.config.stages[a.state.tier+1].name,sub);
    if(a.state.tier<3){var n=a.config.stages[a.state.tier+1];Text(1130,73,420,25,"Aether "+a.state.balance+"/"+n.aether+"   Wood "+earth.state.wood+"/"+n.wood+"   Stone "+earth.state.stone+"/"+n.stone,small);}
    else Text(1130,73,420,25,"Shift: faster flight • Follow the sky current",small);
   }else {Panel(new Rect(1210,24,360,62),ink);Text(1230,43,320,28,builder.pieces.Count+" pieces  •  "+(dirty?"Unsaved":"Saved"),label);}
   DrawRushingWind(air!=null?air.Rush:0);
   Color oldCross=GUI.color;GUI.color=Color.white;GUI.DrawTexture(new Rect(796,449,8,2),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(799,446,2,8),Texture2D.whiteTexture);GUI.color=oldCross;
   if(builder.enabled&&builder.snapped&&!builder.free){
    Vector3 point=cam.WorldToScreenPoint(builder.snapPoint);
    if(point.z>0){float sx=point.x*1600/Screen.width,sy=900-point.y*900/Screen.height;
     Color old=GUI.color;GUI.color=builder.valid?new Color(.45f,.85f,1):new Color(1,.5f,.35f);
     GUI.DrawTexture(new Rect(sx-5,sy-5,10,2),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(sx-5,sy+3,10,2),Texture2D.whiteTexture);
     GUI.DrawTexture(new Rect(sx-5,sy-3,2,6),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(sx+3,sy-3,2,6),Texture2D.whiteTexture);GUI.color=old;}
   }
   if(builder.enabled&&(earth==null||!earth.equipped)&&(air==null||!air.equipped)){
    string name=Kit.Names[builder.index]+(earth!=null?" • "+earth.CostHint(Kit.Ids[builder.index]).Replace("Free trial piece","Free"):"");
    string snap=builder.free?"Free placement":"Alt Free placement • Shift+Tab "+builder.SnapMode+(string.IsNullOrEmpty(builder.snapLabel)?"":" • "+builder.snapLabel);
    BottomGlass(name+"  •  LMB Place  •  RMB Kit",snap+" • MMB Delete • Shift+MMB Copy",builder.valid?"Wheel Rotate • Shift+wheel Reach "+builder.reach.ToString("F1")+" m":builder.reason);
   }else{
    bool magic=earth!=null&&(earth.equipped||air!=null&&air.equipped);
    string primary=air!=null&&air.equipped?(air.held?"Shift+LMB Release • Wheel Distance":"Hold LMB Suction • Hold RMB Push • Shift+LMB Grab"):earth!=null&&earth.equipped?earth.hint:"Tab "+(airSelected?"Air wand":"Earth wand")+" • B Kit • F Build";
    if(string.IsNullOrEmpty(primary))primary=earth!=null&&earth.equipped?earth.phase:"";
    string secondary=magic?"Tab Stow • Q "+(airSelected?"Earth":"Air")+" • R Hover":"R Hover • F5 Save • Esc Menu";
    if(air!=null&&air.hovering)secondary="Space Up • Ctrl Down • R Hover"+(magic?" • Tab Stow":"");
    if(air!=null&&air.equipped&&air.held)secondary="Wheel Distance • Shift+wheel Height • Tab Stow";
    if(air?.aether!=null&&!string.IsNullOrEmpty(air.aether.Prompt))secondary=air.aether.Prompt+" • "+secondary;
    if(air!=null&&air.Travelling){primary="Wind current • carrying you to the next shore";secondary="Look around • movement resumes on arrival";}
    else if(air!=null&&air.hovering&&air.aether!=null&&air.aether.state.tier==3)secondary+=" • Shift Fast flight";
    else secondary+=" • Double jump Glide";
    string aimed="";if(Physics.Raycast(cam.ViewportPointToRay(new Vector3(.5f,.5f,0)),out var aimedHit,builder.reach,~((1<<2)|(1<<6)),QueryTriggerInteraction.Ignore)){var b=aimedHit.collider.GetComponentInParent<BuiltPiece>();if(b&&!TutorialIsland.Fixed(b))aimed="F Build • MMB Delete in build mode • Shift+MMB Copy";}
    BottomGlass(primary,secondary,aimed);
   }
  }
  if(!help&&!palette&&Time.unscaledTime<noticeUntil){
   float tx=menu?700:470,tw=menu?780:660;float h=Mathf.Max(58,label.CalcHeight(new GUIContent(notice),tw-40)+28);Color old=GUI.color;GUI.color=new Color(1,1,1,Mathf.Clamp01((noticeUntil-Time.unscaledTime)/.18f));Panel(new Rect(tx,112,tw,h),ink);Text(tx+20,126,tw-40,h-24,notice);GUI.color=old;
  }
 }
}
public static class KitIcons {
 public static Texture2D Make(string type){
  var b=Kit.Create(type,Vector3.zero,Quaternion.identity,null,true);
  var filters=b.GetComponentsInChildren<MeshFilter>();Quaternion view=Quaternion.Euler(24,-35,0);
  var points=new System.Collections.Generic.List<(Vector3 a,Vector3 b,Vector3 c,Color color)>();
  Vector2 min=Vector2.one*999,max=Vector2.one*-999;
  foreach(var f in filters){var m=f.sharedMesh;if(!m||!f.GetComponent<Renderer>()||!f.GetComponent<Renderer>().enabled)continue;var vs=m.vertices;var ts=m.triangles;var color=f.GetComponent<Renderer>().sharedMaterial.color;
   for(int j=0;j<ts.Length;j+=3){Vector3 a=view*f.transform.TransformPoint(vs[ts[j]]),c=view*f.transform.TransformPoint(vs[ts[j+1]]),d=view*f.transform.TransformPoint(vs[ts[j+2]]);
    foreach(var v in new[]{a,c,d}){min=Vector2.Min(min,v);max=Vector2.Max(max,v);}points.Add((a,c,d,color));}
  }
  const int n=96;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);Color[] pixels=new Color[n*n];float[] depth=new float[n*n];for(int i=0;i<depth.Length;i++)depth[i]=float.MaxValue;
  float scale=80/Mathf.Max(max.x-min.x,max.y-min.y);Vector2 mid=(min+max)*.5f;
  foreach(var tri in points){
   Vector3 a=new Vector3((tri.a.x-mid.x)*scale+48,(tri.a.y-mid.y)*scale+48,tri.a.z);
   Vector3 b1=new Vector3((tri.b.x-mid.x)*scale+48,(tri.b.y-mid.y)*scale+48,tri.b.z);
   Vector3 c=new Vector3((tri.c.x-mid.x)*scale+48,(tri.c.y-mid.y)*scale+48,tri.c.z);
   float area=Cross(b1-a,c-a);if(Mathf.Abs(area)<.001f)continue;
   float shade=.65f+.35f*Mathf.Abs(Vector3.Dot(Vector3.Cross(tri.b-tri.a,tri.c-tri.a).normalized,new Vector3(.3f,.7f,-.6f).normalized));
   Color co=tri.color*shade;co.a=1;
   for(int y=Mathf.Max(0,Mathf.FloorToInt(Mathf.Min(a.y,Mathf.Min(b1.y,c.y))));y<=Mathf.Min(95,Mathf.CeilToInt(Mathf.Max(a.y,Mathf.Max(b1.y,c.y))));y++)
    for(int x=Mathf.Max(0,Mathf.FloorToInt(Mathf.Min(a.x,Mathf.Min(b1.x,c.x))));x<=Mathf.Min(95,Mathf.CeilToInt(Mathf.Max(a.x,Mathf.Max(b1.x,c.x))));x++){
     Vector3 p=new Vector3(x+.5f,y+.5f,0);float u=Cross(b1-p,c-p)/area,v=Cross(c-p,a-p)/area,w=1-u-v;
     if(u<0||v<0||w<0)continue;float z=u*a.z+v*b1.z+w*c.z;int idx=y*n+x;if(z<depth[idx]){depth[idx]=z;pixels[idx]=co;}
    }
  }
  tex.SetPixels(pixels);tex.Apply();UnityEngine.Object.Destroy(b.gameObject);return tex;
 }
 static float Cross(Vector3 a,Vector3 b)=>a.x*b.y-a.y*b.x;
}
}

Shader "Skywright/AeroSky" {
 Properties { _Zenith("Zenith",Color)=(0.045,0.28,0.65,1) _Horizon("Horizon",Color)=(0.40,0.75,0.87,1) }
 SubShader {
 Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
 Cull Off ZWrite Off
 Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 positionOS:POSITION;};
 struct V {float4 positionCS:SV_POSITION;float3 direction:TEXCOORD0;};
 float4 _Zenith,_Horizon;
 V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.direction=a.positionOS.xyz;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 float fbm(float2 p){float v=0,a=.55;for(int i=0;i<5;i++){v+=noise(p)*a;p=p*2.03+11.7;a*=.5;}return v;}
 half4 frag(V i):SV_Target{
  float3 d=normalize(i.direction);float h=saturate(d.y);
  float3 sky=lerp(_Horizon.rgb,_Zenith.rgb,pow(h,.65));
  if(d.y>0.025){
   float2 p=d.xz/(d.y+.12)*1.3;
   float n=fbm(p);float detail=fbm(p*2.5+21);
   float coverage=smoothstep(.53,.73,n+detail*.12);
   coverage*=smoothstep(.03,.14,d.y)*(1-smoothstep(.70,.96,d.y));
   float3 cloud=lerp(float3(.69,.79,.84),float3(.98,.98,.92),saturate((n-.4)*3+detail*.3));
   sky=lerp(sky,cloud,coverage*.94);
  }
  return half4(sky,1);
 }
 ENDHLSL
 }
 }
}

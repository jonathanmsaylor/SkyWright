Shader "Skywright/AetherBubble" {
 Properties {_Shimmer("Suction shimmer",Float)=0 _Tint("Pearl aqua",Color)=(.58,.94,.97,1)}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
 Pass {Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Back
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;};
 struct V{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;};
 CBUFFER_START(UnityPerMaterial)
 half4 _Tint;float _Shimmer;
 CBUFFER_END
 V vert(A v){V o;o.world=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(v.normalOS);return o;}
 half4 frag(V i):SV_Target{
  float3 n=normalize(i.normal),view=normalize(GetCameraPositionWS()-i.world);
  float rim=pow(1-saturate(dot(n,view)),3.5);
  float highlight=pow(saturate(dot(n,normalize(view+float3(-.4,.8,.25)))),70);
  float silk=.5+.5*sin(n.y*5+n.x*3+_Time.y*.6);
  half3 color=lerp(_Tint.rgb,half3(.94,.78,1),silk*.25);color=lerp(color,half3(1,1,1),highlight);
  float shimmer=_Shimmer*(.12+.08*sin(_Time.y*7+i.world.y*9));return half4(lerp(color,half3(1,1,1),shimmer),.045+rim*.5+highlight*.55+shimmer);
 }
 ENDHLSL
 }}
}

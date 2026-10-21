Shader "Skywright/NaturalMeadow" {
 Properties { _BaseMap("Grass",2D)="white"{} }
 SubShader {
 Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
 Pass {
 Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;};
 struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;half fog:TEXCOORD2;};
 V vert(A a){V o;VertexPositionInputs v=GetVertexPositionInputs(a.positionOS.xyz);o.positionCS=v.positionCS;o.world=v.positionWS;o.normal=TransformObjectToWorldNormal(a.normalOS);o.fog=ComputeFogFactor(v.positionCS.z);return o;}
 half4 frag(V i):SV_Target{
  float2 uv=i.world.xz/1.4;half3 grass=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb;
  half3 alternate=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv*.43+float2(.25,.51)).rgb;
  grass=lerp(grass,alternate,.18)*half3(.9,1,.78);
  float blend=1-smoothstep(8,13,length(i.world.xz-float2(-6,-10)));
  half3 base=lerp(half3(.133,.196,.068),grass,blend);
  Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));half3 n=normalize(i.normal);
  half3 col=base*(SampleSH(n)+sun.color*saturate(dot(n,sun.direction))*sun.shadowAttenuation);
  return half4(MixFog(col,i.fog),1);
 }
 ENDHLSL
 }
 }
}

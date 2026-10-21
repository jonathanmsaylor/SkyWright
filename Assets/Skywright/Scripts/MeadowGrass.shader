Shader "Skywright/MeadowGrass" {
 Properties { _BaseMap("Grass atlas",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1) _Cutoff("Cutout",Range(0,1))=.38 _Wind("Breeze",Float)=.035 }
 SubShader {
 Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"} Cull Off
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor;float _Cutoff;float _Wind;
 CBUFFER_END
 struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct V {float4 p:SV_POSITION;float3 ws:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;float4 color:COLOR;float fog:TEXCOORD3;};
 V vert(A a){V o;o.ws=TransformObjectToWorld(a.p.xyz);float phase=o.ws.x*.74+o.ws.z*.51;float bend=(sin(_Time.y*1.8+phase)+.35*sin(_Time.y*2.7-phase*.43))*a.uv.y*a.uv.y*_Wind;o.ws.x+=bend;o.ws.z+=bend*.37;o.p=TransformWorldToHClip(o.ws);o.n=TransformObjectToWorldNormal(a.n);o.uv=a.uv;o.color=a.color;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 sampleGrass(V v){half4 tex=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,v.uv);clip(tex.a-_Cutoff);float fade=1-smoothstep(55,82,distance(_WorldSpaceCameraPos,v.ws));float hash=frac(52.9829189*frac(dot(v.p.xy,float2(.06711056,.00583715))));clip(fade-hash);return tex;}
 ENDHLSL
 Pass {Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
 #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
 #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
 #pragma multi_compile_fog
 half4 frag(V v):SV_Target {half4 tex=sampleGrass(v);float3 n=normalize(v.n);InputData d=(InputData)0;d.positionWS=v.ws;d.positionCS=v.p;d.normalWS=n;d.viewDirectionWS=SafeNormalize(GetWorldSpaceViewDir(v.ws));d.shadowCoord=TransformWorldToShadowCoord(v.ws);d.bakedGI=SampleSH(n);d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(v.p);d.shadowMask=1;SurfaceData s=(SurfaceData)0;s.albedo=tex.rgb*_BaseColor.rgb*v.color.rgb;s.alpha=1;s.smoothness=.08;s.occlusion=lerp(.77,1,v.uv.y);s.normalTS=float3(0,0,1);Light light=GetMainLight(d.shadowCoord);s.emission=s.albedo*.10*saturate(dot(-light.direction,d.viewDirectionWS))*light.color;half4 c=UniversalFragmentPBR(d,s);c.rgb=MixFog(c.rgb,v.fog);return c;}
 ENDHLSL }
 Pass {Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ZWrite On ColorMask 0
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment depth
 half4 depth(V v):SV_Target {sampleGrass(v);return 0;}
 ENDHLSL }
 Pass {Name "DepthOnly" Tags {"LightMode"="DepthOnly"} ZWrite On ColorMask R
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment depth
 half4 depth(V v):SV_Target {sampleGrass(v);return 0;}
 ENDHLSL }
 }
}

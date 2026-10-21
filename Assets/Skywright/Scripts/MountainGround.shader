Shader "Skywright/MountainGround" {
 Properties {
 _Grass("Meadow",2D)="white"{} _GrassNormal("Meadow normal",2D)="bump"{}
 _Soil("Path soil",2D)="white"{} _SoilNormal("Soil normal",2D)="bump"{} _Litter("Woodland floor",2D)="white"{}
 _Rock("Limestone",2D)="white"{} _RockNormal("Limestone normal",2D)="bump"{}
 _BaseMap("Pass texture",2D)="white"{} _BaseColor("Pass color",Color)=(1,1,1,1)
 _Cutoff("Cutoff",Float)=0.5 _Cull("Cull",Float)=2
 }
 SubShader {
 Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
 Pass {
 Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
 #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
 #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_Grass);SAMPLER(sampler_Grass);TEXTURE2D(_GrassNormal);SAMPLER(sampler_GrassNormal);
 TEXTURE2D(_Rock);SAMPLER(sampler_Rock);TEXTURE2D(_RockNormal);SAMPLER(sampler_RockNormal);
 TEXTURE2D(_Soil);SAMPLER(sampler_Soil);TEXTURE2D(_SoilNormal);SAMPLER(sampler_SoilNormal);TEXTURE2D(_Litter);SAMPLER(sampler_Litter);
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise2(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float4 color:COLOR;};
 struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;float3 normalWS:TEXCOORD1;float4 color:COLOR;half fog:TEXCOORD2;};
 V vert(A a){V o;o.positionWS=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(a.normalOS);o.color=a.color;o.fog=ComputeFogFactor(o.positionCS.z);return o;}
 half4 frag(V v):SV_Target {
 float3 p=v.positionWS,n=normalize(v.normalWS),w=pow(abs(n),4);w/=max(dot(w,1),.001);
 float2 uvX=p.zy*.19,uvY=p.xz*.19,uvZ=p.xy*.19;
 float3 rock=SAMPLE_TEXTURE2D(_Rock,sampler_Rock,uvX).rgb*w.x+SAMPLE_TEXTURE2D(_Rock,sampler_Rock,uvY).rgb*w.y+SAMPLE_TEXTURE2D(_Rock,sampler_Rock,uvZ).rgb*w.z;
 float3 grass=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,p.xz*.55).rgb;
 float3 soil=SAMPLE_TEXTURE2D(_Soil,sampler_Soil,p.xz*.552).rgb;
 float3 litter=SAMPLE_TEXTURE2D(_Litter,sampler_Litter,p.xz*.558).rgb;
 float macro=noise2(p.xz*.052);
 float rocky=smoothstep(.82,.53,n.y)+smoothstep(75,170,p.y)*.52+noise2(p.xz*.037)*.13;
 rocky=saturate(max(rocky,v.color.g));
 float path=smoothstep(.1,.82,v.color.r+noise2(p.xz*1.2)*.26)*(1-rocky);
 float woodland=max(exp(-dot((p.xz-float2(-29,1))/float2(11,17),(p.xz-float2(-29,1))/float2(11,17))),exp(-dot((p.xz-float2(-38,27))/float2(12,14),(p.xz-float2(-38,27))/float2(12,14))));
 float3 meadow=lerp(grass*lerp(float3(.83,1.03,.76),float3(1.16,1.18,.91),macro),litter,woodland*.7);
 float3 albedo=lerp(meadow,rock*lerp(float3(.68,.72,.70),float3(1.02,1.04,.98),(noise2(p.xz*.029+p.yy*.018)*.7+noise2(p.xz*.075+p.yy*.032)*.3)),rocky);
 // Broad mineral streaks are world-space and do not tile at the detail texture scale.
 float mineral=noise2(float2(p.x*.19+p.z*.11,p.y*.027));
 float seams=noise2(float2((p.x+p.z)*.09,p.y*.65+noise2(p.xz*.04)*3));
 float weather=lerp(.72,1.03,smoothstep(.18,.78,mineral))*lerp(.83,1.0,smoothstep(.2,.65,seams));
 albedo*=lerp(1.0,weather,rocky);
 albedo=lerp(albedo,soil*float3(1.13,1.08,.96),path*.85);
 float3 gn=UnpackNormal(SAMPLE_TEXTURE2D(_GrassNormal,sampler_GrassNormal,p.xz*.55));
 float3 rnX=UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal,sampler_RockNormal,uvX));
 float3 rnY=UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal,sampler_RockNormal,uvY));
 float3 rnZ=UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal,sampler_RockNormal,uvZ));
 float3 detail=float3(rnX.z*sign(n.x),rnX.y,rnX.x)*w.x+float3(rnY.x,rnY.z*sign(n.y),rnY.y)*w.y+float3(rnZ.x,rnZ.y,rnZ.z*sign(n.z))*w.z;
 float3 worldGrass=normalize(n+float3(gn.x,0,gn.y)*.28);
 n=normalize(lerp(worldGrass,normalize(lerp(n,detail,.40)),rocky));
 InputData input=(InputData)0;input.positionWS=p;input.positionCS=v.positionCS;input.normalWS=n;input.viewDirectionWS=SafeNormalize(GetWorldSpaceViewDir(p));input.shadowCoord=TransformWorldToShadowCoord(p);input.bakedGI=SampleSH(n);input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(v.positionCS);input.shadowMask=1;
 SurfaceData surface=(SurfaceData)0;surface.albedo=albedo;surface.alpha=1;surface.occlusion=1;surface.smoothness=.12;surface.normalTS=float3(0,0,1);
 half4 result=UniversalFragmentPBR(input,surface);result.rgb=MixFog(result.rgb,v.fog);return result;
 }
 ENDHLSL
 }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 UsePass "Universal Render Pipeline/Lit/DepthNormals"
 }
}

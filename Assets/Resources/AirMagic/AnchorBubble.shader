Shader "Skywright/AnchorBubble" {
 Properties { _Tint("Tint",Color)=(0.65,0.94,1,1) _Hemisphere("Island dome",Float)=0 _Shoreline("Authored shoreline",2D)="white"{} _FieldRadius("Radius",Float)=1 _FarthestShore("Farthest shore",Float)=1 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   ZTest LEqual
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;};
   struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float height:TEXCOORD2;};
   CBUFFER_START(UnityPerMaterial)
   half4 _Tint;float _Hemisphere,_FieldRadius,_FarthestShore;
   CBUFFER_END
   TEXTURE2D(_Shoreline);SAMPLER(sampler_Shoreline);
   V vert(A v){V o;o.height=v.positionOS.y;o.world=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(v.normalOS);return o;}
   half4 frag(V i):SV_Target {
    float3 local=normalize(TransformWorldToObject(i.world));float visibility=1;
    float longitude=atan2(local.z,local.x);
    if(_Hemisphere>.5&&local.y<0){
     float shore=SAMPLE_TEXTURE2D_LOD(_Shoreline,sampler_Shoreline,float2(frac(longitude/6.2831853),.5),0).r;
     shore=lerp(shore,_FarthestShore,smoothstep(.85,1,-local.y));
     float reveal=smoothstep(0,6,_FieldRadius-shore);
     visibility=saturate(reveal*25)*smoothstep(-reveal*1.1-.04,-reveal*1.1+.04,local.y);
     clip(visibility-.001);
    }
    float edge=pow(1-abs(dot(normalize(i.normal),normalize(GetCameraPositionWS()-i.world))),2.8);
    float sheen=.5+.5*sin(i.world.y*.65+i.world.x*.17-_Time.y*.35);
    half3 tint=lerp(_Tint.rgb,half3(.96,.83,1),sheen*.2);
    float rim=_Hemisphere*saturate(1-abs(i.height)*180)*.10;
    float latitude=asin(clamp(local.y,-1,1));
    float latPhase=latitude*36-_Time.y*.08,lonPhase=longitude*24+_Time.y*.04;
    // Angular lines converge into a sunburst at the poles. Fade that
    // coordinate singularity and subpixel lines, retaining the field rim.
    float wind=pow(saturate(.5+.5*cos(latPhase)),64)*.065*(1-smoothstep(.3,1.5,fwidth(latPhase)));
    wind+=pow(saturate(.5+.5*cos(lonPhase)),96)*.035*(1-smoothstep(.3,1.5,fwidth(lonPhase)));
    wind*=smoothstep(.025,.14,1-abs(local.y));
    return half4(tint,(.022+edge*.34+rim+wind)*visibility);
   }
   ENDHLSL
  }
 }
}

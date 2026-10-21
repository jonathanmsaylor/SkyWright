Shader "Skywright/AeroFlow" {
 Properties { _Opacity("Opacity",Range(0,1))=.8 _DepthFade("Surface softness",Float)=.32 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
   struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;float depth:TEXCOORD1;};
   CBUFFER_START(UnityPerMaterial)
   float _Opacity,_DepthFade;
   CBUFFER_END
   V vert(A i){V o;float3 p=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(p);o.depth=-TransformWorldToView(p).z;o.uv=i.uv;o.color=i.color;return o;}
   half4 frag(V i):SV_Target{
    float across=abs(i.uv.y*2-1);
    float edge=pow(saturate(1-across),1.6);
    float filament=.78+.22*sin(i.uv.y*24+i.uv.x*9-_Time.y*3);
    float scene=LinearEyeDepth(SampleSceneDepth(i.positionCS.xy/_ScaledScreenParams.xy),_ZBufferParams);
    float soft=saturate((scene-i.depth)/max(.01,_DepthFade));
    float near=smoothstep(.35,1.05,i.depth);
    float width=saturate(18*fwidth(i.uv.y));
    return half4(i.color.rgb,i.color.a*_Opacity*edge*filament*soft*near*width);
   }
   ENDHLSL
  }
 }
}

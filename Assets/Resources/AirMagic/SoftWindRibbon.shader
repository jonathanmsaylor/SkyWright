Shader "Skywright/SoftWindRibbon" {
 Properties { _BaseColor("Wind tint",Color)=(.72,.96,1,.44) _NearStart("Near fade start",Float)=.4 _NearEnd("Near fade end",Float)=1.6 _MaxPixelWidth("Soft width budget",Float)=24 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off ZTest LEqual Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float depth:TEXCOORD1;half4 color:COLOR;};
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;float _NearStart,_NearEnd,_MaxPixelWidth;
   CBUFFER_END
   V vert(A i){V o;float3 world=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(world);o.depth=-TransformWorldToView(world).z;o.uv=i.uv;o.color=i.color;return o;}
   half4 frag(V i):SV_Target{
    float nearFade=smoothstep(_NearStart,_NearEnd,i.depth);
    // A camera-facing line near the eye can cover most of the viewport. Bound its
    // brightness by projected width, rather than letting it resemble a light flash.
    float widthFade=saturate(_MaxPixelWidth*fwidth(i.uv.y));
    float edge=smoothstep(0,.22,i.uv.y)*smoothstep(0,.22,1-i.uv.y);
    return half4(_BaseColor.rgb*i.color.rgb,_BaseColor.a*i.color.a*nearFade*widthFade*edge);
   }
   ENDHLSL
  }
 }
}

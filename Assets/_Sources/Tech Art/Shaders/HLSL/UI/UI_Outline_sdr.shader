Shader "Custom/UI/Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Outer Outline)]
        [Toggle(USE_OUTLINE)] _UseOutline ("Enable Outline", Float) = 0
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size", Range(0, 10)) = 5
        _OutlineSoftness ("Outline Softness", Range(0.1, 1)) = 0.5
        [IntRange] _OutlineSamples ("Outline Samples", Range(4, 16)) = 8

        [Header(Stencil)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma shader_feature_local _ USE_OUTLINE

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                half4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _ClipRect;
                float4 _MainTex_ST;
                half4 _OutlineColor;
                float _OutlineSize;
                float _OutlineSoftness;
                float _OutlineSamples;
            CBUFFER_END

            float4 _MainTex_TexelSize;

            half4 ApplyOutline(half4 color, half4 finalColor, v2f IN)
            {
                half outlineAlpha = 0;
                float2 texelSize = _MainTex_TexelSize.xy * _OutlineSize;
                int samples = (int)_OutlineSamples;
                
                for (int i = 0; i < samples; i++)
                {
                    float angle = (i / (float)samples) * 6.28318530718;
                    float2 offset = float2(cos(angle), sin(angle)) * texelSize;
                    half sampleAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord + offset).a;
                    outlineAlpha += sampleAlpha;
                }
                
                outlineAlpha /= samples;
                
                outlineAlpha = saturate(outlineAlpha / _OutlineSoftness);
                outlineAlpha *= (1.0 - color.a);
                
                half4 outline = _OutlineColor;
                finalColor.rgb = lerp(outline.rgb, color.rgb, color.a);
                finalColor.a = saturate(color.a + outlineAlpha * _OutlineColor.a);

                return finalColor;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = TransformObjectToHClip(v.vertex.xyz);

                OUT.texcoord = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;

                OUT.color = v.color * _Color;
                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord);
                half4 finalColor = color;

                #ifdef USE_OUTLINE
                    finalColor = ApplyOutline(color, finalColor, IN);
                #endif

                finalColor.xyz *= IN.color.xyz;

                #ifdef UNITY_UI_CLIP_RECT
                half2 clipPos = IN.worldPosition.xy;
                half2 inside = step(_ClipRect.xy, clipPos) * step(clipPos, _ClipRect.zw);
                finalColor.a *= inside.x * inside.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif

                return finalColor;
            }
        ENDHLSL
        }
    }
}
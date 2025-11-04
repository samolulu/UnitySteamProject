Shader "Custom/ModelOutLine"
{
    Properties
    {
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" { }
        [MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)
        
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _VertexOffset ("顶点偏移", Float) = 0
        _Power("Power", Range(0.0, 10.0)) = 1
        _FresnelIntensity("FresnelIntensity", Range(0.0, 5.0)) = 1
        
        _Cull ("Cull", Int) = 2
    }

    SubShader
    {
        Pass
        {
            Cull[_Cull]
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //需要光照系统时增加此引入
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _BaseMap_ST;
            half _Cutoff;
            half _VertexOffset;
            half _Power;
            half _FresnelIntensity;
            CBUFFER_END

            // Used in Standard (Physically Based) shader
            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                //通过法向偏移顶点
                half3 positionOS = input.positionOS.xyz;
                positionOS += input.normalOS * _VertexOffset;

                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.normalWS = normalInput.normalWS;
                output.positionWS = TransformObjectToWorld(positionOS);

                return output;
            }

            half4 LitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                //---------------输入数据-----------------
                float2 UV = input.uv;
                half3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));

                half fresnel = saturate(1.0 - dot(input.normalWS, viewDirWS));
                fresnel = pow(fresnel, _Power);

                half4 color = _BaseColor * _FresnelIntensity;
                // clip(color.r - _Cutoff);

                return half4(color.rgb, fresnel);
            }
            ENDHLSL

        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    // CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.RoleShader"

}

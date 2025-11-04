Shader "Custom/URP/CloudShader"
{
    Properties
    {
        _CloudTex ("Cloud Noise Texture", 2D) = "white" {} // 云层噪声纹理
        _NoiseChannel ("Noise Channel (Base)", Vector) = (1, 0, 0) // 基准RGB通道混合值
        // 新增：通道偏移控制
        _NoiseChannelOffsetRange ("Channel Offset Range", Range(0, 0.5)) = 0.2 // 偏移幅度（0=无变化，0.5=最大偏移）
        _NoiseChannelOffsetSpeed ("Channel Offset Speed", Range(0, 0.5)) = 0.1 // 偏移变化速度
        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 1) // 云层颜色
        _CloudDensity ("Cloud Density", Range(0.1, 5.0)) = 1.0 // 云层密度
        _CloudScale ("Cloud Scale", Range(0.1, 100.0)) = 2.0 // 云层缩放
        _CloudSpeed ("Cloud Speed", Range(0, 1.0)) = 0.1 // 云层整体移动速度
        _LightIntensity ("Light Intensity", Range(0.1, 3.0)) = 1.5 // 光照强度
        _WindDirection ("Wind Direction", Vector) = (1, 0, 0) // 风向控制
        //_ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.5 // 阴影强度
        //_myShadowBias ("Shadow Bias", Range(0, 0.1)) = 0.01 // 阴影偏移，防止自阴影
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+1" // 比水晚渲染
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "CastShadows"="True" 
            //"ReceiveShadows"="True"
        }
        
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back

        // 主渲染Pass - 带阴影接收
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // 阴影相关宏
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // 声明属性变量（新增偏移相关属性）
            CBUFFER_START(UnityPerMaterial)
            float4 _CloudTex_ST;
            half4 _CloudColor;
            half _CloudDensity;
            half _CloudScale;
            half _CloudSpeed;
            half _LightIntensity;
            float3 _WindDirection;
            float3 _NoiseChannel; // 基准通道值
            half _NoiseChannelOffsetRange; // 偏移幅度
            half _NoiseChannelOffsetSpeed; // 偏移速度
            half _ShadowIntensity;
            half _myShadowBias;
            float _GlobalCloundDensity = 1.0f; //全局浓度
            CBUFFER_END

            TEXTURE2D(_CloudTex);
            SAMPLER(sampler_CloudTex);

            // 顶点输入结构
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            // 顶点输出结构（新增动态通道值，避免片段着色器重复计算）
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 worldPos     : TEXCOORD1;
                float4 shadowCoord  : TEXCOORD2;
                float3 dynamicNoiseChannel : TEXCOORD3; // 传递动态偏移后的通道值
            };

            // 顶点着色器（核心：计算动态通道偏移）
            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.worldPos);
                // 原有UV动画（云层整体移动）
                OUT.uv = IN.uv * _CloudScale + _Time.y * _CloudSpeed * _WindDirection.xz;
                
                // -------------------------- 新增：动态通道偏移计算 --------------------------
                // 1. 生成随时间变化的"随机"偏移量（用sin/cos区分RGB通道，避免同步变化）
                // _Time.y * _NoiseChannelOffsetSpeed：控制变化速率
                // sin/cos周期不同，使RGB通道偏移独立，效果更自然
                float time = _Time.y * _NoiseChannelOffsetSpeed;
                float3 channelOffset = float3(
                    sin(time),          // R通道偏移（正弦曲线）
                    cos(time * 1.2),    // G通道偏移（余弦曲线，速率1.2倍，避免同步）
                    sin(time * 0.8)     // B通道偏移（正弦曲线，速率0.8倍，差异化）
                );
                
                // 2. 将偏移量归一化到 [-_NoiseChannelOffsetRange, _NoiseChannelOffsetRange]
                // sin/cos输出范围是[-1,1]，乘以范围后得到目标偏移区间
                channelOffset = channelOffset * _NoiseChannelOffsetRange;
                
                // 3. 叠加基准通道值，并限制在[0,1]（避免通道值为负或超过1导致异常）
                OUT.dynamicNoiseChannel = saturate(_NoiseChannel + channelOffset);
                // --------------------------------------------------------------------------

                // 修改阴影坐标计算方式，使用URP正确的方法
                Light mainLight = GetMainLight();
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.worldPos + mainLight.direction * _myShadowBias);
                
                return OUT;
            }

            // 片段着色器（修改：使用动态通道值采样）
            half4 frag (Varyings IN) : SV_Target
            {
                // 1. 采样噪声纹理（原有逻辑）
                float3 tex = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, IN.uv).rgb;
                
                // 2. 使用动态偏移后的通道值混合噪声（核心修改点）
                half noise = dot(tex*_GlobalCloundDensity, IN.dynamicNoiseChannel);
                
                // 3. 原有云层形状计算（基于动态噪声值）
                half cloud = saturate((noise - 0.5) * _CloudDensity + 0.5);
                
                // 无云层时直接透明
                if (cloud < 0.01)
                    discard;

                // 原有光照与阴影计算
                Light mainLight = GetMainLight(IN.shadowCoord);
                half3 lightDir = mainLight.direction;
                half3 normal = normalize(float3(0, 1, 0)); // 云层朝上法线
                half NdotL = saturate(dot(normal, lightDir));
                //half shadowAttenuation = 1.0 - (1.0 - mainLight.shadowAttenuation) * _ShadowIntensity;
                half light = (NdotL * _LightIntensity + 0.2); // +0.2为环境光基础

                // 最终颜色（alpha由云层密度控制）
                half4 finalColor = _CloudColor;
                finalColor.rgb *= light;
                finalColor.a *= cloud;

                return finalColor;
            }
            ENDHLSL
        }

        // 阴影投射Pass（同步修改：使用动态通道值，确保阴影随云朵形状变化）
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            Cull Back
            ColorMask 0 // 仅输出深度，不输出颜色

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 声明属性（包含新增偏移属性）
            CBUFFER_START(UnityPerMaterial)
            float4 _CloudTex_ST;
            half _CloudDensity;
            half _CloudScale;
            half _CloudSpeed;
            float3 _WindDirection;
            float3 _NoiseChannel;
            half _NoiseChannelOffsetRange;
            half _NoiseChannelOffsetSpeed;
            float _GlobalCloundDensity;
            CBUFFER_END

            TEXTURE2D(_CloudTex);
            SAMPLER(sampler_CloudTex);

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 dynamicNoiseChannel : TEXCOORD1; // 动态通道值
            };

            // 顶点着色器（同步计算动态通道偏移）
            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv * _CloudScale + _Time.y * _CloudSpeed * _WindDirection.xz;
                
                // 与主Pass一致的动态通道偏移计算
                float time = _Time.y * _NoiseChannelOffsetSpeed;
                float3 channelOffset = float3(
                    sin(time), 
                    cos(time * 1.2), 
                    sin(time * 0.8)
                ) * _NoiseChannelOffsetRange;
                OUT.dynamicNoiseChannel = saturate(_NoiseChannel + channelOffset);
                
                return OUT;
            }

            // 片段着色器（使用动态通道值判断阴影区域）
            half4 frag (Varyings IN) : SV_Target
            {
                float3 tex = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, IN.uv).rgb;
                half noise = dot(tex*_GlobalCloundDensity, IN.dynamicNoiseChannel); // 动态通道混合
                half cloud = saturate((noise - 0.5) * _CloudDensity + 0.5);
                
                // 阈值0.5：仅密度足够的云层才投射阴影（避免阴影过淡）
                if (cloud < 0.5)
                    discard;
                    
                return 0; // 输出深度（ColorMask 0，颜色无效）
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}

Shader "Custom/BeybladePlatformCleanURP"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.09, 0.10, 0.13, 1)
        _TopColor("Top Tint", Color) = (0.14, 0.16, 0.20, 1)
        _GlowColor("Glow Color", Color) = (0.0, 0.85, 1.0, 1)

        _FresnelPower("Fresnel Power", Range(0.5, 8)) = 3.5
        _EdgeGlowStrength("Edge Glow Strength", Range(0, 8)) = 2.8

        _TopMaskSharpness("Top Mask Sharpness", Range(1, 20)) = 6
        _TopBlendStrength("Top Blend Strength", Range(0, 1)) = 0.65

        _PulseSpeed("Pulse Speed", Range(0, 5)) = 1.2
        _PulseStrength("Pulse Strength", Range(0, 3)) = 0.5

        _HexScale("Hex Scale", Float) = 10
        _HexGlowStrength("Hex Glow Strength", Range(0, 2)) = 0.2

        _SpecularStrength("Specular Strength", Range(0, 2)) = 0.45
        _GlossPower("Gloss Power", Range(8, 128)) = 48
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _TopColor;
                half4 _GlowColor;
                half _FresnelPower;
                half _EdgeGlowStrength;
                half _TopMaskSharpness;
                half _TopBlendStrength;
                half _PulseSpeed;
                half _PulseStrength;
                half _HexScale;
                half _HexGlowStrength;
                half _SpecularStrength;
                half _GlossPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs norm = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = pos.positionCS;
                OUT.positionWS = pos.positionWS;
                OUT.normalWS = normalize(norm.normalWS);
                OUT.viewDirWS = GetWorldSpaceViewDir(pos.positionWS);
                OUT.uv = IN.uv;
                return OUT;
            }

            float HexPattern(float2 uv, float scale)
            {
                uv *= scale;

                float2 gv = frac(uv) - 0.5;
                float2 id = floor(uv);

                if (fmod(id.y, 2.0) > 0.5)
                    gv.x = frac(uv.x + 0.5) - 0.5;

                gv = abs(gv);

                float d = max(dot(gv, normalize(float2(1.0, 1.73205)) * 0.5), gv.x);
                float ring = smoothstep(0.30, 0.26, d) * (1.0 - smoothstep(0.24, 0.20, d));
                return saturate(ring);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);
                float3 H = normalize(L + V);

                float NdotL = saturate(dot(N, L));
                float NdotH = saturate(dot(N, H));

                float3 baseCol = _BaseColor.rgb;

                float topMask = pow(saturate(N.y), _TopMaskSharpness);
                baseCol = lerp(baseCol, _TopColor.rgb, topMask * _TopBlendStrength);

                float diffuse = 0.25 + NdotL;

                float spec = pow(NdotH, _GlossPower) * _SpecularStrength;

                float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);

                float pulse = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed);
                float hex = HexPattern(IN.uv, _HexScale);

                float edgeGlow = fresnel * _EdgeGlowStrength;
                float topHexGlow = hex * topMask * _HexGlowStrength * (0.35 + pulse * _PulseStrength);

                float3 lit = baseCol * diffuse * mainLight.color + spec * mainLight.color;

                float3 emission = _GlowColor.rgb * (edgeGlow + topHexGlow);

                float3 finalColor = lit + emission;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
Shader "Custom/TriplanarGrid"
{
    Properties
    {
        [Header(Grid Colors)]
        [HDR] _LineColor ("Line Color (alpha = line opacity)", Color) = (0.3, 1.0, 0.3, 1.0)
        _BackgroundColor ("Background Color", Color)                  = (0.0, 0.0, 0.0, 1.0)

        [Header(Grid Settings)]
        _CellSize        ("Cell Size (world units)", Float) = 1.0
        _LineThickness   ("Line Thickness (pixels)", Range(0.5, 5.0)) = 1.0

        [Header(Triplanar)]
        _BlendSharpness  ("Blend Sharpness", Range(1.0, 16.0)) = 4.0

        [Header(Matrix Transition)]
        _MatrixBlend     ("Matrix Blend (0 = hidden, 1 = full grid)", Range(0.0, 1.0)) = 1.0
        _GlitchAmount    ("Glitch Amount (0 = none, 1 = max)", Range(0.0, 1.0)) = 0.0
        _GlitchSpeed     ("Glitch Speed", Float) = 12.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Opaque"
            "RenderPipeline"  = "UniversalPipeline"
            "Queue"           = "Geometry"
        }
        LOD 100

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _LineColor;
                float4 _BackgroundColor;
                float  _CellSize;
                float  _LineThickness;
                float  _BlendSharpness;
                float  _MatrixBlend;
                float  _GlitchAmount;
                float  _GlitchSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float  fogCoord    : TEXCOORD2;
                float4 screenPos   : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // 가벼운 해시 노이즈 — 글리치용
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs vIn = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nIn = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = vIn.positionCS;
                OUT.positionWS  = vIn.positionWS;
                OUT.normalWS    = nIn.normalWS;
                OUT.screenPos   = ComputeScreenPos(vIn.positionCS);
                OUT.fogCoord    = ComputeFogFactor(vIn.positionCS.z);

                return OUT;
            }

            float GridMask(float2 uv)
            {
                float2 cell    = uv / _CellSize;
                float2 dF      = fwidth(cell) * _LineThickness;
                float2 grid    = abs(frac(cell - 0.5) - 0.5) / max(dF, 1e-5);
                float  lineVal = 1.0 - min(min(grid.x, grid.y), 1.0);
                return saturate(lineVal);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float3 wp = IN.positionWS;
                float3 n  = normalize(IN.normalWS);

                float gx = GridMask(wp.yz);
                float gy = GridMask(wp.xz);
                float gz = GridMask(wp.xy);

                float3 w = pow(abs(n), _BlendSharpness);
                w /= max(w.x + w.y + w.z, 1e-5);

                float gridStrength = gx * w.x + gy * w.y + gz * w.z;

                // 글리치: 스크린 좌표 + 시간 기반 노이즈로 라인 강도 흔들기
                float2 sUV    = IN.screenPos.xy / max(IN.screenPos.w, 1e-5);
                float  tTick  = floor(_Time.y * _GlitchSpeed);
                float  noise  = Hash21(floor(sUV * 80.0) + tTick);
                float  glitchMask = step(1.0 - _GlitchAmount * 0.5, noise);
                gridStrength = saturate(gridStrength + glitchMask * _GlitchAmount);

                // 매트릭스 블렌드: 페이드인/아웃 시 라인 강도와 알파를 함께 감쇠
                float lineInfluence = gridStrength * _LineColor.a * _MatrixBlend;

                half3 rgb = lerp(_BackgroundColor.rgb, _LineColor.rgb, lineInfluence);

                rgb = MixFog(rgb, IN.fogCoord);

                return half4(rgb, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}

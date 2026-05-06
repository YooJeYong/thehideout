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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _LineColor;
                float4 _BackgroundColor;
                float  _CellSize;
                float  _LineThickness;
                float  _BlendSharpness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float  fogCoord    : TEXCOORD2;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vIn = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nIn = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = vIn.positionCS;
                OUT.positionWS  = vIn.positionWS;
                OUT.normalWS    = nIn.normalWS;
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
                float3 wp = IN.positionWS;
                float3 n  = normalize(IN.normalWS);

                float gx = GridMask(wp.yz);
                float gy = GridMask(wp.xz);
                float gz = GridMask(wp.xy);

                float3 w = pow(abs(n), _BlendSharpness);
                w /= max(w.x + w.y + w.z, 1e-5);

                float gridStrength = gx * w.x + gy * w.y + gz * w.z;

                float lineInfluence = gridStrength * _LineColor.a;

                half3 rgb = lerp(_BackgroundColor.rgb, _LineColor.rgb, lineInfluence);

                rgb = MixFog(rgb, IN.fogCoord);

                return half4(rgb, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}

Shader "Hidden/Dither" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
        _NumLevels("Number of Quantization Levels", Float) = 2

    }
    SubShader
    {
        CGINCLUDE
            #include "UnityCG.cginc"

            struct VertexData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Texture2D _MainTex;
            SamplerState point_clamp_sampler;
            float4 _MainTex_TexelSize;

            v2f vp(VertexData v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG

        // Pixelated Dithering pass
        Pass {
            CGPROGRAM

            #pragma vertex vp
            #pragma fragment fp

            #define MAX_LEVELS 7

            uniform float4 _ColorPalette[16];
            float _NumLevels;
            float _Spread;

            static const float bayer8[8 * 8] = {
                0, 32, 8, 40, 2, 34, 10, 42,
                48, 16, 56, 24, 50, 18, 58, 26,
                12, 44,  4, 36, 14, 46,  6, 38,
                60, 28, 52, 20, 62, 30, 54, 22,
                3, 35, 11, 43,  1, 33,  9, 41,
                51, 19, 59, 27, 49, 17, 57, 25,
                15, 47,  7, 39, 13, 45,  5, 37,
                63, 31, 55, 23, 61, 29, 53, 21
            };

            fixed4 fp(v2f iV) : SV_Target {
                float4 col = _MainTex.Sample(point_clamp_sampler, iV.uv);

                uint x = iV.uv.x * _MainTex_TexelSize.z;
                uint y = iV.uv.y * _MainTex_TexelSize.w;

                float4 output = col + _Spread * (float(bayer8[(x % 8) + (y % 8) * 8]) * (1.0f / 64.0f) - 0.5f);

                float4 sampled = output;
                float4 color = float4(0, 0, 0, 1);
                half dist = 1000;

                [unroll]
                for (int i = 0; i < MAX_LEVELS; ++i)
                {
                    if (i >= _NumLevels) break;

                    float4 paletteColor = _ColorPalette[i];

                    half3 diff = (half3)sampled.rgb - (half3)paletteColor.rgb;
                    half testDist = dot(diff, diff);

                    if (testDist < dist)
                    {
                        dist = testDist;
                        color = paletteColor;
                    }
                }

                return float4(color.rgb, 1);
            }
            ENDCG
        }
            
        // Final pass
        Pass {
            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            fixed4 fp(v2f i) : SV_Target {
                return _MainTex.Sample(point_clamp_sampler, i.uv);
            }
            ENDCG
        }
    }
}
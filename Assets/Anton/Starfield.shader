Shader "Custom/Starfield"
{
    Properties
    {
        _Intensity("Star Intensity", Float) = 1
        _BandMask("Milky Way Band Mask", 2D) = "white" {}
        _NoiseTex("Milky Way Band Mask", 2D) = "white" {}
        _DustIntensity("Dust Intensity", Float) = 0.5
        _DustTiling("Dust Tiling", Float) = 5.0
        _StarFalloffStart("Star Falloff Start", Float) = 0.04
        _StarFalloffEnd("Star Falloff End", Float) = 0.002
        _PoleColorTop("Top Pole Color", Color) = (1, 1, 1, 1)
        _PoleColorBottom("Bottom Pole Color", Color) = (0, 0, 1, 1)
        _PoleFadeSharpness("Pole Edge Falloff", Range(0.01, 2.0)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        Cull Front
        LOD 100

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _Intensity;
            float _DustIntensity;
            float _DustTiling;
            float _StarFalloffStart;
            float _StarFalloffEnd;
            float4 _BandMask_ST;
            float4 _NoiseTex_ST;

            float4 _PoleColorTop;
            float4 _PoleColorBottom;
            float _PoleFadeSharpness;

            TEXTURE2D(_BandMask);   SAMPLER(sampler_BandMask);
            TEXTURE2D(_NoiseTex);   SAMPLER(sampler_NoiseTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 normal : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float2x2 Rot(float a)
            {
                float s = sin(a);
                float c = cos(a);
                return float2x2(c, -s, s, c);
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Star(float2 uv, float flare)
            {
                float d = length(uv);
                float m = 0.05 / d;

                float rays = max(0., 1. - abs(uv.x * uv.y * 1000.));
                m += rays * flare;
                uv = mul(uv, Rot(3.1415 / 4.));
                rays = max(0., 1. - abs(uv.x * uv.y * 1000.));
                m += rays * 0.3 * flare;

                //m *= smoothstep(_StarFalloffStart, _StarFalloffEnd, d);
                m *= pow(saturate(1.0 - d / _StarFalloffStart), _StarFalloffEnd);
                return m;
            }

            float3 StarLayer(float2 uv, float time)
            {
                float3 col = float3(0,0,0);

                float2 gv = frac(uv) - 0.5;
                float2 id = floor(uv);

                float3 starColors[5] = {
                    float3(1.0, 1.0, 1.0),    // white
                    float3(1.0, 0.95, 0.75),  // yellowish
                    float3(1.0, 0.8, 0.6),    // orange-white
                    float3(0.6, 0.6, 1.0),    // bluish
                    float3(0.8, 0.6, 1.0)     // purple-blue
                };
                
                for(int y = -1; y <= 1; y++) {
                    for(int x = -1; x <= 1; x++) {
                        float2 offs = float2(x, y);
                        float n = Hash21(id + offs);
                        float size = frac(n * 345.32);

                        float2 offset = gv - offs - float2(n, frac(n * 34.0)) + 0.5;
                        float star = Star(offset, smoothstep(0.9, 1.0, size) * 0.6);
                        
                        //float3 color = sin(float3(0.2, 0.3, 0.9) * frac(n * 2345.2) * 1.2) * 0.5 + 0.5;
                        //color = color * float3(1.0, 0.25, 1.0 + size) + float3(0.2, 0.2, 0.1) * 2.0;
                        int index = (int)(frac(n * 23.0) * 5);
                        float3 baseColor = starColors[index];

                        // Slight size-based tinting: big stars lean blue, small lean warm
                        float3 tempShift = lerp(float3(1.0, 0.9, 0.7), float3(0.7, 0.8, 1.0), size);
                        float3 color = baseColor * tempShift;


                        //star *= sin((time*2) * 3.0 + n * 6.2831) * 0.5 + .7;
                        float twinkle = sin((time * 3.0 + n * 6.2831) * 2.0);
                        twinkle = pow(saturate(twinkle * 0.5 + 1.), 4.0); // sharper contrast
                        star *= twinkle;
                        
                        col += star * size * color;
                    }
                }
                return col;
            }

            float2 OctahedralWrap(float3 n)
            {
                n /= abs(n.x) + abs(n.y) + abs(n.z);
                float2 wrapped = n.xy;

                if (n.z < 0.0)
                    wrapped = (1.0 - abs(wrapped.yx)) * (wrapped.xy >= 0.0 ? 1.0 : -1.0);

                return wrapped;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.normal = normalize(v.normal);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.normal);
                float2 uv = OctahedralWrap(dir);
                uv = dir.xz * 4.;
                //float2 uv = dir.xy * 2.;

                float t = _Time * 4.;
                float2 rotated = mul(dir.xy, Rot(t));

                float3 col = float3(0, 0, 0);
                [unroll]

                float mask = SAMPLE_TEXTURE2D(_BandMask, sampler_BandMask, uv).r;

                // Add dusty edge using noise and mask
                float2 dustUV = uv * _DustTiling;
                float dust = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, dustUV).r;
                float edgeFade = smoothstep(0.0, 0.6, 1.0 - mask);
                //col += dust * edgeFade * _DustIntensity;

                //col *= mask;
                
                for (int j = 0; j < 8; j++)
                {
                    float i_f = j / 4.0;
                    float depth = frac(i_f );
                    float scale = lerp(20.0, 0.5, depth);
                    float fade = depth * smoothstep(1.0, 0.9, depth);
                    col += StarLayer(uv * scale + i_f * 453.2, t) * fade;
                }

                
                
                col = pow(col * _Intensity, float3(0.4545, 0.4545, 0.4545));

                // Compute vertical direction (normalized -1 to 1)
                float v = dir.y; // -1 = bottom, 0 = equator, 1 = top

                // Normalize to 0-1 for gradient lerp
                float heightT = saturate(v * 0.5 + 0.5);

                // Gradient color from bottom to top
                float3 poleGradient = lerp(_PoleColorBottom.rgb, _PoleColorTop.rgb, heightT);

                
                float fadeTop = smoothstep(1.0 - _PoleFadeSharpness, 1.0, v);
                float fadeBottom = smoothstep(-1.0, -1.0 + _PoleFadeSharpness, v);
                float edgeFader = fadeTop * fadeBottom;

                // Multiply final color
                col *= poleGradient;
                col *= edgeFader;
                return float4(col, 1.0);
            }

            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}

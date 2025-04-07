Shader "Custom/SandURP"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _SandColor ("Sand Color", Color) = (1,1,1,1)
        _SandTex ("Sand Texture", 2D) = "bump" {}
        _SandStrength ("Sand Strength", Range(0,1)) = 0.1

        _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
        _TerrainColor ("Terrain Color", Color) = (1,1,1,1)

        _TerrainRimPower ("Terrain Rim Power", Range(0,1)) = 0.1
        _TerrainRimStrength ("Terrain Rim Strength", Range(0,1)) = 0.1
        _TerrainRimColor ("Terrain Rim Color", Color) = (1,1,1,1)

        _OceanSpecularPower ("Ocean Specular Power", Range(0,1)) = 0.1
        _OceanSpecularStrength ("Ocean Specular Strength", Range(0,1)) = 0.1
        _OceanSpecularColor ("Ocean Specular Color", Color) = (1,1,1,1)

        _GlitterTex ("Glitter Texture", 2D) = "bump" {}
        _GlitterThreshold ("Glitter Threshold", Range(0,1)) = 0.1
        _GlitterColor ("Glitter Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Texture Samplers
            TEXTURE2D(_MainTex);         SAMPLER(sampler_MainTex);
            TEXTURE2D(_SandTex);         SAMPLER(sampler_SandTex);
            TEXTURE2D(_GlitterTex);      SAMPLER(sampler_GlitterTex);

            // Shader Properties
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Glossiness;
            float _Metallic;
            float4 _SandColor;
            float _SandStrength;
            float4 _ShadowColor;
            float4 _TerrainColor;
            float _TerrainRimPower;
            float _TerrainRimStrength;
            float4 _TerrainRimColor;
            float _OceanSpecularPower;
            float _OceanSpecularStrength;
            float4 _OceanSpecularColor;
            float _GlitterThreshold;
            float4 _GlitterColor;
            float4 _SandTex_ST;
            float4 _GlitterTex_ST;
            float4 _SandTex_TexelSize;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 positionCS : SV_POSITION;
            };
            float mipmapLevel (float2 textureCoordinate)
            {
                float2 dx = ddx(textureCoordinate);
                float2 dy = ddy(textureCoordinate);
                float1 deltaMaxSqr = max(dot(dx, dx), dot(dy, dy));
                return 0.5f * log2(deltaMaxSqr);
            }
            float4 tex2Dfold (sampler2D s,float4 uvParams)
            {
                float mipGrad = mipmapLevel(uvParams.xy * uvParams.zw);
                float mip = floor(mipGrad);
                float mipLerp = frac(mipGrad);
                float4 col1 = SAMPLE_TEXTURE2D_LOD(tex, sampler_tex, uvParams.xy / pow(2, mip), mip);
                float4 col2 = SAMPLE_TEXTURE2D_LOD(tex, sampler_tex, uvParams.xy / (pow(2, mip) * 2), mip);
                return lerp(col1, col2, mipLerp);
            }
            // Normalize Lerp Function
            float3 nlerp(float3 n1, float3 n2, float t)
            {
                return normalize(lerp(n1, n2, t));
            }

            // Sand Normal Mapping
            float3 SandNormal(float3 worldPos, float3 N)
            {
                float2 uv = worldPos.xz * _SandTex_ST.xy;
                float4 texParams = float4(uv, _SandTex_TexelSize.zw);
                float3 sandTexSample = tex2Dfold(_SandTex, texParams).rgb;
                //float3 sandTexSample = SAMPLE_TEXTURE2D(_SandTex, sampler_SandTex, uv).rgb;
                
                float3 S = normalize(sandTexSample * 2 - 1); // Ensure proper normalization

                float3 Ns = nlerp(N, S, _SandStrength);
                return Ns;
            }
            

            // Vertex Shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            // Diffuse Lighting Calculation (Matches Original)
            float3 DiffuseColor(float3 N, float3 L)
            {
                N.y *= 0.3; // Dampens y-component for flatter shading
                float NdotL = saturate(4 * dot(N, L)); // Adjust light intensity scaling
                
                float3 color = lerp(_ShadowColor.rgb, _TerrainColor.rgb, NdotL);
                return color;
            }

            // Rim Lighting Effect (Matches Original)
            float3 RimLighting(float3 N, float3 V)
            {
                float rim = 1.0 - saturate(dot(N, V));
                rim = saturate(pow(rim, _TerrainRimPower) * _TerrainRimStrength);
                rim = max(rim, 0);
                return rim * _TerrainRimColor.rgb;
            }

            // Ocean Specular Effect (Matches Original)
            float3 OceanSpecular(float3 N, float3 L, float3 V)
            {
                float3 H = normalize(V + L);
                float NdotH = max(0, dot(N, H));
                float specular = pow(NdotH, _OceanSpecularPower) * _OceanSpecularStrength;
                return specular * _OceanSpecularColor.rgb;
            }

            // Glitter Specular Effect (Matches Original)
            float3 GlitterSpecular(float3 worldPos, float3 N, float3 L, float3 V)
            {
                float2 uv = worldPos.xz;
                uv = uv * _GlitterTex_ST.xy + _GlitterTex_ST.zw;
                
                float3 G = normalize(SAMPLE_TEXTURE2D(_GlitterTex, sampler_GlitterTex, uv).rgb * 2 - 1);
                float3 R = reflect(L, G);
                float RdotV = max(0, dot(R, V));

                if (RdotV < _GlitterThreshold)
                    return 0;

                return (1 - RdotV) * _GlitterColor.rgb;
            }

            // Lighting Function (Recreating LightingDesert)
            float3 LightingDesert(float3 normal, float3 viewDir, float3 lightDir, float3 worldPos)
            {
                float3 diffuseColor = DiffuseColor(normal, lightDir);
                float3 rimColor = RimLighting(normal, viewDir);
                float3 oceanColor = OceanSpecular(normal, lightDir, viewDir);
                float3 glitterColor = GlitterSpecular(worldPos, normal, lightDir, viewDir);

                float3 specularColor = saturate(max(rimColor, oceanColor));
                float3 finalColor = diffuseColor + specularColor + glitterColor;

                return finalColor;
            }

            // Fragment Shader (Handles Normal Mapping + Lighting)
            half4 frag(Varyings input) : SV_Target
            {
                // Calculate the view direction manually (World space view direction)
                float3 cameraPos = _WorldSpaceCameraPos.xyz;  // Built-in variable for camera position in world space
                float3 viewDir = normalize(cameraPos - input.worldPos);  // Calculate view direction

                // Get world normal and modify it using SandNormal
                float3 worldNormal = normalize(input.normalWS);
                float3 modifiedNormal = SandNormal(input.worldPos, worldNormal);

                // Light direction
                //Light mainLight = GetMainLight();
                //float3 lightDir = normalize(mainLight.direction);
                float3 lightDir = normalize(_MainLightPosition.xyz);

                // Get the final lighting result
                float3 lightingResult = LightingDesert(modifiedNormal, viewDir, lightDir, input.worldPos);

                // Sample the albedo texture (MainTex)
                float3 albedoColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb;

                // Multiply the lighting result by the albedo color
                float3 finalColor = lightingResult * albedoColor;

                return half4(finalColor, 1.0);
            }


            ENDHLSL
        }
    }
}

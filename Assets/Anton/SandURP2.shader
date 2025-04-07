Shader "Custom/SandURP2"
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
        
        _ShallowTex ("Shallow Texture", 2D) = "bump" {}
        _SteepTex ("Steep Texture", 2D) = "bump" {}
        _SteepnessSharpnessPower("Steepness Sharpness Power", Range(0,1)) = 0.1
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
            TEXTURE2D(_ShallowTex);      SAMPLER(sampler_ShallowTex);
            TEXTURE2D(_SteepTex);        SAMPLER(sampler_SteepTex);

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
            float4 _ShallowTex_ST;
            float4 _SteepTex_ST;
            float _SteepnessSharpnessPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float4 positionCS : SV_POSITION;
            };

            // Normalize Lerp Function
            float3 nlerp(float3 n1, float3 n2, float t)
            {
                return normalize(lerp(n1, n2, t));
            }

            // Sand Normal Mapping
            float3 SandNormal(float3 worldPos, float3 N, float3 tangentWS, float3 bitangentWS)
            {
                float2 uv = worldPos.xz * _SandTex_ST.xy + _SandTex_ST.zw;
                float3 sandTexSample = UnpackNormal(SAMPLE_TEXTURE2D(_SandTex, sampler_SandTex, uv)).rgb;
                
                float3 S = normalize(sandTexSample * 2 - 1); // Ensure proper normalization
                float3 Ns = nlerp(N, S, _SandStrength);
                float3x3 TBN = float3x3(tangentWS, bitangentWS, N);
                float3 SandNormal = normalize(mul(Ns, TBN));
                return SandNormal;
            }

            float3 RipplesNormal(float3 worldPos, float3 N, float3 tangentWS, float3 bitangentWS, float steepness)
            {
                float2 uv = worldPos.xz;
                //uv = uv * _ShallowTex_ST.xy + _ShallowTex_ST.zw + _SteepTex_ST.xy + _SteepTex_ST.zw;
                float2 shallowuv = uv * _ShallowTex_ST.xy + _ShallowTex_ST.zw;
                float2 steepuv = uv * + _SteepTex_ST.xy + _SteepTex_ST.zw;

                // Sample normal maps using URP-compatible sampling
                float3 shallow = UnpackNormal(SAMPLE_TEXTURE2D(_ShallowTex, sampler_ShallowTex, shallowuv));
                float3 steep   = UnpackNormal(SAMPLE_TEXTURE2D(_SteepTex, sampler_SteepTex, steepuv));

                float3 blendTangent = nlerp(shallow, steep, steepness);
                float3x3 TBN = float3x3(tangentWS, bitangentWS, N);
                float3 worldNormal = normalize(mul(blendTangent, TBN));
                return worldNormal;

                /*float3 bob = shallow + steep;
                // Blend normals based on steepness
                //float3 S = nlerp(steep, shallow, steepness);
                float3 S = nlerp(N, bob, steepness);
                
                return normalize(S);*/
            }
            

            // Vertex Shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.tangentWS = TransformObjectToWorldDir(IN.tangentOS.xyz);
                OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * IN.tangentOS.w; // Correct bitangent
                OUT.uv = TRANSFORM_TEX(IN.uv, _SandTex);
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
            float3 GlitterSpecular(float3 worldPos, float3 N, float3 L, float3 V, float3 tangentWS, float3 bitangentWS)
            {
                float2 uv = worldPos.xz;
                uv = uv * _GlitterTex_ST.xy + _GlitterTex_ST.zw;
                
                float3 G = normalize(SAMPLE_TEXTURE2D(_GlitterTex, sampler_GlitterTex, uv).rgb * 2 - 1);
                float3 R = reflect(L, G);
                float RdotV = max(0, dot(R, V));
                
                if (RdotV < _GlitterThreshold)
                    return 0;
                float3x3 TBN = float3x3(tangentWS, bitangentWS, N);
                float3 R_Tangent = normalize(mul(TBN, R)); // Transform to tangent space
                float sparkleFactor = saturate(R_Tangent.z); // Z+ in tangent space is surface-facing
                
                return (1 - sparkleFactor) * _GlitterColor.rgb;
            }

            // Lighting Function (Recreating LightingDesert)
            float3 LightingDesert(float3 normal, float3 viewDir, float3 lightDir, float3 worldPos, float3 lightColor, float lightIntensity, float3 tangentWS, float3 bitangentWS)
            {
                float3 diffuseColor = DiffuseColor(normal, lightDir);
                float3 rimColor = RimLighting(normal, viewDir);
                float3 oceanColor = OceanSpecular(normal, lightDir, viewDir);
                float3 glitterColor = GlitterSpecular(worldPos, normal, lightDir, viewDir, tangentWS, bitangentWS);

                float3 specularColor = saturate(max(rimColor, oceanColor));
                float3 Color = (diffuseColor + specularColor + glitterColor) * lightColor * lightIntensity;

                return float4(Color * _SandColor, 1);
            }

            // Fragment Shader (Handles Normal Mapping + Lighting)
            half4 frag(Varyings input) : SV_Target
            {
                // Calculate the view direction manually (World space view direction)
                float3 cameraPos = _WorldSpaceCameraPos.xyz;  // Built-in variable for camera position in world space
                float3 viewDir = normalize(cameraPos - input.worldPos);  // Calculate view direction
                
                float3 N_WORLD = TransformObjectToWorldNormal(input.normalWS);
                float3 UP_WORLD = float3(0, 1, 0);
                
                float steepness = saturate(dot(N_WORLD, UP_WORLD));
                steepness = pow(steepness, _SteepnessSharpnessPower);
                //float3 shallow = UnpackNormal(SAMPLE_TEXTURE2D(_ShallowTex, sampler_ShallowTex, input.uv));
                //shallow *= _SteepnessSharpnessPower;
                //shallow = normalize(shallow);
                //return half4(shallow, 1);
                
                
                // Get world normal and modify it using SandNormal
                //float3 worldNormal = normalize(input.normalWS);
                //float3 worldNormal = float3(0, 0, 1);
                //float3 sandRipples = RipplesNormal(input.worldPos, steepness);
                //float3 modifiedNormal = SandNormal(input.worldPos, worldNormal);

                //float3 N = float3(0, 0, 1);
                float3 N = normalize(input.normalWS);
                
                //N = RipplesNormal(input.worldPos, N, steepness);
                N = RipplesNormal(input.worldPos, N, input.tangentWS, input.bitangentWS, steepness);
                N = SandNormal(input.worldPos, N, input.tangentWS, input.bitangentWS);

                N = normalize(N);
                

                // Light direction
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float shadowAttenuation = mainLight.shadowAttenuation;
                //float3 lightDir = normalize(_MainLightPosition.xyz);

                // Get the final lighting result
                float3 lightingResult = shadowAttenuation * LightingDesert(N, viewDir, lightDir, input.worldPos, mainLight.color, mainLight.distanceAttenuation, input.tangentWS, input.bitangentWS);

                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint i = 0; i < additionalLightsCount; ++i)
                {
                    Light light = GetAdditionalLight(i, input.worldPos);
                    float3 additionalLightDir = normalize(light.direction);
                    float attenuation = light.distanceAttenuation * light.shadowAttenuation;
                    lightingResult += attenuation * LightingDesert(N, viewDir, additionalLightDir, input.worldPos, light.color, light.distanceAttenuation, input.tangentWS, input.bitangentWS);
                }
                
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
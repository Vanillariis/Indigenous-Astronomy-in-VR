Shader "Custom/Sand"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _SandColor ("Sand Color", Color) = (1,1,1,1)
        _TerrainColor ("Terrain Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
        _SandTex ("Sand Texture", 2D) = "bump" {}
        _SandStrength ("Sand Strength", Range(0, 1)) = 0.1
        _TerrainRimPower ("Terrain Rim power", Range(0, 1)) = 0.1
        _TerrainRimStrength ("Terrain Rim Strength", Range(0, 1)) = 0.1
        _TerrainRimColor ("Terrain Rim Color", Color) = (1,1,1,1)
        _OceanSpecularPower ("Ocean Specular Power", Range(0, 1)) = 0.1
        _OceanSpecularStrength ("Ocean Specular Strength", Range(0, 1)) = 0.1
        _OceanSpecularColor ("Ocean Specular Color", Color) = (1,1,1,1)
        _GlitterThreshold ("Glitter Threshold", Range(0, 1)) = 0.1
        _GlitterColor ("Glitter Color", Color) = (1,1,1,1)
        _GlitterTex ("Glitter Texture", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #include "UnityPBSLighting.cginc"
        
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Desert fullforwardshadows
        
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_SandTex;
            float3 worldPos;
        };
        
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        
        //Surface variables
        float3 _SandColor;
        sampler2D_float _SandTex;
        float _SandStrength;
        
        //Lighting variables
        float3 _TerrainColor;
        float3 _ShadowColor;
        
        float _TerrainRimPower;
        float _TerrainRimStrength;
        float3 _TerrainRimColor;
        
        float _OceanSpecularPower;
        float _OceanSpecularStrength;
        float3 _OceanSpecularColor;

        sampler2D_float _GlitterTex;
        float _GlitterThreshold;
        float3 _GlitterColor;
        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float3 nlerp (float3 n1, float3 n2, float t)
        {
            return normalize(lerp(n1, n2, t));
        }
        
        float3 SandNormal (float2 uv, float3 N)
        {
            float3 random = tex2D(_SandTex, uv).rgb;

            float3 S = normalize(random * 2 - 1);

            float3 Ns = nlerp(N, S, _SandStrength);
            return Ns;
        }
        
        void surf (Input IN, inout SurfaceOutputStandard  o)
        {
            o.Albedo = _SandColor;
            o.Alpha = 1;

            float3 N = float3(0, 0, 1);
            //N = RipplesNormal(N);
            N = SandNormal (IN.uv_SandTex.xy, N);

            o.Normal = N;

            /*
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            */
        }
        float3 DiffuseColor (float3 N, float3 L)
        {
            N.y *= 0.3;
            float NdotL = saturate(4 * dot(N, L));

            float3 color = lerp(_ShadowColor, _TerrainColor, NdotL);
            return color;
        }
        
        float3 RimLighting (float3 N, float3 V)
        {
            float rim = 1.0 - saturate(dot(N, V));
            rim = saturate(pow(rim, _TerrainRimPower) * _TerrainRimStrength);
            rim = max(rim, 0);
            return rim * _TerrainRimColor;
        }

        float3 OceanSpecular (float3 N, float3 L, float3 V)
        {
            float3 H = normalize(V + L);
            float NdotH = max(0, dot(N, H));
            float specular = pow(NdotH, _OceanSpecularPower) * _OceanSpecularStrength;
            return specular * _OceanSpecularColor;
        }

        float3 GlitterSpecular (float2 uv, float3 N, float3 L, float3 V)
        {
            float3 G = normalize(tex2D(_GlitterTex, uv).rgb * 2 - 1);

            float3 R = reflect(L, G);
            float RdotV = max(0, dot(R, V));

            if (RdotV > _GlitterThreshold)
                return 0;

            return (1 - RdotV) * _GlitterColor;
        }
        
        float4 LightingDesert(SurfaceOutputStandard s, fixed3 viewDir, UnityGI gi)
        {
            float3 L = gi.light.dir;
            float3 N = s.Normal;
            float3 V = viewDir;
            
            float3 diffuseColor = DiffuseColor (N, L);
            float3 rimColor = RimLighting (N, V);
            float3 oceanColor = OceanSpecular (N, L, V);
            float3 glitterColor = GlitterSpecular (IN.uv_SandTex, N, L, V);

            float3 specularColor = saturate(max(rimColor, oceanColor));
            float3 color = diffuseColor + specularColor + glitterColor;
            
            return float4(color * s.Albedo, 1);
        }
        
        void LightingDesert_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
        {
            LightingStandard_GI(s, data, gi);  
        }
        
        ENDCG
    }
    FallBack "Diffuse"
}

Shader "Unlit/StarBurstBillboard"
{
    Properties
    {
        _BaseSize ("Base Size", Float) = 0.2
        _MainColor ("Star Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Cull Off
        Blend One One   // additive blending for glow

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct StarData
            {
                float3 position;
                float3 velocity;
                float life;
                float startTime;
                float3 color;
            };
            
            StructuredBuffer<StarData> _StarBuffer;

            float3 _Origin;
            float _TimeNow;
            float _BaseSize;
            float4 _MainColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;

                StarData star = _StarBuffer[v.instanceID];

                float t = max(0, _TimeNow - star.startTime);
                //float lifeRatio = saturate(t / star.life);
                float speedFactor = exp(t/10) - 1.0; // ~0 to 1.0+
                //float3 pos = star.position + star.velocity * t * speedFactor;
                //float3 pos = star.position + star.velocity * t * speedFactor;
                float3 worldPos = _Origin + star.velocity * t * speedFactor;

                float shrinkFactor = pow(1.0 / (1.0 + t), 1.3); // Try values 1.5 - 4.0
                float size = _BaseSize * shrinkFactor;


                // Get world-space camera right and up vectors from _CameraToWorld matrix
                float3 right = UNITY_MATRIX_IT_MV[0].xyz;
                float3 up = UNITY_MATRIX_IT_MV[1].xyz;

                float3 offset = (right * v.vertex.x + up * v.vertex.y) * size;

                float3 finalWorldPos = worldPos + offset;

                o.pos = UnityWorldToClipPos(float4(finalWorldPos, 1.0));
                o.uv = v.uv;
                o.color = star.color;

                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;
                float dist = dot(uv, uv);

                float alpha = saturate(1.0 - dist * 4.0); // soft circular falloff
                return float4(i.color, 1.0) * alpha;
            }
            ENDHLSL
        }
    }
}

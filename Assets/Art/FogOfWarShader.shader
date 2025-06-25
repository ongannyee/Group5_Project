Shader "Unlit/FogOfWarShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaThreshold ("Alpha Threshold", Range(0, 0.1)) = 0.01
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Lighting Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _AlphaThreshold;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float alpha = tex2D(_MainTex, i.uv).a;
                // Apply threshold to eliminate tiny black dots
                alpha = saturate((alpha - _AlphaThreshold) / (1 - _AlphaThreshold));
                return float4(0, 0, 0, alpha);
            }
            ENDCG
        }
    }
}
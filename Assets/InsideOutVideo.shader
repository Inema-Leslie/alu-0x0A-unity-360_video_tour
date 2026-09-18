Shader "Custom/InsideOutVideo"
{
    Properties
    {
        _MainTex ("Base (RGB) 360 Video", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1, 1, 1, 1)
        _Exposure ("Brightness / Exposure", Range(0.2, 3.0)) = 1.0
        _Contrast ("Contrast", Range(0.5, 2.0)) = 1.05
        _Saturation ("Saturation", Range(0.0, 2.0)) = 1.1
        _Gamma ("Gamma Correction", Range(0.4, 2.2)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        LOD 100
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Exposure;
            float _Contrast;
            float _Saturation;
            float _Gamma;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(float2(1.0 - v.uv.x, v.uv.y), _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.rgb *= _Exposure;
                col.rgb = (col.rgb - 0.5) * _Contrast + 0.5;
                float luminance = dot(col.rgb, half3(0.2126, 0.7152, 0.0722));
                col.rgb = lerp(half3(luminance, luminance, luminance), col.rgb, _Saturation);
                col.rgb = max(half3(0.0, 0.0, 0.0), col.rgb);
                if (abs(_Gamma - 1.0) > 0.01)
                {
                    col.rgb = pow(col.rgb, half3(_Gamma, _Gamma, _Gamma));
                }
                return col;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Texture"
}

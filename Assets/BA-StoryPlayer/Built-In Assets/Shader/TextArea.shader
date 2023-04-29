Shader "Hidden/TextArea"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_TintColor("TintColor",COLOR)=(1,1,1,1)
        _Strength("Strength",Range(1,3)) = 1
        _AlphaScale("AlphaScale",Range(0,1)) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Lighting Off
        Blend One OneMinusSrcAlpha

        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _TintColor;
            float _Strength;
            float _AlphaScale;

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv) * _Strength;
                col = clamp(col,float4(0,0,0,0),float4(1,1,1,1));
                float4 result = _TintColor;
                result.a = col.rgb * _AlphaScale;
                result *= col;
                return result;
            }
            ENDCG
        }
    }
}

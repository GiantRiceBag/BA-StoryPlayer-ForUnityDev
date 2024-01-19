Shader "Hidden/PatternButton"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        _PatternTex("Pattern Texure",2D) = "white" {}
        _PatternAlphaReinforcement("Pattern Alpha Reinforcement",Range(0,10)) = 1
        _PatternTint("Pattern Tint",Color) = (1,1,1,1)
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
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
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color :TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _PatternTex;
            float4 _PatternTex_ST;

            fixed4 _PatternTint;
            float _PatternAlphaReinforcement;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 patternUV = TRANSFORM_TEX(i.uv,_PatternTex);
                fixed4 patternCol = tex2D(_PatternTex,patternUV);

                fixed4 finalCol = col * i.color + col.a * patternCol.a * _PatternAlphaReinforcement * _PatternTint;
                return finalCol;
            }
            ENDCG
        }
    }
}

Shader "Unlit/TextArea"
{
    Properties
    {
       [HideInInspector]_MainTex("MainTex",2D) = "white"{}
       _Color("Color",COLOR) = (1,1,1,1)
       _Power("Power",Range(0,1)) = 0.879
       _FillAmount("Fill Amount",Range(0,2)) = 1.07
       _EdgeSmoothness("Edge Smoothness",Range(0,10)) = 1.28
       _MaxTransparency("Max Transparency",Range(0,1)) = 0.909
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

            fixed4 _Color;
            float _FillAmount;
            float _Power;
            float _EdgeSmoothness;
            float _MaxTransparency;

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

            fixed4 frag (v2f i) : SV_Target
            {
                float uv_y = 1-i.uv.y;
                fixed4 col = fixed4(_Color.xyz,pow(uv_y,_Power));
                col.w += smoothstep(1-_FillAmount,1-_FillAmount+_EdgeSmoothness,uv_y);
                col.w = clamp(col.w,0,_MaxTransparency);

                return col;
            }

            ENDCG
        }
    }
}

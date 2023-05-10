Shader "Hidden/ImageBlur"
{
  	 Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 10)) = 1
        _Weight("Weight",Range(0,1)) = 0
    }
 
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float _BlurAmount;
            float4 _MainTex_TexelSize;
            float _Weight;
 
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;
                float mix = lerp(0,_BlurAmount,_Weight);
                for(int x = -10; x < 10; x++)
                {
                    for(int y = -10; y < 10; y++)
                    {
                        col += tex2D(_MainTex, i.uv + float2(x, y) * _MainTex_TexelSize.xy * mix);
                    }
                }
                col /= 400;
                return col;
            }
            ENDCG
        }
    }
}

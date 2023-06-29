Shader "Hidden/FlowLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Angle("Angle",float) = 30
        _Speed("Speed",float) = 0.2
        [HDR]_Tint("Tint",Color) = (1,1,1,1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Tags{
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define PI 3.1415926535

            float2 Rotate(float2 uv,float rotation,bool clockwise = true)
            {
	            float sine,cosine;
	            float angle = rotation * 2 * PI;
	            sincos(angle,sine,cosine);

	            return lerp(float2(cosine*uv.x + sine*uv.y,-sine*uv.x + cosine*uv.y),float2(cosine*uv.x - sine*uv.y,sine*uv.x + cosine*uv.y),clockwise);
            }

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
            float _Angle;
            float _Speed;
            fixed4 _Tint;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float2 uv = i.uv - 0.5;
                uv = Rotate(uv,_Time.y * _Speed);
                float delta = (atan2(uv.y,uv.x)+PI)/PI * 180;
                float cullingResult = smoothstep(0,_Angle*0.5,delta) - smoothstep(_Angle*0.5,_Angle,delta);
                cullingResult = lerp(smoothstep(180,_Angle*0.5+180,delta) - smoothstep(_Angle*0.5+180,_Angle+180,delta),cullingResult,cullingResult);

                fixed4 finalColor = col * i.color * _Tint;
                finalColor.a *= cullingResult;
                return finalColor;
            }
            ENDCG
        }
    }
}

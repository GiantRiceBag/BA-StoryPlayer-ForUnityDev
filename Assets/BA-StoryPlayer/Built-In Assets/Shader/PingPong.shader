Shader "Hidden/PingPong"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [Toggle(_TRIANGLE_WAVE)] _TriangleWave("Triangle Wave",int) = 0
        _Magnitude("Magnitude",float) = 1
        _Direction("Direction",vector) = (0,1,0,0)
        _Period("Period",float) = 1
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

            #pragma shader_feature _TRIANGLE_WAVE

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

            sampler2D _MainTex;
            float _Period;
            float _Magnitude;
            float4 _Direction;

            v2f vert (appdata v)
            {
                v2f o;
                float4 objectPos = v.vertex;
                #if _TRIANGLE_WAVE
                float sig = sign(sin(_Time.y * (2 * UNITY_PI)/_Period));
                objectPos -= _Direction * 2 * (frac(_Time.y / _Period)-0.5) * sig * _Magnitude;
                #else
                objectPos += abs(sin(_Time.y * (2 * UNITY_PI)/_Period/2)) * _Magnitude * _Direction;
                #endif
                o.vertex = UnityObjectToClipPos(objectPos);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                return col;
            }
            ENDCG
        }
    }
}

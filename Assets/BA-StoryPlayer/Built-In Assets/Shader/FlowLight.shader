Shader "Hidden/FlowLight"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        _PatternTex("Pattern Texure",2D) = "white" {}
        _PatternAlphaReinforcement("Pattern Alpha Reinforcement",Range(0,10)) = 1
        _PatternTint("Pattern Tint",Color) = (1,1,1,1)
        [Space]
        [Toggle]_EnableFlowingLight("Enable Flowing Light",int) = 1
        [Toggle]_FlowingLightOnly("Flowing Light Only",int) = 0
        _FlowingLightLength("Flowing Light Length",Range(0,360)) = 180
        _FlowingLightSpeed("Flowing Light Speed",float) = 0.3
        _FlowingLightEdgeSize("Flowing Light Edge Size",Range(0,10)) = 3.92
        _FlowingLightScale("Flowing Light Scale",Range(0,10)) = 1
        [HDR]_FlowingLightTint("Flowing Light Tint",Color) = (2,1.482,0.6509)
        _FlowingLightReinforcement("Flowing Lght Reinforcement",Range(1,100)) = 10.7
        _EdgeSmoothness("Edge Smoothness",range(0,1)) = 0.5
        _AlphaThreshold("Alpha Threshold",Range(0,1)) = 0.02
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        Pass // Normal Pass
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
            int _FlowingLightOnly;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 patternUV = TRANSFORM_TEX(i.uv,_PatternTex);
                fixed4 patternCol = tex2D(_PatternTex,patternUV);

                fixed4 finalCol = col * i.color + col.a * patternCol.a * _PatternAlphaReinforcement * _PatternTint;
                finalCol.a *= 1 - _FlowingLightOnly;
                return finalCol;
            }
            ENDCG
        }
        Pass // Outline Pass
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

            sampler2D _MainTex;
            float2 _MainTex_TexelSize;

            int _EnableFlowingLight;
            int _FlowingLightOnly;

            float _FlowingLightLength;
            float _FlowingLightSpeed;
            float _FlowingLightEdgeSize;
            float _FlowingLightScale;
            float _AlphaThreshold;
            float _FlowingLightReinforcement;
            float _EdgeSmoothness;
            fixed4 _FlowingLightTint;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                fixed4 edgeCol = tex2D(_MainTex,i.uv + _MainTex_TexelSize*mul(float2(0,1),_FlowingLightEdgeSize));
                edgeCol += tex2D(_MainTex,i.uv + _MainTex_TexelSize*mul(float2(0,-1),_FlowingLightEdgeSize));
                edgeCol += tex2D(_MainTex,i.uv + _MainTex_TexelSize*mul(float2(1,0),_FlowingLightEdgeSize));
                edgeCol += tex2D(_MainTex,i.uv + _MainTex_TexelSize*mul(float2(-1,0),_FlowingLightEdgeSize));
                edgeCol += tex2D(_MainTex,i.uv + _MainTex_TexelSize*mul(float2(1,1),_FlowingLightEdgeSize));
                edgeCol += tex2D(_MainTex,i.uv + _MainTex_TexelSize*mul(float2(1,-1),_FlowingLightEdgeSize));
                edgeCol += tex2D(_MainTex,i.uv + _MainTex_TexelSize*mul(float2(-1,1),_FlowingLightEdgeSize));
                edgeCol += tex2D(_MainTex,i.uv + _MainTex_TexelSize*mul(float2(-1,-1),_FlowingLightEdgeSize));

                float edgeAlpha = clamp(edgeCol.a/8 * _FlowingLightReinforcement,0,1);

                edgeCol =  smoothstep(_AlphaThreshold,_AlphaThreshold+_EdgeSmoothness,(edgeAlpha+0.05 - col.a)) * _FlowingLightTint;
                edgeCol *= _EnableFlowingLight;

                float2 uv = i.uv - 0.5;
                uv = Rotate(uv,_Time.y * _FlowingLightSpeed);
                float delta = (atan2(uv.y,uv.x)+PI)/PI * 180;

                float halfLength = _FlowingLightLength / 2;
                float cullingResult = smoothstep(0,halfLength,delta) - smoothstep(halfLength,_FlowingLightLength,delta);
                cullingResult = lerp(smoothstep(180,halfLength+180,delta) - smoothstep(halfLength+180,_FlowingLightLength+180,delta),cullingResult,cullingResult);

                return fixed4(_FlowingLightTint.xyz,cullingResult*edgeCol.a);
            }
            ENDCG
        }
    }
}

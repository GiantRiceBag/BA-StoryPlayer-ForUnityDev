Shader "Spine/HologramEffect"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[Toggle(_CANVAS_GROUP_COMPATIBLE)] _CanvasGroupCompatible("CanvasGroup Compatible", Int) = 0
		_Color ("Tint", Color) = (1,1,1,1)
		
		[Space]
		_HologramSFXSpeed ("Hologram SFX Speed",float) = 8.62
		_HologramSFXDensity("Hologram SFX Density",Range(0,0.12)) = 0.1151
		_HologramSFXColor("Hologram SFX Color",COLOR) = (0.5896226,0.8559483,1,1)
		_HologramSFXBrightness("Hologram SFX Brightness",Range(0,2)) = 0.8

		_HologramJitterThreshold("Hologram Jitter Threshold",vector) = (0.99,0.99,0,0)
		_HologramJitterSpeedRadio("Hologram Jitter Speed Radio",vector) = (70,70,0,0)
		_HologramJitterRangeVertex("Hologram Jitter Range Vertex",vector) = (20,1500,0,1)
		_HologramJitterOffset("Hologram Jitter Offset",vector) = (2,1,0,0)

		[Space]
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector][Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255

		[HideInInspector] _ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Normal"

		CGPROGRAM
			#pragma shader_feature _ _CANVAS_GROUP_COMPATIBLE
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "Assets/BA-StoryPlayer/Built-In Assets/Shader/Library/HologramSFX.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			struct VertexInput {
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput {
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			VertexOutput vert (VertexInput IN) {
				VertexOutput OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.worldPosition = IN.vertex + float4(HologramSFXJitterOffsetVertex(IN.vertex,fixed2(1,0)),0,0);
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = IN.texcoord;

				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1);
				#endif

				OUT.color = IN.color * float4(_Color.rgb * _Color.a, _Color.a); // Combine a PMA version of _Color with vertexColor.
				return OUT;
			}

			fixed4 frag (VertexOutput IN) : SV_Target
			{
				half4 texColor = tex2D(_MainTex,IN.texcoord + HologramSFXJitterOffseUV(IN.texcoord,_MainTex_TexelSize,float2(1,0)));

				texColor.rgb *= texColor.a;

				half4 color = (texColor + _TextureSampleAdd) * IN.color;
				#ifdef _CANVAS_GROUP_COMPATIBLE
				// CanvasGroup alpha sets vertex color alpha, but does not premultiply it to rgb components.
				color.rgb *= IN.color.a;
				#endif

				color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				HologramSFXInput sfxData;
				sfxData.color = color;
				sfxData.uv = IN.vertex;

				return HologramSFX(sfxData);
			}
		ENDCG
		}
	}
	CustomEditor "SpineShaderWithOutlineGUI"
}

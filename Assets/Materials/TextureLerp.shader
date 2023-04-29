Shader "Unlit/TextureLerp"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Blend("Texture Blend", Range(0,1)) = 0.0
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_MainTex2("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.2
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	struct v2f
	{
		float4 position : SV_POSITION;
		float2 texcoord : TEXCOORD0;
	};

	sampler2D _MainTex;
	sampler2D _MainTex2;
	float4 _MainTex_ST;
	half _Blend;
	float4 _Color;
	float _Cutoff;

	v2f vert(appdata_base v)
	{
		v2f o;
		o.position = UnityObjectToClipPos(v.vertex);
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		return o;
	}

	float4 frag(v2f i) : COLOR
	{
		float4 c = lerp(tex2D(_MainTex, i.texcoord), tex2D(_MainTex2, i.texcoord), _Blend);
		clip(c.a - _Cutoff);
		return c * _Color;
	}

	ENDCG

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
	FallBack "Diffuse"
}
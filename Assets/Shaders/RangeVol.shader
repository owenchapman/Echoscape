Shader "Custom/RangeVol" {
Properties {
	_mPos  ("Mouse Position", COLOR) = (0,0,0,0)
	_OriginalTex  ("Original Texture", 2D) = "" {}


}

SubShader {

	Pass {
	Blend SrcAlpha One

	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"

	float4 _mPos;
	sampler2D _OriginalTex;
	float4 _OriginalTex_ST;

	struct v2f
	{
		float4 pos : POSITION;
		float2 uv  : TEXCOORD0;
	};

	v2f vert (appdata_base v)
	{
		v2f o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _OriginalTex);
		return o;
	}

	float4 frag (v2f i) : COLOR
	{
		float4 original;
		original = tex2D(_OriginalTex, i.uv);

		if(_mPos.w > 0){
			if(abs(i.uv.x - _mPos.x) < 0.001 || abs(i.uv.y - _mPos.y) < 0.001){
				original = float4(1,1,1,0.8);
			}
		}

		return original;
	}
	ENDCG

	}
	}

Fallback off

}
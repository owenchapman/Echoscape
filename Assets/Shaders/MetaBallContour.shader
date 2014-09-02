Shader "Custom/MetaBallContour" {
Properties {
	_OriginalTex  ("Original Texture", 2D) = "" {}
}

SubShader {

	Pass {
	ZWrite Off
	Blend SrcAlpha One
	

	CGPROGRAM
	// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members projPos)
	#pragma exclude_renderers d3d11 xbox360
	// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members projPos)
	#pragma exclude_renderers xbox360
	#pragma vertex vert
	#pragma fragment frag
	#pragma fragmentoption ARB_precision_hint_fastest
	#include "UnityCG.cginc"


	struct v2f
	{
		float4 pos : POSITION;
		float2 uv  : TEXCOORD0;
		float4 projPos : COLOR0;
	};

	sampler2D _OriginalTex;
	float4 _OriginalTex_ST;

	v2f vert (appdata_base v)
	{
		v2f o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _OriginalTex);
		o.projPos = ComputeScreenPos(o.pos);
		return o;
	}

	float4 frag (v2f i) : COLOR0
	{
		i.projPos /= i.projPos.w;

		float4 original = float4(1, 1, 1, 1);

		float4 col = tex2D(_OriginalTex, i.uv);

		float4 inColour = float4(i.projPos.x, i.projPos.y, 0.3, 0.9);
		//float4 inColour = float4(i.uv.x, i.uv.y, 0.3, 0.5);

		float strokeThreshold = 0.1;
		float strokeThickness = 0.05;
		float middle = strokeThreshold + strokeThickness;
		float end = middle + strokeThickness;

		if(col.z > strokeThreshold && col.z <= middle) {
			original.a = smoothstep(strokeThreshold, middle, col.z);
		}
		else if (col.z > middle && col.z <= end) {
			original = lerp(float4(1, 1, 1, smoothstep(end, middle, col.z)), inColour, smoothstep(middle, end, col.z));
		}
		else if (col.z > end) {
			original = inColour;
		}
		else {
			original = float4(0, 0, 0, 0);
		}

		return original;
	}
	ENDCG

		}
	}

Fallback off

}
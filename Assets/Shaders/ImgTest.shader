Shader "Custom/Basic Unlit" {
Properties {
	_PosTex  ("Position Texture", 2D) = "" {}
	_OriginalTex  ("Original Texture", 2D) = "" {}


}

SubShader {

	Pass {

	CGPROGRAM
	#pragma target 3.0
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"


	sampler2D _PosTex;
	sampler2D _OriginalTex;
	uniform float4x4 _viewMat;

	struct v2f
	{
		float4 pos : POSITION;
		float2 uv  : TEXCOORD0;
	};

	v2f vert (appdata_base v)
	{
		v2f o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord;
		return o;
	}

	float4 frag (v2f i) : COLOR
	{
		float4 original;
		original = float4(0, 0, 0, 0);

		int j;
		int k;
		int tSize = 50;
		float offset = 1/(2*tSize);


		for(j = 0; j < tSize; j++){

			//sample the position texture
			float2 uv;
			uv = float2( ((float) j/(tSize-1) + offset), 0.5);

			float4 col;
			col = tex2D(_PosTex, uv);

			if (dot(col,col) > 0){
				//Only use screen space coordsa
				float2 pos;
				pos = float2(col.x, col.y);

				float d = dot(i.uv - pos, i.uv - pos);

				d = 1/(10000*d);

				original += float4(d, d, d, d);
			}

			//if(abs(i.uv.x - pos.x) < 0.01 && abs(i.uv.y - pos.y) < 0.01 )
				//original = float4(1,0,0,1);
		}

		if(original.x < 0.5)
			original = float4(0, 0, 0, 0);

		original += tex2D(_OriginalTex, i.uv);

		return original;
	}
	ENDCG

		}
	}

Fallback off

}
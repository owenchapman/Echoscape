Shader "Custom/SlippyMap" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent" "RenderType"="Transparent"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	float a = 1f - pow(c.r, 2f);

	o.Albedo = _Color.rgb;
	o.Alpha = a * _Color.a;
	o.Emission = 1f - 0.8f *_Color.r;

}
ENDCG
}

Fallback "Transparent/VertexLit"
}

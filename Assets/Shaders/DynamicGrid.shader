Shader "Custom/DynamicGrid" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SubDivision ("Sub Divison", Range(1,10)) = 3
		_LineWidth ("Line Width", Range(0,0.01)) = 0.005
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent"}

		CGPROGRAM
    	#pragma surface surf Lambert
    	#include "UnityCG.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		float _SubDivision;
		float _LineWidth;

		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D (_MainTex, IN.uv_MainTex);

			o.Albedo = 0.3;

			float u = IN.uv_MainTex.x;
			float v = IN.uv_MainTex.y;
			float subD = int(_SubDivision);
			subD = 1/(pow(2,subD));
			float valX = fmod(u, subD/2);
			float valY = fmod(v, subD);

			float lwidth = _LineWidth/2;

			if(valX >= lwidth && valX <= subD - lwidth && valY >= lwidth/2 && valY <= subD - lwidth/2)
			{
				clip(-1);
			}

		}
		ENDCG
	}
	FallBack "Diffuse"
}

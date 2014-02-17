Shader "Hex Grid" {
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainColor ("Main Color", COLOR) = (1,1,1,1)
		_RimColor ("Rim Color", Color) = (1,1,1,1)
      	_RimPower ("Rim Power", Range(0.5,8.0)) = 2.0

	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 200
		Cull Off

		CGPROGRAM
		#pragma surface surf Lambert alpha
		#include "UnityCG.cginc"

		struct Input
		{
			float4 colour;
			float2 uv_MainTex;
			float3 viewDir;
			float3 worldPos;

		};

		sampler2D _MainTex;
		float4 _MainColor;
		float4 _RimColor;
      	float _RimPower;

		void surf (Input IN, inout SurfaceOutput o)
		{

			float4 col = tex2D(_MainTex, IN.uv_MainTex) * _MainColor;
			float t = (1 + sin(10*_Time))/2;
			half rim = saturate(dot (normalize(IN.viewDir), o.Normal));
			o.Emission = _RimColor.rgb * pow (rim, _RimPower);

			//float r = _Time - frac(_Time);
			//r *= 1000;

			//clip (frac((IN.worldPos.y*0.05) * 5) - 0.5);


          	float2 pos;
          	pos.x = IN.worldPos.x;
          	pos.y = IN.worldPos.z;

          	//if(dot(pos, pos) < r){
          		//o.Emission = _RimColor;
          	//}


			o.Albedo = col.rgb;
			o.Alpha = col.a;

		}

		ENDCG
	}

	FallBack "Diffuse"
}

Shader "Custom/DataTiles" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainColor ("Main Color", COLOR) = (1,1,1,1)
		_Scale ("Scale", float) = 100
		_OrgX ("OriginX", float) = 0
		_OrgY ("OriginY", float) = 0
	}
	SubShader {
		Tags { "Queue"="Transparent"}

		CGPROGRAM
    	#pragma surface surf Lambert alpha
    	#pragma target 3.0
    	#include "UnityCG.cginc"

		sampler2D _MainTex;

		float _Scale;
		float _OrgX;
		float _OrgY;

		float4 _MainColor;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;

		};


		void surf (Input IN, inout SurfaceOutput o) {


			//Lon Lat tile center point of tiles with world data
			float _Lon = -73.5;
			float _Lat = 45.5;

			//necessary transformations for world to lat lon
			float x = (IN.worldPos.x - _OrgX) / _Scale;
			float y = (IN.worldPos.z - _OrgY) /  _Scale;
			float lon = degrees(x);
			float lat = 2*atan(exp(y)) - (0.5 * 3.141592);
			lat = degrees(lat);

			//set scale dependent quandrant
			float square = 1;

			float4 col = tex2D(_MainTex, IN.uv_MainTex);

			if(abs(lon - _Lon) < square/2 && abs(lat - _Lat) < square/2)
			{
				o.Alpha = 0.6;
				o.Albedo = col.rgb * _MainColor.rgb;
			}

			else
				clip(-1);

			//debug
			//o.Albedo = float3((lon + radians(180f))/ radians(360f) , (lat + radians(90f))/ radians(180f) , 0);

		}
		ENDCG
	}
	FallBack "Diffuse"
}

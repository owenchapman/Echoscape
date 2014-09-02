Shader "Custom/WorldLonLat" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainColor ("Main Color", COLOR) = (1,1,1,1)
		_Scale ("Scale", float) = 100
		_Lon ("Longitude", Range(-180,180)) = 0
		_Lat ("Latitude", Range(-90,90)) = 0
		_OrgX ("OriginX", float) = 0
		_OrgY ("OriginY", float) = 0
		_Grid ("Grid Size", float) = 100
	}
	SubShader {
		Tags { "Queue"="Transparent"}

		CGPROGRAM
    	#pragma surface surf Lambert alpha
    	#pragma target 3.0
    	#include "UnityCG.cginc"

		sampler2D _MainTex;
		float _Lon;
		float _Lat;
		float _Scale;
		float _OrgX;
		float _OrgY;
		float _Grid;
		float4 _MainColor;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;

		};


		void surf (Input IN, inout SurfaceOutput o) {


			//necessary transformations for world to lat lon
			float x = (IN.worldPos.x - _OrgX) / _Scale;
			float y = (IN.worldPos.z - _OrgY) /  _Scale;
			float lon = degrees(x);
			float lat = 2*atan(exp(y)) - (0.5 * 3.141592);
			lat = degrees(lat);

			//set scale dependent quandrant
			float square = _Grid/_Scale;

			if(abs(lon - _Lon) < square/2 && abs(lat - _Lat) < square/2)
			{
				o.Alpha = _MainColor.a;
				o.Albedo = float4(1,1,1,0.2);
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

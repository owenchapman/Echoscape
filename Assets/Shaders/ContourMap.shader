Shader "Custom/ContourMap" {
	Properties {
      _MainTex ("Texture", 2D) = "white" {}

    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float2 uv_MainTex;
          float2 uv_BumpMap;
          float3 worldPos;
      };
      
      sampler2D _MainTex;
      sampler2D _BumpMap;
      
      void surf (Input IN, inout SurfaceOutput o) {
          
          float d = 0.3;
          float val = fmod(IN.worldPos.y, d)-(0.95*d);
          
          //clip (fmod(IN.worldPos.y, d)-(0.85*d));
          
          if(val < 0)
          {
          	o.Albedo = 0.3f;
          }
          
          if(val >= 0)
          {
          	//clip(-1);
          	o.Albedo = 0.4f;
          	o.Emission = 0.2f;
          }

          //o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
          
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }

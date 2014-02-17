Shader "Custom/LongLatSelector"
{
  Properties
  {
    _MainTex("Texture", 2D) = "white" {}
    _UV("UV Coords", Vector) = (0, 0, 0, 0)

  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" }
    Cull Off


    CGPROGRAM
    #pragma surface surf Lambert
    #include "UnityCG.cginc"

    struct Input
    {
        float2 uv_MainTex;
        float3 worldPos;
    };

    sampler2D _MainTex;
    float3 _UV;


    void surf (Input IN, inout SurfaceOutput o)
    {

		float3 sPos2 = float3(atan(IN.worldPos.z/IN.worldPos.x), acos(IN.worldPos.y), 1);

		if (abs(_UV.y - sPos2.y) < 0.00025 || abs(_UV.x - sPos2.x) < 0.0005)
			o.Albedo = 1;

    	else
        	clip(-1);
    }

    ENDCG
  }
  Fallback "Diffuse"
}

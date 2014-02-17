Shader "Custom/Slices"
{
  Properties
  {
    _MainTex("Texture", 2D) = "white" {}
    _SecondTex("Second Texture", 2D) = "white" {}
    _Min("MinBounds", Vector) = (0, 0, 0, 0)
    _Max("MaxBounds", Vector) = (1, 1, 1, 1)
    _CPos("CameraPos", Vector) = (1, 1, 1, 1)
  }

  SubShader
  {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    Cull Off
    LOD 200

    CGPROGRAM
    #pragma surface surf Lambert
    #include "UnityCG.cginc"

    struct Input
    {
        float2 uv_MainTex;
        float2 uv2_SecondTex;
        float3 worldPos;
        float4 color : COLOR;
    };

    sampler2D _MainTex;
    float3 _Min;
    float3 _Max;
    float3 _CPos;

    void surf (Input IN, inout SurfaceOutput o)
    {
        clip(IN.worldPos.x - _Min.x);
        clip(IN.worldPos.z - _Min.z);
        clip((_Max.x - IN.worldPos.x));
        clip((_Max.z - IN.worldPos.z));

        //float4 base = 0.6 * float4(0, 0.7, 1, 1);
        float3 base = float3(0.45, 0.5, 0.55);

        //base *= IN.color;

        float3 dVec = IN.worldPos - _CPos;
        float d = sqrt(dot(dVec, dVec));
        float val = smoothstep(100, 0, d);

        o.Albedo = base;
        o.Alpha = 0.9;
        float3 lines = 1;
        float strength = pow(lerp(base, 0, val), 2);

        if (IN.uv2_SecondTex.x > 0.97 || IN.uv2_SecondTex.x < 0.01 && IN.uv2_SecondTex.y < 0.997)
        {
            lines = 0;
            o.Alpha = 1;
        //else if (IN.uv_MainTex.y > 0.97 && IN.uv_MainTex.x > 0.999)
          //o.Albedo = pow(lerp(base, 1, val), 2);
        }

        if(IN.uv_MainTex.x > 0.97 && IN.uv_MainTex.y > 0.997)
        {
          lines = 0;
          o.Alpha = 1;
        }
        o.Albedo *= lines;
        o.Albedo.rgb *= tex2D(_MainTex, IN.uv_MainTex);
        o.Emission = 0.6f*IN.color;

    }

    ENDCG
  }
  Fallback "Diffuse"
}
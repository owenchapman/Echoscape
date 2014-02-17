Shader "Custom/FlatBlend" {
	Properties {
    _OriginalTex  ("Original Texture", 2D) = "" {}
    _BackgroundTex  ("Background Texture", 2D) = "" {}
}

SubShader {

    Pass {
    ZWrite Off
    Blend Zero One


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
        float4 projPos;
    };

    sampler2D _OriginalTex;
    float4 _OriginalTex_ST;
    sampler2D _BackgroundTex;
    float4 _BackgroundTex_ST;

    v2f vert (appdata_base v)
    {
        v2f o;
        o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
        o.uv = TRANSFORM_TEX(v.texcoord, _OriginalTex);
        o.projPos = ComputeScreenPos(o.pos);
        return o;
    }

    float4 frag (v2f i) : COLOR
    {

        float4 inColour = float4(0, 0, 0, 0.3);
        float4 currColour = tex2D(_OriginalTex, i.uv);
        float4 backColour = tex2D(_BackgroundTex, i.uv);
        inColour = backColour;

        if(currColour.a > 0.5)
            inColour = 0.5 * backColour;

        return inColour;
    }
    ENDCG

        }
    }

Fallback off

}

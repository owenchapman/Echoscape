// ORIGINAL GLSL SHADER: 'Metablob' by Adrian Boeing (2011)
Shader "mShaders/mblob1"
{
	Properties
	{
	}
	SubShader
		{
			Tags {Queue = Geometry}
			Pass
			{
				CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
 
				struct v2f 
				{
					float4 pos : POSITION;
					float4 color : COLOR0;
					float4 fragPos : COLOR1;
				};
 
				v2f vert (appdata_base v)
				{
					v2f o;
					o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
					o.fragPos = o.pos;
					o.color = float4 (1.0, 1.0, 1.0, 1.0);
					return o;
				};
 
				half4 frag (v2f i) : COLOR
				{
					//the centre point for each blob
					float2 move1;
					move1.x = -7;
					move1.y = 0;

					float2 move2;
					move2.x = 0;
					move2.y = 0;


					//screen coordinates
					float2 p = i.fragPos.xy;

					//radius for each blob
					float r1 =(dot(move1 - p,move1 - p));
					float r2 =(dot(move2 - p,move2 - p));

					//sum the meatballs 
					float metaball =(1.0/r1+1.0/r2);

					//alter the cut-off power
					float col = pow(metaball,5.0);

					//set the output color
					return float4(col,col,col,1.0);
				}
			ENDCG
			}
		}
	FallBack "VertexLit"
}
// Upgrade NOTE: commented out 'float4x4 _Object2World', a built-in variable

// Upgrade NOTE: commented out 'float4x4 _Object2World', a built-in variable

// Upgrade NOTE: commented out 'float4x4 _Object2World', a built-in variable

Shader "Custom/GridPoints" {
	SubShader {
		Pass {

			Blend SrcAlpha One
				Tags { "RenderType" = "Transparent" }
			Cull Off Zwrite Off


				CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

			struct vertexInput {
				float4 vertex : POSITION;
			};

			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 position_in_world_space : TEXCOORD0;
				float dist_from_camera : TEXCOORD1;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				output.pos =  mul(UNITY_MATRIX_MVP, input.vertex);
				output.position_in_world_space = mul(_Object2World, input.vertex);
				output.dist_from_camera = distance(_WorldSpaceCameraPos, mul(_Object2World, input.vertex));

				return output;
			}

			fixed4 frag(vertexOutput input) : COLOR0
			{

				float d = 20;
				float4 gPoint = round(input.position_in_world_space*d);
				float dist = distance(input.position_in_world_space*d, gPoint);

				if (dist <= 0.35)
				{

					float yCol = gPoint.y/100;
					float alpha = 0.35 - pow(dist, 1.3);
					alpha *= 1 - (input.dist_from_camera/50);
					//float4 col = float4(1- (yCol * 0.7), yCol * 0.7, 1- yCol, alpha);
					float4 col = float4(0.15, 0.15, 0.15, alpha);
					col *= 2 * (1 - alpha);
					return col;
					// color near origin
				}
				else
				{
					//clip(-1);
					return float4(0,0,0,0);
				}

				//return float4(0,0,0,0);
			}

			ENDCG
		}
	}

	// ---- Dual texture cards
	SubShader {
		Pass {
			SetTexture [_MainTex] {
				constantColor [_TintColor]
				combine constant * primary
			}
			SetTexture [_MainTex] {
				combine texture * previous DOUBLE
			}
		}
	}

	// ---- Single texture cards (does not do color tint)
	SubShader {
		Pass {
			SetTexture [_MainTex] {
				combine texture * primary
			}
		}
	}
}
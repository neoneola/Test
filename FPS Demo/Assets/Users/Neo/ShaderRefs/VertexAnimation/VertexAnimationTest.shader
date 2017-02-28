Shader "Custom/VertexAnimationTest" {

	Properties {
		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader {

		Pass {
			Cull Off
			CGPROGRAM

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "UnityCG.cginc"

			float4 _Tint;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct VertexData {
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct Interpolators {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;

			};

			Interpolators MyVertexProgram (VertexData v) {
				Interpolators i;
				float4 tempPos = mul(unity_ObjectToWorld, v.position);
				float tempVT = abs(((_Time*3-floor(_Time*3)))-0.5)*2-1;

				v.position.z += v.normal.z *tempVT;
				v.position.x += v.normal.x *tempVT;
				v.position.y += v.normal.y *tempVT;

				i.position = mul(UNITY_MATRIX_MVP, v.position);
				i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				i.color = float4( v.normal, 1 ) * 0.5 + 0.5;

				return i;
			}

			float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
				return tex2D(_MainTex, i.uv) * _Tint;
			}
			ENDCG
		}
	}
}
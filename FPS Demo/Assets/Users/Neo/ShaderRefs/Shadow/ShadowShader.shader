Shader "Custom/ShadowShader" {

	Subshader
	{
		Pass
		{
			Tags
			{
				"LightMode" = "FowardBase"
			}
		}
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardAdd"
			}
		}
		Pass
		{
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram

			//#include "My Shadows.cginc"

			ENDCG
		}
	}
	FallBack "Diffuse"
}

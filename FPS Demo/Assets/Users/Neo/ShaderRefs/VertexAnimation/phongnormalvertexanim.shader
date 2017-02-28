#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

//basic phong CG shader with vertex animation


Shader "CG Shaders/Phong/Phong Texture Pan Vertex Anim"
{
	Properties
	{
		_diffuseColor("Diffuse Color", Color) = (1,1,1,1)
		_diffuseMap("Diffuse", 2D) = "white" {}
		_FrenselPower ("Rim Power", Range(1.0, 10.0)) = 2.5
		_FrenselPower (" ", Float) = 2.5
		_rimColor("Rim Color", Color) = (1,1,1,1)
		_specularPower ("Specular Power", Range(1.0, 50.0)) = 10
		_specularPower (" ", Float) = 10
		_specularColor("Specular Color", Color) = (1,1,1,1)
		_normalMap("Normal / Specular (A)", 2D) = "bump" {}
		//alpha attributes
		_alphaFrenselPower ("Alpha Power", Range(1.0, 10.0)) = 1.0
		_alphaFrenselPower (" ", Float) = 1.0
		_alphaFrenselBias ("Alpha Bias", Range(0.0, 1.0)) = 0.5
		_alphaFrenselBias (" ", Float) = 0.5
		//Unity only allows floats or float4s so we split the pan speed into 2
		//uv offset for panning - X
		_uvPanSpeedX ("UV Pan Speed X", Range(0.0, 1.0)) =  0.1
		_uvPanSpeedX (" ", Float) = 0.1
		//uv offset for panning - Y
		_uvPanSpeedY ("UV Pan Speed Y", Range(0.0, 1.0)) =  0.5
		_uvPanSpeedY (" ", Float) = 0.5
		//scaling for the second sample
		_doubleSampleScale ("Double Sample Scale", Range(1.0, 2.0)) =  1.5
		_doubleSampleScale (" ", Float) = 1.5
		_doubleSampleOffset ("Double Sample Speed Scale", Range(0.5, 1.5)) =  0.9
		_doubleSampleOffset (" ", Float) = 0.9
		//vertex Offset
		_waveOffset ("Wave Offset", Range(0.0, 1.0)) =  1		
		_waveOffset(" ", Float) = 1	
		_peakOffset ("Peak Offset", Range(0.0, 1.0)) =  0.5	
		_peakOffset(" ", Float) = 0.5	
		_animationSpeed("Animation Speed", Float) = 2
		_animationFrequency("Animation Frequency", Float) = 0.5
		_Smoothing ("Smoothing", Range(0.0, 1.0)) =  0		
		_Smoothing(" ", Float) =0
		
	}
	SubShader
	{
		Pass
		{
			Tags { "LightMode" = "ForwardBase" } 
            
			//Alpha Blending - OneMinusSrcAlpha is the same as InvSrcAlpha in Maya
			Blend SrcAlpha OneMinusSrcAlpha 
			
			//Additive Alpha Blending 
			//Blend SrcAlpha One 
			CGPROGRAM
			
			#pragma vertex vShader
			#pragma fragment pShader
			#include "UnityCG.cginc"
			#pragma multi_compile_fwdbase
			#pragma target 3.0
			
			uniform fixed3 _diffuseColor;
			uniform sampler2D _diffuseMap;
			uniform half4 _diffuseMap_ST;			
			uniform fixed4 _LightColor0; 
			uniform half _FrenselPower;
			uniform fixed4 _rimColor;
			uniform half _specularPower;
			uniform fixed3 _specularColor;
			uniform sampler2D _normalMap;
			uniform half4 _normalMap_ST;	
			uniform fixed _uvPanSpeedX;
			uniform fixed _uvPanSpeedY;			
			uniform fixed _doubleSampleScale;	
			uniform fixed _doubleSampleOffset;
			uniform half _alphaFrenselPower;
			uniform fixed _alphaFrenselBias;
			//vertex animation
			fixed _peakOffset;
			fixed _waveOffset;	
			half _animationSpeed;
			half _animationFrequency;
			fixed _Smoothing;
			
			struct app2vert {
				float4 vertex 	: 	POSITION;
				fixed2 texCoord : 	TEXCOORD0;
				fixed4 normal 	:	NORMAL;
				fixed4 tangent : TANGENT;
				
			};
			struct vert2Pixel
			{
				float4 pos 						: 	SV_POSITION;
				half2 uvs						:	TEXCOORD0;
				fixed3 normalDir						:	TEXCOORD1;	
				fixed3 binormalDir					:	TEXCOORD2;	
				fixed3 tangentDir					:	TEXCOORD3;	
				half3 posWorld						:	TEXCOORD4;	
				fixed3 viewDir						:	TEXCOORD5;
				fixed3 lighting						:	TEXCOORD6;
			};
			
			fixed lambert(fixed3 N, fixed3 L)
			{
				return saturate(dot(N, L));
			}
			fixed frensel(fixed3 V, fixed3 N, half P)
			{	
				return pow(1 - saturate(dot(V,N)), P);
			}
			fixed phong(fixed3 R, fixed3 L)
			{
				return pow(saturate(dot(R, L)), _specularPower);
			}
			vert2Pixel vShader(app2vert IN)
			{
				vert2Pixel OUT;
				float4x4 WorldViewProjection = UNITY_MATRIX_MVP;
				float4x4 WorldInverseTranspose = unity_WorldToObject; 
				float4x4 World = unity_ObjectToWorld;
				
				//grab the Position before it is placed in screen space
				float4 deformedPosition = IN.vertex;
				
				//transform into worldspace first				
				deformedPosition = mul(World, deformedPosition);
				//keep track of the original position in world space for smoothing
				float4 originalPosition  = deformedPosition;
				
				//in order to keep uvs seamless, we will recalculate via worldspace position
				//this also allows the uvs to be deformed by the vertex shader
				//if this causes too much streching or pinching just move it below the deformation 
				OUT.uvs = half2(deformedPosition.x,deformedPosition.z);	
				
				//calculate the binormal
				float3 binormal = cross(IN.normal.xyz, IN.tangent.xyz);
								
				//create some other "verts" 
				// for some reason tangent needs to be reversed?
				float3 deformedPositionT = deformedPosition.xyz -IN.tangent;
				float3 originalPositionT  = deformedPositionT;
				float3 deformedPositionB = deformedPosition.xyz + binormal;
				float3 originalPositionB  = deformedPositionB;
				
				//basic offsetting of the position
				//deformedPosition.xyz = deformedPosition.xyz + _vertOffset.xyz;
				
				//offsetting the position by the sin(time);
				//by offsetting the sin wave by deformedPosition.z we can create waves instead of moving the whole mesh
				//by offsetting the cos wave by the same frequency we can sharpen the peaks of the waves.
								
				float Time = _Time.y*_animationSpeed;
				float frequency =  originalPosition.z * _animationFrequency;
				float frequencyT =  originalPositionT.z * _animationFrequency;
				float frequencyB =  originalPositionB.z * _animationFrequency;
				
				//sum 3 sine wave offsets to get the total transformation
				//you could add or reduce this as needed. past 3 waves the cost/benefit ratio drops pretty fast
				deformedPosition.y +=  ((sin(Time + frequency) ) * _waveOffset ) ;
				deformedPosition.z +=  ((cos(Time + frequency) ) * _peakOffset ) ;
				deformedPosition.y +=  ((sin(Time + frequency *2) ) * _waveOffset/ 2 );
				deformedPosition.z +=  ((cos(Time + frequency *2)) * _peakOffset /2) ;
				deformedPosition.y +=  ((sin(Time + frequency *4)) * _waveOffset /4 ) ;
				deformedPosition.z +=  ((cos(Time + frequency *4)) * _peakOffset/4) ;
				//allow the position to return to normal according to the smoothing amount
				deformedPosition.xyz -= (deformedPosition.xyz - originalPosition.xyz)* (_Smoothing);
				
				//offsetting the other "verts" the same way
				//deformedPositionT
				deformedPositionT.y +=  ((sin(Time + frequencyT) ) * _waveOffset ) ;
				deformedPositionT.z +=  ((cos(Time + frequencyT) ) * _peakOffset ) ;
				deformedPositionT.y +=  ((sin(Time + frequencyT *2) ) * _waveOffset / 2 );
				deformedPositionT.z +=  ((cos(Time + frequencyT *2)) * _peakOffset /2) ;
				deformedPositionT.y +=  ((sin(Time + frequencyT *4)) * _waveOffset/4 ) ;
				deformedPositionT.z +=  ((cos(Time + frequencyT *4)) * _peakOffset /4) ;
				//allow the position to return to normal according to the smoothing amount
				deformedPositionT.xyz -= (deformedPositionT.xyz - originalPositionT.xyz)* (_Smoothing);
				
				//deformedPositionB
				deformedPositionB.y +=  ((sin(Time + frequencyB) ) * _waveOffset ) ;
				deformedPositionB.z +=  ((cos(Time + frequencyB) ) * _peakOffset ) ;
				deformedPositionB.y +=  ((sin(Time + frequencyB *2) ) * _waveOffset / 2 );
				deformedPositionB.z +=  ((cos(Time + frequencyB *2)) * _peakOffset /2) ;
				deformedPositionB.y +=  ((sin(Time + frequencyB *4)) * _waveOffset /4 ) ;
				deformedPositionB.z +=  ((cos(Time + frequencyB *4)) * _peakOffset /4) ;
				//allow the position to return to normal according to the smoothing amount
				deformedPositionB.xyz -= (deformedPositionB.xyz - originalPositionB.xyz)* (_Smoothing);
				
				//subtract to find the new binormals
				float3 normB =  deformedPositionB -  deformedPosition.xyz ;
				//subtract to find the new tangent
				//tangent is subtracted backwards
				float3 normT =  deformedPosition.xyz - deformedPositionT ;	
				//use cross to find the new normal
				float3 normC = cross( normB, -normT );
				
				//transform the normals to object space like normal
				OUT.normalDir  = mul(  normC , WorldInverseTranspose ).xyz;				
				OUT.binormalDir = mul(  normB , WorldInverseTranspose ).xyz;				
				OUT.tangentDir = mul( normT , WorldInverseTranspose).xyz;
			
				//just pass along the world position since we are in worldspace already
				OUT.posWorld = deformedPosition.xyz; 
				

				//go back to object space
				deformedPosition = float4(mul(WorldInverseTranspose, deformedPosition).xyz * 1.0 , 1);
				//and then go to view space
				OUT.pos = mul(WorldViewProjection, deformedPosition);
							
				//view as normal
				OUT.viewDir = normalize(_WorldSpaceCameraPos - OUT.posWorld);
				
				//vertex lights
				fixed3 vertexLighting = fixed3(0.0, 0.0, 0.0);
				#ifdef VERTEXLIGHT_ON
				 for (int index = 0; index < 4; index++)
					{    						
						half3 vertexToLightSource = half3(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index]) - OUT.posWorld;
						fixed attenuation  = (1.0/ length(vertexToLightSource)) *.5;	
						fixed3 diffuse = unity_LightColor[index].xyz * lambert(OUT.normalDir, normalize(vertexToLightSource)) * attenuation;
						vertexLighting = vertexLighting + diffuse;
					}
				vertexLighting = saturate( vertexLighting );
				#endif
				OUT.lighting = vertexLighting ;
				
				return OUT;
			}
			
			fixed4 pShader(vert2Pixel IN): COLOR
			{
				half time = _Time.y;
				half2 timeOffset = half2(sin(time)*_uvPanSpeedX,time*_uvPanSpeedY);
				half2 normalUVs = TRANSFORM_TEX(IN.uvs, _normalMap);
				half2 normalUVs1 = normalUVs + timeOffset;
				half2 normalUVs2 = normalUVs * _doubleSampleScale + (timeOffset *_doubleSampleOffset);
				fixed4 normalD = tex2D(_normalMap, normalUVs1);
				normalD.xyz = (normalD.xyz * 2) - 1;
				fixed2 norma2D = tex2D(_normalMap, normalUVs2).xy;
				norma2D = (norma2D * 2) - 1;
				normalD.xy += norma2D;
				
				//half3 normalDir = half3(2.0 * normalSample.xy - float2(1.0), 0.0);
				//deriving the z component
				//normalDir.z = sqrt(1.0 - dot(normalDir, normalDir));
               // alternatively you can approximate deriving the z component without sqrt like so:  
				//normalDir.z = 1.0 - 0.5 * dot(normalDir, normalDir);
				
				fixed3 normalDir = normalD.xyz;	
				fixed specMap = normalD.w;
				normalDir = normalize((normalDir.x * IN.tangentDir) + (normalDir.y * IN.binormalDir) + (normalDir.z * IN.normalDir));
		
				fixed3 ambientL = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				half3 pixelToLightSource =_WorldSpaceLightPos0.xyz - (IN.posWorld*_WorldSpaceLightPos0.w);
				fixed attenuation  = lerp(1.0, 1.0/ length(pixelToLightSource), _WorldSpaceLightPos0.w);				
				fixed3 lightDirection = normalize(pixelToLightSource);
				fixed diffuseL = lambert(normalDir, lightDirection);				
				
				fixed3 rimLight = frensel(normalDir, IN.viewDir, _FrenselPower);
				rimLight *= saturate(dot(fixed3(0,1,0),normalDir)* 0.5 + 0.5)* saturate(dot(fixed3(0,1,0),-IN.viewDir)+ 1.75);	
				fixed3 diffuse = _LightColor0.xyz* (diffuseL+ (rimLight * diffuseL) )* attenuation;
				rimLight *= (1-diffuseL)*(_rimColor.xyz *_rimColor.w);
				diffuse = saturate(IN.lighting + ambientL + diffuse+ rimLight);
				
				fixed specularHighlight = phong(-reflect(IN.viewDir , normalDir) ,lightDirection)*attenuation;
				
				fixed4 outColor;							
				half2 diffuseUVs = TRANSFORM_TEX(IN.uvs, _diffuseMap);
				diffuseUVs = diffuseUVs + timeOffset;
				fixed4 texSample = tex2D(_diffuseMap, diffuseUVs);
				fixed3 diffuseS = (diffuse * texSample.xyz) * _diffuseColor.xyz;
				fixed3 specular = (specularHighlight * _specularColor * specMap);
				fixed rimAlpha = saturate(frensel(normalDir, IN.viewDir, _alphaFrenselPower)+_alphaFrenselBias);	
				outColor = fixed4( diffuseS + specular,rimAlpha);
				return outColor;
			}
			
			ENDCG
		}	
		
		
	}
}
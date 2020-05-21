Shader "Alo/Jelly/BlobShader_01"
{
	Properties {
		_MainColor ("Main Colour", Color) = (1,1,1,1)
		_WrapFactor ("Wrap Factor", Range(0,1)) = 0

		_ImpactPoint1 ("Impact Point - 1", Vector) = (0,0,0,0)
		_Amplitude1 ("Primary Amplitude - Impact 1", Float) = 1
		_Range1 ("Primary Range - Impact 1", Float) = 1
		_Wavelength1 ("Primary Wavelength - Impact 1", Float) = 3
		_Speed1 ("Primary Speed - Impact 1", Float) = 1
		_SecondaryAmplitude1 ("Secondary Amplitude - Impact 1", Float) = 1
		_SecondaryRange1 ("Secondary Range - Impact 1", Float) = 1
		_SecondaryWavelength1 ("Secondary Wavelength - Impact 1", Float) = 1
		_SecondarySpeed1 ("Secondary Speed - Impact 1", Float) = 1
		_TimerImpact1 ("Timer - Impact 1", Float) = 0
		_SubsurfaceRange1 ("Subsurface Range - Impact 1", Range(0,4.25)) = 1
		_SubsurfaceIntensity1 ("Subsurface Intensity - Impact 1", Range(0,1)) = 0
		

		[NoScaleOffset] _CubeMap ("Specular Reflections Cube Map", Cube) = "black"{}

		_ImpactPoint2 ("Impact Point - 2", Vector) = (0,0,0,0)
		_Amplitude2 ("Amplitude - Impact 2", float) = 1
		_Range2 ("Range - Impact 2", Float) = 1
		_Wavelength2 ("Wavelength - Impact 2", Float) = 3
		_Speed2 ("Speed - Impact 2", Float) = 1
		_SecondaryAmplitude2 ("Secondary Amplitude - Impact 2", Float) = 1
		_SecondaryRange2 ("Secondary Range - Impact 2", Float) = 1
		_SecondaryWavelength2 ("Secondary Wavelength - Impact 2", Float) = 1
		_SecondarySpeed2 ("Secondary Speed - Impact 2", Float) = 1
		_TimerImpact2 ("Timer - Impact 2", Float) = 0
		_SubsurfaceRange2 ("Subsurface Range - Impact 2", Range(0,4.25)) = 1
		_SubsurfaceIntensity2 ("Subsurface Intensity - Impact 2", Range(0,1)) = 0

		_Reflectivity ("Reflectivity", Range(0,1)) = 1
		_Roughness ("Roughness", Range(0,1)) = 0
		_FresnelFactor ("Fresnel Factor", Float) = 1
		_FresnelIntensity ("Fresnel Intensity", Range(0,1)) = 0.05
		_ReflectionAttenuation ("Reflection Attenuation Factor", Range(0,1)) = 0

		[NoScaleOffset] _IridescenceRamp ("Iridescence Ramp Texture", 2D) = "white" {}

		_SSSPower ("SSS Power", Float) = 1
		_SSSIntensity ("SSS Intensity", Float) = 1

		[HDR] _SSSColor ("SSS Color", Color) = (0,0,0,1)

		_GroundContactMaskRange ("Ground Contact Mask", Float) = 0.5

		_DebugSlider ("Debug Slider", Range(0,1)) = 0

		//_RippleColorMask ("Ripple Colour Mask", 2D) = "white"{}
		//_ParallaxStrength ("Parallax Strength", Range(0,5)) = 0


		//_TestSlider ("Test Slider", Range(0,1)) = 0

		//_NormalMap ("Normal Map", 2D) = "bump"{}
		//_NormalMapIntensity ("Normal Map Intensity", Range(0,1)) = 1

		_SparkleTex ("Sparkle Texture", 2D) = "black"{}
		_SparkleScale ("Sparkle Scale", Float) = 1
		_SparkleSlideMultiplier ("Sparkle Slide Multiplier", Float) = 1
		_SparkleFar ("Sparkle Far", Float) = 2
		_SparkleNear ("Sparkle Near", Float) = 1
	}
	SubShader {

		//Cull Off

		Pass {
		
			CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			#pragma target 3.0
	
			#include "UnityCG.cginc"

			struct VertexData {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct Interpolators {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float3 wNormal : NORMAL;
				float3 wPos : TEXCOORD1;
				float3 oPos : TEXCOORD2;
				float3 oPos2 : TEXCOORD3;
				float3 oNormal : TEXCOORD4;
				//float3 tangentViewDir : TEXCOORD3;//parallax
				//float4 tangent : TEXCOORD4;//normal mapping
			};


			float4 _MainColor;
			float _WrapFactor;
			float4 _LightColor0;
			float4 _ImpactPoint1;
			float _Amplitude1;
			float _Range1, _Speed1, _Wavelength1;
			samplerCUBE	_CubeMap;
			float _SpecularPow, _Reflectivity;
			float _Roughness;
			float _FresnelFactor, _FresnelIntensity;
			float _ReflectionAttenuation;
			sampler2D _IridescenceRamp;
			float _SSSPower, _SSSIntensity;
			float4 _SSSColor;

			float4 _ImpactPoint2;
			float _Amplitude2;
			float _Range2, _Speed2, _Wavelength2;
			float _GroundContactMaskRange;
			float _SecondaryAmplitude1, _SecondaryRange1, _SecondarySpeed1, _SecondaryWavelength1;
			float _TimerImpact1;

			float _SecondaryAmplitude2, _SecondaryRange2, _SecondarySpeed2, _SecondaryWavelength2;
			float _TimerImpact2;

			float _DebugSlider;
			float _SubsurfaceRange1, _SubsurfaceIntensity1;
			float _SubsurfaceRange2, _SubsurfaceIntensity2;

			//sampler2D _RippleColorMask;
			//float4 _RippleColorMask_ST;

			//float _ParallaxStrength;

			//float _TestSlider;

			//sampler2D _NormalMap;
			//float _NormalMapIntensity;

			sampler2D _SparkleTex;
			float _SparkleScale, _SparkleSlideMultiplier, _SparkleFar, _SparkleNear;
			
			Interpolators VertexProgram (VertexData v){
				Interpolators i;
				
				i.oPos = v.vertex.xyz;//before offset
				float heightMask = smoothstep(0, _GroundContactMaskRange, v.vertex.z);
				heightMask += 0.12 * (1-heightMask);//make the base still wiggly-wobbly a bit

				i.uv = v.uv;// * _RippleColorMask_ST.xy;

				float3 p = v.vertex.xyz;
				float3 tans = v.tangent.xyz;

				//////Hit 1 //////////
				float k = 2 * UNITY_PI / (_Wavelength1);
				float distFromHit1 = distance(p, _ImpactPoint1.xyz);
				float distanceMask1 = smoothstep(0,1, 1 - max(0.0, (distFromHit1) / _Range1));
				float f = k * (distFromHit1 - _Speed1 * _TimerImpact1);

				float3 d = v.vertex.xyz - _ImpactPoint1.xyz;
				d = normalize(d);
				float a = _Amplitude1 * distanceMask1 * heightMask;
				float sinF = sin(f);

				float kSecondary1 = 2 * UNITY_PI / (_SecondaryWavelength1);
				float distanceMaskSecondary1 = smoothstep(0,1, 1 - max(0.0, (distFromHit1) / _SecondaryRange1));
				float fSecondary1 = kSecondary1 * (distFromHit1 - _SecondarySpeed1 * _TimerImpact1);
				float aSecondary1 = _SecondaryAmplitude1 * distanceMaskSecondary1 * heightMask;
				float sinFSecondary1 = sin(fSecondary1);
				//////End Hit 1 //////////

				//////Hit 2 //////////
				float k2 = 2 * UNITY_PI / (_Wavelength2);
				float distFromHit2 = distance(p, _ImpactPoint2.xyz);
				float distanceMask2 = smoothstep(0,1, 1 - max(0.0, (distFromHit2) / _Range2));
				float f2 = k2 * (distFromHit2 - _Speed2 * _TimerImpact2);

				float3 d2 = normalize(v.vertex.xyz - _ImpactPoint2.xyz);
				float a2 = _Amplitude2 * distanceMask2 * heightMask;
				float sinF2 = sin(f2);

				float kSecondary2 = 2 * UNITY_PI / (_SecondaryWavelength2);
				float distanceMaskSecondary2 = smoothstep(0,1, 1 - max(0.0, (distFromHit2) / _SecondaryRange2));
				float fSecondary2 = kSecondary2 * (distFromHit2 - _SecondarySpeed2 * _TimerImpact2);
				float aSecondary2 = _SecondaryAmplitude2 * distanceMaskSecondary2 * heightMask;
				float sinFSecondary2 = sin(fSecondary2);
				//////End Hit 2 //////////

				p.x = p.x + (sinF * a + sinFSecondary1 * aSecondary1 + sinF2 * a2 + sinFSecondary2 * aSecondary2) * v.normal.x;
				p.y = p.y + (sinF * a + sinFSecondary1 * aSecondary1 + sinF2 * a2 + sinFSecondary2 * aSecondary2) * v.normal.y;
				p.z = p.z + (sinF * a + sinFSecondary1 * aSecondary1 + sinF2 * a2 + sinFSecondary2 * aSecondary2) * v.normal.z;

				float derivative = k * a * cos(f) + k2 * a2 * cos(f2) + kSecondary1 * aSecondary1 * cos(fSecondary1) + kSecondary2 * aSecondary2 * cos(fSecondary2);

				float dirDotTangent = dot(d, tans) + dot(d2, tans);
			
				float3 tangent = 0;
				tangent.x = tans.x + v.normal.x * derivative * dirDotTangent;
				tangent.y = tans.y + v.normal.y * derivative * dirDotTangent;
				tangent.z = tans.z + v.normal.z * derivative * dirDotTangent;
				tangent *= -(v.tangent.w * unity_WorldTransformParams.w);

				float3 baseBinormal = cross(v.normal.xyz, v.tangent.xyz) * (v.tangent.w * unity_WorldTransformParams.w);
				float dirDotBinormal = dot(d, baseBinormal) + dot(d2, baseBinormal);

				float3 binormal = 0;
				binormal.x = baseBinormal.x + v.normal.x * derivative * dirDotBinormal;
				binormal.y = baseBinormal.y + v.normal.y * derivative * dirDotBinormal;
				binormal.z = baseBinormal.z + v.normal.z * derivative * dirDotBinormal;


				float3 normal = normalize(cross(binormal, tangent));

				v.vertex.xyz = p;
				v.normal = normal;

				//float3x3 objectToTangent = float3x3(tangent, cross(normal, tangent) * v.tangent.w, normal);//parallax stuff
				//i.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

				i.pos = UnityObjectToClipPos(v.vertex);
				i.wNormal = UnityObjectToWorldNormal(v.normal);
				i.oNormal = v.normal.xyz;
				i.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				i.oPos2 = UnityObjectToClipPos(v.vertex);
				//i.tangent = float4(UnityObjectToWorldDir(tangent.xyz), v.tangent.w);//normal mapping

				return i;
			}



			float3 SampleCubeMap(samplerCUBE _cube, float3 _normal, float _mip){
				return texCUBElod(_cube, float4(_normal, _mip));
			}

			float3 SubsurfaceScattering(float3 _viewDir, float3 _lightColor, float3 _lightDir, float3 _normal, float3 _sssColor) {
				//float3 halfVector = normalize(_lightDir + _normal * _SSSDistortion);
				//float viewDotHalfVec = max(0.0, dot(-halfVector, _viewDir));
				float3 sss = _sssColor * _lightColor * pow(0.5, _SSSPower) * _SSSIntensity;

				return sss;

			}

			float3 CustomNormals(float4 packedNormals, float intensity){
				float3 normal;
				normal.xy = packedNormals.wy * 2 + 1;
				normal.xy *= intensity;
				normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
				return normal;
			}
			
			float4 FragmentProgram (Interpolators i) : SV_Target {

				float distFromHit1 = distance(i.oPos, _ImpactPoint1);
				float distanceMask = smoothstep(0,1, 1 - max(0.0, (distFromHit1) / (_SubsurfaceRange1)));

				float distanceMaskPrimary1 = smoothstep(0,1, 1 - max(0.0, (distFromHit1) / (_Range1*0.25)));

				float distFromHit2 = distance(i.oPos, _ImpactPoint2);
				float distanceMask2 = smoothstep(0,1, 1 - max(0.0, (distFromHit2) / _SubsurfaceRange2));

				float distanceMask_SpecialDemo = smoothstep(0,1, 1 - max(0.0, (distFromHit1) / 4.25));//for demo purposes

				float distanceMaskPrimary2 = smoothstep(0,1, 1 - max(0.0, (distFromHit1) / (_Range2*0.25)));

				float k = 2 * UNITY_PI / (_Wavelength1);
				float f = k * (distFromHit1 - _Speed1 * _TimerImpact1);
				float sinF = sin(f);
				float sineMask = smoothstep(-1,1,sinF);

				float k2 = 2 * UNITY_PI / (_Wavelength2);
				float f2 = k2 * (distFromHit2 - _Speed2 * _TimerImpact2);
				float sinF2 = sin(f2);
				float sineMask2 = smoothstep(-1,1,sinF2);

				//float totalMask = min(1.0, distanceMask * sineMask + distanceMask2 * sineMask2);
				float totalMask = smoothstep(0,1, min(1.0, distanceMask+sineMask*distanceMask + distanceMask2+sineMask2*distanceMask2));
				
				//normal map stuff
				//float3 tNormal = CustomNormals(tex2D(_NormalMap, i.uv), _NormalMapIntensity * totalMask);
				//float3 binormal = cross(i.wNormal, i.tangent.xyz) * (i.tangent.w * unity_WorldTransformParams.w);
				//i.wNormal = normalize(
				//		tNormal.x * i.tangent + 
				//		tNormal.y * binormal + 
				//		tNormal.z * i.wNormal
				//);
				//////



				float3 lightDir = _WorldSpaceLightPos0.xyz;
				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.wPos);

				float viewDotNormal = max(0.0, dot(viewDir, i.wNormal));
				float NdotL = max(0.0, dot(-i.wNormal, lightDir));

				float3 iridescence = tex2D(_IridescenceRamp, viewDotNormal).rgb;

				float fresnel = pow(1 - viewDotNormal, _FresnelFactor) * _FresnelIntensity;

				float3 cubeMapColor = SampleCubeMap(_CubeMap, i.wNormal.xyz, 5 * (min(1.0, _Roughness + min(1.0, sineMask * distanceMaskPrimary1 + sineMask2 * distanceMaskPrimary2)*0.5)));


				//////sparkles
				//float3 osViewDir = mul(unity_WorldToObject, float4(viewDir,1)).xyz;

				float camToPos = length(_WorldSpaceCameraPos.xyz - i.wPos);


				float2 sparkleUV1_xz = i.oPos.xy * _SparkleScale + viewDir.xz * _SparkleSlideMultiplier;
				float2 sparkleUV2_xz = i.oPos.xy * _SparkleScale - viewDir.xz * _SparkleSlideMultiplier;
				//float2 sparkleUV1 = i.wPos.xy * _SparkleScale + camToPos * _SparkleSlideMultiplier;
				//float2 sparkleUV2 = i.wPos.xy * _SparkleScale - camToPos * _SparkleSlideMultiplier;
				float sparkleSpec_xz = tex2D(_SparkleTex, sparkleUV1_xz).r * tex2D(_SparkleTex, sparkleUV2_xz).g;

				float2 sparkleUV1_yz = i.oPos.xz * _SparkleScale + viewDir.xy * _SparkleSlideMultiplier;
				float2 sparkleUV2_yz = i.oPos.xz * _SparkleScale - viewDir.xy * _SparkleSlideMultiplier;
				float sparkleSpec_yz = tex2D(_SparkleTex, sparkleUV1_yz).r * tex2D(_SparkleTex, sparkleUV2_yz).g;

				float2 sparkleUV1_xy = i.oPos.zy * _SparkleScale + viewDir.yz * _SparkleSlideMultiplier;
				float2 sparkleUV2_xy = i.oPos.zy * _SparkleScale - viewDir.yz * _SparkleSlideMultiplier;
				float sparkleSpec_xy = tex2D(_SparkleTex, sparkleUV1_xy).r * tex2D(_SparkleTex, sparkleUV2_xy).g;

				float3 absNormal = abs(i.oNormal);
				float3 weights = absNormal / (absNormal.x + absNormal.y + absNormal.z);
				float sparkleSpec =  sparkleSpec_xz * weights.z + sparkleSpec_yz * weights.y + sparkleSpec_xy * weights.x;

				float sparkleDistance = smoothstep(_SparkleFar, _SparkleNear, camToPos);
				sparkleSpec *= 80 * sparkleDistance;// * max(0.0, i.wNormal.y-(0.2 + 0.2 * rng));
				//////

				////
				//i.tangentViewDir = normalize(i.tangentViewDir);
				//i.tangentViewDir.xy /= (i.tangentViewDir.z + 0.42);
				//float height = tex2D(_RippleColorMask, i.uv.xy).r;
				//height -= 0.5;
				//height *= _ParallaxStrength;
				//float2 parallaxedUV = i.uv.xy + i.tangentViewDir.xy * height;
				////

				
				float3 sssColor = SubsurfaceScattering(viewDir, _LightColor0.rgb, -lightDir, i.wNormal, _SSSColor);
				//float3 reflectionColor = (cubeMapColor * (1 - _ReflectionAttenuation)) * iridescence + sssColor * totalMask * (_Amplitude1/0.13);//HACK HERE
				sssColor = sssColor * max(min(1.0, sineMask*distanceMask *_SubsurfaceIntensity1 + sineMask2*distanceMask2 *_SubsurfaceIntensity2), distanceMask_SpecialDemo* _DebugSlider);
				//sssColor = sssColor * max(min(1.0,sineMask*distanceMask *_SubsurfaceIntensity1, distanceMask* _DebugSlider);
				float3 reflectionColor = (cubeMapColor * (1 - _ReflectionAttenuation)) * iridescence + sssColor;

				//float parallaxColor = tex2D(_RippleColorMask, parallaxedUV).r;

				float3 wrappedDiffuse = _MainColor.rgb * saturate((NdotL + _WrapFactor) / ((1 + _WrapFactor) * (1 + _WrapFactor)));

				float3 color = wrappedDiffuse + reflectionColor * _Reflectivity + fresnel;
				

				color += sparkleSpec;//sparkles

				//return float4(heightMask.xxx, 1);
				return float4(color, 1);
			}
			ENDCG
		}
	}
}

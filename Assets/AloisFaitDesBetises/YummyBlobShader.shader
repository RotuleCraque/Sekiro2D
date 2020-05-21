Shader "Alo/YummyBlobShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColour ("Main Colour", Color) = (0.7, 0.02, 0.27, 1)
        _ShadowColour ("Shadow Colour", Color) = (0.2, 0.02, 0.13, 1)
        _Roughness ("Roughness", Range(0,1)) = 0
        _FallOffHigh ("Terminator Fall-Off High", Range(0, 1)) = 0.5
        _FallOffLow ("Terminator Fall-Off Low", Range(0, 1)) = 0.5

        _CubeMap ("Environment Map", Cube) = "white"{}
        _Reflectivity ("Reflectivity", Range(0,1)) = 0.5

        _SpecularPower ("Specular Power", Float) = 2
        _SpecularIntensity ("SpecularIntensity", Float) = 1

        _SpecularHighlightFallOffHigh ("Specular Highlight Fall-Off High", Range(0,1)) = 1
        _SpecularHighlightFallOffLow ("Specular Highlight Fall-Off Low", Range(0,1)) = 0

        _GradientOffsetY ("Gradient Offset Y", Float) = 0
        _GradientOffsetX ("Gradient Offset X", Float) = 0

        _FresnelPower ("Fresnel Power", Float) = 1



        _Wavelength0 ("Wavelength0", Float) = 5
        _Position0 ("Position0", Vector) = (0,0,0,0)
        _Range0 ("Range0", Float) = 3
        _Timer0 ("Timer0", Float) = 0
        _Amplitude0 ("Amplitude0", Float) = 1
        _Progress0 ("Progress0", Range(0,1)) = 0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram

            #include "UnityCG.cginc"

            struct vertexInput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct vertexOutput {            
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 wsNormal : NORMAL;
                float3 wsPosition : TEXCOORD1;
                float3 lsPosition : TEXCOORD2;
            };

            float _Wavelength0, _Range0, _Timer0, _Amplitude0, _Progress0;
            float4 _Position0;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Roughness, _Reflectivity;
            float4 _MainColour, _ShadowColour;

            float _FallOffHigh, _FallOffLow;
            samplerCUBE _CubeMap;

            float _SpecularPower, _SpecularIntensity, _SpecularHighlightFallOffLow, _SpecularHighlightFallOffHigh;
            float _GradientOffsetY, _GradientOffsetX;

            float _InputDirectionX;
            float _FresnelPower;

            vertexOutput VertexProgram (vertexInput v) {

                /////////////////////////////////////////////
                float k = 2 * 3.1415927 / (_Wavelength0);
				float distFromHit1 = distance(v.vertex.xyz, _Position0.xyz);
				float distanceMask1 = smoothstep(0,1, 1 - max(0.0, (distFromHit1) / _Range0));
				float f = k * (distFromHit1 + _Timer0);

				float3 d = v.vertex.xyz - _Position0.xyz;
				d = normalize(d);
				float a = _Amplitude0 * distanceMask1;
				float sinF = sin(f);

                float3 p = v.vertex.xyz;
                p.x = p.x + sinF * a * v.normal.x;
				p.y = p.y + sinF * a * v.normal.y;
				p.z = p.z + sinF * a * v.normal.z;

                float derivative = k * a * cos(f);

                float dirDotTangent = dot(d, v.tangent.xyz);

                float3 tangent = 0;
				tangent.x = v.tangent.x + v.normal.x * derivative * dirDotTangent;
				tangent.y = v.tangent.y + v.normal.y * derivative * dirDotTangent;
				tangent.z = v.tangent.z + v.normal.z * derivative * dirDotTangent;
				tangent *= -(v.tangent.w * unity_WorldTransformParams.w);

                float3 baseBinormal = cross(v.normal.xyz, v.tangent.xyz) * (v.tangent.w * unity_WorldTransformParams.w);
				float dirDotBinormal = dot(d, baseBinormal);

				float3 binormal = 0;
				binormal.x = baseBinormal.x + v.normal.x * derivative * dirDotBinormal;
				binormal.y = baseBinormal.y + v.normal.y * derivative * dirDotBinormal;
				binormal.z = baseBinormal.z + v.normal.z * derivative * dirDotBinormal;

				float3 normal = normalize(cross(binormal, tangent));

				v.vertex.xyz = p;
				v.normal = normal;
                /////////////////////////

                vertexOutput o;
                o.lsPosition = v.vertex.xyz;
                o.wsPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.wsNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float3 SampleTexCube (samplerCUBE cubeMap, float3 normal, float mip) {
                return texCUBElod(cubeMap, float4(normal, mip));
            }

            fixed4 FragmentProgram (vertexOutput i) : SV_Target {
                fixed4 col = _MainColour;

                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float3 viewDir = normalize(i.wsPosition - _WorldSpaceCameraPos);
                float NdotL = max(0.0, smoothstep(_FallOffLow, _FallOffHigh, dot(i.wsNormal, lightDir)));
                float NdotV = max(0.0, dot(i.wsNormal, viewDir));


                float fresnelMask = pow(1 - saturate(dot(-viewDir, i.wsNormal)), _FresnelPower);

                float groundGradientMask = saturate(i.lsPosition.y - _GradientOffsetY);
                float sideGradientMask = saturate(i.lsPosition.x * _InputDirectionX - _GradientOffsetX) * (1-saturate(i.lsPosition.z + 0.5));
                float gradientMask = groundGradientMask * sideGradientMask;
                gradientMask *= gradientMask;


                col = lerp(_ShadowColour, _MainColour, groundGradientMask * sideGradientMask);


                float3 lightReflectionDir = reflect(lightDir,  i.wsNormal);
                float RdotV = max(0.0, smoothstep(_SpecularHighlightFallOffLow, _SpecularHighlightFallOffHigh, dot(lightReflectionDir, viewDir)));
                float specularity = pow(RdotV, _SpecularPower) * _SpecularIntensity;

                float3 specular = SampleTexCube(_CubeMap, reflect(viewDir, i.wsNormal), _Roughness * 5) * lerp(0, _Reflectivity, gradientMask); 

                col += float4(specular * fresnelMask, 0);
                col = saturate(col + specularity);

                //oren-nayar lighting
                //float roughness2 = _Roughness * _Roughness;
                //float3 orenNayarFraction = roughness2 / (roughness2 + float3(0.33, 0.13, 0.09));
                //float3 orenNayar = float3(1, 0, 0) + float3(-0.5, 0.17, 0.45) * orenNayarFraction;
                //float orenNayar_s = max(0.0, dot(viewDir, lightDir)) - NdotL * NdotV;
                //orenNayar_s /= lerp(max(NdotL, NdotV), 1, step(orenNayar_s, 0));
                //float3 endResult = col * NdotL * (orenNayar.x + col * orenNayar.y + orenNayar.z * orenNayar_s);
                //endResult += specular * (1 - _MainColour.rgb);


                



                //return float4(endResult, 1);
                //return float4(specular,1);
                return col;
                //return float4(i.wsNormal,1);
            }
            ENDCG
        }
    }
}

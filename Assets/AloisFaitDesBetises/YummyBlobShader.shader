Shader "Alo/YummyBlobShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColour ("Main Colour", Color) = (0.7, 0.02, 0.27, 1)
        _ShadowColour ("Shadow Colour", Color) = (0.2, 0.02, 0.13, 1)
        _Roughness ("Roughness", Range(0,1)) = 0
        _FallOffHigh ("Terminator Fall-Off High", Range(0, 1)) = 0.5
        _FallOffLow ("Terminator Fall-Off Low", Range(0, 1)) = 0.5

        _CubeMap ("Environment Map", 3D) = "white"{}
        _Reflectivity ("Reflectivity", Range(0,1)) = 0.5
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
            };

            struct vertexOutput {            
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 wsNormal : NORMAL;
                float wsPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Roughness, _Reflectivity;
            float4 _MainColour, _ShadowColour;

            float _FallOffHigh, _FallOffLow;
            samplerCUBE _CubeMap;

            vertexOutput VertexProgram (vertexInput v) {
                vertexOutput o;
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
                // sample the texture
                fixed4 col = _MainColour;

                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float3 viewDir = normalize(i.wsPosition - _WorldSpaceCameraPos);
                float NdotL = max(0.3, smoothstep(_FallOffLow, _FallOffHigh, dot(i.wsNormal, lightDir)));
                float NdotV = max(0.0, dot(i.wsNormal, viewDir));


                col = lerp(_ShadowColour, _MainColour, NdotL);


                //float3 lightReflectionDir = reflect(lightDir,  i.wsNormal);
                //float RdotV = max(0.0, dot(lightReflectionDir, viewDir));
                //float specularity = pow(RdotV, 40/4) * 1;
                //col += specularity;

                float3 specular = SampleTexCube(_CubeMap, reflect(viewDir, i.wsNormal), _Roughness * 5) * _Reflectivity; 

                //oren-nayar lighting
                float roughness2 = _Roughness * _Roughness;
                float3 orenNayarFraction = roughness2 / (roughness2 + float3(0.33, 0.13, 0.09));
                float3 orenNayar = float3(1, 0, 0) + float3(-0.5, 0.17, 0.45) * orenNayarFraction;
                float orenNayar_s = max(0.0, dot(viewDir, lightDir)) - NdotL * NdotV;
                orenNayar_s /= lerp(max(NdotL, NdotV), 1, step(orenNayar_s, 0));
                float3 endResult = col * NdotL * (orenNayar.x + col * orenNayar.y + orenNayar.z * orenNayar_s); 

                endResult += specular * (1 - _MainColour.rgb);

                return float4(endResult, 1);
                //return col;
            }
            ENDCG
        }
    }
}

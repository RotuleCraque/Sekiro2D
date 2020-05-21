Shader "Alo/EnvironmentBlockBaseShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColour ("Main Colour", Color) = (0.7, 0.02, 0.27, 1)
        _ShadowColour ("Shadow Colour", Color) = (0.2, 0.02, 0.13, 1)
        _GroundColour ("Ground Colour", Color) = (0,0,0,1)
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
        _GradientOffsetZ ("Gradient Offset Z", Float) = 0

        _FogColour ("Fog Colour", Color) = (1,1,1,1)
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
                float3 wsPosition : TEXCOORD1;
                float3 lsPosition : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Roughness, _Reflectivity;
            float4 _MainColour, _ShadowColour, _GroundColour, _FogColour;

            float _FallOffHigh, _FallOffLow;
            samplerCUBE _CubeMap;

            float _SpecularPower, _SpecularIntensity, _SpecularHighlightFallOffLow, _SpecularHighlightFallOffHigh;
            float _GradientOffsetY, _GradientOffsetX, _GradientOffsetZ;

            vertexOutput VertexProgram (vertexInput v) {
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


                //float worldPositionGradientY = saturate(i.wsPosition.y - _GradientOffsetY);
                float worldPositionGradientY = saturate(i.wsPosition.y * 0.04 - _GradientOffsetY);
                float worldPositionGradientX = saturate(i.wsPosition.x * 0.04 - _GradientOffsetX);
                float worldPositionGradient = worldPositionGradientX * worldPositionGradientY;

                float worldPositionGradientZ = saturate(i.wsPosition.z * 0.04 - _GradientOffsetZ);//pretend fog
                //col = worldPositionGradient;

                col = lerp(_ShadowColour, _MainColour, worldPositionGradient);
                col = lerp(_GroundColour, col, saturate(-i.wsNormal.z));

                col = lerp(col, _FogColour, worldPositionGradientZ);

                //return float4(endResult, 1);
                //return float4(specular,1);
                return col;
                //return float4(i.wsNormal,1);
            }
            ENDCG
        }
    }
}

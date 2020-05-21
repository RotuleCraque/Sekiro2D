Shader "Alo/DebugShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColour ("Main Colour", Color) = (0.7, 0.02, 0.27, 1)
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
                float3 lsTangent : TEXCOORD3;
                float3 lsNormal : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainColour;


            vertexOutput VertexProgram (vertexInput v) {
                vertexOutput o;
                o.lsPosition = v.vertex.xyz;
                o.lsNormal = v.normal;
                o.lsTangent = v.tangent.xyz;
                o.wsPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.wsNormal = UnityObjectToWorldNormal(v.normal);



                return o;
            }

            fixed4 FragmentProgram (vertexOutput i) : SV_Target {
                fixed4 col = float4(i.lsTangent, 1);

                return col;
            }
            ENDCG
        }
    }
}

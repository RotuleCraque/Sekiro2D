Shader "Unlit/CustomSkyboxShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_HorizontalPow ("Horizontal Power", Float) = 3
		_VerticalPow ("Vertical Power", Float) = 3
		_TopColour ("Top Colour", Color) = (1,1,1,1)
		_BottomColour ("Bottom Colour", Color) = (1,0,0.2,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float4 grabUV : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _TopColour, _BottomColour;
			float _VerticalPow, _HorizontalPow;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.grabUV = ComputeScreenPos(o.vertex);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

				float grabU = i.grabUV.x / i.grabUV.w;
				float grabV = i.grabUV.y / i.grabUV.w;

				float gradientU = 1-pow(abs(grabU - 0.5), _HorizontalPow);

				float gradientV = pow(grabV.x, _VerticalPow);
				float3 endGRadient = gradientU * gradientV;

				float3 endColour = lerp(_BottomColour, _TopColour, endGRadient);

                return float4(endColour, 1);
				//return float4((1-gradientU.xxx) * gradientV, 1);
            }
            ENDCG
        }
    }
}

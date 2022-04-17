Shader "Custom/SplatterShader" {
	Properties{
		[HideInInspector]
		_SplatTex("Splat texture", 2D) = "" {}

		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_NormalTex("Normal Map", 2D) = "bump" {}
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
	
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _SplatTex;
		sampler2D _MainTex;
		sampler2D _NormalTex;

	struct Input {
		float2 uv_MainTex;
		float2 uv_NormalTex;
		float2 uv2_SplatTex;
	};

	void surf(Input IN, inout SurfaceOutputStandard o) {
		float4 drawData = tex2D(_SplatTex, IN.uv2_SplatTex);
		float4 mainData = tex2D(_MainTex, IN.uv_MainTex);
		fixed4 c = lerp(mainData, drawData, drawData.a);
		c.a = drawData.a + mainData.a;
		o.Albedo = c.rgb;

		//fixed3 normalMap = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));
		//normalMap.x *= drawData.a; // Normal is amplified under the paint
		//normalMap.y *= drawData.a;
		//o.Normal = normalize(normalMap);

		o.Alpha = c.a;
	}
	ENDCG
	}
		FallBack "Diffuse"
}

//Shader "Custom/SplatterShader"
//{
//    Properties
//    {
//        _MainTex ("Main Texture", 2D) = "white" {}
//		_BumpMap("Bump Map", 2D) = "bump" {}
//
//		[HideInInspector]
//		_SplatTex("Splatter Texture", 2D) = "" {}
//    }
//    SubShader
//    {
//        Tags { "RenderType"="Opaque" }
//        LOD 200
//
//        Pass
//        {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//			#pragma target 3.0
//
//            #include "UnityCG.cginc"
//
//			sampler2D _MainTex;
//			sampler2D _BumpMap;
//			sampler2D _SplatTex;
//
//            struct v2f
//            {
//                float2 uv : TEXCOORD0;
//				float3 normal : NORMAL;
//                float4 vertex : SV_POSITION;
//            };
//
//            v2f vert (float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
//            {
//                v2f o;
//                o.vertex = UnityObjectToClipPos(vertex);
//				o.normal = UnityObjectToWorldNormal(normal);
//                o.uv = uv;
//                return o;
//            }
//
//            fixed4 frag (v2f i) : SV_Target
//            {
//				half4 splatTexInfo = tex2D(_SplatTex, i.uv);
//				half4 mainTexInfo = tex2D(_MainTex, i.uv);
//				half3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));
//				normal.x = normal.x * splatTexInfo.a;
//				normal.y = normal.y * splatTexInfo.a;
//				normal = normalize(normal);
//
//				fixed4 color = lerp(mainTexInfo, splatTexInfo, splatTexInfo.a);
//				color.a = mainTexInfo.a + splatTexInfo.a;
//				//color *= normal;
//
//				return color;
//            }
//            ENDCG
//        }
//    }
//}

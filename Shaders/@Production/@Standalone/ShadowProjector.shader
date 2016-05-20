Shader "Zios/General/Shadow Projector"{ 
	Properties {
		shadowMap ("Cookie", 2D) = "gray" {}
		falloffMap ("FallOff", 2D) = "white" {}
	}
	SubShader{
		Tags{"Queue"="Transparent"}
		Pass{
			ZWrite Off
			Fog{Color(1,1,1)}
			AlphaTest Greater 0
			ColorMask RGB
			Blend DstColor Zero
			Offset -1,-1
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D shadowMap;
			sampler2D falloffMap;
			struct v2f {
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
				float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, vertex);
				o.uvShadow = mul (unity_Projector, vertex);
				o.uvFalloff = mul (unity_ProjectorClip, vertex);
				return o;
			}			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texS = tex2Dproj (shadowMap, UNITY_PROJ_COORD(i.uvShadow));
				texS.a = 1.0-texS.a;
 
				fixed4 texF = tex2Dproj (falloffMap, UNITY_PROJ_COORD(i.uvFalloff));
				fixed4 res = lerp(fixed4(1,1,1,0), texS, texF.a);
				return res;
			}
			ENDCG
		}
	}
}

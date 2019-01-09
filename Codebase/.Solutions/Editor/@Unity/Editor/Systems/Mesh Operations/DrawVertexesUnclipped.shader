Shader "Zios/Utility/Draw Vertexes Outlined"{
	Properties{
		[Header(Vertex)]
		[KeywordEnum(Billboard,Surface)] quadMode("Mode",Float) = 0
		[Enum(Less,0,Greater,1,LEqual,2,GEqual,3,Equal,4,NotEqual,5,Always,6)] zTestMode("ZTest",Float) = 4
		displayColor("Color",Color) = (0,0.88235,0.48235,1)
		displayOutlineColor("Color Outline",Color) = (0,0.0235,0.2509,1)
		displayTexture("Texture", 2D) = "white" {}
		displayExtrude("Extrude",Range(0,0.05)) = 0.012
		displaySize("Size",Range(0,0.05)) = 0.02
		displayRoundness("Roundness",Range(0,0.75)) = 0.5
	}
	SubShader{
		ZWrite Off
		ZTest [zTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		Tags{"RenderType"="Transparent" "Queue"="Transparent+1999"}
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma geometry geometryPass
			#pragma multi_compile QUADMODE_BILLBOARD QUADMODE_SURFACE
			struct vertexInput{
				float4 position : POSITION;
				float4 color    : COLOR;
				float3 normal   : NORMAL;
				#if defined(QUADMODE_SURFACE)
				float4 tangent  : TANGENT;
				#endif
			};
			struct fragmentInput{
				float4 position : POSITION;
				float4 color    : COLOR;
				float2 uv       : TEXCOORD0;
			};
			float displayExtrude;
			float displaySize;
			float displayRoundness;
			float4 displayColor;
			float4 displayOutlineColor;
			sampler2D displayTexture;
			float4 displayTexture_ST;
			static float3 vertexes[4];
			static vertexInput vertex;
			static fragmentInput fragment;
			vertexInput vertexPass(vertexInput input){
				vertexInput output;
				UNITY_INITIALIZE_OUTPUT(vertexInput,output);
				output.color = input.color;
				output.position = input.position + float4(normalize(input.normal),1) * displayExtrude;
				#if defined(QUADMODE_SURFACE)
					output.normal = normalize(input.normal);
					output.tangent = normalize(input.tangent);
				#endif
				return output;
			}
			void AddVertex(inout TriangleStream<fragmentInput> stream,int index,float u,float v){
				fragment.position = UnityObjectToClipPos(vertexes[index]);
				fragment.uv = float2(u,v);
				fragment.color = vertex.color;
				stream.Append(fragment);
			}
			void AddQuad(inout TriangleStream<fragmentInput> stream,vertexInput input){
				vertex = input;
				UNITY_INITIALIZE_OUTPUT(fragmentInput,fragment);
				float4 origin = vertex.position;
				#if defined(QUADMODE_BILLBOARD)
					float3 right = -UNITY_MATRIX_IT_MV[0].xyz;
					float3 up = UNITY_MATRIX_IT_MV[1].xyz;
					float3 xSize = displaySize * right;
					float3 ySize = displaySize * up;
					vertexes[0] = float3(origin + xSize - ySize);
					vertexes[1] = float3(origin + xSize + ySize);
					vertexes[2] = float3(origin - xSize - ySize);
					vertexes[3] = float3(origin - xSize + ySize);
				#elif defined(QUADMODE_SURFACE)
					float3 biNormal = normalize(cross(vertex.normal,vertex.tangent));
					float3 xSize = displaySize * 1.5f;
					float3 ySize = displaySize;
					vertexes[0] = float3(origin + xSize * vertex.tangent - ySize * biNormal);
					vertexes[1] = float3(origin + xSize * vertex.tangent + ySize * biNormal);
					vertexes[2] = float3(origin - xSize * vertex.tangent - ySize * biNormal);
					vertexes[3] = float3(origin - xSize * vertex.tangent + ySize * biNormal);
				#endif
				AddVertex(stream,0,0,1);
				AddVertex(stream,1,0,0);
				AddVertex(stream,2,1,1);
				AddVertex(stream,3,1,0);
				stream.RestartStrip();
			}
			[maxvertexcount(12)]
			void geometryPass(triangle vertexInput input[3],inout TriangleStream<fragmentInput> stream){
				AddQuad(stream,input[1]);
				AddQuad(stream,input[0]);
				AddQuad(stream,input[2]);
			}
			fixed4 pixelPass(fragmentInput input) : SV_Target{
				if(distance(input.color,float4(1,1,1,1)) > 0){
					displayColor = input.color;
					displayOutlineColor = input.color * 0.3f;
					if(input.color.a > 0){displayOutlineColor.a = 1;}
				}
				float centerDistance = distance(input.uv,float2(0.5,0.5));
				fixed shadeBias = 1/displayRoundness * 0.75f;
				fixed4 activeColor = centerDistance*shadeBias > 0.7f ? displayOutlineColor : displayColor;
				fixed4 color = tex2D(displayTexture,TRANSFORM_TEX(input.uv, displayTexture));
				if(centerDistance > displayRoundness){clip(-1);}
				return color * activeColor;
			}
			ENDCG
		}
	}
}

Shader "Zios/Mappings/Triplanar Map"{
	Properties{
		blendingFactor("Blending",Range(0.0,1.0)) = 0.70
	}
	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed blendingFactor;
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 normal        : TEXCOORD1;
				float4 original      : TEXCOORD3;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,0);
				return output;
			}
			float4 setupTriplanarMap(sampler2D triplanar,float4 offset,vertexOutput input){
				float3 weights = abs(input.normal);
				float4 position = input.original;
				weights = (weights - blendingFactor); 
				weights = max(weights,0);
				weights /= (weights.x+weights.y+weights.z).xxx;
				fixed4 color1 = tex2D(triplanar,position.yz * 0.01 * offset.xy + offset.zw);
				fixed4 color2 = tex2D(triplanar,position.zx * 0.01 * offset.xy + offset.zw);
				fixed4 color3 = tex2D(triplanar,position.xy * 0.01 * offset.xy + offset.zw);
				fixed4 color = color1.xyzw * weights.xxxx + color2.xyzw * weights.yyyy + color3.xyzw * weights.zzzz;
				return color;
			}
			pixelOutput applyTriplanarDiffuseMap(vertexOutput input,pixelOutput output){
				output.color = setupTriplanarMap(diffuseMap,diffuseMap_ST,input);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyTriplanarDiffuseMap(input,output);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				return output;
			}
			ENDCG
		}
	}
}

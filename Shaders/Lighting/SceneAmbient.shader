Shader "Zios/(Components)/Lighting/Scene Ambient"{
	Properties{
		sceneAmbient("Scene Ambient",Range(0,1.0)) = 1.0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			float4 globalAmbientColor;
			fixed shadingIndex;
			float time;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = float4(0,0,0,0);
				return output;
			}
			pixelOutput applySceneAmbient(vertexOutput input,pixelOutput output,float value){
				fixed4 lookup = globalAmbientColor;
				if(shadingIndex > 0 && shadingIndex < 0.26){lookup.rgb *= 0.5;}
				output.color.rgb = lerp(output.color.rgb,lookup.rgb,lookup.a);
				return output;
			}
			pixelOutput applySceneAmbient(vertexOutput input,pixelOutput output){
				return applySceneAmbient(input,output,time);
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = UnityObjectToClipPos(input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applySceneAmbient(input,output);
				return output;
			}
			ENDCG
		}
	}
}

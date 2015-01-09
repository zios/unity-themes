Shader "Zios/Standalone/Glorius"{
	Properties{
		ambientMap("Ambient Map",2D) = "white"{}
		ambientMapIntensity("Ambient Map Intensity",Range(0.0,1.0)) = 1.0
		ambientMapPower("Ambient Map Power",Range(1.0,4.0)) = 1.0
		specularSize("Specular Size",Range(0.0,15)) = 0.00
		specularHardness("Specular Hardness",Range(0.01,1)) = 0.01
		specularColor("Specular Color",Color) = (1,1,1,1)
		rimSpread("Rim Spread",Range(1.0,2.0)) = 1.0
		rimSoftness("Rim Softness",Range(0.0,20.0)) = 5.0
		rimColor("Rim Color",Color) = (1,1,1,1)
		shadingMap("Shading Map",2D) = "white"{}
		shadingMapFalloff("Shading Map Falloff",Range(0.0,2.0)) = 1.0
		shadingMapIntensity("Shading Map Intensity",Range(0.0,1.0)) = 1.0
		shadingBand("Shading Band",2D) = "white"{}
	}
	SubShader{
		Tags{"LightMode"="ForwardBase" "Queue"="Geometry-1"}
		Usepass "Hidden/Zios/Shadow Pass/Normal Index Map/SHADOWCOLLECTOR"
		Pass{
			AlphaTest Greater 0
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			sampler2D shadingMap;
			sampler2D shadingBand;
			sampler2D ambientMap;
			fixed4 specularColor;
			fixed specularSize;
			fixed specularHardness;
			fixed4 rimColor;
			fixed rimSpread;
			fixed rimSoftness;
			fixed4 ambientMap_ST;
			fixed ambientMapIntensity;
			fixed ambientMapPower;
			fixed shadingMapFalloff;
			fixed shadingMapIntensity;
			fixed4 shadingMap_ST;
			fixed4 shadingBand_ST;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float3 view	         : TEXCOORD2;
				float  lighting      : TEXCOORD3;
				float3 worldNormal   : TEXCOORD4;
				float3 worldPosition : TEXCOORD5;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = float4(0,0,0,1);
				return output;
			}
			vertexOutput setupInput(vertexOutput input){
				//input.normal.xyz = normalize(input.normal.xyz);
				//input.lightNormal = normalize(input.lightNormal);
				input.view = normalize(input.view);
				return input;
			}
			vertexOutput setupLighting(vertexOutput input){
				input.lighting = saturate(dot(input.normal.xyz,input.lightNormal));
				return input;
			}
			float4 setupTriplanarMap(sampler2D triplanar,float4 offset,vertexOutput input){
				float4 color1 = tex2D(triplanar,input.worldPosition.xy * 0.01 * offset.xy + offset.zw);
				float4 color2 = tex2D(triplanar,input.worldPosition.zx * 0.01 * offset.xy + offset.zw);
				float4 color3 = tex2D(triplanar,input.worldPosition.zy * 0.01 * offset.xy + offset.zw);
				input.worldNormal = normalize(input.worldNormal);
				float3 projectedNormal = saturate(pow(input.worldNormal*1.5,4));
				float3 color = lerp(color2,color1,projectedNormal.z);
				color = lerp(color,color3,projectedNormal.x);
				return float4(color,1.0);
			}
			vertexOutput setupShadingMap(vertexOutput input){
				fixed color = setupTriplanarMap(shadingMap,shadingMap_ST,input);
				fixed destination = input.lighting * color * input.lighting;
				input.lighting = lerp(input.lighting,destination,shadingMapIntensity);
				//input.lighting *= tex2D(shadingMap,TRANSFORM_TEX(input.UV.xy,shadingMap)) * (input.lighting);
				return input;
			}
			pixelOutput applyAmbientMap(vertexOutput input,pixelOutput output){
				float3 destination = output.color.rgb * tex2D(ambientMap,TRANSFORM_TEX(input.UV.xy,ambientMap));
				output.color.rgb = lerp(output.color.rgb,destination*ambientMapPower,ambientMapIntensity);
				return output;
			}
			pixelOutput applyDividedShadingMap(vertexOutput input,pixelOutput output){
				float3 destination = output.color.rgb / setupTriplanarMap(shadingMap,shadingMap_ST,input);
				output.color.rgb = lerp(output.color.rgb,destination,shadingMapIntensity);
				return output;
			}
			pixelOutput applyBlendedShadingMap(vertexOutput input,pixelOutput output){
				float3 destination = output.color.rgb * setupTriplanarMap(shadingMap,shadingMap_ST,input);
				output.color.rgb = lerp(output.color.rgb,destination,shadingMapIntensity);
				return output;
			}
			pixelOutput applyAdditiveShadingMap(vertexOutput input,pixelOutput output){
				fixed lightFactor = saturate((1-input.lighting/shadingMapFalloff));
				float3 destination = output.color.rgb + (setupTriplanarMap(shadingMap,shadingMap_ST,input) * lightFactor);
				output.color.rgb = lerp(output.color.rgb,destination,shadingMapIntensity);
				return output;
			}
			pixelOutput applyShadingBand(vertexOutput input,pixelOutput output){
				float2 shading = float2(input.lighting,0);
				fixed4 lookup = tex2D(shadingBand,shading);
				output.color.rgb += lookup.rgb * lookup.a;
				output.color.a = lookup.a;
				return output;
			}
			pixelOutput applySpecularFull(vertexOutput input,pixelOutput output){
				float3 reflect = normalize(2*input.lighting*input.normal-input.lightNormal.xyz);
				float intensity = pow(saturate(dot(reflect,input.view)),10/specularSize);
				output.color.rgb += specularColor * intensity;
				return output;
			}
			pixelOutput applySpecular(vertexOutput input,pixelOutput output){
				float3 reflect = normalize(input.lightNormal + input.view);
				float intensity = pow(saturate(dot(input.normal,reflect)),10/specularSize);
				intensity = floor((intensity / specularHardness)+0.5) * specularHardness;
				output.color.rgb += specularColor * intensity;
				return output;
			}
			pixelOutput applyFixedRim(vertexOutput input,pixelOutput output){
				rimColor = ((rimColor - 0.5) * 2) * rimColor.a;
				half rimPower = rimSpread - max(dot(input.normal,input.view),0.01);
				half rimPotency = pow(rimPower,25/rimSoftness);
				output.color.rgb += rimPotency * rimColor;
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.view = ObjSpaceViewDir(input.vertex);
				output.normal = float4(input.normal,0);
				output.worldNormal = mul(_Object2World,float4(input.normal,0.0f)).xyz;
				output.worldPosition = mul(_Object2World,input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupInput(input);
				input = setupLighting(input);
				//input = setupShadingMap(input);
				output = applyShadingBand(input,output);
				//output = applyDividedShadingMap(input,output);
				//output = applyBlendedShadingMap(input,output);
				output = applyAdditiveShadingMap(input,output);
				output = applyFixedRim(input,output);
				output = applyAmbientMap(input,output);
				output = applySpecular(input,output);
				return output;
			}
			ENDCG
		}
	}
	Fallback "Hidden/Zios/Fallback/Vertex Lit"
}
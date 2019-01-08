Shader "Zios/Standalone/Megashader UI"{
	Properties{
		[Header(General)]
			baseColor("Color",Color) = (1,1,1,1)
		[Header(Texture)]
			textureMap("Texture",2D) = "white"{}
		[Header(Triplanar)]
			triplanarScale("Scale",Range(0.05,50)) = 1
		[Header(Scroll)]
			[KeywordEnum(Off,On)] scrollState("State",Float) = 0
			scrollX("UV Scroll X",Range(0.0,5)) = 0.05
			scrollY("UV Scroll Y",Range(0.0,5)) = 0.03
		[Header(Warp)]
			[KeywordEnum(None,Offset,Center)] warpType("Type",Float) = 0
			[KeywordEnum(Off,On)] warpDistort("Distort",Float) = 0
			warpFrequency("UV Warp Frequency",Range(0,256)) = 5
			warpPowerX("UV Warp Speed X",Range(-5,5)) = 0.03
			warpPowerY("UV Warp Speed Y",Range(-5,5)) = 0.03
			warpScale("UV Warp Scale",Range(-10,10)) = 0.1
	}
	SubShader{
		Tags{"LightMode"="ForwardBase" "Queue"="Geometry"}
		Pass{
			ZWrite On
			Colormask 0
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma target 3.0
			float cullDistance;
			float4 vertexPass(float4 vertex:POSITION) : SV_POSITION{
				float4 position = UnityObjectToClipPos(vertex);
                return position;
            }
			fixed4 pixelPass() : SV_Target{return fixed4(0,0,0,0);}
			ENDCG
		}
		Pass{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			#define pi = 3.14159265f
			#define doublePi 6.28318548f
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : SV_POSITION;
				float3 worldNormal   : TEXCOORD4;
				float3 worldPosition : TEXCOORD5;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			static vertexOutput input;
			static pixelOutput output;
			float timeConstant;
			fixed ClampRange(fixed min,fixed max,fixed value){return saturate((value-min)/(max-min));}
			//===========================
			// Scroll
			//===========================
			float scrollX;
			float scrollY;
			float2 SetupScroll(float2 uv){
				uv.x -= scrollY * timeConstant;
				uv.y -= scrollX * timeConstant;
				return uv;
			}
			//===========================
			// Warp
			//===========================
			float warpFrequency;
			float warpScale;
			float warpPowerX;
			float warpPowerY;
			float2 SetupWarp(float2 uv){
				float speed = timeConstant*warpFrequency;
				uv.x += (sin(uv.y + speed) * warpPowerX) * warpScale;
				uv.y += (sin(uv.x + speed) * warpPowerY) * warpScale;
				return uv;
			}
			//===========================
			// Texture
			//===========================
			sampler2D textureMap;
			fixed4 textureMap_ST;
			float2 GetUV(float2 uv,float2 scale,float2 offset){
				uv = SetupWarp(uv * scale + offset);
				uv = SetupScroll(uv);
				return uv;
			}
			void BlendTexture(fixed4 color){
				output.color *= color;
			}
			//===========================
			// Triplanar
			//===========================
			fixed xBlending = 1;
			fixed yBlending = 1;
			fixed zBlending = 1;
			fixed triplanarScale;
			float4 SetupTriplanarMap(sampler2D triplanar,float2 scale,float2 offset){
				scale *= 1/triplanarScale;
				offset *= -1;
				float4 color1 = tex2D(triplanar,GetUV(input.worldPosition.xy,scale,offset));
				float4 color2 = tex2D(triplanar,GetUV(input.worldPosition.zx,scale,offset));
				float4 color3 = tex2D(triplanar,GetUV(input.worldPosition.zy,scale,offset));
				float3 projectedNormal = saturate(pow(input.worldNormal*1.5,4));
				if(xBlending != 0){projectedNormal.x = ceil(projectedNormal.x-0.5f);}
				if(yBlending != 0){projectedNormal.y = ceil(projectedNormal.y-0.5f);}
				if(zBlending != 0){projectedNormal.z = ceil(projectedNormal.z-0.5f);}
				float4 color = lerp(color2,color1,projectedNormal.z);
				color = lerp(color,color3,projectedNormal.x);
				return color;
			}
			void ApplyTextureTriplanar(){
				fixed4 lookup = SetupTriplanarMap(textureMap,textureMap_ST.xy,textureMap_ST.zw);
				BlendTexture(lookup);
			}
			//===========================
			// Color
			//===========================
			fixed4 baseColor;
			void ApplyColor(){
				output.color = baseColor;
			}
			//===========================
			// Lighting
			//===========================
			fixed lighting;
			fixed lightingSteps;
			fixed3 pointColor;
			fixed4 pointIntensity;
			fixed pointAverageIntensity;
			fixed directionalIntensity;
			fixed3 directionalColor;
			fixed SetupHalf(fixed intensity){return pow(intensity * 0.5f + 0.5f,2);}
			fixed SetupStepped(fixed intensity,fixed steps){return saturate(ceil((intensity / steps)-0.5) * steps);}
			fixed4 SetupHalf(fixed4 intensity){return pow(intensity * 0.5f + 0.5f,2);}
			fixed4 SetupStepped(fixed4 intensity,fixed steps){return saturate(ceil((intensity / steps)-0.5) * steps);}
			void SetupDirectionalLighting(){}
			void SetupPointLighting(){}
			void SetupLighting(){}
			//===========================
			// Main
			//===========================
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = UnityObjectToClipPos(input.vertex);
				output.worldNormal = UnityObjectToWorldNormal(input.normal);
				output.worldPosition = mul(unity_ObjectToWorld,input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput pixelInput){
				input = pixelInput;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				ApplyColor();
				ApplyTextureTriplanar();
				return output;
			}
			ENDCG
		}
	}
	Fallback "Hidden/Zios/Fallback/Vertex Lit"
}

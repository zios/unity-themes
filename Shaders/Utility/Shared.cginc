//==========================
// Variables
//==========================
sampler2D genericMap;
sampler2D sceneAmbientMap;
sampler2D shadingAtlas;
sampler2D shadingMap;
sampler2D bumpMap;
sampler2D normalMap;
sampler2D diffuseMap;
sampler2D outlineMap;
sampler2D specularMap;
sampler2D indexMap;
sampler2D splatMap;
sampler2D normalIndexMap;
sampler2D blendMapRed;
sampler2D blendMapGreen;
sampler2D blendMapBlue;
sampler2D unity_Lightmap;
sampler2D _MainTex;
fixed4 _MainTex_ST;
fixed4 unity_LightmapST;
fixed4 genericMap_ST;
fixed4 sceneAmbientMap_ST;
fixed4 shadingAtlas_ST;
fixed4 shadingMap_ST;
fixed4 bumpMap_ST;
fixed4 normalMap_ST;
fixed4 diffuseMap_ST;
fixed4 outlineMap_ST;
fixed4 specularMap_ST;
fixed4 indexMap_ST;
fixed4 splatMap_ST;
fixed4 normalIndexMap_ST;
fixed4 blendMapRed_ST;
fixed4 blendMapGreen_ST;
fixed4 blendMapBlue_ST;
fixed4 globalAmbientColor;
fixed4 lookupColorAStart;
fixed4 lookupColorBStart;
fixed4 lookupColorCStart;
fixed4 lookupColorDStart;
fixed4 lookupColorEStart;
fixed4 lookupColorFStart;
fixed4 lookupColorGStart;
fixed4 lookupColorHStart;
fixed4 lookupColorAEnd;
fixed4 lookupColorBEnd;
fixed4 lookupColorCEnd;
fixed4 lookupColorDEnd;
fixed4 lookupColorEEnd;
fixed4 lookupColorFEnd;
fixed4 lookupColorGEnd;
fixed4 lookupColorHEnd;
fixed4 lerpColor;
fixed4 lerpColor2;
fixed4 diffuseColor;
fixed4 shadowColor;
fixed4 specularColor;
fixed4 ambientColor;
fixed4 lightDirection;
fixed4 meshNormal;
fixed4 atlasUV;
fixed4 atlasUVScale;
fixed4 paddingUV;
fixed4 clipUV;
fixed diffuseCutoff;
fixed ambientCutoff;
fixed lerpCutoff;
fixed hue;
fixed hueIntensity;
fixed lightmapIntensity;
fixed alphaCutoff;
fixed alphaCutoffGlobal;
fixed alpha;
fixed intensity;
fixed overlayIntensity;
fixed4 overlayColor;
fixed fresnelSize;
fixed fresnelIntensity;
fixed4 fresnelColor;
fixed4 outlineColor;
fixed outlineDarken;
fixed outlineLength;
fixed outlineShrinkStart;
fixed outlineShrinkEnd; 
fixed outlineSize;
fixed outlineScale;
fixed outlineMapAlias;
fixed outlineMapIntensity;
fixed outlineMapFading;
fixed outlineMapCutoff;
//fixed outlineDistanceScale;
fixed4 rimColor;
fixed rimAlpha;
fixed rimSpread;
fixed rimSoftness;
fixed specularSteps;
fixed specularSize;
fixed specularHardness;
fixed zMin;
fixed zRange;
fixed blendingFactor;
fixed opacity;
fixed normalMapSpread;
fixed normalMapContrast;
fixed bumpMapContrast;
fixed bumpMapSpread;
fixed4 shadingColor;
fixed shadingID;
fixed shadingIgnoreCutoff;
fixed shadingSpread;
fixed shadingContrast;
fixed shadingSteps;
fixed selfShadowSpread;
fixed selfShadowContrast;
fixed desaturateAmount;
fixed sceneAmbient;
float time;
float timeConstant;
fixed UVScrollX;
fixed UVScrollY;
fixed shadingIndex;
fixed shadingStart = 0;
fixed shadingEnd = 1;
//==========================
// Structures
//==========================
struct vertexInput{
	float4 vertex        : POSITION;
	float4 texcoord      : TEXCOORD0;
	float4 texcoord1     : TEXCOORD1;
	float3 normal        : NORMAL;
	float4 tangent       : TANGENT;
	fixed4 color         : COLOR;
};
struct vertexInputTrimmed{
	float4 vertex        : POSITION;
	float4 texcoord      : TEXCOORD0;
	float3 normal        : NORMAL;
};
struct vertexOutput{
	float4 pos           : POSITION;
	float4 UV            : COLOR0;
	float3 lightPosition : COLOR1;
	float3 lightNormal	 : TEXCOORD0;
	float4 normal        : TEXCOORD1;
	float4 tangent       : TEXCOORD2;
    float4 original      : TEXCOORD3;
	float3 view	         : TEXCOORD4;
    float  lighting      : TEXCOORD5;
	LIGHTING_COORDS(6,7)
};
struct pixelOutput{
	fixed4 color         : COLOR0;
};
//==========================
// Vertex Components
//==========================
vertexOutput vertexPass(vertexInput input){
	vertexOutput output;
	UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
	float2 lightmapUV = input.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
	output.lightNormal = ObjSpaceLightDir(input.vertex);
	output.view = ObjSpaceViewDir(input.vertex);
	output.UV = float4(input.texcoord.x,input.texcoord.y,lightmapUV.x,lightmapUV.y);
	output.normal = float4(input.normal,0);
	output.tangent = input.tangent;
	output.original = input.vertex;
	TRANSFER_VERTEX_TO_FRAGMENT(output);
	return output;
}
vertexOutput vertexPassTrimmed(vertexInputTrimmed input){
	vertexOutput output;
	UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
	output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
	output.UV = input.texcoord;
	return output;
}
vertexOutput vertexPassSprite(vertexInput input){
	vertexOutput output;
	UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
	output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
	output.lightNormal = ObjSpaceLightDir(input.vertex);
	output.view = ObjSpaceViewDir(input.vertex);
	output.UV = float4(input.texcoord.x,input.texcoord.y,input.texcoord1.x,input.texcoord1.y);
	output.normal = float4(input.normal,0);
	output.tangent = input.tangent;
	output.original = input.color;
	return output;
}
vertexOutput vertexPassManual(vertexInput input){
	vertexOutput output;
	UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
	input.normal = meshNormal.xyz;
	float2 lightmapUV = input.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
	output.lightNormal = ObjSpaceLightDir(input.vertex);
	output.view = ObjSpaceViewDir(input.vertex);
	output.UV = float4(input.texcoord.x,input.texcoord.y,lightmapUV.x,lightmapUV.y);
	output.normal = float4(input.normal,0);
	output.tangent = input.tangent;
	output.original = input.vertex;
	return output;
}
vertexOutput vertexPassSimple(vertexInput input){
	vertexOutput output;
	UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
	output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
	output.lightNormal = ObjSpaceLightDir(input.vertex);
	output.UV.xy = float2(input.texcoord.x,input.texcoord.y);
	output.normal = float4(input.normal+meshNormal.xyz,0);
	return output;
}
vertexOutput vertexPassOutline(vertexInput input){ 
	vertexOutput output;
	UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
	float3 view = ObjSpaceViewDir(input.vertex);
    float3 outline = input.vertex + input.normal * outlineLength;
	output.pos = mul(UNITY_MATRIX_MVP,float4(outline,1));
	output.UV.xy = float2(input.texcoord.x,input.texcoord.y);
	return output;
}
vertexOutput vertexPassOutlineFull(vertexInput input){ 
	vertexOutput output;
	UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
	float3 view = ObjSpaceViewDir(input.vertex);
    float shrinker = 1 - log(distance(view,input.vertex)/outlineShrinkStart)/log(outlineShrinkEnd/outlineShrinkStart);
    float3 outline = input.vertex + input.normal * outlineLength * shrinker;
	output.pos = mul(UNITY_MATRIX_MVP,float4(outline,1));
	output.UV.xy = float2(input.texcoord.x,input.texcoord.y);
	return output;
}
vertexOutput vertexPassBillboard(vertexInput input){
	vertexOutput output;
	UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
	output.pos = mul(UNITY_MATRIX_P,mul(UNITY_MATRIX_MV, float4(0.0f, 0.0f, 0.0f, 1.0f)) + float4(input.vertex.x, input.vertex.y, 0.0f, 0.0f));
	output.lightNormal = ObjSpaceLightDir(input.vertex);
	output.UV.xy = float2(input.texcoord.x,input.texcoord.y);
	output.normal = float4(input.normal+meshNormal.xyz,0);
	return output;
}
pixelOutput pixelPassOutline(vertexOutput input){
	pixelOutput output;
	UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
	output.color = outlineColor;
	return output;
}
//==========================
// Utility
//==========================
float2 clampRange(float2 min,float2 max,float2 value){return saturate((value-min)/(max-min));}
float2 UnpackFloat2(float value){
	value = value * 10000000;
	return fmod(float2(value/4096.0,value),4096.0) / 4096.0;
}
float3 UnpackFloat3(float value){
	value = value * 10000000;
	return fmod(float3(value/65536.0,value/256.0,value),256.0) / 256.0;
}
float4 UnpackFloat4(float value){
	value = value * 10000000;
	return fmod(float4(value/262144.0,value/4096.0,value/64.0,value),64.0) / 64.0;
}
//==========================
// UV Manipulation
//==========================
vertexOutput setupUVScroll(vertexOutput input,float xScroll,float yScroll,float scale){
	input.UV.x += (xScroll * scale);
	input.UV.y += (yScroll * scale);
	return input;
}
vertexOutput setupUVScroll(vertexOutput input,float xScroll,float yScroll){
	input = setupUVScroll(input,xScroll,yScroll,timeConstant);
	return input;
}
vertexOutput setupUVScroll(vertexOutput input,float scale){
	input = setupUVScroll(input,UVScrollX,UVScrollY,scale);
	return input;
}
vertexOutput setupUVScroll(vertexOutput input){
	input = setupUVScroll(input,UVScrollX,UVScrollY,1);
	return input;
}
vertexOutput setupClipping(vertexOutput input){
	if(input.UV.x < clipUV.x){clip(-1);}
	if(input.UV.x > clipUV.z){clip(-1);}
	if(input.UV.y < 1-clipUV.w){clip(-1);}
	if(input.UV.y > 1-clipUV.y){clip(-1);}
	return input;
}
vertexOutput setupPadding(vertexOutput input){
	input.UV.xy = clampRange(paddingUV.xy,paddingUV.zw,input.UV.xy);
	return input;
}
vertexOutput setupAtlas(vertexOutput input){
	input.UV.xy = lerp(atlasUV.xy,atlasUV.zw,fmod(input.UV.xy*atlasUVScale.xy,1));
	return input;
}
//==========================
// Lighting / Shading
//==========================
pixelOutput setupPixel(vertexOutput input){
	pixelOutput output;
	UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
	output.color = fixed4(0,0,0,0);
	return output;
}
vertexOutput setupInput(vertexOutput input){
	input.normal.xyz = normalize(input.normal.xyz);
	input.lightNormal = normalize(input.lightNormal);
	input.view = normalize(input.view);
	return input;
}
vertexOutput setupSimpleLighting(vertexOutput input){
	input.lighting = 1.0;
	return input;
}
vertexOutput setupLighting(vertexOutput input){
	input.lighting = saturate(dot(input.normal.xyz,input.lightNormal));
	return input;
}
vertexOutput setupLighting(float3 lightDirection,vertexOutput input){
	input.lighting = saturate(dot(lightDirection,input.lightNormal));
	return input;
}
vertexOutput setupCustomLighting(vertexOutput input){
	input.lighting = saturate(dot(input.normal.xyz,input.lightNormal)*(1.0+shadingContrast))+shadingSpread;
	return input;
}
vertexOutput setupCustomLighting(float3 lightDirection,vertexOutput input){
	input.lighting = saturate(dot(input.normal.xyz,lightDirection)*(1.0+shadingContrast))+shadingSpread;
	return input;
}
vertexOutput setupSteppedLighting(vertexOutput input,float shadingSteps){
	input = setupLighting(input);
	float stepSize = shadingSteps;
	input.lighting = ceil((input.lighting / stepSize)-0.5) * stepSize;
	return input;
}
vertexOutput setupSteppedLighting(vertexOutput input){
	return setupSteppedLighting(input,1.0 / (shadingSteps-1));
}
vertexOutput setupComplexSteppedLighting(vertexOutput input){
	input = setupLighting(input);
	float stepSize = 1.0 / (shadingSteps-1);
	input.lighting = ceil((input.lighting / stepSize)-0.5) * stepSize;
	input.lighting = lerp(shadingStart,shadingEnd,input.lighting);
	return input;
}
vertexOutput prepareLODShadingMap(vertexInput input,vertexOutput output){
	float zMax = zRange*zMin;
	float4 cameraPosition = mul(_World2Object,float4(_WorldSpaceCameraPos,1.0));
	float z = distance(cameraPosition,input.vertex);
	output.normal.a = 1 - log(z/zMin)/log(zMax/zMin);
	return output;
}
pixelOutput applyGradientShading2(vertexOutput input,pixelOutput output){
	fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
	if(lookup == 1){output.color = lerp(lookupColorAStart,lookupColorAEnd,input.lighting);}
	if(lookup == 2){output.color = lerp(lookupColorBStart,lookupColorBEnd,input.lighting);}
	return output;
}
pixelOutput applyGradientShading3(vertexOutput input,pixelOutput output){
	fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
	if(lookup == 1){output.color = lerp(lookupColorAStart,lookupColorAEnd,input.lighting);}
	if(lookup == 2){output.color = lerp(lookupColorBStart,lookupColorBEnd,input.lighting);}
	if(lookup == 3){output.color = lerp(lookupColorCStart,lookupColorCEnd,input.lighting);}
	return output;
}
pixelOutput applyGradientShading4(vertexOutput input,pixelOutput output){
	fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
	if(lookup == 1){output.color = lerp(lookupColorAStart,lookupColorAEnd,input.lighting);}
	if(lookup == 2){output.color = lerp(lookupColorBStart,lookupColorBEnd,input.lighting);}
	if(lookup == 3){output.color = lerp(lookupColorCStart,lookupColorCEnd,input.lighting);}
	if(lookup == 4){output.color = lerp(lookupColorDStart,lookupColorDEnd,input.lighting);}
	return output;
}
pixelOutput applyGradientShading8(vertexOutput input,pixelOutput output){
	fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
	if(lookup == 1){output.color = lerp(lookupColorAStart,lookupColorAEnd,input.lighting);}
	if(lookup == 2){output.color = lerp(lookupColorBStart,lookupColorBEnd,input.lighting);}
	if(lookup == 3){output.color = lerp(lookupColorCStart,lookupColorCEnd,input.lighting);}
	if(lookup == 4){output.color = lerp(lookupColorDStart,lookupColorDEnd,input.lighting);}
	if(lookup == 5){output.color = lerp(lookupColorEStart,lookupColorEEnd,input.lighting);}
	if(lookup == 6){output.color = lerp(lookupColorFStart,lookupColorFEnd,input.lighting);}
	if(lookup == 7){output.color = lerp(lookupColorGStart,lookupColorGEnd,input.lighting);}
	if(lookup == 8){output.color = lerp(lookupColorHStart,lookupColorHEnd,input.lighting);}
	return output;
}
pixelOutput applyDiffuseShading(vertexOutput input,pixelOutput output){
	output.color.rgb = input.lighting * (shadingColor.rgb * shadingColor.a);
	output.color.a = 1;
	return output;
}
pixelOutput applyDiffuseAddShading(vertexOutput input,pixelOutput output){
	output.color.rgb += input.lighting * (shadingColor.rgb * shadingColor.a);
	output.color.a = 1;
	return output;
}
pixelOutput applyDiffuseBlendShading(vertexOutput input,pixelOutput output){
	output.color.rgb *= input.lighting * (shadingColor.rgb * shadingColor.a);
	output.color.a = 1;
	return output;
}
pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output,fixed4 shadingColor,float shadingCutoff){
	if(length(output.color.rgb) > shadingCutoff){
		float shadeValue = saturate(input.lighting+(1-shadingColor.a));
		output.color.rgb = lerp(shadingColor.rgb,output.color.rgb,shadeValue);
	}
	return output;
}
pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output){
	return applyDiffuseLerpShading(input,output,shadingColor,shadingIgnoreCutoff);
}
pixelOutput applyLODShadingMap(vertexOutput input,pixelOutput output){
	float2 shading = float2(input.lighting,input.normal.a);
	output.color += tex2D(shadingMap,shading);
	return output;
}
pixelOutput applyShadingAtlas(float shadeRow,vertexOutput input,pixelOutput output){
	float2 shading = float2(input.lighting,shadeRow);
	fixed4 lookup = tex2D(shadingAtlas,shading);
	shadingIndex = shadeRow;
	if(shadeRow == 0){clip(-1);}
	output.color.rgb += lookup.rgb * lookup.a;
	output.color.a = lookup.a;
	return output;
}
pixelOutput applyShadingAtlas(sampler2D indexMap,vertexOutput input,pixelOutput output){
	float shadeRow = 1.0 - tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r;
	output = applyShadingAtlas(shadeRow,input,output);
	return output;
}
pixelOutput applyShadingAtlas(vertexOutput input,pixelOutput output){
	float shadeRow = 1.0 - input.normal.a;
	output = applyShadingAtlas(shadeRow,input,output);
	return output;
}
//==========================
// Shadows
//==========================
pixelOutput applyShadows(vertexOutput input,pixelOutput output){
	output.color.rgb *= LIGHT_ATTENUATION(input) + shadowColor;
	return output;
}
pixelOutput applyLightMap(vertexOutput input,pixelOutput output){
	output.color.rgb *= DecodeLightmap(tex2D(unity_Lightmap,input.UV.zw)) + shadowColor;
	return output;
}
pixelOutput applyShadowLightMap(vertexOutput input,pixelOutput output){
	output.color.rgb *= saturate(DecodeLightmap(tex2D(unity_Lightmap,input.UV.zw))) + shadowColor;
	return output;
}
//==========================
// Color Replace
//==========================
pixelOutput applyColorReplace2(vertexOutput input,pixelOutput output){
	fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
	if(lookup == 1){output.color = lookupColorAStart;}
	if(lookup == 2){output.color = lookupColorBStart;}
	return output;
}
pixelOutput applyColorReplace3(vertexOutput input,pixelOutput output){
	fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
	if(lookup == 1){output.color = lookupColorAStart;}
	if(lookup == 2){output.color = lookupColorBStart;}
	if(lookup == 3){output.color = lookupColorCStart;}
	return output;
}
pixelOutput applyColorReplace4(vertexOutput input,pixelOutput output){
	fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
	if(lookup == 1){output.color = lookupColorAStart;}
	if(lookup == 2){output.color = lookupColorBStart;}
	if(lookup == 3){output.color = lookupColorCStart;}
	if(lookup == 4){output.color = lookupColorDStart;}
	return output;
}
pixelOutput applyColorReplace6(vertexOutput input,pixelOutput output){
	fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
	if(lookup == 1){output.color = lookupColorAStart;}
	if(lookup == 2){output.color = lookupColorBStart;}
	if(lookup == 3){output.color = lookupColorCStart;}
	if(lookup == 4){output.color = lookupColorDStart;}
	if(lookup == 5){output.color = lookupColorEStart;}
	if(lookup == 6){output.color = lookupColorFStart;}
	return output;
}
pixelOutput applyColorReplace8(vertexOutput input,pixelOutput output){
	fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
	if(lookup == 1){output.color = lookupColorAStart;}
	if(lookup == 2){output.color = lookupColorBStart;}
	if(lookup == 3){output.color = lookupColorCStart;}
	if(lookup == 4){output.color = lookupColorDStart;}
	if(lookup == 5){output.color = lookupColorEStart;}
	if(lookup == 6){output.color = lookupColorFStart;}
	if(lookup == 7){output.color = lookupColorGStart;}
	if(lookup == 8){output.color = lookupColorHStart;}
	return output;
}
//==========================
// Bump Mapping
//==========================
vertexOutput setupTangentSpace(vertexOutput input){
	float3 binormal = cross(input.normal.xyz,input.tangent.xyz) * input.tangent.w;
	float3x3 tangentRotate = float3x3(input.tangent.xyz,binormal,input.normal.xyz);
	input.lightNormal = mul(tangentRotate,input.lightNormal);
	return input;
}
vertexOutput setupNormalMap(vertexOutput input){
	input = setupTangentSpace(input);
	fixed4 lookup = tex2D(normalMap,TRANSFORM_TEX(input.UV.xy,normalMap));
	input.normal.xyz = (lookup.rgb*2)-1.0;
	input.normal.w = lookup.a;
	return input;
}
vertexOutput setupBumpMap(vertexOutput input){
	input = setupTangentSpace(input);
	float height = tex2D(bumpMap,TRANSFORM_TEX(input.UV.xy,bumpMap)).r;
	float shade = ((height*2)-1.0)*(1.0+bumpMapContrast)+bumpMapSpread;
	input.normal.xyz = float3(shade,shade,shade);
	return input;
}
vertexOutput setupSelfShadow(vertexOutput input){
	float height = 1.0-tex2D(bumpMap,TRANSFORM_TEX(input.UV.xy,bumpMap)).r;
	float shadow = saturate((input.lighting - height) * (1.8+selfShadowContrast)) + (0.5+selfShadowSpread);
	//shadow = shadow * shadow * (3.0 - 2.0 * shadow);
	input.lighting *= shadow;
	return input;
}
//==========================
// Rim Effects
//==========================
pixelOutput applyFixedRim(vertexOutput input,pixelOutput output){
	rimColor = ((rimColor - 0.5) * 2) * rimColor.a;
	half rimPower = rimSpread - max(dot(input.normal,input.view),0.01);
	float stepSize = 1.0 / (shadingSteps-1);
	float rimPotency = ceil((rimPower / stepSize)-0.5) * stepSize;
	//half rimPotency = pow(rimPower,25/rimSoftness);
	output.color.rgb += rimPotency * rimColor;
	output.color.a -= rimPotency * rimAlpha;
	return output;
}
//==========================
// Alpha/Color Blends
//==========================
pixelOutput applyHue(vertexOutput input,pixelOutput output){
	float3 hueColor;
    hueColor.r = abs(hue * 6 - 3) - 1;
    hueColor.g = 2 - abs(hue * 6 - 2);
    hueColor.b = 2 - abs(hue * 6 - 4);
    //output.color.rgb = fmod(output.color.rgb * saturate(hueColor),1);
	output.color.rgb = lerp(output.color.rgb,saturate(hueColor),hueIntensity);
	return output;
}
pixelOutput applyDesaturate(vertexOutput input,pixelOutput output){
	float grayscale = (output.color.r + output.color.g + output.color.b) / 3;
	float3 grayscaleColor = float3(grayscale,grayscale,grayscale);
	output.color.rgb = lerp(output.color.rgb,grayscaleColor,desaturateAmount);
	return output;
}
pixelOutput applyGrayscale(vertexOutput input,pixelOutput output){
	float grayscale = (output.color.r + output.color.g + output.color.b) / 3;
	output.color.rgb = float3(grayscale,grayscale,grayscale);
	return output;
}
pixelOutput applyAlphaSimple(vertexOutput input,pixelOutput output){
	output.color.a *= alpha;
	return output;
}
pixelOutput applyAlpha(vertexOutput input,pixelOutput output,float alpha){
	output.color.a *= alpha;
	if(alphaCutoff == 0){alphaCutoff = alphaCutoffGlobal;}
	if(output.color.a <= alphaCutoff){clip(-1);}
	return output;
}
pixelOutput applyAlpha(vertexOutput input,pixelOutput output){
	return applyAlpha(input,output,alpha);
}
pixelOutput applyVertexColor(vertexOutput input,pixelOutput output){
	output.color.rgb = input.lightNormal.rgb;
	return output;
}
pixelOutput applyAmbientColor(vertexOutput input,pixelOutput output){
	output.color.rgb += (ambientColor * ambientColor.a);
	return output;
}
pixelOutput applyAmbientColor(vertexOutput input,pixelOutput output,float cutoff){
	if(length(output.color.rgb) >= ambientCutoff){
		output.color.rgb += (ambientColor * ambientColor.a);
	}
	return output;
}
pixelOutput applyDiffuseColor(vertexOutput input,pixelOutput output,float cutoff){
	if(length(output.color.rgb) >= diffuseCutoff){
		output.color.rgb *= (diffuseColor.rgb * diffuseColor.a);
	}
	return output;
}
pixelOutput applyLerpColor(vertexOutput input,pixelOutput output,fixed4 color,fixed cutoff){
	if(length(output.color.rgb) >= cutoff){
		output.color.rgb = lerp(output.color.rgb,color.rgb,color.a);
	}
	return output;
}
pixelOutput applyLerpColor(vertexOutput input,pixelOutput output){
	return applyLerpColor(input,output,lerpColor,lerpCutoff);
}
pixelOutput applyIntensity(vertexOutput input,pixelOutput output,float intensity){
	output.color.rgb *= intensity;
	if(alphaCutoff == 0){alphaCutoff = alphaCutoffGlobal;}
	if(output.color.a <= alphaCutoff){clip(-1);}
	return output;
}
pixelOutput applyIntensity(vertexOutput input,pixelOutput output){
	return applyIntensity(input,output,intensity);
}
//==========================
// Diffuse Map
//==========================
pixelOutput applyDiffuseMap(vertexOutput input,pixelOutput output){
	output.color += tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap));
	return output;
}
pixelOutput additiveDiffuseMap(vertexOutput input,pixelOutput output){
	output.color.rgb += tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap)).rgb;
	output.color.a = output.color.r;
	return output;
}
pixelOutput divideDiffuseMap(vertexOutput input,pixelOutput output){
	output.color /= tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap));
	return output;
}
pixelOutput applyDrawTexture(vertexOutput input,pixelOutput output){
	output.color += tex2D(_MainTex,TRANSFORM_TEX(input.UV.xy,_MainTex));
	return output;
}
pixelOutput applyOutlineMap(vertexOutput input,pixelOutput output){
	float4 lookup = tex2D(outlineMap,TRANSFORM_TEX(input.UV.xy,outlineMap));
	output.color.rgb = lerp(output.color.rgb,0,lookup.a);
	return output;
}
pixelOutput applySpecular(vertexOutput input,pixelOutput output){
	if(length(output.color.rgb) > shadingIgnoreCutoff){
		float3 reflect = normalize(input.lightNormal + input.view);
		float intensity = pow(saturate(dot(input.normal,reflect)),10/specularSize);
		intensity = floor((intensity / specularHardness)+0.5) * specularHardness;
		output.color.rgb += specularColor * intensity;
	}
	return output;
}
pixelOutput applySpecularMap(vertexOutput input,pixelOutput output){
	float mapIntensity = tex2D(specularMap,TRANSFORM_TEX(input.UV.xy,specularMap));
	if(length(output.color.rgb) > shadingIgnoreCutoff && mapIntensity > 0){
		float3 reflect = normalize(input.lightNormal + input.view);
		float intensity = pow(saturate(dot(input.normal,reflect)),10/specularSize);
		float stepSize = 1.0 / (specularSteps-1);
		intensity = ceil((intensity / stepSize)-0.5) * stepSize;
		output.color.rgb += specularColor * intensity * mapIntensity;
	}
	return output;
}
pixelOutput applySpecularFull(vertexOutput input,pixelOutput output){
	float3 reflect = normalize(2*input.lighting*input.normal-input.lightNormal.xyz);
	float intensity = pow(saturate(dot(reflect,input.view)),10/specularSize);
	output.color.rgb += specularColor * intensity;
	return output;
}
pixelOutput applyFresnel(vertexOutput input,pixelOutput output){
	float4 specularDot = dot(input.lightNormal,input.view);
	float3 fresnelDot = saturate(dot(input.view,input.normal));
	float fresnelStrength = saturate(1 - fresnelDot) * 10/fresnelIntensity + 1;
	float fresnelLight = fresnelDot * 10/fresnelSize + 1;
	output.color.rgb += pow(saturate(specularDot),fresnelLight) * fresnelStrength * fresnelColor;
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
vertexOutput setupTriplanarNormalMap(vertexOutput input){
	input = setupTangentSpace(input);
	fixed4 lookup = setupTriplanarMap(normalMap,normalMap_ST,input);
	input.normal.xyz = ((lookup.rgb*2)-1.0)*(1.0+normalMapContrast)+normalMapSpread;
	input.normal.w = lookup.a;
	return input;
}
pixelOutput applyTriplanarDiffuseMap(vertexOutput input,pixelOutput output){
	output.color = setupTriplanarMap(diffuseMap,diffuseMap_ST,input);
	return output;
}
pixelOutput applyTriplanarIndexMap(vertexOutput input,pixelOutput output){
	float shadeRow = 1.0 - setupTriplanarMap(indexMap,indexMap_ST,input).r;
	output = applyShadingAtlas(shadeRow,input,output);
	return output;
}
pixelOutput applySplatMap(vertexOutput input,pixelOutput output){
	fixed4 splat = tex2D(splatMap,TRANSFORM_TEX(input.UV.xy,splatMap));
	output.color += tex2D(blendMapRed,TRANSFORM_TEX(input.UV.xy,blendMapRed)) * splat.r;
	output.color += tex2D(blendMapGreen,TRANSFORM_TEX(input.UV.xy,blendMapGreen)) * splat.g;
	output.color += tex2D(blendMapBlue,TRANSFORM_TEX(input.UV.xy,blendMapBlue)) * splat.b;
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
pixelOutput applySceneAmbientMap(vertexOutput input,pixelOutput output,float value){
	float2 UV = float2(input.lighting,value);
	fixed4 lookup = tex2D(sceneAmbientMap,TRANSFORM_TEX(UV,sceneAmbientMap));
	if(shadingIndex > 0 && shadingIndex < 0.26){lookup.rgb *= 0.5;}
	output.color.rgb = lerp(output.color.rgb,lookup.rgb,lookup.a);
	return output;
}
pixelOutput applySceneAmbientMap(vertexOutput input,pixelOutput output){
	return applySceneAmbient(input,output,time);
}
pixelOutput applyTexture(sampler2D genericMap,vertexOutput input,pixelOutput output){
	output.color = tex2D(genericMap,TRANSFORM_TEX(input.UV.xy,genericMap));
	return output;
}
pixelOutput showTextureAlpha(sampler2D map,vertexOutput input,pixelOutput output){
	output.color = tex2D(map,input.UV.xy).a;
	return output;
}
pixelOutput applyOverlayColor(vertexOutput input,pixelOutput output){
	output.color = lerp(output.color,overlayColor,overlayIntensity);
	return output;
}
#define BlendLinearDodgef             BlendAddf
#define BlendLinearBurnf              BlendSubstractf
#define BlendAddf(base,blend)         min(base + blend,1.0)
#define BlendSubstractf(base,blend)   max(base + blend - 1.0,0.0)
#define BlendLightenf(base,blend)     max(blend,base)
#define BlendDarkenf(base,blend)      min(blend,base)
#define BlendLinearLightf(base,blend) (blend < 0.5 ? BlendLinearBurnf(base,(2.0 * blend)) : BlendLinearDodgef(base,(2.0 * (blend - 0.5))))
#define BlendScreenf(base,blend)      (1.0 - ((1.0 - base) * (1.0 - blend)))
#define BlendOverlayf(base,blend)     (base < 0.5 ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend)))
#define BlendSoftLightf(base,blend)   ((blend < 0.5) ? (2.0 * base * blend + base * base * (1.0 - 2.0 * blend)) : (sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend)))
#define BlendColorDodgef(base,blend)  ((blend == 1.0) ? blend : min(base / (1.0 - blend),1.0))
#define BlendColorBurnf(base,blend)   ((blend == 0.0) ? blend : max((1.0 - ((1.0 - base) / blend)),0.0))
#define BlendVividLightf(base,blend)  ((blend < 0.5) ? BlendColorBurnf(base,(2.0 * blend)) : BlendColorDodgef(base,(2.0 * (blend - 0.5))))
#define BlendPinLightf(base,blend)    ((blend < 0.5) ? BlendDarkenf(base,(2.0 * blend)) : BlendLightenf(base,(2.0 *(blend - 0.5))))
#define BlendHardMixf(base,blend)     ((BlendVividLightf(base,blend) < 0.5) ? 0.0 : 1.0)
#define BlendReflectf(base,blend)     ((blend == 1.0) ? blend : min(base * base / (1.0 - blend),1.0))

#define Blend(base,blend,funcf)       float3(funcf(base.r,blend.r),funcf(base.g,blend.g),funcf(base.b,blend.b))
#define BlendMultiply(base,blend)     (base * blend)
#define BlendAverage(base,blend)      (base + blend) / 2.0)
#define BlendAdd(base,blend)          min(base + blend,float3(1.0,1.0,1.0))
#define BlendSubstract(base,blend)    max(base + blend - float3(1.0,1.0,1.0),float3(0.0,0.0,0.0))
#define BlendDifference(base,blend)   abs(base - blend)
#define BlendNegation(base,blend)     (float3(1.0,1.0,1.0) - abs(float3(1.0,1.0,1.0) - base - blend))
#define BlendExclusion(base,blend)    (base + blend - 2.0 * base * blend)
#define BlendLinearDodge              BlendAdd
#define BlendLinearBurn               BlendSubstract

#define BlendGlow(base,blend)         BlendReflect(blend,base)
#define BlendPhoenix(base,blend)      (min(base,blend) - max(base,blend) + float3(1.0,1.0,1.0))
#define BlendOpacity(base,blend,F,O)  (F(base,blend) * O + blend * (1.0 - O))
using Harmony12;
using NovaEnv;
using Pathea;
using Pathea.WeatherNs;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace PostProcessing
{
    static partial class Main
    {


        private static bool defaultPostProcessingSet = false;
        private static VignetteModel.Settings defaultVignetteSettings;
        private static BloomModel.Settings defaultBloomSettings;
        private static EyeAdaptationModel.Settings defaultEyeAdaptSettings;
        private static MotionBlurModel.Settings defaultMotionBlurSettings;
        private static DepthOfFieldModel.Settings defaultDepthOfFieldSettings;
        private static ColorGradingModel.Settings defaultColorGradingSettings;
        private static AmbientOcclusionModel.Settings defaultAOSettings;

        private static bool defaultAASet = false;
        private static AntialiasingModel.Settings defaultAATaaSettings;
        private static AntialiasingModel.Settings defaultAAFxaaSettings;

        [HarmonyPatch(typeof(PostProcessingBehaviour), "OnEnable")]
        static class PostProcessingBehaviour_OnEnable_Patch
        {

            static void Prefix(PostProcessingBehaviour __instance)
            {
                if(__instance.profile != null && !defaultPostProcessingSet)
                {
                    defaultVignetteSettings = __instance.profile.vignette.settings;
                    defaultBloomSettings = __instance.profile.bloom.settings;
                    defaultEyeAdaptSettings = __instance.profile.eyeAdaptation.settings;
                    defaultMotionBlurSettings = __instance.profile.motionBlur.settings;
                    defaultDepthOfFieldSettings = __instance.profile.depthOfField.settings;
                    defaultColorGradingSettings = __instance.profile.colorGrading.settings;
                    defaultAOSettings = __instance.profile.ambientOcclusion.settings;
                    defaultPostProcessingSet = true;
                }
            }
        }

        [HarmonyPatch(typeof(PostProcessingBehaviour), "OnPreCull")]
        static class PostProcessingBehaviour_OnPreCull_Patch
        {
            static void Postfix(ref VignetteComponent ___m_Vignette, ref BloomComponent ___m_Bloom, ref EyeAdaptationComponent ___m_EyeAdaptation, ref DepthOfFieldComponent ___m_DepthOfField, ref MotionBlurComponent ___m_MotionBlur, ref ColorGradingComponent ___m_ColorGrading, ref TaaComponent ___m_Taa, ref FxaaComponent ___m_Fxaa, ref AmbientOcclusionComponent ___m_AmbientOcclusion)
            {

                if (enabled && settings.customVignette)
                {

                    VignetteModel.Settings vSettings = new VignetteModel.Settings
                    {
                        mode = VignetteModel.Mode.Classic,
                        intensity = settings.vignetteIntensity,
                        color = new Color(settings.vignetteColorRed, settings.vignetteColorGreen, settings.vignetteColorBlue, settings.vignetteColorAlpha),
                        center = new Vector2(settings.vignetteX,settings.vignetteY),
                        smoothness = settings.vignetteSmoothness,
                        roundness = settings.vignetteRoundness,
                        rounded = settings.vignetteRounded
                    };
                    ___m_Vignette.model.settings = vSettings;
                }
                else
                {
                    ___m_Vignette.model.settings = defaultVignetteSettings;
                }
                
                if (enabled && settings.customBloom)
                {
                    BloomModel.BloomSettings bbSettings = new BloomModel.BloomSettings
                    {
                        intensity = settings.bloomIntensity,
                        threshold = settings.bloomThreshold,
                        softKnee = settings.bloomSoftKnee,
                        radius = settings.bloomRadius,
                        antiFlicker = settings.bloomAntiFlicker
                    };

                    BloomModel.LensDirtSettings blSettings = new BloomModel.LensDirtSettings
                    {
                        texture = ___m_Bloom.model.settings.lensDirt.texture,
                        intensity = settings.bloomLensDirtIntensity
                    };
                    BloomModel.Settings bSettings = new BloomModel.Settings
                    {
                        bloom = bbSettings,
                        lensDirt = blSettings
                    };

                    ___m_Bloom.model.settings = bSettings;
                }
                else
                {
                    ___m_Bloom.model.settings = defaultBloomSettings;
                }
                
                if (enabled && settings.customEyeAdapt)
                {
                    EyeAdaptationModel.Settings eSettings = new EyeAdaptationModel.Settings
                    {
                        lowPercent = settings.eyeAdaptLowPercent,
                        highPercent = settings.eyeAdaptHighPercent,
                        minLuminance = settings.eyeAdaptMinLuminance,
                        maxLuminance = settings.eyeAdaptMaxLuminance,
                        keyValue = settings.eyeAdaptKeyValue,
                        dynamicKeyValue = settings.eyeAdaptDynamicKeyValue,
                        adaptationType = settings.eyeAdaptAdaptationFixed?EyeAdaptationModel.EyeAdaptationType.Fixed: EyeAdaptationModel.EyeAdaptationType.Progressive,
                        speedUp = settings.eyeAdaptSpeedUp,
                        speedDown = settings.eyeAdaptSpeedDown,
                        logMin = settings.eyeAdaptLogMin,
                        logMax = settings.eyeAdaptLogMax,
                    };

                    ___m_EyeAdaptation.model.settings = eSettings;
                }
                else
                {
                    ___m_EyeAdaptation.model.settings = defaultEyeAdaptSettings;
                }

                if (enabled && settings.customMotionBlur)
                {
                    MotionBlurModel.Settings mSettings = new MotionBlurModel.Settings
                    {
                        shutterAngle = settings.motionBlurShutterAngle,
                        sampleCount = settings.motionBlurSampleCount,
                        frameBlending = settings.motionBlurFrameBlending
                    };

                    ___m_MotionBlur.model.settings = mSettings;
                }
                else
                {
                    ___m_MotionBlur.model.settings = defaultMotionBlurSettings;
                }

                if (enabled && settings.customDepthOfField)
                {
                    DepthOfFieldModel.Settings dSettings = new DepthOfFieldModel.Settings
                    {
                        focusDistance = settings.depthOfFieldFocusDistance,
                        aperture = settings.depthOfFieldAperture,
                        focalLength = settings.depthOfFieldFocalLength,
                        useCameraFov = settings.depthOfFieldUseCameraFov,
                        kernelSize = (DepthOfFieldModel.KernelSize)settings.depthOfFieldKernelSize,
                    };

                    ___m_DepthOfField.model.settings = dSettings;
                    ___m_DepthOfField.model.enabled = true;
                }
                else
                {
                    ___m_DepthOfField.model.settings = defaultDepthOfFieldSettings;
                    ___m_DepthOfField.model.enabled = false;
                }

                if (enabled && settings.customColorGrading)
                {
                    ColorGradingModel.TonemappingSettings ctSettings = new ColorGradingModel.TonemappingSettings
                    {
                        tonemapper = (ColorGradingModel.Tonemapper)settings.colorGradingTonemapper,
                        neutralBlackIn = settings.colorGradingNeutralBlackIn,
                        neutralWhiteIn = settings.colorGradingNeutralWhiteIn,
                        neutralBlackOut = settings.colorGradingNeutralBlackOut,
                        neutralWhiteOut = settings.colorGradingNeutralWhiteOut,
                        neutralWhiteLevel = settings.colorGradingNeutralWhiteLevel,
                        neutralWhiteClip = settings.colorGradingNeutralWhiteClip
                    };

                    ColorGradingModel.BasicSettings cbSettings = new ColorGradingModel.BasicSettings
                    {
                        postExposure = settings.colorGradingPostExposure,
                        temperature = settings.colorGradingTemperature,
                        tint = settings.colorGradingTint,
                        hueShift = settings.colorGradingHueShift,
                        saturation = settings.colorGradingSaturation,
                        contrast = settings.colorGradingContrast
                    };

                    ColorGradingModel.Settings cSettings = new ColorGradingModel.Settings
                    {
                        tonemapping = ctSettings,
                        basic = cbSettings,
                        channelMixer = ___m_ColorGrading.model.settings.channelMixer,
                        colorWheels = ___m_ColorGrading.model.settings.colorWheels,
                        curves = ___m_ColorGrading.model.settings.curves
                    };

                    ___m_ColorGrading.model.settings = cSettings;
                }
                else
                {
                    ___m_ColorGrading.model.settings = defaultColorGradingSettings;
                }

                if (enabled && settings.customAO)
                {
                    AmbientOcclusionModel.Settings aSettings = new AmbientOcclusionModel.Settings
                    {
                        intensity = settings.AOIntensity,
                        radius = settings.AORadius,
                        sampleCount = (AmbientOcclusionModel.SampleCount)sampleCounts.Values.ToArray()[settings.AOSampleCount],
                        downsampling = settings.AODownsampling,
                        forceForwardCompatibility = settings.AOForceForwardCompatibility,
                        ambientOnly = settings.AOAmbientOnly,
                        highPrecision = settings.AOHighPrecision
                    };

                    ___m_AmbientOcclusion.model.settings = aSettings;
                }
                else
                {
                    ___m_AmbientOcclusion.model.settings = defaultAOSettings;
                }

                /*

                if(!defaultAASet)
                {
                    defaultAATaaSettings = ___m_Taa.model.settings;
                    defaultAAFxaaSettings = ___m_Fxaa.model.settings;
                    defaultAASet = true;
                }

                if (enabled && settings.customAA)
                {
                    AntialiasingModel.FxaaSettings afSettings = new AntialiasingModel.FxaaSettings
                    {
                        preset = (AntialiasingModel.FxaaPreset) settings.AAFxaaPreset
                    };

                    AntialiasingModel.TaaSettings atSettings = new AntialiasingModel.TaaSettings
                    {
                        jitterSpread = settings.AAJitterSpread,
                        sharpen = settings.AASharpen,
                        stationaryBlending = settings.AAStationaryBlending,
                        motionBlending = settings.AAMotionBlending
                        
                    };

                    AntialiasingModel.Settings aSettings = new AntialiasingModel.Settings
                    {
                        //method = settings.AAMethodTaa?AntialiasingModel.Method.Taa: AntialiasingModel.Method.Fxaa,
                        method = ___m_Taa.model.settings.method,
                        fxaaSettings = afSettings,
                        taaSettings = atSettings
                    };

                    AntialiasingModel.Settings aSettings2 = new AntialiasingModel.Settings
                    {
                        //method = settings.AAMethodTaa?AntialiasingModel.Method.Taa: AntialiasingModel.Method.Fxaa,
                        method = ___m_Fxaa.model.settings.method,
                        fxaaSettings = afSettings,
                        taaSettings = atSettings
                    };

                    ___m_Taa.model.settings = aSettings;
                    ___m_Fxaa.model.settings = aSettings2;
                }
                else
                {
                    ___m_Taa.model.settings = defaultAATaaSettings;
                    ___m_Fxaa.model.settings = defaultAAFxaaSettings;
                }
                */
            }
        }
    }
}

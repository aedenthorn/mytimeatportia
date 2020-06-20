using Harmony12;
using Hont.ExMethod.Collection;
using NovaEnv;
using Pathea.WeatherNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace PostProcessing
{
    public static partial class Main
    {
        private static Settings settings;
        private static bool enabled;
        private static bool isDebug = false;
        private static float labelWidth = 80f;
        private static float indentSpace = 30f;
        private static string[] toneMappers = new string[] {
            "None",
            "ACES",
            "Neutral"
        };

        private static string[] kernelSizes = new string[] {
            "Small",
            "Medium",
            "Large",
            "Very Large"
        };

        private static string[] fxaaPresets = new string[] {
            "Extreme Performance",
			"Performance",
			"Default",
			"Quality",
			"Extreme Quality"
        };

        private static Dictionary<string,int> sampleCounts = new Dictionary<string, int> {
            { "Lowest",3 },
            { "Low",6 },
            { "Medium",10 },
            { "High",16 },
        };

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                UnityEngine.Debug.Log((pref ? "Post Processing " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            modEntry.OnUpdate = OnUpdate;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (Input.GetKeyDown(settings.ToggleKey))
            {
                modEntry.OnToggle(modEntry, !enabled);
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Toggle Hotkey:"), new GUILayoutOption[] { GUILayout.Width(labelWidth * 2) });
            settings.ToggleKey = GUILayout.TextField(settings.ToggleKey, new GUILayoutOption[] { GUILayout.Width(labelWidth * 2) });
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

                GUILayout.BeginHorizontal();
                GUILayout.Space(indentSpace);
                GUILayout.BeginVertical();
                settings.customVignette = GUILayout.Toggle(settings.customVignette, $"<b>Use Custom Vignette</b>", new GUILayoutOption[0]);
                if (settings.customVignette)
                {
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentSpace);
                    GUILayout.BeginVertical();

                    GUILayout.Label("<b>Color</b>", new GUILayoutOption[0]);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentSpace);
                    GUILayout.BeginVertical();

                    GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ", settings.vignetteColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    settings.vignetteColorRed = GUILayout.HorizontalSlider((float)settings.vignetteColorRed * 100f, 0, 100f) / 100f;
                    GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ", settings.vignetteColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    settings.vignetteColorGreen = GUILayout.HorizontalSlider((float)settings.vignetteColorGreen * 100f, 0, 100f) / 100f;
                    GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ", settings.vignetteColorBlue), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    settings.vignetteColorBlue = GUILayout.HorizontalSlider((float)settings.vignetteColorBlue * 100f, 0, 100f) / 100f;
                    GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ", settings.vignetteColorAlpha), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    settings.vignetteColorAlpha = GUILayout.HorizontalSlider((float)settings.vignetteColorAlpha * 100f, 0, 100f) / 100f;
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(10f);


                    GUILayout.Label("<b>Center</b>", new GUILayoutOption[0]);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentSpace);
                    GUILayout.BeginVertical();

                    GUILayout.Label(string.Format("X: <b>{0:F2}</b> ", settings.vignetteX), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    settings.vignetteX = GUILayout.HorizontalSlider((float)settings.vignetteX * 100f, 0, 100f) / 100f;
                    GUILayout.Label(string.Format("Y: <b>{0:F2}</b> ", settings.vignetteY), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    settings.vignetteY = GUILayout.HorizontalSlider((float)settings.vignetteY * 100f, 0, 100f) / 100f;
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();


                    GUILayout.Space(10f);

                    GUILayout.Label(string.Format("Intensity <b>{0:F2}</b> ", settings.vignetteIntensity), new GUILayoutOption[0]);
                    settings.vignetteIntensity = GUILayout.HorizontalSlider((float)settings.vignetteIntensity * 100f, 0, 100f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(string.Format("Smoothness <b>{0:F2}</b> ", settings.vignetteSmoothness), new GUILayoutOption[0]);
                    settings.vignetteSmoothness = GUILayout.HorizontalSlider((float)settings.vignetteSmoothness * 100f, 1, 100f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(string.Format("Roundness <b>{0:F2}</b> ", settings.vignetteRoundness), new GUILayoutOption[0]);
                    settings.vignetteRoundness = GUILayout.HorizontalSlider((float)settings.vignetteRoundness * 100f, 0, 100f) / 100f;

                    GUILayout.Space(10f);

                    settings.vignetteRounded = GUILayout.Toggle(settings.vignetteRounded, "<b>Force Round Vignette</b>", new GUILayoutOption[0]);

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }
                GUILayout.Space(10f);
                settings.customBloom = GUILayout.Toggle(settings.customBloom, "<b>Use Custom Bloom</b>", new GUILayoutOption[0]);
                if (settings.customBloom)
                {
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentSpace);
                    GUILayout.BeginVertical();

                    GUILayout.Label(new GUIContent(string.Format("Intensity <b>{0:F2}</b> ", settings.bloomIntensity), "Strength of the bloom filter."), new GUILayoutOption[0]);
                    settings.bloomIntensity = GUILayout.HorizontalSlider((float)settings.bloomIntensity * 100f, 0, 1000f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Threshold <b>{0:F2}</b> ", settings.bloomThreshold), "Filters out pixels under this level of brightness."), new GUILayoutOption[0]);
                    settings.bloomThreshold = GUILayout.HorizontalSlider((float)settings.bloomThreshold * 100f, 0, 1000f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Soft Knee <b>{0:F2}</b> ", settings.bloomSoftKnee), "Makes transition between under/over-threshold gradual (0 = hard threshold, 1 = soft threshold)."), new GUILayoutOption[0]);
                    settings.bloomSoftKnee = GUILayout.HorizontalSlider((float)settings.bloomSoftKnee * 100f, 0, 100f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Radius <b>{0:F2}</b> ", settings.bloomRadius), "Changes extent of veiling effects in a screen resolution-independent fashion."), new GUILayoutOption[0]);
                    settings.bloomRadius = GUILayout.HorizontalSlider((float)settings.bloomRadius * 10f, 10, 70f) / 10f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Lens Dirt Intensity <b>{0:F2}</b> ", settings.bloomLensDirtIntensity), "Amount of lens dirtiness."), new GUILayoutOption[0]);
                    settings.bloomLensDirtIntensity = GUILayout.HorizontalSlider((float)settings.bloomLensDirtIntensity * 100f, 0, 1000f) / 100f;

                    GUILayout.Space(10f);


                    settings.bloomAntiFlicker = GUILayout.Toggle(settings.bloomAntiFlicker, new GUIContent("<b>Anti-flicker</b>", "Reduces flashing noise with an additional filter."), new GUILayoutOption[0]);

                    GUILayout.Space(10f);
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }
                GUILayout.Space(10f);


                settings.customEyeAdapt = GUILayout.Toggle(settings.customEyeAdapt, "<b>Use Custom Eye Adaptation</b>", new GUILayoutOption[0]);
                if (settings.customEyeAdapt)
                {
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentSpace);
                    GUILayout.BeginVertical();

                    GUILayout.Label(new GUIContent(string.Format("Low Percent <b>{0:F0}</b> ", settings.eyeAdaptLowPercent), "Filters the dark part of the histogram when computing the average luminance to avoid very dark pixels from contributing to the auto exposure. Unit is in percent."), new GUILayoutOption[0]);
                    settings.eyeAdaptLowPercent = GUILayout.HorizontalSlider((float)settings.eyeAdaptLowPercent, 1f, 99f);

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("High Percent <b>{0:F0}</b> ", settings.eyeAdaptHighPercent), "Filters the bright part of the histogram when computing the average luminance to avoid very dark pixels from contributing to the auto exposure. Unit is in percent."), new GUILayoutOption[0]);
                    settings.eyeAdaptHighPercent = GUILayout.HorizontalSlider((float)settings.eyeAdaptHighPercent, 1f, 99f);

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Min Avg Luminance for AE<b>{0:F2}</b> ", settings.eyeAdaptMinLuminance), "Minimum average luminance to consider for auto exposure (in EV)."), new GUILayoutOption[0]);
                    settings.eyeAdaptMinLuminance = GUILayout.HorizontalSlider((float)settings.eyeAdaptMinLuminance * 100f, -600, 2100f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Max Avg Luminance for AE<b>{0:F2}</b> ", settings.eyeAdaptMaxLuminance), "Maximum average luminance to consider for auto exposure (in EV)."), new GUILayoutOption[0]);
                    settings.eyeAdaptMaxLuminance = GUILayout.HorizontalSlider((float)settings.eyeAdaptMaxLuminance * 100f, -600, 2100f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Exposure Bias<b>{0:F2}</b> ", settings.eyeAdaptKeyValue), "Exposure bias. Use this to offset the global exposure of the scene."), new GUILayoutOption[0]);
                    settings.eyeAdaptKeyValue = GUILayout.HorizontalSlider((float)settings.eyeAdaptKeyValue * 100f, 0, 100f) / 100f;

                    GUILayout.Space(10f);


                    settings.eyeAdaptDynamicKeyValue = GUILayout.Toggle(settings.eyeAdaptDynamicKeyValue, new GUIContent("<b>Dynamic Key Value</b>", "Turn this on to let Unity handle the key value automatically based on average luminance."), new GUILayoutOption[0]);

                    GUILayout.Space(10f);


                    settings.eyeAdaptAdaptationFixed = GUILayout.Toggle(settings.eyeAdaptAdaptationFixed, new GUIContent("<b>Fixed Eye Adaptation</b>", "Turn off if you want the auto exposure to be animated."), new GUILayoutOption[0]);

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Dark-to-light Adaptation Speed <b>{0:F2}</b> ", settings.eyeAdaptSpeedUp), "Adaptation speed from a dark to a light environment."), new GUILayoutOption[0]);
                    settings.eyeAdaptSpeedUp = GUILayout.HorizontalSlider((float)settings.eyeAdaptSpeedUp * 100f, 0, 1000f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Light-to-dark Adaptation Speed <b>{0:F2}</b> ", settings.eyeAdaptSpeedDown), "Adaptation speed from a light to a dark environment."), new GUILayoutOption[0]);
                    settings.eyeAdaptSpeedDown = GUILayout.HorizontalSlider((float)settings.eyeAdaptSpeedDown * 100f, 0, 1000f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Brightness Range Min<b>{0}</b> ", settings.eyeAdaptLogMin), "Lower bound for the brightness range of the generated histogram (in EV). The bigger the spread between min & max, the lower the precision will be."), new GUILayoutOption[0]);
                    settings.eyeAdaptLogMin = (int)GUILayout.HorizontalSlider((float)settings.eyeAdaptLogMin, -16f, -1f);

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Brightness Range Max<b>{0}</b> ", settings.eyeAdaptLogMax), "Upper bound for the brightness range of the generated histogram (in EV). The bigger the spread between min & max, the lower the precision will be."), new GUILayoutOption[0]);
                    settings.eyeAdaptLogMax = (int)GUILayout.HorizontalSlider((float)settings.eyeAdaptLogMax, 1f, 16f);

                    GUILayout.Space(10f);


                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }

                GUILayout.Space(10f);

                settings.customMotionBlur = GUILayout.Toggle(settings.customMotionBlur, "<b>Use Custom Motion Blur</b>", new GUILayoutOption[0]);
                if (settings.customMotionBlur)
                {
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentSpace);
                    GUILayout.BeginVertical();

                    GUILayout.Label(new GUIContent(string.Format("Shutter Angle <b>{0:F0}</b> ", settings.motionBlurShutterAngle), "The angle of rotary shutter. Larger values give longer exposure."), new GUILayoutOption[0]);
                    settings.motionBlurShutterAngle = GUILayout.HorizontalSlider((float)settings.motionBlurShutterAngle, 0f, 360f);

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Sample Count <b>{0}</b> ", settings.eyeAdaptLogMin), "The amount of sample points, which affects quality and performances."), new GUILayoutOption[0]);
                    settings.eyeAdaptLogMin = (int)GUILayout.HorizontalSlider((float)settings.eyeAdaptLogMin, 4f, 32f);

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Frame Blending <b>{0:F2}</b> ", settings.motionBlurFrameBlending), "The strength of multiple frame blending. The opacity of preceding frames are determined from this coefficient and time differences."), new GUILayoutOption[0]);
                    settings.motionBlurFrameBlending = GUILayout.HorizontalSlider((float)settings.motionBlurFrameBlending * 100f, 0, 100f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }

                GUILayout.Space(10f);


                settings.customDepthOfField = GUILayout.Toggle(settings.customDepthOfField, "<b>Use Custom Depth of Field</b>", new GUILayoutOption[0]);
                if (settings.customDepthOfField)
                {
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentSpace);
                    GUILayout.BeginVertical();

                    GUILayout.Label(new GUIContent(string.Format("Focus Distance <b>{0:F2}</b> ", settings.depthOfFieldFocusDistance), "Distance to the point of focus."), new GUILayoutOption[0]);
                    settings.depthOfFieldFocusDistance = GUILayout.HorizontalSlider((float)settings.depthOfFieldFocusDistance*100f, 10f, 1000f)/100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Aperature <b>{0:F2}</b> ", settings.depthOfFieldAperture), "Ratio of aperture (known as f-stop or f-number). The smaller the value is, the shallower the depth of field is."), new GUILayoutOption[0]);
                    settings.depthOfFieldAperture = GUILayout.HorizontalSlider((float)settings.depthOfFieldAperture * 100f, 5f, 3200f)/100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Focal Length <b>{0:F0}</b> ", settings.depthOfFieldFocalLength), "Distance between the lens and the film. The larger the value is, the shallower the depth of field is."), new GUILayoutOption[0]);
                    settings.depthOfFieldFocalLength = GUILayout.HorizontalSlider((float)settings.depthOfFieldFocalLength, 1f, 300f);

                    GUILayout.Space(10f);


                    settings.depthOfFieldUseCameraFov = GUILayout.Toggle(settings.depthOfFieldUseCameraFov, new GUIContent("Calculate the focal length automatically from the field-of-view value set on the camera. Using this setting isn't recommended."), new GUILayoutOption[0]);


                    GUILayout.Label(new GUIContent(string.Format("Kernel Size <b>{0:F2}</b> ", kernelSizes[settings.depthOfFieldKernelSize]), "Convolution kernel size of the bokeh filter, which determines the maximum radius of bokeh. It also affects the performance (the larger the kernel is, the longer the GPU time is required)."), new GUILayoutOption[0]);
                    settings.depthOfFieldKernelSize = (int)GUILayout.HorizontalSlider((float)settings.depthOfFieldKernelSize, 0f, 3f);

                    GUILayout.Space(10f);


                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }

                GUILayout.Space(10f);


                settings.customColorGrading = GUILayout.Toggle(settings.customColorGrading, "<b>Use Custom Color Grading</b>", new GUILayoutOption[0]);
                if (settings.customColorGrading)
                {
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentSpace);
                    GUILayout.BeginVertical();

                    GUILayout.Label(new GUIContent(string.Format("Tone Mapper <b>{0}</b> ", toneMappers[settings.colorGradingTonemapper]), "Tonemapping algorithm to use at the end of the color grading process. Use \"Neutral\" if you need a customizable tonemapper."), new GUILayoutOption[0]);
                    settings.colorGradingTonemapper = (int)GUILayout.HorizontalSlider((float)settings.colorGradingTonemapper, 0f, 2f);

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Neutral Black In <b>{0:F2}</b> ", settings.colorGradingNeutralBlackIn), ""), new GUILayoutOption[0]);
                    settings.colorGradingNeutralBlackIn = GUILayout.HorizontalSlider((float)settings.colorGradingNeutralBlackIn * 100f, -10f, 10f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Neutral White In <b>{0:F1}</b> ", settings.colorGradingNeutralWhiteIn), ""), new GUILayoutOption[0]);
                    settings.colorGradingNeutralWhiteIn = GUILayout.HorizontalSlider((float)settings.colorGradingNeutralWhiteIn * 10f, 10, 200f) / 10f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Neutral Black Out <b>{0:F2}</b> ", settings.colorGradingNeutralBlackOut), ""), new GUILayoutOption[0]);
                    settings.colorGradingNeutralBlackOut = GUILayout.HorizontalSlider((float)settings.colorGradingNeutralBlackOut * 100f, -9f, 10f) / 100f;

                    GUILayout.Space(10f);

                    GUILayout.Label(new GUIContent(string.Format("Neutral White Out <b>{0:F1}</b> ", settings.colorGradingNeutralWhiteOut), ""), new GUILayoutOption[0]);
                    settings.colorGradingNeutralWhiteOut = GUILayout.HorizontalSlider((float)settings.colorGradingNeutralWhiteOut * 10f, 10, 190f) / 10f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Neutral White Level <b>{0:F1}</b> ", settings.colorGradingNeutralWhiteLevel), ""), new GUILayoutOption[0]);
                    settings.colorGradingNeutralWhiteLevel = GUILayout.HorizontalSlider((float)settings.colorGradingNeutralWhiteLevel * 10f, 1f, 200f) / 10f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Neutral White Clip <b>{0:F1}</b> ", settings.colorGradingNeutralWhiteClip), ""), new GUILayoutOption[0]);
                    settings.colorGradingNeutralWhiteClip = GUILayout.HorizontalSlider((float)settings.colorGradingNeutralWhiteClip * 10f, 10f, 100f) / 10f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Post Exposure <b>{0:F2}</b> ", settings.colorGradingPostExposure), "Adjusts the overall exposure of the scene in EV units. This is applied after HDR effect and right before tonemapping so it won't affect previous effects in the chain."), new GUILayoutOption[0]);
                    settings.colorGradingPostExposure = GUILayout.HorizontalSlider((float)settings.colorGradingPostExposure * 100f, -600, 2100f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Temperature <b>{0:F1}</b> ", settings.colorGradingTemperature), "Sets the white balance to a custom color temperature."), new GUILayoutOption[0]);
                    settings.colorGradingTemperature = GUILayout.HorizontalSlider((float)settings.colorGradingTemperature * 10f, -1000, 1000f) / 10f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Tint <b>{0:F1}</b> ", settings.colorGradingTint), "Sets the white balance to compensate for a green or magenta tint."), new GUILayoutOption[0]);
                    settings.colorGradingTint = GUILayout.HorizontalSlider((float)settings.colorGradingTint * 10f, -1000f, 1000f) / 10f;

                    GUILayout.Space(10f);

                    GUILayout.Label(new GUIContent(string.Format("Hue Shift <b>{0:F0}</b> ", settings.colorGradingHueShift), ""), new GUILayoutOption[0]);
                    settings.colorGradingHueShift = GUILayout.HorizontalSlider((float)settings.colorGradingHueShift, -180f, 180f);

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Saturation <b>{0:F2}</b> ", settings.colorGradingSaturation), "Pushes the intensity of all colors."), new GUILayoutOption[0]);
                    settings.colorGradingSaturation = GUILayout.HorizontalSlider((float)settings.colorGradingSaturation * 100f, 0f, 100f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Contrast <b>{0:F2}</b> ", settings.colorGradingContrast), "Expands or shrinks the overall range of tonal values."), new GUILayoutOption[0]);
                    settings.colorGradingContrast = GUILayout.HorizontalSlider((float)settings.colorGradingContrast * 100f, 0f, 100f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }

                GUILayout.Space(10f);


                settings.customAO = GUILayout.Toggle(settings.customAO, "<b>Use Custom Ambient Occlusion</b>", new GUILayoutOption[0]);
                if (settings.customAO)
                {
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentSpace);
                    GUILayout.BeginVertical();

                    GUILayout.Label(new GUIContent(string.Format("Intensity <b>{0:F2}</b> ", settings.AOIntensity), "Degree of darkness produced by the effect."), new GUILayoutOption[0]);
                    settings.AOIntensity = GUILayout.HorizontalSlider((float)settings.AOIntensity * 100f, 0f, 400f) / 100f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Radius <b>{0:F2}</b> ", settings.AORadius), "Radius of sample points, which affects extent of darkened areas."), new GUILayoutOption[0]);
                    settings.AORadius = GUILayout.HorizontalSlider((float)settings.AORadius * 10000f, 1f, 10000) / 10000f;

                    GUILayout.Space(10f);


                    GUILayout.Label(new GUIContent(string.Format("Sample Count <b>{0:F2}</b> ", sampleCounts.Keys.ToArray()[settings.AOSampleCount]), "Number of sample points, which affects quality and performance."), new GUILayoutOption[0]);
                    settings.AOSampleCount = (int)GUILayout.HorizontalSlider((float)settings.AOSampleCount, 0f, 3f);

                    GUILayout.Space(10f);


                    settings.AODownsampling = GUILayout.Toggle(settings.AODownsampling, new GUIContent("Downsample", "Halves the resolution of the effect to increase performance at the cost of visual quality."), new GUILayoutOption[0]);
                    GUILayout.Space(10f);

                    settings.AOForceForwardCompatibility = GUILayout.Toggle(settings.AOForceForwardCompatibility, new GUIContent("Force Forward Compatibility", "Forces compatibility with Forward rendered objects when working with the Deferred rendering path."), new GUILayoutOption[0]);
                    GUILayout.Space(10f);

                    settings.AOAmbientOnly = GUILayout.Toggle(settings.AOAmbientOnly, new GUIContent("Ambient Only", "Enables the ambient-only mode in that the effect only affects ambient lighting. This mode is only available with the Deferred rendering path and HDR rendering."), new GUILayoutOption[0]);
                    GUILayout.Space(10f);

                    settings.AOHighPrecision = GUILayout.Toggle(settings.AOHighPrecision, new GUIContent("High Precision", "Toggles the use of a higher precision depth texture with the forward rendering path (may impact performances). Has no effect with the deferred rendering path."), new GUILayoutOption[0]);
                    GUILayout.Space(10f);

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }

            /*
                GUILayout.Space(10f);

                settings.customAA = GUILayout.Toggle(settings.customAA, "<b>Use Custom Anti-Aliasing</b>", new GUILayoutOption[0]);
                if (settings.customAA)
                {
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentSpace);
                    GUILayout.BeginVertical();

                    settings.AAMethodTaa = !GUILayout.Toggle(!settings.AAMethodTaa, "<b>Use Fxaa Anti-Aliasing</b>", new GUILayoutOption[0]);
                    settings.AAMethodTaa = GUILayout.Toggle(settings.AAMethodTaa, "<b>Use Taa Anti-Aliasing</b>", new GUILayoutOption[0]);

                    if (settings.AAMethodTaa)
                    {
                        GUILayout.Label(new GUIContent(string.Format("Jitter Spread <b>{0:F2}</b> ", settings.AAJitterSpread), "The diameter (in texels) inside which jitter samples are spread. Smaller values result in crisper but more aliased output, while larger values result in more stable but blurrier output."), new GUILayoutOption[0]);
                        settings.AAJitterSpread = GUILayout.HorizontalSlider((float)settings.AAJitterSpread*100f, 10f, 100f)/100f;

                        GUILayout.Space(10f);


                        GUILayout.Label(new GUIContent(string.Format("Sharpen <b>{0:F2}</b> ", settings.AASharpen), "Controls the amount of sharpening applied to the color buffer."), new GUILayoutOption[0]);
                        settings.AASharpen = GUILayout.HorizontalSlider((float)settings.AASharpen * 100f, 0, 300f)/100f;

                        GUILayout.Space(10f);


                        GUILayout.Label(new GUIContent(string.Format("Stationary Blending <b>{0:F2}</b> ", settings.AAStationaryBlending), "The blend coefficient for a stationary fragment. Controls the percentage of history sample blended into the final color."), new GUILayoutOption[0]);
                        settings.AAStationaryBlending = GUILayout.HorizontalSlider((float)settings.AAStationaryBlending * 100f, 0f, 99f)/100f;

                        GUILayout.Space(10f);


                        GUILayout.Label(new GUIContent(string.Format("Motion Blending <b>{0:F2}</b> ", settings.AAMotionBlending), "The blend coefficient for a fragment with significant motion. Controls the percentage of history sample blended into the final color."), new GUILayoutOption[0]);
                        settings.AAMotionBlending = GUILayout.HorizontalSlider((float)settings.AAMotionBlending * 100f, 0f, 99f)/100f;

                    }
                    else
                    {
                        GUILayout.Label(new GUIContent(string.Format("Fxaa Preset <b>{0}</b> ", fxaaPresets[settings.AAFxaaPreset]), ""), new GUILayoutOption[0]);
                        settings.AAFxaaPreset = (int)GUILayout.HorizontalSlider((float)settings.AAFxaaPreset, 0f, 4f);


                    }

                    GUILayout.Space(10f);


                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }
            */
                GUILayout.Space(20f);

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

        }


        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

    }
}

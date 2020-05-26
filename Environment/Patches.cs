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

namespace Environment
{
    static partial class Main
    {

        public static List<Dictionary<string,Gradient>> defaultGradients;
        public static BiomoTheme[] currentBiomoThemes;

        static void UpdateGradients()
        {
            if(WeatherModule.Self == null)
            {
                return;
            }
            WeatherCtr weatherCtr = (WeatherCtr)typeof(WeatherModule).GetField("weatherCtr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(WeatherModule.Self);
            if (weatherCtr == null)
            {
                return;
            }
            Executor executor = (Executor)typeof(WeatherCtr).GetField("executor", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(weatherCtr);

            if (defaultGradients == null)
            {
                Dbgl("setting defaults");
                defaultGradients = new List<Dictionary<string, Gradient>>();
                foreach(BiomoTheme bt in executor.BiomoThemes)
                {
                    Dictionary<string, Gradient> dic = new Dictionary<string, Gradient>();
                    foreach(string grad in gradientMasks)
                    {
                        dic.Add(grad, (Gradient)typeof(BiomoTheme).GetField(grad).GetValue(bt));
                    }
                    defaultGradients.Add(dic);
                }
            }
            if (defaultGradients == null)
                return;

            GradientColorKey[] gc = defaultGradients[executor.BiomoIndex]["AmbientSkyColorChange"].colorKeys;
            string str = "";
            foreach(GradientColorKey g in gc)
            {
                str += g.color+"\r\n";
            }
            Dbgl(str);

            for (int k = 0; k < executor.BiomoThemes.Length; k++)
            {
                foreach (string name in gradientMasks)
                {
                    Gradient gradient = new Gradient();
                    gradient.colorKeys = defaultGradients[k][name].colorKeys;
                    gradient.alphaKeys = defaultGradients[k][name].alphaKeys;

                    //Dbgl("checking " + name);
                    if (enabled && (bool)typeof(Settings).GetField(name + "GradientMaskEnable").GetValue(settings))
                    {
                        //Dbgl("setting " + name);
                        GradientColorKey[] gradientColorKeys = gradient.colorKeys;
                        for (int i = 0; i < gradientColorKeys.Length; i++)
                        {
                            Color color = gradientColorKeys[i].color;
                            if ((bool)typeof(Settings).GetField(name + "GradientMaskForceMonochrome").GetValue(settings))
                            {
                                float mask = (float)typeof(Settings).GetField(name + "GradientMaskColorMonochrome").GetValue(settings);
                                float orig = (color.r + color.g + color.b) / 3;
                                float grey;
                                if (mask >= 0)
                                {
                                    grey = orig + (1f - orig) * mask;
                                }
                                else
                                {
                                    grey = orig + orig * mask;
                                }
                                gradientColorKeys[i].color.r = grey;
                                gradientColorKeys[i].color.g = grey;
                                gradientColorKeys[i].color.b = grey;
                            }
                            else
                            {
                                float r = (float)typeof(Settings).GetField(name + "GradientMaskR").GetValue(settings);
                                float g = (float)typeof(Settings).GetField(name + "GradientMaskG").GetValue(settings);
                                float b = (float)typeof(Settings).GetField(name + "GradientMaskB").GetValue(settings);
                                if (r >= 0)
                                {
                                    gradientColorKeys[i].color.r = color.r + (1f - color.r) * r;
                                }
                                else
                                {
                                    gradientColorKeys[i].color.r = color.r + color.r * r;
                                }
                                if (g >= 0)
                                {
                                    gradientColorKeys[i].color.g = color.g + (1f - color.g) * g;
                                }
                                else
                                {
                                    gradientColorKeys[i].color.g = color.g + color.g * g;
                                }
                                if (b >= 0)
                                {
                                    gradientColorKeys[i].color.b = color.b + (1f - color.b) * b;
                                }
                                else
                                {
                                    gradientColorKeys[i].color.b = color.b + color.b * b;
                                }

                            }
                            float a = (float)typeof(Settings).GetField(name + "GradientMaskA").GetValue(settings);
                            if (a >= 0)
                            {
                                gradientColorKeys[i].color.a = color.a + (1f - color.a) * a;
                            }
                            else
                            {
                                gradientColorKeys[i].color.a = color.a + color.a * a;
                            }

                        }
                        gradient.colorKeys = gradientColorKeys;
                        //Dbgl("setting " + name + " " + gradient.colorKeys[0].color);
                    }
                    GradientAlphaKey[] gradientAlphaKeys = gradient.alphaKeys;
                    float a2 = (float)typeof(Settings).GetField(name + "GradientMaskA").GetValue(settings);
                    for (int i = 0; i < gradientAlphaKeys.Length; i++)
                    {
                        float alpha = gradientAlphaKeys[i].alpha;
                        if (a2 >= 0)
                        {
                            gradientAlphaKeys[i].alpha = alpha + (1f - alpha) * a2;
                        }
                        else
                        {
                            gradientAlphaKeys[i].alpha = alpha + alpha * a2;
                        }
                    }
                    gradient.alphaKeys = gradientAlphaKeys;
                    typeof(BiomoTheme).GetField(name).SetValue(executor.BiomoThemes[k], gradient);
                }
            }
        }


        [HarmonyPatch(typeof(Output), "Apply")]
        static class Output_Patch
        {
            static void Prefix(ref Output __instance)
            {
                if (!enabled)
                    return;
                /*
                Dbgl("SunLightColor " + __instance.SunLightColor);
                Dbgl("SunLightIntensity " + __instance.SunLightIntensity);

                Dbgl("overcastFColor " + __instance.overcastFColor);
                Dbgl("overcastBColor " + __instance.overcastBColor);
                Dbgl("flowCloudFColor " + __instance.flowCloudFColor);
                Dbgl("flowCloudBColor " + __instance.flowCloudBColor);
                Dbgl("overcastAlpha " + __instance.OvercastAlpha);
                Dbgl("flowCloudAlpha " + __instance.flowCloudAlpha);
                */

                if (settings.SunLightColorEnable)
                    __instance.SunLightColor = new Color(settings.SunLightColorR, settings.SunLightColorG, settings.SunLightColorB, settings.SunLightColorA);
                if (settings.SunLightIntensityEnable)
                    __instance.SunLightIntensity = settings.SunLightIntensity;

                if (settings.OvercastFColorEnable)
                    __instance.overcastFColor = new Color(settings.OvercastFColorR, settings.OvercastFColorG, settings.OvercastFColorB, settings.OvercastFColorA);
                if (settings.OvercastBColorEnable)
                    __instance.overcastBColor = new Color(settings.OvercastBColorR, settings.OvercastBColorG, settings.OvercastBColorB, settings.OvercastBColorA);
                if (settings.flowCloudFColorEnable)
                    __instance.flowCloudFColor = new Color(settings.flowCloudFColorR, settings.flowCloudFColorG, settings.flowCloudFColorB, settings.flowCloudFColorA);
                if (settings.flowCloudBColorEnable)
                    __instance.flowCloudBColor = new Color(settings.flowCloudBColorR, settings.flowCloudBColorG, settings.flowCloudBColorB, settings.flowCloudBColorA);

                if (settings.SunLightColorEnable)
                    __instance.OvercastAlpha = settings.OvercastAlpha;
                if (settings.SunLightColorEnable)
                    __instance.flowCloudAlpha = settings.flowCloudAlpha;

            }
        }
        [HarmonyPatch(typeof(Sky), "Apply")]
        static class Sky_Patch
        {
            static void Prefix(ref Sky __instance)
            {
                if (!enabled)
                    return;
                /*
                Dbgl("SkyColor"+__instance.SkyColor);
                Dbgl("FogColorA"+__instance.FogColorA);
                Dbgl("FogColorB"+__instance.FogColorB);
                Dbgl("UnityFogColor"+__instance.UnityFogColor);
                Dbgl("SunBloomColor"+__instance.SunBloomColor);
                Dbgl("SunColor"+__instance.SunColor);
                Dbgl("Moon1BloomColor"+__instance.Moon1BloomColor);
                Dbgl("FogHeight"+__instance.FogHeight);
                Dbgl("SunColorFade"+__instance.SunColorFade);
                Dbgl("Moon1Power"+__instance.Moon1Power);
                Dbgl("SunSize"+__instance.SunSize);
                Dbgl("SunPower"+__instance.SunPower);
                Dbgl("Overcast"+__instance.Overcast);
                Dbgl("BloomSpread"+__instance.BloomSpread);
                Dbgl("StarPower"+__instance.StarPower);
                */

                if (settings.SkyColorEnable)
                    __instance.SkyColor = new Color(settings.SkyColorR, settings.SkyColorG, settings.SkyColorB, settings.SkyColorA);
                if (settings.FogColorAEnable)
                    __instance.FogColorA = new Color(settings.FogColorAR, settings.FogColorAG, settings.FogColorAB, settings.FogColorAA);
                if (settings.FogColorBEnable)
                    __instance.FogColorB = new Color(settings.FogColorBR, settings.FogColorBG, settings.FogColorBB, settings.FogColorBA);
                if (settings.UnityFogColorEnable)
                    __instance.UnityFogColor = new Color(settings.UnityFogColorR, settings.UnityFogColorG, settings.UnityFogColorB, settings.UnityFogColorA);
                if (settings.SunBloomColorEnable)
                    __instance.SunBloomColor = new Color(settings.SunBloomColorR, settings.SunBloomColorG, settings.SunBloomColorB, settings.SunBloomColorA);
                if (settings.SunColorEnable)
                    __instance.SunColor = new Color(settings.SunColorR, settings.SunColorG, settings.SunColorB, settings.SunColorA);
                if (settings.Moon1BloomColorEnable)
                    __instance.Moon1BloomColor = new Color(settings.Moon1BloomColorR, settings.Moon1BloomColorG, settings.Moon1BloomColorB, settings.Moon1BloomColorA);
                if (settings.FogHeightEnable)
                    __instance.FogHeight = settings.FogHeight;
                if (settings.SunColorFadeEnable)
                    __instance.SunColorFade = settings.SunColorFade;
                if (settings.Moon1PowerEnable)
                    __instance.Moon1Power = settings.Moon1Power;
                if (settings.SunSizeEnable)
                    __instance.SunSize = settings.SunSize;
                if (settings.SunPowerEnable)
                    __instance.SunPower = settings.SunPower;
                if (settings.OvercastEnable)
                    __instance.Overcast = settings.Overcast;
                if (settings.BloomSpreadEnable)
                    __instance.BloomSpread = settings.BloomSpread;
                if (settings.StarPowerEnable)
                    __instance.StarPower = settings.StarPower;
            }
        }

        private static bool defaultPostProcessingSet = false;
        private static VignetteModel.Settings defaultVignetteSettings;
        private static BloomModel.Settings defaultBloomSettings;

        [HarmonyPatch(typeof(PostProcessingBehaviour), "OnEnable")]
        static class PostProcessingBehaviour_OnEnable_Patch
        {

            static void Prefix(PostProcessingBehaviour __instance)
            {
                if(__instance.profile != null && !defaultPostProcessingSet)
                {
                    defaultVignetteSettings = __instance.profile.vignette.settings;
                    defaultBloomSettings = __instance.profile.bloom.settings;
                    defaultPostProcessingSet = true;
                }
                else
                {
                    defaultVignetteSettings = VignetteModel.Settings.defaultSettings;
                    defaultBloomSettings = BloomModel.Settings.defaultSettings;
                }
            }
        }

        [HarmonyPatch(typeof(PostProcessingBehaviour), "OnPreCull")]
        static class PostProcessingBehaviour_OnPreCull_Patch
        {
            static void Postfix(PostProcessingBehaviour __instance, PostProcessingContext ___m_Context, ref VignetteComponent ___m_Vignette, ref BloomComponent ___m_Bloom)
            {
                PostProcessingContext postProcessingContext = ___m_Context;

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
            }
        }
    }
}

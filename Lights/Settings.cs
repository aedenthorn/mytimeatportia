using UnityModManagerNet;

namespace Lights
{
    public class Settings : UnityModManager.ModSettings
    {

        public bool showWorkshopSettings = false;
        public bool showSceneSettings = false;

        public float lampLightIntensity = 1.3f;
        public float lampLightRange = 10.0f;
        public float lampBounceIntensity = 1.0f;
        public float lampColorR = 0.978f;
        public float lampColorG = 0.813f;
        public float lampColorB = 0.518f;

        public bool lampsCastShadows = true;
        public float lampShadowNearPlane = 1.0f;
        public float lampShadowStrength = 1.0f;
        public float lampShadowBias = 0.05f;
        public float lampShadowNormalBias = 0.4f;
        public int lampShadowResolution = -1;


        public bool customStreetlightLights = false;

        public float streetlightLightIntensityMult = 0;
        public float streetlightLightRangeMult = 0;
        public float streetlightBounceIntensityMult = 0;
        public float streetlightColorRMult = 0;
        public float streetlightColorGMult = 0;
        public float streetlightColorBMult = 0;

        public bool streetlightsCastShadows = true;
        public float streetlightShadowNearPlane = 1.0f;
        public float streetlightShadowStrength = 1.0f;
        public float streetlightShadowBias = 0.05f;
        public float streetlightShadowNormalBias = 0.4f;
        public int streetlightShadowResolution = -1;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
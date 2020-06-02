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

        public float streetlightLightIntensity = 1.3f;
        public float streetlightLightRange = 10.0f;
        public float streetlightBounceIntensity = 1.0f;
        public float streetlightColorR = 0.978f;
        public float streetlightColorG = 0.813f;
        public float streetlightColorB = 0.518f;

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
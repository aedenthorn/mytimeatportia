using UnityModManagerNet;
namespace Environment
{
    public class Settings : UnityModManager.ModSettings
    {
		public float SkyColorR = 0.092f;
		public float SkyColorG = 0.582f;
		public float SkyColorB = 0.810f;
		public float SkyColorA = 1;

		public float FogColorAR = 0.655f;
		public float FogColorAG = 0.832f;
		public float FogColorAB = 0.731f;
		public float FogColorAA = 1;

		public float FogColorBR = 0.502f;
		public float FogColorBG = 0.852f;
		public float FogColorBB = 0.993f;
		public float FogColorBA = 1;

		public float UnityFogColorR = 0.572f;
		public float UnityFogColorG = 0.843f;
		public float UnityFogColorB = 0.874f;
		public float UnityFogColorA = 1;

		public float SunBloomColorR = 0.422f;
		public float SunBloomColorG = 0.355f;
		public float SunBloomColorB = 0.289f;
		public float SunBloomColorA = 1;

		public float SunColorR = 2.346f;
		public float SunColorG = 2.013f;
		public float SunColorB = 1.208f;
		public float SunColorA = 2.422f;

		public float Moon1BloomColorR = 0.565f;
		public float Moon1BloomColorG = 0.659f;
		public float Moon1BloomColorB = 0.753f;
		public float Moon1BloomColorA = 1;


		public float SunLightColorR = 0.966f;
		public float SunLightColorG = 0.842f;
		public float SunLightColorB = 0.575f;
		public float SunLightColorA = 1f;


		public float OvercastFColorR = 1f;
		public float OvercastFColorG = 1f;
		public float OvercastFColorB = 1f;
		public float OvercastFColorA = 1f;

		public float OvercastBColorR = 1f;
		public float OvercastBColorG = 1f;
		public float OvercastBColorB = 1f;
		public float OvercastBColorA = 1f;

		public float flowCloudFColorR = 1f;
		public float flowCloudFColorG = 1f;
		public float flowCloudFColorB = 1f;
		public float flowCloudFColorA = 1f;

		public float flowCloudBColorR = 1f;
		public float flowCloudBColorG = 1f;
		public float flowCloudBColorB = 1f;
		public float flowCloudBColorA = 1f;



		public float FogHeight = 0.6782711f;
		public float SunColorFade = -0.5600431f;
		public float Moon1Power = 23.40322f;
		public float SunSize = 1.707245f;
		public float SunPower = 13.17698f;
		public float Overcast = 1f;
		public float BloomSpread = 0.3318552f;
		public float StarPower = 0f;

		public float SunLightIntensity = 1f;

		public float OvercastAlpha = 0f;
		public float flowCloudAlpha = 0f;


		public bool SkyColorEnable = false;
		public bool FogColorAEnable = false;
		public bool FogColorBEnable = false;
		public bool UnityFogColorEnable = false;
		public bool SunBloomColorEnable = false;
		public bool SunColorEnable = false;
		public bool Moon1BloomColorEnable = false;
		public bool SunColorFadeEnable = false;
		public bool FogHeightEnable = false;
		public bool Moon1PowerEnable = false;
		public bool SunSizeEnable = false;
		public bool SunPowerEnable = false;
		public bool OvercastEnable = false;
		public bool BloomSpreadEnable = false;
		public bool StarPowerEnable = false;

		public bool SunLightColorEnable = false;
		public bool SunLightIntensityEnable = false;
		
		public bool OvercastAlphaEnable = false;

		public bool OvercastFColorEnable = false;
		public bool OvercastBColorEnable = false;
		public bool flowCloudFColorEnable = false;
		public bool flowCloudBColorEnable = false;

		public string ToggleKey = "page up";
		internal bool flowCloudAlphaEnable;

		public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
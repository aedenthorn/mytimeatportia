using UnityModManagerNet;
namespace PostProcessing
{
    public class Settings : UnityModManager.ModSettings
    {
	
		public bool customVignette = false;
		public bool customBloom = false;
		public bool customEyeAdapt = false;
		public bool customMotionBlur = false;
		public bool customColorGrading = false;
		public bool customDepthOfField = false;

		public float vignetteColorRed = 0f;
		public float vignetteColorGreen = 0f;
		public float vignetteColorBlue = 0f;
		public float vignetteColorAlpha = 0f;
		
		public float vignetteIntensity = 0f;
		public float vignetteX = 0.5f;
		public float vignetteY = 0.5f;
		public float vignetteSmoothness = 1f;
		public float vignetteRoundness = 0f;
		public bool vignetteRounded = false;

		public float bloomIntensity = 0.5f;
		public float bloomThreshold = 1.1f;
		public float bloomSoftKnee = 0.5f;
		public float bloomRadius = 4f;
		public bool bloomAntiFlicker = false;
		public float bloomLensDirtIntensity = 3f;

		public float eyeAdaptLowPercent = 45f;
		public float eyeAdaptHighPercent = 95f;
		public float eyeAdaptMinLuminance = -5f;
		public float eyeAdaptMaxLuminance = 1f;
		public float eyeAdaptKeyValue = 0.25f;
		public bool eyeAdaptDynamicKeyValue = true;
		public bool eyeAdaptAdaptationFixed = false;
		public float eyeAdaptSpeedUp = 2f;
		public float eyeAdaptSpeedDown = 1f;
		public int eyeAdaptLogMin = -8;
		public int eyeAdaptLogMax = 4;

		public float motionBlurShutterAngle = 270f;
		public int motionBlurSampleCount = 10;
		public float motionBlurFrameBlending = 0f;

		public float depthOfFieldFocusDistance = 10f;
		public float depthOfFieldAperture = 5.6f;
		public float depthOfFieldFocalLength = 50f;
		public bool depthOfFieldUseCameraFov = false;
		public int depthOfFieldKernelSize = 1;

		public int colorGradingTonemapper = 2;
		public float colorGradingNeutralBlackIn = 0.02f;
		public float colorGradingNeutralWhiteIn = 10f;
		public float colorGradingNeutralBlackOut = 0f;
		public float colorGradingNeutralWhiteOut = 10f;
		public float colorGradingNeutralWhiteLevel = 5.3f;
		public float colorGradingNeutralWhiteClip = 10f;

		public float colorGradingPostExposure = 0f;
		public float colorGradingTemperature = 0f;
		public float colorGradingTint = 0f;
		public float colorGradingHueShift = 0f;
		public float colorGradingSaturation = 1f;
		public float colorGradingContrast = 1f;

		public string ToggleKey = "page down";

		public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
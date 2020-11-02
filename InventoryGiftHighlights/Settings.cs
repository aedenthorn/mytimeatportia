using UnityModManagerNet;

namespace InventoryGiftHighlights
{
    public class Settings : UnityModManager.ModSettings
    {
        public float hateColorRed = 1f;
        public float hateColorGreen = 0f;
        public float hateColorBlue = 0f;
        public float hateColorAlpha = 1f;

        public float dislikeColorRed = 1f;
        public float dislikeColorGreen = 0.5f;
        public float dislikeColorBlue = 0f;
        public float dislikeColorAlpha = 1f;

        public float neutralColorRed = 1f;
        public float neutralColorGreen = 1f;
        public float neutralColorBlue = 1f;
        public float neutralColorAlpha = 1f;

        public float likeColorRed = 0f;
        public float likeColorGreen = 1f;
        public float likeColorBlue = 0f;
        public float likeColorAlpha = 1f;

        public float loveColorRed = 1f;
        public float loveColorGreen = 0.8f;
        public float loveColorBlue = 0.8f;
        public float loveColorAlpha = 1f;

        public bool ShowOnlyKnown { get; set; } = true;
        public bool ShowHated { get; set; } = true;
        public bool ShowDisliked { get; set; } = true;
        public bool ShowNeutral { get; set; } = true;
        public bool ShowLiked { get; set; } = true;
        public bool ShowLoved { get; set; } = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 
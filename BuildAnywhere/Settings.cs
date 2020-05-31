using UnityModManagerNet;
namespace BuildAnywhere
{
    public class Settings : UnityModManager.ModSettings
    {

        public bool allowOverlapInWorkshop = false;

		public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
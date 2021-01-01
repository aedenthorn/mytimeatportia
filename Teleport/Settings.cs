using UnityModManagerNet;
namespace Teleport
{
    public class Settings : UnityModManager.ModSettings
    {
	
		public string ModifierKey = "left shift";

		public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
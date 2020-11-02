using Harmony12;
using Pathea.FarmFactoryNs;
using Pathea.HomeNs;
using Pathea.ModuleNs;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace ConnectedFactoryStorage
{
    public class Main
    {
        public static Settings settings { get; private set; }

        public static bool enabled;

        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "ConnectedFactoryStorage " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
        }


        [HarmonyPatch(typeof(HomeModule), nameof(HomeModule.GetItemCount))]
        static class HomeModule_GetItemCount_Patch
        {
            static void Postfix(ref int __result, int itemId)
            {
                if (!enabled)
                    return;

                FarmFactory factory = Module<FarmFactoryMgr>.Self.GetFactory(1000);

                if (factory != null)
                {
                    __result += factory.GetMatCount(itemId);
                }
            }
        }

        [HarmonyPatch(typeof(HomeModule), nameof(HomeModule.DeleteItem))]
        static class HomeModule_DeleteItem_Patch
        {
            static void Postfix(ref int __result, int itemId)
            {
                if (!enabled || __result <= 0)
                    return;

                FarmFactory factory = Module<FarmFactoryMgr>.Self.GetFactory(1000);

                if (factory != null)
                {
                    factory.RemoveMat(itemId, __result);
                }
            }
        }
    }
}

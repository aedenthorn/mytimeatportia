using Harmony12;
using Pathea;
using Pathea.ItemDropNs;
using Pathea.ModuleNs;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace InstantLoot
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;

        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "InstantLoot " : "") + str);
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
            GUILayout.Label(string.Format("Autoloot distance multiplier: <b>{0:F1}</b>", settings.DistanceMult), new GUILayoutOption[0]);
            settings.DistanceMult = GUILayout.HorizontalSlider(settings.DistanceMult, 1f, 100f, new GUILayoutOption[0]);
        }
        [HarmonyPatch(typeof(PlayerAutoPickTarget), "Start")]
        static class PlayerAutoPickTarget_Start_Patch
        {
            static bool Prefix(PlayerAutoPickTarget __instance)
            {
                if (!enabled)
                    return true;

                if (Module<Player>.Self == null || Module<Player>.Self.actor == null)
                {
                    return true;
                }

                Dbgl($"start PlayerAutoPickTarget; pick distance: {__instance.autoPickDistance * settings.DistanceMult}, distance {Vector3.Distance(__instance.transform.position, Module<Player>.Self.GamePos + new Vector3(0f, Module<Player>.Self.actor.GetHeight() / 2f, 0f))}");

                ItemDrop component = __instance.GetComponent<ItemDrop>();
                ItemPickFollow follow = __instance.GetComponent<ItemPickFollow>();
                if (Vector3.Distance(__instance.transform.position, Module<Player>.Self.GamePos + new Vector3(0f, Module<Player>.Self.actor.GetHeight() / 2f, 0f)) >= __instance.autoPickDistance * settings.DistanceMult)
                {
                    return true;
                }
                Dbgl("1");
                if (component != null && follow != null)
                {
                    Dbgl("2");
                    if (follow.CheckCanAddBag(component))
                    {
                        Dbgl("3");
                        component.FetchItem();
                        if (Module<ItemDropManager>.Self != null)
                        {
                            Module<ItemDropManager>.Self.DestroyDropItem(component);
                        }
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerAutoPickTarget), "UpdatePickToTarget")]
        static class PlayerAutoPickTarget_UpdatePickToTarget_Patch
        {
            static bool Prefix(PlayerAutoPickTarget __instance)
            {
                if (!enabled)
                    return true;

                if (Module<Player>.Self == null || Module<Player>.Self.actor == null)
                {
                    return true;
                }


                Dbgl($"UpdatePickToTarget PlayerAutoPickTarget; pick distance: {__instance.autoPickDistance * settings.DistanceMult}, distance {Vector3.Distance(__instance.transform.position, Module<Player>.Self.GamePos + new Vector3(0f, Module<Player>.Self.actor.GetHeight() / 2f, 0f))}");

                ItemDrop component = __instance.GetComponent<ItemDrop>();
                ItemPickFollow follow = __instance.GetComponent<ItemPickFollow>();
                if (Vector3.Distance(__instance.transform.position, Module<Player>.Self.GamePos + new Vector3(0f, Module<Player>.Self.actor.GetHeight() / 2f, 0f)) >= __instance.autoPickDistance * settings.DistanceMult)
                {
                    return true;
                }
                Dbgl("1");
                if (component != null && follow != null)
                {
                    Dbgl("2");
                    if (follow.CheckCanAddBag(component))
                    {
                        Dbgl("3");
                        component.FetchItem();
                        if (Module<ItemDropManager>.Self != null)
                        {
                            Module<ItemDropManager>.Self.DestroyDropItem(component);
                        }
                        return false;
                    }
                }
                return true;
            }
        }
    }
}

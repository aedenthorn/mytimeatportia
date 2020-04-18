using Harmony12;
using Pathea;
using Pathea.DungeonModuleNs;
using Pathea.ItemDropNs;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.TreasureRevealerNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace TreasureRevealerMod
{
    public class Main
    {
        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "TreasureRevealerMod " : "") + str);
        }
        public static bool enabled;
        public static Settings settings { get; private set; }

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Scanner Range Multiplier: <b>{0:F1}</b>", settings.RangeMult), new GUILayoutOption[0]);
            settings.RangeMult = GUILayout.HorizontalSlider(Main.settings.RangeMult * 10, 1f, 100f, new GUILayoutOption[0]) / 10;
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static List<TreasureRevealerItem> tril = new List<TreasureRevealerItem>();

        [HarmonyPatch(typeof(VoxelDungeonData), "ResetAllData")]
        static class ResetAllData_Patch
        {
            static void Postfix(VoxelDungeonData __instance)
            {
                if (!enabled)
                    return;

                //__instance.GetType().GetMethod("UpdateLimitedItem", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { });

            }
        }
        [HarmonyPatch(typeof(VoxelDungeonController), "GenerateVoxel")]
        static class VoxelDungeonController_GenerateVoxel_Patch
        {
            static void Postfix()
            {
                Dbgl("test");
                if (!enabled)
                    return;

                Dbgl("GenerateVoxel");
                GetAllTreasures(TreasureRevealer.Instance);

            }
        }

        static void GetAllTreasures(TreasureRevealer __instance)
        {
            List<ItemDrop> itemDrop = Module<VoxelDungeonManager>.Self.GetItemDrop(Module<ScenarioModule>.Self.CurScenarioNameForGeneration);
            List<RevealLightPoint> list = new List<RevealLightPoint>();
            List<Transform> dynamicPortalEnterPoint = Module<VoxelDungeonManager>.Self.GetDynamicPortalEnterPoint(Module<ScenarioModule>.Self.CurScenarioNameForGeneration);

            if (itemDrop == null && dynamicPortalEnterPoint == null)
            {
                Dbgl("no items!");
                return;
            }
            Dbgl($"got {itemDrop.Count} items!");

            tril.Clear();

            using (List<ItemDrop>.Enumerator enumerator3 = itemDrop.GetEnumerator())
            {
                while (enumerator3.MoveNext())
                {
                    ItemDrop t = enumerator3.Current;
                    list.Add(new RevealLightPoint(t.transform, LightPointType.Treasure));
                }
            }
            foreach (Transform t2 in dynamicPortalEnterPoint)
            {
                list.Add(new RevealLightPoint(t2, LightPointType.PortalEnter));
            }

            string output = "";
            for (int l = 0; l < list.Count; l++)
            {
                RevealLightPoint rlp = list[l];
                TreasureRevealerItem component;
                if (rlp.type == LightPointType.Treasure)
                {
                    component = UnityEngine.Object.Instantiate<GameObject>(__instance.template).GetComponent<TreasureRevealerItem>();
                }
                else
                {
                    component = UnityEngine.Object.Instantiate<GameObject>(__instance.templatePortal).GetComponent<TreasureRevealerItem>();
                }
                string name = string.Empty;
                ItemDrop itemDrop2 = itemDrop.Find((ItemDrop itr) => itr.transform == rlp.transf);
                if (null != itemDrop2)
                {
                    name = itemDrop2.Item.ItemBase.Name;
                    output += name + "\r\n";
                }

                float maxDistance = Module<TreasureRevealerManager>.Self.property.detectRange * settings.RangeMult;
                component.Init(rlp.transf, TreasureRevealerItem.ModeEnum.Preview, maxDistance, name, null);

                tril.Add(component);
            }
            Dbgl(output);

        }

        [HarmonyPatch(typeof(TreasureRevealer), "RefreshTreasureRevealerItem")]
        static class RefreshTreasureRevealerItem_Patch
        {
            static bool Prefix(TreasureRevealer __instance)
            {
                if (!enabled)
                    return true;

                if (tril.Count == 0)
                {
                    GetAllTreasures(__instance);
                }

                float maxDistance = Module<TreasureRevealerManager>.Self.property.detectRange * settings.RangeMult;

                TreasureRevealerItem.ModeEnum mEnum = (!Module<TreasureRevealerManager>.Self.property.showBorder) ? TreasureRevealerItem.ModeEnum.Selection : TreasureRevealerItem.ModeEnum.Border;

                for (int i = 0; i < tril.Count; i++)
                {
                    TreasureRevealerItem tri = tril[i];
                    if (tri != null && tri.gameObject != null && Vector3.Distance(Module<Player>.Self.GamePos, tri.pos) <= maxDistance)
                    {
                        tri.ResetState(mEnum, Module<TreasureRevealerManager>.Self.property.showName);
                    }
                    else
                    {
                        tri.ResetState(TreasureRevealerItem.ModeEnum.Preview, false);
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(TreasureRevealer), "OnDestroy")]
        static class TreasureRevealer_OnDestroy_Patch
        {
            static void Postfix(TreasureRevealer __instance)
            {
                if (!enabled)
                    return;

                Dbgl("destroying");
                for (int i = 0; i < tril.Count; i++)
                {
                    TreasureRevealerItem tri = tril[i];
                    if (tri != null && tri.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(tri.gameObject);
                        UnityEngine.Object.Destroy(tri);
                    }
                }
                tril.Clear();
            }
        } 
    }
}

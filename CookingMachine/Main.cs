using Harmony12;
using Pathea;
using Pathea.CompoundSystem;
using Pathea.HomeNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace CookingMachine
{
    public static class Main
    {
        public static bool enabled;
        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "CookingMachine " : "") + str);
        }
        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUpdate = OnUpdate;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }


        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {

        }


        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {

        }

        [HarmonyPatch(typeof(CompoundManager), "InitCompoundData")]
        static class CompoundManager_GetSourceData
        {
            static void Postfix(ref List<CompoundItemData> ___sourceData, ref CompoundTable[] ___m_compoundTable)
            {
                if (!enabled)
                    return;

                CompoundItemData cidd = ___sourceData.Find((CompoundItemData c) => c.ItemID == 2000046);

                int start = 5000;
                int start2 = 5001000;
                List<CookMenuData> datas = Singleton<CookMenuMgr>.Instance.DataList;
                foreach(CookMenuData data in datas)
                {
                    CompoundItemData cid = cidd;
                    cid.ID = start++;
                    cid.NameID = Module<ItemDataMgr>.Self.GetItemNameId(data.Food);
                    cid.ItemID = data.Food;
                    cid.BookId = start2++;
                    if (data.Mats.Count > 0 && ItemObject.CreateItem(data.Mats[0].ID) != null)
                    {
                        cid.RequireItem1 = data.Mats[0].ID;
                        cid.RequireItemNum1 = data.Mats[0].Num;
                        if (data.Mats.Count > 1 && ItemObject.CreateItem(data.Mats[1].ID) != null)
                        {
                            cid.RequireItem2 = data.Mats[1].ID;
                            cid.RequireItemNum2 = data.Mats[1].Num;
                            if (data.Mats.Count > 2 && ItemObject.CreateItem(data.Mats[2].ID) != null)
                            {
                                cid.RequireItem3 = data.Mats[2].ID;
                                cid.RequireItemNum3 = data.Mats[2].Num;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                    Dbgl("adding to "+ (int)cidd.CompoundType);
                    CompoundItem ci = new CompoundItem(cid);
                    ___m_compoundTable[(int)cidd.CompoundType].AddItem(ci);
                    //___sourceData.Add(cid);
                }
            }

        }
    }
}
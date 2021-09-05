using Harmony12;
using Pathea;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace InventoryResize
{
    public partial class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        public static List<int> vacuumed;
        public static int pages = 3;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
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

            GUILayout.Label(string.Format("Inventory Pages: <b>{0}</b>", pages), new GUILayoutOption[0]);
            pages = (int)GUILayout.HorizontalSlider(pages, 3f, 100f, new GUILayoutOption[0]);
            if (GUILayout.Button("Resize", new GUILayoutOption[0]) && Module<Player>.Self?.bag != null)
            {
                Dbgl("Resizing");

                ItemBag bag = Module<Player>.Self.bag;
                ItemTable[] table = (ItemTable[])typeof(ItemBag).GetField("itemTables", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bag);
                for (int i = 0; i < table.Length; i++)
                {
                    ItemTable t = table[i];
                    TableSlot[] slots = (TableSlot[])typeof(ItemTable).GetField("slots", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(t);
                    TableSlot[] newslots = new TableSlot[40 * pages];
                    for (int j = 0; j < 40 * pages; j++)
                    {
                        if (j < slots.Length)
                            newslots[j] = slots[j];
                        else
                            newslots[j] = new TableSlot();
                    }
                    Dbgl($"Replacing table {i}");
                    typeof(ItemTable).GetField("slots", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(table[i], newslots);
                }
                Dbgl($"Replacing tables");
                typeof(ItemBag).GetField("itemTables", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Module<Player>.Self.bag, table);

            }
            if (GUILayout.Button("Unlock", new GUILayoutOption[0]))
            {
                Dbgl("Unlocking");

                Module<Player>.Self.bag.UnlockSlotByRow(500, false);
            }
        }

    }
}

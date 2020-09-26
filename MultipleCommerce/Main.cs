using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using Harmony12;
using Pathea.Missions;
using Pathea.ModuleNs;

namespace MultipleCommerce
{
    public static partial class Main
    {

        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref?"Multiple Commerce ":"") + str);
        }
        public static bool enabled;

        public static Settings settings { get; private set; }

        private static int DIVERSE_ID_COUNTER = 42000;
        private static readonly int FISH_GROUP_ID = 424242;

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Maximum total number of commerce orders available per day: <b>{0:F0}</b>", settings.NumberCommerceOrders), new GUILayoutOption[0]);
            settings.NumberCommerceOrders = (int)GUILayout.HorizontalSlider((float)Main.settings.NumberCommerceOrders, 1f, 20f, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Maximum number of <b>big</b> commerce orders available per day: <b>{0:F0}</b>", settings.NumberBigOrders), new GUILayoutOption[0]);
            settings.NumberBigOrders = (int)GUILayout.HorizontalSlider((float)Main.settings.NumberBigOrders, 1f, 10f, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Maximum number of <b>special</b> commerce missions available per day: <b>{0:F0}</b>", settings.NumberSpecialOrders), new GUILayoutOption[0]);
            settings.NumberSpecialOrders = (int)GUILayout.HorizontalSlider((float)Main.settings.NumberSpecialOrders, 1f, 10f, new GUILayoutOption[0]);
            GUILayout.Space(20);
            settings.MoreDiverseOrders = GUILayout.Toggle(settings.MoreDiverseOrders, "More diverse commerce orders (experimental)", new GUILayoutOption[0]);
            GUILayout.Space(20);
            settings.RandomOrderLevel = GUILayout.Toggle(settings.RandomOrderLevel, "Randomize commerce mission level (experimental)", new GUILayoutOption[0]);

            GUILayout.Space(20);
            GUILayout.Label("Use the buttons below to add missing orders to the order board. Warning! This is really experimental and should only be used if they haven't shown up as expected as a last resort! ", new GUILayoutOption[0]);
            if (GUILayout.Button("Assemble Dee-Dee Transport", new GUILayoutOption[]{
                GUILayout.Width(300f)
            }))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100302,5,0,"0",0,"0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100302, 7, 12052);
            }
            if (GUILayout.Button("The Bassanio Lift", new GUILayoutOption[]{
                GUILayout.Width(300f)
            }))
            {
                MissionBaseInfo missionBaseInfo = MissionManager.GetmissionBaseInfo(1100361);
                Module<MissionManager>.Self.DeliverMission(missionBaseInfo,0,"0",0,"0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100361, 7, 12052);
            }

            if (GUILayout.Button("Let There be Light", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100400, 4, 0, "0", 0, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100400, 7, 12052);
            }

            if (GUILayout.Button("The Portia Bridge 1", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100411, 4, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100411, 7, 12052);
            }

            if (GUILayout.Button("The Portia Bridge 2", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100417, 4, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100417, 7, 12052);
            }

            if (GUILayout.Button("The Portia Bridge 3", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100419, 4, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100419, 7, 12052);
            }

            if (GUILayout.Button("The Eufaula Tunnel", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100448, 4, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100448, 7, 12052);
            }

            if (GUILayout.Button("South Block Development 1", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100486, 2, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100486, 7, 12052);
            }

            if (GUILayout.Button("South Block Development 2", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100495, 1, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100495, 7, 12052);
            }

            if (GUILayout.Button("South Block Development 3", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100497, 1, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100497, 7, 12052);
            }

            if (GUILayout.Button("South Block Development 4", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100499, 1, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100499, 7, 12052);
            }

            if (GUILayout.Button("The Harbor Crane", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100535, 1, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100535, 7, 12052);
            }

            if (GUILayout.Button("The Portia Harbor", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100537, 5, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100537, 7, 12052);
            }

            if (GUILayout.Button("Long-Haul Bus", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100543, 2, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100543, 7, 12052);
            }

            if (GUILayout.Button("A Boat to Starlight", new GUILayoutOption[] {GUILayout.Width(300f)}))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100556, 1, 0, "0", 7, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100556, 7, 12052);
            }


        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
    }
}
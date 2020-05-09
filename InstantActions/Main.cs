using Harmony12;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace InstantActions
{
    public partial class Main
    {

        private static bool isDebug = false;

        public static void Dbgl(string str)
        {
            if(isDebug)
                Debug.Log("InstantActions " + str);
        }

        public static Settings settings { get; private set; }
        public static bool enabled;
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
            settings.InstantTreeKick = GUILayout.Toggle(settings.InstantTreeKick, "Instant Tree Kicking", new GUILayoutOption[0]);
            settings.InstantFarmPet = GUILayout.Toggle(settings.InstantFarmPet, "Instant Farm Animal Petting", new GUILayoutOption[0]);
            settings.InstantCookInput = GUILayout.Toggle(settings.InstantCookInput, "Instant Cooking Ingredient Adding", new GUILayoutOption[0]);
            settings.InstantFertilize = GUILayout.Toggle(settings.InstantFertilize, "Instant Fertilize Plants", new GUILayoutOption[0]);
            settings.InstantDeeDee = GUILayout.Toggle(settings.InstantDeeDee, "Instant Dee Dee travel", new GUILayoutOption[0]);
            settings.InstantWakeup = GUILayout.Toggle(settings.InstantWakeup, "Instant Wakeup (no alarm)", new GUILayoutOption[0]);
            settings.MoveWhileActing = GUILayout.Toggle(settings.MoveWhileActing, "Allow moving while acting (Drilling, etc.)", new GUILayoutOption[0]);
            settings.InstantText = GUILayout.Toggle(settings.InstantText, "Instant Dialog Text", new GUILayoutOption[0]);

            GUILayout.Label(string.Format("Hug Speed Multiplier : <b>{0:F1}x</b>", settings.HugSpeed), new GUILayoutOption[0]);
            settings.HugSpeed = GUILayout.HorizontalSlider((float)Main.settings.HugSpeed * 10f, 1f, 100f, new GUILayoutOption[0])/10f;

            GUILayout.Space(20);
            GUILayout.Label(string.Format("Kiss Speed Multiplier : <b>{0:F1}x</b>", settings.KissSpeed), new GUILayoutOption[0]);
            settings.KissSpeed = GUILayout.HorizontalSlider((float)Main.settings.KissSpeed * 10f, 1f, 60f, new GUILayoutOption[0]) / 10f;
            
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Massage Speed Multiplier : <b>{0:F1}x</b>", settings.MassageSpeed), new GUILayoutOption[0]);
            settings.MassageSpeed = GUILayout.HorizontalSlider((float)Main.settings.MassageSpeed * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
            
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Horse Petting Speed Multiplier : <b>{0:F1}x</b>", settings.HorseTouchSpeed), new GUILayoutOption[0]);
            settings.HorseTouchSpeed = GUILayout.HorizontalSlider((float)Main.settings.HorseTouchSpeed * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
            
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Pet Petting Speed Multiplier : <b>{0:F1}x</b>", settings.PetPetSpeed), new GUILayoutOption[0]);
            settings.PetPetSpeed = GUILayout.HorizontalSlider((float)Main.settings.PetPetSpeed * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
            
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Pet Embracing Speed Multiplier : <b>{0:F1}x</b>", settings.PetHugSpeed), new GUILayoutOption[0]);
            settings.PetHugSpeed = GUILayout.HorizontalSlider((float)Main.settings.PetHugSpeed * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
            
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Sowing / Gathering Speed Multiplier: <b>{0:F1}x</b>", settings.SowGatherSpeed), new GUILayoutOption[0]);
            settings.SowGatherSpeed = GUILayout.HorizontalSlider((float)Main.settings.SowGatherSpeed * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(20);
            GUILayout.Label(string.Format("Mining Speed Multiplier: <b>{0:F1}x</b>", settings.PickAxeSpeed), new GUILayoutOption[0]);
            settings.PickAxeSpeed = GUILayout.HorizontalSlider((float)Main.settings.PickAxeSpeed * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(20);
            GUILayout.Label(string.Format("Axe Speed Multiplier: <b>{0:F1}x</b>", settings.AxeSpeed), new GUILayoutOption[0]);
            settings.AxeSpeed = GUILayout.HorizontalSlider((float)Main.settings.AxeSpeed*10f, 1f, 100f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(20);
            GUILayout.Label(string.Format("Throw 1 Speed Multiplier: <b>{0:F1}x</b>", settings.Throw1Speed), new GUILayoutOption[0]);
            settings.Throw1Speed = GUILayout.HorizontalSlider((float)Main.settings.Throw1Speed * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
            
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Throw 2 Speed Multiplier: <b>{0:F1}x</b>", settings.Throw2Speed), new GUILayoutOption[0]);
            settings.Throw2Speed = GUILayout.HorizontalSlider((float)Main.settings.Throw2Speed * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
            //GUILayout.Label(string.Format("Rock,Paper, Scissors Speed Multiplier : <b>{0:F0}x</b>", settings.RPCSpeed), new GUILayoutOption[0]);
            //settings.RPCSpeed = (int)GUILayout.HorizontalSlider((float)Main.settings.RPCSpeed, 1f, 100f, new GUILayoutOption[0]);

            //settings.InstantWorkshopCollect = GUILayout.Toggle(settings.InstantWorkshopCollect, "Instant Product Collection", new GUILayoutOption[0]);
        }
    }
}

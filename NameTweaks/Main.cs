using Harmony12;
using Pathea.GreenBarNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.CustomActor;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace NameTweaks
{
    public class Main
    {
        private static readonly bool isDebug = true;
        private static bool enabled = true;
        private static string[] strings;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
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
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
        }


        [HarmonyPatch(typeof(HomeNameEdit), "Start")] 
        static class HomeNameEdit_Patch
        {
            static void Prefix(ref TMP_InputField ___inputField)
            {
                if (!enabled)
                    return;
                ___inputField.characterLimit = 9999;
            }
        }

        [HarmonyPatch(typeof(GreenBarModule), nameof(GreenBarModule.HasDirtyWord))]
        static class HasDirtyWord_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!enabled)
                    return true;
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(CustomActorUI), "Start")]
        static class CustomActorUI_Patch
        {
            private static TMP_InputField.OnChangeEvent onValueChanged;

            static void Prefix(ref TMP_InputField ___nameInput)
            {
                if (!enabled)
                    return;
                ___nameInput.characterLimit = 9999;
            }
        }

        [HarmonyPatch(typeof(CustomActorUI), "SetName")]
        static class CustomActorUI_SetName_Patch
        {
            static void Prefix(string str, ref TMP_InputField ___nameInput)
            {
                if (!enabled)
                    return;
                ___nameInput.characterLimit = 9999;
            }

        }

        [HarmonyPatch(typeof(HomeNameEdit), "SetName")]
        static class HomeNameEdit_SetName_Patch
        {
            static void Prefix(string str, ref TMP_InputField ___inputField)
            {
                if (!enabled)
                    return;
                ___inputField.characterLimit = 9999;
            }

        }

        [HarmonyPatch(typeof(NameLengthCheck), "GetSplitName")]
        static class NameLengthCheck_Patch
        {
            static void Prefix(ref TMP_InputField ___input, ref int ___characterLimit)
            {
                if (!enabled)
                    return;
                Dbgl("Checking name length");
                ___characterLimit = 9999;
                ___input.characterLimit = 9999;
            }

        }

    }
}

using Harmony12;
using Pathea;
using Pathea.AudioNs;
using Pathea.MiniGameNs.Fishing;
using Pathea.ModuleNs;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityModManagerNet;

namespace HereFishy
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        public static List<int> vacuumed;
        public static int pages = 3;
        private static AudioClip audioClip;
        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Singleton<TaskRunner>.Self.StartCoroutine(PreloadClipCoroutine());
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
        public static IEnumerator PreloadClipCoroutine()
        {
            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\herefishy.wav";

            string filename = "file:///" + path.Replace("\\", "/");

            Dbgl($"filename: {filename}");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filename, AudioType.WAV))
            {

                www.SendWebRequest();
                yield return null;
                //Dbgl($"checking downloaded {filename}");
                if (www != null)
                {
                    //Dbgl("www not null. errors: " + www.error);
                    DownloadHandlerAudioClip dac = ((DownloadHandlerAudioClip)www.downloadHandler);
                    if (dac != null)
                    {
                        AudioClip ac = dac.audioClip;
                        if (ac != null)
                        {
                            Dbgl("audio clip is not null. samples: " + ac.samples);
                            audioClip = ac;
                        }
                        else
                        {
                            Dbgl("audio clip is null. data: " + dac.text);
                        }
                    }
                    else
                    {
                        Dbgl("DownloadHandler is null. bytes downloaded: " + www.downloadedBytes);
                    }
                }
                else
                {
                    Dbgl("www is null " + www.url);
                }
            }
        }
        [HarmonyPatch(typeof(FishingSystem_t), "WaitForFish")]
        static class FishingSystem_t_WaitForFish_Patch
        { 
            static bool Prefix(FishingSystem_t __instance)
            {
                if (!enabled)
                    return true;
                Dbgl("Wait for fish");

                AudioPlayer.Self.PlayVoice(audioClip, true, Player.Self.actor.transform);
                Singleton<TaskRunner>.Self.RunDelayTask(audioClip.length, true, delegate
                {
                    typeof(FishingSystem_t).GetMethod("FishingEnd", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { true, false });
                });

                return false;
            }
        }
    }
}

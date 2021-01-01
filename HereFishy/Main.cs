using Harmony12;
using Pathea;
using Pathea.ACT;
using Pathea.AudioNs;
using Pathea.CameraSystemNs;
using Pathea.ConfigNs;
using Pathea.MiniGameNs;
using Pathea.MiniGameNs.Fishing;
using Pathea.ModuleNs;
using Pathea.PlayerOperateNs;
using System;
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
        private static AudioClip fishyClip;
        private static AudioClip weeClip;
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
            GUILayout.Label(string.Format("Jump Height: <b>{0:F1}</b>", settings.JumpHeight), new GUILayoutOption[0]);
            settings.JumpHeight = GUILayout.HorizontalSlider(settings.JumpHeight * 10f, 0f, 40f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Jump Speed: <b>{0:F1}</b>", settings.JumpSpeed), new GUILayoutOption[0]);
            settings.JumpSpeed = GUILayout.HorizontalSlider(settings.JumpSpeed * 10f, 1f, 10f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);
            settings.PlayHereFishy = GUILayout.Toggle(settings.PlayHereFishy, "Play 'Here Fishy!' call", new GUILayoutOption[0]);
            GUILayout.Space(10);
            settings.PlayWee = GUILayout.Toggle(settings.PlayWee, "Play weeeee sound", new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label(string.Format("Audio volume: <b>{0:F1}</b>", settings.Volume), new GUILayoutOption[0]);
            settings.Volume = GUILayout.HorizontalSlider(settings.Volume * 10f, 1f, 10f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);
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
                            fishyClip = ac;
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
            path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\wee.wav";

            filename = "file:///" + path.Replace("\\", "/");

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
                            weeClip = ac;
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
        private static int origBaitID = -1;

        [HarmonyPatch(typeof(FishingSystem_t), "WaitForFish")]
        static class FishingSystem_t_WaitForFish_Patch
        {
            private static Vector3 origPos;
            private static Vector3 flatPos;

            static bool Prefix(FishingSystem_t __instance, FishInfo ___fishInfo, Transform ___shoal, IFishingMan ___fishingMan, ref MoveArea ___area, FishingInfo ___fishingAreaInfo, float ___fishSpeedBaseFactor, float ___fishDashBaseFactor, float ___fishWaitBaseFactor, FishingUI_t ___hud)
            {
                if (!enabled)
                {
                    if (origBaitID != -1)
                        OtherConfig.Self.BaitID = origBaitID;
                        return true;
                }
                Dbgl("Wait for fish");

                if(OtherConfig.Self.BaitID != -1)
                {
                    origBaitID = OtherConfig.Self.BaitID;
                    OtherConfig.Self.BaitID = -1;
                }

                Fish_t fish = null;
                if (string.IsNullOrEmpty(___fishInfo.fishPrefabPath))
                {
                    UnityEngine.Debug.LogError("fishPrefabPath = is null!");
                }
                else
                {
                    GameObject original = Singleton<ResMgr>.Instance.LoadSync<GameObject>(___fishInfo.fishPrefabPath, false, false);
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, ___fishInfo.FishBornTrans.position, Quaternion.identity);
                    gameObject.transform.parent = ___shoal;
                    gameObject.transform.localScale = Vector3.one;
                    gameObject.SetActive(true);
                    fish = gameObject.GetComponent<Fish_t>();
                }
                if (fish != null)
                {
                    Vector3 vector = ___fishInfo.FishBornTrans.position - ___fishingMan.Pos;
                    ___area = new MoveArea();
                    ___area.angle = ___fishingAreaInfo.angleLimit / 2f;
                    ___area.dir = vector.normalized;
                    ___area.fishingManPos = ___fishingMan.Pos;
                    ___fishInfo.GenPowerFactor(fish.PowerValue);
                    ___fishInfo.GenSpeedFactor(fish.SpeedValue, ___fishSpeedBaseFactor);
                    ___fishInfo.GenAngerFactor(fish.AngerValue, ___fishDashBaseFactor);
                    ___fishInfo.GenTenacityFactor(fish.TenacityValue);
                    ___fishInfo.SetBaseWaitTime(___fishWaitBaseFactor);
                    fish.SetFishInfo(___fishInfo);
                }

                if (settings.PlayHereFishy && fishyClip != null)
                {
                    AudioSource audio = Player.Self.actor.gameObject.AddComponent<AudioSource>();
                    audio.volume = settings.Volume;
                    audio.clip = fishyClip;
                    audio.Play();
                    //AudioPlayer.Self.PlayVoice(fishyClip, false, CameraManager.Instance.SourceTransform);
                }

                Singleton<TaskRunner>.Self.RunDelayTask(fishyClip.length, true, delegate
                {
                    if (___hud == null)
                        return;

                    if (settings.PlayWee && weeClip != null)
                    {
                        AudioSource audio = Player.Self.actor.gameObject.AddComponent<AudioSource>();
                        audio.volume = settings.Volume;
                        audio.clip = weeClip;
                        audio.Play();
                    }

                    if (fish != null)
                    {
                        __instance.GetType().GetField("curFish", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, fish);
                        origPos = (__instance.GetType().GetField("curFish", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as Fish_t).gameObject.transform.position;
                        flatPos = origPos;
                        Singleton<TaskRunner>.Self.StartCoroutine(FishJump(__instance));
                    }
                    else
                    {
                        Module<Player>.Self.actor.TryDoAction(ACType.Animation, ACTAnimationPara.Construct("Throw_2", null, null, false));

                        typeof(FishingSystem_t).GetMethod("FishingEnd", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { true, false });
                    }
                    //__instance.GetType().GetField("curFish", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, __instance.GetType().GetMethod("CreateFish", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[0]));
                    //typeof(FishingSystem_t).GetMethod("FishingBegin", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { });
                });

                return false;
            }

            private static IEnumerator FishJump(FishingSystem_t instance)
            {
                for (; ; )
                {
                    Fish_t fish = (Fish_t)instance.GetType().GetField("curFish", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);

                    flatPos = Vector3.MoveTowards(flatPos, Module<Player>.Self.actor.gamePos, settings.JumpSpeed);

                    Vector3 playerPos = Module<Player>.Self.actor.gamePos;

                    float travelled = Vector3.Distance(flatPos, origPos);
                    float total = Vector3.Distance(playerPos, origPos);

                    float height = (float)Math.Sin(travelled * Math.PI / total) * settings.JumpHeight;

                    try
                    {
                        fish.gameObject.transform.position = new Vector3(flatPos.x, flatPos.y + height, flatPos.z);
                    }
                    catch
                    {
                        break;
                    }


                    if (Vector3.Distance(Module<Player>.Self.actor.gamePos, fish.gameObject.transform.position) < settings.JumpSpeed * 20)
                    {
                        Module<Player>.Self.actor.TryDoAction(ACType.Animation, ACTAnimationPara.Construct("Throw_2", null, null, false));
                    }

                    if (Vector3.Distance(Module<Player>.Self.actor.gamePos, fish.gameObject.transform.position) < settings.JumpSpeed)
                    {

                        typeof(FishingSystem_t).GetMethod("FishingEnd", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, new object[] { true, false });
                        break;
                    }
                    yield return null;
                }
            }
        }

        [HarmonyPatch(typeof(FishingUI_t), "ShowPoleTips")]
        static class FishingUI_t_ShowPoleTips_Patch
        { 
            static bool Prefix()
            {
                if (!enabled)
                    return true;

                return false;
            }
        }

        [HarmonyPatch(typeof(FishingSystem_t), "Update")]
        static class FishingSystem_t_Update_Patch
        { 
            static bool Prefix()
            {
                if (!enabled)
                    return true;

                return false;
            }
        }
        
        [HarmonyPatch(typeof(FishingSystem_t), "ResetFishingPole")]
        static class FishingSystem_t_ResetFishingPole_Patch
        { 
            static bool Prefix()
            {
                if (!enabled)
                    return true;

                return false;
            }
        }
        
        [HarmonyPatch(typeof(FishingInteraction), "CheckCanStartFish")]
        static class FishingInteraction_CheckCanStartFish_Patch
        { 
            static bool Prefix(ref bool __result)
            {
                if (!enabled || Module<Player>.Self.actor.IsActionRunning(ACType.Fish) || (!Module<FishingMatchMgr>.Self.IsInMatch && !Module<PlayerOperateModule>.Self.CheckStaminaAndSendTooltip(OperateType.Fishing, 1, false)))
                    return true;

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(FishingInteraction), "CheckCanContinueFish")]
        static class FishingInteraction_CheckCanContinueFish_Patch
        { 
            static bool Prefix(ref bool __result)
            {
                if (!enabled || Module<Player>.Self.actor.IsActionRunning(ACType.Fish) || (!Module<FishingMatchMgr>.Self.IsInMatch && !Module<PlayerOperateModule>.Self.CheckStaminaAndSendTooltip(OperateType.Fishing, 1, false)))
                    return true;

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(FishingInteraction), nameof(FishingInteraction.HasFishBait))]
        static class FishingInteraction_HasFishBait_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!enabled)
                    return true;

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(FishingInteraction), nameof(FishingInteraction.HasFishPole))]
        static class FishingInteraction_HasFishPole_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!enabled)
                    return true;

                __result = true;
                return false;
            }
        }

    }
}

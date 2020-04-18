using Harmony12;
using Pathea;
using Pathea.ACT;
using Pathea.ActorNs;
using Pathea.AudioNs;
using Pathea.BlackBoardNs;
using Pathea.FavorSystemNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using Pathea.ScenarioNs;
using PatheaScriptExt;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace PennyComeBack
{
    public partial class Main
    {
        public static bool enabled;
        public static Settings settings { get; private set; }
        public static readonly int PennyID = 4000141;
        public static readonly int EmilyID = 4000003;
        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            harmony.Patch(AccessTools.Method(typeof(AudioPlayer), nameof(AudioPlayer.ResumeBGM)), new HarmonyMethod(typeof(Main).GetMethod("AudioPlayer_ResumeBGM_Patch_Prefix")));

            MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(OnWakeUp));
            SceneManager.activeSceneChanged += Main.ActiveSceneChanged;
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.ReplaceMusic = GUILayout.Toggle(settings.ReplaceMusic, "Replace outdoor background music with Penny's song", new GUILayoutOption[0]);
            GUILayout.Label(string.Format("Music Volume %: <b>{00:F0}</b>", settings.MusicVolume*100), new GUILayoutOption[0]);
            settings.MusicVolume = GUILayout.HorizontalSlider(Main.settings.MusicVolume, 0f, 3f, new GUILayoutOption[0]);
            GUILayout.Label(string.Format("Music positional %: <b>{00:F0}</b>", settings.SpatialBlend*100), new GUILayoutOption[0]);
            settings.SpatialBlend = GUILayout.HorizontalSlider(Main.settings.SpatialBlend, 0f, 1f, new GUILayoutOption[0]);
            GUILayout.Label("Music falloff distance: <b>"+ settings.MusicDistance+"</b>", new GUILayoutOption[0]);
            settings.MusicDistance = (int)GUILayout.HorizontalSlider((float)Main.settings.MusicDistance, 1f, 10000f, new GUILayoutOption[0]);
        }
        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static void OnWakeUp(object[] obj)
        {
            if (!enabled)
                return;

            if (Module<GlobleBlackBoard>.Self.HasInfo("pennyleave"))
            {
                if (!Module<GlobleBlackBoard>.Self.HasInfo("pennycomebackagain"))
                {
                    Module<GlobleBlackBoard>.Self.SetInfo("pennycomebackagain", "1");
                    Module<FavorManager>.Self.GainFavorValue(PennyID, -1, 600);
                }

                Vector3 position = new Vector3(225.5f, 48.0f, -94.5f);
                Vector3 rot = new Vector3(0f, -90f, 0f);
                Actor actor = StoryHelper.GetActor(PennyID, string.Empty);

                //Module<SceneItemManager>.Self.CreateWithSceneFlag((SceneItemType)0, 168510, "Mission/Mission_pennyshow", "Main", "show_taizi", "0", false, AssetType.Mission);

                if (actor == null)
                {
                    actor = Module<NpcRepository>.Self.CreateNpc(4000141);
                }
                if (actor != null)
                {
                    actor.ClearActionQueue();
                    actor.DoCommand(ACType.Transfer, ACTransferPara.Construct("Main", position, rot));
                    actor.gamePos = position;
                    //actor.SetAiActive(true);
                    actor.DoCommand(ACType.Animation, ACTAnimationPara.Construct("AlwaysGuitar", null, null, false));
                    //actor.SetBehaviorState(BehaveState.Default);
                }

            }
        }

        private static AudioSource audio = null;

        private static void ActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            if (!enabled)
                return;

            if (newScene.name == "Main" && settings.ReplaceMusic)
            {

                if (Module<GlobleBlackBoard>.Self != null && Module<GlobleBlackBoard>.Self.HasInfo("pennyleave"))
                {
                    Actor actor = StoryHelper.GetActor(4000141, string.Empty);
                    if (actor == null)
                        return;
                    GameObject gameObject = actor.gameObject;
                    audio = actor.gameObject.AddComponent<AudioSource>();
                }
                else
                {
                    audio = Player.Self.actor.gameObject.AddComponent<AudioSource>();
                }
                audio.dopplerLevel = 0;
                audio.volume = settings.MusicVolume;
                audio.maxDistance = settings.MusicDistance;
                audio.minDistance = 1;
                audio.loop = true;
                audio.spatialBlend = settings.SpatialBlend;
                audio.rolloffMode = AudioRolloffMode.Logarithmic;

                AudioClip audioClip = Singleton<ResMgr>.Instance.LoadSync<AudioClip>("Audio/Music/Wanderer\u2019sDream", false, false);
                if (audioClip == null)
                    Dbgl("audio clip is null");
                else
                {
                    audio.clip = audioClip;
                    audio.Play();
                }
                //Module<AudioPlayer>.Self.PlayEffect2D(52115, false, true, false);

            }
            else if(audio != null)
                    audio.Stop();
        }

        public static bool AudioPlayer_ResumeBGM_Patch_Prefix()
        {
            if (!enabled || Player.Self == null)
                return true;

            if (Module<ScenarioModule>.Self.CurrentScenarioName == "Main" && settings.ReplaceMusic)
                return false;
            return true;
        }
        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "PennyComeBack " : "") + str);
        }
    }
}

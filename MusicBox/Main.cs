using Harmony12;
using Pathea;
using Pathea.AudioNs;
using Pathea.ModuleNs;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityModManagerNet;

namespace MusicBox
{
    public static partial class Main
    {
        public static bool enabled;

        public static Settings settings { get; private set; }

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "MusicBox " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PreloadAudioClips();
            //MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(OnWakeUp));

        }
        private static void OnWakeUp(object[] obj)
        {
            AudioSource[] ass = AccessTools.FieldRefAccess<AudioPlayer, AudioSource[]>(Module<AudioPlayer>.Self, "effects3D");
            for(int i = 0; i < ass.Length; i++) 
            {
                AudioSource aas = ass[i];
                if(aas.gameObject.name.Contains(MusicBoxAudioIDs[0].ToString()) || aas.gameObject.name.Contains(MusicBoxAudioIDs[1].ToString()))
                {
                    AccessTools.FieldRefAccess<AudioPlayer, AudioSource[]>(Module<AudioPlayer>.Self, "effects3D")[i].Stop();
                    AccessTools.FieldRefAccess<AudioPlayer, AudioSource[]>(Module<AudioPlayer>.Self, "effects3D")[i].clip = GetAudioToPlay();
                    AccessTools.FieldRefAccess<AudioPlayer, AudioSource[]>(Module<AudioPlayer>.Self, "effects3D")[i].Play();
                }
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Audio Volume: <b>{0:F1}</b>", settings.MusicVolume), new GUILayoutOption[0]);
            settings.MusicVolume = GUILayout.HorizontalSlider(settings.MusicVolume*10f, 1f, 10f, new GUILayoutOption[0])/10f;

            GUILayout.Space(10f);

            if (GUILayout.Button("Reload Files", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                PreloadAudioClips();
            }
            if (GUILayout.Button("Shuffle Tracks", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                combinedAudio = null;
                audioClips.Shuffle();
            }
            GUILayout.Space(10f);
            settings.ShuffleOrder = GUILayout.Toggle(settings.ShuffleOrder, "Shuffle tracks every time", new GUILayoutOption[0]);
            settings.PlayAll = GUILayout.Toggle(settings.PlayAll, "Play all tracks", new GUILayoutOption[0]);
            settings.LoopAudio = GUILayout.Toggle(settings.LoopAudio, "Loop audio", new GUILayoutOption[0]);
            settings.SilenceAlarm = GUILayout.Toggle(settings.SilenceAlarm, "Silence wake up alarm (in case you keep your music box running in your bedroom)", new GUILayoutOption[0]);
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static List<AudioClip> audioClips = new List<AudioClip>();
        private static System.Random rand = new System.Random();
        private static string[] audioFiles;
        private static int nextIndex = 0;
        private static AudioClip combinedAudio = null;
        private static void PreloadAudioClips()
        {
            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\";
            audioFiles = Directory.GetFiles(path, "*.ogg");
            PreloadClips();
        }

        private static AudioClip GetAudioToPlay()
        {
            if (settings.ShuffleOrder)
            {
                combinedAudio = null;
                audioClips.Shuffle();
            }
            if (settings.PlayAll)
            {
                if (combinedAudio == null)
                    combinedAudio = CombineAudioClips(audioClips);

                return combinedAudio;
            }
            nextIndex %= audioClips.Count;
            return audioClips[nextIndex++];
        }

        public static IEnumerator WaitForSound(float wait, Vector3 pos)
        {
            yield return new WaitForSeconds(wait);
            if (IsPlayingEffect3D(MusicBoxAudioIDs[0]) || IsPlayingEffect3D(MusicBoxAudioIDs[1]))
            {
                Module<AudioPlayer>.Self.StopEffect3D(MusicBoxAudioIDs[0], pos, 0);
                Module<AudioPlayer>.Self.StopEffect3D(MusicBoxAudioIDs[1], pos, 0);
                Module<AudioPlayer>.Self.PlayEffect3D(MusicBoxAudioIDs[0], pos, false, 0);
            }
        }

        public static bool IsPlayingEffect3D(int id)
        {
            foreach (AudioSource audioSource in AccessTools.FieldRefAccess<AudioPlayer, AudioSource[]>(Module<AudioPlayer>.Self, "effects3D"))
            {
                AudioClip audio = AudioPlayer.GetAudio(id);
                if (audio != null && audioSource.clip != null && audioSource.clip.name == audio.name)
                {
                    return true;
                }
            }
            return false;
        }

        public static void PreloadClips()
        {
            audioClips.Clear();
            foreach (string file in audioFiles)
            {
                Singleton<TaskRunner>.Self.StartCoroutine(Coroutine(file));

            }
        }
        public static IEnumerator Coroutine(string filename)
        {
            var uri = new System.Uri(filename);

            //Dbgl(uri.AbsolutePath);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri.AbsolutePath, AudioType.OGGVORBIS))
            {
                www.SendWebRequest();
                yield return www;
                if (www != null)
                {
                    audioClips.Add(((DownloadHandlerAudioClip)www.downloadHandler).audioClip);
                }
            }
        }

        public class StaticCoroutine
        {
            private static StaticCoroutineRunner runner;

            public static Coroutine Start(IEnumerator coroutine)
            {
                EnsureRunner();
                return runner.StartCoroutine(coroutine);
            }

            private static void EnsureRunner()
            {
                if (runner == null)
                {
                    runner = new GameObject("[Static Coroutine Runner]").AddComponent<StaticCoroutineRunner>();
                    UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
                }
            }

            private class StaticCoroutineRunner : MonoBehaviour { }
        }



        public static AudioClip CombineAudioClips(List<AudioClip> clips)
        {
            if (clips == null || clips.Count == 0)
                return null;

            int length = 0;
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i] == null)
                    continue;

                length += clips[i].samples * clips[i].channels;
            }

            float[] data = new float[length];
            length = 0;
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i] == null)
                    continue;

                float[] buffer = new float[clips[i].samples * clips[i].channels];
                clips[i].GetData(buffer, 0);
                //System.Buffer.BlockCopy(buffer, 0, data, length, buffer.Length);
                buffer.CopyTo(data, length);
                length += buffer.Length;
            }

            if (length == 0)
                return null;

            AudioClip result = AudioClip.Create("Combine", length / 2, 2, 44100, false);
            result.SetData(data, 0);

            return result;
        }
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}

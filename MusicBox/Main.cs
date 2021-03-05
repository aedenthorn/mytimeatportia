using Harmony12;
using Pathea;
using Pathea.AudioNs;
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

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.ShuffleOrder = GUILayout.Toggle(settings.ShuffleOrder, "Shuffle tracks every time", new GUILayoutOption[0]);
            settings.PlayAll = GUILayout.Toggle(settings.PlayAll, "Play all tracks", new GUILayoutOption[0]);
            settings.LoopAudio = GUILayout.Toggle(settings.LoopAudio, "Loop audio", new GUILayoutOption[0]);
            settings.SilenceAlarm = GUILayout.Toggle(settings.SilenceAlarm, "Silence wake up alarm (in case you keep your music box running in your bedroom)", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            if (GUILayout.Button("Shuffle tracks now", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                combinedAudio = null;
                audioClips.Shuffle();
            }
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Audio Volume: <b>{0:F1}</b>", settings.MusicVolume), new GUILayoutOption[0]);
            settings.MusicVolume = GUILayout.HorizontalSlider(settings.MusicVolume * 10f, 1f, 10f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Min Attenuation Distance: <b>{0:F0}</b>", settings.MinDistance), new GUILayoutOption[0]);
            settings.MinDistance = GUILayout.HorizontalSlider(settings.MinDistance, 1f, 1000f, new GUILayoutOption[0]);

            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Max Attenuation Distance: <b>{0:F0}</b>", settings.MaxDistance), new GUILayoutOption[0]);
            settings.MaxDistance = GUILayout.HorizontalSlider(settings.MaxDistance, 1f, 1000f, new GUILayoutOption[0]);

            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Audio Spatiality: <b>{0:F1}</b>", settings.Spatiality), new GUILayoutOption[0]);
            settings.Spatiality = GUILayout.HorizontalSlider(settings.Spatiality * 10f, 0f, 10f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);
            settings.UseUncompressedWAVFiles = GUILayout.Toggle(settings.UseUncompressedWAVFiles, "Use uncompressed audio (Requires restart and you must have downloaded and unpacked the the optional zip file into MusicBox\\wav\\)", new GUILayoutOption[0]);
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
            string path;
            if (settings.UseUncompressedWAVFiles)
            {
                path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\wav\\";
                if (Directory.Exists(path))
                {
                    audioFiles = Directory.GetFiles(path, "*.wav");
                    PreloadClips();
                }
                else
                {
                    Dbgl("wav file directory not found at Mods\\MusicBox\\wav\\" );
                    path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\";
                    if (Directory.Exists(path))
                    {
                        audioFiles = Directory.GetFiles(path, "*.ogg");
                        PreloadClips();
                    }
                    else
                    {
                        Dbgl("asset file directory not found at Mods\\MusicBox\\assets\\");
                        audioFiles = new string[] { };
                    }
                }
            }
            else
            {
                path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\";
                if (Directory.Exists(path))
                {
                    audioFiles = Directory.GetFiles(path, "*.ogg");
                    PreloadClips();
                }
                else
                {
                    Dbgl("asset file directory not found at Mods\\MusicBox\\assets\\");
                    audioFiles = new string[] { };
                }
            }
        }

        private static IEnumerator PlayAudioCoroutine(AudioPlayer instance, Vector3 pos, int id, int hashCode)
        {
            AudioClip audioClip;

            if (settings.ShuffleOrder)
            {
                combinedAudio = null;
                audioClips.Shuffle();
            }
            if (settings.PlayAll)
            {
                if (combinedAudio == null)
                {
                    combinedAudio = CombineAudioClips(audioClips);
                    yield return null;
                }
                audioClip = combinedAudio;
            }
            else
            {
                nextIndex %= audioClips.Count;
                audioClip = audioClips[nextIndex++];
            }

            AudioSource audioSource = (AudioSource)instance.GetType().GetMethod("PlayEffect3D", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, new object[] { audioClip, pos, AudioPlayer.GetOutPut(id), settings.LoopAudio, 0f });
            audioSource.gameObject.name = id.ToString() + "-" + hashCode;
            audioSource.dopplerLevel = 0;
            audioSource.minDistance = settings.MinDistance;
            audioSource.maxDistance = settings.MaxDistance;
            audioSource.spatialBlend = settings.Spatiality;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.volume = settings.MusicVolume;
            yield break;
        }

        public static void PreloadClips()
        {
            audioClips.Clear();
            foreach (string file in audioFiles)
            {
                Singleton<TaskRunner>.Self.StartCoroutine(PreloadClipCoroutine(file));

            }
        }

        public static IEnumerator PreloadClipCoroutine(string filename)
        {
            filename = "file:///" + filename.Replace("\\","/");

            //Dbgl($"filename: {filename}");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filename, settings.UseUncompressedWAVFiles ? AudioType.WAV : AudioType.OGGVORBIS))
            {

                www.SendWebRequest();
                yield return null;
                //Dbgl($"checking downloaded {filename}");
                if (www != null)
                {
                    //Dbgl("www not null. errors: " + www.error);
                    DownloadHandlerAudioClip dac = ((DownloadHandlerAudioClip)www.downloadHandler);
                    if (dac != null )
                    {
                        AudioClip ac = dac.audioClip;
                        if(ac != null)
                        {
                            //Dbgl("audio clip is not null. samples: " + ac.samples);
                            audioClips.Add(ac);
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

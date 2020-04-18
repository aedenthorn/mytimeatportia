using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BehaviorDesigner.Runtime.Tasks;
using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.AudioNs;
using Pathea.BlackBoardNs;
using Pathea.ModuleNs;
using PatheaScriptExt;
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

        private static List<AudioClip> audioClips = new List<AudioClip>();
        private static PreloadAudio mPa = new PreloadAudio();
        private static System.Random rand = new System.Random();
        private static void PreloadAudioClips()
        {
            mPa.PreloadAllClips();
        }

















        private static List<PreloadAudio> mPaList = new List<PreloadAudio>();

        public static void StartMusic(Vector3 pos)
        {
            foreach (PreloadAudio mPa in mPaList)
            {
                if (mPa.mPos == pos)
                {
                    mPa.GetRandomAudio();
                    return;
                }
            }
            PreloadAudio nPa = new PreloadAudio();
            nPa.Start(pos);
            mPaList.Add(nPa);
        }
        public static void StopMusic(Vector3 pos)
        {
            foreach (PreloadAudio mPa in mPaList)
            {
                if (mPa.mPos == pos)
                {
                    mPa.StopMusic();
                }
            }
        }

        private class PreloadAudio : MonoBehaviour
        {
            private static float targetTime = 0f;
            public Vector3 mPos;
            private static AudioSource audio;
            private static AudioClip clip = null;
            private static bool playingAudio = false;
            public void Start(Vector3 pos)
            {
                mPos = pos;
                GetRandomAudio();
            }

            public void PreloadAllClips()
            {
                audioClips.Clear();
                string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\";
                string[] files = Directory.GetFiles(path, "*.wav");
                foreach(string file in files)
                {
                    StaticCoroutine.Start(Coroutine2(file));
                }
            }
            public AudioClip GetRandomAudio()
            {
                return audioClips[rand.Next(0, audioClips.Count)];
            }

            public void Update()
            {
                if (playingAudio)
                {
                    targetTime -= Time.deltaTime;

                    if (targetTime <= 0.0f)
                    {
                        playingAudio = false;
                        this.GetRandomAudio();
                    }
                }
                else
                {
                    if (clip != null)
                    {
                        PlayAudioClip(clip);
                    }
                }
            }
            public IEnumerator Coroutine(string filename)
            {
                var uri = new System.Uri(filename);

                Dbgl(uri.AbsolutePath);

                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri.AbsolutePath, AudioType.WAV))
                {
                    www.SendWebRequest();
                    yield return www;
                    if (www != null)
                    {
                        clip = null;
                        clip = DownloadHandlerAudioClip.GetContent(www);
                    }
                }
            }

            public IEnumerator Coroutine2(string filename)
            {
                var uri = new System.Uri(filename);

                Dbgl(uri.AbsolutePath);

                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri.AbsolutePath, AudioType.WAV))
                {
                    www.SendWebRequest();
                    yield return www;
                    if (www != null)
                    {
                        audioClips.Add(DownloadHandlerAudioClip.GetContent(www));
                    }
                }
            }

            private void PlayAudioClip(AudioClip audioClip)
            {
                audio = Player.Self.actor.gameObject.AddComponent<AudioSource>();
                audio.dopplerLevel = 0;
                //audio.volume = settings.MusicVolume;
                audio.maxDistance = 100f;
                audio.minDistance = 1;
                audio.loop = false;
                audio.spatialBlend = 50f;
                audio.rolloffMode = AudioRolloffMode.Logarithmic;

                if (audioClip == null)
                {
                    Start(mPos);
                }
                else
                {
                    targetTime = audioClip.length;
                    audio.clip = audioClip;
                    audio.Play();
                    playingAudio = true;
                }
            }

            internal void StopMusic()
            {
                if (audio != null && audio.isPlaying)
                {
                    audio.Stop();
                }
                playingAudio = false;
                targetTime = 0;
                clip = null;
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
    }
}

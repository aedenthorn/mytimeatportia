using Harmony12;
using Pathea.AudioNs;
using Pathea.ModuleNs;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using static Harmony12.AccessTools;

namespace MarriageMod
{
    public static partial class Main
    {
        // change kissing audio

        static FieldRef<AudioPlayer, AudioMixer> audioMixerByRef = FieldRefAccess<AudioPlayer, AudioMixer>("audioMixer");
        static float lastKissAudio = 0;

        [HarmonyPatch(typeof(AudioPlayer), "PlayEffect3D", new Type[] { typeof(AudioClip), typeof(Vector3), typeof(AudioMixerGroup), typeof(bool), typeof(float) })]
        static class AudioPlayer_PlayEffect3D_Patch
        {
            static void Prefix(ref AudioClip clip, AudioData ___tempData)
            {
                if (!enabled || !settings.KissSound)
                    return;
                if (___tempData.id == kissAudioId)
                {
                    Dbgl("3d kiss");
                    clip = kissAudioClip;
                }
            }
        }
        [HarmonyPatch(typeof(AudioPlayer), "PlayEffect2d", new Type[] { typeof(AudioClip), typeof(AudioMixerGroup), typeof(bool) })]
        static class AudioPlayer_PlayEffect2D_Patch
        {
            static bool Prefix(ref AudioClip clip, AudioMixerGroup output, AudioData ___tempData)
            {
                if (!enabled || !settings.KissSound || kissAudioClip == null || ___tempData == null || ___tempData.id == null)
                    return true;
                if(___tempData.id == kissAudioId) {

                    Dbgl("2d kiss");

                    float currentTime = Time.time;
                    if (currentTime - lastKissAudio < 1)
                    {
                        return false;
                    }
                    else
                    {
                        lastKissAudio = currentTime;
                    }

                    clip = kissAudioClip;
                    if (kissLocations.Count > 0)
                    {
                        AudioPlayer.Self.PlayEffect3D(kissAudioId, kissLocations[0]);
                        kissLocations.RemoveAt(0);
                        return false;
                    }
                    else
                        clip = kissAudioClip;
                }
                return true;
            }
        }


        static class AudioPlayer_GetAudioAsync_Patch
        {
            static void Prefix(ref int id, Action<AudioClip, AudioMixerGroup> action)
            {
                if (!enabled || !settings.KissSound || AudioPlayer.Self == null)
                    return;

                MethodInfo dynMethod = AudioPlayer.Self.GetType().GetMethod("GetAudioData", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
                AudioData ad = (AudioData)dynMethod.Invoke(AudioPlayer.Self, new object[] { id });
                if (ad == null)
                    return;
                if(ad.path == "Audio/Effects/Npc_Kiss" && kissAudioClip != null)
                {
                    Dbgl("Kiss audio id = " + id);
                    AudioMixer audioMixer = audioMixerByRef(AudioPlayer.Self); 
                    if (!string.IsNullOrEmpty(ad.output) && audioMixer != null)
                    {
                        AudioMixerGroup[] array = audioMixer.FindMatchingGroups(ad.output);
                        if (array != null && array.Length > 0)
                        {
                            AudioMixerGroup output = array[0];
                            action(kissAudioClip, output);
                            id = 0;
                        }
                    }
                }

            }
        }
    }
}
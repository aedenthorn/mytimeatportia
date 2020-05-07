using Harmony12;
using Hont.ExMethod.Collection;
using Pathea;
using Pathea.AudioNs;
using System;
using System.Reflection;
using UnityEngine;

namespace MusicBox
{
    public static partial class Main
    {


        private static int[] MusicBoxAudioIDs = {1028,1029};
        

        [HarmonyPatch(typeof(AudioPlayer), "PlayEffect2D", new Type[] { typeof(int), typeof(bool), typeof(bool), typeof(bool) })]
        static class AudioPlayer_PlayEffect2D_Patch
        {
            static bool Prefix(int id)
            {
                if (!enabled || !settings.SilenceAlarm)
                    return true;
                return id != 62;
            }
        }

        [HarmonyPatch(typeof(AudioPlayer), "PlayEffect3DInternal", new Type[] { typeof(int), typeof(Vector3), typeof(bool), typeof(int), typeof(float) })]
        static class AudioPlayer_PlayEffect3DInternal_Patch
        {
            static bool Prefix(AudioPlayer __instance, int id, Vector3 pos, ref AudioData ___tempData, int hashCode)
            {
                if (!enabled)
                    return true;
                if (MusicBoxAudioIDs.Contains(id))
                {
                    Singleton<TaskRunner>.Self.StartCoroutine(PlayAudioCoroutine(__instance, pos, id, hashCode));
                    return false;
                }
                return true;
            }
        }
    }
}
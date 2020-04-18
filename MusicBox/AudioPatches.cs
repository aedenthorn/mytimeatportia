using Harmony12;
using Pathea.AudioNs;
using Pathea.ModuleNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hont.ExMethod.Collection;
using Pathea;
using Pathea.HomeNs;
using Pathea.InputSolutionNs;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using static Harmony12.AccessTools;

namespace MusicBox
{
    public static partial class Main
    {


        private static int[] MusicBoxAudioIDs = {1028,1029};
        /*
        //[HarmonyPatch(typeof(RecordCtr), "Toggle")]
        static class RecordCtr_Toggle_Patch
        {
            static bool Prefix(ref bool ___isPlaying, PlayerTargetMultiAction ___playerTarget, Transform ___MusicTrans, int ___AudioID)
            {
                if (!enabled)
                    return true;

                ___isPlaying = !___isPlaying;
                if (___isPlaying)
                {
                    ___playerTarget.SetAction(ActionType.ActionInteract, 100024, ActionTriggerMode.Normal);
                    pa.Start(___MusicTrans.transform.position);
                }
                else
                {
                    ___playerTarget.SetAction(ActionType.ActionInteract, 100023, ActionTriggerMode.Normal);
                    pa.StopMusic();
                }

                return false;
            }
        }
        */

        //[HarmonyPatch(typeof(AudioPlayer), "StopEffect3D")]
        static class AudioPlayer_StopEffect3D_Patch
        {
            static void Prefix(int id, Vector3 pos)
            {
                if (!enabled)
                    return;
                if (MusicBoxAudioIDs.Contains(id))
                {
                    StopMusic(pos);
                    Dbgl("3d audio");
                }
            }
        }
        [HarmonyPatch(typeof(AudioPlayer), "PlayEffect3D", new Type[] { typeof(AudioClip), typeof(Vector3), typeof(AudioMixerGroup), typeof(bool), typeof(float) })]
        static class AudioPlayer_PlayEffect3D_Patch
        {
            static void Prefix(ref AudioClip clip, Vector3 pos, ref bool loop, AudioData ___tempData)
            {
                if (!enabled)
                    return;
                if (MusicBoxAudioIDs.Contains(___tempData.id))
                {
                    Dbgl("3d audio");
                    loop = false;
                    clip = mPa.GetRandomAudio();
                    
                }
            }
        }
        /*
        //[HarmonyPatch(typeof(AudioPlayer), "PlayEffect2d", new Type[] { typeof(AudioClip), typeof(AudioMixerGroup), typeof(bool) })]
        static class AudioPlayer_PlayEffect2D_Patch
        {
            static bool Prefix(ref AudioClip clip, AudioMixerGroup output, AudioData ___tempData)
            {
                if (!enabled || ___tempData == null || ___tempData.id == null)
                    return true;
                if (MusicBoxAudioIDs.Contains(___tempData.id))
                {
                    //SetAudioClip();
                    Dbgl("2d audio");
                    clip = mAudioClip;
                }
                return true;
            }
        }
        */
    }
}
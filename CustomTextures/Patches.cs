using Harmony12;
using Pathea.ActorNs;
using Pathea.AppearNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.NpcAppearNs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTextures
{
    public partial class Main
    {

        [HarmonyPatch(typeof(Actor), "Start")]
        private static class Actor_Start_Patch
        {
            private static void Postfix(Actor __instance, ref SkinnedMeshRenderer ___skinnedMeshRenderer)
            {

                if (customTextures.ContainsKey(__instance.InstanceId) && __instance.GetComponent<NpcAppear>() == null)
                {
                    Dbgl($"got texture for {__instance.ActorName}");
                    if (___skinnedMeshRenderer != null)
                        ___skinnedMeshRenderer.material.mainTexture = customTextures[__instance.InstanceId];
                }
            }
        }
        [HarmonyPatch(typeof(NpcAppear), "RebuildMesh", new Type[] { typeof(List<NpcAppearUnit>), typeof(Transform), typeof(SkinnedMeshRenderer[]) })]
        private static class NpcAppear_RebuildMesh_Patch
        {
            private static void Prefix(NpcAppear __instance, List<NpcAppearUnit> npcAppearUnits, Actor ___m_Actor)
            {
                if (___m_Actor == null)
                {
                    Dbgl($"no actor");
                    return;
                }
                if (npcAppearUnits == null)
                {
                    Dbgl($"no npcAppearUnits");
                    return;
                }
                if (customTextures.ContainsKey(___m_Actor.InstanceId))
                {
                    foreach (NpcAppearUnit unit in npcAppearUnits)
                    {
                        if (unit == null || unit.smrs == null)
                            continue;
                        //Dbgl($"got mesh for: {___m_Actor.ActorName}");
                        unit.smrs[0].material.mainTexture = customTextures[___m_Actor.InstanceId];
                    }
                }
                else if (customTexturesExact.ContainsKey(___m_Actor.InstanceId))
                {
                    foreach (NpcAppearUnit unit in npcAppearUnits)
                    {
                        if (unit == null || unit.smrs == null)
                            continue;
                        Dbgl($"mesh {unit.smrs[0].name}");
                        if (customTexturesExact[___m_Actor.InstanceId].ContainsKey(unit.smrs[0].name))
                        {
                            unit.smrs[0].material.mainTexture = customTexturesExact[___m_Actor.InstanceId][unit.smrs[0].name];
                        }
                    }
                }
                else if (customTexturesPartial.ContainsKey(___m_Actor.InstanceId))
                {
                    int i = 1;
                    foreach (NpcAppearUnit unit in npcAppearUnits)
                    {
                        if (unit == null || unit.smrs == null)
                            continue;
                        if (customTexturesPartial[___m_Actor.InstanceId].ContainsKey(i))
                        {
                            //Dbgl($"got mesh for: {___m_Actor.ActorName} at {i}");
                            unit.smrs[0].material.mainTexture = customTexturesPartial[___m_Actor.InstanceId][i];
                        }
                        i++;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AppearTarget), "BuildMesh", new Type[] { typeof(List<AppearUnit>), typeof(AppearData), typeof(AppearUnit), typeof(string) })]
        static class BuildMesh1_Patch
        {
            static void Prefix(ref List<AppearUnit> units)
            {
                //Dbgl($"Building player mesh");

                if (!enabled)
                    return;

                for (int i = 0; i < units.Count; i++)
                {
                    string name = units[i].name;
                    Dbgl($"appear part name: {name}");
                    if (units[i].Smr != null)
                    {

                        if (units[i].Smr.material.HasProperty("_MainTex") && customTexturesMisc.ContainsKey(name))
                        {
                            Dbgl($"Changing texture for {name}");
                            units[i].Smr.material.mainTexture = customTexturesMisc[name];
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(RegionViewer), "CreateGameObj", new Type[] { typeof(string), typeof(Area), typeof(ItemPutInfo), typeof(bool) })]
        static class RegionViewer_CreateGameObj_Patch
        {
            static void Postfix(GameObject __result)
            {
                if (!enabled)
                    return;

                FixOneTexture(__result);
            }
        }

        //[HarmonyPatch(typeof(HomeItemSelector), "CreatePreviewObj")]
        static class HomeItemSelector_ShowPreviewObj_Patch
        {
            static void Postfix(ref GameObject __result)
            {
                if (!enabled || __result == null)
                    return;

                FixOneTexture(__result);
            }
        }

    }
}

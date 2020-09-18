using System.Reflection;
using UnityModManagerNet;
using Harmony12;
using Pathea.FavorSystemNs;
using System;
using Pathea.NpcInstanceNs;
using UnityEngine;
using Pathea.ModuleNs;
using Pathea.ActorNs;
using System.Collections.Generic;
using Pathea.Missions;

namespace NPCNoDelete
{
    public static class Main
    {
        public static bool enabled;

        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            ReturnLocations[4000038] = new Vector3[] {new Vector3(-204.2401f, 79.45f, 13.4f),new Vector3(0f, 133.7f, 0f)};
            ReturnLocations[4000093] = new Vector3[] {new Vector3(15.4f, 53.46f, -106.9f),new Vector3(0, 188f, 0) };
            ReturnLocations[4000141] = new Vector3[] { SceneFlagManager.Instance.GetTransform("Main","penny_arrive").position, SceneFlagManager.Instance.GetTransform("Main", "penny_arrive").rotation.eulerAngles };
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Space(10f);
            settings.Aadit = GUILayout.Toggle(settings.Aadit, "Keep Aadit", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.Ginger = GUILayout.Toggle(settings.Ginger, "Keep Ginger", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.Penny = GUILayout.Toggle(settings.Penny, "Keep Penny", new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.Label("Attempt to manually bring back and give +100 favour:", new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            if (GUILayout.Button("Aadit", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                BringBack(4000038);
            }
            if (GUILayout.Button("+100", new GUILayoutOption[] { GUILayout.Width(150f) }))
            {
                IncreaseFavor(4000038);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            if (GUILayout.Button("Ginger", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                BringBack(4000093);

            }
            if (GUILayout.Button("+100", new GUILayoutOption[] { GUILayout.Width(150f) }))
            {
                IncreaseFavor(4000093);

            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            if (GUILayout.Button("Penny", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                BringBack(4000141);

            }
            if (GUILayout.Button("+100", new GUILayoutOption[]{GUILayout.Width(150f)}))
            {
                IncreaseFavor(4000141);

            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);
        }

        private static void IncreaseFavor(int id)
        {
            Module<FavorManager>.Self.GainFavorValue(id, -1, 100);
        }

        private static void BringBack(int id)
        {
            Actor actor = Module<ActorMgr>.Self.Get(id);
            actor.SetBehaviorState(BehaveState.Default, true);
            Module<ActorMgr>.Self.MoveToScenario(actor, "Main", ReturnLocations[id][0]);
            FavorObject f = Module<FavorManager>.Self.GetFavorObject(id);
            FavorRelationshipId r = f.Relationship;
            f.Relationship = FavorRelationshipId.Friend;
            FavorRelationshipUtil.TryUpgradeRelationShip(f, false, false, true);
            Module<FavorInfluenceManager>.Self.OnUpdateRelation(f.ID, r, f.Relationship);

            if (typeof(FavorManager).GetMethod("RemoveFromBlackList") != null)
            {
                typeof(FavorManager).GetMethod("RemoveFromBlackList").Invoke(Module<FavorManager>.Self, new object[] { f.ID });
            }
            else if (typeof(FavorManager).GetMethod("RemoveToBlackList") != null)
            {
                typeof(FavorManager).GetMethod("RemoveToBlackList").Invoke(Module<FavorManager>.Self, new object[] { f.ID });
            }
        }

        private static Dictionary<int, Vector3[]> ReturnLocations = new Dictionary<int, Vector3[]>();

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        [HarmonyPatch(typeof(FavorRelationshipUtil), "DeleteNpcFavor")]
        static class FavorRelationshipUtil_DeleteNpcFavor_Patch
        {
            static bool Prefix(int npcId)
            {
                if (!enabled)
                    return true;
                switch (npcId)
                {
                    case 4000038:
                        if (settings.Aadit)
                        {
                            return false;
                        }
                        break;
                    case 4000093:
                        if (settings.Ginger)
                        {
                            return false;
                        }
                        break;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(NpcInstanceModule), "MoveToFarAwayScene")]
        static class NpcInstanceModule_MoveToFarAwayScene_Patch
        {
            static bool Prefix(int instanceId)
            {
                if (!enabled)
                    return true;
                switch (instanceId)
                {
                    case 4000038:
                        if (settings.Aadit)
                        {
                            return false;
                        }
                        break;
                    case 4000093:
                        if (settings.Ginger)
                        {
                            return false;
                        }
                        break;
                    case 4000141:
                        if (settings.Penny)
                        {
                            return false;
                        }
                        break;
                }
                return true;
            }
        }

    }
}
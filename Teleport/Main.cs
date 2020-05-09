using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using Harmony12;
using Pathea.Missions;
using Pathea.GuildRanking;
using Pathea.ScenarioNs;
using Pathea.ModuleNs;
using UnityEngine.SceneManagement;
using Pathea.UISystemNs;
using UnityEngine.UI;
using Pathea;

namespace Teleport
{
    public static class Main
    {
        public static bool enabled;
        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "Teleport " : "") + str);
        }
        //public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            //settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            SceneManager.activeSceneChanged += ChangeScene;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void ChangeScene(Scene arg0, Scene arg1)
        {
            Dbgl("new scene: " + arg1.name);
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            //settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            return;
            if (GUILayout.Button("Outdoors", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("Main");
            }
            if (Module<ScenarioModule>.Self == null || Module<ScenarioModule>.Self.CurrentScenarioName != "Main")
                return;
            if (GUILayout.Button("Player Home", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("PlayerHome");
            }
            if (GUILayout.Button("Temple", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("Temple");
            }
            if (GUILayout.Button("Chamber of Commerce", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("chamber");
            }
            if (GUILayout.Button("A&G Construction", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("Company");
            }
            if (GUILayout.Button("Civil Corps", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("adventure");
            }
            if (GUILayout.Button("Dr. Xu's Clinic", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("clinic");
            }
            if (GUILayout.Button("Apartments", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("Apartment");
            }
            if (GUILayout.Button("Research Centre", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("science");
            }
            if (GUILayout.Button("Museum", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("Museum");
            }
            if (GUILayout.Button("School", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("school");
            }
            if (GUILayout.Button("Gale's House", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("Gale");
            }
            if (GUILayout.Button("Sophie's Farm", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("field");
            }
            if (GUILayout.Button("Southblock Motel", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("Yizhan");
            }
            if (GUILayout.Button("Abandoned Ruins 1", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("VoxelDungeon");
            }
            if (GUILayout.Button("Abandoned Ruins 2", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("VoxelDungeon2");
            }
            if (GUILayout.Button("Abandoned Ruins 3", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("VoxelDungeon3");
            }
            if (GUILayout.Button("Abandoned Ruins 4", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("VoxelDungeon4");
            }
            if (GUILayout.Button("Ghost Cave", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                ButtonTeleport("GhostCave");
            }
        }

        private static void ButtonTeleport(string scene)
        {
            UnityModManager.UI.Instance.ToggleWindow();
            //UIStateMgr.Instance.PopState(false);
            Teleport(scene);
        }

        private static void Teleport(string scene)
        {
            Teleport(scene, Vector3.zero);
        }
        private static void Teleport(string scene, Vector3 vector)
        {
            if(vector == Vector3.zero)
            {
                Module<ScenarioModule>.Self.TransferToScenario(scene);
            }
            else
            {
                if(Module<Player>.Self.actor.SceneName == scene)
                {
                    Module<Player>.Self.GamePos = vector;
                }
                else
                {
                    Module<ScenarioModule>.Self.TransferToScenario(scene, vector, Vector3.forward);
                }
            }
        }
        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static Vector3 nextMainPos;

        [HarmonyPatch(typeof(WholeMapViewer), "SetMarkIcon")]
        static class RefreshTreasureRevealerItem_Patch
        {

            static bool Prefix(WholeMapViewer __instance, Image iconImage, Image ___NowPlayingMap)
            {
                if (Input.GetKey("left shift"))
                {
                    Dbgl("Trying to teleport");
                    Vector3 v1 = iconImage.rectTransform.position;
                    Vector3 v2 = ___NowPlayingMap.rectTransform.InverseTransformPoint(v1);
                    Vector3 v3 = (Vector3)typeof(WholeMapViewer).GetMethod("ReverseConvertGamePosFromGameMap", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { new Vector3(v2.x, v2.y, 0f) });
                    Vector3 v4 = GetValidPos(v3);
                    //v4 += new Vector3(0, 20f, 0);
                    Dbgl($"Player Pos: {Module<Player>.Self.actor.gamePos}");
                    Dbgl($"Teleport Pos: {v3}");
                    Dbgl($"Safe Pos: {v3}");

                    UIStateMgr.Instance.PopState(false);
                    if (Module<ScenarioModule>.Self.CurrentScenarioName != "Main")
                    {
                        nextMainPos = v3;
                        SceneManager.activeSceneChanged += TeleportFromIndoors;
                        Teleport("Main");
                        return false;
                    }
                    Teleport("Main",v4);
                    return false;
                }
                return true;
            }

        }
        private static void TeleportFromIndoors(Scene arg0, Scene arg1)
        {
            if(arg1.name != "Main")
            {
                return;
            }
            SceneManager.activeSceneChanged -= TeleportFromIndoors;
            Vector3 v = GetValidPos(nextMainPos);
            Dbgl($"Safe Pos: {v}");
            Teleport("Main", v);
        }

        private static Vector3 GetValidPos(Vector3 pos)
        {
            Ray ray = default;
            RaycastHit[] hits = new RaycastHit[4];
            ray.origin = pos + Vector3.up * 1000f;
            ray.direction = Vector3.down;
            int num = Physics.RaycastNonAlloc(ray, hits, 999f, 256);
            for (int i = 0; i < num; i++)
            {
                RaycastHit raycastHit = hits[i];
                if (raycastHit.collider)
                {
                    return raycastHit.point;
                }
            }
            return Vector3.zero;
        }

    }
}
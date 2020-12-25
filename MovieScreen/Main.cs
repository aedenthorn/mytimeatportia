using Harmony12;
using Pathea;
using Pathea.CameraSystemNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.OptionNs;
using Pathea.UISystemNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Video;
using UnityModManagerNet;

namespace MovieScreen
{
    public class Main
    {
        private static readonly bool isDebug = true;
        private static bool enabled = true;
        public static Settings settings { get; private set; }

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
        }

        [HarmonyPatch(typeof(PhotoFrameCtr), "Start")]
        static class PhotoFrameCtr_Start_Patch
        {
            static void Postfix(PhotoFrameCtr __instance)
            {
                if (!enabled)
                    return;
                Dbgl($"starting photoframectr");

                MeshRenderer[] bbms = __instance.gameObject.GetComponentsInChildren<MeshRenderer>();

                MeshRenderer screenMr = null;
                foreach (MeshRenderer m in bbms)
                {
                    //Dbgl($"mesh: {m.name}");
                    if (m.name == "Item_Billboard_2") 
                    {
                        screenMr = m;
                        break;
                    }
                }

                if (screenMr == null)
                    return;

                GameObject tvt = Singleton<ResMgr>.Instance.LoadSyncByType<GameObject>(AssetType.Home, "HomeItem_TVTable");

                TVBenchUnitViewer tvtv = tvt.GetComponentInChildren<TVBenchUnitViewer>(true);
                /*
                CabinetUnit unit = new CabinetUnit(3031001);
                unit.PutCabinet(4030001, 1, out ItemObject item);
                typeof(TVBenchUnitViewer).GetMethod("SetUnitInternal", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tvtv, new object[] { unit });
                Dbgl($"Set Unit");
                */

                GameObject tvtr = UnityEngine.Object.Instantiate(tvt, __instance.gameObject.transform);
                //Dbgl($"Instantiated");


                ItemObject item = ItemObject.CreateItem(4030001);
                //Dbgl($"got item {item.ItemBase.Name}");

                GameUtils.ClearChildren(typeof(TVBenchUnitViewer).GetField("placeHolder", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tvtv) as GameObject, false);
                GameObject tvObject = GameUtils.AddChild(typeof(TVBenchUnitViewer).GetField("placeHolder", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tvtv) as GameObject, item.ItemBase.DropModelPath, false, AssetType.ItemSystem);
                TVCtr tvc = tvObject.GetComponentInChildren<TVCtr>(true);
                //Dbgl($"tvc is null? {tvc == null}");

                typeof(TVBenchUnitViewer).GetField("tVCtr", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tvtv, tvc); 

                MeshRenderer[] tvtms = tvtr.GetComponentsInChildren<MeshRenderer>();

                for (int i = 0; i < tvtms.Length; i++)
                {
                    MeshRenderer m = tvtms[i];
                    m.gameObject.SetActive(false);
                }

                MeshRenderer[] tvms = tvObject.GetComponentsInChildren<MeshRenderer>();

                for (int i = 0; i < tvms.Length; i++)
                {
                    MeshRenderer m = tvms[i];
                    m.gameObject.SetActive(false);
                }

                GameObject screen = typeof(TVCtr).GetField("screen", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tvc) as GameObject;
                screen.GetComponentInChildren<MeshFilter>().mesh = screenMr.GetComponentInChildren<MeshFilter>().mesh;

                //screen.transform.localScale = new Vector3(32f, 18f, 1f);

                PlayerTargetMultiAction CurPlayerTarget = (PlayerTargetMultiAction)typeof(UnitViewer).GetProperty("CurPlayerTarget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, null);

                CurPlayerTarget.SetAction(ActionType.ActionRoll, 103809, ActionTriggerMode.Normal);

            }
        }

        [HarmonyPatch(typeof(PhotoFrameCtr), "Handle")]
        static class PhotoFrameCtr_Handle_Patch
        {
            static bool Prefix(PhotoFrameCtr __instance, ActionType obj)
            {
                if (!enabled || obj != ActionType.ActionRoll)
                    return true;

                MeshRenderer[] bbms = __instance.gameObject.GetComponentsInChildren<MeshRenderer>();

                MeshRenderer screenMr = null;
                foreach (MeshRenderer m in bbms)
                {
                    //Dbgl($"mesh: {m.name}");
                    if (m.name == "Item_Billboard_2")
                    {
                        screenMr = m;
                        break;
                    }
                }

                if (screenMr == null)
                    return true;

                Dbgl($"handling photoframectr");
                TVBenchUnitViewer tvtv = __instance.gameObject.GetComponentInChildren<TVBenchUnitViewer>();
                TVCtr tvc = typeof(TVBenchUnitViewer).GetField("tVCtr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tvtv) as TVCtr;
                Action<string> action = delegate (string fileName)
                {
                    tvc.Play(fileName);
                };
                Transform t = typeof(TVBenchUnitViewer).GetField("camPivort", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tvtv) as Transform;
                t.position = new Vector3(__instance.transform.position.x, __instance.transform.position.y + 9.2f, __instance.transform.position.z) - t.forward * 8f;
                UIStateMgr.Instance.ChangeStateByType(UIStateMgr.StateType.TVPlayer, false, new object[]
                {
                    t,
                    action,
                    tvc.CurPlay
                });
                return false;
            }
        }

        [HarmonyPatch(typeof(TVBenchUnitViewer), "FreshItemShow")]
        static class TVBenchUnitViewer_FreshItems_Patch
        {
            static void Postfix(TVBenchUnitViewer __instance, GameObject ___placeHolder, ref TVCtr ___tVCtr)
            {
                if (!enabled || __instance.GetComponentInParent<PhotoFrameCtr>() == null)
                    return;
                Dbgl($"Fresh Items");
                ItemObject item = ItemObject.CreateItem(4030001);
                GameObject tvObject = GameUtils.AddChild(___placeHolder, item.ItemBase.DropModelPath, false, AssetType.ItemSystem);

                MeshRenderer[] tvms = tvObject.GetComponentsInChildren<MeshRenderer>();

                for (int i = 0; i < tvms.Length; i++)
                {
                    MeshRenderer m = tvms[i];
                    m.gameObject.SetActive(false);
                }

                ___tVCtr = tvObject.GetComponentInChildren<TVCtr>(true);
                AudioSource audioSource = typeof(TVCtr).GetField("audioSource", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(___tVCtr) as AudioSource;
                audioSource.spatialBlend = settings.Spatiality;
                audioSource.volume = 1f;
                typeof(TVCtr).GetField("audioSource", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(___tVCtr, audioSource);
                //VideoPlayer videoPlayer = typeof(TVCtr).GetField("videoPlayer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(___tVCtr) as VideoPlayer;
                //typeof(TVCtr).GetField("videoPlayer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(___tVCtr, videoPlayer);
                GameObject newObj = Singleton<ResMgr>.Instance.LoadSyncByType<GameObject>(AssetType.Home, "HomeItem_ADBoard");
                GameObject screen = typeof(TVCtr).GetField("screen", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(___tVCtr) as GameObject;
                MeshRenderer[] bbms = newObj.GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer m in bbms)
                {
                    //Dbgl($"mesh: {m.name}"); 
                    if (m.name == "Item_Billboard_2")
                    {
                        screen.GetComponentInChildren<MeshFilter>().mesh = m.GetComponentInChildren<MeshFilter>().mesh;
                        screen.GetComponentInChildren<MeshRenderer>().materials = m.materials;
                        //screen.GetComponentInChildren<Transform>().position = m.gameObject.GetComponentInChildren<Transform>().position;
                        break;
                    }
                }
                /*
                Component[] components = screen.GetComponents(typeof(Component));
                foreach (Component component in components)
                {
                    Dbgl($"component: {component.ToString()}");
                }
                */
                //Vector3 scale = screen.GetComponentInChildren<MeshFilter>().transform.localScale;
                //screen.GetComponentInChildren<MeshFilter>().transform.localScale = new Vector3(scale.x, 1.6f, scale.z);
                //Dbgl($"local scale: {screen.transform.localScale}");
                //screen.GetComponentInChildren<VideoPlayer>().aspectRatio = VideoAspectRatio.NoScaling;
                Vector3 pos = screen.GetComponentInChildren<MeshFilter>().transform.position;
                screen.transform.position = new Vector3(pos.x, pos.y - 0.84f, pos.z) - screen.GetComponentInChildren<MeshFilter>().transform.forward * 0.02f;
            }
        }


        [HarmonyPatch(typeof(TVBenchUnitViewer), "FreshActions")]
        static class TVBenchUnitViewer_FreshActions_Patch
        {
            static bool Prefix(TVBenchUnitViewer __instance, CabinetUnit ___cabineUnit)
            {
                if (!enabled || __instance.GetComponentInParent<PhotoFrameCtr>() == null)
                    return true;
                Dbgl($"Fresh Actions");
                return false;

                PlayerTargetMultiAction CurPlayerTarget = (PlayerTargetMultiAction)typeof(UnitViewer).GetProperty("CurPlayerTarget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, null);

                CurPlayerTarget.SetAction(ActionType.ActionInteract, 103809, ActionTriggerMode.Normal);
                CurPlayerTarget.RemoveAction(ActionType.ActionInteract, ActionTriggerMode.Hold);

                return false;


                ItemObject item = ___cabineUnit.GetItem(0);
                CurPlayerTarget.RemoveAction(ActionType.ActionRoll, ActionTriggerMode.Normal);
                if (item != null)
                {
                }
                CurPlayerTarget.SetAction(ActionType.ActionRoll, "Switch", ActionTriggerMode.Normal);
            }
        }

        //[HarmonyPatch(typeof(TVBenchUnitViewer), "Handler")]
        static class TVBenchUnitViewer_Patch
        {
            static bool Prefix(TVBenchUnitViewer __instance, ActionType obj)
            {
                if (!enabled || obj != ActionType.ActionRoll)
                    return true;
                Dbgl($"Handler");

                GameObject tv = __instance.gameObject;
                TVCtr tvc = tv.GetComponentInChildren<TVCtr>(true);
                GameObject screen = typeof(TVCtr).GetField("screen", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tvc) as GameObject;

                GameObject newObj;
                if (screen.name == "BigScreen")
                {
                    newObj = Singleton<ResMgr>.Instance.LoadSyncByType<GameObject>(AssetType.Home, "HomeItemTV");
                }
                else
                {
                    newObj = Singleton<ResMgr>.Instance.LoadSyncByType<GameObject>(AssetType.Home, "HomeItem_ADBoard");
                }

                MeshRenderer[] newMs = newObj.GetComponentsInChildren<MeshRenderer>();
                MeshRenderer[] tvms = tv.GetComponentsInChildren<MeshRenderer>();

                Dictionary<string, MeshRenderer> objDic = new Dictionary<string, MeshRenderer>();

                foreach (MeshRenderer m in newMs)
                {
                    //Dbgl($"mesh: {m.name}");
                    objDic.Add(m.name, m);
                }

                for (int i = 0; i < tvms.Length; i++)
                {
                    MeshRenderer m = tvms[i];
                    //Dbgl($"mesh: {m.name}");
                    m.GetComponentInChildren<MeshFilter>().mesh = newMs[i].GetComponentInChildren<MeshFilter>().mesh;
                    m.materials = newMs[i].materials;
                    m.bounds.SetMinMax(newMs[i].bounds.min, newMs[i].bounds.max);
                }


                if (screen.name == "BigScreen")
                {
                    screen.GetComponentInChildren<MeshFilter>().mesh = objDic["Item_TV_b"].GetComponentInChildren<MeshFilter>().mesh;
                    screen.name = "Screen";
                }
                else
                {
                    screen.GetComponentInChildren<MeshFilter>().mesh = objDic["Item_Billboard_2"].GetComponentInChildren<MeshFilter>().mesh;
                    screen.name = "BigScreen";
                }
                return false;
            }
        }

    }
}

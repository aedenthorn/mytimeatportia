using BehaviorDesigner.Runtime.Tasks.Basic.UnityGameObject;
using CutScene;
using Harmony12;
using Hont.ExMethod.Collection;
using Pathea;
using Pathea.ActorNs;
using Pathea.ModuleNs;
using Pathea.NpcAppearNs;
using Pathea.NpcRepositoryNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityModManagerNet;

namespace ModelSwitcher
{
    public partial class Main
    {
            
        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "ModelSwitcher " : "") + str);
        }
        public static bool enabled;

        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            settings = Settings.Load<Settings>(modEntry);
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            for (int i = 0; i < sortedIdToNames.Count; i++)
            {
                string id = sortedIdToNames.Keys.ToArray()[i];
                bool enable = (bool)typeof(Settings).GetField($"EnableCustomModelFor{id}").GetValue(settings);
                string modelId = (string)typeof(Settings).GetField($"CustomModelFor{id}").GetValue(settings);
                if (modelId == null || !idToModels.ContainsKey(modelId))
                {
                    modelId = defaultModelIdForId[id];
                }
                typeof(Settings).GetField($"EnableCustomModelFor{id}").SetValue(settings,GUILayout.Toggle(enable, string.Format(" Enable custom model for {0} ({1}){2}", Module<NpcRepository>.Self.GetNpcName(int.Parse(id)), id,(enable? string.Format(": <b>{0}</b> (<i>{1}</i>)", TextMgr.GetStr(int.Parse(sortedIdToNames[modelId]), -1), idToModels[modelId]) : "")), new GUILayoutOption[0]));
                if (enable)
                {
                    typeof(Settings).GetField($"CustomModelFor{id}").SetValue(settings, idToModels.Keys.ToArray()[(int)GUILayout.HorizontalSlider(idToModels.Keys.ToArray().IndexOf(modelId), 0f, idToModels.Keys.Count-1, new GUILayoutOption[0])]);
                }
            }
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        [HarmonyPatch(typeof(ActorInfo), "Instantiate")]
        private static class ActorInfo_Instantiate_Patch
        {
            private static bool Prefix(Transform parent, ActorInfo __instance, ref string ___model, ref Actor __result)
            {
                if (!modelToIds.ContainsKey(___model))
                {
                    return true;
                }

                if (false && ___model == "Actor/Npc_Nora")
                {
                    Dbgl("found Nora: " + ___model);
                    string asset = "Cg/Cutscene_Marry_CG001";

                    SingleBundle bundle = AccessTools.FieldRefAccess<CutSceneMngr, SingleBundle>(Singleton<CutSceneMngr>.Instance, "bundle");

                    if (!bundle.Loaded)
                    {
                        bundle.Load("cg");
                    }
                    UnityEngine.Object scene = bundle.LoadAsset<GameObject>(asset, false);
                    if (scene == null)
                    {
                        Dbgl($"cannot find scene: {asset}");
                    }
                    else
                    {
                      
                        //GameObject go = GameObject.Instantiate(scene as GameObject);
                        GameObject go = (scene as GameObject);
                        Dbgl("found scene: " + asset);
                        foreach (Transform t in go.GetComponentsInChildren<Transform>())
                        {
                            break;
                            if(t.gameObject != null && t.gameObject.name == "medium_NoraNew_Model")
                            {
                                UnityEngine.Object @object = Singleton<ResMgr>.Instance.LoadSyncByType(AssetType.Actor, ___model, false, false);
                                GameObject gameObject = UnityEngine.Object.Instantiate(@object, parent) as GameObject;
                                Actor actor = gameObject.GetComponent<Actor>();
                                GameObject gameObject2 = GameObject.Instantiate(t.gameObject, actor.gameObject.transform);
                                SkinnedMeshRenderer smro = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                                smro = SkinnedMeshRenderer.Instantiate(gameObject2.GetComponentInChildren<SkinnedMeshRenderer>(),smro.transform.parent);
                                //AccessTools.FieldRefAccess<Actor, SkinnedMeshRenderer>(actor, "skinnedMeshRenderer") = ;
                                __result = actor;
                                return false;
                                //__result.RefreshMeshReference(smrs[0]);
                            }
                        }
                    }
                }
                

                string id = modelToIds[___model];

                bool enable = (bool)typeof(Settings).GetField($"EnableCustomModelFor{id}").GetValue(settings);
                if (!enable)
                {
                    return true;
                }

                string modelId = (string)typeof(Settings).GetField($"CustomModelFor{id}").GetValue(settings);
                string model = idToModels[modelId];
                //Dbgl($"model: {model}");
                ___model = model;
                return true;
            }
        }
    }
}

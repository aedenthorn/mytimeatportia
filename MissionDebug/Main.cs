using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Ccc;
using Harmony12;
using Pathea;
using Pathea.BlackBoardNs;
using Pathea.ItemSystem;
using Pathea.Missions;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using PatheaScript;
using UnityEngine;
using UnityModManagerNet;
namespace MissionDebug
{
    public class Main
    {
        public static Settings settings { get; private set; }
        private static UnityModManager.ModEntry.ModLogger logger;
        public static bool enabled;
        private static string runMissionTempNo;
        private static string checkVarTempVal;
        private static bool isDebug = true;
        private static string dumpMissionInfoTempVal;
        private static string deliverMissionTempNo;
        private static string checkBBVarTempVal;
        private static string setVarTempVar;
        private static string setVarTempVal;
        private static string setBBVarTempVar;
        private static string setBBVarTempVal;
        private static Vector2 missionScrollPosition;
        private static float standardWidth = 200f;
        private static string setMissionVarTempMission;
        private static string setMissionVarTempVar;
        private static string setMissionVarTempVal;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                UnityEngine.Debug.Log((pref ? "Mission Debugger " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);
            logger = modEntry.Logger;
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
            settings.Save(modEntry);
        }
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("<b>Start Missions</b>", new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Activate Mission", new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            runMissionTempNo = GUILayout.TextField(runMissionTempNo, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            if (GUILayout.Button("Activate", new GUILayoutOption[] { GUILayout.Width(standardWidth) }))
            {
                Module<Story>.Self.ScriptMgr.AddToLoadList(int.Parse(runMissionTempNo));
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Deliver Mission", new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            deliverMissionTempNo = GUILayout.TextField(deliverMissionTempNo, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            if (GUILayout.Button("Deliver", new GUILayoutOption[] { GUILayout.Width(standardWidth) }))
            {
                if (MissionManager.allMissionBaseInfo.ContainsKey(int.Parse(deliverMissionTempNo)))
                {
                    Module<MissionManager>.Self.DeliverMission(MissionManager.allMissionBaseInfo[int.Parse(deliverMissionTempNo)], 0, "0", 0, "0");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            GUILayout.Label("<b>Remove Missions</b>", new GUILayoutOption[0]);
            missionScrollPosition = GUILayout.BeginScrollView(missionScrollPosition, new GUILayoutOption[] { GUILayout.Height(200f)});
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            GUILayout.Label("Activated", new GUILayoutOption[0]);
            List<Mission> actives = Module<MissionManager>.Self.GetMissionActived();
            for (int i = 0; i < actives.Count; i++)
            {
                Mission mi = actives[i];
                if (GUILayout.Button(mi.MissionId+" x", new GUILayoutOption[0]))
                {
                    Module<MissionManager>.Self.RemoveActiveMission(mi.MissionId);
                }
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            GUILayout.Label("Running", new GUILayoutOption[0]);
            List<Mission> runnings = Module<MissionManager>.Self.GetMissionRunning();
            for (int i = 0; i < runnings.Count; i++)
            {
                Mission mi = runnings[i];
                if (GUILayout.Button(mi.MissionId + " x", new GUILayoutOption[0]))
                {
                    AccessTools.FieldRefAccess<MissionManager, List<Mission>>(Module<MissionManager>.Self, "m_missions_Running").RemoveAll((Mission it) =>  it.InstanceID == mi.InstanceID);
                    
                }
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            GUILayout.Label("Ended", new GUILayoutOption[0]);
            List<Mission> endeds = Module<MissionManager>.Self.GetMissionEnd();
            for (int i = 0; i < endeds.Count; i++)
            {
                Mission mi = endeds[i];
                if (GUILayout.Button(mi.MissionId + " x", new GUILayoutOption[0]))
                {
                    AccessTools.FieldRefAccess<MissionManager, List<Mission>>(Module<MissionManager>.Self, "m_missions_End").RemoveAll((Mission it) => it.InstanceID == mi.InstanceID);

                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            GUILayout.Space(20f);

            GUILayout.Label("<b>Dump info to Log</b>", new GUILayoutOption[0]);
            if (GUILayout.Button("Dump All Missions", new GUILayoutOption[] { GUILayout.Width(standardWidth*3) }))
            {
                List<string> outInfo = new List<string>();
                outInfo.Add("Activated Missions:\r\n");
                foreach (Mission mi in Module<MissionManager>.Self.GetMissionActived())
                {
                    AddMissionInfoToList(ref outInfo, mi);
                }
                outInfo.Add("\r\nRunning Missions:\r\n");
                foreach (Mission mi in Module<MissionManager>.Self.GetMissionRunning())
                {
                    AddMissionInfoToList(ref outInfo, mi);
                }
                outInfo.Add("\r\nEnded Missions:\r\n");
                foreach (Mission mi in Module<MissionManager>.Self.GetMissionEnd())
                {
                    AddMissionInfoToList(ref outInfo, mi);
                }
                Dbgl(string.Join("\r\n", outInfo.ToArray()));
            }
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Dump Single Mission Info ", new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            dumpMissionInfoTempVal = GUILayout.TextField(dumpMissionInfoTempVal, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            if (GUILayout.Button("Dump", new GUILayoutOption[]{GUILayout.Width(standardWidth)}))
            {
                if (dumpMissionInfoTempVal != null && dumpMissionInfoTempVal.Length > 0)
                {
                    if (MissionManager.allMissionBaseInfo.ContainsKey(int.Parse(dumpMissionInfoTempVal)))
                    {
                        List<string> outInfo = GetMissionInfo(dumpMissionInfoTempVal);
                        Dbgl(string.Join("\r\n", outInfo.ToArray()));
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            if (GUILayout.Button("Dump Blackboard", new GUILayoutOption[] { GUILayout.Width(standardWidth*3) }))
            {
                List<string>outVars = new List<string>();
                VariableMgr mgr = AccessTools.FieldRefAccess<PsScriptMgr, VariableMgr>(Module<Story>.Self.ScriptMgr, "mVarMgr");
                Dictionary<string, string> mDicVar = Module<GlobleBlackBoard>.Self.InfoDic;
                outVars.Add("Global Blackboard Variables:");
                foreach (KeyValuePair<string,string> kvp in mDicVar)
                {
                    outVars.Add("\t" + kvp.Key + " " + kvp.Value);
                }
                Dbgl(string.Join("\r\n", outVars.ToArray()));
            }
            GUILayout.Space(10f);
            if (GUILayout.Button("Dump Global Vars", new GUILayoutOption[] { GUILayout.Width(standardWidth*3) }))
            {
                List<string>outVars = new List<string>();
                VariableMgr mgr = AccessTools.FieldRefAccess<PsScriptMgr, VariableMgr>(Module<Story>.Self.ScriptMgr, "mVarMgr");
                Dictionary<string, Variable> mDicVar = Module<Story>.Self.ScriptMgr.MVarMgr.MDicVar;
                outVars.Add("Global Variables:");
                foreach (KeyValuePair<string,Variable> kvp in mDicVar)
                {
                    outVars.Add("\t"+kvp.Key + " " + kvp.Value);
                }
                Dbgl(string.Join("\r\n", outVars.ToArray()));
            }

            GUILayout.Space(20f);

            GUILayout.Label("<b>Set Variables</b>", new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Set Blackboard Var: ", new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            setBBVarTempVar = GUILayout.TextField(setBBVarTempVar, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            GUILayout.Label(" Value: ", new GUILayoutOption[] { GUILayout.Width(standardWidth/2) });
            setBBVarTempVal = GUILayout.TextField(setBBVarTempVal, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            if (GUILayout.Button("Set", new GUILayoutOption[]{GUILayout.Width(standardWidth/2)}))
            {
                if (setBBVarTempVar != null && setBBVarTempVar.Length > 0)
                {
                    Module<GlobleBlackBoard>.Self.SetInfo(setBBVarTempVar, setBBVarTempVal);
                }
            }
            if (setBBVarTempVar != null && setBBVarTempVar.Length > 0 && Module<GlobleBlackBoard>.Self.HasInfo(setBBVarTempVar))
            {
                GUILayout.Label($" {Module<GlobleBlackBoard>.Self.GetInfo(setBBVarTempVar)}", new GUILayoutOption[0]);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Set Global Var: ", new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            setVarTempVar = GUILayout.TextField(setVarTempVar, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            GUILayout.Label(" Value: ", new GUILayoutOption[] { GUILayout.Width(standardWidth / 2) });
            setVarTempVal = GUILayout.TextField(setVarTempVal, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            if (GUILayout.Button("Set", new GUILayoutOption[] { GUILayout.Width(standardWidth / 2) }))
            {
                if (setVarTempVar != null && setVarTempVar.Length > 0)
                {
                    Variable var = Module<Story>.Self.ScriptMgr.GetVar(setVarTempVar);
                    if (var == null)
                    {
                        Module<Story>.Self.ScriptMgr.AddVar(setVarTempVar, new Variable(setVarTempVal));
                    }
                    else
                    {
                        Module<Story>.Self.ScriptMgr.MVarMgr.MDicVar[setVarTempVar] = new Variable(setVarTempVal);
                    }
                }
            }
            if (setVarTempVar != null && setVarTempVar.Length > 0)
            {
                Variable var = Module<Story>.Self.ScriptMgr.GetVar(setVarTempVar);
                if (var != null)
                {
                    GUILayout.Label($" {var.Value}", new GUILayoutOption[0]);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Set Mission Var: ", new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            setMissionVarTempMission = GUILayout.TextField(setMissionVarTempMission, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            GUILayout.Label("Var: ", new GUILayoutOption[] { GUILayout.Width(standardWidth/3) });
            setMissionVarTempVar = GUILayout.TextField(setMissionVarTempVar, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            GUILayout.Label(" Value: ", new GUILayoutOption[] { GUILayout.Width(standardWidth / 3) });
            setMissionVarTempVal = GUILayout.TextField(setMissionVarTempVal, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            if (GUILayout.Button("Set", new GUILayoutOption[] { GUILayout.Width(standardWidth / 3) }))
            {
                if (setMissionVarTempMission != null && setMissionVarTempMission.Length > 0 && setMissionVarTempVar != null && setMissionVarTempVar.Length > 0)
                {
                    Mission mi = actives.Find((Mission m) => m.MissionId+"" == setMissionVarTempMission);
                    if(mi == null)
                    {
                        mi = runnings.Find((Mission m) => m.MissionId + "" == setMissionVarTempMission);
                    }
                    if (mi == null)
                    {
                        mi = endeds.Find((Mission m) => m.MissionId + "" == setMissionVarTempMission);
                    }
                    if (mi != null)
                    {
                        PsScript script = Module<Story>.Self.ScriptMgr.MScriptList.Find(s => s.Id == mi.MissionId);
                        if (script.VarDict.ContainsKey(setMissionVarTempVar))
                        {
                            script.VarDict[setMissionVarTempVar] = new Variable(setMissionVarTempVal);
                        }
                        else
                        {
                            script.VarDict.Add(setMissionVarTempVar, new Variable(setMissionVarTempVal));
                        }
                    }

                }
            }
            GUILayout.EndHorizontal();

        }

        private static void AddMissionInfoToList(ref List<string> outInfo, Mission mi)
        {
            outInfo.Add($"\r\n\tMission Id: {mi.MissionId}");
            outInfo.Add($"\t\tMission NO: {mi.MissionNO}");
            outInfo.Add($"\t\tIsMain: {mi.IsMain}");
            outInfo.Add($"\t\tMission Name: {TextMgr.GetStr(mi.MissionShowName, -1)}");
            outInfo.Add($"\t\tGroup Type: {mi.groupType}");
            outInfo.Add($"\t\tGroup Id: {mi.groupId}");

            outInfo.Add($"\t\tDescription: \r\n\t\t\t{mi.MissionDescribe.Replace("\r\n","\r\n\t\t\t")}");
            outInfo.Add($"\t\treceive Npc: {NpcRepository.Self.GetNpcName(mi.MissionReceiveNPC)}");
            outInfo.Add($"\t\tpreTalk: {mi.PreTalk}");
            outInfo.Add($"\t\tPossible rewards:");
            foreach (MissionRewards mr in mi.m_missionRewardsList)
            {
                outInfo.Add($"\r\n\t\t\tMoney: {mr.Money}");
                outInfo.Add($"\t\t\tExp: {mr.Exp}");
                outInfo.Add($"\t\t\tItems:");
                foreach (IdCount idc in mr.ItemList)
                {
                    outInfo.Add($"\t\t\t\t{Module<ItemDataMgr>.Self.GetItemName(idc.id)} {idc.count}");
                }
                outInfo.Add($"\t\t\tFavor:");
                foreach (IdCount idc in mr.FavorList)
                {
                    outInfo.Add($"\t\t\t\t{Module<NpcRepository>.Self.GetNpcName(idc.id)} {idc.count}");
                }
                outInfo.Add($"\t\t\tRep: {mr.Reputation}");
            }
            PsScript script = Module<Story>.Self.ScriptMgr.MScriptList.Find(s => s.Id == mi.MissionId);
            if (script != null)
            {
                outInfo.Add("\t\tVariables:");
                if(script.VarDict != null)
                {
                    foreach (KeyValuePair<string, Variable> kvp in script.VarDict)
                    {
                        outInfo.Add("\t\t\t" + kvp.Key + " " + kvp.Value);
                    }
                }
            }
        }

        private static List<string> GetMissionInfo(string dumpMissionInfoTempVal, List<string> outInfo = null)
        {
            if(outInfo == null)
            {
                outInfo = new List<string>();
            }
            MissionBaseInfo mbi = MissionManager.allMissionBaseInfo[int.Parse(dumpMissionInfoTempVal)];
            outInfo.Add($"Mission NO: {mbi.MissionNO}");
            outInfo.Add($"IsMain: {mbi.IsMain}");
            outInfo.Add($"Mission Name: {TextMgr.GetStr(mbi.MissionNameId, -1)}");
            outInfo.Add($"Properties: {mbi.Properties}");
            outInfo.Add($"Group Type: {mbi.GroupType}");
            outInfo.Add($"Group Id: {mbi.GroupId}");

            List<string> outDesc = new List<string>();
            foreach (int i in mbi.description)
            {
                outDesc.Add(TextMgr.GetStr(i, -1));
            }
            outInfo.Add($"description:\r\n\t{string.Join("\r\n\t", outDesc.ToArray())}");
            outInfo.Add($"receive Npc: {NpcRepository.Self.GetNpcName(mbi.receiveNpc)}");
            outInfo.Add($"preTalk: {mbi.preTalk}");
            outInfo.Add($"reward:\r\n\t{string.Join("\r\n\t", mbi.reward.ToArray())}");
            PsScript script = Module<Story>.Self.ScriptMgr.MScriptList.Find(s => s.Id == int.Parse(dumpMissionInfoTempVal));
            if (script != null)
            {
                outInfo.Add("Variables:");
                foreach (KeyValuePair<string, Variable> kvp in script.VarDict)
                {
                    outInfo.Add("\t" + kvp.Key + " " + kvp.Value);
                }
            }
            return outInfo;
        }
    }
}

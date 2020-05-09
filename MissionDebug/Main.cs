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
        private static string dumpMissionVarTempVal;
        private static string dumpMissionInfoTempVal;
        private static string deliverMissionTempNo;
        private static string checkBBVarTempVal;
        private static string setVarTempVar;
        private static string setVarTempVal;
        private static string setBBVarTempVar;
        private static string setBBVarTempVal;

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
            if (GUILayout.Button("Dump Blackboard", new GUILayoutOption[]{
                GUILayout.Width(350f)
            }))
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
            GUILayout.Space(20f);
            if (GUILayout.Button("Dump Variables", new GUILayoutOption[]{
                GUILayout.Width(350f)
            }))
            {
                List<string>outVars = new List<string>();
                VariableMgr mgr = AccessTools.FieldRefAccess<PsScriptMgr, VariableMgr>(Module<Story>.Self.ScriptMgr, "mVarMgr");
                Dictionary<string, Variable> mDicVar = Module<Story>.Self.ScriptMgr.MVarMgr.MDicVar;
                foreach(KeyValuePair<string,Variable> kvp in mDicVar)
                {
                    outVars.Add(kvp.Key + " " + kvp.Value);
                }
                Dbgl(string.Join("\r\n", outVars.ToArray()));
            }
            GUILayout.Space(20f);
            if (GUILayout.Button("Dump Missions", new GUILayoutOption[]{
                GUILayout.Width(350f)
            }))
            {
                List<string> outInfo = new List<string>();
                outInfo.Add("Activated Missions:\r\n");
                foreach(Mission mi in Module<MissionManager>.Self.GetMissionActived())
                {
                    AddMissionInfoToList(ref outInfo, mi);
                }
                outInfo.Add("\r\nRunning Missions:\r\n");
                foreach(Mission mi in Module<MissionManager>.Self.GetMissionRunning())
                {
                    AddMissionInfoToList(ref outInfo, mi);
                }
                outInfo.Add("\r\nEnded Missions:\r\n");
                foreach(Mission mi in Module<MissionManager>.Self.GetMissionEnd())
                {
                    AddMissionInfoToList(ref outInfo, mi);
                }
                Dbgl(string.Join("\r\n", outInfo.ToArray()));
            }
            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Dump Mission Info ", new GUILayoutOption[0]);
            dumpMissionInfoTempVal = GUILayout.TextField(dumpMissionInfoTempVal, new GUILayoutOption[] { GUILayout.Width(300f) });
            if (GUILayout.Button("Dump", new GUILayoutOption[]{
                GUILayout.Width(150f)
            }))
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
            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Check Global Var ", new GUILayoutOption[0]);
            checkVarTempVal = GUILayout.TextField(checkVarTempVal, new GUILayoutOption[] { GUILayout.Width(300f) });
            if (checkVarTempVal != null && checkVarTempVal.Length > 0)
            {
                Variable var = Module<Story>.Self.ScriptMgr.GetVar(checkVarTempVal);
                if (var != null)
                {
                    GUILayout.Label($" {var.Value}", new GUILayoutOption[0]);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Check Blackboard Var ", new GUILayoutOption[0]);
            checkBBVarTempVal = GUILayout.TextField(checkBBVarTempVal, new GUILayoutOption[] { GUILayout.Width(300f) });
            if (checkBBVarTempVal != null && checkBBVarTempVal.Length > 0 && Module<GlobleBlackBoard>.Self.HasInfo(checkBBVarTempVal))
            {
                string var = Module<GlobleBlackBoard>.Self.GetInfo(checkBBVarTempVal);
                if (var != null)
                {
                    GUILayout.Label($" {var}", new GUILayoutOption[0]);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Set Global Var: ", new GUILayoutOption[0]);
            setVarTempVar = GUILayout.TextField(setVarTempVar, new GUILayoutOption[] { GUILayout.Width(300f) });
            GUILayout.Label(" Value: ", new GUILayoutOption[0]);
            setVarTempVal = GUILayout.TextField(setVarTempVal, new GUILayoutOption[] { GUILayout.Width(300f) });
            if (GUILayout.Button("Set", new GUILayoutOption[]{
                    GUILayout.Width(150f)
                }))
            {
                if (setVarTempVar != null && setVarTempVar.Length > 0 && Module<GlobleBlackBoard>.Self.HasInfo(setVarTempVar))
                {
                    Module<GlobleBlackBoard>.Self.SetInfo(setVarTempVar, setVarTempVal);
                }
            }
            if (setVarTempVar != null && setVarTempVar.Length > 0 && Module<GlobleBlackBoard>.Self.HasInfo(setVarTempVar))
            {
                GUILayout.Label($" {Module<GlobleBlackBoard>.Self.GetInfo(setVarTempVar)}", new GUILayoutOption[0]);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Set Blackboard Var: ", new GUILayoutOption[0]);
            setBBVarTempVar = GUILayout.TextField(setBBVarTempVar, new GUILayoutOption[] { GUILayout.Width(300f) });
            GUILayout.Label(" Value: ", new GUILayoutOption[0]);
            setBBVarTempVal = GUILayout.TextField(setBBVarTempVal, new GUILayoutOption[] { GUILayout.Width(300f) });
            if (GUILayout.Button("Set", new GUILayoutOption[]{
                    GUILayout.Width(150f)
                }))
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
            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Run Mission", new GUILayoutOption[0]);
            runMissionTempNo = GUILayout.TextField(runMissionTempNo, new GUILayoutOption[] {GUILayout.Width(300f) });
            if (GUILayout.Button("Run", new GUILayoutOption[]{
                GUILayout.Width(150f)
            }))
            {
                Module<Story>.Self.ScriptMgr.AddToLoadList(int.Parse(runMissionTempNo));
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Deliver Mission", new GUILayoutOption[0]);
            deliverMissionTempNo = GUILayout.TextField(deliverMissionTempNo, new GUILayoutOption[] {GUILayout.Width(300f) });
            if (GUILayout.Button("Deliver", new GUILayoutOption[]{
                GUILayout.Width(150f)
            }))
            {
                if (MissionManager.allMissionBaseInfo.ContainsKey(int.Parse(deliverMissionTempNo)))
                {
                    Module<MissionManager>.Self.DeliverMission(MissionManager.allMissionBaseInfo[int.Parse(deliverMissionTempNo)], 0, "0", 0, "0");
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
            Dbgl("1");
            foreach (MissionRewards mr in mi.m_missionRewardsList)
            {
                outInfo.Add($"\r\n\t\t\tMoney: {mr.Money}");
                outInfo.Add($"\t\t\tExp: {mr.Exp}");
                outInfo.Add($"\t\t\tItems:");
                Dbgl("2");
                foreach (IdCount idc in mr.ItemList)
                {
                    outInfo.Add($"\t\t\t\t{Module<ItemDataMgr>.Self.GetItemName(idc.id)} {idc.count}");
                }
                Dbgl("3");
                outInfo.Add($"\t\t\tFavor:");
                foreach (IdCount idc in mr.FavorList)
                {
                    outInfo.Add($"\t\t\t\t{Module<NpcRepository>.Self.GetNpcName(idc.id)} {idc.count}");
                }
                outInfo.Add($"\t\t\tRep: {mr.Reputation}");
            }
            Dbgl("4");
            PsScript script = Module<Story>.Self.ScriptMgr.MScriptList.Find(s => s.Id == mi.MissionId);
            if (script != null)
            {
                outInfo.Add("\t\tVariables:");
                Dbgl("5");
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
            PsScript script = Module<Story>.Self.ScriptMgr.MScriptList.Find(s => s.Id == int.Parse(dumpMissionVarTempVal));
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

using Ccc;
using Harmony12;
using Pathea;
using Pathea.Conversations;
using Pathea.MessageSystem;
using Pathea.Missions;
using Pathea.ModuleNs;
using Pathea.OptionNs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityModManagerNet;

namespace CustomQuests
{
    public partial class Main
    {
        public static bool enabled;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "CustomQuests " : "") + str);
        }
        public static Settings settings { get; private set; }

        public static Dictionary<int, string> dictStrings;
        public static Dictionary<int,string> newMissions;
        public static List<int> activeMissions;

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            AddScripts();

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(OnLoadGame));

            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static void AddStrings()
        {
            string lang = "";

            switch ((int)Singleton<OptionsMgr>.Instance.LanguageGame)
            {
                case 0:
                    lang = ("Chinese");
                    break;
                case 1:
                    lang = ("English");
                    break;
                case 2:
                    lang = ("German");
                    break;
                case 3:
                    lang = ("French");
                    break;
                case 4:
                    lang = ("T_Chinese");
                    break;
                case 5:
                    lang = ("Italian");
                    break;
                case 6:
                    lang = ("Spanish");
                    break;
                case 7:
                    lang = ("Japanese");
                    break;
                case 8:
                    lang = ("Russian");
                    break;
                case 9:
                    lang = ("Turkish");
                    break;
                case 10:
                    lang = ("Korean");
                    break;
                case 11:
                    lang = ("Portuguese");
                    break;
                case 12:
                    lang = ("Vietnamese");
                    break;
            }
            if (lang == "")
                return;



            dictStrings = new Dictionary<int, string>();
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"assets");

            string[] files = Directory.GetFiles(path, $"{lang}*.txt", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                Dbgl("No translation files found at " + path);
            }
            else
            {
                Dbgl($"{files.Length} translation files found at {path}");
            }
            foreach (string filename in files)
            {
                try
                {

                    var lines = File.ReadAllLines(filename);
                    for (var i = 0; i < lines.Length; i += 1)
                    {
                        string[] line = lines[i].Split('=');
                        if (line.Length < 2)
                            continue;
                        var text = new string[line.Length - 1];
                        Array.Copy(line, 1, text, 0, line.Length - 1);
                        if (int.TryParse(line[0], out int id))
                        {
                            dictStrings.Add(id, text.Join());
                        }
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(ex.ToString());
                }
            }
        }

        private static void AddConv()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");
            string[] files = Directory.GetFiles(path, "conversations*.txt", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                Dbgl("No conversation files found at " + path);
            }
            else
            {
                Dbgl($"{files.Length} conversation files found at {path}");
            }
            foreach (string filename in files)
            {
                try
                {
                    var lines = File.ReadAllLines(filename);
                    for (var i = 0; i < lines.Length; i += 1)
                    {
                        string[] line = lines[i].Split('|');
                        if (line.Length != 5)
                            continue;
                        if (!int.TryParse(line[0], out int id))
                        {
                            continue;
                        }
                        string[] trans = line[1].Split(',');
                        List<int> transId = new List<int>();
                        foreach (string str in trans)
                        {
                            if (int.TryParse(str, out int atrans))
                            {
                                transId.Add(atrans);
                            }
                        }
                        if (!int.TryParse(line[2], out int type))
                        {
                            continue;
                        }
                        if (type == 0)
                        {
                            ConvBase.mDict[id] = new ConvBSentence(id, transId, line[3], line[4]);
                        }
                        else if (type == 1)
                        {
                            ConvBase.mDict[id] = new ConvBChoice(id, transId, line[3], line[4]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(ex.ToString());
                }
            }
        }

        private static void AddScripts()
        {
            newMissions = new Dictionary<int, string>();
            activeMissions = new List<int>();
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");
            string[] files = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);
            if(files.Length == 0)
            {
                Dbgl("No xml files found at " + path);
            }
            else
            {
                Dbgl($"{files.Length} xml files found at {path}");
            }
            foreach (string filename in files)
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(filename);
                    XmlNode xmlNode = xmlDocument.SelectSingleNode("//MISSION");
                    if (xmlNode != null)
                    {
                        MissionBaseInfo missionBaseInfo = new MissionBaseInfo();
                        missionBaseInfo.InstanceID = PatheaScript.Util.GetInt(xmlNode, "id");
                        missionBaseInfo.MissionNameId = PatheaScript.Util.GetInt(xmlNode, "nameId");
                        missionBaseInfo.MissionNO = PatheaScript.Util.GetInt(xmlNode, "missionNO");
                        missionBaseInfo.IsMain = PatheaScript.Util.GetBool(xmlNode, "isMain");
                        missionBaseInfo.Properties = Uri.UnescapeDataString(PatheaScript.Util.GetString(xmlNode, "properties")).Trim();
                        List<string> list = new List<string>(xmlNode.ChildNodes.Count);
                        IEnumerator enumerator = xmlNode.ChildNodes.GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                object obj = enumerator.Current;
                                XmlNode xmlNode2 = (XmlNode)obj;
                                string @string = PatheaScript.Util.GetString(xmlNode2, "name");
                                list.Add(@string);
                            }
                        }
                        finally
                        {
                            IDisposable disposable;
                            if ((disposable = (enumerator as IDisposable)) != null)
                            {
                                disposable.Dispose();
                            }
                        }
                        using (List<string>.Enumerator enumerator2 = list.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                string name = enumerator2.Current;
                                if (list.FindAll((string it) => it == name).Count > 1)
                                {
                                    UnityEngine.Debug.LogError(string.Concat(new object[]
                                    {
                                        "重复trigger名！",
                                        name,
                                        ", No=",
                                        missionBaseInfo.MissionNO,
                                        ", Id=",
                                        missionBaseInfo.InstanceID
                                    }));
                                }
                            }
                        }
                        MissionManager.AddMissionBaseInfo(missionBaseInfo);
                        newMissions.Add(missionBaseInfo.InstanceID, filename);
                        Dbgl("added " + missionBaseInfo.InstanceID);
                        if (Path.GetFileName(filename).StartsWith("active"))
                        {
                            activeMissions.Add(missionBaseInfo.InstanceID);
                        }
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(ex.ToString());
                }
            }
        }

        private static void OnLoadGame(object[] obj)
        {
            foreach (int id in activeMissions)
            {
                Module<Story>.Self.ScriptMgr.AddToLoadList(id);
                Dbgl("activated " + id);
            }
        }
    }
}

using Harmony12;
using Pathea.Missions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityModManagerNet;

namespace DebugUtils
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                UnityEngine.Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            File.WriteAllText(GetLogPath(), "");

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.Patch(
                AccessTools.Method((typeof(UnityEngine.Debug).GetField("s_Logger", BindingFlags.Static | BindingFlags.NonPublic).GetValue(UnityEngine.Debug.unityLogger) as Logger).logHandler.GetType(), "LogFormat"), 
                prefix: new HarmonyMethod(typeof(Main).GetMethod(nameof(DebugLogHandler_LogFormat_Prefix)))
            );
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static string GetLogPath()
        {
            string[] array = new string[]
            {
                Application.persistentDataPath,
                Application.dataPath
            };
            string[] array2 = new string[]
            {
                "Player.log",
                "output_log.txt"
            };
            foreach (string path in array)
            {
                foreach (string path2 in array2)
                {
                    string text = Path.Combine(path, path2);
                    if (File.Exists(text))
                    {
                        text = text.Replace("/", "\\");
                        return text.Replace(".txt", "_clean.txt").Replace(".log", "_clean.log");
                    }
                }
            }
            return null;
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
            settings.StackTrace = GUILayout.Toggle(settings.StackTrace, "Log StackTrace Line", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.WriteNativeLog = GUILayout.Toggle(settings.WriteNativeLog, "Log Native Log (wasteful)", new GUILayoutOption[0]);
            GUILayout.Space(10f);

            if (GUILayout.Button("Open Clean Log File", new GUILayoutOption[]{
                GUILayout.Width(250f),
                GUILayout.Height(80f),
            }))
            {
                OpenLogFile();
            }
            if (GUILayout.Button("Open Log File Location", new GUILayoutOption[]{
                GUILayout.Width(250f),
                GUILayout.Height(80f),
            }))
            {
                OpenLogFileLocation();
            }

        }
        private static void CleanLogFile()
        {
            Dbgl($"showing gui");

            string text = GetLogPath();
            if (text != null)
            {
                Dbgl($"cleaning up {text}");
                string[] lines = File.ReadAllLines(text);
                List<string> newLines = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[0].StartsWith("(Filename:"))
                        continue;
                    newLines.Add(lines[0]);
                }
                File.WriteAllLines(text, newLines.ToArray());
            }
        }

        private static void OpenLogFile()
        {
            string text = GetLogPath();
            if (text != null)
            {
                Dbgl($"Opening cleaned log file {text}");
                Thread.Sleep(500);
                Application.OpenURL(text);
                return;
            }
        }

        private static void OpenLogFileLocation()
        {
            string text = GetLogPath();
            if (text != null)
            {
                string path = Path.GetDirectoryName(text);
                Dbgl($"Opening log file location {path}");
                Thread.Sleep(500);
                Process.Start("explorer.exe", path);
                return;
            }
        }

        public static bool DebugLogHandler_LogFormat_Prefix(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if(!enabled)
                return true;
            if (settings.StackTrace)
            {
                string[] lines = Environment.StackTrace.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.None
                );
                foreach(string line in lines)
                {
                    if (line.Contains("at System") || line.Contains("at DebugUtils") || line.Contains("at UnityEngine"))
                        continue;
                    File.AppendAllText(GetLogPath(), $"[{line}]\r\n");
                    break;
                }
            }

            File.AppendAllText(GetLogPath(), string.Format(format, args) + "\r\n\r\n");
            return settings.WriteNativeLog;
        }
    }
}

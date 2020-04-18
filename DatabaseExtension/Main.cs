using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Harmony12;
using Pathea;
using UnityEngine;
using UnityModManagerNet;
using Pathea.MessageSystem;
using Newtonsoft.Json;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;

namespace DatabaseExtension
{
    public class Main
    {
        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "DatabaseExtension " : "") + str);
        }
        public static Settings settings { get; private set; }
        public static bool enabled;

        public static List<rows> NewRows = new List<rows>();
        public static List<rows> RowAppends = new List<rows>();


        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PreloadExtensions();
        }

        private static void OnLoadGame()
        {
            if (!enabled)
                return;
            Dbgl(TextMgr.GetStr(1,-1));
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
        }


        private static void PreloadExtensions()
        {
            if (!enabled)
                return;

            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Tables";
            if (!Directory.Exists(path))
            {
                Dbgl($"Directory {path} not found!");
                return;
            }
            foreach (string file in Directory.GetFiles(path, "*.xml"))
            {
                Dbgl($"File {file} found!");
                XmlSerializer ser = new XmlSerializer(typeof(tables));
                using (XmlReader reader = XmlReader.Create(path))
                {
                    tables mTables;
                    mTables = (tables)ser.Deserialize(reader);
                    foreach (object item in mTables.Items)
                    {
                        if (item.GetType() == typeof(tablesNewRows))
                        {
                            foreach (rows r in ((tablesNewRows) item).rows)
                            {
                                NewRows.Add(r);
                            }
                        }
                        if (item.GetType() == typeof(tablesRowAppends))
                        {
                            foreach (rows r in ((tablesRowAppends)item).rows)
                            {
                                RowAppends.Add(r);
                            }
                        }
                    }
                }

            }
        }

        //[HarmonyPatch(typeof(ScenarioModule), "PostLoad")]
        static class ScenarioModule_PostLoad_Patch
        {
            static void Postfix()
            {
                if (!enabled)
                    return;
                OnLoadGame();
            }
        }

        //[HarmonyPatch(typeof(SqliteAccessCS), "ExecuteQuery")]
        static class ExecuteQuery_Patch
        {
            static void Prefix(ref string sqlQuery)
            {
                if (!enabled)
                    return;

                if (sqlQuery.StartsWith("SELECT * FROM"))
                {
                    var tableName = sqlQuery.Replace("SELECT * FROM ", "");
                    if (tableName.Contains(" "))
                        return;

                    string addString = "";

                    foreach (rows r in NewRows)
                    {
                        if (r.table == tableName)
                        {
                            foreach (var row in r.row)
                            {
                                addString += " UNION SELECT";
                                var colList = new List<string>();
                                foreach (rowsRowCol col in row)
                                {
                                    colList.Add($" \"{col.Value}\" AS {col.name}");
                                }

                                addString += $" {string.Join(", ", colList.ToArray())}";
                            }
                        }
                    }


                    foreach (rows r in RowAppends)
                    {
                        if (r.table == tableName)
                        {
                            var rowList = new List<string>();
                            foreach (var row in r.row)
                            {
                                string rowString = "SELECT ";
                                var colList = new List<string>();
                                var where = "";
                                foreach (rowsRowCol col in row)
                                {
                                    if (where == "")
                                    {
                                        where = $" FROM {tableName} WHERE {col.name} LIKE \"{col.Value}\"";
                                    }
                                    else
                                    {
                                        colList.Add($"REPLACE({col.name},{col.name} || \"{col.Value}\") AS {col.name}");
                                    }
                                }

                                rowString += string.Join(", ", colList.ToArray()) + where;
                                rowList.Add(rowString);
                            }

                            string newString = string.Join(" UNION ", rowList.ToArray());
                            sqlQuery += " UNION " + newString;
                        }
                    }

                    sqlQuery += addString;

                    Dbgl(sqlQuery);
                }
            }
        }
    }
}

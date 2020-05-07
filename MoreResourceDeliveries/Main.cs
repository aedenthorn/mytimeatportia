using Harmony12;
using Pathea.GameResPointNs;
using Pathea.UISystemNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace MoreResourceDeliveries
{
    public partial class Main
    {
        public static bool enabled;
        public static Settings settings { get; private set; }

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Price limit multiplier (requires game restart): <b>{0}x</b>", settings.PriceLimitMult), new GUILayoutOption[0]);
            settings.PriceLimitMult = (int)GUILayout.HorizontalSlider((float)Main.settings.PriceLimitMult, 1f, 100f, new GUILayoutOption[0]);
        }
        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        [HarmonyPatch(typeof(GameResPointOutputBarUI), "Initial")]
        static class GameResPointOutputBarUI_Initial_Patch
        {
            static void Prefix(int itemId, int index)
            {
                Dbgl("ui: " + itemId + " " + index);
            }
        }
        [HarmonyPatch(typeof(GameResPointModule), "OnLoad", new Type[] { })]
        static class GameResPointModule_OnLoad_Patch
        {
            static void Postfix(GameResPointModule __instance, ref List<ResPointInfo> ___infoList)
            {
                for (int i = 0; i < ___infoList.Count; i++)
                {
                    if (___infoList[i].level == 5)
                    {
                        ___infoList.Remove(___infoList[i]);
                    }
                }

                foreach (ResPointData rpd in TreeResData)
                {
                    ___infoList.Add(rpd.value);
                }
                foreach (ResPointData rpd in MineResData)
                {
                    ___infoList.Add(rpd.value);
                }
                for (int i = 0; i < ___infoList.Count; i++)
                {
                    if (___infoList[i].level == 0)
                        continue;

                    ___infoList[i].priceLimit *= settings.PriceLimitMult;

                    var productsId = FieldRefAccess<ResPointInfo, string>(___infoList[i], "productsId");
                    var extraOutput = FieldRefAccess<ResPointInfo, string>(___infoList[i], "extraOutput");
                    if (___infoList[i].id == 1) // add apples and aroma apples
                    {
                        productsId = "4000047;"+productsId;
                        FieldRefAccess<ResPointInfo, string>(___infoList[i], "productsId") = productsId;
                        extraOutput += ",4000014_" + i;
                        FieldRefAccess<ResPointInfo, string>(___infoList[i], "extraOutput") = extraOutput;
                    }
                    else if (___infoList[i].id == 2) // add blood stone and marble
                    {
                        productsId += ";4000079;4000121"; 
                        FieldRefAccess<ResPointInfo, string>(___infoList[i], "productsId") = productsId;
                    }
                }
                foreach (ResPointInfo resPointInfo in ___infoList)
                {
                    resPointInfo.InitialOutPorts();
                }
            }
        }

        public class ResPointData
        {
            public ResPointData(int id, int level, int upgradeLevelCost, int priceLimit, string productsId, string extraOutput, int capacity, int mailId)
            {
                value.id = id;
                value.level = level;
                value.upgradeLevelCost = upgradeLevelCost;
                value.priceLimit = priceLimit;

                FieldRefAccess<ResPointInfo, string>(value, "productsId") = productsId;
                FieldRefAccess<ResPointInfo, string>(value, "extraOutput") = extraOutput;

                value.capacity = capacity;
                value.mailId = mailId;
            }
            public ResPointData(ResPointInfo rpi)
            {
                value.id = rpi.id;
                value.level = rpi.level;
                value.upgradeLevelCost = rpi.upgradeLevelCost;
                value.priceLimit = rpi.priceLimit;

                FieldRefAccess<ResPointInfo, string>(value, "productsId") = FieldRefAccess<ResPointInfo, string>(rpi, "productsId");
                FieldRefAccess<ResPointInfo, string>(value,"extraOutput") = FieldRefAccess<ResPointInfo, string>(rpi, "extraOutput");

                value.capacity = rpi.capacity;
                value.mailId = rpi.mailId;
            }
            public ResPointInfo value = new ResPointInfo();

        }
        private static ResPointData[] TreeResData = {
            new ResPointData(1,5,160000,8000,"4000138;4000012;4000001;4000077;4000085;4000042;4000109","0.5;4000282_10,4000014_10,4000312_4,4000084_20,4000050_20,4000279_3,4000086_1,4000342_1",16000,3601), // chance of poplar, birds nest, honeycomb
            new ResPointData(1,6,320000,16000,"4000138;4000012;4000001;4000077;4000085;4000042;4000109;4000084;4000050;4000282;4000014","0.5;4000312_8,4000279_6,4000086_2,4000342_2",32000,3601), // add honey, wax, royal honey, aroma apples
            new ResPointData(1,7,640000,32000,"4000138;4000012;4000001;4000077;4000085;4000042;4000109;4000084;4000050;4000282;4000014;4000279","0.5;4000312_10,4000086_3,4000342_3,",64000,3601), // add poplar
            new ResPointData(1,8,-1,64000,"4000138;4000012;4000001;4000077;4000085;4000042;4000109;4000084;4000050;4000282;4000014;4000279;4000312","0.5;4000086_4,4000342_4",128000,3601), // add resin
        };

        private static ResPointData[] MineResData = {
            new ResPointData(2,5,160000,8000,"4000013;4000139;4000035;4000119;4000118;4000283;4000284;4000162;","0.5;2060001_5,4000285_6,4000274_1",16000,3602), 
            new ResPointData(2,6,320000,16000,"4000013;4000139;4000035;4000119;4000118;4000283;4000284;4000162;4000147;4000017;4000120","0.5;2060001_10,4000285_8,4000274_2,4000148_1",32000,3602), // topaz, rock salt, crystal, chance sapphire
            new ResPointData(2,7,640000,32000,"4000013;4000139;4000035;4000119;4000118;4000283;4000284;4000162;4000147;4000017;4000120;4000148;4000038;4000045","0.5;2060001_20,4000285_10,4000274_3,4000063_1",64000,3602), // sapphire, nitre, sulfate, chance ruby
            new ResPointData(2,8,-1,64000,"4000013;4000139;4000035;4000119;4000118;4000283;4000284;4000162;4000147;4000017;4000120;4000148;4000038;4000299;4000045;4000297;4000063","0.5;2060001_30,4000285_12,4000274_4,4000171_1",128000,3602), // ruby, zeolite, igneous rock, chance diamond
        };

        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "MoreResourceDeliveries " : "") + str);
        }

    }
}

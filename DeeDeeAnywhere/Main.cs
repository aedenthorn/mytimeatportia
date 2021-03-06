﻿using Harmony12;
using Pathea;
using Pathea.Behavior;
using Pathea.InputSolutionNs;
using Pathea.MapNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.UISystemNs;
using PatheaScriptExt;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace DeeDeeAnywhere
{
    public class Main
    {

        public static bool enabled;

        public static Settings settings { get; private set; }

        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "DeeDeeAnywhere " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

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

        public static MapIconPoolObj playerTransport;
        public static MapInput input;
        public static MapIconInteractTransfer curTransfer;

        [HarmonyPatch(typeof(WholeMapViewer), "Awake")]
        public static class WholeMapViewer_Awake_Patch
        {

            public static void Postfix(WholeMapViewer __instance, GameObject ___layerParent)
            {
                if (!enabled)
                {
                    return;
                }
                input = __instance.GetComponent<MapInput>();
            }

        }

        //[HarmonyPatch(typeof(MapIconInteractTransfer), "OnPointerClick")]
        public static class OnPointerClick_Patch
        {
            public static void Prefix(MapIconInteractTransfer __instance)
            {
                if (!enabled)
                {
                    return;
                }
                IMap imap = AccessTools.FieldRefAccess<MapIconInteract, IMap>(__instance, "curImap");
                //Dbgl($"{} {imap.GetPos()} {imap.GetHoverInfo()}");
            }

        }
        [HarmonyPatch(typeof(WholeMapViewer), "OnEnable")]
        public static class WholeMapViewer_OnEnable_Patch
        {
            public static void Postfix(WholeMapViewer __instance)
            {
                if (!enabled)
                {
                    return;
                }
                List<MapIconData> listIconPrefabs = AccessTools.FieldRefAccess<MapViewer, List<MapIconData>>(__instance, "listIconPrefabs");
                var i = 0;
                foreach (MapIconData mid in listIconPrefabs)
                {
                    if (mid != null && mid.imap != null && mid.imap.GetIconInfo().HasInfo(MapType.Transport) && (mid.imap.GetIconInfo() as PresetIcon) != null && (mid.imap.GetIconInfo() as PresetIcon).IsIcon(MapIcon.TRANSFERPOINT))
                    {
                        //Dbgl($"scene: {mid.imap.GetScenarioName()} pos: {mid.imap.GetPos()} hover: {mid.imap.GetHoverInfo()}");
                    }
                }
                MessageManager.Instance.Subscribe("UIOtherMapTransfer", new Action<object[]>(JoyStickConfirm));
            }

        }

        [HarmonyPatch(typeof(WholeMapViewer), "OnDisable")]
        public static class WholeMapViewer_OnDisable_Patch
        {
            public static void Postfix(List<MapIconData> ___listIconPrefabs)
            {
                if (!enabled)
                {
                    return;
                }
                MessageManager.Instance.Unsubscribe("UIOtherMapTransfer", new Action<object[]>(JoyStickConfirm));

                for (int i = 0; i < ___listIconPrefabs.Count; i++)
                {
                    MapIconData iconData = ___listIconPrefabs[i];
                    if (iconData.CurIcon is PresetIcon && (iconData.CurIcon as PresetIcon).IsIcon(MapIcon.TRANSFERPOINT))
                    {
                        UnityEngine.Object.Destroy(iconData.iconObject.GetComponent<MapIconInteractTransfer>());
                        UnityEngine.Object.Destroy(iconData.iconObject.GetComponent<Selectable>());
                    }
                }
            }
        }
        private static GameObject transLayer;

        [HarmonyPatch(typeof(WholeMapViewer), "FreshMapIconCondition")]
        public static class WholeMapViewer_FreshMapIconCondition_Patch
		{
            public static bool Prefix(WholeMapViewer __instance, MapIconData iconData, MapIconInfo icon, float ___curLayerMin, float ___curLayerMax) 
            {
                if (!enabled)
                {
                    return true;
                }
                PresetIcon presetIcon = icon as PresetIcon;
                if (icon.HasInfo(MapType.Transport) && presetIcon != null && presetIcon.IsIcon(MapIcon.TRANSFERPOINT))
                {
                    GameObject parent = (GameObject)typeof(MapViewer).GetMethod("GetIconLayerParent", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { icon.GetIconLayer() });
                    typeof(MapViewer).GetMethod("ShowIcon", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { iconData, parent, icon });
                    typeof(WholeMapViewer).GetMethod("SetIconRectTrans", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { iconData, icon });
                    return false;
                }
                return true;

            }
        }

        //[HarmonyPatch(typeof(WholeMapViewer), "SetMarkIcon")]
        public static class WholeMapViewer_SetMarkIcon_Patch
        {
            public static bool Prefix()
            {
                if (!enabled)
                {
                    return true;
                }
                return false;
            }
        }
        
        [HarmonyPatch(typeof(WholeMapViewer), "SetCurIcon")]
        public static class WholeMapViewer_SetCurIcon_Patch
        {
            public static bool Prefix(WholeMapViewer __instance, ref MapIconData iconData, ref MapIconInfo icon)
            {
                if (!enabled)
                {
                    return true;
                }
                if (iconData.CurIcon is PresetIcon && (iconData.CurIcon as PresetIcon).IsIcon(MapIcon.TRANSFERPOINT))
                {
                    iconData.iconObject.GetComponent<Image>().raycastTarget = false;
                    UnityEngine.Object.Destroy(iconData.iconObject.GetComponent<MapIconInteractTransfer>());
                    UnityEngine.Object.Destroy(iconData.iconObject.GetComponent<Selectable>());
                }
                iconData.SetCurIcon(icon, __instance.GetHashCode());

                PresetIcon presetIcon = icon as PresetIcon;
                if (icon is PresetIcon && presetIcon.IsIcon(MapIcon.TRANSFERPOINT))
                {
                    (iconData.iconObject as MapIconPoolSprite).SetRaycastTarget(true);
                    if (iconData.iconObject.gameObject.GetComponent<MapIconInteractTransfer>() == null)
                    {

                        iconData.iconObject.gameObject.AddComponent<MapIconInteractTransfer>();
                        iconData.iconObject.gameObject.AddComponent<Selectable>();
                    }
                    iconData.iconObject.gameObject.GetComponent<MapIconInteractTransfer>().SetImap(iconData.imap);
                    int tranId = Singleton<TransferTransIdDataBase>.Self.GetTranId((iconData.imap as SceneItemTransfer_IMap).SItem.ID);
                    string name = (tranId <= 0) ? string.Empty : TextMgr.GetStr(tranId, -1);
                    Dbgl($"name: {name} pos: {iconData.imap.GetPos()}");
                }
                return false;
            }
        }

        //[HarmonyPatch(typeof(WholeMapViewer), "ShowDetails")]
        public static class WholeMapViewer_ShowDetails_Patch
        {
            public static bool Prefix(WholeMapViewer __instance, Image image, MapHoverDetail details, bool isDrag, ref Vector2 nearestAim, List<IMap> ___imapShowInfo, List<MapIconData> ___listIconPrefabs)
            {
                if (!enabled)
                {
                    return true;
                }
                nearestAim = Vector2.zero;
                float num = 100f;
                bool flag = false;
                ___imapShowInfo.Clear();
                if (!isDrag)
                {
                    Rect rect = WholeMapViewer.RectTransformToWorldSpace(image.rectTransform);
                    for (int i = 0; i < ___listIconPrefabs.Count; i++)
                    {
                        if (!(___listIconPrefabs[i].iconObject == null))
                        {
                            if (___listIconPrefabs[i].iconObject.gameObject.activeSelf)
                            {
                                MapIconPoolObj iconObject = ___listIconPrefabs[i].iconObject;
                                Rect other = WholeMapViewer.RectTransformToWorldSpace(iconObject.ColRect);
                                if (Singleton<InputDeviceDetector>.Instance.CurDevice != InputDevice.MouseKeyboard)
                                {
                                    Vector3 v = input.GetActiveIcon().rectTransform.InverseTransformPoint(___listIconPrefabs[i].iconObject.transform.position);
                                    float num2 = Vector2.Distance(Vector2.zero, v);
                                    if (num > num2)
                                    {
                                        num = num2;
                                        nearestAim = v;
                                    }
                                }
                                if (rect.Overlaps(other))
                                {
                                    ___imapShowInfo.Add(___listIconPrefabs[i].imap);
                                    MapIconInteractTransfer component = ___listIconPrefabs[i].iconObject.GetComponent<MapIconInteractTransfer>();
                                    if (component != null && curTransfer == null && Input.GetMouseButton(0))
                                    {
                                        flag = true;
                                        curTransfer = component;

                                        Dbgl(rect.ToString());

                                        IMap imap = AccessTools.FieldRefAccess<MapIconInteractTransfer, IMap>(component, "curImap");

                                        int tranId = Singleton<TransferTransIdDataBase>.Self.GetTranId((imap as SceneItemTransfer_IMap).SItem.ID);
                                        string name = (tranId <= 0) ? string.Empty : TextMgr.GetStr(tranId, -1);
                                        //Dbgl($"name: {name} pos: {imap.GetPos()}");

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!flag)
                {
                    return true;
                }
                details.Fresh(___imapShowInfo);
                return false;

            }
        }
        public static void JoyStickConfirm(object[] obj)
        {
            if (curTransfer != null)
            {
                curTransfer.OnPointerClick(null);
                curTransfer = null;
            }
        }


        //[HarmonyPatch(typeof(GamingSolution), "UpdateMenuHotKey")]
        public static class UpdateMenuHotKey_Patch
        {
            public static bool Prefix()
            {
                if (!enabled)
                {
                    return true;
                }
                if (Module<ScenarioModule>.Self.CurrentScenarioName == "Main" && Module<PlayerActionModule>.Self.WasAcionPressed(ActionType.ActionMap))
                {
                    MessageManager.Instance.Dispatch("TransportMap", new object[] { }, DispatchType.IMME, 2f);
                    curTransfer = null;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapIconInteractTransfer), "GetQuestion")]
        public static class MapIconInteractTransfer_GetQuestion
        {
            public static bool Prefix(MapIconInteractTransfer __instance, IMap ___curImap, ref string __result)
            {
                if (!enabled)
                {
                    return true;
                }

                int busStopTransferTime = Module<SceneItemManager>.Self.GetBusStopTransferTime(___curImap);
                int num = busStopTransferTime / 60;
                int num2 = busStopTransferTime - num * 60;

                int tranId = Singleton<TransferTransIdDataBase>.Self.GetTranId((___curImap as SceneItemTransfer_IMap).SItem.ID);
                string name = (tranId <= 0) ? string.Empty : TextMgr.GetStr(tranId, -1);
                __result = string.Format($"<b>{name}</b>\r\n"+TextMgr.GetStr(91200043, -1), num.ToString(), num2.ToString(), Module<SceneItemManager>.Self.GetBusStopTransferMoney(___curImap));
                return false;
            }
        }
    }
}

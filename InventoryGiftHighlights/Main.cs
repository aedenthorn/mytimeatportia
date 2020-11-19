using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.FavorSystemNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.Grid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace InventoryGiftHighlights
{
    public partial class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        private static float indentSpace = 30f;
        private static float labelWidth = 80f;
        
        private static Sprite hate;
        private static Sprite dislike;
        private static Sprite neutral;
        private static Sprite like;
        private static Sprite love;
        private static Sprite hates;
        private static Sprite dislikes;
        private static Sprite neutrals;
        private static Sprite likes;
        private static Sprite loves;
        private static Sprite bkgs;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            CreateTextures(true);

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.Patch(
               original: AccessTools.Method(typeof(GiveGiftUICtr), "FreshPage"),
               postfix: new HarmonyMethod(typeof(Main), nameof(Update_Gifts))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GiveGiftUICtr), "SelectMain"),
               prefix: new HarmonyMethod(typeof(Main), nameof(Reset_Gifts)),
               postfix: new HarmonyMethod(typeof(Main), nameof(Update_Gifts))
            );
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
            CreateTextures();
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.ShowOnlyKnown = GUILayout.Toggle(settings.ShowOnlyKnown, "Only show known preferences", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.ShowHated = GUILayout.Toggle(settings.ShowHated, "Show hated", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.ShowDisliked = GUILayout.Toggle(settings.ShowDisliked, "Show disliked", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.ShowNeutral = GUILayout.Toggle(settings.ShowNeutral, "Show neutral", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.ShowLiked = GUILayout.Toggle(settings.ShowLiked, "Show liked", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.ShowLoved = GUILayout.Toggle(settings.ShowLoved, "Show loved", new GUILayoutOption[0]);
            GUILayout.Space(20f);

            GUILayout.Label("<b>Hate Color</b>", new GUILayoutOption[0]);

            GUILayout.BeginHorizontal();
            GUILayout.Space(indentSpace);
            GUILayout.BeginVertical();

            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ", settings.hateColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.hateColorRed = GUILayout.HorizontalSlider((float)settings.hateColorRed * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ", settings.hateColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.hateColorGreen = GUILayout.HorizontalSlider((float)settings.hateColorGreen * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ", settings.hateColorBlue), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.hateColorBlue = GUILayout.HorizontalSlider((float)settings.hateColorBlue * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ", settings.hateColorAlpha), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.hateColorAlpha = GUILayout.HorizontalSlider((float)settings.hateColorAlpha * 100f, 0, 100f) / 100f;
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);
            GUILayout.Label("<b>Dislike Color</b>", new GUILayoutOption[0]);

            GUILayout.BeginHorizontal();
            GUILayout.Space(indentSpace);
            GUILayout.BeginVertical();

            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ", settings.dislikeColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.dislikeColorRed = GUILayout.HorizontalSlider((float)settings.dislikeColorRed * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ", settings.dislikeColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.dislikeColorGreen = GUILayout.HorizontalSlider((float)settings.dislikeColorGreen * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ", settings.dislikeColorBlue), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.dislikeColorBlue = GUILayout.HorizontalSlider((float)settings.dislikeColorBlue * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ", settings.dislikeColorAlpha), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.dislikeColorAlpha = GUILayout.HorizontalSlider((float)settings.dislikeColorAlpha * 100f, 0, 100f) / 100f;
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);
            GUILayout.Label("<b>Neutral Color</b>", new GUILayoutOption[0]);

            GUILayout.BeginHorizontal();
            GUILayout.Space(indentSpace);
            GUILayout.BeginVertical();

            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ", settings.neutralColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.neutralColorRed = GUILayout.HorizontalSlider((float)settings.neutralColorRed * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ", settings.neutralColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.neutralColorGreen = GUILayout.HorizontalSlider((float)settings.neutralColorGreen * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ", settings.neutralColorBlue), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.neutralColorBlue = GUILayout.HorizontalSlider((float)settings.neutralColorBlue * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ", settings.neutralColorAlpha), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.neutralColorAlpha = GUILayout.HorizontalSlider((float)settings.neutralColorAlpha * 100f, 0, 100f) / 100f;
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            GUILayout.Label("<b>Like Color</b>", new GUILayoutOption[0]);

            GUILayout.BeginHorizontal();
            GUILayout.Space(indentSpace);
            GUILayout.BeginVertical();

            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ", settings.likeColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.likeColorRed = GUILayout.HorizontalSlider((float)settings.likeColorRed * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ", settings.likeColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.likeColorGreen = GUILayout.HorizontalSlider((float)settings.likeColorGreen * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ", settings.likeColorBlue), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.likeColorBlue = GUILayout.HorizontalSlider((float)settings.likeColorBlue * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ", settings.likeColorAlpha), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.likeColorAlpha = GUILayout.HorizontalSlider((float)settings.likeColorAlpha * 100f, 0, 100f) / 100f;
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            GUILayout.Label("<b>Love Color</b>", new GUILayoutOption[0]);

            GUILayout.BeginHorizontal();
            GUILayout.Space(indentSpace);
            GUILayout.BeginVertical();

            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ", settings.loveColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.loveColorRed = GUILayout.HorizontalSlider((float)settings.loveColorRed * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ", settings.loveColorRed), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.loveColorGreen = GUILayout.HorizontalSlider((float)settings.loveColorGreen * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ", settings.loveColorBlue), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.loveColorBlue = GUILayout.HorizontalSlider((float)settings.loveColorBlue * 100f, 0, 100f) / 100f;
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ", settings.loveColorAlpha), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.loveColorAlpha = GUILayout.HorizontalSlider((float)settings.loveColorAlpha * 100f, 0, 100f) / 100f;
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);
        }

        private static Texture2D TextureFromFile(string file)
        {
            Texture2D tex = new Texture2D(2, 2);
            byte[] imageData = File.ReadAllBytes(file);
            tex.LoadImage(imageData);
            return tex;
        }

        private static void CreateTextures(bool check = false)
        {
            if(check
                && File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "hate.png")))
                && File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "dislike.png")))
                && File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "neutral.png")))
                && File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "like.png")))
                && File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "love.png")))
                && File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "hates.png")))
                && File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "dislikes.png")))
                && File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "neutrals.png")))
                && File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "likes.png")))
                && File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "loves.png")))
            )
            {
                Dbgl($"Check: Textures exist, not recreating");
                ReloadTextures();
                return;
            }


            Dbgl($"Creating textures");

            string file = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\bg_item.png";
            string file2 = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\bg_item_select.png";
            string file3 = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\bg_item_select_orig.png";

            if (!File.Exists(file) || !File.Exists(file2))
            {
                Dbgl($"Files not found!");
                return;
            }

            Texture2D bkg = new Texture2D(2, 2);
            byte[] imageData = File.ReadAllBytes(file);
            bkg.LoadImage(imageData);

            Texture2D bkg2 = new Texture2D(2, 2);
            byte[] imageDatas = File.ReadAllBytes(file2);
            bkg2.LoadImage(imageDatas);

            Texture2D bkg3 = new Texture2D(2, 2);
            byte[] imageDatao = File.ReadAllBytes(file3);
            bkg3.LoadImage(imageDatao);


            Texture2D hateT = new Texture2D(83, 83);
            Texture2D dislikeT = new Texture2D(83, 83);
            Texture2D neutralT = new Texture2D(83, 83);
            Texture2D likeT = new Texture2D(83, 83);
            Texture2D loveT = new Texture2D(83, 83);
            Texture2D hateTs = new Texture2D(83, 83);
            Texture2D dislikeTs = new Texture2D(83, 83);
            Texture2D neutralTs = new Texture2D(83, 83);
            Texture2D likeTs = new Texture2D(83, 83);
            Texture2D loveTs = new Texture2D(83, 83);

            Color[] d = bkg.GetPixels();
            Color[] ds = bkg2.GetPixels();
            Color[] d1 = new Color[83 * 83];
            Color[] d2 = new Color[83 * 83];
            Color[] d3 = new Color[83 * 83];
            Color[] d4 = new Color[83 * 83];
            Color[] d5 = new Color[83 * 83];
            Color[] d6 = new Color[83 * 83];
            Color[] d7 = new Color[83 * 83];
            Color[] d8 = new Color[83 * 83];
            Color[] d9 = new Color[83 * 83];
            Color[] d10 = new Color[83 * 83];

            for (int i = 0; i < d.Length; i++)
            {
                //Dbgl($"Pixel color: {d[i]}");

                if (d[i] != Color.white)
                    continue;
                d1[i] = new Color(settings.hateColorRed, settings.hateColorGreen, settings.hateColorBlue, settings.hateColorAlpha);
                d2[i] = new Color(settings.dislikeColorRed, settings.dislikeColorGreen, settings.dislikeColorBlue, settings.dislikeColorAlpha);
                d3[i] = new Color(settings.neutralColorRed, settings.neutralColorGreen, settings.neutralColorBlue, settings.neutralColorAlpha);
                d4[i] = new Color(settings.likeColorRed, settings.likeColorGreen, settings.likeColorBlue, settings.likeColorAlpha);
                d5[i] = new Color(settings.loveColorRed, settings.loveColorGreen, settings.loveColorBlue, settings.loveColorAlpha);

                if (ds[i] == Color.white)
                {
                    d6[i] = new Color(settings.hateColorRed, settings.hateColorGreen, settings.hateColorBlue, settings.hateColorAlpha);
                    d7[i] = new Color(settings.dislikeColorRed, settings.dislikeColorGreen, settings.dislikeColorBlue, settings.dislikeColorAlpha);
                    d8[i] = new Color(settings.neutralColorRed, settings.neutralColorGreen, settings.neutralColorBlue, settings.neutralColorAlpha);
                    d9[i] = new Color(settings.likeColorRed, settings.likeColorGreen, settings.likeColorBlue, settings.likeColorAlpha);
                    d10[i] = new Color(settings.loveColorRed, settings.loveColorGreen, settings.loveColorBlue, settings.loveColorAlpha);
                }
                else
                {
                    d6[i] = ds[i];
                    d7[i] = ds[i];
                    d8[i] = ds[i];
                    d9[i] = ds[i];
                    d10[i] = ds[i];
                }
            }

            hateT.SetPixels(d1);
            dislikeT.SetPixels(d2);
            neutralT.SetPixels(d3);
            likeT.SetPixels(d4);
            loveT.SetPixels(d5);

            hateT.Apply(false);
            dislikeT.Apply(false);
            neutralT.Apply(false);
            likeT.Apply(false);
            loveT.Apply(false);

            hateTs.SetPixels(d6);
            dislikeTs.SetPixels(d7);
            neutralTs.SetPixels(d8);
            likeTs.SetPixels(d9);
            loveTs.SetPixels(d10);

            hateTs.Apply(false);
            dislikeTs.Apply(false);
            neutralTs.Apply(false);
            likeTs.Apply(false);
            loveTs.Apply(false);

            /*
            hate = Sprite.Create(hateT, new Rect(0, 0, 83, 83), Vector2.zero);
            dislike = Sprite.Create(dislikeT, new Rect(0, 0, 83, 83), Vector2.zero);
            neutral = Sprite.Create(neutralT, new Rect(0, 0, 83, 83), Vector2.zero);
            like = Sprite.Create(likeT, new Rect(0, 0, 83, 83), Vector2.zero);
            love = Sprite.Create(loveT, new Rect(0, 0, 83, 83), Vector2.zero);

            hates = Sprite.Create(hateTs, new Rect(0, 0, 83, 83), Vector2.zero);
            dislikes = Sprite.Create(dislikeTs, new Rect(0, 0, 83, 83), Vector2.zero);
            neutrals = Sprite.Create(neutralTs, new Rect(0, 0, 83, 83), Vector2.zero);
            likes = Sprite.Create(likeTs, new Rect(0, 0, 83, 83), Vector2.zero);
            loves = Sprite.Create(loveTs, new Rect(0, 0, 83, 83), Vector2.zero);
            */

            bkgs = Sprite.Create(bkg3, new Rect(0, 0, 83, 83), Vector2.zero);
            byte[] bytes;
            
            bytes = hateT.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "hate.png")), bytes);
            
            bytes = dislikeT.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "dislike.png")), bytes);
            
            bytes = neutralT.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "neutral.png")), bytes);
            
            bytes = likeT.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "like.png")), bytes);
            
            bytes = loveT.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "love.png")), bytes);

            bytes = hateTs.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "hates.png")), bytes);
            
            bytes = dislikeTs.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "dislikes.png")), bytes);
            
            bytes = neutralTs.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "neutrals.png")), bytes);
            
            bytes = likeTs.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "likes.png")), bytes);
            
            bytes = loveTs.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "loves.png")), bytes);
        }

        private static void ReloadTextures()
        {
            Dbgl($"Loading textures into memory");

            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "hate.png"));
            hate = Sprite.Create(TextureFromFile(file), new Rect(0, 0, 83, 83), Vector2.zero);
            file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "dislike.png"));
            dislike = Sprite.Create(TextureFromFile(file), new Rect(0, 0, 83, 83), Vector2.zero);
            file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "neutral.png"));
            neutral = Sprite.Create(TextureFromFile(file), new Rect(0, 0, 83, 83), Vector2.zero);
            file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "like.png"));
            like = Sprite.Create(TextureFromFile(file), new Rect(0, 0, 83, 83), Vector2.zero);
            file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "love.png"));
            love = Sprite.Create(TextureFromFile(file), new Rect(0, 0, 83, 83), Vector2.zero);

            file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "hates.png"));
            hates = Sprite.Create(TextureFromFile(file), new Rect(0, 0, 83, 83), Vector2.zero);
            file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "dislikes.png"));
            dislikes = Sprite.Create(TextureFromFile(file), new Rect(0, 0, 83, 83), Vector2.zero);
            file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "neutrals.png"));
            neutrals = Sprite.Create(TextureFromFile(file), new Rect(0, 0, 83, 83), Vector2.zero);
            file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "likes.png"));
            likes = Sprite.Create(TextureFromFile(file), new Rect(0, 0, 83, 83), Vector2.zero);
            file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("assets", "loves.png"));
            loves = Sprite.Create(TextureFromFile(file), new Rect(0, 0, 83, 83), Vector2.zero);

        }

        static void Reset_Gifts(ref GridPage ___page, Sprite ___normalBg)
        {
            for (int i = 0; i < ___page.allIcons.Count; i++)
            {
                GridIconWithNum gridIconWithNum = ___page.allIcons[i] as GridIconWithNum;
                gridIconWithNum.selectableBg.image.sprite = ___normalBg;
                SpriteState ss = gridIconWithNum.selectableBg.spriteState;
                ss.highlightedSprite = bkgs;
                ss.pressedSprite = bkgs;
                gridIconWithNum.selectableBg.spriteState = ss;
            }
        }
        static void Update_Gifts(ref GridPage ___page, List<ItemObject> ___allGiftItem, Actor ___targetActor, Sprite ___normalBg, Sprite ___wantedBg, int ___curItemIndex, WishItemData ___wishData)
        {
            ReloadTextures();
            int num = ___page.allIcons.Count * ___page.curPage;

            List<int> giftHistory = FavorUtility.GetGiftHistory(___targetActor.InstanceId);

            for (int i = 0; i < ___page.allIcons.Count; i++)
            {
                int num2 = i + num;
                GridIconWithNum gridIconWithNum = ___page.allIcons[i] as GridIconWithNum;
                SpriteState ss = gridIconWithNum.selectableBg.spriteState;
                if (num2 < ___allGiftItem.Count)
                {

                    if ((settings.ShowOnlyKnown && !giftHistory.Contains(___allGiftItem[num2].ItemBase.ID)))
                    {
                        continue;
                    }
                    if(___wishData != null && ___wishData.ItemId == ___allGiftItem[num2].ItemDataId)
                    {
                        Sprite sprite = ___wantedBg;
                        gridIconWithNum.selectableBg.image.sprite = sprite;
                        continue;
                    }

                    GiveGiftResult result = FavorUtility.GetFavorBehaviorInfo(___targetActor.InstanceId, ___allGiftItem[num2].ItemBase.ID);

                    //Dbgl($"num2 {num2} curItemIndex {___curItemIndex} i {i}");

                    switch (result.FeeLevel)
                    {
                        case FeeLevelEnum.Hate:
                            //Dbgl($"item {i}: {___allGiftItem[num2].ItemBase.ID} HATE");
                            if (settings.ShowHated)
                            {
                                gridIconWithNum.selectableBg.image.sprite = (num2 != ___curItemIndex ? hate : hates);
                                ss.highlightedSprite = hates;
                                ss.pressedSprite = hates;
                            }
                            break;
                        case FeeLevelEnum.DisLike:
                            //Dbgl($"item {i}: {___allGiftItem[num2].ItemBase.ID} DISLIKE");
                            if (settings.ShowDisliked)
                            {
                                gridIconWithNum.selectableBg.image.sprite = (num2 != ___curItemIndex ? dislike : dislikes);
                                ss.highlightedSprite = dislikes;
                                ss.pressedSprite = dislikes;
                            }
                            break;
                        case FeeLevelEnum.Neutral:
                            //Dbgl($"item {i}: {___allGiftItem[num2].ItemBase.ID} NEUTRAL");
                            if (settings.ShowNeutral)
                            {
                                gridIconWithNum.selectableBg.image.sprite = (num2 != ___curItemIndex ? neutral : neutrals);
                                ss.highlightedSprite = neutrals;
                                ss.pressedSprite = neutrals;
                            }
                            break;
                        case FeeLevelEnum.Like:
                            //Dbgl($"item {i}: {___allGiftItem[num2].ItemBase.ID} LIKE");
                            if (settings.ShowLiked)
                            {
                                gridIconWithNum.selectableBg.image.sprite = (num2 != ___curItemIndex ? like : likes);
                                ss.highlightedSprite = likes;
                                ss.pressedSprite = likes;
                            }
                            break;
                        case FeeLevelEnum.Excellent:
                            //Dbgl($"item {i}: {___allGiftItem[num2].ItemBase.ID} LOVE");
                            if (settings.ShowLoved)
                            {
                                gridIconWithNum.selectableBg.image.sprite = (num2 != ___curItemIndex ? love : loves);
                                ss.highlightedSprite = loves;
                                ss.pressedSprite = loves;
                            }
                            break;
                    }
                }
                else
                {
                    ss.highlightedSprite = ___normalBg;
                    ss.pressedSprite = ___normalBg;
                }
                gridIconWithNum.selectableBg.spriteState = ss;
            }
        }
    }
}

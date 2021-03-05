using Harmony12;
using Hont.ExMethod.Collection;
using Pathea;
using Pathea.ACT;
using Pathea.ActorNs;
using Pathea.FavorSystemNs;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using Pathea.ScenarioNs;
using PatheaScriptExt;
using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace RelationManager
{
    public partial class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }

		private static int[] relationEnum = new int[] { 
			-1,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,20,21,25,26,27,28,29,40,41
		};


		private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

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
			float infoBoxWidth = (typeof(UnityModManager).GetProperty("Params", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, new object[] { }) as UnityModManager.Param).WindowWidth * 0.9f;
			if (infoBoxWidth == 0)
				infoBoxWidth = 960;

			float buttonWidthShort = infoBoxWidth / 7f;

			float buttonWidthMedium = infoBoxWidth / 2f;


			if (Module<FavorManager>.Self?.GetAllShowFavorObjects() == null)
			{
				npcContent = null;
				return;
			}
			if (npcContent == null || npcContent.Length != Module<FavorManager>.Self.GetAllShowFavorObjects().Length + 1)
			{
				npcContent = new string[Module<FavorManager>.Self.GetAllShowFavorObjects().Length + 1];
			}

			string[] tempNPC = new string[Module<FavorManager>.Self.GetAllShowFavorObjects().Length];

			for (int i = 0; i < Module<FavorManager>.Self.GetAllShowFavorObjects().Length; i++)
			{
				FavorObject favorObject = Module<FavorManager>.Self.GetAllShowFavorObjects()[i];
				NpcData npcData = Module<FavorManager>.Self.npcData[i];
				string text = (npcData == null) ? favorObject.ID.ToString() : npcData.Name;
				string text2 = (!favorObject.IsDebut) ? string.Empty : "(?) ";
				tempNPC[i] = string.Concat(new object[]
				{
					text,
					" ",
					text2,
					favorObject.ID,
					" ",
					FavorRelationshipUtil.GetGenderRelation(npcData.gender, favorObject.Relationship),
					" ",
					favorObject.FavorValue
				});
			}
			tempNPC.CopyTo(npcContent, 0);
			string[] sortedNPC = new string[Module<FavorManager>.Self.GetAllShowFavorObjects().Length + 1];
			Array.Sort(tempNPC);
			tempNPC.CopyTo(sortedNPC, 0);
			npcContent[npcContent.Length - 1] = "All NPCs";
			sortedNPC[sortedNPC.Length - 1] = "All NPCs";

			GUILayout.Box("NPC Count：" + Module<FavorManager>.Self.GetAllShowFavorObjects().Length, new GUILayoutOption[0]);
			realSelectIndex = GUILayout.SelectionGrid(realSelectIndex, sortedNPC, 2, new GUILayoutOption[]
			{
				GUILayout.Width((float)infoBoxWidth)
			});
			selectIndex = npcContent.IndexOf(sortedNPC[realSelectIndex]);
			GUILayout.Space(10);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button("Talk", new GUILayoutOption[]
			{
				GUILayout.Width((float)buttonWidthShort)
			}))
			{
				if (selectIndex < npcContent.Length - 1)
				{
					FavorObject favorObject2 = Module<FavorManager>.Self.GetAllShowFavorObjects()[selectIndex];
					IFavorBehavior favorBehavior = favorObject2.FavorBehaviorList.Find((IFavorBehavior it) => it is FavorBehavior_Dialog);
					favorBehavior.Execute(favorObject2, null);
					favorObject2.IsDebut = false;
				}
				else
				{
					foreach (FavorObject favorObject3 in Module<FavorManager>.Self.GetAllShowFavorObjects())
					{
						IFavorBehavior favorBehavior2 = favorObject3.FavorBehaviorList.Find((IFavorBehavior it) => it is FavorBehavior_Dialog);
						favorBehavior2.Execute(favorObject3, null);
						favorObject3.IsDebut = false;
					}
				}
			}
			giftIdStr = GUILayout.TextField(giftIdStr, new GUILayoutOption[]
			{
				GUILayout.Width(58f)
			});
			if (GUILayout.Button("Gift", new GUILayoutOption[]
			{
				GUILayout.Width((float)buttonWidthShort)
			}))
			{
				int num;
				int.TryParse(giftIdStr, out num);
				if (selectIndex < npcContent.Length - 1)
				{
					FavorObject favorObject4 = Module<FavorManager>.Self.GetAllShowFavorObjects()[selectIndex];
					IFavorBehavior favorBehavior3 = favorObject4.FavorBehaviorList.Find((IFavorBehavior it) => it is FavorBehavior_GiveGift);
					favorBehavior3.Execute(favorObject4, new object[]
					{
						num
					});
					favorObject4.IsDebut = false;
				}
				else
				{
					foreach (FavorObject favorObject5 in Module<FavorManager>.Self.GetAllShowFavorObjects())
					{
						IFavorBehavior favorBehavior4 = favorObject5.FavorBehaviorList.Find((IFavorBehavior it) => it is FavorBehavior_GiveGift);
						favorBehavior4.Execute(favorObject5, new object[]
						{
							num
						});
						favorObject5.IsDebut = false;
					}
				}
			}
			addFavor = GUILayout.TextField(addFavor, new GUILayoutOption[]
			{
				GUILayout.Width(40f)
			});
			if (GUILayout.Button("Add", new GUILayoutOption[]
			{
				GUILayout.Width((float)buttonWidthShort)
			}))
			{
				int gainFavorValue;
				int.TryParse(addFavor, out gainFavorValue);
				if (selectIndex < npcContent.Length - 1)
				{
					FavorObject favorObject6 = Module<FavorManager>.Self.GetAllShowFavorObjects()[selectIndex];
					favorObject6.GainFavorValue(-1, gainFavorValue, true, true, true);
					favorObject6.IsDebut = false;
				}
				else
				{
					foreach (FavorObject favorObject7 in Module<FavorManager>.Self.GetAllShowFavorObjects())
					{
						Actor actor = Module<ActorMgr>.Self.Get(favorObject7.ID);
						if (!(actor == null) && (!(actor != null) || !actor.InFarAwayScene()))
						{
							favorObject7.GainFavorValue(-1, gainFavorValue, true, true, true);
							favorObject7.IsDebut = false;
						}
					}
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button("Love", new GUILayoutOption[]
			{
				GUILayout.Width((float)buttonWidthShort)
			}))
			{
				if (selectIndex < npcContent.Length - 1)
				{
					FavorObject favorObject10 = Module<FavorManager>.Self.GetAllShowFavorObjects()[selectIndex];
					FavorUtility.SetNpcRelation(favorObject10.ID, FavorRelationshipId.NormalLover, 0);
					favorObject10.IsDebut = false;
				}
				else
				{
					foreach (FavorObject favorObject11 in Module<FavorManager>.Self.GetAllShowFavorObjects())
					{
						FavorUtility.SetNpcRelation(favorObject11.ID, FavorRelationshipId.NormalLover, 0);
						favorObject11.IsDebut = false;
					}
				}
			}
			if (GUILayout.Button("BreakUp", new GUILayoutOption[]
			{
				GUILayout.Width((float)buttonWidthShort)
			}))
			{
				if (selectIndex < npcContent.Length - 1)
				{
					FavorObject favorObject12 = Module<FavorManager>.Self.GetAllShowFavorObjects()[selectIndex];
					FavorRelationLover.Breakup(favorObject12);
					favorObject12.IsDebut = false;
				}
				else
				{
					foreach (FavorObject favorObject13 in Module<FavorManager>.Self.GetAllShowFavorObjects())
					{
						FavorRelationLover.Breakup(favorObject13);
						favorObject13.IsDebut = false;
					}
				}
			}
			if (GUILayout.Button("Marry", new GUILayoutOption[]
			{
				GUILayout.Width((float)buttonWidthShort)
			}))
			{
				if (selectIndex < npcContent.Length - 1)
				{
					FavorObject favorObject14 = Module<FavorManager>.Self.GetAllShowFavorObjects()[selectIndex];
					FavorUtility.SetNpcRelation(favorObject14.ID, FavorRelationshipId.NormalCouple, 0);
					favorObject14.IsDebut = false;
				}
				else
				{
					foreach (FavorObject favorObject15 in Module<FavorManager>.Self.GetAllShowFavorObjects())
					{
						favorObject15.IsDebut = false;
						FavorUtility.SetNpcRelation(favorObject15.ID, FavorRelationshipId.NormalCouple, 0);
					}
				}
			}
			if (GUILayout.Button("Divorce", new GUILayoutOption[]
			{
				GUILayout.Width((float)buttonWidthShort)
			}))
			{
				if (selectIndex < npcContent.Length - 1)
				{
					FavorObject favorObject16 = Module<FavorManager>.Self.GetAllShowFavorObjects()[selectIndex];
					FavorRelationMarriage.DoDivorce(favorObject16);
					favorObject16.IsDebut = false;
				}
				else
				{
					foreach (FavorObject favorObject17 in Module<FavorManager>.Self.GetAllShowFavorObjects())
					{
						FavorRelationMarriage.DoDivorce(favorObject17);
						favorObject17.IsDebut = false;
					}
				}
			}
			if (GUILayout.Button("Move", new GUILayoutOption[]
			{
				GUILayout.Width((float)buttonWidthShort)
			}))
			{
				if (selectIndex < npcContent.Length - 1)
				{
					FavorObject favorObject18 = Module<FavorManager>.Self.GetAllShowFavorObjects()[selectIndex];
					if (favorObject18 != null)
					{
						Actor actor2 = StoryHelper.GetActor(favorObject18.ID, string.Empty);
						if (actor2 == null)
						{
							actor2 = Module<NpcRepository>.Self.CreateNpc(favorObject18.ID);
							Debug.Log(string.Concat(new object[]
							{
								"创建NPC->",
								favorObject18.ID,
								" Name:",
								(!actor2) ? "Null" : actor2.ActorName,
								" 于地点：",
								Module<Player>.Self.actor.gamePos,
								new Vector3(0f, 2f, 0f)
							}));
						}
						if (actor2 != null)
						{
							actor2.ClearActionQueue();
							actor2.DoCommand(ACType.Transfer, ACTransferPara.Construct(Module<ScenarioModule>.Self.CurrentScenarioName, Module<Player>.Self.actor.gamePos + new Vector3(0f, 2f, 0f), Vector3.zero));
						}
					}
				}
				else
				{
					foreach (FavorObject favorObject19 in Module<FavorManager>.Self.GetAllShowFavorObjects())
					{
						if (favorObject19 != null)
						{
							Actor actor3 = StoryHelper.GetActor(favorObject19.ID, string.Empty);
							if (actor3 == null)
							{
								actor3 = Module<NpcRepository>.Self.CreateNpc(favorObject19.ID);
								Debug.Log(string.Concat(new object[]
								{
									"创建NPC->",
									favorObject19.ID,
									" Name:",
									(!actor3) ? "Null" : actor3.ActorName,
									" 于地点：",
									Module<Player>.Self.actor.gamePos,
									new Vector3(0f, 2f, 0f)
								}));
							}
							if (actor3 != null)
							{
								actor3.ClearActionQueue();
								actor3.DoCommand(ACType.Transfer, ACTransferPara.Construct(Module<ScenarioModule>.Self.CurrentScenarioName, Module<Player>.Self.actor.gamePos + new Vector3(0f, 2f, 0f), Vector3.zero));
							}
						}
					}
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label($"Relation: {Enum.GetName(typeof(FavorRelationshipId), relationEnum[relationId])}", new GUILayoutOption[] { GUILayout.Width(150) });
			relationId = (int)GUILayout.HorizontalSlider(relationId, 0, relationEnum.Length - 1, new GUILayoutOption[0]);
			if (GUILayout.Button("Set", new GUILayoutOption[]
			{
				GUILayout.Width(150)
			}))
			{
				try
				{
					int relationIdEnum = relationEnum[relationId];
					if (selectIndex < npcContent.Length - 1)
					{
						FavorObject favorObject8 = Module<FavorManager>.Self.GetAllShowFavorObjects()[selectIndex];
						FavorUtility.SetNpcRelation(favorObject8.ID, (FavorRelationshipId)relationIdEnum, 0);
						favorObject8.IsDebut = false;
					}
					else
					{
						foreach (FavorObject favorObject9 in Module<FavorManager>.Self.GetAllShowFavorObjects())
						{
							FavorUtility.SetNpcRelation(favorObject9.ID, (FavorRelationshipId)relationIdEnum, 0);
							favorObject9.IsDebut = false;
						}
					}

				}
				catch { }
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(20);

		}

		private static string addFavor = string.Empty;

		private static string giftIdStr = string.Empty;

		private static int relationId;

		private static int realSelectIndex;

		private static int selectIndex;

		private static string[] npcContent;
	}
}

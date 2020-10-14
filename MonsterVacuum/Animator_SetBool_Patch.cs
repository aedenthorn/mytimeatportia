using Harmony12;
using Hont.ExMethod.Collection;
using Pathea;
using Pathea.ActorNs;
using Pathea.DungeonModuleNs;
using Pathea.FeatureNs;
using Pathea.ItemDropNs;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.Spawn;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonsterVacuum
{
    public partial class Main
    {
        [HarmonyPatch(typeof(Animator), "SetBool", new Type[]{typeof(string),typeof(bool)})]
        static class Animator_SetBool_Patch
        {
            static void Prefix(string name)
            {
                if (!enabled)
                    return;

                //Dbgl("animation "+ name);
                switch (name)
                {
                    case "XiChengQi":
                        VacuumLoot();
                        break;
                }

            }

            private static void VacuumLoot()
            {
                try
                {
                    Dbgl($"Vacuuming monsters in {ScenarioModule.Self.CurrentScenarioName}");

                    Actor actor = null;

                    if (ScenarioModule.Self.IsMainScene())
                    {
                        List<ActorAgent> actorAgents = AccessTools.FieldRefAccess<SpawnMgr, List<ActorAgent>>("mActorAgent").Invoke(Module<SpawnMgr>.Self);

                        Dbgl($"actorAgents: {actorAgents.Count}");

                        foreach (ActorAgent aa in actorAgents)
                        {
                            Actor a = AccessTools.FieldRefAccess<ActorAgent, Actor>("_actor").Invoke(aa);
                            if (a == null)
                            {
                                continue;
                            }
                            Dbgl($"checking actor {a.ActorName}");
                            if (Vector2.Distance(a.gamePos, Player.Self.GamePos) < settings.VacuumRadius && !vacuumed.Contains(a.InstanceId))
                            {
                                if (actor == null || (Vector2.Distance(a.gamePos, Player.Self.GamePos) < Vector2.Distance(actor.gamePos, Player.Self.GamePos)))
                                    actor = a;
                            }
                        }
                    }
                    else
                    {

                        List<MonsterData> monsterList = TrialRandomDungeonManager.Self.monsterList;
                        Dbgl($"trial monsters: {monsterList.Count}");

                        foreach (MonsterData md in monsterList)
                        {
                            Actor a = md.actor;
                            if (a == null)
                            {
                                continue;
                            }
                            Dbgl($"checking actor {a.ActorName}");
                            if (Vector2.Distance(a.gamePos, Player.Self.GamePos) < settings.VacuumRadius && !vacuumed.Contains(a.InstanceId))
                            {
                                if (actor == null || (Vector2.Distance(a.gamePos, Player.Self.GamePos) < Vector2.Distance(actor.gamePos, Player.Self.GamePos)))
                                {
                                    actor = a;
                                }
                            }
                        }
                        List<Actor> actors = AccessTools.FieldRefAccess<DynamicRoomSceneManager, List<Actor>>("MonsterListForDelete").Invoke(Module<DynamicRoomSceneManager>.Self);
                        Dbgl($"dynamic room monsters: {actors.Count}");
                        foreach (Actor a in actors)
                        {
                            if (a == null)
                            {
                                continue;
                            }
                            Dbgl($"checking actor {a.ActorName}");
                            if (Vector2.Distance(a.gamePos, Player.Self.GamePos) < settings.VacuumRadius && !vacuumed.Contains(a.InstanceId))
                            {
                                if (actor == null || (Vector2.Distance(a.gamePos, Player.Self.GamePos) < Vector2.Distance(actor.gamePos, Player.Self.GamePos)))
                                    actor = a;
                            }
                        }
                    }

                    if (actor == null)
                    {
                        Dbgl($"No monster in range");
                        return;
                    }
                    Dbgl($"Vacuuming monster: distance: {Vector2.Distance(actor.gamePos, Player.Self.GamePos)} drop group: {Module<ActorMgr>.Self.GetActorInfo(actor.TmpltId).dropGroup} monster: {actor.MonsterTag}");

                    float scaleNum = 1f + Module<FeatureModule>.Self.ModifyFloat(FeatureType.MonsterDrop, new object[] { 1f });
                    float expScale = actor.ExpScale;
                    Module<ItemDropManager>.Self.DropItemListByIdWithScale(Module<ActorMgr>.Self.GetActorInfo(actor.TmpltId).dropGroup, actor.transform.position, scaleNum, expScale);
                    vacuumed.Add(actor.InstanceId);
                }
                catch(Exception ex)
                {
                    Dbgl($"Error dropping loot:\r\n\r\n {ex}");
                }
            }
        }
    }
}

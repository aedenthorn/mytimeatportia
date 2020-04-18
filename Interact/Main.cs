using Harmony12;
using Pathea;
using Pathea.ACT;
using Pathea.ActorAction;
using Pathea.ActorNs;
using Pathea.Behavior;
using Pathea.EG;
using Pathea.FavorSystemNs;
using Pathea.GameFlagNs;
using Pathea.Interactive;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using Pathea.ScenarioNs;
using Pathea.UISystemNs;
using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BehaviorDesigner.Runtime;
using Pathea.ConfigNs;
using Pathea.EngagementsNs.GameNs;
using Pathea.MiniGameNs;
using Pathea.TipsNs;
using UnityEngine;
using UnityModManagerNet;
using static Harmony12.AccessTools;
using Animation = UnityEngine.Animation;
using InteractType = Pathea.ActorNs.InteractType;

namespace Interact
{
    public partial class Main
    {
        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "InteractMod " : "") + str);
        }
        public static void TpSend(string str = "")
        {
            if (isDebug)
                Singleton<TipsMgr>.Instance.SendSystemTip(str, SystemTipType.warning);
        }
        public static void WriteToModDir(string str = "")
        {
            if (isDebug)
            {
                string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\log.txt";

                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(path, true))
                {
                    file.WriteLine(str);
                }
            }
        }
        public static bool enabled;
        public static Settings settings { get; private set; }

        // Send a1 response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //MessageManager.Instance.Subscribe("InteractModAnimEnd", ResetAnimations);

            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static string relationshipName(int rel)
        {
            string str = "None";
            int minRel = -1;
            Array values = Enum.GetValues(typeof(FavorRelationshipId));

            foreach (FavorRelationshipId val in values)
            {
                if (rel >= (int) val && (int)val > minRel)
                {
                    minRel = (int) val;
                    str = Enum.GetName(typeof(FavorRelationshipId), val);
                }
            }
            return str;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Max distance to interact: <b>{0:F1}m</b>", settings.MaxInteractDistance), new GUILayoutOption[0]);
            settings.MaxInteractDistance = GUILayout.HorizontalSlider(settings.MaxInteractDistance * 10f, 10f, 1000f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Time to hug for: <b>{0:F1}s</b>", settings.HugTime), new GUILayoutOption[0]);
            settings.HugTime = GUILayout.HorizontalSlider(settings.HugTime * 10f, 10f, 1000f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(20);
            GUILayout.Label($"Minimum relationship to allow hugs: <b>{relationshipName(settings.HugRelationshipMin)}</b>", new GUILayoutOption[0]);
            settings.HugRelationshipMin = (int)GUILayout.HorizontalSlider(settings.HugRelationshipMin, -1, 15, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label($"Minimum relationship to allow kisses: <b>{relationshipName(settings.KissRelationshipMin)}</b>", new GUILayoutOption[0]);
            settings.KissRelationshipMin = (int)GUILayout.HorizontalSlider(settings.KissRelationshipMin, -1, 15, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label($"Minimum relationship to allow massages: <b>{relationshipName(settings.MassageRelationshipMin)}</b>", new GUILayoutOption[0]);
            settings.MassageRelationshipMin = (int)GUILayout.HorizontalSlider(settings.MassageRelationshipMin, -1, 15, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Hug Key"), new GUILayoutOption[0]);
            settings.HugKey = GUILayout.TextField(settings.HugKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Kiss Key"), new GUILayoutOption[0]);
            settings.KissKey = GUILayout.TextField(settings.KissKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Massage Key"), new GUILayoutOption[0]);
            settings.MassageKey = GUILayout.TextField(settings.MassageKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            settings.AddRelationshipPoints = GUILayout.Toggle(settings.AddRelationshipPoints, "Add relationship points for quick interactions", new GUILayoutOption[0]);
            if (settings.AddRelationshipPoints)
            {
                GUILayout.Space(20);
                settings.LimitRelationshipPointGain = GUILayout.Toggle(settings.LimitRelationshipPointGain, "Respect daily limits on relationship gain", new GUILayoutOption[0]);
            }
            GUILayout.Space(20);
            settings.ReplaceTeeterTotterGame = GUILayout.Toggle(settings.ReplaceTeeterTotterGame, "Replace the teeter totter game with an interaction game", new GUILayoutOption[0]);
        }
        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static bool isHugging = false;
        private static float targetTime = 0;
        private static List<Actor> hugTargets = new List<Actor>();

        [HarmonyPatch(typeof(Player), "Update")]
        static class Player_Patch
        {
            static void Postfix()
            {
                if (isHugging)
                {
                    //Dbgl($"is hugging: {targetTime}");
                    targetTime -= Time.deltaTime;

                    if (targetTime <= 0.0f)
                    {
                        Dbgl("done hugging");
                        isHugging = false;
                        foreach (Actor a in hugTargets)
                        {
                            ResetOneAnimation(a);
                        }
                        hugTargets.Clear();
                        return;
                    }
                }
                if (!Singleton<GameFlag>.Self.Gaming || (!Input.GetKeyDown(settings.KissKey) && !Input.GetKeyDown(settings.MassageKey) &&
                    !Input.GetKeyDown(settings.HugKey)) || UIStateMgr.Instance.currentState.type != UIStateMgr.StateType.Play || Player.Self.actor == null)
                    return;

                Transform pt = Player.Self.actor.transform;

                Vector3 forward = pt.TransformDirection(Vector3.forward);

                List<NpcData> npcs = FieldRefAccess<NpcRepository, List<NpcData>>(NpcRepository.Self,"mNpcInstanceDatas"); 
                
                float minDistance = settings.MaxInteractDistance;
                Actor actor = null;
                foreach (NpcData npcData in npcs)
                {
                    int npc = npcData.id;
                    Dbgl($"Checking for {NpcRepository.Self.GetNpcName(npc)}");
                    Actor other = ActorMgr.Self.Get(npc);
                    if (other == null || !other.InActiveScene)
                    {
                        //Dbgl($"{NpcRepository.Self.GetNpcName(npc)} is not in scene!");
                        continue;
                    }
                    Transform ot = other.transform;
                    Vector3 toOther = ot.position - pt.position;
                    float dot = Vector3.Dot(forward, toOther);
                    if (dot < 0) // behind us
                    {
                        //Dbgl($"{NpcRepository.Self.GetNpcName(npc)} is behind us: {dot}");
                        continue;
                    }

                    float distance = Vector3.Distance(pt.position, ot.position);
                    if (distance < minDistance)
                    {
                        Dbgl($"{NpcRepository.Self.GetNpcName(npc)} is near ({distance}) and in front of us ({dot})");
                        minDistance = distance;
                        actor = other;
                    }
                }

                if (actor == null)
                {
                    Dbgl("no actor near and in front of us");
                    return;
                }
                int relationship = FavorRelationshipUtil.GetRelationShip(actor.InstanceId);
                Dbgl($"Relationship for {actor.ActorName}: {relationship}");

                if (Input.GetKeyDown(settings.KissKey))
                {

                    Dbgl("kissing");
                    if (settings.KissRelationshipMin > -1 && relationship < settings.KissRelationshipMin)
                    {

                        Singleton<TipsMgr>.Instance.SendSystemTip($"Your relationship with {actor.ActorName} is not high enough to kiss them!", SystemTipType.warning);
                        return;
                    }
                    string modelType = Module<ActorMgr>.Self.GetActorInfo(actor.TmpltId).modelType;
                    DoInteract(Player.Self.actor,actor,1);

                }
                else if (Input.GetKeyDown(settings.HugKey))
                {
                    Dbgl("hugging");
                    if (settings.HugRelationshipMin > -1 && relationship < settings.HugRelationshipMin)
                    {

                        Singleton<TipsMgr>.Instance.SendSystemTip($"Your relationship with {actor.ActorName} is not high enough to hug them!", SystemTipType.warning);
                        return;
                    }

                    DoInteract(Player.Self.actor,actor,0);

                }
                else if (Input.GetKeyDown(settings.MassageKey))
                {
                    Dbgl("getting massaged");
                    if (settings.MassageRelationshipMin > -1 && relationship < settings.MassageRelationshipMin)
                    {
                        Singleton<TipsMgr>.Instance.SendSystemTip($"Your relationship with {actor.ActorName} is not high enough to get a massage from them!", SystemTipType.warning);
                        return;
                    }

                    DoInteract(Player.Self.actor,actor,2);

                }
            }
        }

        //[HarmonyPatch(typeof(ITStartAction), "OnStart")]
        static class ITStartAction_Patch
        {
            static void Prefix(ref SharedFloat ___femaleDistance, ref SharedFloat ___distance, SharedActor ___actor)
            {
                //Dbgl(Environment.StackTrace);
                //Actor actor = ___actor.Value;
                //WriteToModDir($"{{\"{actor.ActorName}\", new List<float>{{{___femaleDistance}f, {___distance}f}},");

            }
        }

        [HarmonyPatch(typeof(ITAnimation), "OnStart")]
        static class ITAnimation_Patch
        {
            static void Prefix(ITAnimation __instance, Actor ___actor)
            {
                Dbgl($"animation: {__instance.hugAnimName.Value}, ikName: {__instance.ikName.Value}");
                //WriteToModDir(__instance.hugAnimName.Value);

            }
        }


        public static bool CanInteract(Actor a, Actor ao)
        {

            if (!a.CanInteract || a.InBattle || !a.Visible || a.IsActionRunning(ACType.Sleep) || a.IsActionRunning(ACType.Sit) || a.IsActionRunning(ACType.Conversation) || a.IsActionRunning(ACType.Interact) || Module<EGMgr>.Self.IsEngagementEvent(a.InstanceId) || a.OnBus || a.IsInteractive())
            {
                Singleton<TipsMgr>.Instance.SendSystemTip($"You cannot interact right now!", SystemTipType.warning);
                Dbgl("a1 cannot interact");
                return false;
            }
            if (ao.InstanceId == a.InstanceId || !ao.CanInteract || ao.InBattle || !ao.Visible || ao.IsActionRunning(ACType.Sleep) || ao.IsActionRunning(ACType.Sit) || ao.IsActionRunning(ACType.Conversation) || ao.IsActionRunning(ACType.Interact) || Module<EGMgr>.Self.IsEngagementEvent(ao.InstanceId) || ao.OnBus || ao.IsInteractive())
            {
                Singleton<TipsMgr>.Instance.SendSystemTip($"{ao.ActorName} cannot interact right now!", SystemTipType.warning);
                Dbgl("a2 cannot interact");
                return false;
            }

            return true;
        }

        private static TurnOffGravity turnOffGravity = new TurnOffGravity();

        private static Dictionary<string, string> iKNames = new Dictionary<string, string>(){ {"Interact_Kiss","IKPeck"}, {"Interact_Kiss_Tall","IKPeck"},{"Interact_Massage","IKMassage"},{"Hug","IKHug"},{"Petting_Chuan","IKPetting"},{"Petting","IKPetting"},{"Petting_Dog","IKPetting"},{"Embrace_Chuan","IKPetting"},{"Embrace_Dog","IKPetting"},{"Embrace_Cat","IKPetting"} };
        private static Dictionary<string, Pathea.ActorNs.InteractType> animTypes = new Dictionary<string, Pathea.ActorNs.InteractType>(){ {"Interact_Kiss",InteractType.Kiss}, {"Interact_Kiss_Tall", InteractType.Kiss},{"Interact_Massage", InteractType.Massage},{"Hug", InteractType.Embrace} };

        private static List<AnimationPair> actorPairs = new List<AnimationPair>();

        private static Dictionary<string,List<float>> distanceList = new Dictionary<string, List<float>>(){
            {"Sonia", new List<float> {-0.02f,0f}},
            {"Alice", new List<float> {-0.27f,-0.25f}},
            {"Petra", new List<float> {-0.30f,-0.25f}},
            {"Arlo", new List<float> {-0.05f,-0.04f}},
            {"Phyllis", new List<float> {-0.05f,-0.01f}},
            {"Django", new List<float> {-0.05f,-0.05f}},
            {"Nora", new List<float> {-0.04f,0.031f}},
            {"Sam", new List<float> {-0.05f,0.035f}},
            {"Paulie", new List<float> {0.08f, 0.09f}},
            {"Liuwa", new List<float>{-0.15f, -0.15f}},
        };

        public class AnimationPair
        {
            public Actor a1;
            public Actor a2;
            public string animName;

            public AnimationPair(string animName, Actor a1, Actor a2)
            {
                this.a1 = a1;
                this.a2 = a2;
                this.animName = animName;
            }
        }

        public static void DoInteractTest(Actor a, Actor ao, string animName)
        {
            Module<Player>.Self.SetInteractive(ao, Pathea.ActorNs.InteractType.None);
            Module<Player>.Self.InteractiveStart(ao, Pathea.ActorNs.InteractType.Kiss);
        }

        public static float GetDistance(Actor ao, string animName)
        {
            var num = (Module<Player>.Self.GetGender() != Gender.Male) ? -0.05f : -0.03f;

            if (animName == "Interact_Kiss")
            {
                if (distanceList.ContainsKey(ao.ActorName))
                    num = (Module<Player>.Self.GetGender() != Gender.Male) ? distanceList[ao.ActorName][0] : distanceList[ao.ActorName][1];
                else if (Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType == "Npc_Tall")
                        num = (Module<Player>.Self.GetGender() != Gender.Male) ? distanceList["Arlo"][0] : distanceList["Arlo"][1];
                else if (Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType == "Npc_Strong")
                    num = (Module<Player>.Self.GetGender() != Gender.Male) ? distanceList["Paulie"][0] : distanceList["Paulie"][1];
                else if (Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType == "Npc_Medium")
                    num = (Module<Player>.Self.GetGender() != Gender.Male) ? distanceList["Nora"][0] : distanceList["Nora"][1];
                else if (Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType == "Npc_Thin")
                    num = (Module<Player>.Self.GetGender() != Gender.Male) ? distanceList["Petra"][0] : distanceList["Petra"][1];
                else if (Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType == "Npc_Fat")
                    num = (Module<Player>.Self.GetGender() != Gender.Male) ? distanceList["Liuwa"][0] : distanceList["Liuwa"][1];
            }
            else if (animName == "Hug")
            {
                num = (Module<Player>.Self.GetGender() != Gender.Male) ? -0.015f : 0.18f;
            }
            else if (animName == "Interact_Massage")
            {
                num = -0.1f;
            }

            return num;
        }

        public static bool isInteracting = false;

        public static void DoInteract(Actor a, Actor ao, int anim)
        {
            if ((!teeterGameRunning && !CanInteract(a, ao)) || (anim == 1 && Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType == "Npc_Little"))
                return;

            string animName = Interacts[anim];

            Dbgl($"DoInteract: animName {animName} modelType {Module<ActorMgr>.Self.GetActorInfo(a.TmpltId).modelType} {Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType}");
            if (Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType == "0")
            {
                animName = CheckPet(ao, animName);
                Dbgl($"pet anim: {animName}");
            }

            bool isTall = animName == "Interact_Kiss" && (Module<ActorMgr>.Self.GetActorInfo(a.TmpltId).modelType == "Npc_Tall" || Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType == "Npc_Tall");
            string mAnimName = animName;
            if (isTall)
                mAnimName += "_Tall";


            Vector3 pos1 = a.gamePos;
            Vector3 pos2 = ao.gamePos;

            float dist = Vector3.Distance(pos1, pos2);

            float _radiusSelf = a.GetActorRadius();
            float _radiusTar = ao.GetActorRadius(); //0.25, 0.28 if tall

            float num = GetDistance(ao, animName);

            float _dis = _radiusSelf + _radiusTar + num;

            Vector3 _tarDir = ao.gamePos - a.gamePos;
            _tarDir.y = 0f;
            Vector3 tarPos;
            if (Module<ScenarioModule>.Self.CurrentScenarioName == "Main")
            {
                Util.TryGetGroundPosition(out tarPos, a.gamePos, _tarDir, _dis, 3f, 3f, true, 0.1f, _radiusTar, 36);
            }
            else
            {
                Util.TryGetGroundPosition(out tarPos, a.gamePos, _tarDir, _dis, 2f, 2f, true, 0.1f, _radiusTar, 36);
            }

            float y = Mathf.Max(a.gamePos.y, tarPos.y);

            Vector3 apos = a.gamePos;
            Vector3 arot = a.gameRot.eulerAngles;

            Vector3 aopos = ao.gamePos;
            Vector3 aorot = ao.gameRot.eulerAngles;

            Vector3 _sefPos = a.gamePos;
            _sefPos.y = y;
            Vector3 _tarPos = tarPos;
            _tarPos.y = y;
            Vector3 _sefRot = Quaternion.LookRotation(_tarPos - _sefPos, Vector3.up).eulerAngles;
            Vector3 _tarRot = Quaternion.LookRotation(_sefPos - _tarPos, Vector3.up).eulerAngles;
            Vector3 vector = (_sefPos + _tarPos) * 0.5f;
            if (a.TryDoAction(ACType.Transfer, ACTransferPara.Construct(string.Empty, _sefPos, _sefRot)))
            {
                if (ao.TryDoAction(ACType.Transfer, ACTransferPara.Construct(string.Empty, _tarPos, _tarRot)))
                {
                    a.StopAction(ACType.Animation, false);
                    if (a.TryDoAction(ACType.Animation, ACTAnimationPara.Construct(mAnimName, null, null, true)))
                    {
                        ao.StopAction(ACType.Animation, false);
                        if (ao.TryDoAction(ACType.Animation, ACTAnimationPara.Construct(mAnimName, null, EndAnimation, true)))
                        {
                            actorPairs.Add(new AnimationPair(animName, a, ao));
                            a.SetHoldingObjVisible(false);
                            ao.SetHoldingObjVisible(false);

                            isInteracting = true;

                            if (animName == "Hug")
                            {
                                Dbgl("started hugging");
                                isHugging = true;
                                hugTargets.Add(a);
                                hugTargets.Add(ao);
                                targetTime = settings.HugTime;
                            }

                            // start animating
                            a.AddGravityEffector(turnOffGravity);
                            a.StartInteraction(FullBodyBipedEffector.LeftHand, ao.GetInteractionObject(iKNames[animName]), true);
                            a.StartInteraction(FullBodyBipedEffector.RightHand, ao.GetInteractionObject(iKNames[animName]), true);

                            ao.AddGravityEffector(turnOffGravity);
                            ao.StartInteraction(FullBodyBipedEffector.LeftHand, a.GetInteractionObject(iKNames[animName]), true);
                            ao.StartInteraction(FullBodyBipedEffector.RightHand, a.GetInteractionObject(iKNames[animName]), true);

                            return;
                        }
                        else
                        {
                            if(actorPairs.Count > 0)
                                actorPairs.RemoveAt(actorPairs.Count-1);
                            ResetOneAnimation(a);
                            a.TryDoAction(ACType.Transfer, ACTransferPara.Construct(string.Empty, apos, arot));
                            ao.TryDoAction(ACType.Transfer, ACTransferPara.Construct(string.Empty, aopos, aorot));
                        }
                    }
                }
                else
                {
                    if (actorPairs.Count > 0)
                        actorPairs.RemoveAt(actorPairs.Count - 1);
                    a.TryDoAction(ACType.Transfer, ACTransferPara.Construct(string.Empty, apos, arot));
                }
            }

        }

        private static string CheckPet(Actor ao, string mAnimName)
        {
            Dbgl($"{ao.ActorName} id: {ao.InstanceId} {ao.TmpltId}");
            if (mAnimName == "Interact_Kiss")
            {
                Dbgl($"kissing");
                if (ao.InstanceId == 4000144 || ao.InstanceId == 4000007) // Pangolin or Pig
                    return "Petting_Chuan";
                if (ao.InstanceId == 4000069) // Cat
                    return "Petting";
                if (ao.InstanceId == 4000128) // Dog
                    return "Petting_Dog";
            }
            if (mAnimName == "Hug")
            {
                Dbgl($"hugging");
                if (ao.InstanceId == 4000143 || ao.InstanceId == 4000144 || ao.InstanceId == 4000007) // Pangolin or Pig
                    return "Embrace_Chuan";
                if (ao.InstanceId == 4000069) // Cat
                    return "Embrace_Cat";
                if (ao.InstanceId == 4000128) // Dog
                    return "Embrace_Dog";
            }
            return mAnimName;
        }

        private static readonly string[] Interacts = 
        {
            "Hug",
            "Interact_Kiss",
            "Interact_Massage",
            "Petting",
            "Petting_Dog",
            "Petting_Chuan",
            "Embrace_Cat",
            "Embrace_Chuan",
            "Embrace_Dog"
        };
        private static readonly Dictionary<string, int> InteractIds = new Dictionary<string, int>()
        {
            { "Hug", 0 },
            { "Interact_Kiss", 1 },
            { "Interact_Massage", 4 },
            { "Petting", 2 },
            { "Petting_Dog", 2 },
            { "Petting_Chuan", 7 },
            { "Embrace_Cat", 5 },
            { "Embrace_Chuan", 8 },
            { "Embrace_Dog", 6 }
        };



        private static void DoAddRelationship(Actor ao, string animName)
        {

            if (settings.LimitRelationshipPointGain)
            {

                var mBehavior = FieldRefAccess<Actor, BehaviorTree>(ao, "mBehavior");
                var mITActions = mBehavior.FindTasks<ITDecorator>();
                if (mITActions != null && mITActions.Count > 0)
                {
                    for (int i = 0; i < mITActions.Count; i++)
                    {
                        Dbgl($"{animName} interact id: {mITActions[i].iid.Value}");
                        if (mITActions[i] != null && mITActions[i].iid.Value == InteractIds[animName])
                        {
                            FieldRef<ITDecorator, int> mCurTimesRef = FieldRefAccess<ITDecorator, int>("mCurTimes");
                            if (mCurTimesRef(mITActions[i]) < InteractiveData.GetTimes(mITActions[i].iid.Value))
                            {
                                mCurTimesRef(mITActions[i])++;
                                Module<FavorManager>.Self.GainFavorValue(ao.InstanceId,
                                    InteractiveData.GetFavorValue(InteractIds[animName]), true, true);
                            }
                            return;
                        }
                    }
                }
            }

            Module<FavorManager>.Self.GainFavorValue(ao.InstanceId, InteractiveData.GetFavorValue(InteractIds[animName]), true, true);

        }

        public static Action<object[]> ResetAnimations = new Action<object[]>(delegate(object[] o)
        {
            EndAnimation.Invoke();
        });
        public static Action EndAnimation = new Action(delegate ()
        {
            isInteracting = false;
            if (actorPairs.Count < 1)
                return;

            var actors = actorPairs[actorPairs.Count - 1];

            if (!teeterGameRunning && settings.AddRelationshipPoints && actors.a1.InstanceId == Player.Self.actor.InstanceId)
            {
                DoAddRelationship(actors.a2, actors.animName);
            }

            ResetOneAnimation(actors.a1);
            ResetOneAnimation(actors.a2);
            if (actorPairs.Count > 0)
                actorPairs.RemoveAt(actorPairs.Count - 1);
        });

        private static void ResetOneAnimation(Actor actor)
        {
            if (actor == null)
                return;

            actor.SetHoldingObjVisible(true);

            actor.StopAction(ACType.Animation, false);
            actor.RemoveGravityEffector(turnOffGravity);
            actor.ResumeInteraction(FullBodyBipedEffector.LeftHand);
            actor.ResumeInteraction(FullBodyBipedEffector.RightHand);
        }

        //[HarmonyPatch(typeof(AnimController), "SetBool", new Type[] { typeof(string), typeof(bool) })]
        static class AnimController_SetBool_Patch
        {
            static void Prefix(string str, bool val, ref Animator ___animator, AnimController __instance)
            {
                if (!enabled)
                    return;

                if (___animator.isInitialized)
                {
                    if (val)
                    {
                        //Dbgl($"xxyy SetBool anim name: {str}");
                    }
                    if (iKNames.ContainsKey(str) && !val)
                        MessageManager.Instance.Dispatch("InteractModAnimEnd");
                }
            }
        }
    }
}

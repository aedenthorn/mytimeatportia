using Harmony12;
using Pathea.ACT;
using System;
using System.Collections.Generic;

namespace InstantActions
{
    public partial class Main
    {
         
        //allow moving while acting

        [HarmonyPatch(typeof(ACTMgr), "CanDoAction", new Type[] { typeof(int) })]
        static class ACTCastSkill_CanInterrupt_Patch
        {
            static bool Prefix(int index, ACTAction[] ____acts, List<int> ____runs, ref bool __result)
            {
                if (!enabled || !settings.MoveWhileActing || (ACType)index != ACType.Move)
                    return true;

                if (index >= ____acts.Length)
                {
                    __result = false;
                    return false;
                }

                if (!____acts[index].CanDo())
                {
                    Dbgl("can't do: " + index);
                    __result = false;
                    return false;
                }
                for (int i = 0; i < ____runs.Count; i++)
                {
                    if (ACTInterrupt.IsInterrupted(index, ____runs[i]) && !ACTInterrupt.IsParallel(index, ____runs[i]))
                    {
                        Dbgl("can't do: " + index + " parallel with " + ____runs[i]);
                        switch ((ACType) ____runs[i])
                        {
                            case ACType.Drilling:
                            case ACType.DigStone:
                            case ACType.Planting:
                            case ACType.Cutree:
                            case ACType.Chainsaw:
                                Dbgl("doing: " + index + " in spite of " + ____runs[i]);
                                __result = true;
                                return false;
                        }
                        __result = false;
                        return false;
                    }
                    if (ACTInterrupt.IsInterrupt(index, ____runs[i]) && !____acts[____runs[i]].CanInterrupt(index))
                    {
                        Dbgl("can't do: " + index + " to interrupt " + ____runs[i]);
/*
                        switch ((ACType)____runs[i])
                        {
                            case ACType.Drilling:
                            case ACType.DigStone:
                            case ACType.Planting:
                            case ACType.Cutree:
                            case ACType.Chainsaw:
                                Dbgl("doing: " + index + " in spite of " + ____runs[i]);
                                __result = true;
                                return false;
                        }
    */                    
                        __result = false;
                        return false;
                    }
                }
                __result = true;
                return false;
            }
        }
    }
}

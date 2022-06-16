using Verse;
using HarmonyLib;
using System.Reflection;

namespace FasterAging
{
    /// <summary>
    /// Patches Pawn_AgeTracker.AgeTick()
    /// This fixes a problem caused by how vanilla RimWorld calculates life stages.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_AgeTracker), "AgeTick")]
    public static class FasterAgingLifestagePatch
    {
        /// <summary>
        /// Runs after AgeTick(), as often as it is called.
        /// Performs a daily recalculation of life stage.
        /// </summary>
        /// <param name="__instance">AgeTracker of the Pawn that is having its AgeTick method called</param>
        [HarmonyPostfix]
        public static void DailyRecalc(Pawn_AgeTracker __instance)
        {
            if (Find.TickManager.TicksGame % 60000 == 0)
            {
                MethodInfo info = AccessTools.Method(__instance.GetType(), "RecalculateLifeStageIndex", null, null); //Gets accessor info on the private RecalculateLifeStageIndex method
                info.Invoke(__instance, null); //Invokes the method on the pawn
            }

            //TODO/Note -- I actually don't know if this patch is necessary, and it certainly doesn't feel like the right way of doing things.
            //I don't even quite recall the problem that it was trying to solve. I think the issue is if the aging multiplier is >240 it skips CalculateGrowth calls?
            //I don't know if that system has a backup/catchup built in, it's hard to read.
            //I should definitely look into whether this is necessary/best practice either way.
        }
    }
}

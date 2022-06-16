using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FasterAging
{
    /// <summary>
    /// Patches CompBiosculpterPod_AgeReversalCycle.CycleCompleted()
    /// Modifies the amount of age reversal done based on user settings.
    /// </summary>
    [HarmonyPatch(typeof(CompBiosculpterPod_AgeReversalCycle), "CycleCompleted")]
    public class FasterAgingBiosculptReversalPatch
    {
        /// <summary>
        /// Postfix patch on CompBiosculpterPod_AgeReversalCycle.CycleCompleted().
        /// Increases the amount of reverse aging done on the pawn if the user has set a higher de-aging amount.
        /// </summary>
        /// <param name="pawn">Pawn that has be age-reversed</param>
        [HarmonyPostfix]
        public static void CycleCompletedPostfix(ref Pawn pawn)
        {
            if (FasterAgingMod.biosculptAgeReversalYears > 1)
            {
                int ticksToReverse = 3600000 * (FasterAgingMod.biosculptAgeReversalYears - 1); //Number of additional reversed age ticks. Reduce multiplier by 1 because 1 year has already been done by the vanilla method.
                long adultMinAgeTicks = (long)(3600000f * pawn.ageTracker.AdultMinAge); //Minimum age of adulthood for pawn -- not to be undershot.
                pawn.ageTracker.AgeBiologicalTicks = Math.Max(adultMinAgeTicks, pawn.ageTracker.AgeBiologicalTicks - ticksToReverse); //Prevent undershooting min adult age.
            }
        }
    }
}

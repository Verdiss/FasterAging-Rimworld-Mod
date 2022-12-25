using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FasterAging
{
    /// <summary>
    /// Patches Pawn_AgeTracker.CalculateGrowth to correctly account for animal aging multipliers.
    /// The vanilla function does not read the pawn's BiologicalAgeTicksPerTick
    /// </summary>
    [HarmonyPatch(typeof(Pawn_AgeTracker), "CalculateGrowth")]
    public static class FasterAgingCalculateGrowthPatch
    {
        [HarmonyPrefix]
        public static void CalculateGrowthPrefix(ref int interval, Pawn_AgeTracker __instance)
        {
            //The vanilla issue is that pawns not operating on the BioTech aging system (i.e. all animal pawns) run the CalculateGrowth function under the assumption that their aging rate is 1x
            //By multiplying the interval input by their aging rate, this causes the amount of growth they receive (every fixed time period) to be multiplied by their aging mult
            //Now note that this only works because vanilla uses magic numbers in the CalculateGrowth method (a 240 where they should re-use interval).
            //If that ever changes then this fix will break and I will have to probably entirely override the vanilla method.
            interval = (int)Math.Round(interval * __instance.BiologicalTicksPerTick);
        }
    }
}

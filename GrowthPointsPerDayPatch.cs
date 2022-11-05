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
    [HarmonyPatch(typeof(Pawn_AgeTracker), "GrowthPointsPerDayAtLearningLevel")]
    public static class GrowthPointsPerDayPatch
    {
        /// <summary>
        /// Overwrites the GrowthPointsPerDayAtLearningLevel method, which determines childhood growth rates
        /// </summary>
        /// <param name="level"></param>
        /// <param name="__result"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        public static bool GrowthPointsPerDayAtLearningLevelOverwrite(float level, ref float __result, Pawn_AgeTracker __instance)
        {
            //Replicates vanilla behavior, except using a growth rate calc that includes modded growth rate

            float growthMult = __instance.BiologicalTicksPerTick / 1f; //Ratio of actual aging to standard aging. Includes genetic factors and mod settings

            float? growthPointsFactor = typeof(Pawn_AgeTracker).GetProperty("GrowthPointsFactor", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as float?; //Vanilla value that is typically 0.75 or 1.0
            
            __result = level * (float)growthPointsFactor * growthMult;

            return false; //Skip original method
        }
    }
}

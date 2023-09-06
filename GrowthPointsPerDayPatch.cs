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
        /// Overwrites the GrowthPointsPerDayAtLearningLevel method, which determines childhood "growth" tier gain speed
        /// </summary>
        /// <param name="level"></param>
        /// <param name="__result"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        public static bool GrowthPointsPerDayAtLearningLevelOverwrite(float level, ref float __result, Pawn_AgeTracker __instance)
        {
            //Replicates vanilla behavior, except using a growth rate calc that includes modded growth rate
            float agingMult = __instance.BiologicalTicksPerTick / 1f; //Ratio of actual aging to standard aging. Includes genetic factors and mod settings

            float? growthPointsFactor = typeof(Pawn_AgeTracker).GetProperty("GrowthPointsFactor", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as float?; //Vanilla value that is typically 0.75 for 3-7 y/o or 1.0 for older children

            //Use the mod-settings growthPointsFactor if that is enabled instead
            if (FasterAgingMod.modifyChildGrowthPoints)
            {
                if (__instance.AgeBiologicalYearsFloat < 7f) growthPointsFactor = FasterAgingMod.childGrowthRate3to7;
                else if (__instance.AgeBiologicalYearsFloat >= 7f && __instance.AgeBiologicalYearsFloat < 10f) growthPointsFactor = FasterAgingMod.childGrowthRate7to10;
                else if (__instance.AgeBiologicalYearsFloat >= 10f) growthPointsFactor = FasterAgingMod.childGrowthRate10to13;
                //defaults to the vanilla value if something goes wrong
            }
            
            __result = level * (float)growthPointsFactor * agingMult;

            return false; //Skip original method
        }
    }
}

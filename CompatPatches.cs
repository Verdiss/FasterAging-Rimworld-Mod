using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace FasterAging
{
    public static class CompatPatches
    {
        /// <summary>
        /// Rocketman slows down the rate at which animals tick and has its own patch to correct the age difference, assuming vanilla values. This patch here adjusts Rocketman's to also take faster aging into account.
        /// </summary>
        public static IEnumerable<CodeInstruction> RocketmanCompatibilityTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            Type[] types = { typeof(Pawn_AgeTracker) };
            var GetPawnAgingMultiplierMethod = AccessTools.Method(typeof(FasterAging), nameof(FasterAging.GetPawnAgingMultiplier), types);
            for (int i = 0; i < codes.Count; i++)
            {
                if (CodesToChange(codes, i))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, GetPawnAgingMultiplierMethod);
                    yield return new CodeInstruction(OpCodes.Mul);
                    yield return codes[i];
                    i += 1;
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        //not actually changing all these, but I believe checking for more values than necessary is good here, to avoid hitting other mods
        public static bool CodesToChange(List<CodeInstruction> codes, int i)
        {
            return i > 4 && i < codes.Count - 1 && 
                   codes[i - 4].opcode == OpCodes.Call && codes[i - 4].operand as MethodInfo == AccessTools.Method(Type.GetType("Soyuz.ContextualExtensions, Soyuz"), "GetDeltaT") &&
                   codes[i - 3].opcode == OpCodes.Ldc_I4 &&
                   codes[i - 2].opcode == OpCodes.Sub &&
                   codes[i - 1].opcode == OpCodes.Conv_I8 &&
                   codes[i].opcode == OpCodes.Add &&
                   codes[i + 1].opcode == OpCodes.Conv_I8;
        }
    }

    /// <summary>
    /// Above is the transpiler, below is what the result should look like after transpiling.
    /// </summary>

    static class CompatPatch
    {
        private static void AgeTick_Patch(Pawn_AgeTracker __instance)
        {
            if (__instance.pawn.IsValidWildlifeOrWorldPawn() && __instance.pawn.IsSkippingTicks())
            {
                //only the multiplier at the end gets added here.
                __instance.ageBiologicalTicksInt += (__instance.pawn.GetDeltaT() - 1) * FasterAging.GetPawnAgingMultiplier(__instance);
            }
        }

        //placeholders for visualization.
        private static bool IsValidWildlifeOrWorldPawn(this Pawn pawn) { return pawn.RaceProps.Humanlike; }
        private static bool IsSkippingTicks(this Pawn pawn) { return pawn.RaceProps.Humanlike; }
        private static int GetDeltaT(this Thing thing) { return GenTicks.TicksGame; }
    }
}

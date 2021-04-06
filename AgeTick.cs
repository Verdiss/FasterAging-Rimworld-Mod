using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace FasterAging
{
    /// <summary>
    /// Patches Pawn_AgeTracker.AgeTick().
    /// </summary>
    public static class AgeTick
    {
        //Comments further down
        public static IEnumerable<CodeInstruction> FasterAgingTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            generator.DeclareLocal(typeof(long));
            var label1 = generator.DefineLabel();
            Type[] types1 = { typeof(Pawn_AgeTracker) };
            Type[] types2 = { typeof(Pawn_AgeTracker), typeof(long) };
            var GetPawnAgingMultiplierMethod = AccessTools.Method(typeof(FasterAging), nameof(FasterAging.GetPawnAgingMultiplier), types1);
            var ModifyChronologicalAgeMethod = AccessTools.Method(typeof(FasterAging), nameof(FasterAging.ModifyChronologicalAge), types2);
            for (int i = 0; i < codes.Count; i++)
            {
                if (CodesToChange1(codes, i))
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Call, GetPawnAgingMultiplierMethod);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, label1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return codes[i + 1];
                    yield return codes[i + 2];
                    codes[i + 3].opcode = OpCodes.Ldloc_0;
                    yield return codes[i + 3];
                    i += 4;
                }
                else if (CodesToChange2(codes, i))
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    codes[i + 1].opcode = OpCodes.Bge_S;
                    codes[i + 1].operand = label1;
                    yield return codes[i + 1];
                    i += 1;
                }
                else if (CodesToChange3(codes, i))
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = { label1 } };
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, ModifyChronologicalAgeMethod);
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static bool CodesToChange1(List<CodeInstruction> codes, int i)
        {
            return i < codes.Count - 3 &&
                   codes[i].opcode == OpCodes.Ldarg_0 &&
                   codes[i + 1].opcode == OpCodes.Ldarg_0 &&
                   codes[i + 2].opcode == OpCodes.Ldfld && codes[i + 2].operand as FieldInfo == AccessTools.Field(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.ageBiologicalTicksInt)) &&
                   codes[i + 3].opcode == OpCodes.Ldc_I4_1 &&
                   codes[i + 4].opcode == OpCodes.Conv_I8;
        }

        public static bool CodesToChange2(List<CodeInstruction> codes, int i)
        {
            return i < codes.Count - 1 &&
                   codes[i].opcode == OpCodes.Rem &&
                   codes[i + 1].opcode == OpCodes.Brtrue_S;
        }

        public static bool CodesToChange3(List<CodeInstruction> codes, int i)
        {
            return codes[i].opcode == OpCodes.Call && codes[i].operand as MethodInfo == AccessTools.Method(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.BirthdayBiological));
        }
    }

    /// <summary>
    /// Above is the transpiler, below is what the result should look like after transpiling.
    /// </summary>

    class Pawn_AgeTracker_AgeTick
    {
        /// <summary>
        /// The Age Tick.
        /// </summary>
        /// <param name="__instance">The pawn</param>
        private void AgeTick(Pawn_AgeTracker __instance)
        {
            //multiplier, adjusted for settings
            long multiplier = FasterAging.GetPawnAgingMultiplier(__instance);

            //skip the age tick if the multiplier is 0
            if (multiplier != 0)
            {
                //add an amount of ticks equal to the multiplier to age
                __instance.ageBiologicalTicksInt += multiplier;

                //this part is vanilla
                if (Find.TickManager.TicksGame >= __instance.nextLifeStageChangeTick)
                {
                    __instance.RecalculateLifeStageIndex();
                }

                //if the remainder of age/years is smaller than the multiplier a birthday happened
                if (__instance.ageBiologicalTicksInt % 3600000L < multiplier)
                {
                    __instance.BirthdayBiological();
                }
            }

            //Chronological age modification. Off by default
            FasterAging.ModifyChronologicalAge(__instance, multiplier);
        }
    }
}

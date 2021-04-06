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
    /// Patches Pawn_AgeTracker.AgeTickMothballed()
    /// </summary>
    public static class WorldPawnAgeTick
    {
        //Comments further down
        public static IEnumerable<CodeInstruction> FasterAgingWorldPawnTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var label1 = generator.DefineLabel();
            var label2 = generator.DefineLabel();
            Type[] types1 = { typeof(Pawn_AgeTracker) };
            Type[] types2 = { typeof(Pawn_AgeTracker), typeof(long), typeof(int) };
            var GetPawnAgingMultiplierMethod = AccessTools.Method(typeof(FasterAging), nameof(FasterAging.GetPawnAgingMultiplier), types1);
            var ModifyChronologicalAgeMethod = AccessTools.Method(typeof(FasterAging), nameof(FasterAging.ModifyChronologicalAge), types2);
            var BirthAbsTicksProperty = AccessTools.Property(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.BirthAbsTicks));
            var AgeBiologicalYearsProperty = AccessTools.Property(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.AgeBiologicalYears));
            var RecalculateLifeStageIndexMethod = AccessTools.Method(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.RecalculateLifeStageIndex));
            var instruction1 = new CodeInstruction(OpCodes.Blt_S, label2);
            var instruction2 = new CodeInstruction(OpCodes.Ldarg_0);
            var instruction3 = new CodeInstruction(OpCodes.Call, RecalculateLifeStageIndexMethod);
            for (int i = 0; i < codes.Count; i++)
            {
                if (CodesToChange1(codes, i))
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Call, GetPawnAgingMultiplierMethod);
                    yield return codes[i + 2];
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, label1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return codes[i + 1];
                    i += 2;
                }
                else if (CodesToChange2(codes, i))
                {
                    yield return codes[i];
                    yield return codes[i + 1];
                    yield return codes[i + 2];
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Mul);
                    i += 2;
                }
                else if (CodesToChange3(codes, i))
                {
                    instruction1 = codes[i];
                    instruction2 = codes[i + 1];
                    instruction3 = codes[i + 2];
                    i += 2;
                }
                else if (CodesToChange4(codes, i))
                {
                    instruction1.opcode = OpCodes.Blt_S;
                    instruction1.operand = label2;
                    yield return instruction1;
                    yield return instruction2;
                    yield return instruction3;
                    codes[i + 2].labels.Add(label2);
                    i += 1;
                }
                else if (CodesToChange5(codes, i))
                {
                    codes[i].opcode = OpCodes.Ldc_I4_1;
                    codes[i].operand = null;
                    yield return codes[i];
                }
                else if (CodesToChange6(codes, i))
                {
                    yield return codes[i + 1];
                    codes[i + 2].opcode = OpCodes.Callvirt;
                    codes[i + 2].operand = AgeBiologicalYearsProperty.GetGetMethod();
                    yield return codes[i + 2];
                    yield return codes[i + 6];
                    codes[i].opcode = OpCodes.Ldarg_0;
                    codes[i].labels.Add(label1);
                    yield return codes[i];
                    codes[i + 3].opcode = OpCodes.Ldloc_0;
                    codes[i + 3].operand = null;
                    yield return codes[i + 3];
                    codes[i + 4].opcode = OpCodes.Ldarg_1;
                    yield return codes[i + 4];
                    codes[i + 5].opcode = OpCodes.Call;
                    codes[i + 5].operand = ModifyChronologicalAgeMethod;
                    yield return codes[i + 5];
                    i += 6;
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static bool CodesToChange1(List<CodeInstruction> codes, int i)
        {
            return i < codes.Count - 2 &&
                   codes[i].opcode == OpCodes.Ldarg_0 &&
                   codes[i + 1].opcode == OpCodes.Ldfld &&
                   codes[i + 2].opcode == OpCodes.Stloc_0;
        }

        public static bool CodesToChange2(List<CodeInstruction> codes, int i)
        {
            return i < codes.Count - 1 &&
                   codes[i].opcode == OpCodes.Ldfld &&
                   codes[i + 1].opcode == OpCodes.Ldarg_1 &&
                   codes[i + 2].opcode == OpCodes.Conv_I8;
        }

        public static bool CodesToChange3(List<CodeInstruction> codes, int i)
        {
            return i < codes.Count - 2 &&
                   codes[i].opcode == OpCodes.Br_S &&
                   codes[i + 1].opcode == OpCodes.Ldarg_0 &&
                   codes[i + 2].opcode == OpCodes.Call && codes[i + 2].operand as MethodInfo == AccessTools.Method(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.RecalculateLifeStageIndex));
        }

        public static bool CodesToChange4(List<CodeInstruction> codes, int i)
        {
            return i < codes.Count - 1 &&
                   codes[i].opcode == OpCodes.Bge_S &&
                   codes[i + 1].opcode == OpCodes.Ldloc_0;
        }

        public static bool CodesToChange5(List<CodeInstruction> codes, int i)
        {
            return i < codes.Count - 1 &&
                   codes[i].opcode == OpCodes.Ldc_I4 &&
                   codes[i + 1].opcode == OpCodes.Add;
        }

        public static bool CodesToChange6(List<CodeInstruction> codes, int i)
        {
            return i < codes.Count - 5 &&
                   codes[i].opcode == OpCodes.Conv_I8 &&
                   codes[i + 1].opcode == OpCodes.Ldarg_0 &&
                   codes[i + 2].opcode == OpCodes.Ldfld &&
                   codes[i + 3].opcode == OpCodes.Ldc_I4 &&
                   codes[i + 4].opcode == OpCodes.Conv_I8 &&
                   codes[i + 5].opcode == OpCodes.Div &&
                   codes[i + 6].opcode == OpCodes.Blt_S;
        }
    }

    /// <summary>
    /// Above is the transpiler, below is what the result should look like after transpiling.
    /// </summary>

    class Pawn_AgeTracker_AgeTickMothballed
    {
        /// <summary>
        /// The world pawn age tick. This triggers once every 15000 regular ticks.
        /// </summary>
        /// <param name="__instance">The pawn</param>
        /// <param name="interval">The interval. 15000.</param>
        private void AgeTickMothballed(Pawn_AgeTracker __instance, int interval)
        {
            //multiplier, adjusted for settings
            long multiplier = FasterAging.GetPawnAgingMultiplier(__instance);

            //skip the age tick if the multiplier is 0
            if (multiplier != 0)
            {
                //Save a snapshot of the age before ticking
                long ageBiologicalTicksInt = __instance.ageBiologicalTicksInt;

                //add an amount of ticks equal to the multiplier to age
                __instance.ageBiologicalTicksInt += interval * multiplier;

                //vanilla code for life stage recalculation
                //Technically possible for this to work incorrectly with extremely high multipliers combined with animals that only have few lifestages. 
                if (Find.TickManager.TicksGame >= __instance.nextLifeStageChangeTick)
                {
                    __instance.RecalculateLifeStageIndex();
                }

                //Current age compared to the age in the snapshot above. Birthdays get triggered for every year that passed, though to actually get more than one a multiplier above 240 would be necessary.
                for (int i = (int)(ageBiologicalTicksInt / 3600000L); i < __instance.AgeBiologicalYears; i++)
                {
                    __instance.BirthdayBiological();
                }
            }

            //Chronological age modification. Off by default
            FasterAging.ModifyChronologicalAge(__instance, multiplier, interval);
        }
    }
}

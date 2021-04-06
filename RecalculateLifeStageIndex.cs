using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace FasterAging
{
    /// <summary>
    /// Patches Pawn_AgeTracker.RecalculateLifeStageIndex()
    /// </summary>
    public static class RecalculateLifeStageIndex
    {
        public static IEnumerable<CodeInstruction> FasterAgingLifeStageTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
			Type[] types = { typeof(Pawn_AgeTracker) };
			var GetPawnAgingMultiplierMethod = AccessTools.Method(typeof(FasterAging), nameof(FasterAging.GetPawnAgingMultiplier), types);
			for (int i = 0; i < codes.Count; i++)
            {
                if (CodesToChange(codes, i))
                {
                    yield return codes[i];
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, GetPawnAgingMultiplierMethod);
					yield return new CodeInstruction(OpCodes.Conv_R4);
                    yield return new CodeInstruction(OpCodes.Div);
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static bool CodesToChange(List<CodeInstruction> codes, int i)
        {
            return codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 3600000f;
        }
    }

	/// <summary>
	/// Above is the transpiler, below is what the result should look like after transpiling.
	/// </summary>
    class Pawn_AgeTracker_RecalculateLifeStageIndex
    {
		/// <summary>
		/// Method for life stage recalculation
		/// </summary>
		/// <param name="__instance">pawn</param>
		private void RecalculateLifeStageIndex(Pawn_AgeTracker __instance)
		{
			int num = -1;
			List<LifeStageAge> lifeStageAges = __instance.pawn.RaceProps.lifeStageAges;
			for (int i = lifeStageAges.Count - 1; i >= 0; i--)
			{
				if (lifeStageAges[i].minAge <= __instance.AgeBiologicalYearsFloat + 1E-06f)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				num = 0;
			}
			bool flag = __instance.cachedLifeStageIndex != num;
			__instance.cachedLifeStageIndex = num;
			if (flag && !__instance.pawn.RaceProps.Humanlike)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					__instance.pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
				});
				__instance.CheckChangePawnKindName();
			}
			if (__instance.cachedLifeStageIndex < lifeStageAges.Count - 1)
			{
				float num2 = lifeStageAges[__instance.cachedLifeStageIndex + 1].minAge - __instance.AgeBiologicalYearsFloat;
				int num3 = (Current.ProgramState == ProgramState.Playing) ? Find.TickManager.TicksGame : 0;

				//Everything except for the multiplier here is vanilla. The multiplier reduces the time to the next lifestagechangetick to match up with the faster aging speed.
				__instance.nextLifeStageChangeTick = (long)num3 + (long)Mathf.Ceil(num2 * (3600000f / FasterAging.GetPawnAgingMultiplier(__instance)));
				//as far as I can tell rimworld refreshes this value when reloading a save (as well as when it actually changes)
				return;
			}
			__instance.nextLifeStageChangeTick = long.MaxValue;
		}
	}
}

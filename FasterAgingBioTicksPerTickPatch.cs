using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FasterAging
{
    [HarmonyPatch(typeof(Pawn_AgeTracker), "BiologicalTicksPerTick", MethodType.Getter)]

    public static class FasterAgingBioTicksPerTickPatch
    {
        [HarmonyPrefix]
        public static bool GetBiologicalTicksPerTickOverwrite(ref float __result, Pawn_AgeTracker __instance)
        {
            //Vanilla code for reference:
            //if (ModsConfig.BiotechActive && pawn.ParentHolder != null && pawn.ParentHolder is Building_GrowthVat)
            //{
            //    return 1f;
            //}
            //float num = 1f;
            //num = ((!pawn.DevelopmentalStage.Adult()) ? (num * ChildAgingMultiplier) : (num * AdultAgingMultiplier));
            //if (pawn.genes != null)
            //{
            //    num *= pawn.genes.BiologicalAgeTickFactor;
            //}
            //return num;


            //Mod code:
            //pawn is a private field of Pawn_AgeTracker, because Tynan hates me, so I have to do the ugly grab below
            Pawn pawn = typeof(Pawn_AgeTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as Pawn;
            if (pawn != null)
            {
                //Growth vats, as vanilla code -- control over vat aging is handled by FasterAgingBioTicksPerTickPatch
                if (ModsConfig.BiotechActive && pawn.ParentHolder != null && pawn.ParentHolder is Building_GrowthVat)
                {
                    __result = 1f;
                }
                else //Not in a vat, perform full calculation
                {
                    float ageMult = 1f;

                    //First apply age mult mod settings
                    ageMult *= FasterAgingMod.GetPawnAgingMultiplier(pawn);

                    //Aging genes
                    //This is unmodified, even though it doesn't correclty account for custom cutoffs.
                    //Accounting for custom cutoffs would involve def editing the genes themselves, so I'm simply opting to warn users about the behavior
                    if (pawn.genes != null)
                    {
                        ageMult *= pawn.genes.BiologicalAgeTickFactor;
                    }

                    //Note that the vanilla storyteller settings for aging multipliers are not included in this calculation - my mod overrides these settings

                    __result = ageMult;
                }


                return false; //Skips original method
            }


            return true; //This happens if the search for pawn somehow failed, in which case it's up to vanilla code to handle that particular shitstorm
        }
    }
}

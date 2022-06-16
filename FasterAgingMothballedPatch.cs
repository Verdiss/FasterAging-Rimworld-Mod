using Verse;
using HarmonyLib;
using System.Reflection;

namespace FasterAging
{
    /// <summary>
    /// Patches Pawn_AgeTracker.AgeTickMothballed()
    /// Applies the appropriate aging changes to mothballed (off-map) pawns
    /// </summary>
    [HarmonyPatch(typeof(Pawn_AgeTracker), "AgeTickMothballed")]
    public static class FasterAgingMothballedPatch
    {
        /// <summary>
        /// This patch modifies the core game's AgeTickMothballed, which handles aging for off-map pawns
        /// It applies the aging rate modification by intercepting and changing the base function's interval argument, which determines how many ageticks to add every call.
        /// It multiplies this argument to increase or halt the aging rate of mothballed pawns.
        /// </summary>
        /// <param name="interval">Vanilla value that determines how many age ticks to advance the pawn through</param>
        /// <param name="__instance">AgeTracker of the Pawn that is having its AgeTickMothballed method called</param>
        [HarmonyPrefix]
        public static void AgeTickMothballedArgsPatch(ref int interval, Pawn_AgeTracker __instance)
        {
            //Find the pawn this tracker belongs to - it's a private field
            Pawn pawn = null;
            FieldInfo pawnFieldInfo = AccessTools.Field(__instance.GetType(), "pawn");
            if (pawnFieldInfo != null && pawnFieldInfo.FieldType.Equals(typeof(Pawn)))
            {
                pawn = (Pawn)pawnFieldInfo.GetValue(__instance);
            }

            if (pawn != null)
            {
                if (!pawn.Suspended) //Only do things if the pawn is not suspended (i.e. in cryosleep)
                {
                    //Determine multiplier
                    int multiplier = FasterAgingMod.GetPawnAgingMultiplier(pawn);


                    //Chronological age modification if the setting is enabled
                    if (FasterAgingMod.modifyChronologicalAge)
                    {
                        __instance.BirthAbsTicks -= (multiplier - 1) * interval; //Similar system to how the AgeTick patch works, just on the scale of the base interval rather than single ticks. Moves the pawn's birthday to correctly account for modified age rate.
                    }


                    //Edit the REFERENCED interval value to take the multiplier into account.
                    //The game will now run AgeTickMothballed() with this edited interval, accelerating or halting aging.
                    interval = multiplier * interval;
                }
            }
        }
    }
}

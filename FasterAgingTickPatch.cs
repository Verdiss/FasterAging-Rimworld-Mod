using Verse;
using HarmonyLib;

namespace FasterAging
{
    /// <summary>
    /// Patches Pawn.Tick().
    /// Applies the aging modifications to on-map pawns
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "Tick")]
    public static class FasterAgingTickPatch
    {
        /// <summary>
        /// Applies aging rate modification to on-map pawns.
        /// Harmony runs this on every Pawn every tick.
        /// </summary>
        /// <param name="__instance">Pawn that is having its Tick method called</param>
        [HarmonyPostfix]
        public static void PawnTickPostfix(Pawn __instance)
        {
            if (!__instance.Suspended) //Only do things if the pawn is not suspended (i.e. in cryosleep)
            {
                //Determine multiplier
                int multiplier = FasterAgingMod.GetPawnAgingMultiplier(__instance);


                //Run extra aging
                if (multiplier == 0) //Aging is disabled - Manually revert the age increase from the core game's AgeTick call
                {
                    __instance.ageTracker.AgeBiologicalTicks += -1;
                    //Note - there is still a potential issue if the age multiplier setting is changed by the player to 0 on a birthday tick, causing that birthday to get re-run.
                    //This is a hilariously impossible edge case, but if I want to improve code safety, this is something to work on.
                }
                else
                {
                    if (!FasterAgingMod.useCompatiblityAlgorithm)
                    {
                        //Standard aging algorithm - directly increment the pawn's age tick value to the appropriate amount. Similar to vanilla AgeTickMothballed.
                        int ageYearsBefore = __instance.ageTracker.AgeBiologicalYears; //Save the pawn's age before the increment for birthday calculation

                        __instance.ageTracker.AgeBiologicalTicks += multiplier - 1; //Advances the pawn's age value. It has already been increased by 1 by vanilla code.

                        //Vanilla's AgeTickMothballed includes a check for life stage recalculation here.
                        //This is difficult to do as the required method is private.
                        //I don't think this is strictly necessary/it has already been solved by my daily check.
                        //TODO if I change the daily lifestage calculation thing, come back and look at this too.

                        //Birthday check - run BirthdayBiological once for every year of age that the pawn has rolled over this tick
                        for (int year = ageYearsBefore; year < __instance.ageTracker.AgeBiologicalYears; year++)
                        {
                            __instance.ageTracker.DebugForceBirthdayBiological(); //This method is public where BirthdayBiological is private. The debug method does nothing but call the proper method fortunately.
                        }
                    }
                    else
                    {
                        //Compatibility aging algorithm - repeat calls AgeTick until a correct number of calls has been made to match the multiplier.
                        //This is here in case the user is also running a mod that expects pawn age to increase once per AgeTick() call.
                        //This system is more performance intensive, and will also trip up any mods that expect AgeTick to be called once per pawn tick.
                        //Realistically, this is something I could cut from the mod without really doing much harm, but I don't think it causes any real harm if it stays either.
                        for (int additionalTick = 0; additionalTick < multiplier - 1; additionalTick++) //AgeTick already runs once naturally - subtract 1 from the multiplier to get how many times it should be called again
                        {
                            __instance.ageTracker.AgeTick();
                        }
                    }
                }


                //Chronological age modification if the setting is enabled
                if (FasterAgingMod.modifyChronologicalAge)
                {
                    __instance.ageTracker.BirthAbsTicks -= multiplier - 1; //Move the character's birthday earlier in time, accelerating how distant it is from the current. If multiplier is 0, actually moves birthday forward 1 tick, keeping it a constant distance from the current tick and thus a constant age.
                }
            }
        }
    }
}

/*
 * Faster Aging Mod by Verdiss
 * Mod Version 1.4 2020-12-21 tested for Rimworld 1.2
 * You are free to redistribute this mod and use its code for non-commercial purposes, just give credit to Verdiss in descriptions and in source code.
 * You are also free to laugh at how badly the mod is implemented, and wash out your eyes with bleach after closing your IDE in horror.
 */

using Verse;
using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using System.Reflection;
using System;

namespace FasterAging
{
    /// <summary>
    /// Loads and prepares HugsLib mod config settings, as well as being loaded into HugsLib system for Harmony patching.
    /// </summary>
    public class FasterAging : ModBase
    {
        public override string ModIdentifier => "FasterAging";

        private SettingHandle<int> pawnSpeedMultBeforeCutoffSetting; //HugsLib setting controlling the speed multiplier for humanoid pawns below the cutoff age
        private SettingHandle<int> pawnSpeedMultAfterCutoffSetting; //HugsLib setting controlling the speed multiplier for humanoid pawns equal to or above the cutoff age
        private SettingHandle<int> pawnCutoffAgeSetting; //HugsLib setting controlling the cutoff age for age speed for humanoid pawns.

        private SettingHandle<int> animalSpeedMultBeforeCutoffSetting; //HugsLib setting controlling the speed multiplier for animals below the cutoff age
        private SettingHandle<int> animalSpeedMultAfterCutoffSetting; //HugsLib setting controlling the speed multiplier for animals equal to or above the cutoff age
        private SettingHandle<int> animalCutoffAgeSetting; //HugsLib setting controlling the cutoff age for age speed for animals.

        private SettingHandle<bool> modifyChronologicalAgeSetting; //HugsLib setting controlling whether chronological age is also modified.

        private SettingHandle<bool> useAlternateAgingAlgorithmSetting; //HugsLib setting controls whether the mod uses the alternate aging algorithm.


        public static int pawnSpeedMultBeforeCutoff = 1; //Actual value of the pawn speed multiplier before cutoff setting
        public static int pawnSpeedMultAfterCutoff = 1; //Actual value of the pawn speed multiplier after cutoff setting
        public static int pawnCutoffAge = 1000; //Actual value of the pawn cutoff age setting
        public static long pawnCutoffAgeTicks => (pawnCutoffAge * 3600000) + 1000; //Cutoff age converted to ticks. 1000 ticks are added to this value as a buffer around birthdays to prevent repeatedly calling birthday code when aging is disabled.

        public static int animalSpeedMultBeforeCutoff = 1; //Actual value of the animal speed multiplier before cutoff setting
        public static int animalSpeedMultAfterCutoff = 1; //Actual value of the animal speed multiplier after cutoff setting
        public static int animalCutoffAge = 1000; //Actual value of the animal cutoff age setting
        public static long animalCutoffAgeTicks => (animalCutoffAge * 3600000) + 1000; //Cutoff age converted to ticks. 1000 ticks are added to this value as a buffer around birthdays to prevent repeatedly calling birthday code when aging is disabled.

        public static bool modifyChronologicalAge = false; //Whether to also change chronological age, not just biological

        public static bool useAlternateAging = false; //Whether to use the alternate aging algorithm.

        /// <summary>
        /// Runs during game startup.
        /// Initializes settings and setting values.
        /// </summary>
        public override void DefsLoaded()
        {
            //Setup and get settings values
            pawnSpeedMultBeforeCutoffSetting = Settings.GetHandle<int>(settingName: "speedMultSetting", title: "settings_pawnSpeedMultBefore_title".Translate(), description: "settings_pawnSpeedMultBefore_description".Translate(), defaultValue: 1); //Using old setting title speedMultSetting for compatability
            pawnSpeedMultBeforeCutoff = pawnSpeedMultBeforeCutoffSetting.Value;

            pawnSpeedMultAfterCutoffSetting = Settings.GetHandle<int>(settingName: "pawnSpeedMultAfter", title: "settings_pawnSpeedMultAfter_title".Translate(), description: "settings_pawnSpeedMultAfter_description".Translate(), defaultValue: 1);
            pawnSpeedMultAfterCutoff = pawnSpeedMultAfterCutoffSetting.Value;

            pawnCutoffAgeSetting = Settings.GetHandle<int>(settingName: "pawnCutoffAge", title: "settings_pawnCutoffAge_title".Translate(), description: "settings_pawnCutoffAge_description".Translate(), defaultValue: 1000);
            pawnCutoffAge = pawnCutoffAgeSetting.Value;


            animalSpeedMultBeforeCutoffSetting = Settings.GetHandle<int>(settingName: "animalSpeedMultBefore", title: "settings_animalSpeedMultBefore_title".Translate(), description: "settings_animalSpeedMultBefore_description".Translate(), defaultValue: 1);
            animalSpeedMultBeforeCutoff = animalSpeedMultBeforeCutoffSetting.Value;

            animalSpeedMultAfterCutoffSetting = Settings.GetHandle<int>(settingName: "animalSpeedMultAfter", title: "settings_animalSpeedMultAfter_title".Translate(), description: "settings_animalSpeedMultAfter_description".Translate(), defaultValue: 1);
            animalSpeedMultAfterCutoff = animalSpeedMultAfterCutoffSetting.Value;

            animalCutoffAgeSetting = Settings.GetHandle<int>(settingName: "animalCutoffAge", title: "settings_animalCutoffAge_title".Translate(), description: "settings_animalCutoffAge_description".Translate(), defaultValue: 1000);
            animalCutoffAge = animalCutoffAgeSetting.Value;


            modifyChronologicalAgeSetting = Settings.GetHandle<bool>(settingName: "modifyChronologicalAge", title: "settings_modifyChronologicalAge_title".Translate(), description: "settings_modifyChronologicalAge_description".Translate(), defaultValue: false);
            modifyChronologicalAge = modifyChronologicalAgeSetting.Value;


            useAlternateAgingAlgorithmSetting = Settings.GetHandle<bool>(settingName: "useAlternateAgingAlgorithm", title: "settings_useAlternateAgingAlgorithm_title".Translate(), description: "settings_modifyChronologicalAge_description".Translate(), defaultValue: false);
            useAlternateAging = useAlternateAgingAlgorithmSetting.Value;
        }

        /// <summary>
        /// Runs whenever the user changes a setting during runtime.
        /// Adjusts the internal variables to match the new settings.
        /// </summary>
        public override void SettingsChanged()
        {
            if (pawnSpeedMultBeforeCutoffSetting.Value < 0)
            {
                pawnSpeedMultBeforeCutoffSetting.Value = 0; //All settings have checks to prevent negative values. Reverse aging ain't happening.
            }
            pawnSpeedMultBeforeCutoff = pawnSpeedMultBeforeCutoffSetting.Value;


            if (pawnSpeedMultAfterCutoffSetting.Value < 0)
            {
                pawnSpeedMultAfterCutoffSetting.Value = 0;
            }
            pawnSpeedMultAfterCutoff = pawnSpeedMultAfterCutoffSetting.Value;


            if (pawnCutoffAgeSetting.Value < 0)
            {
                pawnCutoffAgeSetting.Value = 0;
            }
            pawnCutoffAge = pawnCutoffAgeSetting.Value;



            if (animalSpeedMultBeforeCutoffSetting.Value < 0)
            {
                animalSpeedMultBeforeCutoffSetting.Value = 0;
            }
            animalSpeedMultBeforeCutoff = animalSpeedMultBeforeCutoffSetting.Value;


            if (animalSpeedMultAfterCutoffSetting.Value < 0)
            {
                animalSpeedMultAfterCutoffSetting.Value = 0;
            }
            animalSpeedMultAfterCutoff = animalSpeedMultAfterCutoffSetting.Value;


            if (animalCutoffAgeSetting.Value < 0)
            {
                animalCutoffAgeSetting.Value = 0;
            }
            animalCutoffAge = animalCutoffAgeSetting.Value;



            modifyChronologicalAge = modifyChronologicalAgeSetting.Value;


            useAlternateAging = useAlternateAgingAlgorithmSetting.Value;
        }

        /// <summary>
        /// Gets the aging rate multiplier value for the input pawn, based on the mod's settings, and the pawn's type and age.
        /// </summary>
        /// <param name="pawn">Pawn to determine the age rate multiplier for</param>
        /// <returns></returns>
        public static int GetPawnAgingMultiplier(Pawn pawn)
        {
            int multiplier = 0; //How much the settings say the pawn's age speed should be multiplied by.


            if (!pawn.RaceProps.Humanlike)
            {
                //It's an animal
                if (pawn.ageTracker.AgeBiologicalTicks >= animalCutoffAgeTicks)
                {
                    //It's after the cutoff age
                    multiplier = animalSpeedMultAfterCutoff;
                }
                else
                {
                    //It's before the cutoff age
                    multiplier = animalSpeedMultBeforeCutoff;
                }
            }
            else
            {
                //It's a humanlike
                if (pawn.ageTracker.AgeBiologicalTicks >= pawnCutoffAgeTicks)
                {
                    //It's after the cutoff age
                    multiplier = pawnSpeedMultAfterCutoff;
                }
                else
                {
                    //It's before the cutoff age
                    multiplier = pawnSpeedMultBeforeCutoff;
                }
            }


            return multiplier;
        }
    }

    /// <summary>
    /// Patches Pawn.Tick().
    /// </summary>
    [HarmonyPatch(typeof(Verse.Pawn), "Tick")]
    public static class FasterAgingPatch
    {
        /// <summary>
        /// Runs on every pawn every tick.
        /// The pawn (__instance) has its AgeTick method called as required.
        /// </summary>
        /// <param name="__instance">Pawn that is having its Tick method called</param>
        [HarmonyPostfix]
        public static void MultiTick(Pawn __instance)
        {
            //Determine multiplier
            int multiplier = FasterAging.GetPawnAgingMultiplier(__instance);


            //Run extra aging.
            if (multiplier == 0) //Aging is disabled - Manually revert the age increase from the core game's AgeTick call
            {
                __instance.ageTracker.AgeBiologicalTicks += -1;
                //Note - there is still a potential issue if the age multiplier setting is changed by the player to 0 on a birthday tick, causing that birthday to get re-run.
                //This is a hilariously impossible edge case, but if I want to improve code safety, this is something to work on.
            }
            else
            {
                if (!FasterAging.useAlternateAging)
                {
                    //Primary aging algorithm - repeat calls AgeTick until a correct number of calls has been made to match the multiplier.
                    //This system is more performance intensive, and will trip up any mods that expect AgeTick to be called once per pawn tick.
                    //However, I judge that it is more likely and more problematic that a mod expects a specific age tick count to occur after
                    //an AgeTick call, which will get skipped using the alternative method.
                    //Thus this is the default algorithm.
                    for (int additionalTick = 0; additionalTick < multiplier - 1; additionalTick++) //AgeTick already runs once naturally - subtract 1 from the multiplier to get how many times it should be called again
                    {
                        __instance.ageTracker.AgeTick();
                    }
                } else
                {
                    //Alternative aging algorithm - directly increment the pawn's age tick value to the appropriate amount. Similar to vanilla AgeTickMothballed
                    //This is much less performance hungry, but may cause issues if another mod expects AgeTick to only increment the age value by 1, or if 
                    //another mod has a trigger set up to check for a specific age tick value after AgeTick is called.
                    //It's an optional alternate method for these reasons.
                    int ageYearsBefore = __instance.ageTracker.AgeBiologicalYears; //Save the pawn's age before the increment for birthday calculation

                    __instance.ageTracker.AgeBiologicalTicks += multiplier - 1; //Advances the pawn's age value. It has already been increased by 1 by vanilla code.

                    //Vanilla's AgeTickMothballed includes a check for life stage recalculation here.
                    //I don't think this is necessary/it has already been solved by my daily check.
                    //It also includes a Find call that I worry might not work well with a mod, so I didn't include the code.

                    //Birthday check - run BiologicalBirthday once for every full year of age that the pawn has advanced during this algorithm
                    //Tynan used a much more complicated system of comparing age tick values, but I cannot say why. If this is an issue, though, I should replace this with his system.
                    for (int year = ageYearsBefore; year < __instance.ageTracker.AgeBiologicalYears; year++)
                    {
                        __instance.ageTracker.DebugForceBirthdayBiological(); //This method is public where BirthdayBiological is private. The debug method does nothing but call the proper method fortunately.
                    }
                }
            }


            //Experimental Chronological age modification
            //I judge this too likely to cause issues and too practically insignificant to include as a default feature.
            if (FasterAging.modifyChronologicalAge)
            {
                __instance.ageTracker.BirthAbsTicks -= multiplier - 1; //Move the character's birthday earlier in time, accelerating how distant it is from the current. If multiplier is 0, actually moves birthday forward 1 tick, keeping it a constant distance from the current tick and thus a constant age.
            }
        }
    }


    /// <summary>
    /// Patches Pawn_AgeTracker.AgeTickMothballed()
    /// </summary>
    [HarmonyPatch(typeof(Verse.Pawn_AgeTracker), "AgeTickMothballed")]
    public static class FasterAgingMothballedPatch
    {
        /// <summary>
        /// This patch modifies the core game's AgeTickMothballed's interval argument, which determines how many ageticks to add every call.
        /// It multiplies this argument to increase or halt the aging rate of mothballed pawns.
        /// </summary>
        /// <param name="interval">Vanilla value that determines how many age ticks to advance the pawn through</param>
        /// <param name="__instance">Pawn that is having its AgeTickMothballed method called</param>
        [HarmonyPrefix]
        public static void MultipliedAgeTick(ref int interval, Pawn_AgeTracker __instance)
        {
            //Find the pawn this tracker belongs to
            Pawn pawn = null;
            FieldInfo pawnFieldInfo = AccessTools.Field(__instance.GetType(), "pawn");
            if (pawnFieldInfo != null && pawnFieldInfo.FieldType.Equals(typeof(Verse.Pawn)))
            {
                pawn = (Verse.Pawn)pawnFieldInfo.GetValue(__instance);
            }

            if (pawn != null)
            {
                //Determine multiplier
                int multiplier = FasterAging.GetPawnAgingMultiplier(pawn);


                //Experimental Chronological age modification
                //I judge this too likely to cause issues and too practically insignificant to include as a default feature.
                if (FasterAging.modifyChronologicalAge)
                {
                    __instance.BirthAbsTicks -= (multiplier - 1) * interval; //Similar system to how the AgeTick patch works, just on the scale of the base interval rather than single ticks. Moves the pawn's birthday to correctly account for modified age rate.
                }


                //Edit the referenced interval value to take the multiplier into account.
                //The game will now run AgeTickMothballed() with this edited interval, accelerating or halting aging.
                interval = multiplier * interval;
            }
        }
    }


    /// <summary>
    /// Patches Pawn_AgeTracker.AgeTick()
    /// This fixes a problem caused by how vanilla RimWorld calculates life stages.
    /// </summary>
    [HarmonyPatch(typeof(Verse.Pawn_AgeTracker), "AgeTick")]
    public static class FasterAgingLifestagePatch
    {
        /// <summary>
        /// Runs after AgeTick(), as often as it is called.
        /// Performs a daily recalculation of life stage.
        /// </summary>
        /// <param name="__instance">Pawn that is having its AgeTick method called</param>
        [HarmonyPostfix]
        public static void DailyRecalc(Pawn_AgeTracker __instance)
        {
            if (Find.TickManager.TicksGame % 60000 == 0)
            {
                //Log.Message("I have performed a daily check");
                MethodInfo info = AccessTools.Method(__instance.GetType(), "RecalculateLifeStageIndex", null, null);
                info.Invoke(__instance, null);
            }
        }
    }
}

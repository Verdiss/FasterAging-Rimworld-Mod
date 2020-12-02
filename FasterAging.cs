/*
 * Faster Aging Mod by Verdiss
 * Mod Version 1.3 2020-12-02 tested for Rimworld 1.2
 * You are free to redistribute this mod and use its code for non-commercial purposes, just give credit to Verdiss in descriptions and in source code.
 * You are also free to laugh at how badly the mod is implemented, and wash out your eyes with bleach after closing your IDE in horror.
 */

using Verse;
using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using System.Reflection;

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


        public static int pawnSpeedMultBeforeCutoff = 1; //Actual value of the pawn speed multiplier before cutoff setting
        public static int pawnSpeedMultAfterCutoff = 1; //Actual value of the pawn speed multiplier after cutoff setting
        public static int pawnCutoffAge = 1000; //Actual value of the pawn cutoff age setting
        public static long pawnCutoffAgeTicks => (pawnCutoffAge * 3600000) + 1000; //Cutoff age converted to ticks. 1000 ticks are added to this value as a buffer around birthdays to prevent repeatedly calling birthday code when aging is disabled.

        public static int animalSpeedMultBeforeCutoff = 1; //Actual value of the animal speed multiplier before cutoff setting
        public static int animalSpeedMultAfterCutoff = 1; //Actual value of the animal speed multiplier after cutoff setting
        public static int animalCutoffAge = 1000; //Actual value of the animal cutoff age setting
        public static long animalCutoffAgeTicks => (animalCutoffAge * 3600000) + 1000; //Cutoff age converted to ticks. 1000 ticks are added to this value as a buffer around birthdays to prevent repeatedly calling birthday code when aging is disabled.

        public static bool modifyChronologicalAge = false; //Whether to also change chronological age, not just biological

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
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        public static void MultiTick(Pawn __instance)
        {
            //Determine multiplier
            int multiplier = 0; //How much the settings say the pawn's age speed should be multiplied by.

            if (__instance.RaceProps.Humanlike)
            {
                //It's a humanlike
                if (__instance.ageTracker.AgeBiologicalTicks < FasterAging.pawnCutoffAgeTicks)
                {
                    //It's before the cutoff age
                    multiplier = FasterAging.pawnSpeedMultBeforeCutoff;
                } else
                {
                    //It's after the cutoff age
                    multiplier = FasterAging.pawnSpeedMultAfterCutoff;
                }
            } else
            {
                //It's an animal
                if (__instance.ageTracker.AgeBiologicalTicks < FasterAging.animalCutoffAgeTicks)
                {
                    //It's before the cutoff age
                    multiplier = FasterAging.animalSpeedMultBeforeCutoff;
                }
                else
                {
                    //It's after the cutoff age
                    multiplier = FasterAging.animalSpeedMultAfterCutoff;
                }
            }


            //Run extra aging.
            if (multiplier == 0) //Aging is disabled - Manually revert the age increase from the core game's AgeTick call
            {
                __instance.ageTracker.AgeBiologicalTicks += -1;
                //Note - there is still a potential issue if the age multiplier setting is changed by the player to 0 on a birthday tick, causing that birthday to get re-run.
                //This is a hilariously impossible edge case, but if I want to improve code safety, this is something to work on.
            }
            else
            {
                //Now repeat the same AgeTick method until it hase been done speedMult times this tick.
                //Just a note for future me and for any other readers on why I am choosing this way of increasing age rate.
                //I technically could simply directly modify AgeBiologicalTicks like I do with disabled aging.
                //However, this means any code that is run through the AgeTick method that expects an exact age tick value,
                //such as birthdays, will not be run. Additionally, any mods that also hook into the AgeTick method likely
                //want their code ran for as many AgeTick calls as there are, not just once a tick. Calling AgeTick repeatedly
                //solves both of these issues, even if it is comparitively quite expensive.
                //In short, I judged this the safest and most compatible way of doing things.
                for (int additionalTick = 0; additionalTick < multiplier - 1; additionalTick++) //AgeTick already runs once naturally - subtract 1 from the multiplier to get how many times it should be called again
                {
                    __instance.ageTracker.AgeTick();
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
        /// <param name="interval"></param>
        /// <param name="__instance"></param>
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
                int multiplier = 0; //How much the settings say the pawn's age speed should be multiplied by.

                if (pawn.RaceProps.Humanlike)
                {
                    //It's a humanlike
                    if (__instance.AgeBiologicalTicks < FasterAging.pawnCutoffAgeTicks)
                    {
                        //It's before the cutoff age
                        multiplier = FasterAging.pawnSpeedMultBeforeCutoff;
                    }
                    else
                    {
                        //It's after the cutoff age
                        multiplier = FasterAging.pawnSpeedMultAfterCutoff;
                    }
                }
                else
                {
                    //It's an animal
                    if (__instance.AgeBiologicalTicks < FasterAging.animalCutoffAgeTicks)
                    {
                        //It's before the cutoff age
                        multiplier = FasterAging.animalSpeedMultBeforeCutoff;
                    }
                    else
                    {
                        //It's after the cutoff age
                        multiplier = FasterAging.animalSpeedMultAfterCutoff;
                    }
                }


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
        /// <param name="__instance"></param>
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

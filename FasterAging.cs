/*
 * Faster Aging Mod by Verdiss
 * Mod Version 1.1 2019-09-05 tested for Rimworld 1.0
 * You are free to redistribute this mod and use its code for non-commercial purposes, just give credit to Verdiss in descriptions and in source code.
 * You are also free to laugh at how badly the mod is implemented, and wash out your eyes with bleach after closing your IDE in horror.
 */

using System;
using Verse;
using Harmony;
using HugsLib;
using HugsLib.Settings;

namespace FasterAging
{
    /*
     * This class loads and prepares HugsLib mod config settings, as well as being loaded into HugsLib system for Harmony patching.
     */
    public class FasterAging : ModBase
    {
        public override string ModIdentifier => "FasterAging";

        private SettingHandle<int> pawnSpeedMultBeforeCutoffSetting; //HugsLib setting controlling the speed multiplier for humanoid pawns below the cutoff age
        private SettingHandle<int> pawnSpeedMultAfterCutoffSetting; //HugsLib setting controlling the speed multiplier for humanoid pawns equal to or above the cutoff age
        private SettingHandle<int> pawnCutoffAgeSetting; //HugsLib setting controlling the cutoff age for age speed for humanoid pawns.

        private SettingHandle<int> animalSpeedMultBeforeCutoffSetting; //HugsLib setting controlling the speed multiplier for animals below the cutoff age
        private SettingHandle<int> animalSpeedMultAfterCutoffSetting; //HugsLib setting controlling the speed multiplier for animals equal to or above the cutoff age
        private SettingHandle<int> animalCutoffAgeSetting; //HugsLib setting controlling the cutoff age for age speed for animals.

        public static int pawnSpeedMultBeforeCutoff = 1; //Actual value of the pawn speed multiplier before cutoff setting
        public static int pawnSpeedMultAfterCutoff = 1; //Actual value of the pawn speed multiplier after cutoff setting
        public static int pawnCutoffAge = 1000; //Actual value of the pawn cutoff age setting

        public static int animalSpeedMultBeforeCutoff = 1; //Actual value of the animal speed multiplier before cutoff setting
        public static int animalSpeedMultAfterCutoff = 1; //Actual value of the animal speed multiplier after cutoff setting
        public static int animalCutoffAge = 1000; //Actual value of the animal cutoff age setting

        /*
         * Runs during game startup and initializes settings and setting values.
         */
        public override void DefsLoaded()
        {
            //Setup and get settings values
            pawnSpeedMultBeforeCutoffSetting = Settings.GetHandle<int>("speedMultSetting", "settings_pawnSpeedMultBefore_title".Translate(), "settings_pawnSpeedMultBefore_description".Translate(), 1); //Using old setting title speedMultSetting for compatability
            pawnSpeedMultBeforeCutoff = pawnSpeedMultBeforeCutoffSetting.Value;

            pawnSpeedMultAfterCutoffSetting = Settings.GetHandle<int>("pawnSpeedMultAfter", "settings_pawnSpeedMultAfter_title".Translate(), "settings_pawnSpeedMultAfter_description".Translate(), 1);
            pawnSpeedMultAfterCutoff = pawnSpeedMultAfterCutoffSetting.Value;

            pawnCutoffAgeSetting = Settings.GetHandle<int>("pawnCutoffAge", "settings_pawnCutoffAge_title".Translate(), "settings_pawnCutoffAge_description".Translate(), 1000);
            pawnCutoffAge = pawnCutoffAgeSetting.Value;


            animalSpeedMultBeforeCutoffSetting = Settings.GetHandle<int>("animalSpeedMultBefore", "settings_animalSpeedMultBefore_title".Translate(), "settings_animalSpeedMultBefore_description".Translate(), 1);
            animalSpeedMultBeforeCutoff = animalSpeedMultBeforeCutoffSetting.Value;

            animalSpeedMultAfterCutoffSetting = Settings.GetHandle<int>("animalSpeedMultAfter", "settings_animalSpeedMultAfter_title".Translate(), "settings_animalSpeedMultAfter_description".Translate(), 1);
            animalSpeedMultAfterCutoff = animalSpeedMultAfterCutoffSetting.Value;

            animalCutoffAgeSetting = Settings.GetHandle<int>("animalCutoffAge", "settings_animalCutoffAge_title".Translate(), "settings_animalCutoffAge_description".Translate(), 1000);
            animalCutoffAge = animalCutoffAgeSetting.Value;
        }

        /*
         * Runs whenever the user changes a setting in the mod config, adjusts variables to match new values during runtime.
         */
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
        }
    }

    /*
     * This class patches the Tick method to adjust with the settings.
     */
    [HarmonyPatch(typeof(Verse.Pawn), "Tick")]
    public static class FasterAgingPatch
    {
        /*
         * Runs on every pawn every tick. The pawn (__instance) has its AgeTick method called as required.
         */
        [HarmonyPostfix]
        public static void multiTick(Pawn __instance)
        {
            int multiplier = 0; //How much the settings say the pawn's age speed should be multiplied by.

            //Determine multiplier
            if (__instance.RaceProps.Humanlike)
            {
                //It's a humanlike
                if (__instance.ageTracker.AgeBiologicalYears < FasterAging.pawnCutoffAge)
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
                if (__instance.ageTracker.AgeBiologicalYears < FasterAging.animalCutoffAge)
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
            if (multiplier == 0) //Aging disabled, reverse every tick of age increase
            {
                __instance.ageTracker.AgeBiologicalTicks += -1; //This theoretically could cause a birthday every tick if the multiplier is set to 0 on the tick before a birthday. It would be better as a prefix patch that prevents AgeTick from even running, but that's a lot of work for a super edge case.
            }
            else
            {
                for (int additionalTick = 0; additionalTick < multiplier - 1; additionalTick++) //Repeat the same AgeTick method until it hase been done speedMult times this tick
                {
                    __instance.ageTracker.AgeTick();
                }
            }
        }
    }
}

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

namespace FasterAging
{
    /// <summary>
    /// Loads and prepares HugsLib mod config settings, as well as being loaded into HugsLib system for Harmony patching.
    /// </summary>
    [EarlyInit]
    public class FasterAging : ModBase
    {
        public override string ModIdentifier => "FasterAging";

        //Disabling autopatching to allow conditional compatibility patches.
        protected override bool HarmonyAutoPatch => false;

        //The Harmony ID. This can really be whatever, I just chose the least creative name possible here.
        Harmony harmony = new Harmony(id: "rimworld.Faster.Aging.main");

        private SettingHandle<long> pawnSpeedMultBeforeCutoffSetting; //HugsLib setting controlling the speed multiplier for humanoid pawns below the cutoff age
        private SettingHandle<long> pawnSpeedMultAfterCutoffSetting; //HugsLib setting controlling the speed multiplier for humanoid pawns equal to or above the cutoff age
        private SettingHandle<long> pawnCutoffAgeSetting; //HugsLib setting controlling the cutoff age for age speed for humanoid pawns.

        private SettingHandle<long> animalSpeedMultBeforeCutoffSetting; //HugsLib setting controlling the speed multiplier for animals below the cutoff age
        private SettingHandle<long> animalSpeedMultAfterCutoffSetting; //HugsLib setting controlling the speed multiplier for animals equal to or above the cutoff age
        private SettingHandle<long> animalCutoffAgeSetting; //HugsLib setting controlling the cutoff age for age speed for animals.

        private SettingHandle<bool> modifyChronologicalAgeSetting; //HugsLib setting controlling whether chronological age is also modified.


        public static long pawnSpeedMultBeforeCutoff = 1; //Actual value of the pawn speed multiplier before cutoff setting
        public static long pawnSpeedMultAfterCutoff = 1; //Actual value of the pawn speed multiplier after cutoff setting
        public static long pawnCutoffAge = 1000; //Actual value of the pawn cutoff age setting
        public static long PawnCutoffAgeTicks => (pawnCutoffAge * 3600000) + 1000; //Cutoff age converted to ticks. 1000 ticks are added to this value as a buffer around birthdays to prevent repeatedly calling birthday code when aging is disabled.

        public static long animalSpeedMultBeforeCutoff = 1; //Actual value of the animal speed multiplier before cutoff setting
        public static long animalSpeedMultAfterCutoff = 1; //Actual value of the animal speed multiplier after cutoff setting
        public static long animalCutoffAge = 1000; //Actual value of the animal cutoff age setting
        public static long AnimalCutoffAgeTicks => (animalCutoffAge * 3600000) + 1000; //Cutoff age converted to ticks. 1000 ticks are added to this value as a buffer around birthdays to prevent repeatedly calling birthday code when aging is disabled.

        public static bool modifyChronologicalAge = false; //Whether to also change chronological age, not just biological

        public override void EarlyInitialize()
        {
            //Debug Logging. Enabling this puts a harmony.log.txt file with transpiler outputs on the desktop.
            //Harmony.DEBUG = true;

            //Harmony patches
            harmony.Patch(AccessTools.Method(typeof(Verse.Pawn_AgeTracker), nameof(Verse.Pawn_AgeTracker.AgeTick)), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(typeof(AgeTick), nameof(AgeTick.FasterAgingTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Verse.Pawn_AgeTracker), nameof(Verse.Pawn_AgeTracker.AgeTickMothballed)), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(typeof(WorldPawnAgeTick), nameof(WorldPawnAgeTick.FasterAgingWorldPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Verse.Pawn_AgeTracker), nameof(Verse.Pawn_AgeTracker.RecalculateLifeStageIndex)), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(typeof(RecalculateLifeStageIndex), nameof(RecalculateLifeStageIndex.FasterAgingLifeStageTranspiler)));
        }

        public override void WorldLoaded()
        {
            //Fix for Rocketman compatibility. This needs to be loaded late, and WorldLoaded() happens to be late.
            if (ModLister.HasActiveModWithName("[NTS] [EXPERIMENTAL] RocketMan"))
            {
                harmony.Patch(AccessTools.Method(typeof(Verse.Pawn_AgeTracker), nameof(Verse.Pawn_AgeTracker.AgeTick)), prefix: null, postfix: null,
                    transpiler: new HarmonyMethod(typeof(CompatPatches), nameof(CompatPatches.RocketmanCompatibilityTranspiler)));
            }
        }

        /// <summary>
        /// Runs during game startup.
        /// Initializes settings and setting values.
        /// </summary>
        public override void DefsLoaded()
        {
            //Setup and get settings values
            pawnSpeedMultBeforeCutoffSetting = Settings.GetHandle<long>(settingName: "speedMultSetting", title: "settings_pawnSpeedMultBefore_title".Translate(), description: "settings_pawnSpeedMultBefore_description".Translate(), defaultValue: 1); //Using old setting title speedMultSetting for compatibility
            pawnSpeedMultBeforeCutoff = pawnSpeedMultBeforeCutoffSetting.Value;

            pawnSpeedMultAfterCutoffSetting = Settings.GetHandle<long>(settingName: "pawnSpeedMultAfter", title: "settings_pawnSpeedMultAfter_title".Translate(), description: "settings_pawnSpeedMultAfter_description".Translate(), defaultValue: 1);
            pawnSpeedMultAfterCutoff = pawnSpeedMultAfterCutoffSetting.Value;

            pawnCutoffAgeSetting = Settings.GetHandle<long>(settingName: "pawnCutoffAge", title: "settings_pawnCutoffAge_title".Translate(), description: "settings_pawnCutoffAge_description".Translate(), defaultValue: 1000);
            pawnCutoffAge = pawnCutoffAgeSetting.Value;


            animalSpeedMultBeforeCutoffSetting = Settings.GetHandle<long>(settingName: "animalSpeedMultBefore", title: "settings_animalSpeedMultBefore_title".Translate(), description: "settings_animalSpeedMultBefore_description".Translate(), defaultValue: 1);
            animalSpeedMultBeforeCutoff = animalSpeedMultBeforeCutoffSetting.Value;

            animalSpeedMultAfterCutoffSetting = Settings.GetHandle<long>(settingName: "animalSpeedMultAfter", title: "settings_animalSpeedMultAfter_title".Translate(), description: "settings_animalSpeedMultAfter_description".Translate(), defaultValue: 1);
            animalSpeedMultAfterCutoff = animalSpeedMultAfterCutoffSetting.Value;

            animalCutoffAgeSetting = Settings.GetHandle<long>(settingName: "animalCutoffAge", title: "settings_animalCutoffAge_title".Translate(), description: "settings_animalCutoffAge_description".Translate(), defaultValue: 1000);
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
            /*if (pawnSpeedMultBeforeCutoffSetting.Value < 0)
            {
                pawnSpeedMultBeforeCutoffSetting.Value = 0; //All settings have checks to prevent negative values. Reverse aging ain't happening. ---> Reverse aging works now, actually. Mostly. The game won't die.
            }*/
            pawnSpeedMultBeforeCutoff = pawnSpeedMultBeforeCutoffSetting.Value;


            /*if (pawnSpeedMultAfterCutoffSetting.Value < 0)
            {
                pawnSpeedMultAfterCutoffSetting.Value = 0;
            }*/
            pawnSpeedMultAfterCutoff = pawnSpeedMultAfterCutoffSetting.Value;


            /*if (pawnCutoffAgeSetting.Value < 0)
            {
                pawnCutoffAgeSetting.Value = 0;
            }*/
            pawnCutoffAge = pawnCutoffAgeSetting.Value;



            /*if (animalSpeedMultBeforeCutoffSetting.Value < 0)
            {
                animalSpeedMultBeforeCutoffSetting.Value = 0;
            }*/
            animalSpeedMultBeforeCutoff = animalSpeedMultBeforeCutoffSetting.Value;


            /*if (animalSpeedMultAfterCutoffSetting.Value < 0)
            {
                animalSpeedMultAfterCutoffSetting.Value = 0;
            }*/
            animalSpeedMultAfterCutoff = animalSpeedMultAfterCutoffSetting.Value;


            /*if (animalCutoffAgeSetting.Value < 0)
            {
                animalCutoffAgeSetting.Value = 0;
            }*/
            animalCutoffAge = animalCutoffAgeSetting.Value;



            modifyChronologicalAge = modifyChronologicalAgeSetting.Value;
        }

        /// <summary>
        /// Gets the aging rate multiplier value for the input pawn, based on the mod's settings, and the pawn's type and age.
        /// </summary>
        /// <param name="pawn">Pawn to determine the age rate multiplier for</param>
        /// <returns></returns>
        public static long GetPawnAgingMultiplier(Pawn_AgeTracker _instance)
        {
            Pawn pawn = _instance.pawn;

            if (pawn.RaceProps.Humanlike)
            {
                //It's a humanlike
                if (pawn.ageTracker.AgeBiologicalTicks >= PawnCutoffAgeTicks)
                {
                    //It's after the cutoff age
                    return pawnSpeedMultAfterCutoff;
                }
                else
                {
                    //It's before the cutoff age
                    return pawnSpeedMultBeforeCutoff;
                }
            }
            else
            {
                //It's an animal
                if (pawn.ageTracker.AgeBiologicalTicks >= AnimalCutoffAgeTicks)
                {
                    //It's after the cutoff age
                    return animalSpeedMultAfterCutoff;
                }
                else
                {
                    //It's before the cutoff age
                    return animalSpeedMultBeforeCutoff;
                }
            }
        }

        //For the pawn tick
        public static void ModifyChronologicalAge(Pawn_AgeTracker _instance, long multiplier)
        {
            //Experimental Chronological age modification
            //I judge this too likely to cause issues and too practically insignificant to include as a default feature.
            if (modifyChronologicalAge)
            {
                _instance.BirthAbsTicks -= multiplier - 1L; //Move the character's birthday earlier in time, accelerating how distant it is from the current. If multiplier is 0, actually moves birthday forward 1 tick, keeping it a constant distance from the current tick and thus a constant age.
            }
        }

        //For the mothball (worldpawn) tick
        public static void ModifyChronologicalAge(Pawn_AgeTracker _instance, long multiplier, int interval)
        {
            //Experimental Chronological age modification
            //I judge this too likely to cause issues and too practically insignificant to include as a default feature.
            if (modifyChronologicalAge)
            {
                _instance.BirthAbsTicks -= (multiplier - 1L) * interval; //Move the character's birthday earlier in time, accelerating how distant it is from the current. If multiplier is 0, actually moves birthday forward 1 tick, keeping it a constant distance from the current tick and thus a constant age.
            }
        }
    }  
}

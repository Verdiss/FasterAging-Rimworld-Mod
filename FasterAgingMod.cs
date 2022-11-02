/*
 * Faster Aging Mod by Verdiss
 * Mod Version 1.5, last updated 2022-06-16, tested for Rimworld 1.3
 * You are free to redistribute this mod and use its code for non-commercial purposes, just give credit to Verdiss in descriptions and in source code.
 */

using Verse;
using HugsLib;
using HugsLib.Settings;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace FasterAging
{
    /// <summary>
    /// Mod Core. Handles settings and setting-values, and provides some key methods for the aging patches.
    /// </summary>
    public class FasterAgingMod : ModBase
    {
        public override string ModIdentifier => "FasterAging";

        //HugsLib Settings
        private SettingHandle<int> pawnAgingMultSetting;
        private SettingHandle<int> pawnAgingMultAfterCutoffSetting;
        private SettingHandle<int> pawnCutoffAgeSetting;

        private SettingHandle<int> animalAgingMultSetting;
        private SettingHandle<int> animalAgingMultAfterCutoffSetting;
        private SettingHandle<int> animalCutoffAgeSetting;

        private SettingHandle<bool> enableAgeCutoffsSetting;

        //private SettingHandle<bool> enablePerPawnRateSetting; //Disabled due to not working


        private SettingHandle<int> biosculptAgeReversalYearsSetting;

        private SettingHandle<int> growthVatAgeTicksPerTickSetting;


        //Vars pulled from the HugsLib Settings
        public static int pawnAgingMult = 1; //Multiplier to human pawn aging speed (before the age cutoff if that system is enabled)
        public static int pawnAgingMultAfterCutoff = 1; //Multiplier to human pawn aging speed after the age cutoff
        public static int pawnCutoffAge = 18; //Human aging rate age cutoff
        public static long pawnCutoffAgeTicks => ((long)pawnCutoffAge * 3600000L) + 1000L; //Cutoff age converted to ticks. 1000 ticks are added to this value as a buffer around birthdays to prevent repeatedly calling birthday code when aging is disabled.

        public static int animalAgingMult = 1; //Multiplier to animal pawn aging speed (before the age cutoff if that system is enabled)
        public static int animalAgingMultAfterCutoff = 1; //Multiplier to animal pawn aging speed after the age cutoff
        public static int animalCutoffAge = 18; //Animal aging rate age cutoff
        public static long animalCutoffAgeTicks => ((long)animalCutoffAge * 3600000L) + 1000L; //Cutoff age converted to ticks. 1000 ticks are added to this value as a buffer around birthdays to prevent repeatedly calling birthday code when aging is disabled.

        public static bool enableAgeCutoffs = false; //Whether the age cutoffs system is enabled

        public static bool enablePerPawnRate = false; //Whether the per-pawn rate system is enabled and its control button shown.


        public static int biosculptAgeReversalYears = 1; //Number of years that are taken off a pawn's age at the completion of an age-reversal biosculpting process


        public static int growthVatAgeTicksPerTick = 20; //Numer of biological age ticks gained per tick in a growth vat


        //Misc
        public static Dictionary<string, int> perPawnRates; //Stores any per-pawn custom selected aging rates. Key is pawn's LoadID, accessed via pawn.GetUniqueLoadID(). Value is aging multiplier for that pawn.
        



        /// <summary>
        /// HugsLib runs this during RimWorld executable startup.
        /// Initializes settings and setting values.
        /// </summary>
        public override void DefsLoaded()
        {
            //Setup and get settings values
            pawnAgingMultSetting = Settings.GetHandle<int>(settingName: "fa_pawnAgingMult", title: "fa_settings_pawnAgingMult_title".Translate(), description: "fa_settings_pawnAgingMult_description".Translate(), defaultValue: 1, Validators.IntRangeValidator(0, 99999));
            pawnAgingMult = pawnAgingMultSetting.Value;

            pawnAgingMultAfterCutoffSetting = Settings.GetHandle<int>(settingName: "fa_pawnAgingMultAfter", title: "fa_settings_pawnAgingMultAfter_title".Translate(), description: "fa_settings_pawnAgingMultAfter_description".Translate(), defaultValue: 1, Validators.IntRangeValidator(0, 99999));
            pawnAgingMultAfterCutoffSetting.VisibilityPredicate = delegate () { return enableAgeCutoffsSetting.Value; }; //Hide if the system is disabled
            pawnAgingMultAfterCutoff = pawnAgingMultAfterCutoffSetting.Value;

            pawnCutoffAgeSetting = Settings.GetHandle<int>(settingName: "fa_pawnCutoffAge", title: "fa_settings_pawnCutoffAge_title".Translate(), description: "fa_settings_pawnCutoffAge_description".Translate(), defaultValue: 18, Validators.IntRangeValidator(0, 99999));
            pawnCutoffAgeSetting.VisibilityPredicate = delegate () { return enableAgeCutoffsSetting.Value; }; //Hide if the system is disabled
            pawnCutoffAge = pawnCutoffAgeSetting.Value;



            animalAgingMultSetting = Settings.GetHandle<int>(settingName: "fa_animalAgingMult", title: "fa_settings_animalAgingMult_title".Translate(), description: "fa_settings_animalAgingMult_description".Translate(), defaultValue: 1, Validators.IntRangeValidator(0, 99999));
            animalAgingMult = animalAgingMultSetting.Value;

            animalAgingMultAfterCutoffSetting = Settings.GetHandle<int>(settingName: "fa_animalAgingMultAfter", title: "fa_settings_animalAgingMultAfter_title".Translate(), description: "fa_settings_animalAgingMultAfter_description".Translate(), defaultValue: 1, Validators.IntRangeValidator(0, 99999));
            animalAgingMultAfterCutoffSetting.VisibilityPredicate = delegate () { return enableAgeCutoffsSetting.Value; }; //Hide if the system is disabled
            animalAgingMultAfterCutoff = animalAgingMultAfterCutoffSetting.Value;

            animalCutoffAgeSetting = Settings.GetHandle<int>(settingName: "fa_animalCutoffAge", title: "fa_settings_animalCutoffAge_title".Translate(), description: "fa_settings_animalCutoffAge_description".Translate(), defaultValue: 18, Validators.IntRangeValidator(0, 99999));
            animalCutoffAgeSetting.VisibilityPredicate = delegate () { return enableAgeCutoffsSetting.Value; }; //Hide if the system is disabled
            animalCutoffAge = animalCutoffAgeSetting.Value;



            enableAgeCutoffsSetting = Settings.GetHandle<bool>(settingName: "fa_enableAgeCutoffs", title: "fa_settings_enableAgeCutoffs_title".Translate(), description: "fa_settings_enableAgeCutoffs_description".Translate(), defaultValue: false);
            enableAgeCutoffs = enableAgeCutoffsSetting.Value;



            //Disabled due to not working
            //enablePerPawnRateSetting = Settings.GetHandle<bool>(settingName: "enablePerPawnRate", title: "fa_settings_enablePerPawnRate_title".Translate(), description: "fa_settings_enablePerPawnRate_description".Translate(), defaultValue: false);
            //enablePerPawnRate = enablePerPawnRateSetting.Value;
            



            biosculptAgeReversalYearsSetting = Settings.GetHandle<int>(settingName: "fa_biosculptAgeReversalYears", title: "fa_settings_biosculptAgeReversalYears_title".Translate(), description: "fa_settings_biosculptAgeReversalYears_description".Translate(), defaultValue: 1, Validators.IntRangeValidator(1, 100));
            biosculptAgeReversalYearsSetting.VisibilityPredicate = delegate () { return ModsConfig.IdeologyActive; }; //Only show when the user has Ideology
            biosculptAgeReversalYears = biosculptAgeReversalYearsSetting.Value;


            growthVatAgeTicksPerTickSetting = Settings.GetHandle<int>(settingName: "fa_growthVatAgeTicksPerTick", title: "fa_settings_growthVatAgeTicksPerTick_title".Translate(), description: "fa_settings_growthVatAgeTicksPerTick_description".Translate(), defaultValue: 20, Validators.IntRangeValidator(1, 99999));
            growthVatAgeTicksPerTickSetting.VisibilityPredicate = delegate () { return ModsConfig.BiotechActive; }; //Only show when the user has Biotech
            growthVatAgeTicksPerTick = growthVatAgeTicksPerTickSetting.Value;
        }

        /// <summary>
        /// HugsLib runs this when a game world is finished loading, i.e. after loading a save or starting a new game
        /// Handles loading game save data
        /// </summary>
        public override void WorldLoaded()
        {
            //Set the per-pawn rates store to be saved/loaded
            Scribe_Collections.Look(ref perPawnRates, "perPawnRatesFA", LookMode.Value, LookMode.Value);
            if (perPawnRates == null)
            {
                perPawnRates = new Dictionary<string, int>(); //Initialize if the dic wasn't loaded
            }
        }

        /// <summary>
        /// Runs whenever the user changes a setting during runtime.
        /// Adjusts the internal variables to match the new settings.
        /// </summary>
        public override void SettingsChanged()
        {
            //TODO there is apparently a better way of doing this via setting's ValueChanged event, but I can't get that working right now. Come back to this in the future.


            pawnAgingMult = pawnAgingMultSetting.Value;
            pawnAgingMultAfterCutoff = pawnAgingMultAfterCutoffSetting.Value;
            pawnCutoffAge = pawnCutoffAgeSetting.Value;

            animalAgingMult = animalAgingMultSetting.Value;
            animalAgingMultAfterCutoff = animalAgingMultAfterCutoffSetting.Value;
            animalCutoffAge = animalCutoffAgeSetting.Value;

            enableAgeCutoffs = enableAgeCutoffsSetting.Value;

            //Disabled due to not working
            //enablePerPawnRate = enablePerPawnRateSetting.Value;
            //if (!enablePerPawnRate && perPawnRates != null) perPawnRates.Clear(); //Removes all previously selected custom rates when toggling off - this allows the user to reset their choices if they did something silly.


            biosculptAgeReversalYears = biosculptAgeReversalYearsSetting.Value;


            growthVatAgeTicksPerTick = growthVatAgeTicksPerTickSetting.Value;
        }

        /// <summary>
        /// Gets the aging rate multiplier value for the input pawn, based on the mod's settings, and the pawn's type and age, and whether the pawn has a per-pawn rate set
        /// </summary>
        /// <param name="pawn">Pawn to determine the age rate multiplier for</param>
        /// <returns></returns>
        public static int GetPawnAgingMultiplier(Pawn pawn)
        {
            //First check if the pawn has a per-pawn rate selected and use it if they do
            if (enablePerPawnRate)
            {
                int perPawnRate = GetPerPawnAgingMultiplier(pawn);
                if (perPawnRate != -1)
                {
                    return perPawnRate;
                }
            }

            if (!pawn.RaceProps.Humanlike)
            {
                //It's an animal
                if (enableAgeCutoffs && pawn.ageTracker.AgeBiologicalTicks >= animalCutoffAgeTicks)
                {
                    //It's after the cutoff age and that system is enabled
                    //Log.Message("Animal pawn after cutoff, name: " + pawn.Name + ", age ticks: " + pawn.ageTracker.AgeBiologicalTicks + ", cutoff ticks: " + animalCutoffAgeTicks);
                    return animalAgingMultAfterCutoff;
                }
                else
                {
                    //It's before the cutoff age or the cutoff system is disabled
                    //Log.Message("Animal pawn before cutoff, name: " + pawn.Name + ", age ticks: " + pawn.ageTracker.AgeBiologicalTicks + ", cutoff ticks: " + animalCutoffAgeTicks);
                    return animalAgingMult;
                }
            }
            else
            {
                //It's a humanlike
                if (enableAgeCutoffs && pawn.ageTracker.AgeBiologicalTicks >= pawnCutoffAgeTicks)
                {
                    //It's after the cutoff age and that system is enabled
                    //Log.Message("Human pawn after cutoff, name: " + pawn.Name + ", age ticks: " + pawn.ageTracker.AgeBiologicalTicks + ", cutoff ticks: " + pawnCutoffAgeTicks);
                    return pawnAgingMultAfterCutoff;
                }
                else
                {
                    //It's before the cutoff age or the cutoff system is disabled
                    //Log.Message("Human pawn before cutoff, name: " + pawn.Name + ", age ticks: " + pawn.ageTracker.AgeBiologicalTicks + ", cutoff ticks: " + pawnCutoffAgeTicks);
                    return pawnAgingMult; //This is intentionally the default case - if I add more conditions I should move this to the last else
                }
            }
        }


        /// <summary>
        /// Returns the per-pawn custom aging rate associated with the input Pawn, if it exists. If it is not found, returns -1.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static int GetPerPawnAgingMultiplier(Pawn pawn)
        {
            if (perPawnRates.ContainsKey(pawn.GetUniqueLoadID())) return perPawnRates[pawn.GetUniqueLoadID()];

            return -1;
        }

        /// <summary>
        /// Sets the per-pawn custom aging rate for the input pawn to the input multiplier.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="mult"></param>
        public static void SetPerPawnAgingMultiplier(Pawn pawn, int mult)
        {
            perPawnRates[pawn.GetUniqueLoadID()] = mult;
        }
    }
}

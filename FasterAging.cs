/*
 * Faster Aging Mod by Verdiss
 * Mod Version 1.0 2019-08-31 tested for Rimworld 1.0
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
     * This class loads and prepares HugsLib mod config settings, as well as being loaded into HugsLib for Harmony patching.
     */
    public class FasterAging : ModBase
    {
        public override string ModIdentifier => "FasterAging";

        private SettingHandle<int> speedMultSetting; //HugsLib setting controlling the speed multiplier

        public static int speedMult = 1; //Actual value of the speed multiplier setting

        public override void DefsLoaded()
        {
            //Setup and get setting value
            speedMultSetting = Settings.GetHandle<int>("speedMultSetting", "settings_speedMult_title".Translate(), "settings_speedMult_description".Translate(), 1);
            speedMult = speedMultSetting.Value;
        }

        public override void SettingsChanged()
        {
            if (speedMultSetting.Value < 0)
            {
                speedMultSetting.Value = 0; //Don't go under 0, because bad things (might) happen
            }
            speedMult = speedMultSetting.Value;
        }
    }

    /*
     * This class patches the Tick() method of pawns to repeat ticks as many times as required by the config.
     */
    [HarmonyPatch(typeof(Verse.Pawn), "Tick")]
    public static class FasterAgingPatch
    {
        [HarmonyPostfix]
        public static void multiTick(Pawn __instance)
        {
            if (FasterAging.speedMult == 0) //Aging disabled, reverse every tick of age increase
            {
                __instance.ageTracker.AgeBiologicalTicks += -1; //This theoretically could cause a birthday every tick if the multiplier is set to 0 on the tick before a birthday. It would be better as a prefix patch that prevents AgeTick from even running, but that's a lot of work for a super edge case.
            }
            else
            {
                for (int additionalTick = 0; additionalTick < FasterAging.speedMult - 1; additionalTick++) //Repeat the same AgeTick method until it hase been done speedMult times this tick
                {
                    __instance.ageTracker.AgeTick();
                }
            }
        }
    }
}

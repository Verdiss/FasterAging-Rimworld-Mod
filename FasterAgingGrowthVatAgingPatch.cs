using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FasterAging
{
    [HarmonyPatch(typeof(Pawn_AgeTracker), "Notify_TickedInGrowthVat")]
    public static class FasterAgingGrowthVatAgingPatch
    {
        [HarmonyPrefix]
        public static void Notify_TickedInGrowthVatInterceptArgs(ref int ticks)
        {
            //This intercepts the call to Notify_TickedInGrowthVat to edit the input ticks amount
            //This is necessary because whoever programmed the Growth Vat Tick method, instead of using the 100% unreferenced AgeTicksPerTickInGrowthVat,
            //which of course would be a very easy thing to edit the usage of, instead of using that, decided to simply use the value it was set to (20), in the place it should have been used
            //This of course makes my life difficult. It's a pure stroke of luck that Notify_TickedInGrowthVat is a middle-man function that gives me access to this call chain
            //But yeah this is annoying has hell.

            //That aside, what this does in practice is change the interval value that the pawn gets passed into their AgeTickMothballed by the growth vat.
            //This and this alone determines how quickly they age, when in the vat.
            //Note that growth vat aging does not account for any other settings, or genes. Pawns will always age exactly growthVatAgeTicksPerTick ticks per tick in a vat.
            ticks = FasterAgingMod.growthVatAgeTicksPerTick;
        }
    }
}

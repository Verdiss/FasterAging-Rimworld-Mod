using Verse;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace FasterAging
{
    /// <summary>
    /// Patches Pawn.GetGizmos()
    /// Adds a new Gizmo (pawn control bar button) that allows the player to set a custom aging rate for the selected pawn
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    [StaticConstructorOnStartup]
    public static class FasterAgingPerPawnGizmoPatch
    {
        public static Texture2D perPawnGizmoIcon = ContentFinder<Texture2D>.Get("UI/Commands/FAPerPawnGizmo", true);


        //Below disabled as per pawn system is not working
        ///// <summary>
        ///// If the per-pawn aging system is enabled, then patch adds a gizmo (pawn control bar button) to the pawn that opens a ui to change that rate for this pawn.
        ///// </summary>
        ///// <param name="__result"></param>
        ///// <param name="__instance"></param>
        ///// <returns></returns>
        //[HarmonyPostfix]
        //public static IEnumerable<Gizmo> AddPerPawnRateControlGizmo(IEnumerable<Gizmo> __result, Pawn __instance)
        //{
        //    //Copied from another mod -- I don't really understand how yield works but I think it returns all of the prior gizmos already stored in __result before going on to building and returning this mod's gizmo
        //    //This seemed like a cleaner way of doing things than the other way I found.
        //    foreach (var gizmo in __result)
        //    {
        //        yield return gizmo;
        //    }

        //    if (FasterAgingMod.enablePerPawnRate)
        //    {
        //        yield return new Command_Action
        //        {
        //            defaultLabel = "fa_perPawnRateGizmoLabel".Translate(),
        //            defaultDesc = "fa_perPawnRateGizmoDesc".Translate(),
        //            icon = perPawnGizmoIcon,
        //            action = delegate
        //            {
        //                //Open a dialog box with a slider to select the aging rate
        //                string textGetter(int x) => (x != -1 ? "fa_perPawnDialogText".Translate(x) : "fa_perPawnDialogText_global".Translate(x));
        //                Dialog_Slider window = new Dialog_Slider(textGetter, -1, 100, delegate (int value)
        //                {
        //                    FasterAgingMod.SetPerPawnAgingMultiplier(__instance, value);
        //                }, FasterAgingMod.GetPerPawnAgingMultiplier(__instance));
        //                Find.WindowStack.Add(window);
        //            },
        //        };
        //    }
        //}
    }
}

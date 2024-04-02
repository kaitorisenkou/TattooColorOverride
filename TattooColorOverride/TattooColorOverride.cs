using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace TattooColorOverride {
    [StaticConstructorOnStartup]
    public class TattooColorOverride {
        static TattooColorOverride() {
            Log.Message("[TattooColorOverride] Now active");
            var harmony = new Harmony("kaitorisenkou.TattooColorOverride");
            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderNode_Tattoo), nameof(PawnRenderNode_Tattoo.ColorFor), null, null),
                null,
                new HarmonyMethod(typeof(TattooColorOverride), nameof(Patch_TattooColorFor), null),
                null,
                null
                );
            Log.Message("[TattooColorOverride] Harmony patch complete!");
        }

        public static void Patch_TattooColorFor(ref Color __result, Pawn pawn, PawnRenderNode_Tattoo __instance) {
            Pawn_StyleTracker styleTracker = pawn.style;
            if (styleTracker == null) return;
            TattooDef tattoo = null;
            if (__instance is PawnRenderNode_Tattoo_Body) {
                tattoo = styleTracker.BodyTattoo;
            }
            if (__instance is PawnRenderNode_Tattoo_Head) {
                tattoo = styleTracker.FaceTattoo;
            }
            var ovr = tattoo?.GetModExtension<ModExtension_TattooColorOverride>();
            if (ovr == null) return;
            __result = ovr.color;
            return;
        }
    }
}

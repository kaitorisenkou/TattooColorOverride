using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using System.Reflection;

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
            if (AccessTools.AllAssemblies().Any(t => t.FullName.Contains("AlienRace"))) {
                var HARHArmonyMethod = new HarmonyMethod(typeof(TattooColorOverride), nameof(Patch_HAR), null);
                HARHArmonyMethod.after = new string[] { "rimworld.erdelf.alien_race.main" };
                harmony.Patch(
                    AccessTools.Method(typeof(TattooDef), nameof(TattooDef.GraphicFor), null, null),
                    null,
                    null,
                    HARHArmonyMethod,
                    null
                    );
            }
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


        public static IEnumerable<CodeInstruction> Patch_HAR(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            Log.Message("[TattooColorOverride] Patch_HAR");
            var instructionList = instructions.ToList();
            int stage = 0;
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Ldstr) {
                    var label0 = generator.DefineLabel();
                    instructionList[i+7].labels = new List<Label>() { label0 };
                    i -= 2;
                    var label1 = generator.DefineLabel();
                    instructionList[i].labels = new List<Label>() { label1 };
                    instructionList.InsertRange(
                        i,
                        new CodeInstruction[] {
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Def),nameof(Def.HasModExtension),generics:new Type[]{typeof(ModExtension_TattooColorOverride)})),
                            new CodeInstruction(OpCodes.Brfalse,label1),
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(TattooColorOverride),nameof(TattooColorOverride.GetTattooColorOverride))),
                            new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Color),"get_white")),
                            new CodeInstruction(OpCodes.Br,label0)
                        });
                    stage++;
                    break;
                }
            }
            if (stage < 1) {
                Log.Error("[TattooColorOverride] Patch_HAR failed (stage:" + stage + ")");
            }
            return instructionList;
        }
        public static Color GetTattooColorOverride(TattooDef def) {
            var ov = def.GetModExtension<ModExtension_TattooColorOverride>();
            return ov.color;
        }
    }
}

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CraftWithColor {
    [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
    public static partial class GenRecipe_MakeRecipeProducts_Patch {
        public static void Prefix(Thing product, ref Precept_ThingStyle precept) {
            if (product != null) {
                var def = product.def;
                if (State.StyleActiveForLast(def)) {
                    precept = null;
                }
            }
        }

        public static void Postfix(Thing product) {
            if (product != null) {
                product = product.GetInnerIfMinified();
                var def = product.def;
                Color? color = State.ColorForLast(def);
                if (color.HasValue) {
                    product.TryGetComp<CompColorable>()?.SetColor(color.Value);
                }

                if (State.StyleActiveForLast(def)) {
                    var style = State.StyleForLast(def);
                    product.StyleDef = style;
                }
            }
        }
    }
}

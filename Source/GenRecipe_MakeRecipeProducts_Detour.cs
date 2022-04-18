﻿using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static partial class GenRecipe_MakeRecipeProducts_Detour
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> result)
        {
            return new EnumerableWithActionOnNext<Thing>(result, Process);
        }

        private static void Process(Thing thing)
        {
            Color? color = State.ColorForLast(thing?.def);
            if (color.HasValue)
            {
                thing.TryGetComp<CompColorable>()?.SetColor(color.Value);
            }

        }
    }
}

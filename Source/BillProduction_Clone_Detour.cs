﻿using HarmonyLib;
using RimWorld;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(Bill_Production), nameof(Bill_Production.Clone))]
    public static class BillProduction_Clone_Detour
    {
        public static void Postfix(Bill_Production __instance, Bill __result)
        {
            State.AddClone(__instance, (Bill_Production) __result);
        }
    }
}

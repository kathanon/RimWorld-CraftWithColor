using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CraftWithColor {
    public static class Math_Compat {

        public static readonly bool Active = 
            ModLister.AllInstalledMods.Any(m => m.PackageIdNonUnique == StaticStrings.MATH_ID && m.Active);

        public static readonly Type Dialog =
            Active ? Find("CrunchyDuck.Math.", "Dialog_MathBillConfig", "", "Dialogs.") : null;

        private static Type Find(string prefix, string name, params string[] paths) {
            foreach (var path in paths) {
                var type = AccessTools.TypeByName(prefix + path + name);
                if (type != null) return type;
            }
            return null;
        }
    }

    [HarmonyPatch]
    public static class MathCompat_DoWindowContents_Patch {

        [HarmonyPrepare]
        public static bool ShouldPatch() => 
            Math_Compat.Active;

        [HarmonyTargetMethod]
        public static MethodBase Method() => 
            AccessTools.Method(Math_Compat.Dialog, "DoWindowContents");

        [HarmonyPrefix]
        public static void DoWindowContents(Rect inRect, Bill_Production ___bill, float ___extraPanelAllocation) => 
            DialogBillConfig_Patches.DoWindowContents(inRect, ___bill, -___extraPanelAllocation);
    }

#if !VERSION_1_3
    [HarmonyPatch]
    public static class MathCompat_LateWindowOnGUI_Patch {

        [HarmonyPrepare]
        public static bool ShouldPatch() => 
            Math_Compat.Active;

        [HarmonyTargetMethod]
        public static MethodBase Method() => 
            AccessTools.Method(Math_Compat.Dialog, "LateWindowOnGUI");

        [HarmonyPrefix]
        public static void LateWindowOnGUI(Bill_Production ___bill) => 
            DialogBillConfig_Patches.LateWindowOnGUI(___bill);
    }
#endif
}

using HarmonyLib;
using ImprovedWorkbenches;
using RimWorld;
using System.Linq;
using System.Reflection;
using Verse;

namespace CraftWithColor {
    [HarmonyPatch]
    public static class BetterWorkbenchManagement_ExtendedBillDataStorage_MirrorBills_Patch {
        public static bool Prepare() {
            return BetterWorkbenchManagement.Active;
        }

        public static MethodBase TargetMethod() {
            return typeof(ExtendedBillDataStorage).GetMethod(nameof(ExtendedBillDataStorage.MirrorBills));
        }

        public static void Postfix(Bill_Production sourceBill, Bill_Production destinationBill) {
            Log.Message("ExtendedBillDataStorage.MirrorBills");
            State.BWM_MirrorBills(sourceBill, destinationBill);
        }
    }

    [HarmonyPatch]
    public static class BetterWorkbenchManagement_BillCopyPaste_CanWorkTableDoRecipeNow_Patch {
        public static bool Prepare() {
            return BetterWorkbenchManagement.Active;
        }

        public static MethodBase TargetMethod() {
            return typeof(BillCopyPaste).GetMethod("CanWorkTableDoRecipeNow", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static void Prefix(ref RecipeDef recipe) {
            recipe = State.GetOriginalRecipie(recipe);
        }
    }

    public static class BetterWorkbenchManagement {
        public static readonly bool Active =
            ModLister.AllInstalledMods.Any(m => m.PackageIdNonUnique == Strings.BWM_ID && m.Active);
    }
}

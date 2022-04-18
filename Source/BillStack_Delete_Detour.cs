using HarmonyLib;
using RimWorld;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(BillStack), nameof(BillStack.Delete))]
    public static class BillStack_Delete_Detour
    {
        public static void Postfix(Bill bill)
        {
            State.RemoveBill(bill);
        }
    }
}

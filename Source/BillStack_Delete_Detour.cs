using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

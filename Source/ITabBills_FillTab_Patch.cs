using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CraftWithColor
{
    [HarmonyPatch(typeof(ITab_Bills), "FillTab")]
    public static class ITabBills_FillTab_Patch
    {
        public static bool disable = false;

        public static void Prefix()
        {
            State.ResetBill(BillUtility.Clipboard);
        }

        public static void Postfix()
        {
            State.UpdateBill(BillUtility.Clipboard);
        }
    }
}

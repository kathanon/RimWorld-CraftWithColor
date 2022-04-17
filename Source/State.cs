using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    internal class State
    {
        private static Dictionary<Bill_Production, BillAddition> dict = new Dictionary<Bill_Production, BillAddition>();
        public static Bill_Production LastFinishedBill = null;

        internal static void UnsetLastFinishedBillIf(Bill_Production bill)
        {
            if (LastFinishedBill == bill)
            {
                LastFinishedBill = null;
            }
        }

        // TODO: save/load additions
        // TODO: remove additions for old bills

        internal static BillAddition GetAddition(Bill_Production bill)
        {
            if (!dict.ContainsKey(bill))
            {
                dict[bill] = new BillAddition(bill.recipe);
            }
            return dict[bill];
        }

        public static Color? ColorForLast(ThingDef def)
        {
            foreach (var product in LastFinishedBill.recipe.products)
            {
                if (product.thingDef == def)
                {
                    return ColorFor(LastFinishedBill);
                }
            }
            return null;
        }

        internal static Color? ColorFor(Bill bill)
        {
            if (bill is Bill_Production)
            {
                if (dict.TryGetValue(bill as Bill_Production, out BillAddition add))
                {
                    if (add.active)
                    {
                        return add.TargetColor;
                    }
                }
            }
            return null;
        }
    }

}

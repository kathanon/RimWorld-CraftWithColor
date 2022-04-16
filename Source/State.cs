using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    internal class State
    {
        private static Dictionary<Bill_Production, BillAddition> billDict = new Dictionary<Bill_Production, BillAddition>();
        private static Dictionary<RecipeDef, BillAddition> recipieDict = new Dictionary<RecipeDef, BillAddition>();
        public static Bill_Production LastFinishedBill = null;

        public static Color? ColorForLast => ColorFor(LastFinishedBill);

        // TODO: save/load additions
        // TODO: remove additions for old bills

        internal static BillAddition GetAddition(Bill_Production bill)
        {
            if (!billDict.ContainsKey(bill))
            {
                billDict[bill] = new BillAddition(bill.recipe);
            }
            return billDict[bill];
        }

        internal static Color? ColorFor(Bill bill)
        {
            if (bill is Bill_Production)
            {
                if (billDict.TryGetValue(bill as Bill_Production, out BillAddition add))
                {
                    if (add.active)
                    {
                        return add.TargetColor;
                    }
                }
            }
            return null;
        }

        internal static void RecipieCreated(BillAddition add)
        {
            recipieDict.Add(add.ColoredRecipie, add);
        }
    }

}

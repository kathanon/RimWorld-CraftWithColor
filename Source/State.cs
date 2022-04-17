using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    internal class State
    {
        private static Dictionary<Bill, BillAddition> dict = 
            new Dictionary<Bill, BillAddition>();
        public static Bill_Production LastFinishedBill = null;

        public static void UnsetLastFinishedBillIf(Bill_Production bill)
        {
            if (LastFinishedBill == bill)
            {
                LastFinishedBill = null;
            }
        }

        // TODO: remove additions for old bills

        public static BillAddition GetAddition(Bill_Production bill)
        {
            if (!dict.ContainsKey(bill))
            {
                dict[bill] = new BillAddition(bill.recipe);
            }
            return dict[bill];
        }

        public static void UpdateBill(Bill bill)
        {
            if (dict.ContainsKey(bill))
            {
                dict[bill].UpdateBill(bill as Bill_Production);
            }
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

        public static Color? ColorFor(Bill bill)
        {
            if (dict.TryGetValue(bill, out BillAddition add))
            {
                if (add.active)
                {
                    return add.TargetColor;
                }
            }
            return null;
        }

        public static void RemoveBill(Bill bill)
        {
            dict.Remove(bill);
        }

        private static void CleanupBills()
        {
            var toDelete = new List<Bill>(dict.Keys.Where(b => b.deleted || !(b is Bill_Production)));
            foreach (var key in toDelete)
            {
                dict.Remove(key);
            }
        }

        public static void ExposeData(SaveState save)
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                CleanupBills();
            }
            if (Scribe.EnterNode(Main.ModId)) {
                Scribe_Collections.Look(ref dict, "additions", LookMode.Reference, LookMode.Deep, ref save.addsKeyWorkList, ref save.addsValueWorkList);
                Scribe.ExitNode();
            }
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                CleanupBills();
                foreach (var add in dict)
                {
                    add.Value.UpdateBill(add.Key as Bill_Production);
                }
            }
        }
    }

    public class SaveState : GameComponent
    {
        internal List<Bill> addsKeyWorkList;
        internal List<BillAddition> addsValueWorkList;

        public SaveState(Game game) { }

        public override void ExposeData()
        {
            State.ExposeData(this);
        }
    }

}

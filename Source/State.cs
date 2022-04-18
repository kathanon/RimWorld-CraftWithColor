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

        private static Dictionary<Bill, BillAddition> dict = new Dictionary<Bill, BillAddition>();
        private static List<Color> savedColors = new List<Color>();
        public static Color? defaultColor = null;
        
        public static Bill_Production LastFinishedBill = null;

        public static List<Color> SavedColors { get { return savedColors; } }

        public static Color DefaultColor
        {
            get 
            {
                if (!defaultColor.HasValue) 
                {
                    defaultColor = 
                          Find.IdeoManager.classicMode 
                        ? DefDatabase<ColorDef>.GetRandom().color 
                        : Find.FactionManager.OfPlayer.ideos.PrimaryIdeo.ApparelColor;
                }
                return defaultColor.Value;
            }
        }

        public static void UnsetLastFinishedBillIf(Bill_Production bill)
        {
            if (LastFinishedBill == bill)
            {
                LastFinishedBill = null;
            }
        }

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
            if (def != null)
            {
                foreach (var product in LastFinishedBill.recipe.products)
                {
                    if (product.thingDef == def)
                    {
                        return ColorFor(LastFinishedBill);
                    }
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
                Scribe_Collections.Look(ref savedColors, "savedColors", LookMode.Value, null);
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
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                int max = SelectColorDialog.SavedColorsMax;
                if (savedColors == null) 
                {
                    savedColors = new List<Color>();
                }
                else if (savedColors.Count > max)
                {
                    savedColors.RemoveRange(max, savedColors.Count - max);
                }
                defaultColor = null;
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

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CraftWithColor {
    using AdditionDict = Dictionary<Bill, BillAddition>;
    using RecipeDict = Dictionary<RecipeDef, RecipeDef>;
    using StyleDict = Dictionary<ThingDef, IEnumerable<ThingStyleDef>>;
    
    internal class State {


        private static AdditionDict dict = new AdditionDict();
        private static readonly RecipeDict recipes = new RecipeDict();
        private static readonly StyleDict styles = new StyleDict();
        private static List<Color> savedColors = new List<Color>();
        public static Color? defaultColor = null;

        public static Bill_Production LastFinishedBill = null;
        public static Color? LastFinishedBillColor = null;

        public static List<Color> SavedColors { get => savedColors; }

        public static IEnumerable<ThingStyleDef> StylesFor(ThingDef thing) {
            if (thing == null) return Enumerable.Empty<ThingStyleDef>();
            if (!styles.ContainsKey(thing)) {
                if (thing.HasComp(typeof(CompStyleable))) {
                    var list = (from s in DefDatabase<StyleCategoryDef>.AllDefs
                                where s.thingDefStyles != null
                                from pair in s.thingDefStyles
                                where pair.ThingDef == thing
                                select pair.StyleDef).ToList();
                    styles[thing] = list.Any() ? list : Enumerable.Empty<ThingStyleDef>();
                } else {
                    styles[thing] = Enumerable.Empty<ThingStyleDef>();
                }
            }
            return styles[thing];
        }

        public static Color DefaultColor {
            get {
                if (!defaultColor.HasValue) {
                    defaultColor =
                          Find.IdeoManager.classicMode
                        ? DefDatabase<ColorDef>.GetRandom().color
                        : Find.FactionManager.OfPlayer.ideos.PrimaryIdeo.ApparelColor;
                }
                return defaultColor.Value;
            }
        }

        public static void UnsetLastFinishedBillIf(Bill_Production bill) {
            if (LastFinishedBill == bill) {
                LastFinishedBill = null;
                LastFinishedBillColor = null;
            }
        }

        public static BillAddition TryGetAddition(Bill bill) =>
            bill != null ? dict.TryGetValue(bill) : null;

        public static BillAddition GetAddition(Bill_Production bill) {
            if (!dict.ContainsKey(bill)) {
                dict[bill] = new BillAddition(bill);
            }
            return dict[bill];
        }

        public static void UpdateBill(Bill bill) {
            if (bill != null && dict.ContainsKey(bill)) {
                dict[bill].UpdateBill();
            }
        }

        public static void ResetBill(Bill bill) {
            if (bill != null && dict.ContainsKey(bill)) {
                dict[bill].ResetBill();
            }
        }

        public static void AddClone(Bill_Production original, Bill_Production clone) {
            if (dict.ContainsKey(original)) {
                dict[clone] = new BillAddition(clone, dict[original]);
            }
        }

        public static void UpdateAll() {
            foreach (var add in dict.Values) {
                add.UpdateBill();
            }
        }

        public static void TriggerRecolor() {
            if (MySettings.SwitchColor) {
                foreach (var add in dict.Values) {
                    add.TriggerRecolor();
                }
            }
        }

        private static readonly List<ThingDefCountClass> ColorForLast_EMPTY = new List<ThingDefCountClass>();

        public static Color? ColorForLast(ThingDef def) => ForLast(def, ColorFor, LastFinishedBillColor);

        public static ThingStyleDef StyleForLast(ThingDef def) => ForLast(def, StyleFor);

        private static T ForLast<T>(ThingDef def, Func<Bill_Production,T> fromBill, T value = default(T)) {
            if (def != null) {
                foreach (var product in LastFinishedBill?.recipe?.products ?? ColorForLast_EMPTY) {
                    if (product.thingDef == def) {
                        return (value != null) ? value : fromBill(LastFinishedBill);
                    }
                }
            }
            return default(T);
        }

        public static Color? ColorFor(Bill bill) => dict.TryGetValue(bill, out BillAddition add) ? add.ActiveColor : null;

        public static ThingStyleDef StyleFor(Bill bill) => dict.TryGetValue(bill, out BillAddition add) ? add.ActiveStyle : null;

        public static void RemoveBill(Bill bill) {
            if (bill != null) {
                if (dict.ContainsKey(bill)) {
                    dict[bill].RemoveFromRecipieMap();
                }
                dict.Remove(bill);
            }
        }

        public static RecipeDef GetOriginalRecipie(RecipeDef colored) {
            if (recipes.ContainsKey(colored)) {
                return recipes[colored];
            }
            return colored;
        }

        public static void AddToRecipieMap(RecipeDef colored, RecipeDef original) {
            if (colored != null && original != null) {
                recipes[colored] = original;
            }
        }

        public static void RemoveFromRecipieMap(RecipeDef colored) {
            if (colored != null) {
                recipes.Remove(colored);
            }
        }

        public static void BWM_MirrorBills(Bill_Production source, Bill_Production dest) {
            if (dict.ContainsKey(source)) {
                GetAddition(dest).CopyFrom(dict[source]);
            }
        }

        private static void CleanupBills() {
            var toDelete = dict.Keys.Where(b => b.DeletedOrDereferenced || !(b is Bill_Production)).ToList();
            foreach (var key in toDelete) {
                RemoveBill(key);
            }
        }

        public static void ExposeData(SaveState save) {
            if (Scribe.mode == LoadSaveMode.Saving) {
                CleanupBills();
            }

            if (Scribe.EnterNode(Strings.MOD_IDENTIFIER)) {
                Scribe_Collections.Look(ref dict, "additions", LookMode.Reference, LookMode.Deep, ref save.addsKeyWorkList, ref save.addsValueWorkList);
                Scribe_Collections.Look(ref savedColors, "savedColors", LookMode.Value, null);
                Scribe.ExitNode();
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                CleanupBills();
                foreach (var add in dict) {
                    add.Value.SetBillAfterLoad(add.Key as Bill_Production);
                    add.Value.UpdateBill();
                }
            } else if (Scribe.mode == LoadSaveMode.LoadingVars) {
                int max = SelectColorDialog.SavedColorsMax;
                if (savedColors == null) {
                    savedColors = new List<Color>();
                } else if (savedColors.Count > max) {
                    savedColors.RemoveRange(max, savedColors.Count - max);
                }
                defaultColor = null;
            }
        }
    }

    public class SaveState : GameComponent {
        internal List<Bill> addsKeyWorkList;
        internal List<BillAddition> addsValueWorkList;

        public SaveState(Game game) { }

        public override void ExposeData() {
            State.ExposeData(this);
        }
    }

}

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace CraftWithColor {
    using RandomType = BillAddition.RandomType;
    using AdditionDict = Dictionary<Bill, BillAddition>;
    using RecipeDict = Dictionary<RecipeDef, RecipeDef>;
    using StyleDict = Dictionary<ThingDef, IEnumerable<ThingStyleDef>>;

#if VERSION_1_3
    internal enum ColorType {
        Ideo
    }
#endif 

    internal static class State {
        private static AdditionDict dict = new AdditionDict();
        private static readonly RecipeDict recipes = new RecipeDict();
        private static readonly StyleDict styles = new StyleDict();
        private static List<Color> savedColors = new List<Color>();
        private static readonly Dictionary<string, ColorDef> customColorDefs = new Dictionary<string, ColorDef>();
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
                    thing.randomStyle?.Select(x => x.StyleDef).Do(list.Add);
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
                        ? BillAddition.RandomColor[RandomType.Standard] ?? Color.white
                        : Find.FactionManager?.OfPlayer?.ideos?.PrimaryIdeo?.ApparelColor;
                }
                return defaultColor ?? Color.white;
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


        public static ColorDef CustomColorDef(Color color, ColorType type) {
            var c32 = (Color32) color;
            int cint = (c32.r << 16) | (c32.g << 8) | c32.b;
            return CustomColorDef(Strings.DEF + cint, color, type);
        }

        public static ColorDef CustomColorDef(string name, ColorType type) {
            if (name?.StartsWith(Strings.DEF) ?? false) {
                try {
                    int c = int.Parse(name.Substring(Strings.DEF.Length));
                    var color = new ColorInt(c >> 16, (c >> 8) & 0xFF, c & 0xFF).ToColor;
                    return CustomColorDef(name, color, type);
                } catch {}
            }
            return null;
        }
        
        private static readonly Dictionary<Type, HashSet<ushort>> takenHashDict =
            Traverse.Create(typeof(ShortHashGiver)).Field<Dictionary<Type, HashSet<ushort>>>("takenHashesPerDeftype").Value;
        private static readonly Action<Def, Type, HashSet<ushort>> giveHash = 
            AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash").CreateDelegate<Action<Def, Type, HashSet<ushort>>>();
        private static readonly Type colorDef = typeof(ColorDef);

        public static T CreateDelegate<T>(this MethodInfo method) where T : Delegate {
            return (T) method.CreateDelegate(typeof(T));
        }

        private static ColorDef CustomColorDef(string name, Color color, ColorType type) {
            if (!customColorDefs.ContainsKey(name)) {
                var def = customColorDefs[name] = new ColorDef { 
                    color = color,
                    defName = name,
                    label = "custom color",
#if VERSION_1_3
                    hairOnly = false,
#else
                    colorType = type, 
                    displayInStylingStationUI = false,
#endif
                };
                giveHash(def, colorDef, takenHashDict[colorDef]);
                DefDatabase<ColorDef>.Add(def);
            }
            return customColorDefs[name];
        }


        private static readonly IEnumerable<ThingDefCountClass> ForLast_EMPTY = 
            Enumerable.Empty<ThingDefCountClass>();

        public static Color? ColorForLast(ThingDef def) 
            => ForLast(def, ColorFor, LastFinishedBillColor, true);

        public static ThingStyleDef StyleForLast(ThingDef def) 
            => ForLast(def, StyleFor);

        public static bool StyleActiveForLast(ThingDef def) 
            => ForLast(def, StyleActiveFor);

        private static T ForLast<T>(ThingDef def, Func<BillAddition,T> fromAdd, T value = default, bool color = false) {
            if (def != null) {
                foreach (var product in LastFinishedBill?.recipe.products ?? ForLast_EMPTY) {
                    if (product.thingDef == def) {
                        if (color && value != null) {
                            return value;
                        } else {
                            TryGetAddition(LastFinishedBill)?.TriggerRandom();
                        }
                        return fromAdd(TryGetAddition(LastFinishedBill));
                    }
                }
            }
            return default;
        }

        public static Color? ColorFor(BillAddition add) 
            => add?.ActiveColor;

        public static ThingStyleDef StyleFor(BillAddition add) 
            => add?.TargetStyle;

        public static bool StyleActiveFor(BillAddition add) 
            => add?.styleActive ?? false;

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

        private static int saveFormat = 0;

        public static void ExposeData(SaveState save) {
            if (Scribe.mode != LoadSaveMode.Saving && Scribe.EnterNode(Strings.ID) && saveFormat < 2) {
                Scribe_Collections.Look(ref dict, "additions", LookMode.Reference, LookMode.Deep, ref save.addsKeyWorkList, ref save.addsValueWorkList);
                Scribe_Collections.Look(ref savedColors, "savedColors", LookMode.Value, null);
                Scribe.ExitNode();
                saveFormat = 1;
            } else {
                Scribe_Collections.Look(ref savedColors, "savedColors", LookMode.Value, null);
                saveFormat = 2;
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

            if (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving) {
                saveFormat = 0;
            }
        }

        public static void ExposeData(Bill_Production bill) {
            if (!dict.TryGetValue(bill, out var add) && Scribe.mode != LoadSaveMode.LoadingVars) {
                return;
            }

            if (Scribe.EnterNode(Strings.ID)) {
                if (Scribe.mode == LoadSaveMode.LoadingVars) {
                    add = dict[bill] = new BillAddition(bill);
                }
                add.ExposeData();
                Scribe.ExitNode();
            }
        }
    }

    public class SaveState : GameComponent {
        internal List<Bill> addsKeyWorkList;
        internal List<BillAddition> addsValueWorkList;

        public SaveState(Game _) { }

        public override void ExposeData() {
            State.ExposeData(this);
        }
    }

}

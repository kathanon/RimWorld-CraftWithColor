using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CraftWithColor {
    public class BillAddition : IExposable, ITargetColor {
        public enum RandomType { None, Any, Ideo, Favorite, Saved, Standard }

        private static readonly Func<BillAddition,Color?>[] randomFuncs = new Func<BillAddition,Color?>[] {
            b => b?.targetColor ?? State.DefaultColor,

            b => UnityEngine.Random.ColorHSV(),

            b => Find.IdeoManager.IdeosInViewOrder
                .Select<Ideo,Color?>(p => p.ApparelColor)
                .RandomElementWithFallback(null),

            b => Find.CurrentMap.mapPawns.FreeColonists
                .Select(p => p.story.favoriteColor)
                .Where(c => c != null)
                .RandomElementWithFallback(null),

            b => State.SavedColors
                .Select<Color,Color?>(c => c)
                .RandomElementWithFallback(null),

            b => DefDatabase<ColorDef>.AllDefs
#if VERSION_1_3
                .Where(d => !d.hairOnly)
#else
                .Where(d => d.colorType != ColorType.Hair)
#endif
                .Select<ColorDef,Color?>(d => d.color)
                .RandomElementWithFallback(null),
        };

        private static readonly string[] randomTip = new string[] {
            null,
            Strings.RandomAny,
            Strings.RandomIdeo,
            Strings.RandomFavo,
            Strings.RandomSaved,
            Strings.RandomStd,
        };

        private RecipeDef coloredRecipie;
        private RecipeDef originalRecipe;
        private Bill_Production bill;
        private Color targetColor;
        private RandomType random = RandomType.None;
        private StyleSetting targetStyle;
        private ThingStyleDef saveStyleDef;
        private int saveStyleId;

        private List<StyleSetting> availableStyles;
        private bool colorable;

        public bool colorActive = false;
        public bool styleActive = false;

        public Color TargetColor {
            get => targetColor;
            set {
                if (random != RandomType.None || !value.IndistinguishableFrom(targetColor)) {
                    targetColor = value;
                    TriggerRecolor();
                }
                random = RandomType.None;
            }
        }

        public RandomType RandomColorType {
            get => random;
            set { 
                random = value;
                TriggerRandomColor();
            }
        }

        public string RandomColorTip => randomTip[(int) random];

        public bool HasRandomColor => random != RandomType.None;

        public bool HasRandomStyle => targetStyle is RandomStyle;

        public static readonly RandomColorAccessor RandomColor = new RandomColorAccessor();

        public class RandomColorAccessor {
            public Color? this[RandomType t]     => For(t, null);

            public Color? this[BillAddition add] => For(add.random, add);

            private Color? For(RandomType t, BillAddition add) => 
                randomFuncs[(int) t].Invoke(add);
        }

        public Color? ActiveColor => colorActive ? (Color?) TargetColor : null;

        public Color MenuColor => colorActive ? TargetColor : Color.white;

        public IEnumerable<StyleSetting> Styles 
            => availableStyles;

        public ThingStyleDef TargetStyle {
            get => targetStyle.Style;
        }

        public StyleSetting StyleSelection {
            get => targetStyle;
            set {
                targetStyle = value;
                value.TriggerRandom();
            }
        }

        public bool CanColor => colorable;

        public bool CanStyle => availableStyles.Skip(2).Any() && MySettings.SetStyle;

        public bool Update => true;

        public ThingDef Thing => bill.recipe.ProducedThingDef;

        public override string ToString() => 
            $"{originalRecipe.defName}: color = {colorActive}/{targetColor}, style = {styleActive}/{targetStyle}";

        public BillAddition() { }

        public BillAddition(Bill_Production bill) {
            ThingDef thing = bill.recipe.ProducedThingDef;
            this.bill       = bill;
            originalRecipe  = bill.recipe;
            targetColor     = State.DefaultColor;
            colorable       = thing.HasComp(typeof(CompColorable));
            availableStyles = StylesFor(thing);
            targetStyle     = availableStyles[0];
        }

        public BillAddition(Bill_Production bill, BillAddition copyFrom) : this(bill) {
            CopyFrom(copyFrom);
            originalRecipe = copyFrom.originalRecipe;
        }

        public void CopyFrom(BillAddition copyFrom) {
            targetColor     = copyFrom.targetColor;
            colorActive     = copyFrom.colorActive;
            random          = copyFrom.random;
            availableStyles = StylesFor(copyFrom.originalRecipe.ProducedThingDef);
            targetStyle     = CopyStyle(copyFrom.targetStyle);
        }

        private List<StyleSetting> StylesFor(ThingDef thing) {
            var list = new List<StyleSetting>();
            list.Add(new BasicStyle(this, 0));
            list.AddRange(State.StylesFor(thing).Select(s => new ExplicitStyle(this, 1, s)));
            list.Add(new RandomStyle(this, 2, thing));
            return list;
        }

        public RecipeDef ColoredRecipie {
            get {
                if (coloredRecipie == null) {
                    coloredRecipie = CreateColoredRecipie(OriginalRecipe);
                    AddToRecipieMap();
                }
                return coloredRecipie;
            }
        }

        public RecipeDef Recipie => (colorActive && MySettings.RequireDye) ? ColoredRecipie : OriginalRecipe;

        public RecipeDef OriginalRecipe => originalRecipe;

        public void TriggerRandomColor() => 
            targetColor = RandomColor[this] ?? RandomColor[RandomType.Any] ?? Color.white;

        public void TriggerRandom() {
            TriggerRandomColor();
            targetStyle.TriggerRandom();
        }

        public void TriggerRecolor() {
            if (colorActive && MySettings.SwitchColor && random == RandomType.None) {
                Pawn pawn = bill.billStack?.billGiver?.Map.mapPawns.FreeColonists.Find(p => p.CurJob.bill == bill);
                pawn?.RetriggerCurrentJob();
            }
        }

        public void UpdateBill() {
            bill.recipe = Recipie;
        }

        public void ResetBill() {
            bill.recipe = originalRecipe;
        }

        public void AddToRecipieMap() {
            State.AddToRecipieMap(coloredRecipie, originalRecipe);
        }

        public void RemoveFromRecipieMap() {
            State.RemoveFromRecipieMap(coloredRecipie);
        }

        private static RecipeDef CreateColoredRecipie(RecipeDef original) {
            RecipeDef res = CopyRecipeFields(original);
            res.generated = true;
            res.useIngredientsForColor = false;
            res.ingredients = AmendIngredients(original.ingredients);
            return res;
        }

        private static RecipeDef CopyRecipeFields(RecipeDef original) =>
            new RecipeDef {
                adjustedCount = original.adjustedCount,
                allowMixingIngredients = original.allowMixingIngredients,
                conceptLearned = original.conceptLearned,
                defaultIngredientFilter = original.defaultIngredientFilter,
                defName = original.defName,
                description = original.description,
                descriptionHyperlinks = original.descriptionHyperlinks,
                dontShowIfAnyIngredientMissing = original.dontShowIfAnyIngredientMissing,
                effectWorking = original.effectWorking,
                efficiencyStat = original.efficiencyStat,
                factionPrerequisiteTags = original.factionPrerequisiteTags,
                fileName = original.fileName,
                fixedIngredientFilter = original.fixedIngredientFilter,
                forceHiddenSpecialFilters = original.forceHiddenSpecialFilters,
                fromIdeoBuildingPreceptOnly = original.fromIdeoBuildingPreceptOnly,
                interruptIfIngredientIsRotting = original.interruptIfIngredientIsRotting,
                isViolation = original.isViolation,
                jobString = original.jobString,
                label = original.label,
                memePrerequisitesAny = original.memePrerequisitesAny,
                minPartCount = original.minPartCount,
                modContentPack = original.modContentPack,
                modExtensions = original.modExtensions,
                productHasIngredientStuff = original.productHasIngredientStuff,
                products = original.products,
                recipeUsers = original.recipeUsers,
                requiredGiverWorkType = original.requiredGiverWorkType,
                researchPrerequisite = original.researchPrerequisite,
                researchPrerequisites = original.researchPrerequisites,
                shortHash = original.shortHash,
                skillRequirements = original.skillRequirements,
                soundWorking = original.soundWorking,
                specialProducts = original.specialProducts,
                targetCountAdjustment = original.targetCountAdjustment,
                unfinishedThingDef = original.unfinishedThingDef,
                workerClass = original.workerClass,
                workerCounterClass = original.workerCounterClass,
                workAmount = original.workAmount,
                workSkill = original.workSkill,
                workSkillLearnFactor = original.workSkillLearnFactor,
                workSpeedStat = original.workSpeedStat,
                workTableEfficiencyStat = original.workTableEfficiencyStat,
                workTableSpeedStat = original.workTableSpeedStat
            };

        private static List<IngredientCount> AmendIngredients(List<IngredientCount> original) {
            var dye = new IngredientCount();
            dye.filter = new ThingFilter();
            dye.filter.SetAllow(ThingDefOf.Dye, true);
            dye.SetBaseCount(1);

            return new List<IngredientCount>(original) { dye };
        }

        public void ExposeData() {
            if (Scribe.mode == LoadSaveMode.Saving) {
                saveStyleId = targetStyle.Id;
                saveStyleDef = targetStyle.Style;
            }

            Scribe_Values.Look(ref colorActive,  "active",      false);
            Scribe_Values.Look(ref targetColor,  "color",       forceSave: true);
            Scribe_Values.Look(ref styleActive,  "styleActive", false);
            Scribe_Values.Look(ref random,       "random",      RandomType.None);
            Scribe_Values.Look(ref saveStyleId,  "styleType",   -1);
            Scribe_Defs.Look(ref saveStyleDef,   "style");
            Scribe_Defs.Look(ref originalRecipe, "recipie");
        }

        public void SetBillAfterLoad(Bill_Production bill) {
            if (this.bill == null) {
                this.bill = bill;
            }

            ThingDef thing = Thing;
            if (availableStyles == null) {
                availableStyles = StylesFor(thing);
            }
            targetStyle = StyleFromIdDef(saveStyleId, saveStyleDef);
            colorable = thing.HasComp(typeof(CompColorable));
        }

        private StyleSetting StyleFromIdDef(int id, ThingStyleDef def) 
            => availableStyles.Find(x => x.Match(id, def)) ?? availableStyles[0];

        private StyleSetting CopyStyle(StyleSetting style) 
            => StyleFromIdDef(style.Id, style.Style);

        public abstract class StyleSetting {
            protected readonly BillAddition add;
            private readonly int id;

            protected StyleSetting(BillAddition add, int id) {
                this.add = add;
                this.id = id;
            }

            public abstract ThingStyleDef Style { get; }

            public virtual FloatMenuOption MenuOption {
                get {
                    var opt = new FloatMenuOption(Label, Set, add.Thing) {
                        forceThingColor = add.ActiveColor,
                    };
                    UpdateMenuOption(opt);
                    return opt;
                }
            }

            public abstract string Label { get; }

            protected virtual void UpdateMenuOption(FloatMenuOption opt) {}

            internal virtual void TriggerRandom() {}

            public int Id => id;

            public virtual bool Match(int id, ThingStyleDef style)
                => this.id == id;

            protected bool MatchCompat(int id)
                => this.id == id || id < 0;

            protected void Set() 
                => add.StyleSelection = this;
        }

        public class ExplicitStyle : StyleSetting {
            private readonly ThingStyleDef style;

            public ExplicitStyle(BillAddition add, int id, ThingStyleDef style) : base(add, id)
                => this.style = style;

            public override ThingStyleDef Style => style;

            protected override void UpdateMenuOption(FloatMenuOption opt) 
                => opt.thingStyle = style;

            public override string Label 
                => style.Category?.LabelCap ?? style.overrideLabel?.CapitalizeFirst() ?? style.defName;

            public override bool Match(int id, ThingStyleDef style)
                => style == this.style && MatchCompat(id);
        }

        public class BasicStyle : StyleSetting {
            public BasicStyle(BillAddition add, int id) : base(add, id) {}

            public override ThingStyleDef Style => null;

#if !VERSION_1_3
            protected override void UpdateMenuOption(FloatMenuOption opt) 
                => opt.forceBasicStyle = true;
#endif

            public override string Label
                => Strings.BasicStyle;

            public override bool Match(int id, ThingStyleDef style)
                => style == null && MatchCompat(id);
        }

        public class RandomStyle : StyleSetting {
            private ThingStyleDef current;
            private readonly ThingDef thing;

            public RandomStyle(BillAddition add, int id, ThingDef thing) : base(add, id)
                => this.thing = thing;

            public override ThingStyleDef Style => current;

            public override FloatMenuOption MenuOption
                => new FloatMenuOption(Label, Set, Textures.RandomMenu, add.MenuColor);

            public override string Label 
                => Strings.RandomStyle;

            internal override void TriggerRandom() 
                => current = State.StylesFor(thing).Prepend(null).RandomElement();
        }
    }

}

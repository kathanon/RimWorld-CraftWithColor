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
            .Where(d => !d.hairOnly)
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
        private ThingStyleDef targetStyle;

        private IEnumerable<ThingStyleDef> availableStyles;
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
                TriggerRandom();
            }
        }

        public string RandomColorTip => randomTip[(int) random];

        public bool HasRandomColor => random != RandomType.None;

        public static readonly RandomColorAccessor RandomColor = new RandomColorAccessor();

        public class RandomColorAccessor {
            public Color? this[RandomType t]     => For(t, null);

            public Color? this[BillAddition add] => For(add.random, add);

            private Color? For(RandomType t, BillAddition add) => 
                randomFuncs[(int) t].Invoke(add);
        }

        public Color? ActiveColor => colorActive ? (Color?) TargetColor : null;

        public IEnumerable<ThingStyleDef> Styles => availableStyles;

        public ThingStyleDef TargetStyle {
            get => targetStyle;
            set {
                if (availableStyles.Contains(value)) {
                    targetStyle = value;
                }
            }
        }

        public bool UseStyle => styleActive && MySettings.SetStyle;

        public ThingStyleDef ActiveStyle => UseStyle ? targetStyle : null;

        public bool CanColor => colorable;

        public bool CanStyle => availableStyles.Any() && MySettings.SetStyle;

        public bool Update => true;

        public ThingDef Thing => bill.recipe.ProducedThingDef;

        public BillAddition() { }

        public BillAddition(Bill_Production bill) {
            ThingDef thing = bill.recipe.ProducedThingDef;
            this.bill       = bill;
            originalRecipe  = bill.recipe;
            targetColor     = State.DefaultColor;
            colorable       = thing.HasComp(typeof(CompColorable));
            availableStyles = State.StylesFor(thing);
            targetStyle     = null;
        }

        public BillAddition(Bill_Production bill, BillAddition copyFrom) {
            this.bill = bill;
            CopyFrom(copyFrom);
        }

        public void CopyFrom(BillAddition copyFrom) {
            targetColor     = copyFrom.targetColor;
            colorActive     = copyFrom.colorActive;
            targetStyle     = copyFrom.targetStyle;
            random          = copyFrom.random;
            availableStyles = State.StylesFor(copyFrom.originalRecipe.ProducedThingDef);
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

        public void TriggerRandom() => 
            targetColor = RandomColor[this] ?? RandomColor[RandomType.Any] ?? Color.white;

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
            Scribe_Values.Look(ref colorActive, "active", false);
            Scribe_Values.Look(ref targetColor, "color", forceSave: true);
            Scribe_Values.Look(ref styleActive, "styleActive", false);
            Scribe_Values.Look(ref random, "random", RandomType.None);
            Scribe_Defs.Look(ref targetStyle, "style");
            Scribe_Defs.Look(ref originalRecipe, "recipie");
        }

        public void SetBillAfterLoad(Bill_Production bill) {
            if (this.bill == null) {
                this.bill = bill;
            }

            ThingDef thing = Thing;
            if (availableStyles == null) {
                availableStyles = State.StylesFor(thing);
            }
            colorable = thing.HasComp(typeof(CompColorable));
        }
    }

}

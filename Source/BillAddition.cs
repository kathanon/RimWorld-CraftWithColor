﻿using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CraftWithColor {
    public class BillAddition : IExposable, ITargetColor {
        private RecipeDef coloredRecipie;
        private RecipeDef originalRecipe;
        private Color targetColor;
        private Bill_Production bill;
        private ThingStyleDef targetStyle;
        private IEnumerable<ThingStyleDef> availableStyles;
        private bool colorable;

        public bool colorActive = false;
        public bool styleActive = false;

        public Color TargetColor {
            get => targetColor;
            set {
                if (!value.IndistinguishableFrom(targetColor)) {
                    targetColor = value;
                    TriggerRecolor();
                }
            }
        }

        public Color? ActiveColor => colorActive ? (Color?) targetColor : null;

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
            originalRecipe  = copyFrom.originalRecipe;
            targetColor     = copyFrom.targetColor;
            coloredRecipie  = copyFrom.coloredRecipie;
            colorActive     = copyFrom.colorActive;
            availableStyles = copyFrom.availableStyles;
            targetStyle     = copyFrom.targetStyle;
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

        public void TriggerRecolor() {
            if (colorActive && MySettings.SwitchColor) {
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

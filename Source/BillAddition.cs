using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    internal class BillAddition : IExposable, ITargetColor
    {
        private RecipeDef coloredRecipie;
        private RecipeDef originalRecipe;
        private Color targetColor;
        
        public bool active = false;
        
        public Color TargetColor 
        { 
            get => targetColor; set => targetColor = value; 
        }

        public Color? ActiveColor { get => active ? (Color?) targetColor : null; }

        public bool Update { get => true; }

        public BillAddition() { }

        public BillAddition(RecipeDef originalRecipe)
        {
            this.originalRecipe = originalRecipe;
            targetColor = State.DefaultColor;
        }

        public RecipeDef ColoredRecipie
        {
            get
            {
                if (coloredRecipie == null)
                {
                    coloredRecipie = CreateColoredRecipie(OriginalRecipe);
                }
                return coloredRecipie;
            }
        }

        public RecipeDef Recipie => (active && MySettings.RequireDye) ? ColoredRecipie : OriginalRecipe;

        public RecipeDef OriginalRecipe => originalRecipe;

        public void UpdateBill(Bill_Production bill)
        {
            bill.recipe = Recipie;
        }

        public void ResetBill(Bill_Production bill)
        {
            bill.recipe = OriginalRecipe;
        }

        private static RecipeDef CreateColoredRecipie(RecipeDef original)
        {
            RecipeDef res = CopyRecipeFields(original);
            res.generated = true;
            res.useIngredientsForColor = false;
            res.ingredients = AmendIngredients(original.ingredients);
            return res;
        }

        private static RecipeDef CopyRecipeFields(RecipeDef original) => 
            new RecipeDef
            {
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

        private static List<IngredientCount> AmendIngredients(List<IngredientCount> original)
        {
            var dye = new IngredientCount();
            dye.filter = new ThingFilter();
            dye.filter.SetAllow(ThingDefOf.Dye, true);
            dye.SetBaseCount(1);

            return new List<IngredientCount>(original) { dye };
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref active, "active", false);
            Scribe_Values.Look(ref targetColor, "color", forceSave: true);
            Scribe_Defs.Look(ref originalRecipe, "recipie");
        }
    }

}

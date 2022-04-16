using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CraftWithColor
{
    internal class BillAddition
    {
        private Color targetColor = Color.white;
        private RecipeDef coloredRecipie;

        public bool active = false;
        public readonly RecipeDef OriginalRecipe;

        public BillAddition(RecipeDef recipe)
        {
            this.OriginalRecipe = recipe;
        }

        public RecipeDef ColoredRecipie
        {
            get
            {
                if (coloredRecipie == null)
                {
                    coloredRecipie = CreateColoredRecipie(OriginalRecipe);
                    State.RecipieCreated(this);
                }
                return coloredRecipie;
            }
        }

        public RecipeDef Recipie => active ? ColoredRecipie : OriginalRecipe; 


        public Color TargetColor { 
            get => targetColor;
            set
            {
                targetColor = value;
            }
        }

        private static RecipeDef CreateColoredRecipie(RecipeDef original)
        {
            RecipeDef res = CopyRecipeFields(original);
            res.generated = true;
            res.defName = original.defName + "_kathanon_CraftWithColor_colored";
            res.shortHash = (ushort)(original.shortHash ^ 841);
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
            dye.SetBaseCount(1);

            var ingredients = new List<IngredientCount>(original);
            ingredients.Add(dye);
            return ingredients;
        }

        private static List<ThingDefCountClass> ReplaceProducts(List<ThingDefCountClass> original)
        {
            var replaced = new List<ThingDefCountClass>(original.Count);
            foreach (var product in original)
            {
                ThingDef thingDef = product.thingDef;
                
                replaced.Add(new ThingDefCountClass(thingDef, product.count));
            }
            throw new NotImplementedException();
        }

        private static ThingDef ReplaceUnfinished(ThingDef def)
        {
            throw new NotImplementedException();
        }
    }

}

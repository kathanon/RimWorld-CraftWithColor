using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CraftWithColor 
{
    [HarmonyPatch(typeof(Toils_Recipe), nameof(Toils_Recipe.MakeUnfinishedThingIfNeeded))]
    public static class ToilsRecipe_MakeUnfinishedThingIfNeeded_Detour
    {
        public static Toil Postfix(Toil __result)
        {
            Toil toil = __result;
            Action original = toil.initAction;
            if (original != null)
            {
                toil.initAction = delegate
                {
                    original();
                    Job curJob = toil.actor.jobs.curJob;
                    Color? color = State.ColorFor(curJob.bill);
                    if (color.HasValue)
                    {
                        Thing thing = curJob.GetTarget(TargetIndex.B).Thing;
                        if (thing is UnfinishedThing)
                        {
                            thing.TryGetComp<CompColorable>()?.SetColor(color.Value);
                        }
                    }
                };
            }
            return toil;
        }
    }

    [HarmonyPatch(typeof(Toils_Recipe), nameof(Toils_Recipe.FinishRecipeAndStartStoringProduct))]
    public static class ToilsRecipe_FinishRecipeAndStartStoringProduct_Detour
    {
        public static Toil Postfix(Toil __result)
        {
            Toil toil = __result;
            Action original = toil.initAction;
            if (original != null)
            {
                toil.initAction = delegate
                {
                    Job curJob = toil.actor.jobs.curJob;
                    Bill_Production bill = curJob.bill as Bill_Production;
                    State.LastFinishedBill = bill;
                    original();
                    State.UnsetLastFinishedBillIf(bill);
                };
            }
            return toil;
        }
    }
}

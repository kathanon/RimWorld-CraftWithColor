using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CraftWithColor 
{
    [HarmonyPatch(typeof(Toils_Recipe), nameof(Toils_Recipe.MakeUnfinishedThingIfNeeded))]
    public static class ToilsRecipe_MakeUnfinishedThingIfNeeded_Patch {
        public static Toil Postfix(Toil __result)
        {
            Toil toil = __result;
            Action original = toil.initAction;
            if (original != null)
            {
                toil.initAction = delegate
                {
                    Job job = toil.actor.jobs.curJob;
                    var add = State.TryGetAddition(job.bill);
                    Color? color = add?.ActiveColor;
                    bool createUnfinished = job.GetTarget(TargetIndex.B).Thing as UnfinishedThing == null;
                    bool updateColor = !createUnfinished && NeedsUpdate(job, color);
                    bool setColor = (createUnfinished && color != null) || updateColor;
                    if (updateColor && MySettings.SwitchUseDye)
                    {
                        // TODO: Add steps: take dye if available, otherwise skip switch
                    }
                    original();
                    if (setColor)
                    {
                        CompColorable comp = ToilsUtil.GetColorable(job);
                        if (comp != null) {
                            comp.SetColor(color.Value);
                            if (createUnfinished) add?.TriggerRandom();
                        }
                    }
                };
            }
            return toil;
        }

        private static bool NeedsUpdate(Job job, Color? color)
        {
            if (color == null || !MySettings.SwitchColor) return false;
            var colorable = ToilsUtil.GetColorable(job);
            if (colorable == null) return false;
            return !colorable.Color.IndistinguishableFrom(color.Value);
        }
    }

    [HarmonyPatch(typeof(Toils_Recipe), nameof(Toils_Recipe.FinishRecipeAndStartStoringProduct))]
    public static class ToilsRecipe_FinishRecipeAndStartStoringProduct_Patch {
        public static Toil Postfix(Toil __result)
        {
            Toil toil = __result;
            Action original = toil?.initAction;
            if (original != null)
            {
                toil.initAction = delegate
                {
                    Job curJob = toil?.actor?.jobs?.curJob;
                    Bill_Production bill = curJob?.bill as Bill_Production;
                    State.LastFinishedBill = bill;
                    State.LastFinishedBillColor = ToilsUtil.GetColorable(curJob)?.ActiveColor();
                    original();
                    State.UnsetLastFinishedBillIf(bill);
                };
            }
            return toil;
        }
    }

    static class ToilsUtil
    {
        public static CompColorable GetColorable(Job job) =>
            (job.GetTarget(TargetIndex.B).Thing as UnfinishedThing)?.TryGetComp<CompColorable>();

        public static Color? ActiveColor(this CompColorable col) => col.Active ? (Color?) col.Color : null;
    }
}

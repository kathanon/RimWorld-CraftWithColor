using UnityEngine;
using Verse;
using RimWorld;
using Verse.AI;

namespace CraftWithColor
{
    internal static class ExtensionMethods
    {
        public static Rect[] SliceHorizontal(this Rect rect, int n, float gap = 0f)
        {
            float height = (rect.height - gap * (n - 1)) / n;
            Rect[] result = new Rect[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = rect;
                result[i].height = height;
                result[i].y = rect.y + i * (height + gap);
            }
            return result;
        }

        public static Rect[] SliceVertical(this Rect rect, int n, float gap = 0f)
        {
            float width = (rect.width - gap * (n - 1)) / n;
            Rect[] result = new Rect[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = rect;
                result[i].width = width;
                result[i].x = rect.x + i * (width + gap);
            }
            return result;
        }

        public static void RetriggerCurrentJob(this Pawn pawn)
        {
            Pawn_JobTracker jobs = pawn.jobs;
            if (jobs != null)
            {
                JobQueue queue = jobs.CaptureAndClearJobQueue();
                Job job = JobMaker.MakeJob(JobDefOf.Goto, pawn.Position);
                jobs.TryTakeOrderedJob(job);
                jobs.RestoreCapturedJobs(queue);
            }
        }

        public static void SetStyle(this CompStyleable comp, ThingStyleDef style) => comp.styleDef = style;
    }
}


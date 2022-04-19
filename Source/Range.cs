using System;
using System.Collections.Generic;
using UnityEngine;

namespace CraftWithColor
{
    public struct Range
    {
        public float start;
        public float end;

        public Range(float start, float length)
        {
            this.start = start;
            this.end = start + length;
            if (length < 0) throw new ArgumentException($"Called new Range({start}, {length}): length must be >= 0.");
        }

        public float Length 
        {
            get => end - start; 
            set => end = start + value; 
        }

        public void Expand(float l)
        {
            start -= l;
            end += l;
        }

        public void Contract(float l) => Expand(-l);

        public Range Union(Range r) => new Range { start = Mathf.Min(start, r.start), end = Mathf.Max(end, r.end) };

        public bool Intersects(Range r) => (start <= r.end && end >= r.end) || (start <= r.start && end >= r.start);

        public override string ToString() => $"({start}, {end})";

        public static bool operator <(Range a, Range b) => a.end < b.start;
        public static bool operator >(Range a, Range b) => a.start > b.end;
    }

    public class MultiRange
    {
        private readonly List<Range> list = new List<Range>();

        public void Merge(Range range)
        {
            bool merged = false;
            for (int i = 0; !merged && i < list.Count; i++)
            {
                if (range.Intersects(list[i]))
                {
                    list[i] = list[i].Union(range);
                    merged = true;
                    if (i < list.Count - 1 && list[i].Intersects(list[i + 1])) {
                        list[i] = list[i].Union(list[i + 1]);
                        list.RemoveAt(i + 1);
                    } 
                }
            }
            if (!merged)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (range < list[i])
                    {
                        list.Insert(i, range);
                        return;
                    }
                }
                list.Add(range);
            }
        }

        public Range FindClosestFreeRange(Range preferred, Range limit)
        {
            int last = list.Count - 1;
            if (list.Count == 0 || list[0] > preferred || list[last] < preferred)
            {
                return preferred;
            }

            float length = preferred.Length;
            float distance = float.MaxValue;
            float position = float.MinValue;
            bool valid = false;

            if (limit.start + length <= list[0].start)
            {
                position = list[0].start - length;
                valid = true;
                distance = Mathf.Abs(preferred.start - position);
            }

            for (int i = 0; i < last; i++)
            {
                if (list[i] < preferred && list[i + 1] > preferred)
                {
                    return preferred;
                }
                if (list[i + 1].start - list[i].end >= length)
                {
                    float pos1 = list[i].end;
                    float pos2 = list[i + 1].start - length;
                    float dist1 = Mathf.Abs(preferred.start - pos1);
                    float dist2 = Mathf.Abs(preferred.start - pos2);
                    if (dist2 < dist1)
                    {
                        pos1 = pos2;
                        dist1 = dist2;
                    }
                    if (pos1 + length < limit.end && dist1 < distance)
                    {
                        position = pos1;
                        distance = dist1;
                        valid = true;
                    }
                }
            }

            float distEnd = Mathf.Abs(preferred.start - list[last].end);
            if (list[last].end < limit.end && distEnd < distance)
            {
                position = list[last].end;
                valid = true;
            }

            if (valid)
            {
                return new Range(position, position + length);
            }
            else
            {
                throw new RangeLimitException("No valid range found within limit.");
            }
        }
    }

    public class RangeLimitException : Exception
    {
        public RangeLimitException(string message) : base(message) { }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;

namespace CraftWithColor
{
    public static partial class GenRecipe_MakeRecipeProducts_Patch
    {
        public class EnumerableWithActionOnNext<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> parent;
            private readonly Action<T> action;

            public EnumerableWithActionOnNext(IEnumerable<T> parent, Action<T> action)
            {
                this.parent = parent;
                this.action = action;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enum(parent.GetEnumerator(), action);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            private class Enum : IEnumerator<T>
            {
                private readonly IEnumerator<T> parent;
                private readonly Action<T> action;

                public Enum(IEnumerator<T> parent, Action<T> action)
                {
                    this.parent = parent;
                    this.action = action;
                }

                public bool MoveNext()
                {
                    bool res = parent.MoveNext();
                    action(parent.Current);
                    return res;
                }

                public T Current => parent.Current;

                object IEnumerator.Current => throw new System.NotImplementedException();

                public void Dispose() => parent.Dispose();

                public void Reset() => parent.Reset();
            }
        }
    }
}

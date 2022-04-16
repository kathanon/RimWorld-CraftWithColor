using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CraftWithColor
{
    internal class State
    {
        private static BillAddition add = new BillAddition();

        internal static BillAddition GetAddition(Bill_Production bill)
        {
            return add;
        }
    }

    internal class BillAddition
    {
        public bool active = false;
        public Color color = Color.white;
    }

}

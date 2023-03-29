using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SpectatorGUI
{
    public static class Extensions
    {
        private static void BreakdownComponentsInternal(this GameObject obj, int lvl = 0)
        {
            string space = "";
            for (int i = 0; i < lvl; i++)
            {
                space += "\t";
            }

            Console.WriteLine(space + obj.name + "...");
            foreach (var comp in obj.GetComponents<Component>())
            {
                Console.WriteLine(space + "    -" + comp.GetType());
            }

            foreach (var child in obj.GetComponentsInChildren<Transform>())
            {
                if (child != obj.transform)
                    child.gameObject.BreakdownComponentsInternal(lvl + 1);
            }
        }

        public static void BreakdownComponents(this GameObject obj)
        {
            BreakdownComponentsInternal(obj, 0);
        }
    }
}

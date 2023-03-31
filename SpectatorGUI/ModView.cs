using ComputerInterface;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using System;
using UnityEngine;

namespace SpectatorGUI
{
    public class ModEntry : IComputerModEntry
    {
        public string EntryName => "Spectator GUI";
        public Type EntryViewType => typeof(ModView);
    }

    public class ModView : ComputerView
    {
        string template = "Use the arrow keys to anchor\n" +
            "the stats text to a \n" +
            "different side of the screen\n\n" +
            "Current anchor: {0}";

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Text = string.Format(template, anchor);
        }

        int x = 0, y = 0;
        TextAnchor anchor = TextAnchor.LowerLeft;
        public override void OnKeyPressed(EKeyboardKey key)
        {
            TextAnchor? anchor = null;
            switch (key)
            {
                case EKeyboardKey.Delete:
                    ReturnToMainMenu();
                    break;
                case EKeyboardKey.Left:
                    anchor = SpectatorGUI.Instance?.SetStatTextPosition(0);
                    break;
                case EKeyboardKey.Right:
                    anchor = SpectatorGUI.Instance?.SetStatTextPosition(1);
                    break;
                case EKeyboardKey.Down:
                    anchor = SpectatorGUI.Instance?.SetStatTextPosition(2);
                    break;
                case EKeyboardKey.Up:
                    anchor = SpectatorGUI.Instance?.SetStatTextPosition(3);
                    break;
            }
            if (anchor.HasValue)
            {
                this.anchor = anchor.Value;
                Text = string.Format(template, this.anchor);
            }
        }
    }
}

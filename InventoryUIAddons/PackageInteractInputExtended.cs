using InControl;
using Pathea.InputSolutionNs;
using System;

namespace InventoryUIAddons
{
    public class PackageInteractInputExtended : PackageInteractInput
    {
        public PackageInteractInputExtended() : base()
        {
            this.packageSort.AddDefaultBinding(InputControlType.LeftStickButton);
            this.packageInteract.AddDefaultBinding(InputControlType.RightStickButton);
            this.packageEquip.AddDefaultBinding(InputControlType.Action1);
            this.packageSplitMouse.AddDefaultBinding(new Key[] { Key.LeftShift });
            this.action4.AddDefaultBinding(InputControlType.Action4);
            this.packageSplitOne = base.CreatePlayerAction("packageSplitOne");
            this.packageSplitOne.AddDefaultBinding(new Key[] { (Key)Enum.Parse(typeof(Key), Main.settings.GrabOneModKey) });
            this.packageSplitHalf = base.CreatePlayerAction("packageSplitHalf");
            this.packageSplitHalf.AddDefaultBinding(new Key[] { (Key)Enum.Parse(typeof(Key), Main.settings.GrabHalfModKey) });
        }
        public PlayerAction packageSplitOne;
        public PlayerAction packageSplitHalf;

    }
}
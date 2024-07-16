using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonBuilder.modules.window_objects
{
    public class RoomDataMenu
    {
        public NumericUpDown? RoomID { get; set; }

        public Button? AddRoom { get; set; }
        public Button? RemoveRoom { get; set; }
        public Grid? RoomTiles { get; set; }

        public NumericUpDown? SizeX { get; set; }
        public NumericUpDown? SizeY { get; set; }
        public ToggleButton? HasDoor { get; set; }
        public ToggleButton? IsExit { get; set; }
        public Grid? RoomOutline { get; set; }

    }
}

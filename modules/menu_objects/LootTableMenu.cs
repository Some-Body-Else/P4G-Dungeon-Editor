using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonBuilder.modules.menu_objects
{
    public class LootTableMenu
    {
        public class LootTableEntry
        {
            public MaskedTextBox ItemWeight { get; set; }
            public MaskedTextBox ItemID { get; set; }
            public MaskedTextBox ChestFlags { get; set; }
            public ToggleButton ChestModel { get; set; }
        }
        public NumericUpDown LootID;
        public List<LootTableEntry> Entries;
    }
}

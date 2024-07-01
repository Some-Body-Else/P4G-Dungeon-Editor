using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonBuilder.modules.menu_objects
{
    public class FloorMenu
    {
        public NumericUpDown FloorID { get; set; }
        public MaskedTextBox FloorName { get; set; }
        public MaskedTextBox FieldID { get; set; }
        public MaskedTextBox RoomID { get; set; }
        public MaskedTextBox MinTileCount { get; set; }
        public MaskedTextBox MaxTileCount { get; set; }
        public MaskedTextBox DungeonScriptID { get; set; }
        public MaskedTextBox EnvID { get; set; }
        public MaskedTextBox Byte04 { get; set; }
        public MaskedTextBox Byte0A { get; set; }
        public MaskedTextBox EncounterTableID { get; set; }
        public MaskedTextBox LootTableID { get; set; }
        public MaskedTextBox MaxChestCount { get; set; }
        public MaskedTextBox InitialEnemyCount { get; set; }
        public MaskedTextBox MinEnemyCount { get; set; }
    }
}

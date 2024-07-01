using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;


namespace DungeonBuilder.modules.menu_objects
{
    public class EncounterMenu
    {
        public NumericUpDown EncounterID { get; set; }

        public MaskedTextBox[] Unit = new MaskedTextBox[5];
        public MaskedTextBox Flags { get; set; }
        public MaskedTextBox Field04 { get; set; }
        public MaskedTextBox Field06 { get; set; }
        public MaskedTextBox FieldID { get; set; }
        public MaskedTextBox MusicID { get; set; }
        public MaskedTextBox RoomID { get; set; }
    }
}

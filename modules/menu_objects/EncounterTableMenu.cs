using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonBuilder.modules.menu_objects
{
    public class EncounterTableMenu
    {
        public MaskedTextBox Byte00 { get; set; }
        public MaskedTextBox Byte03 { get; set; }
        public MaskedTextBox Byte06 { get; set; }

        public MaskedTextBox[] Encounters = new MaskedTextBox[30];
    }
}

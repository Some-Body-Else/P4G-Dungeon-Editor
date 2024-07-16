using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonBuilder.modules.window_objects
{
    public class EncounterTableMenu
    {
        public NumericUpDown EncountTable { get; set; }
        public MaskedTextBox NormalWeight { get; set; }
        public MaskedTextBox RareWeight { get; set; }
        public MaskedTextBox GoldWeight { get; set; }

        public MaskedTextBox AlwaysFF { get; set; }
        public MaskedTextBox RarePercent { get; set; }
        public MaskedTextBox GoldPercent { get; set; }

        public MaskedTextBox[] NormalEncounters = new MaskedTextBox[20];
        public MaskedTextBox[] RareEncounters = new MaskedTextBox[5];
        public MaskedTextBox[] GoldEncounters = new MaskedTextBox[5];

        public MaskedTextBox[] NormalEncounterWeights = new MaskedTextBox[20];
        public MaskedTextBox[] RareEncounterWeights = new MaskedTextBox[5];
        public MaskedTextBox[] GoldEncounterWeights = new MaskedTextBox[5];

        public ToggleButton IsRainy { get; set; }
    }
}

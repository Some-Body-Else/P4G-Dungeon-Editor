using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonBuilder.modules.menu_objects
{
    public class TemplateMenu
    {
        public NumericUpDown TemplateID;
        public List<Panel> RegularRooms = new();
        public List<Panel> ExitRooms = new();
        public NumericUpDown FieldSelected;
        public WrapPanel FieldsThatUse;
        
    }
}

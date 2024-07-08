using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonBuilder.modules.internal_objects
{
    public class DungeonTemplates
    {
        public int roomCount { get; set; }
        public int roomExCount { get; set; }
        public int exitNum { get; set; }
        public List<byte> rooms { get; set; }
    }
}

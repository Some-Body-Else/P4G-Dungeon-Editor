using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4gpc.modules.internal_objects
{
    
    public class DungeonFloor
    {
        public ushort ID { get; set; }
        public ushort subID { get; set; }
        public uint Byte04 { get; set; }
        public byte tileCountMin { get; set; }
        public byte tileCountMax { get; set; }
        public ushort Byte0A { get; set; }
        public byte dungeonScript{ get; set; }
        public byte usedEnv{ get; set; }
        public string? floorName { get; set; }
        // public nuint nameAddress { get; set; }
    }
}

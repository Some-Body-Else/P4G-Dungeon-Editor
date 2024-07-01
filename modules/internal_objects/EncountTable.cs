using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace DungeonBuilder.modules.internal_objects
{
    public abstract class EncountTable
    {
        UInt32 _size;
        public EncountTable()
        {

        }
        public EncountTable(UInt32 size)
        {
            _size = size;
        }

        public abstract bool LoadFrom(byte[] data, int offset);

        public abstract int GetSize();
    }
    public class EnemyEncountTable : EncountTable
    {
        // size in bytes
        private static int _entrySize = 24;
        public class EnemyEncountEntry
        {
            public UInt32 _flags;
            public UInt16 _field04;
            public UInt16 _field06;
            // Array instead of a list due to hard cap on enemies per encounter
            public UInt16[] _units = new UInt16[5];

            // 0 if encounter is not a boss encounter
            public UInt16 _fieldID;
            // 0 if encounter is not a boss encounter
            public UInt16 _roomID;
            // 0 is Time To Make History
            public UInt16 _musicID;

        }
        public List<EnemyEncountEntry> _enemyEncountEntries;
        public override bool LoadFrom(byte[] data, int offset)
        {
            _enemyEncountEntries = new List<EnemyEncountEntry>();
            Int32 size = data[offset + 0];
            size += (data[offset + 1] << 8);
            size += (data[offset + 2] << 16);
            size += (data[offset + 3] << 24);
            
            int entryCount = size / _entrySize;
            if (entryCount * _entrySize != size)
            {
                return false;
            }

            UInt32 curr32;
            UInt16 curr16;
            for (int i = 0; i < entryCount; i++)
            {
                EnemyEncountEntry en = new();
                curr32 = data[offset + i*_entrySize + 4];
                curr32 += (UInt32)(data[offset + i*_entrySize + 5] << 8);
                curr32 += (UInt32)(data[offset + i*_entrySize + 6] << 16);
                curr32 += (UInt32)(data[offset + i*_entrySize + 7] << 24);

                en._flags = curr32;

                curr16 = data[offset + i*_entrySize + 8];
                curr16 += (UInt16)(data[offset + i*_entrySize + 9] << 8);

                en._field04 = curr16;


                curr16 = data[offset + i*_entrySize + 10];
                curr16 += (UInt16)(data[offset + i*_entrySize + 11] << 8);

                en._field06 = curr16;


                curr16 = data[offset + i*_entrySize + 12];
                curr16 += (UInt16)(data[offset + i*_entrySize + 13] << 8);

                en._units[0] = curr16;


                curr16 = data[offset + i*_entrySize + 14];
                curr16 += (UInt16)(data[offset + i*_entrySize + 15] << 8);

                en._units[1] = curr16;


                curr16 = data[offset + i*_entrySize + 16];
                curr16 += (UInt16)(data[offset + i*_entrySize + 17] << 8);

                en._units[2] = curr16;


                curr16 = data[offset + i*_entrySize + 18];
                curr16 += (UInt16)(data[offset + i*_entrySize + 19] << 8);

                en._units[3] = curr16;


                curr16 = data[offset + i*_entrySize + 20];
                curr16 += (UInt16)(data[offset + i*_entrySize + 21] << 8);

                en._units[4] = curr16;

                curr16 = data[offset + i*_entrySize + 22];
                curr16 += (UInt16)(data[offset + i*_entrySize + 23] << 8);

                en._fieldID = curr16;


                curr16 = data[offset + i*_entrySize + 24];
                curr16 += (UInt16)(data[offset + i*_entrySize + 25] << 8);

                en._roomID = curr16;


                curr16 = data[offset + i*_entrySize + 26];
                curr16 += (UInt16)(data[offset + i*_entrySize + 27] << 8);

                en._musicID = curr16;

                _enemyEncountEntries.Add(en);
            }

            return true;
        }

        public override int GetSize()
        {
            return _enemyEncountEntries.Count*_entrySize;
        }
    }
    public class FloorObjectTable : EncountTable
    {
        private static int _entrySize = 10;
        public class FloorObjectEntry
        {
            public UInt16 _EncountTableLookup;
            // Minimum number of encounters on a floor at any given time
            public byte _MinEncounterCount;
            // The amount of encounters the floor intitially generates with
            public byte _InitialEncounterCount;
            public byte _MaxChestCount;
            // Internally, this appears to be a 4-byte value, but likely just padding
            public uint _LootTableLookup;
        }
        public List<FloorObjectEntry> _floorObjectEntries;
        public override bool LoadFrom(byte[] data, int offset)
        {
            _floorObjectEntries = new();
            Int32 size = data[offset + 0];
            size += (data[offset + 1] << 8);
            size += (data[offset + 2] << 16);
            size += (data[offset + 3] << 24);

            int entryCount = size / _entrySize;
            if (entryCount * _entrySize != size)
            {
                return false;
            }

            UInt32 curr32;
            UInt16 curr16;
            for (int i = 0; i < entryCount; i++)
            {
                FloorObjectEntry obj = new();
                curr16 = data[offset + i*_entrySize + 4];
                curr16 += (UInt16)(data[offset + i*_entrySize + 5] << 8);

                obj._EncountTableLookup = curr16;

                obj._MinEncounterCount = data[offset + i*_entrySize + 6];

                obj._InitialEncounterCount = data[offset + i*_entrySize + 7];

                obj._MaxChestCount = data[offset + i*_entrySize + 8];

                obj._LootTableLookup =  data[offset + i*_entrySize + 10];

                _floorObjectEntries.Add(obj);
            }

            return true;
        }
        public override int GetSize()
        {
            return _floorObjectEntries.Count*_entrySize;
        }
    }
    public class FloorEncountTable : EncountTable
    {
        private static int _entrySize = 252;
        public class FloorEncountEntry
        {

            // Unsure what these value are, need a proper name for them later. 
            // Only known property is that they are summed together under certain conditions
            // and that together they equal 100 for all entries except entry 0, which could be unused
            public byte _u1;
            public byte _u2;
            public byte _u3;

            public List<UInt16> _encounters = new List<UInt16>(30);
        }
        public List<FloorEncountEntry> _floorEncounterEntries;
        public override bool LoadFrom(byte[] data, int offset)
        {
            _floorEncounterEntries = new();
            Int32 size = data[offset + 0];
            size += (data[offset + 1] << 8);
            size += (data[offset + 2] << 16);
            size += (data[offset + 3] << 24);

            int entryCount = size / _entrySize;
            if (entryCount * _entrySize != size)
            {
                return false;
            }

            UInt32 curr32;
            UInt16 curr16;
            for (int i = 0; i < entryCount; i++)
            {
                FloorEncountEntry en = new();
                en._u1 = data[offset + i*_entrySize + 4];
                en._u2 = data[offset + i*_entrySize + 7];
                en._u3 = data[offset + i*_entrySize + 10];
                for (int j = 0; j < 30; j++)
                {
                    curr16 = data[offset + i*_entrySize + 138 + j*4];
                    curr16 += (UInt16)(data[offset + i*_entrySize + 139 + j*4] << 8);
                    en._encounters.Add( curr16 );
                }
                _floorEncounterEntries.Add( en );
            }
            return true;
        }
        public override int GetSize()
        {
            return _floorEncounterEntries.Count*_entrySize;
        }
    }
    public class ChestLootTable : EncountTable
    {
        private static int _entrySize = 348;

        public class LootEntry
        {
            // Something to do with how likely an item is to generate
            public UInt16 _itemWeight;
            // Item contained in the chest
            public UInt16 _itemID;

            // 1 if locked, any other value to indicate opened, but may have more than one purpose
            // *Thinking enemies popping out of chest, need more testing*
            public UInt16 _chestFlags;

            // Single byte containing the value 0x01, indicates chest to be valid

            // 0 for small red chest, 1 for big gold chest
            public byte _chestModel;
        }
        public class ChestLootEntry
        {
            public List<LootEntry> _lootEntries;
        }
        public List<ChestLootEntry> _chestLootEntries;
        public override bool LoadFrom(byte[] data, int offset)
        {
            _chestLootEntries = new();
            Int32 size = data[offset + 0];
            size += (data[offset + 1] << 8);
            size += (data[offset + 2] << 16);
            size += (data[offset + 3] << 24);

            int entryCount = size / _entrySize;
            if (entryCount * _entrySize != size)
            {
                return false;
            }

            UInt32 curr32;
            UInt16 curr16;
            for (int i = 0; i < entryCount; i++)
            {
                ChestLootEntry en = new();
                en._lootEntries = new List<LootEntry>();
                for (int j = 0; j < 29; j++)
                {
                    LootEntry loot = new();
                    curr16 = data[offset + i*_entrySize + j*12 + 4];
                    curr16 += (UInt16)(data[offset + i*_entrySize + j*12 + 5] << 8);
                    loot._itemWeight = curr16;

                    curr16 = data[offset + i*_entrySize + j*12 + 6];
                    curr16 += (UInt16)(data[offset + i*_entrySize + j*12 +7] << 8);
                    loot._itemID = curr16;


                    curr16 = data[offset + i*_entrySize + j*12 + 8];
                    curr16 += (UInt16)(data[offset + i*_entrySize + j*12 + 9] << 8);
                    loot._chestFlags = curr16;

                    loot._chestModel = data[offset + i*_entrySize +j*12 + 11];
                    en._lootEntries.Add(loot);
                }

                _chestLootEntries.Add(en);
            }

            return true;
        }
        public override int GetSize()
        {
            return _chestLootEntries.Count*_entrySize;
        }
    }
}

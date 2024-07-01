using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System;

using DungeonBuilder.modules.internal_objects;
using System.IO;
using DungeonBuilder.modules.menu_objects;

using System.Text.Json;
using Avalonia.Controls.Primitives;
using p4gpc.modules.internal_objects;
using System.Collections.Generic;
using Avalonia.VisualTree;
using System.Linq;
using Avalonia.Input;

using System.Text;
using System.Net.Http.Json;
using System.Diagnostics.Metrics;
using Avalonia;
using static DungeonBuilder.modules.menu_objects.LootTableMenu;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Reflection;


namespace DungeonBuilder
{

    public class MapTile : Panel
    {
        public byte value = 0;
    }

    public partial class MainWindow : Window
    {
        EncounterMenu _encounterMenu;
        EncounterTableMenu _encounterTableMenu;
        LootTableMenu _lootTableMenu;
        RoomDataMenu _roomDataMenu;
        FloorMenu _floorMenu;
        TemplateMenu _templateMenu;

        EnemyEncountTable _EnemyEncountTable;
        FloorObjectTable _FloorObjectTable;
        FloorEncountTable _FloorEncountTable;
        ChestLootTable _ChestLootTable;
        List<DungeonTemplates> _templates;
        List<DungeonFloor> _floors;
        List<DungeonRoom> _rooms;
        List<DungeonMinimap> _minimap;
        Dictionary<byte, byte> _dungeon_template_dict;

        bool project_loaded = false;
        int lastEncountID;
        int lastEncountTableID;
        int lastLootID;
        int lastRoomDataID;
        int lastFloorID;
        int lastTemplateID;

        string current_project_path;
        

        public static FilePickerFileType ENCOUNT_TBL { get; } = new("ENCOUNT_TBL")
        {
            Patterns = ["*.TBL"],
            // Figure out these two for ENCOUNT.TBL, currently unknown
            AppleUniformTypeIdentifiers = ["public.image"],
            MimeTypes = ["image/*"]
        };

        public MainWindow()
        {
            InitializeComponent();

            AddHandler(DragDrop.DropEvent, Drop);
            // When stylizing, use these to indicate which block stuff is being dragged into,
            // remove if we decide against that
            AddHandler(DragDrop.DragEnterEvent, DragEnter);
            AddHandler(DragDrop.DragLeaveEvent, DragLeave);

        }
        private async void HandleProjectMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender == NewProjectButton)
            {
                // Setup a crapton of internal classes to keep everything in check here
                // Need all of the default data in place
                var file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select ENCOUNT.TBL",
                    AllowMultiple = false,
                    FileTypeFilter = [ENCOUNT_TBL]
                });
                if (file.Count >= 1)
                {
                    lastEncountID = 0;
                    lastEncountTableID = 0;
                    lastLootID = 0;
                    lastRoomDataID = 0;
                    lastFloorID = 0;
                    lastTemplateID = 0;

                    _encounterMenu = new();
                    _encounterTableMenu = new();
                    _lootTableMenu = new();
                    // Room model menu here, when we get to it
                    _roomDataMenu = new();
                    _floorMenu = new();
                    _templateMenu = new();
                    _dungeon_template_dict = new();

                    byte[] encount_tbl;
                    // Open reading stream from the first file.
                    await using var stream = await file[0].OpenReadAsync();
                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        encount_tbl = br.ReadBytes((int)stream.Length);
                    }


                    // Going to want more flexible offsets at a later point, but for now this works fine
                    _EnemyEncountTable = new EnemyEncountTable();
                    _EnemyEncountTable.LoadFrom(encount_tbl, 0);

                    _FloorObjectTable = new FloorObjectTable();
                    _FloorObjectTable.LoadFrom(encount_tbl, 0x5890);

                    _FloorEncountTable = new FloorEncountTable();
                    _FloorEncountTable.LoadFrom(encount_tbl, 0x6070);

                    _ChestLootTable = new ChestLootTable();
                    _ChestLootTable.LoadFrom(encount_tbl, 0xA370);

                    StreamReader jsonStream = new StreamReader("JSON\\dungeon_floors.json");
                    string jsonContents = jsonStream.ReadToEnd();
                    _floors = JsonSerializer.Deserialize<List<DungeonFloor>>(jsonContents)!;


                    jsonStream = new StreamReader("JSON\\dungeon_rooms.json");
                    jsonContents = jsonStream.ReadToEnd();
                    _rooms = JsonSerializer.Deserialize<List<DungeonRoom>>(jsonContents)!;

                    jsonStream = new StreamReader("JSON\\dungeon_minimap.json");
                    jsonContents = jsonStream.ReadToEnd();
                    _minimap = JsonSerializer.Deserialize<List<DungeonMinimap>>(jsonContents)!;

                    jsonStream = new StreamReader("JSON\\dungeon_templates.json");
                    jsonContents = jsonStream.ReadToEnd();
                    _templates = JsonSerializer.Deserialize<List<DungeonTemplates>>(jsonContents)!;

                    Dictionary<string, byte> temp;
                    jsonStream = new StreamReader("JSON\\dungeon_template_dict.json");
                    jsonContents = jsonStream.ReadToEnd();
                    temp = JsonSerializer.Deserialize<Dictionary<string, byte>>(jsonContents)!;
                    foreach (string key in temp.Keys)
                    {
                        _dungeon_template_dict.Add(Byte.Parse(key), temp[key]);
                    }
                    project_loaded = true;

                    SaveProjectButton.IsEnabled = true;
                }
                // Currently, Room Model button is marked as false, change that once we begin to tackle that side of things
            }
            else if (sender == LoadProjectButton)
            {
                var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select folder to save project to",
                    AllowMultiple=false
                });
                if (folder.Count > 0)
                {
                    var current_project_path = folder[0].Path;

                    lastEncountID = 0;
                    lastEncountTableID = 0;
                    lastLootID = 0;
                    lastRoomDataID = 0;
                    lastFloorID = 0;
                    lastTemplateID = 0;

                    _encounterMenu = new();
                    _encounterTableMenu = new();
                    _lootTableMenu = new();
                    // Room model menu here, when we get to it
                    _roomDataMenu = new();
                    _floorMenu = new();
                    _templateMenu = new();
                    _dungeon_template_dict = new();

                    byte[] encount_tbl;
                    // Open reading stream from the first file.
                    var stream = File.OpenRead(Path.Combine(current_project_path.LocalPath.ToString(), "ENCOUNT.TBL"));
                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        encount_tbl = br.ReadBytes((int)stream.Length);
                    }


                    // Going to want more flexible offsets at a later point, but for now this works fine
                    _EnemyEncountTable = new EnemyEncountTable();
                    _EnemyEncountTable.LoadFrom(encount_tbl, 0);

                    _FloorObjectTable = new FloorObjectTable();
                    _FloorObjectTable.LoadFrom(encount_tbl, 0x5890);

                    _FloorEncountTable = new FloorEncountTable();
                    _FloorEncountTable.LoadFrom(encount_tbl, 0x6070);

                    _ChestLootTable = new ChestLootTable();
                    _ChestLootTable.LoadFrom(encount_tbl, 0xA370);

                    StreamReader jsonStream = new StreamReader(Path.Combine(current_project_path.LocalPath.ToString(), "dungeon_floors.json"));
                    string jsonContents = jsonStream.ReadToEnd();
                    _floors = JsonSerializer.Deserialize<List<DungeonFloor>>(jsonContents)!;


                    jsonStream = new StreamReader(Path.Combine(current_project_path.LocalPath.ToString(), "dungeon_rooms.json"));
                    jsonContents = jsonStream.ReadToEnd();
                    _rooms = JsonSerializer.Deserialize<List<DungeonRoom>>(jsonContents)!;

                    jsonStream = new StreamReader(Path.Combine(current_project_path.LocalPath.ToString(), "dungeon_minimap.json"));
                    jsonContents = jsonStream.ReadToEnd();
                    _minimap = JsonSerializer.Deserialize<List<DungeonMinimap>>(jsonContents)!;

                    jsonStream = new StreamReader(Path.Combine(current_project_path.LocalPath.ToString(), "dungeon_templates.json"));
                    jsonContents = jsonStream.ReadToEnd();
                    _templates = JsonSerializer.Deserialize<List<DungeonTemplates>>(jsonContents)!;

                    Dictionary<string, byte> temp;
                    jsonStream = new StreamReader(Path.Combine(current_project_path.LocalPath.ToString(), "dungeon_template_dict.json"));
                    jsonContents = jsonStream.ReadToEnd();
                    temp = JsonSerializer.Deserialize<Dictionary<string, byte>>(jsonContents)!;
                    foreach (string key in temp.Keys)
                    {
                        _dungeon_template_dict.Add(Byte.Parse(key), temp[key]);
                    }
                    project_loaded = true;

                    SaveProjectButton.IsEnabled = true;
                }
            }
            else if (sender == SaveProjectButton)
            {
                var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select folder to save project to",
                    AllowMultiple=false
                });
                if (folder.Count > 0)
                {
                    var current_project_path = folder[0].Path;
                    var serializeOptions = new JsonSerializerOptions { WriteIndented = true };

                    string jsonContents = JsonSerializer.Serialize(_floors, serializeOptions);
                    var writer = File.Create(Path.Combine(current_project_path.LocalPath.ToString(), "dungeon_floors.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();


                    jsonContents = JsonSerializer.Serialize(_dungeon_template_dict, serializeOptions);
                    writer = File.Create(Path.Combine(current_project_path.LocalPath.ToString(), "dungeon_template_dict.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();

                    foreach (DungeonRoom room in _rooms)
                    {
                        sbyte offset_x = 0;
                        sbyte offset_y = 0;
                        for (int i = 0; i < _rooms[lastRoomDataID].mapRamOutline.Count; i++)
                        {
                            for (int j = 0; j < _rooms[lastRoomDataID].mapRamOutline[i].Count; j++)
                            {
                                if (_rooms[lastRoomDataID].mapRamOutline[i][j] == 1)
                                {
                                    if (-i > offset_y)
                                    {

                                        offset_y = (sbyte)-i;
                                    }
                                    if (-j > offset_x)
                                    {
                                        offset_x = (sbyte)-j;
                                    }
                                }
                            }
                        }
                        _rooms[lastRoomDataID].x_y_offsets[0] = offset_x;
                        _rooms[lastRoomDataID].x_y_offsets[0] = offset_y;
                    }

                    jsonContents = JsonSerializer.Serialize(_rooms, serializeOptions);
                    writer = File.Create(Path.Combine(current_project_path.LocalPath.ToString(), "dungeon_rooms.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();

                    jsonContents = JsonSerializer.Serialize(_templates, serializeOptions);
                    writer = File.Create(Path.Combine(current_project_path.LocalPath.ToString(), "dungeon_templates.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();

                    jsonContents = JsonSerializer.Serialize(_minimap, serializeOptions);
                    writer = File.Create(Path.Combine(current_project_path.LocalPath.ToString(), "dungeon_minimap.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();


                    writer = File.Create(Path.Combine(current_project_path.LocalPath.ToString(), "ENCOUNT.TBL"));
                    WriteEncountTbl(writer);

                    writer.Close();
                }

            }
        }

        private void WriteEncountTbl(FileStream? writer)
        {
            var binWriter = new BinaryWriter(writer);
            int overall_size = 0;
            int size = _EnemyEncountTable.GetSize();
            binWriter.Write(size);
            foreach (var encounter in _EnemyEncountTable._enemyEncountEntries)
            {
                binWriter.Write(encounter._flags);
                binWriter.Write(encounter._field04);
                binWriter.Write(encounter._field06);
                foreach (UInt16 unit in encounter._units)
                {
                    binWriter.Write(unit);
                }
                binWriter.Write(encounter._fieldID);
                binWriter.Write(encounter._roomID);
                binWriter.Write(encounter._musicID);
            }

            overall_size += size+4;
            if ((overall_size) % 16 != 0)
            {
                for (int i = ((overall_size) % 16); i < 16; i++)
                {
                    binWriter.Write((byte)0);
                    overall_size++;
                }
            }

            size = _FloorObjectTable.GetSize();
            binWriter.Write(size);
            foreach (var floor_entry in _FloorObjectTable._floorObjectEntries)
            {
                binWriter.Write(floor_entry._EncountTableLookup);
                binWriter.Write(floor_entry._MinEncounterCount);
                binWriter.Write(floor_entry._InitialEncounterCount);
                binWriter.Write(floor_entry._MaxChestCount);
                binWriter.Write((byte)0);
                binWriter.Write(floor_entry._LootTableLookup);
            }

            overall_size += size+4;
            if ((overall_size) % 16 != 0)
            {
                for (int i = ((overall_size) % 16); i < 16; i++)
                {
                    binWriter.Write((byte)0);
                    overall_size++;
                }
            }
            /*
             Since we're currently writing a new ENCOUNT.TBL instead of a json surrogate, we have to account
             for a blank table that exists between the actually useful ones. It's 200 bytes large and is
             filled entirely with 0s.
             */
            size = 200;
            binWriter.Write(size);
            for (int i = 0; i < 200/4; i++)
            {
                binWriter.Write((int)0);
            }

            overall_size += size+4;
            if ((overall_size) % 16 != 0)
            {
                for (int i = ((overall_size) % 16); i < 16; i++)
                {
                    binWriter.Write((byte)0);
                    overall_size++;
                }
            }

            size = _FloorEncountTable.GetSize();
            binWriter.Write(size);
            foreach (var floor_encounter in _FloorEncountTable._floorEncounterEntries)
            {
                binWriter.Write(floor_encounter._u1);
                // Two other values follow, but they appear unused, so just duplicating this one
                binWriter.Write(floor_encounter._u1);
                binWriter.Write(floor_encounter._u1);


                binWriter.Write(floor_encounter._u2);
                // Two other values follow, but they appear unused, so just duplicating this one
                binWriter.Write(floor_encounter._u2);
                binWriter.Write(floor_encounter._u2);

                binWriter.Write(floor_encounter._u3);
                // Two other values follow, but they appear unused, so just duplicating this one
                binWriter.Write(floor_encounter._u3);
                binWriter.Write(floor_encounter._u3);

                // Three more bytes follow, appearing to always be 0
                binWriter.Write( (byte)0 );
                binWriter.Write( (byte)0 );
                binWriter.Write( (byte)0 );
            
                for (int i = 0; i < 2; i++)
                {
                    // Doubling up because there are two versions of each table presented, even though
                    // only one of them appears to be in use
                    foreach (var encounter in floor_encounter._encounters)
                    {
                        // Two values here, but the only thing that appears to matter for the first is
                        // being nonzero, at least from my observations. We'll find out one way or another
                        if (encounter != 0)
                        {
                            binWriter.Write((UInt16)1);
                        }
                        else
                        {
                            binWriter.Write((UInt16)0);
                        }
                        binWriter.Write((UInt16)encounter);

                    }
                }
            }

            overall_size += size+4;
            if ((overall_size) % 16 != 0)
            {
                for (int i = ((overall_size) % 16); i < 16; i++)
                {
                    binWriter.Write((byte)0);
                    overall_size++;
                }
            }

            size = _ChestLootTable.GetSize();
            binWriter.Write(size);
            foreach (var loot_entry in _ChestLootTable._chestLootEntries)
            {
                foreach (var entry in loot_entry._lootEntries)
                {
                    binWriter.Write(entry._itemWeight);
                    binWriter.Write(entry._itemID);
                    binWriter.Write(entry._chestFlags);
                    if (entry._itemID != 0)
                    {
                        binWriter.Write((byte)1);
                    }
                    else
                    {
                        binWriter.Write((byte)0);
                    }
                    binWriter.Write(entry._chestModel);
                    binWriter.Write((int)0);
                }
            }

            overall_size += size+4;
            if ((overall_size) % 16 != 0)
            {
                for (int i = ((overall_size) % 16); i < 16; i++)
                {
                    binWriter.Write((byte)0);
                    overall_size++;
                }
            }

            binWriter.Close();
        }

        private void HandleAreaRadioButtonClick(object sender, RoutedEventArgs e)
        {
            if (!project_loaded)
            {
                return;
            }

            ActiveGrid.Children.Clear();
            ActiveGrid.RowDefinitions.Clear();
            ActiveGrid.ColumnDefinitions.Clear();

            Avalonia.Media.BrushConverter BrushColor = new();
            TextBlock Identifier;
            MaskedTextBox TextBoxInput;
            Border ItemOutline;
            DockPanel ItemContainer;
            Grid InnerGrid;
            Grid InnerGrid2;

            if (sender == EncounterButton)
            {
                StackPanel PairContents;
                MaskedTextBox WritableTextbox;
                TextBlock PlainText;
                
                ActiveGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(5, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.SetValue(Grid.BackgroundProperty, BrushColor.ConvertFrom("#3330a0"));



                // https://docs.avaloniaui.net/docs/reference/controls/numericupdown
                NumericUpDown EncountSelector = new();
                ItemOutline = new();
                EncountSelector.SetValue(NumericUpDown.NameProperty, "EncounterID");
                EncountSelector.SetValue(NumericUpDown.ValueProperty, lastEncountID);
                EncountSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                EncountSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                EncountSelector.SetValue(NumericUpDown.MaximumProperty, _EnemyEncountTable._enemyEncountEntries.Count-1);
                EncountSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);

                EncountSelector.ValueChanged+=ChangeActiveEncounter;
                _encounterMenu.EncounterID = EncountSelector;
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.Child = EncountSelector;
                ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#150F80") );
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#5f5f0f") );
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10") );
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8") );
                ActiveGrid.Children.Add(ItemOutline);

                InnerGrid = new();
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(2, GridUnitType.Star));

                InnerGrid.SetValue(Grid.RowProperty, 1);
                InnerGrid.SetValue(Grid.ColumnProperty, 0);
                InnerGrid.SetValue(Grid.ColumnSpanProperty, 2);
                InnerGrid.SetValue(Grid.RowProperty, 2);
                ActiveGrid.Children.Add(InnerGrid);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#150F80"));
                ItemOutline.SetValue(Border.MarginProperty, Thickness.Parse("5 0 5 0"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                InnerGrid2 = new();
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                ItemOutline.Child = (InnerGrid2);


                for (int i = 0; i < 5; i++)
                {

                    ItemOutline = new();
                    ItemOutline.SetValue(Grid.RowProperty, i);
                    ItemOutline.SetValue( Border.BorderThicknessProperty, Thickness.Parse("2"));
                    ItemOutline.SetValue( Border.MarginProperty, Thickness.Parse("2"));
                    InnerGrid2.Children.Add(ItemOutline);

                        ItemContainer = new();
                        ItemOutline.Child = ItemContainer;

                            ItemOutline = new();
                            ItemOutline.SetValue(Border.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#990046"));
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3"));
                            ItemContainer.Children.Add(ItemOutline);

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, BrushColor.ConvertFrom("#BF0045"));
                                Identifier.SetValue(TextBlock.TextProperty, "Unit "+(i+1));
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 64);
                                ItemOutline.Child = Identifier;

                            WritableTextbox = new();
                            WritableTextbox.SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[i].ToString());
                            WritableTextbox.SetValue(MaskedTextBox.CornerRadiusProperty, CornerRadius.Parse("0"));
                            WritableTextbox.TextChanged += ChangeEncounterData;
                            _encounterMenu.Unit[i] = WritableTextbox;
                            ItemContainer.Children.Add(WritableTextbox);
                }

                InnerGrid2 = new();
                InnerGrid2.SetValue(Grid.RowProperty, 1);
                InnerGrid2.SetValue(Grid.ColumnProperty, 1);
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(3, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));

                InnerGrid.Children.Add(InnerGrid2);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#150F80"));
                ItemOutline.SetValue(Border.MarginProperty, Thickness.Parse("5 0 5 0"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid2.Children.Add(ItemOutline);

                InnerGrid2 = new();
                InnerGrid2.SetValue(Grid.RowProperty, 1);
                InnerGrid2.SetValue(Grid.ColumnProperty, 1);
                InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                ItemOutline.Child = InnerGrid2;
                
                    ItemOutline = new();
                    ItemOutline.SetValue(Grid.RowProperty, 0);
                    ItemOutline.SetValue(Grid.ColumnProperty, 0);
                    ItemOutline.SetValue(Border.MarginProperty, Thickness.Parse("10 10 5 0"));
                    ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("2"));
                    ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                    InnerGrid2.Children.Add(ItemOutline);

                        ItemContainer = new();
                        ItemOutline.Child = ItemContainer;

                            ItemOutline = new();
                            ItemOutline.SetValue(Border.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#990046"));
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, BrushColor.ConvertFrom("#BF0045"));
                                Identifier.SetValue(TextBlock.TextProperty, "Flags");
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72);
                                ItemOutline.Child = Identifier;


                            ItemContainer.Children.Add(ItemOutline);

                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._flags.ToString());
                            TextBoxInput.TextChanged += ChangeEncounterData;
                            _encounterMenu.Flags = TextBoxInput;
                            ItemContainer.Children.Add(TextBoxInput);

                    ItemOutline = new();
                    ItemOutline.SetValue(Grid.RowProperty, 0);
                    ItemOutline.SetValue(Grid.ColumnProperty, 1);
                    ItemOutline.SetValue(Border.MarginProperty, Thickness.Parse("5 10 10 0"));
                    ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("2"));
                    ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                    InnerGrid2.Children.Add(ItemOutline);

                        ItemContainer = new();
                        ItemOutline.Child = ItemContainer;

                            ItemOutline = new();
                            ItemOutline.SetValue(Border.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#990046"));
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, BrushColor.ConvertFrom("#BF0045"));
                                Identifier.SetValue(TextBlock.TextProperty, "Music ID");
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72);
                                ItemOutline.Child = Identifier;

                            ItemContainer.Children.Add(ItemOutline);

                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._musicID.ToString());
                            TextBoxInput.TextChanged += ChangeEncounterData;
                            _encounterMenu.MusicID = TextBoxInput;
                            ItemContainer.Children.Add(TextBoxInput);

                    ItemOutline = new();
                    ItemOutline.SetValue(Grid.RowProperty, 1);
                    ItemOutline.SetValue(Grid.ColumnProperty, 0);
                    ItemOutline.SetValue(Border.MarginProperty, Thickness.Parse("10 10 5 5"));
                    ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("2"));
                    ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                    InnerGrid2.Children.Add(ItemOutline);

                        ItemContainer = new();
                        ItemOutline.Child = ItemContainer;

                            ItemOutline = new();
                            ItemOutline.SetValue(Border.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#990046"));
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, BrushColor.ConvertFrom("#BF0045"));
                                Identifier.SetValue(TextBlock.TextProperty, "Field ID");
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72);
                                ItemOutline.Child = Identifier;


                            ItemContainer.Children.Add(ItemOutline);

                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._fieldID.ToString());
                            TextBoxInput.TextChanged += ChangeEncounterData;
                            _encounterMenu.FieldID = TextBoxInput;
                            ItemContainer.Children.Add(TextBoxInput);

                    ItemOutline = new();
                    ItemOutline.SetValue(Grid.RowProperty, 1);
                    ItemOutline.SetValue(Grid.ColumnProperty, 1);
                    ItemOutline.SetValue(Border.MarginProperty, Thickness.Parse("5 5 10 5"));
                    ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("2"));
                    ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                    InnerGrid2.Children.Add(ItemOutline);

                        ItemContainer = new();
                        ItemOutline.Child = ItemContainer;

                            ItemOutline = new();
                            ItemOutline.SetValue(Border.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#990046"));
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );
                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, BrushColor.ConvertFrom("#BF0045"));
                                Identifier.SetValue(TextBlock.TextProperty, "Room ID");
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72);
                                ItemOutline.Child = Identifier;
                
                
                            ItemContainer.Children.Add(ItemOutline);

                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._roomID.ToString());
                            TextBoxInput.TextChanged += ChangeEncounterData;
                            _encounterMenu.RoomID = TextBoxInput;
                            ItemContainer.Children.Add(TextBoxInput);

                    ItemOutline = new();
                    ItemOutline.SetValue(Grid.RowProperty, 2);
                    ItemOutline.SetValue(Grid.ColumnProperty, 0);
                    ItemOutline.SetValue(Border.MarginProperty, Thickness.Parse("10 5 5 10"));
                    ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("2"));
                    ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                    InnerGrid2.Children.Add(ItemOutline);

                        ItemContainer = new();
                        ItemOutline.Child = ItemContainer;

                            ItemOutline = new();
                            ItemOutline.SetValue(DockPanel.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#990046"));
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, BrushColor.ConvertFrom("#BF0045"));
                                Identifier.SetValue(TextBlock.TextProperty, "Field04");
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72);
                                ItemOutline.Child = Identifier;
                
                            ItemContainer.Children.Add(ItemOutline);
                

                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._field04.ToString());
                            TextBoxInput.TextChanged += ChangeEncounterData;
                            _encounterMenu.Field04 = TextBoxInput;
                            ItemContainer.Children.Add(TextBoxInput);

                    ItemOutline = new();
                    ItemOutline.SetValue(Grid.RowProperty, 2);
                    ItemOutline.SetValue(Grid.ColumnProperty, 1);
                    ItemOutline.SetValue(Border.MarginProperty, Thickness.Parse("5 5 10 10"));
                    ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("2"));
                    ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                    InnerGrid2.Children.Add(ItemOutline);

                        ItemContainer = new();
                        ItemOutline.Child = ItemContainer;

                            ItemOutline = new();
                            ItemOutline.SetValue(DockPanel.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#990046"));
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, BrushColor.ConvertFrom("#BF0045"));
                                Identifier.SetValue(TextBlock.TextProperty, "Field06");

                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72); 
                                ItemOutline.Child = Identifier;
                
                
                            ItemContainer.Children.Add(ItemOutline);
                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._field06.ToString());
                            TextBoxInput.TextChanged += ChangeEncounterData;
                            _encounterMenu.Field06 = TextBoxInput;
                            ItemContainer.Children.Add(TextBoxInput);

            }
            else if (sender == EncounterTableButton)
            {

                StackPanel PairContents;
                MaskedTextBox WritableTextbox;
                TextBlock PlainText;
                ActiveGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(5, GridUnitType.Star));
                ActiveGrid.SetValue(Grid.BackgroundProperty, BrushColor.ConvertFrom("#65379E"));

                InnerGrid = new();
                InnerGrid.SetValue(Grid.RowProperty, 0);
                InnerGrid.SetValue(Grid.ColumnProperty, 1);
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));

                ItemOutline = new();
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#320368"));
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#5f5f0f"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                    // https://docs.avaloniaui.net/docs/reference/controls/numericupdown
                    NumericUpDown EncountSelector = new();
                EncountSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                EncountSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                EncountSelector.SetValue(NumericUpDown.MaximumProperty, _FloorEncountTable._floorEncounterEntries.Count-1);
                EncountSelector.SetValue(NumericUpDown.ValueProperty, lastEncountTableID);
                EncountSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);
                ItemOutline.Child = EncountSelector;
                EncountSelector.ValueChanged += ChangeActiveEncounterTable;
                InnerGrid.Children.Add(ItemOutline);
                ActiveGrid.Children.Add(InnerGrid);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#320368"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                ActiveGrid.Children.Add(ItemOutline);

                InnerGrid = new();
                ItemOutline.Child = InnerGrid;

                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(5, GridUnitType.Star));

                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#5212AA"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                PairContents = new();
                ItemOutline.Child = PairContents;


                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                                        WritableTextbox = new();
                        Identifier = new();
                        Identifier.SetValue(TextBlock.TextProperty, "Byte00");
                        Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                        PairContents.Children.Add(Identifier);


                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.TextProperty, _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._u1.ToString());

                WritableTextbox.TextChanged += ChangeEncounterTableData;
                    _encounterTableMenu.Byte00 = WritableTextbox;
                    PairContents.Children.Add(WritableTextbox);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 2);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#5212AA"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                PairContents = new();
                ItemOutline.Child = PairContents;


                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                        WritableTextbox = new();
                        Identifier = new();
                        Identifier.SetValue(TextBlock.TextProperty, "Byte03");
                        Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                        PairContents.Children.Add(Identifier);

                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.TextProperty, _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._u2.ToString());
                        WritableTextbox.TextChanged += ChangeEncounterTableData;
                        _encounterTableMenu.Byte03 = WritableTextbox;
                        PairContents.Children.Add(WritableTextbox);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 4);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#5212AA"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                PairContents = new();
                ItemOutline.Child = PairContents;
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);

                        WritableTextbox = new();
                        Identifier = new();
                Identifier.SetValue(TextBlock.TextProperty, "Byte06");
                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                PairContents.Children.Add(Identifier);

                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.TextProperty, _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._u3.ToString());

                WritableTextbox.TextChanged += ChangeEncounterTableData;
                        _encounterTableMenu.Byte06 = WritableTextbox;
                        PairContents.Children.Add(WritableTextbox);
                ScrollViewer EncountContainer = new();
                EncountContainer.SetValue(Grid.RowProperty, 1);
                EncountContainer.SetValue(Grid.ColumnProperty, 0);
                EncountContainer.SetValue(Grid.ColumnSpanProperty, 5);
                InnerGrid.Children.Add(EncountContainer);

                StackPanel EncountContents = new();
                EncountContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                EncountContainer.SetValue(ContentProperty, EncountContents);

                int counter = 1;
                for (int i = 1; i <= 6; i++)
                {
                    InnerGrid2= new();
                    InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    if (i % 2 == 0)
                    {
                        InnerGrid2.SetValue(Grid.BackgroundProperty, BrushColor.ConvertFrom("#5212AA"));
                    }
                    else
                    {
                        InnerGrid2.SetValue(Grid.BackgroundProperty, BrushColor.ConvertFrom("#6728BF"));
                    }
                    InnerGrid2.SetValue(MarginProperty, Thickness.Parse("0 5"));
                    for (int j = 0; j < 5; j++)
                    {

                        PairContents = new();
                        PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                        PairContents.SetValue(Grid.RowProperty, i);
                        PairContents.SetValue(Grid.ColumnProperty, j);
                        Identifier = new();
                        Identifier.SetValue(TextBlock.TextProperty, "Encounter "+(counter));
                        Identifier.SetValue(TextBlock.MarginProperty, Thickness.Parse("4, 0"));
                        Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                        Identifier.SetValue(TextBlock.FontStyleProperty, FontStyle.Oblique);
                        PairContents.Children.Add(Identifier);

                        WritableTextbox = new();
                        WritableTextbox.SetValue(TextBlock.TextProperty, _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._encounters[i].ToString());
                        WritableTextbox.SetValue(TextBlock.MarginProperty, Thickness.Parse("4, 0"));
                        WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                        WritableTextbox.TextChanged += ChangeEncounterTableData;
                        _encounterTableMenu.Encounters[counter-1] = WritableTextbox;
                        PairContents.Children.Add(WritableTextbox);

                        InnerGrid2.Children.Add(PairContents);
                        counter++;
                    }
                    EncountContents.Children.Add(InnerGrid2);
                }

            }
            else if (sender == LootTableButton)
            {
                TextBlock PlainText;
                MaskedTextBox WritableTextbox;
                ToggleButton ChestTypeToggle;
                ScrollViewer EntryShell = new();
                StackPanel ActiveEntries = new StackPanel();
                StackPanel PairControl;
                ActiveGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(5, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.SetValue(Grid.BackgroundProperty, BrushColor.ConvertFrom("#F99C03"));



                ItemOutline = new();
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#CC7D00"));
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#5f5f0f"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                // https://docs.avaloniaui.net/docs/reference/controls/numericupdown
                NumericUpDown LootSelector = new();
                LootSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                LootSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                LootSelector.SetValue(NumericUpDown.MaximumProperty, _ChestLootTable._chestLootEntries.Count-1);
                LootSelector.SetValue(NumericUpDown.ValueProperty, lastEncountTableID);
                LootSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);
                LootSelector.ValueChanged+=ChangeActiveLootTable;
                ItemOutline.Child = LootSelector;

                ActiveGrid.Children.Add(ItemOutline);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Grid.ColumnSpanProperty, 2);
                ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#CC7D00"));
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("3"));
                ItemOutline.Child = EntryShell;
                EntryShell.Content = ActiveEntries;
                ActiveEntries.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                ActiveGrid.Children.Add(ItemOutline);
                _lootTableMenu.Entries = new List<LootTableEntry>();
                for (int i = 0; i < _ChestLootTable._chestLootEntries[lastLootID]._lootEntries.Count;  i++)
                {
                    LootTableMenu.LootTableEntry entry = new();
                    InnerGrid = new();
                    InnerGrid.ColumnDefinitions.Add(new(2, GridUnitType.Star));
                    InnerGrid.ColumnDefinitions.Add(new(2, GridUnitType.Star));
                    InnerGrid.ColumnDefinitions.Add(new(2, GridUnitType.Star));
                    InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    if (i % 2 == 0)
                    {
                        InnerGrid.SetValue(Grid.BackgroundProperty, BrushColor.ConvertFrom("#19CDCA"));
                    }
                    else
                    {
                        InnerGrid.SetValue(Grid.BackgroundProperty, BrushColor.ConvertFrom("#C50000"));
                    }


                    PairControl = new();
                    WritableTextbox = new();
                    PlainText = new();

                    PairControl.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                    PairControl.SetValue(StackPanel.MarginProperty, Thickness.Parse("2 3 2 3"));
                    PairControl.SetValue(Grid.ColumnProperty, 0);

                    PlainText.Text = "Item Weight";
                    PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                    PlainText.SetValue(TextBlock.MarginProperty, Thickness.Parse("0 3 0 0"));
                    PairControl.Children.Add(PlainText);

                    InnerGrid.Children.Add(PairControl);

                    WritableTextbox.Text = _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._itemWeight.ToString();
                    WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                    WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("0 0 0 3"));
                    WritableTextbox.TextChanged += ChangeLootTableData;
                    entry.ItemWeight = WritableTextbox;
                    PairControl.Children.Add(WritableTextbox);

                    PairControl = new();
                    WritableTextbox = new();
                    PlainText = new();

                    PairControl.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                    PairControl.SetValue(StackPanel.MarginProperty, Thickness.Parse("2 3 2 3"));
                    PairControl.SetValue(Grid.ColumnProperty, 1);

                    PlainText.Text = "Item ID";
                    PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                    PlainText.SetValue(TextBlock.MarginProperty, Thickness.Parse("0 3 0 0"));
                    PairControl.Children.Add(PlainText);


                    WritableTextbox.Text = _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._itemID.ToString();
                    WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                    WritableTextbox.SetValue(StackPanel.MarginProperty, Thickness.Parse("0 0 0 3"));
                    WritableTextbox.TextChanged += ChangeLootTableData;
                    entry.ItemID = WritableTextbox;
                    PairControl.Children.Add(WritableTextbox);


                    InnerGrid.Children.Add(PairControl);

                    PairControl = new();
                    WritableTextbox = new();
                    PlainText = new();

                    PairControl.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                    PairControl.SetValue(StackPanel.MarginProperty, Thickness.Parse("2 3 2 3"));
                    PairControl.SetValue(Grid.ColumnProperty, 2);


                    PlainText.Text = "Chest Flags";
                    PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                    PlainText.SetValue(TextBlock.MarginProperty, Thickness.Parse("0 3 0 0"));
                    PairControl.Children.Add(PlainText);

                    WritableTextbox.Text = _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._chestFlags.ToString();
                    WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                    WritableTextbox.SetValue(StackPanel.MarginProperty, Thickness.Parse("0 0 0 3"));
                    WritableTextbox.TextChanged += ChangeLootTableData;
                    entry.ChestFlags = WritableTextbox;
                    PairControl.Children.Add(WritableTextbox);

                    InnerGrid.Children.Add(PairControl);

                    PairControl = new();
                    ChestTypeToggle = new();
                    PlainText = new();

                    PairControl.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                    PairControl.SetValue(StackPanel.MarginProperty, Thickness.Parse("2 3 2 3"));
                    PairControl.SetValue(Grid.ColumnProperty, 3);

                    PlainText.Text = "Big Chest";
                    PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                    PlainText.SetValue(TextBlock.MarginProperty, Thickness.Parse("0 3 0 0"));
                    PairControl.Children.Add(PlainText);

                    // Might want to swap later
                    ChestTypeToggle.IsChecked = _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._chestModel > 0;
                    ChestTypeToggle.SetValue(ToggleButton.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Stretch);
                    ChestTypeToggle.SetValue(ToggleButton.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Center);
                    ChestTypeToggle.SetValue(ToggleButton.MarginProperty, Thickness.Parse("0 0 0 3"));
                    // ChestTypeToggle.SetValue(ToggleButton.NameProperty, "BigChest");
                    ChestTypeToggle.Click += ChangeLootTableData;
                    entry.ChestModel = ChestTypeToggle;
                    PairControl.Children.Add(ChestTypeToggle);


                    InnerGrid.Children.Add(PairControl);
                    ActiveEntries.Children.Add(InnerGrid);

                    _lootTableMenu.Entries.Add(entry);
                }
                
            }
            else if (sender == RoomModelButton)
            {
                // Handle with MUCH later
                ActiveGrid.Background = Brushes.PaleVioletRed;
            }
            else if (sender == RoomDataButton)
            {

                StackPanel PairContents;
                MaskedTextBox WritableTextbox;
                ToggleButton Toggle;
                Button AddSubRoom;
                TextBlock PlainText;
                ActiveGrid.ColumnDefinitions.Add(new(65, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(5, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(30, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(5, GridUnitType.Star));
                ActiveGrid.SetValue(Grid.BackgroundProperty, BrushColor.ConvertFrom("#005127"));



                InnerGrid = new();
                InnerGrid.SetValue(Grid.ColumnProperty, 0);
                InnerGrid.SetValue(Grid.RowProperty, 0);
                InnerGrid.SetValue(Grid.ColumnSpanProperty, 3);
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.Children.Add(InnerGrid);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.ColumnProperty, 2);
                ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#003D1C"));
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#5f5f0f"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                // https://docs.avaloniaui.net/docs/reference/controls/numericupdown
                NumericUpDown RoomSelector = new();
                RoomSelector.SetValue(NumericUpDown.ValueProperty, lastRoomDataID);
                RoomSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                RoomSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                RoomSelector.SetValue(NumericUpDown.MaximumProperty, _rooms.Count-1);
                RoomSelector.SetValue(Grid.RowProperty, 0);
                RoomSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);
                RoomSelector.ValueChanged+=ChangeActiveRoom;
                _roomDataMenu.RoomID = RoomSelector;
                ItemOutline.Child = RoomSelector;

                InnerGrid = new();
                InnerGrid.SetValue(Grid.RowProperty, 1);
                InnerGrid.SetValue(Grid.ColumnProperty, 2);
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(3, GridUnitType.Star));


                PairContents = new();
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Horizontal);
                PairContents.SetValue(Grid.RowProperty, 0);
                PlainText = new();
                AddSubRoom = new();
                PlainText.Text = "+";
                AddSubRoom.Content = PlainText;
                AddSubRoom.SetValue(NumericUpDown.NameProperty, "AddRoom");
                _roomDataMenu.AddRoom = AddSubRoom;
                AddSubRoom.Click+=AddRoom;
                AddSubRoom.SetValue(Grid.RowProperty, 0);
                AddSubRoom.SetValue(StackPanel.MarginProperty, Avalonia.Thickness.Parse("4"));
                AddSubRoom.SetValue(Button.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Stretch);
                AddSubRoom.SetValue(Button.HorizontalContentAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center);
                AddSubRoom.SetValue(Button.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Center);
                InnerGrid.Children.Add(AddSubRoom);

                string sizex = _rooms[lastRoomDataID].sizeX.ToString();
                string sizey = _rooms[lastRoomDataID].sizeY.ToString();

                StackPanel PairContents2 = new();
                PairContents2.SetValue(Grid.RowProperty, 1);
                PairContents2.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents = new();
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Horizontal);
                PlainText = new();
                PlainText.Text = "Size (X) ";
                PairContents.Children.Add(PlainText);

                RoomSelector = new();
                RoomSelector.SetValue(NumericUpDown.NameProperty, "SizeX");
                RoomSelector.SetValue(NumericUpDown.ValueProperty, _rooms[lastRoomDataID].sizeX);
                RoomSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                RoomSelector.SetValue(NumericUpDown.MinimumProperty, 1);
                RoomSelector.SetValue(NumericUpDown.MaximumProperty, 3);
                RoomSelector.ValueChanged += ChangeRoomData;
                _roomDataMenu.SizeX = RoomSelector;
                PairContents.Children.Add(RoomSelector);
                PairContents.SetValue(StackPanel.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center);
                PairContents.SetValue(Panel.MarginProperty, Avalonia.Thickness.Parse("4"));
                PairContents2.Children.Add(PairContents);

                PairContents = new();
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Horizontal);
                PlainText = new();
                PlainText.Text = "Size (Y) ";
                PairContents.Children.Add(PlainText);
                RoomSelector = new();
                RoomSelector.SetValue(NumericUpDown.NameProperty, "SizeY");
                RoomSelector.SetValue(NumericUpDown.ValueProperty, _rooms[lastRoomDataID].sizeY);
                RoomSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                RoomSelector.SetValue(NumericUpDown.MinimumProperty, 1);
                RoomSelector.SetValue(NumericUpDown.MaximumProperty, 3);
                RoomSelector.ValueChanged += ChangeRoomData;
                _roomDataMenu.SizeY = RoomSelector;
                PairContents.Children.Add(RoomSelector);
                PairContents.SetValue(StackPanel.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center);
                PairContents.SetValue(Panel.MarginProperty, Avalonia.Thickness.Parse("4"));
                PairContents2.Children.Add(PairContents);

                InnerGrid.Children.Add(PairContents2);

                PairContents = new();
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Horizontal);
                PairContents.SetValue(Grid.RowProperty, 2);
                PlainText = new();
                Toggle = new();
                PlainText.Text = "Has Door";
                Toggle.Content = PlainText;
                Toggle.SetValue(NumericUpDown.NameProperty, "HasDoor");
                _roomDataMenu.HasDoor = Toggle;
                Toggle.IsChecked =_rooms[lastRoomDataID].hasDoor;
                Toggle.IsCheckedChanged+=ChangeRoomData;
                PairContents.Children.Add(Toggle);
                PlainText = new();
                Toggle = new();
                PlainText.Text = "Is Exit";
                Toggle.Content = PlainText;
                Toggle.SetValue(NumericUpDown.NameProperty, "IsExit");
                _roomDataMenu.IsExit = Toggle;
                Toggle.IsChecked =_rooms[lastRoomDataID].isExit;
                Toggle.IsCheckedChanged+=ChangeRoomData;
                PairContents.Children.Add(Toggle);
                PairContents.SetValue(StackPanel.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center);
                PairContents.SetValue(Panel.MarginProperty, Avalonia.Thickness.Parse("4"));
                InnerGrid.Children.Add(PairContents);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 4);
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#6A966E"));
                ItemOutline.SetValue(Border.HeightProperty, 160);
                ItemOutline.SetValue(Border.WidthProperty, 160);
                ItemOutline.SetValue(Border.PaddingProperty, Thickness.Parse("8 8 0 8"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("4"));
                InnerGrid.Children.Add(ItemOutline);

                Grid OutlineGrid = new();
                ItemOutline.Child = OutlineGrid;
                OutlineGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                OutlineGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                OutlineGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                OutlineGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                OutlineGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));

                OutlineGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                OutlineGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                OutlineGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                OutlineGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                OutlineGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                OutlineGrid.SetValue(UniformGrid.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center);
                // OutlineGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                CheckBox outline_property;
                for (int i = 0; i < OutlineGrid.RowDefinitions.Count; i++)
                {
                    for (int j = 0; j < OutlineGrid.ColumnDefinitions.Count; j++)
                    {
                        outline_property = new();
                        outline_property.SetValue(Grid.ColumnProperty, j);
                        outline_property.SetValue(Grid.RowProperty, i);
                        outline_property.SetValue(CheckBox.IsThreeStateProperty, true);
                         
                        outline_property.IsCheckedChanged+=UpdateRoomOutline;
                        OutlineGrid.Children.Add(outline_property);
                    }
                }
                _roomDataMenu.RoomOutline = OutlineGrid;

                SetupRoomTileView();

                
                ActiveGrid.Children.Add(InnerGrid);

            }
            else if (sender == FloorButton)
            {
                StackPanel PairContents;
                MaskedTextBox WritableTextbox;
                TextBlock PlainText;
                ActiveGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(5, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.SetValue(Grid.BackgroundProperty, BrushColor.ConvertFrom("#751600"));

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#4B0200"));
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#5f5f0f"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                NumericUpDown FloorSelector = new();
                ItemOutline.Child = FloorSelector;
                FloorSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                FloorSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                FloorSelector.SetValue(NumericUpDown.MaximumProperty, _floors.Count-1);
                FloorSelector.SetValue(NumericUpDown.ValueProperty, lastFloorID);
                FloorSelector.ValueChanged += ChangeActiveFloor;
                FloorSelector.SetValue(Grid.RowProperty, 0);
                FloorSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);
                FloorSelector.SetValue(Grid.ColumnProperty, 1);
                ActiveGrid.Children.Add(ItemOutline);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#4B0200"));
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#5f5f0f"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                WritableTextbox = new();
                ItemOutline.Child = WritableTextbox;
                WritableTextbox.SetValue(Grid.RowProperty, 0);
                WritableTextbox.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].floorName);
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.TextChanged += ChangeFloorData;
                WritableTextbox.SetValue(Grid.ColumnProperty, 0);
                _floorMenu.FloorName = WritableTextbox;
                ActiveGrid.Children.Add(ItemOutline);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#4B0200"));
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#6A211F"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid = new();
                ItemOutline.Child = InnerGrid;
                ActiveGrid.Children.Add(ItemOutline);

                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);

                PairContents = new();
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 0);
                PairContents.SetValue(Grid.ColumnProperty, 0);
                PlainText = new();
                PlainText.Text = "Field ID";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.Text = _floors[lastFloorID].ID.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.FieldID = WritableTextbox;
                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);

                InnerGrid.Children.Add(PairContents);

                PairContents = new();
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 0);
                PairContents.SetValue(Grid.ColumnProperty, 1);
                PlainText = new();
                PlainText.Text = "Room ID";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.Text = _floors[lastFloorID].subID.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.RoomID = WritableTextbox;
                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);

                InnerGrid.Children.Add(PairContents);


                PairContents = new();
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 1);
                PairContents.SetValue(Grid.ColumnProperty, 0);
                PlainText = new();
                PlainText.Text = "Min Tile Count";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.Text = _floors[lastFloorID].tileCountMin.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.MinTileCount = WritableTextbox;
                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);

                InnerGrid.Children.Add(PairContents);

                PairContents = new();
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 1);
                PairContents.SetValue(Grid.ColumnProperty, 1);
                PlainText = new();
                PlainText.Text = "Max Tile Count";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.Text = _floors[lastFloorID].tileCountMax.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.MaxTileCount = WritableTextbox;
                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);

                InnerGrid.Children.Add(PairContents);


                PairContents = new();
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 2);
                PairContents.SetValue(Grid.ColumnProperty, 0);
                PlainText = new();
                PlainText.Text = "Dungeon Script";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.Text = _floors[lastFloorID].dungeonScript.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.DungeonScriptID = WritableTextbox;
                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);

                InnerGrid.Children.Add(PairContents);

                PairContents = new();
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 2);
                PairContents.SetValue(Grid.ColumnProperty, 1);
                PlainText = new();
                PlainText.Text = ".ENV";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.Text = _floors[lastFloorID].usedEnv.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.EnvID = WritableTextbox;
                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);
                InnerGrid.Children.Add(PairContents);


                PairContents = new();
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 3);
                PairContents.SetValue(Grid.ColumnProperty, 0);
                PlainText = new();
                PlainText.Text = "Byte04";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.Text = _floors[lastFloorID].Byte04.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.Byte04 = WritableTextbox;
                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);

                InnerGrid.Children.Add(PairContents);

                PairContents = new();
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 3);
                PairContents.SetValue(Grid.ColumnProperty, 1);
                PlainText = new();
                PlainText.Text = "Byte0A";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.Text = _floors[lastFloorID].Byte0A.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.Byte0A = WritableTextbox;
                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);

                InnerGrid.Children.Add(PairContents);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#4B0200"));
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#6A211F"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid = new();
                ItemOutline.Child = InnerGrid;
                ActiveGrid.Children.Add(ItemOutline);
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);

                PairContents = new();
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 0);
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PlainText = new();
                PlainText.Text = "Encounter Table";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.Text = _FloorObjectTable._floorObjectEntries[lastFloorID]._EncountTableLookup.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.EncounterTableID = WritableTextbox;

                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);
                InnerGrid.Children.Add(PairContents);

                PairContents = new();
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 1);
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PlainText = new();
                PlainText.Text = "Loot Table";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.Text = _FloorObjectTable._floorObjectEntries[lastFloorID]._LootTableLookup.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.LootTableID = WritableTextbox;

                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);
                InnerGrid.Children.Add(PairContents);


                PairContents = new();
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(Grid.RowProperty, 2);
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PlainText = new();
                PlainText.Text = "Max Chest Count";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.Text = _FloorObjectTable._floorObjectEntries[lastFloorID]._MaxChestCount.ToString();
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.TextChanged += ChangeFloorData;
                _floorMenu.MaxChestCount = WritableTextbox;

                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);
                InnerGrid.Children.Add(PairContents);

                StackPanel SP2 = new();
                SP2.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Horizontal);
                SP2.SetValue(Grid.RowProperty, 3);
                SP2.SetValue(Grid.ColumnSpanProperty, 2);

                PairContents = new();
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PlainText = new();
                PlainText.Text = "Max Enemy Count";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.Text = _FloorObjectTable._floorObjectEntries[lastFloorID]._InitialEncounterCount.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                _floorMenu.InitialEnemyCount = WritableTextbox;
                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);

                SP2.Children.Add(PairContents);

                PairContents = new();
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                PairContents.SetValue(StackPanel.MarginProperty, Thickness.Parse("4 0 4 0"));
                PlainText = new();
                PlainText.Text = "Min. Enemy Count";
                PlainText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox = new();
                WritableTextbox.Text = _FloorObjectTable._floorObjectEntries[lastFloorID]._MinEncounterCount.ToString();
                WritableTextbox.TextChanged += ChangeFloorData;
                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.MarginProperty, Thickness.Parse("4 0 4 0"));
                _floorMenu.MinEnemyCount = WritableTextbox;
                PairContents.Children.Add(PlainText);
                PairContents.Children.Add(WritableTextbox);
                SP2.Children.Add(PairContents);

                InnerGrid.Children.Add(SP2);

            }
            else if (sender == TemplateButton)
            {
                StackPanel PairContents;
                Button button;
                MaskedTextBox WritableTextbox;
                TextBlock PlainText;

                ActiveGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(3, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(2, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.SetValue(Grid.BackgroundProperty, BrushColor.ConvertFrom("#BAA000"));
                // https://docs.avaloniaui.net/docs/reference/controls/numericupdown

                ItemOutline = new();
                ActiveGrid.Children.Add(ItemOutline);
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Border.BorderBrushProperty, BrushColor.ConvertFrom("#827000"));
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#5f5f0f"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                NumericUpDown RoomSelector = new();
                ItemOutline.Child = RoomSelector;
                RoomSelector.SetValue(NumericUpDown.ValueProperty, lastTemplateID);
                RoomSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                RoomSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                RoomSelector.SetValue(NumericUpDown.MaximumProperty, _templates.Count-1);
                RoomSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);
                RoomSelector.ValueChanged+=ChangeActiveTemplate;
                _roomDataMenu.RoomID = RoomSelector;


                InnerGrid2 = new();
                InnerGrid2.SetValue(Grid.MarginProperty, Avalonia.Thickness.Parse("4"));
                InnerGrid2.SetValue(Grid.RowProperty, 1);
                InnerGrid2.SetValue(Grid.ColumnSpanProperty, 2);
                InnerGrid2.ColumnDefinitions.Add(new(10, GridUnitType.Star));
                InnerGrid2.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                ActiveGrid.Children.Add(InnerGrid2);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#E0C100"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid = new();
                ItemOutline.Child = InnerGrid;
                InnerGrid.SetValue(Grid.MarginProperty, Avalonia.Thickness.Parse("4"));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.Children.Add(ItemOutline);

                // EFA422
                ScrollViewer view1 = new();
                WrapPanel panel1 = new();
                view1.SetValue(Grid.RowProperty, 0);
                view1.SetValue(Grid.ColumnProperty, 0);
                panel1.SetValue(Panel.BackgroundProperty, BrushColor.ConvertFrom("#EFA422"));
                panel1.SetValue(Panel.NameProperty, "UsedRooms");
                ScrollViewer view2 = new();
                WrapPanel panel2 = new();
                view2.SetValue(Grid.RowProperty, 1);
                view2.SetValue(Grid.ColumnProperty, 0);
                view2.SetValue(Grid.ColumnSpanProperty, 2);
                panel2.SetValue(Panel.BackgroundProperty, BrushColor.ConvertFrom("#EFA422"));
                panel2.SetValue(Panel.NameProperty, "UnusedRooms");

                ScrollViewer view3 = new();
                WrapPanel panel3 = new();
                view3.SetValue(Grid.RowProperty, 0);
                view3.SetValue(Grid.ColumnProperty, 1);
                panel3.SetValue(Panel.BackgroundProperty, BrushColor.ConvertFrom("#EFA422"));
                
                panel3.SetValue(Panel.NameProperty, "UsedRoomsEx");

                InnerGrid.Children.Add(view1);
                InnerGrid.Children.Add(view2);
                InnerGrid.Children.Add(view3);



                ItemOutline = new();
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#E0C100"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid = new();
                ItemOutline.Child = InnerGrid;
                InnerGrid.SetValue(Grid.MarginProperty, Avalonia.Thickness.Parse("4"));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.Children.Add(ItemOutline);

                ScrollViewer view5 = new();
                WrapPanel panel5 = new();
                view5.SetValue(Grid.RowProperty, 0);
                panel5.SetValue(Panel.BackgroundProperty, BrushColor.ConvertFrom("#EFA422"));
                panel5.SetValue(Panel.NameProperty, "UsedExit");
                ScrollViewer view6 = new();
                WrapPanel panel6 = new();
                view6.SetValue(Grid.RowProperty, 1);
                panel6.SetValue(Panel.BackgroundProperty, BrushColor.ConvertFrom("#EFA422"));
                panel6.SetValue(Panel.NameProperty, "UnusedExit");

                InnerGrid.Children.Add(view5);
                InnerGrid.Children.Add(view6);

                view1.Content = panel1;
                view2.Content = panel2;
                view3.Content = panel3;
                view5.Content = panel5;
                view6.Content = panel6;
                panel1.SetValue(DragDrop.AllowDropProperty, true);
                panel1.SetValue(WrapPanel.MarginProperty, Avalonia.Thickness.Parse("8"));
                panel2.SetValue(DragDrop.AllowDropProperty, true);
                panel2.SetValue(WrapPanel.MarginProperty, Avalonia.Thickness.Parse("8"));
                panel3.SetValue(DragDrop.AllowDropProperty, true);
                panel3.SetValue(WrapPanel.MarginProperty, Avalonia.Thickness.Parse("8"));
                panel5.SetValue(DragDrop.AllowDropProperty, true);
                panel5.SetValue(WrapPanel.MarginProperty, Avalonia.Thickness.Parse("8"));
                panel6.SetValue(DragDrop.AllowDropProperty, true);
                panel6.SetValue(WrapPanel.MarginProperty, Avalonia.Thickness.Parse("8"));

                _templateMenu.RegularRooms.Add(panel1);
                _templateMenu.RegularRooms.Add(panel2);
                _templateMenu.RegularRooms.Add(panel3);

                _templateMenu.ExitRooms.Add(panel5);
                _templateMenu.ExitRooms.Add(panel6);

                for (int i = 0; i < _templates[lastTemplateID].roomExCount; i++)
                {
                    int current = _templates[lastTemplateID].rooms[i];
                    PlainText = new();
                    PlainText.Text = current.ToString();
                    PlainText.SetValue(TextBlock.MarginProperty, Avalonia.Thickness.Parse("8"));
                    PlainText.PointerPressed+= TemplateDragBlock;
                    if (_rooms[current].isExit)
                    {
                        panel5.Children.Add(PlainText);
                    }
                    else if (i >= _templates[lastTemplateID].roomCount)
                    {
                        panel3.Children.Add(PlainText);
                    }
                    else
                    {
                        panel1.Children.Add(PlainText);
                    }
                }

                for (byte i = 0; i < _rooms.Count; i++)
                { 
                    if (!_templates[lastTemplateID].rooms.Contains(i))
                    {
                        PlainText = new();
                        PlainText.Text = i.ToString();
                        PlainText.SetValue(TextBlock.MarginProperty, Avalonia.Thickness.Parse("8"));
                        PlainText.PointerPressed+= TemplateDragBlock;
                        if (_rooms[i].isExit)
                        {
                            panel6.Children.Add(PlainText);
                        }
                        else
                        {
                            panel2.Children.Add(PlainText);
                        }
                        
                    }
                }


                InnerGrid = new();
                InnerGrid.SetValue(Grid.RowProperty, 2);
                InnerGrid.SetValue(Grid.ColumnSpanProperty, 2);
                InnerGrid.ColumnDefinitions.Add(new(7, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                panel1.SetValue(Grid.ColumnProperty, 0);

                ItemOutline = new();
                InnerGrid.Children.Add(ItemOutline);
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#EFA422"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("6"));
                panel1 = new();
                ItemOutline.Child = panel1;;
                foreach (var kvp in _dungeon_template_dict)
                {
                    if (kvp.Value == lastTemplateID)
                    {
                        PlainText = new();
                        PlainText.Text = kvp.Key.ToString();
                        PlainText.SetValue(TextBlock.MarginProperty, Avalonia.Thickness.Parse("8"));
                        panel1.Children.Add(PlainText);
                    }   
                }
                _templateMenu.FieldsThatUse = panel1;

                PairContents = new();
                RoomSelector = new();
                RoomSelector.SetValue(NumericUpDown.NameProperty, "FieldSelected");
                RoomSelector.SetValue(NumericUpDown.ValueProperty, 0);
                RoomSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                RoomSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                RoomSelector.SetValue(NumericUpDown.MaximumProperty, 255);
                _templateMenu.FieldSelected = RoomSelector;
                PairContents.Children.Add(RoomSelector);
                StackPanel PairContents2 = new();
                button = new();
                button.Content = "+";
                button.SetValue(StackPanel.MarginProperty, Avalonia.Thickness.Parse("4"));
                button.SetValue(Button.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Stretch);
                button.SetValue(Button.HorizontalContentAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center);
                button.SetValue(Button.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Center);
                button.Click+=AddToTemplateDict;
                PairContents2.Children.Add(button);

                button = new();
                button.Content = "-";
                button.SetValue(StackPanel.MarginProperty, Avalonia.Thickness.Parse("4"));
                button.SetValue(Button.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Stretch);
                button.SetValue(Button.HorizontalContentAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center);
                button.SetValue(Button.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Center);
                button.Click+=RemoveFromTemplateDict;
                PairContents2.Children.Add(button);
                PairContents2.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Horizontal);
                PairContents.Children.Add(PairContents2);
                PairContents.SetValue(Grid.ColumnProperty, 1);
                InnerGrid.Children.Add(PairContents);

                ActiveGrid.Children.Add(InnerGrid);

            }
            else
            {
                // Might want something else here
            }
        }
        private void SetupRoomTileView()
        {
            if (_roomDataMenu.RoomTiles is not null)
            {
                ActiveGrid.Children.Remove(_roomDataMenu.RoomTiles);
            }
            Avalonia.Media.BrushConverter BrushColor = new();


            Border ItemOutline = new();
            ItemOutline.SetValue(Grid.RowProperty, 4);
            ItemOutline.SetValue(Border.BackgroundProperty, BrushColor.ConvertFrom("#6A966E"));
            ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("15"));
            ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("4"));
            ActiveGrid.Children.Add(ItemOutline);


            _roomDataMenu.RoomTiles = new Grid();
            ItemOutline.Child = _roomDataMenu.RoomTiles;


            _roomDataMenu.RoomTiles.ColumnDefinitions.Add(new(6, GridUnitType.Star));
            for (int i = 1; i < _rooms[lastRoomDataID].sizeX; i++)
            {
                _roomDataMenu.RoomTiles.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                _roomDataMenu.RoomTiles.ColumnDefinitions.Add(new(6, GridUnitType.Star));
            }
            
            _roomDataMenu.RoomTiles.RowDefinitions.Add(new(6, GridUnitType.Star));
            for (int i = 1; i < _rooms[lastRoomDataID].sizeY; i++)
            {
                _roomDataMenu.RoomTiles.RowDefinitions.Add(new(1, GridUnitType.Star));
                _roomDataMenu.RoomTiles.RowDefinitions.Add(new(6, GridUnitType.Star));
            }

            _roomDataMenu.RoomTiles.SetValue(Grid.WidthProperty, 300);
            _roomDataMenu.RoomTiles.SetValue(Grid.HeightProperty, 300);
            _roomDataMenu.RoomTiles =  _roomDataMenu.RoomTiles;
            MapTile panel;
            ToggleButton Toggle;
            Avalonia.Media.BrushConverter bgColor = new();
            _roomDataMenu.RoomTiles.Children.Clear();
            for (int i = 0; i < 2*_rooms[lastRoomDataID].sizeY-1; i++)
            {
                for (int j = 0; j < 2*_rooms[lastRoomDataID].sizeX-1; j++)
                {
                    if (i % 2 == 0 && j % 2 == 0)
                    {


                        panel = new();
                        panel.SetValue(Grid.ColumnProperty, j);
                        panel.SetValue(Grid.RowProperty, i);


                        Toggle = new();
                        Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Stretch);
                        Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Top);
                        Toggle.SetValue(ToggleButton.MarginProperty, Avalonia.Thickness.Parse("20, 0, 20, 10"));
                        Toggle.SetValue(ToggleButton.HeightProperty, 10);
                        Toggle.SetValue(ToggleButton.NameProperty, "Up");
                        Toggle.IsChecked = (_rooms[lastRoomDataID].connectionValues[i/2][j/2] & 0x1) > 0;
                        Toggle.IsCheckedChanged+=ChangeTileDirectionData;
                        panel.Children.Add(Toggle);


                        Toggle = new();
                        Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Left);
                        Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Stretch);
                        Toggle.SetValue(ToggleButton.MarginProperty, Avalonia.Thickness.Parse("0, 20, 10, 20"));
                        Toggle.SetValue(ToggleButton.WidthProperty, 10);
                        Toggle.SetValue(ToggleButton.NameProperty, "Left");
                        Toggle.IsChecked = (_rooms[lastRoomDataID].connectionValues[i/2][j/2] & 0x2) > 0;
                        Toggle.IsCheckedChanged+=ChangeTileDirectionData;
                        panel.Children.Add(Toggle);

                        Toggle = new();
                        Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Stretch);
                        Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Bottom);
                        Toggle.SetValue(ToggleButton.MarginProperty, Avalonia.Thickness.Parse("20, 10, 20, 0"));
                        Toggle.SetValue(ToggleButton.HeightProperty, 10);
                        Toggle.SetValue(ToggleButton.NameProperty, "Down");
                        Toggle.IsChecked = (_rooms[lastRoomDataID].connectionValues[i/2][j/2] & 0x4) > 0;
                        Toggle.IsCheckedChanged+=ChangeTileDirectionData;
                        panel.Children.Add(Toggle);

                        Toggle = new();
                        Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Right);
                        Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Stretch);
                        Toggle.SetValue(ToggleButton.MarginProperty, Avalonia.Thickness.Parse("10, 20, 0, 20"));
                        Toggle.SetValue(ToggleButton.WidthProperty, 10);
                        Toggle.SetValue(ToggleButton.NameProperty, "Right");
                        Toggle.IsChecked = (_rooms[lastRoomDataID].connectionValues[i/2][j/2] & 0x8) > 0;
                        Toggle.IsCheckedChanged+=ChangeTileDirectionData;
                        panel.Children.Add(Toggle);

                        panel.SetValue(Panel.BackgroundProperty, Brushes.White);
                        panel.PointerPressed += ChangeTileBackground;

                        panel.value = (byte)(_rooms[lastRoomDataID].revealProperties[i/2][j/2] >> 4);
                        if (panel.value != 0)
                        {
                            Int32 color = 0xFFFFFF / 16;
                            color = color * (panel.value);
                            panel.SetValue(MapTile.BackgroundProperty, bgColor.ConvertFrom("#" + (color.ToString("X").PadLeft(6))));
                        }
                        else
                        {
                            panel.SetValue(MapTile.BackgroundProperty, bgColor.ConvertFrom("#FFFFFF"));
                        }
                        _roomDataMenu.RoomTiles.Children.Add(panel);
                    }
                    else
                    {
                        Toggle = new();
                        Toggle.SetValue(Grid.ColumnProperty, j);
                        Toggle.SetValue(Grid.RowProperty, i);
                        if (j % 2 == 1 && i % 2 == 0)
                        {
                            Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center);
                            Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Stretch);


                            var val1 = (_rooms[lastRoomDataID].connectionValues[(i-1)/2][j/2] >> 0x8);
                            var val2 = (_rooms[lastRoomDataID].connectionValues[(i+1)/2][j/2] >> 0x8);

                            Toggle.IsChecked = ((val1 & 0x4) > 0) && ((val2 & 0x1) > 0);

                            Toggle.IsVisible = _rooms[lastRoomDataID].hasDoor;
                            Toggle.IsCheckedChanged+=ChangeTileDoorData;
                            _roomDataMenu.RoomTiles.Children.Add(Toggle);
                        }
                        else if (i % 2 == 1 & j % 2 == 0)
                        {


                            Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Stretch);
                            Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Center);
                            var val1 = (_rooms[lastRoomDataID].connectionValues[i/2][(j-1)/2] >> 0x8);
                            var val2 = (_rooms[lastRoomDataID].connectionValues[i/2][(j+1)/2] >> 0x8);

                            Toggle.IsChecked = ((val1 & 0x8) > 0) && ((val2 & 0x2) > 0);

                            Toggle.IsVisible = _rooms[lastRoomDataID].hasDoor;
                            Toggle.IsCheckedChanged+=ChangeTileDoorData;
                            _roomDataMenu.RoomTiles.Children.Add(Toggle);
                        }

                    }
                }
            }
            

            int row;
            int col;
            foreach (CheckBox box in _roomDataMenu.RoomOutline.Children)
            {
                row = box.GetValue(Grid.RowProperty);
                col = box.GetValue(Grid.ColumnProperty);
                if ( _rooms[lastRoomDataID].mapRamOutline[row][col] == 0 )
                {
                        box.SetValue(CheckBox.IsCheckedProperty, false);
                }
                else if ( _rooms[lastRoomDataID].mapRamOutline[row][col] == 1 )
                {
                    box.SetValue(CheckBox.IsCheckedProperty, true);
                }
                else if ( _rooms[lastRoomDataID].mapRamOutline[row][col] == 2 )
                {
                    box.SetValue(CheckBox.IsCheckedProperty, null);
                }
            }
        }
        private void ChangeActiveEncounter(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            lastEncountID = (int)((NumericUpDown)sender).Value;
            if ( lastEncountID > _EnemyEncountTable._enemyEncountEntries.Count)
            {
                ((NumericUpDown)sender).Text = (_EnemyEncountTable._enemyEncountEntries.Count-1).ToString();
                lastEncountID = _EnemyEncountTable._enemyEncountEntries.Count-1;
            }
            else if (lastEncountID < 0)
            {
                ((NumericUpDown)sender).Text = "0";
                lastEncountID = 0;
            }

            _encounterMenu.Unit[0].SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[0].ToString());
            _encounterMenu.Unit[1].SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[1].ToString());
            _encounterMenu.Unit[2].SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[2].ToString());
            _encounterMenu.Unit[3].SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[3].ToString());
            _encounterMenu.Unit[4].SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[4].ToString());


            _encounterMenu.Flags.SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._flags.ToString());
            _encounterMenu.Field04.SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._field04.ToString());
            _encounterMenu.Field06.SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._field06.ToString());
            _encounterMenu.FieldID.SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._fieldID.ToString());
            _encounterMenu.MusicID.SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._musicID.ToString());
            _encounterMenu.RoomID.SetValue(MaskedTextBox.TextProperty, _EnemyEncountTable._enemyEncountEntries[lastEncountID]._roomID.ToString());
        }
        private void ChangeEncounterData(object? sender, EventArgs e)
        {
            uint value;
            try
            {
                value = (uint)int.Parse(((MaskedTextBox)sender).Text);
            }
            catch
            {
                value = 0;
            }
            if (sender.Equals(_encounterMenu.Unit[0]))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[0] = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Unit[1]))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[1] = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Unit[2]))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[2] = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Unit[3]))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[3] = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Unit[4]))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._units[4] = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Flags))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._flags = value;
            }
            else if (sender.Equals(_encounterMenu.Field04))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._field04 = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Field06))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._field06 = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.FieldID))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._fieldID = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.MusicID))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._musicID = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.RoomID))
            {
                _EnemyEncountTable._enemyEncountEntries[lastEncountID]._roomID = (ushort)value;
            }
            else
            {
                throw new Exception();
            }
        }
        private void ChangeActiveEncounterTable(object? sender, NumericUpDownValueChangedEventArgs e)  
        {
            lastEncountTableID = (int)((NumericUpDown)sender).Value;
            if (lastEncountTableID > _FloorEncountTable._floorEncounterEntries.Count)
            {
                ((NumericUpDown)sender).Text = "0";
                lastEncountTableID = 0;
            }
            else if (lastEncountTableID < 0)
            {
                ((NumericUpDown)sender).Text = (_FloorEncountTable._floorEncounterEntries.Count-1).ToString();
                lastEncountTableID = _FloorEncountTable._floorEncounterEntries.Count-1;
            }

            _encounterTableMenu.Byte00.SetValue(MaskedTextBox.TextProperty, _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._u1.ToString());
            _encounterTableMenu.Byte03.SetValue(MaskedTextBox.TextProperty, _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._u2.ToString());
            _encounterTableMenu.Byte06.SetValue(MaskedTextBox.TextProperty, _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._u3.ToString());
            for (int i = 0; i < _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._encounters.Count; i++)
            {
                _encounterTableMenu.Encounters[i].SetValue(MaskedTextBox.TextProperty, _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._encounters[i].ToString());
            }
        }
        private void ChangeEncounterTableData(object? sender, EventArgs e)
        {

            uint value;
            try
            {
                value = (uint)int.Parse(((MaskedTextBox)sender).Text);
            }
            catch
            {
                value = 0;
            }

            if (sender ==  _encounterTableMenu.Byte00)
            {
                _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._u1 = (byte)value;
            }
            else if (sender ==  _encounterTableMenu.Byte03)
            {
                _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._u2 = (byte)value;
            }
            else if (sender ==  _encounterTableMenu.Byte06)
            {
                _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._u3 = (byte)value;
            }
            else
            {
                for (int i = 0; i < _encounterTableMenu.Encounters.Length; i++)
                {
                    if (_encounterTableMenu.Encounters[i] == sender)
                    {
                        _FloorEncountTable._floorEncounterEntries[lastEncountTableID]._encounters[i] = (ushort)value;
                        return;
                    }
                }
                throw new Exception();
                
            }
        }
        private void ChangeActiveLootTable(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            lastLootID = (int)((NumericUpDown)sender).Value;
            if (lastLootID > _ChestLootTable._chestLootEntries.Count-1)
            {
                ((NumericUpDown)sender).Text = "0";
                lastLootID = 0;
            }
            else if (lastLootID < 0)
            {
                ((NumericUpDown)sender).Text = (_ChestLootTable._chestLootEntries.Count-1).ToString();
                lastLootID = _ChestLootTable._chestLootEntries.Count-1;
            }
            for (int i = 0; i < _lootTableMenu.Entries.Count; i++)
            {
                _lootTableMenu.Entries[i].ItemWeight.SetValue(MaskedTextBox.TextProperty, _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._itemWeight.ToString());
                _lootTableMenu.Entries[i].ItemID.SetValue(MaskedTextBox.TextProperty, _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._itemID.ToString());
                _lootTableMenu.Entries[i].ChestFlags.SetValue(MaskedTextBox.TextProperty, _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._chestFlags.ToString());
                _lootTableMenu.Entries[i].ChestModel.SetValue(ToggleButton.IsCheckedProperty, _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._chestModel > 0);
            }
        }
        private void ChangeLootTableData(object? sender, EventArgs e)
        {
            if (sender is MaskedTextBox box)
            {
                for (int i = 0; i < _lootTableMenu.Entries.Count; i++)
                {
                    var entry = _lootTableMenu.Entries[i];
                    if (entry.ItemWeight == box)
                    {
                        _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._itemWeight = (ushort)int.Parse(box.Text);
                        break;
                    }
                    else if (entry.ItemID == box)
                    {
                        _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._itemID = (ushort)int.Parse(box.Text);
                        break;
                    }
                    else if (entry.ChestFlags == box)
                    {
                        _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._chestFlags= (ushort)int.Parse(box.Text);
                        break;
                    }
                }
            }
            else if (sender is ToggleButton toggleButton)
            {
                for (int i = 0; i < _lootTableMenu.Entries.Count; i++)
                {
                    var entry = _lootTableMenu.Entries[i];
                    if (entry.ChestModel == toggleButton)
                    {
                        if (toggleButton.IsChecked == false)
                        {
                            _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._chestModel = 0;
                        }
                        else
                        {
                            _ChestLootTable._chestLootEntries[lastLootID]._lootEntries[i]._chestModel = 1;
                        }
                        break;
                    }
                }
            }
        }
        private void ChangeActiveFloor(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (((NumericUpDown)sender).Value == null)
            {
                return;
            }
            lastFloorID = (int)((NumericUpDown)sender).Value;
            if (lastFloorID > _floors.Count-1)
            {
                ((NumericUpDown)sender).Text = "0";
                lastFloorID = 0;
            }
            else if (lastFloorID < 0)
            {
                ((NumericUpDown)sender).Text = (_floors.Count-1).ToString();
                lastFloorID = _floors.Count-1;
            }

            _floorMenu.FloorName.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].floorName);
            _floorMenu.FieldID.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].ID.ToString());
            _floorMenu.RoomID.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].subID.ToString());
            _floorMenu.MinTileCount.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].tileCountMin.ToString());
            _floorMenu.MaxTileCount.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].tileCountMax.ToString());
            _floorMenu.DungeonScriptID.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].dungeonScript.ToString());
            _floorMenu.EnvID.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].usedEnv.ToString());
            _floorMenu.Byte04.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].Byte04.ToString());
            _floorMenu.Byte0A.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].Byte0A.ToString());

            _floorMenu.EncounterTableID.SetValue(MaskedTextBox.TextProperty, _FloorObjectTable._floorObjectEntries[lastFloorID]._EncountTableLookup.ToString());
            _floorMenu.LootTableID.SetValue(MaskedTextBox.TextProperty, _FloorObjectTable._floorObjectEntries[lastFloorID]._LootTableLookup.ToString());
            _floorMenu.MaxChestCount.SetValue(MaskedTextBox.TextProperty, _FloorObjectTable._floorObjectEntries[lastFloorID]._MaxChestCount.ToString());
            _floorMenu.InitialEnemyCount.SetValue(MaskedTextBox.TextProperty, _FloorObjectTable._floorObjectEntries[lastFloorID]._InitialEncounterCount.ToString());
            _floorMenu.MinEnemyCount.SetValue(MaskedTextBox.TextProperty, _FloorObjectTable._floorObjectEntries[lastFloorID]._MinEncounterCount.ToString());


        }
        private void ChangeFloorData(object? sender, EventArgs e)
        {
            if (sender  == _floorMenu.FloorName)
            {
                if (((MaskedTextBox)sender).Text != null)
                {
                    // There's a weird bug going on where some hits to this function involves null text.
                    // As far as I'm aware, this shouldn't be an issue due to all text boxes being given some text BEFORE
                    // registering this function with the TextChanged event
                    // This is a quick and dirty workaround for the moment, ideally I'll find a proper solution later
                    _floors[lastFloorID].floorName = ((MaskedTextBox)sender).Text;
                }
                return;
            }
            uint value;
            try
            {
                value = (uint)int.Parse(((MaskedTextBox)sender).Text);
            }
            catch
            {
                value = 0;
            }
            if (sender  == _floorMenu.FieldID)
            {
                _floors[lastFloorID].ID = (ushort)value;
            }
            else if (sender  == _floorMenu.RoomID)
            {
                _floors[lastFloorID].subID = (ushort)value;
            }
            else if (sender  == _floorMenu.MinTileCount)
            {
                _floors[lastFloorID].tileCountMin = (byte)value;
            }
            else if (sender  == _floorMenu.MaxTileCount)
            {
                _floors[lastFloorID].tileCountMax = (byte)value;
            }
            else if (sender  == _floorMenu.DungeonScriptID)
            {
                _floors[lastFloorID].dungeonScript = (byte)value;
            }
            else if (sender  == _floorMenu.EnvID)
            {
                _floors[lastFloorID].usedEnv = (byte)value;
            }
            else if (sender  == _floorMenu.Byte04)
            {
                _floors[lastFloorID].Byte04 = (uint)value;
            }
            else if (sender  == _floorMenu.Byte0A)
            {
                _floors[lastFloorID].Byte0A = (ushort)value;
            }


            else if (sender  == _floorMenu.EncounterTableID)
            {
                _FloorObjectTable._floorObjectEntries[lastFloorID]._EncountTableLookup = (ushort)value;
            }
            else if (sender  == _floorMenu.LootTableID)
            {
                _FloorObjectTable._floorObjectEntries[lastFloorID]._LootTableLookup = value;
            }
            else if (sender  == _floorMenu.MaxChestCount)
            {
                _FloorObjectTable._floorObjectEntries[lastFloorID]._MaxChestCount = (byte)value;
            }
            else if (sender  == _floorMenu.InitialEnemyCount)
            {
                _FloorObjectTable._floorObjectEntries[lastFloorID]._InitialEncounterCount = (byte)value;
            }
            else if (sender  == _floorMenu.MinEnemyCount)
            {
                _FloorObjectTable._floorObjectEntries[lastFloorID]._MinEncounterCount = (byte)value;
            }
        }
        private void ChangeActiveRoom(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (((NumericUpDown)sender).Value == null)
            {
                return;
            }
            lastRoomDataID = (int)((NumericUpDown)sender).Value;
            if (lastRoomDataID > _rooms.Count-1)
            {
                ((NumericUpDown)sender).Text = "0";
                lastRoomDataID = 0;
            }
            else if (lastRoomDataID < 0)
            {
                ((NumericUpDown)sender).Text = (_rooms.Count-1).ToString();
                lastRoomDataID = _rooms.Count-1;
            }


            _roomDataMenu.SizeX.Text = _rooms[lastRoomDataID].sizeX.ToString();
            _roomDataMenu.SizeY.Text = _rooms[lastRoomDataID].sizeY.ToString();
            _roomDataMenu.HasDoor.IsChecked = _rooms[lastRoomDataID].hasDoor;
            _roomDataMenu.IsExit.IsChecked = _rooms[lastRoomDataID].isExit;

            SetupRoomTileView();

        }
        private void ChangeRoomData(object? sender, EventArgs e)
        {
            if (sender == _roomDataMenu.SizeX)
            { 
                if ( _roomDataMenu.SizeX.Value != _rooms[lastRoomDataID].sizeX) 
                {
                    
                    if (_rooms[lastRoomDataID].sizeX > _roomDataMenu.SizeX.Value)
                    {
                        // Removing all pre-existing properties from rooms
                        for (int i = _rooms[lastRoomDataID].sizeX-1; i > _roomDataMenu.SizeX.Value-1; i--)
                        {
                            for (int j = 0; j < _rooms[lastRoomDataID].sizeY; j++)
                            {
                                _rooms[lastRoomDataID].connectionValues[j][i] = 0;
                                _rooms[lastRoomDataID].revealProperties[j][i] = 0;
                            }
                        }
                        for (int j = 0; j < _rooms[lastRoomDataID].sizeY; j++)
                        {
                            // Now that no values are to the right of the new rightmost value, it now is 'promoted'
                            // to indicating where another room connects to
                            if ( (_rooms[lastRoomDataID].connectionValues[j][(int)(_roomDataMenu.SizeX.Value-1)] & 0x8) > 0 )
                            {
                                _rooms[lastRoomDataID].connectionValues[j][(int)(_roomDataMenu.SizeX.Value-1)] |= 0x80;
                            }
                        }
                    }
                    _rooms[lastRoomDataID].sizeX = (byte)_roomDataMenu.SizeX.Value;
                    
                    SetupRoomTileView();
                }
            }
            else if (sender == _roomDataMenu.SizeY)
            {
                if (_roomDataMenu.SizeY.Value != _rooms[lastRoomDataID].sizeY)
                {
                    // Removing all pre-existing properties from rooms
                    for (int i = _rooms[lastRoomDataID].sizeY-1; i > _roomDataMenu.SizeY.Value-1; i--)
                    {
                        for (int j = 0; j < _rooms[lastRoomDataID].sizeX; j++)
                        {
                            _rooms[lastRoomDataID].connectionValues[i][j] = 0;
                            _rooms[lastRoomDataID].revealProperties[i][j] = 0;
                        }
                    }
                    for (int j = 0; j < _rooms[lastRoomDataID].sizeX; j++)
                    {
                        // Now that no values are to the right of the new rightmost value, it now is 'promoted'
                        // to indicating where another room connects to
                        if ((_rooms[lastRoomDataID].connectionValues[(int)(_roomDataMenu.SizeY.Value-1)][j] & 0x4) > 0)
                        {
                            _rooms[lastRoomDataID].connectionValues[(int)(_roomDataMenu.SizeY.Value-1)][j] |= 0x40;
                        }
                    }

                    _rooms[lastRoomDataID].sizeY = (byte)_roomDataMenu.SizeY.Value;

                    SetupRoomTileView();
                }
            }
            else if (sender == _roomDataMenu.HasDoor)
            {

                bool has_door = _roomDataMenu.HasDoor.IsChecked ?? false;
                _rooms[lastRoomDataID].hasDoor = has_door;
                foreach (ToggleButton button in _roomDataMenu.RoomTiles.Children.OfType<ToggleButton>())
                {
                    button.IsVisible = has_door;
                }
            }
            else if (sender == _roomDataMenu.IsExit)
            {
                _rooms[lastRoomDataID].isExit = _roomDataMenu.IsExit.IsChecked ?? false;
            }
        }
        private void AddRoom(object? sender, EventArgs e)
        {
            if (_rooms.Count <257)
            {
                DungeonRoom NewRoom = new();

                NewRoom.mapRamOutline= new List<List<byte>>() { new List<byte> { 0, 0, 0, 0, 0 },
                                                                new List<byte> { 0, 0, 0, 0, 0 },
                                                                new List<byte> { 0, 0, 0, 0, 0 },
                                                                new List<byte> { 0, 0, 0, 0, 0 },
                                                                new List<byte> { 0, 0, 0, 0, 0 } };


                NewRoom.revealProperties = new List<List<byte>>() { new List<byte> { 0, 0, 0 },
                                                                new List<byte> { 0, 0, 0 },
                                                                new List<byte> { 0, 0, 0 } };

                NewRoom.connectionValues = new List<List<int>>() { new List<int> { 0, 0, 0 },
                                                               new List<int> { 0, 0, 0 },
                                                               new List<int> { 0, 0, 0 } };
                NewRoom.hasDoor = false;
                NewRoom.isExit = false;
                NewRoom.sizeX = 1;
                NewRoom.sizeY = 1;
                NewRoom.x_y_offsets = new();
                NewRoom.ID = (byte)_rooms.Count;
                _rooms.Add(NewRoom);
                _roomDataMenu.RoomID.SetValue(NumericUpDown.MaximumProperty, _rooms.Count-1);
            }
            else
            {
                // Maybe let them know ;)
            }
        }
        private void ChangeTileDirectionData(object? sender, EventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            Control? parent = (Control)button.Parent;
            int y = parent.GetValue(Grid.RowProperty);
            int x = parent.GetValue(Grid.ColumnProperty);

            // 'other room', in this context, refers to a tile that is not defined in the scope of 
            // the current set of room data.
            bool connects_to_other_room = true;

            // The conditionals XORs are for the outermost tiles, which have the upper nybble set
            // indicating the direction that other rooms connect from.

            if (button.Name == "Up")
            {
                _rooms[lastRoomDataID].connectionValues[y/2][x/2] ^= 0x1;
                if ((y/2-1) >= 0)
                {
                    for (int i = (y/2-1)-1; i >= 0; i--)
                    {
                        // Assume that anything non-zero is a valid tile
                        if (_rooms[lastRoomDataID].connectionValues[i][x/2] != 0)
                        {
                            connects_to_other_room = false;
                            if ((_rooms[lastRoomDataID].connectionValues[i][x/2] & 0x40) > 0)
                            {
                                // Something's blocking the once-open space for this tile so need to update it
                                _rooms[lastRoomDataID].connectionValues[i][x/2] ^= 0x40;
                            }
                        }
                    }
                }

                if (connects_to_other_room)
                {
                    _rooms[lastRoomDataID].connectionValues[y/2][x/2] ^= 0x10;
                }
            }
            else if (button.Name == "Right")
            {
                _rooms[lastRoomDataID].connectionValues[y/2][x/2] ^= 0x8;

                if ((x/2+1) < _rooms[lastRoomDataID].sizeX)
                {
                    for (int i = x/2+1; i < _rooms[lastRoomDataID].sizeX; i++)
                    {
                        // Assume that anything non-zero is a valid tile
                        if (_rooms[lastRoomDataID].connectionValues[y/2][i] != 0)
                        {
                            connects_to_other_room = false;
                            if ((_rooms[lastRoomDataID].connectionValues[y/2][i] & 0x20) > 0)
                            {
                                // Something's blocking the once-open space for this tile so need to update it
                                _rooms[lastRoomDataID].connectionValues[y/2][i] ^= 0x20;
                            }
                        }
                    }
                }

                if (connects_to_other_room)
                {
                    _rooms[lastRoomDataID].connectionValues[y/2][x/2] ^= 0x80;
                }

            }
            else if (button.Name == "Down")
            {
                _rooms[lastRoomDataID].connectionValues[y/2][x/2] ^= 0x4;
                if ( (y/2+1) < _rooms[lastRoomDataID].sizeY )
                {
                    for (int i = y/2+1; i < _rooms[lastRoomDataID].sizeY; i++)
                    {
                        // Assume that anything non-zero is a valid tile
                        if (_rooms[lastRoomDataID].connectionValues[i][x/2] != 0)
                        {
                            connects_to_other_room = false;
                            if ((_rooms[lastRoomDataID].connectionValues[i][x/2] & 0x10) > 0)
                            {
                                // Something's blocking the once-open space for this tile so need to update it
                                _rooms[lastRoomDataID].connectionValues[i][x/2] ^= 0x10;
                            }
                        }
                    }
                }

                if (connects_to_other_room)
                {
                    _rooms[lastRoomDataID].connectionValues[y/2][x/2] ^= 0x40;
                }
            }
            else if (button.Name == "Left")
            {
                _rooms[lastRoomDataID].connectionValues[y/2][x/2] ^= 0x2;

                if ( (x/2-1) >= 0 )
                {
                    for (int i = x/2-1; i >= 0; i--)
                    {
                        // Assume that anything non-zero is a valid tile
                        if (_rooms[lastRoomDataID].connectionValues[y/2][i] != 0)
                        {
                            connects_to_other_room = false;
                            if ((_rooms[lastRoomDataID].connectionValues[y/2][i] & 0x80) > 0)
                            {
                                // Something's blocking the once-open space for this tile so need to update it
                                _rooms[lastRoomDataID].connectionValues[y/2][i] ^= 0x80;
                            }
                        }
                    }
                }

                if (connects_to_other_room)
                {
                    _rooms[lastRoomDataID].connectionValues[y/2][x/2] ^= 0x20;
                }
            }
        }
        private void ChangeTileDoorData(object? sender, EventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            int y = button.GetValue(Grid.RowProperty);
            int x = button.GetValue(Grid.ColumnProperty);
            if (y % 2 == 1)
            {
                // want rooms to the above and below button
                _rooms[lastRoomDataID].connectionValues[(y-1)/2][x/2] ^= 0x400;
                _rooms[lastRoomDataID].connectionValues[(y+1)/2][x/2] ^= 0x100;
            }
            else
            {
                // want rooms left and right of button
                _rooms[lastRoomDataID].connectionValues[y/2][(x-1)/2] ^= 0x800;
                _rooms[lastRoomDataID].connectionValues[y/2][(x+1)/2] ^= 0x200;
            }
        }
        private void UpdateRoomOutline(object? sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            byte value;
            if ( box.IsChecked == false)
            {
                value = 0;
            }
            else if ( box.IsChecked == true )
            {
                value = 1;
            }
            else
            {
                value = 2;
            }

            _rooms[lastRoomDataID].mapRamOutline[box.GetValue(Grid.RowProperty)][box.GetValue(Grid.ColumnProperty)] = value;
        }
        private void ChangeTileBackground(object? sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(sender as Control);

            MapTile panel = (MapTile)sender;
            int x = panel.GetValue(Grid.RowProperty) / 2;
            int y = panel.GetValue(Grid.ColumnProperty) / 2;

            // Since the reveal data is stuffed into the upper nybble of some other data,
            // at most a dungeon can have 16 unique textures per tile (hypothetical, haven't tested yet)
            if (point.Properties.IsLeftButtonPressed)
            {
                panel.value = (byte)((panel.value + 1) % 16);
            }
            else if (point.Properties.IsRightButtonPressed)
            {
                var newval = panel.value-1;
                if (newval < 0)
                {
                    newval = 15;
                }
                panel.value = (byte)((newval) % 16);
            }
            _rooms[lastRoomDataID].revealProperties[x][y] = (byte)(panel.value << 4);
            Avalonia.Media.BrushConverter bgColor = new();
            if (panel.value > 0)
            {

                Int32 color = 0xFFFFFF / 16;
                color = color * panel.value;
                panel.SetValue(MapTile.BackgroundProperty, bgColor.ConvertFrom("#" + (color.ToString("X").PadLeft(6))));
            }
            else
            {
                panel.SetValue(MapTile.BackgroundProperty, bgColor.ConvertFrom("#FFFFFF") );
            }
            
        }
        private void ChangeActiveTemplate(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (((NumericUpDown)sender).Value == null)
            {
                return;
            }
            lastTemplateID = (int)((NumericUpDown)sender).Value;
            if (lastTemplateID > _templates.Count-1)
            {
                ((NumericUpDown)sender).Text = "0";
                lastTemplateID = 0;
            }
            else if (lastTemplateID < 0)
            {
                ((NumericUpDown)sender).Text = (_templates.Count-1).ToString();
                lastTemplateID = _templates.Count-1;
            }
            Panel usedRooms = new();
            Panel usedRoomsEx = new();
            Panel unusedRooms = new();
            Panel usedExit = new();
            Panel unusedExit = new();
            foreach (Panel panel in _templateMenu.RegularRooms)
            {
                panel.Children.Clear();
                if (panel.Name == "UsedRooms")
                {
                    usedRooms = panel;
                }
                else if (panel.Name == "UsedRoomsEx")
                {
                    usedRoomsEx = panel;
                }
                else
                {
                    unusedRooms = panel;
                }
            }
            foreach (Panel panel in _templateMenu.ExitRooms)
            {
                panel.Children.Clear();
                if (panel.Name == "UsedExit")
                {
                    usedExit = panel;
                }
                else
                {
                    unusedExit = panel;
                }
            }

            TextBlock PlainText;
            for (int i = 0; i < _templates[lastTemplateID].roomExCount; i++)
            {
                int current = _templates[lastTemplateID].rooms[i];
                PlainText = new();
                PlainText.Text = current.ToString();
                PlainText.SetValue(TextBlock.MarginProperty, Avalonia.Thickness.Parse("8"));
                PlainText.PointerPressed+= TemplateDragBlock;
                if (_rooms[current].isExit)
                {
                    usedExit.Children.Add(PlainText);
                }
                else if (i >= _templates[lastTemplateID].roomCount)
                {
                    usedRoomsEx.Children.Add(PlainText);
                }
                else
                {
                    usedRooms.Children.Add(PlainText);
                }
            }

            for (byte i = 0; i < _rooms.Count; i++)
            {
                if (!_templates[lastTemplateID].rooms.Contains(i))
                {
                    PlainText = new();
                    PlainText.Text = i.ToString();
                    PlainText.SetValue(TextBlock.MarginProperty, Avalonia.Thickness.Parse("8"));
                    PlainText.PointerPressed+= TemplateDragBlock;
                    if (_rooms[i].isExit)
                    {
                        unusedExit.Children.Add(PlainText);
                    }
                    else
                    {
                        unusedRooms.Children.Add(PlainText);
                    }

                }
            }
            _templateMenu.FieldsThatUse.Children.Clear();
            foreach (var kvp in _dungeon_template_dict)
            {
                if (kvp.Value == lastTemplateID)
                {
                    PlainText = new();
                    PlainText.Text = kvp.Key.ToString();
                    PlainText.SetValue(TextBlock.MarginProperty, Avalonia.Thickness.Parse("8"));
                    _templateMenu.FieldsThatUse.Children.Add(PlainText);
                }
            }
        }
        private void AddToTemplateDict(object? sender, EventArgs e)
        {
            byte field_to_add = (byte)_templateMenu.FieldSelected.Value;
            try
            {
                _dungeon_template_dict.Add(field_to_add, (byte)lastTemplateID);
                TextBlock text = new();
                text.Text = field_to_add.ToString();
                text.SetValue(TextBlock.MarginProperty, Avalonia.Thickness.Parse("8"));
                _templateMenu.FieldsThatUse.Children.Add(text);
            }
            catch
            { 

            }
        }
        private void RemoveFromTemplateDict(object? sender, EventArgs e)
        {
            byte field_to_remove = (byte)_templateMenu.FieldSelected.Value;
            if (_dungeon_template_dict.Remove(field_to_remove))
            {
                foreach (TextBlock text in _templateMenu.FieldsThatUse.Children)
                {
                    if (text.Text == field_to_remove.ToString())
                    {
                        _templateMenu.FieldsThatUse.Children.Remove(text);
                        break;
                    }
                }
            }
        }
        private async void TemplateDragBlock(object? sender, PointerPressedEventArgs e)
        {
            var mousePos = e.GetPosition(this);
            // Might want to look into the ghost item thing a bit more, seems to be
            // for visuals
            // var ghostPos = GhostItem.Bounds.Position;
            
            var dragData = new DataObject();
            dragData.Set("ObjToMove", sender);
            var result = DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
        }
        private void Drop(object? sender, DragEventArgs e)
        {
            var obj = e.Data.Get("ObjToMove");
            if (obj is not TextBlock moveBlock)
            {
                return;
            }
            WrapPanel? old_parent = moveBlock.FindAncestorOfType<WrapPanel>();
            if (old_parent == null)
            {
                return;
            }

            WrapPanel? new_parent;
            if (e.Source is TextBlock targetBlock)
            {
                new_parent = targetBlock.FindAncestorOfType<WrapPanel>();
            }
            else if (e.Source is WrapPanel new_panel)
            {
                new_parent = new_panel;
            }
            else
            {
                return;
            }
            
            if (new_parent == null)
            {
                return;
            }
            if (new_parent == old_parent)
            {
                return;
            }

            if (_templateMenu.RegularRooms.Contains(old_parent) && _templateMenu.RegularRooms.Contains(new_parent))
            {
                old_parent.Children.Remove(moveBlock);
                new_parent.Children.Add(moveBlock);
                byte var1 = (byte)(int.Parse(moveBlock.Text));
                if (new_parent.Name == "UsedRooms")
                {
                    _templates[lastEncountID].rooms.Insert(_templates[lastEncountID].roomCount-1, var1 );
                    _templates[lastEncountID].roomCount++;
                    _templates[lastEncountID].roomExCount++;
                }
                else if (new_parent.Name == "UsedRoomsEx")
                {
                    _templates[lastEncountID].rooms.Insert(_templates[lastEncountID].roomExCount, var1);
                    _templates[lastEncountID].roomExCount++;
                }
                else
                {
                    // Remove from the list and decrement numbers
                    _templates[lastEncountID].rooms.Remove(var1);
                    if (old_parent.Name == "UsedRooms")
                    {
                        // Decrement both counters
                        _templates[lastEncountID].roomCount--;
                    }
                    _templates[lastEncountID].roomExCount--;
                }
            }
            else if (_templateMenu.ExitRooms.Contains(old_parent) && _templateMenu.ExitRooms.Contains(new_parent))
            {
                TextBlock old_exit = (TextBlock)new_parent.Children[0];
                old_parent.Children.Remove(moveBlock);
                new_parent.Children.Add(moveBlock);
                new_parent.Children.Remove(old_exit);
                old_parent.Children.Add(old_exit);
                byte var1 = (byte)(int.Parse(moveBlock.Text));
                byte var2 = (byte)(int.Parse(old_exit.Text));
                if (new_parent.Name == "UsedExit")
                {
                    _templates[lastEncountID].rooms.Insert(_templates[lastEncountID].roomCount, var1);
                    _templates[lastEncountID].rooms.Remove(var2);
                }
                else
                {
                    _templates[lastEncountID].rooms.Insert(_templates[lastEncountID].roomCount, var2);
                    _templates[lastEncountID].rooms.Remove(var1);
                }
            }

        }
        private void DragEnter(object? sender, DragEventArgs e)
        {
        }
        private void DragLeave(object? sender, DragEventArgs e)
        {
        }
    }
}
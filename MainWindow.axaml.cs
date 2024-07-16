using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System;

using DungeonBuilder.modules.internal_objects;
using System.IO;
using DungeonBuilder.modules.window_objects;

using System.Text.Json;
using Avalonia.Controls.Primitives;
using System.Collections.Generic;
using Avalonia.VisualTree;
using System.Linq;
using Avalonia.Input;

using System.Text;
using System.Net.Http.Json;
using System.Diagnostics.Metrics;
using Avalonia;
using static DungeonBuilder.modules.window_objects.LootTableMenu;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using Rectangle = Avalonia.Controls.Shapes.Rectangle;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using VerticalAlignment = Avalonia.Layout.VerticalAlignment;
using Path = System.IO.Path;
using Avalonia.Controls.Shapes;
using Avalonia.Logging;
using DungeonBuilder.modules.file_formats;
using System.Xml.Linq;

namespace DungeonBuilder
{

    public class MapTile : Panel
    {
        public byte value = 0;
    }

    public class MapCoordSelect : Rectangle
    {

    }

    public partial class MainWindow : Window
    {
        EncounterMenu _encounterMenu;
        EncounterTableMenu _encounterTableMenu;
        LootTableMenu _lootTableMenu;
        RoomDataMenu _roomDataMenu;
        MinimapMenu _minimapMenu;
        FloorMenu _floorMenu;
        TemplateMenu _templateMenu;

        List<EnemyEncounter> _enemyEncounters;
        List<FloorEncounter> _floorEncounters;
        List<LootTable> _lootTables;
        List<DungeonTemplates> _templates;
        List<DungeonFloor> _floors;
        List<DungeonRoom> _rooms;
        List<DungeonMinimap> _minimap;
        Dictionary<string, Bitmap> _minimapTiles;
        Dictionary<byte, byte> _dungeon_template_dict;

        bool project_loaded = false;
        int lastEncountID;
        int lastEncountTableID;
        int lastLootID;
        int lastMinimapID;
        int lastMinimapNameID;
        int lastRoomDataID;
        int lastFloorID;
        int lastTemplateID;
        string current_project_path;
        double cellsizeX;
        double cellsizeY;
        double rot;
        RadioButton ActiveButton;
        
        List<int> tile_colors = new List<int>() 
        { 0xFFFFFF, 0xFFA88D, 0x896AA7, 0xA3B5FD, 0x75CBE7, 0x7FBDAF, 0xEE7179, 0xF6DA6F,
          0xFFFFFF/2, 0xFFA88D/2, 0x896AA7/2, 0xA3B5FD/2, 0x75CBE7/2, 0x7FBDAF/2, 0xEE7179/2, 0xF6DA6F/2,};
        List<IBrush> palette = new List<IBrush> { Brush.Parse("#FEE117"), Brush.Parse("#FFEB18"), Brush.Parse("#EEDB15"),
                                                  Brush.Parse("#FFB403"), Brush.Parse("#646464"), Brush.Parse("#3D3D3D") };

        public static FilePickerFileType MinimapTile { get; } = new("Minimap tile image")
        {
            Patterns = ["*.png"],
            AppleUniformTypeIdentifiers = ["public.image"],
            MimeTypes = ["image/*"]
        };
        public MainWindow()
        {
            Logger.TryGet(LogEventLevel.Fatal, LogArea.Control)?.Log(this, "Avalonia Infrastructure");

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
                current_project_path = "";
                lastEncountID = 0;
                lastEncountTableID = 0;
                lastLootID = 0;
                lastMinimapID = 0;
                lastMinimapNameID = 0;
                lastRoomDataID = 0;
                lastFloorID = 0;
                lastTemplateID = 0;

                _encounterMenu = new();
                _encounterTableMenu = new();
                _lootTableMenu = new();
                // Room model menu here, when we get to it
                _roomDataMenu = new();
                _floorMenu = new();
                _minimapMenu = new();
                _templateMenu = new();
                _dungeon_template_dict = new();

                LoadJSON(Path.GetFullPath("JSON"));

                project_loaded = true;

                SaveProjectButton.IsEnabled = true;
                // Currently, Room Model button is marked as false, change that once we begin to tackle that side of things

                if (ActiveButton != null)
                {
                    ActiveButton.IsChecked = false;
                }
                SetupMinimapTileDictionaryNew();
                TMXHandler.Initialize();

                ActiveGrid.RowDefinitions.Clear();
                ActiveGrid.ColumnDefinitions.Clear();
                ActiveGrid.Children.Clear();
                ActiveGrid.SetValue(Grid.BackgroundProperty, palette[0]);
                Label success = new();
                success.SetValue(Label.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                success.SetValue(Label.VerticalAlignmentProperty, VerticalAlignment.Center);
                success.SetValue(Label.ContentProperty, "New project started! Select a button on the left to start editing.");
                ActiveGrid.Children.Add(success);
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
                    current_project_path = folder[0].Path.LocalPath.ToString();

                    lastEncountID = 0;
                    lastEncountTableID = 0;
                    lastLootID = 0;
                    lastRoomDataID = 0;
                    lastMinimapID = 0;
                    lastMinimapNameID = 0;
                    lastFloorID = 0;
                    lastTemplateID = 0;

                    _encounterMenu = new();
                    _encounterTableMenu = new();
                    _lootTableMenu = new();
                    // Room model menu here, when we get to it
                    _roomDataMenu = new();
                    _minimapMenu = new();
                    _floorMenu = new();
                    _templateMenu = new();
                    _dungeon_template_dict = new();

                    LoadJSON(Path.GetFullPath("JSON"));


                    project_loaded = true;

                    SaveProjectButton.IsEnabled = true;

                    if (ActiveButton != null)
                    {
                        ActiveButton.IsChecked = false;
                    }
                    SetupMinimapTileDictionaryLoad();
                    TMXHandler.Initialize();

                    ActiveGrid.RowDefinitions.Clear();
                    ActiveGrid.ColumnDefinitions.Clear();
                    ActiveGrid.Children.Clear();
                    ActiveGrid.SetValue(Grid.BackgroundProperty, palette[0]);
                    Label success = new();
                    success.SetValue(Label.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    success.SetValue(Label.VerticalAlignmentProperty, VerticalAlignment.Center);
                    success.SetValue(Label.ContentProperty, "Project loaded!");
                    ActiveGrid.Children.Add(success);
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
                    current_project_path = folder[0].Path.LocalPath.ToString();

                    var serializeOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                                                                       WriteIndented = true };

                    string jsonContents = JsonSerializer.Serialize(_floors, serializeOptions);
                    jsonContents.Replace("\u0027", "'");
                    var writer = File.Create(Path.Combine(current_project_path, "dungeon_floors.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();

                    jsonContents = JsonSerializer.Serialize(_dungeon_template_dict, serializeOptions);
                    writer = File.Create(Path.Combine(current_project_path, "dungeon_template_dict.json"));
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
                    writer = File.Create(Path.Combine(current_project_path, "dungeon_rooms.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();

                    jsonContents = JsonSerializer.Serialize(_templates, serializeOptions);
                    writer = File.Create(Path.Combine(current_project_path, "dungeon_templates.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();

                    jsonContents = JsonSerializer.Serialize(_minimap, serializeOptions);
                    writer = File.Create(Path.Combine(current_project_path, "dungeon_minimap.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();

                    jsonContents = JsonSerializer.Serialize(_enemyEncounters, serializeOptions);
                    writer = File.Create(Path.Combine(current_project_path, "encounters.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();

                    jsonContents = JsonSerializer.Serialize(_floorEncounters, serializeOptions);
                    writer = File.Create(Path.Combine(current_project_path, "encounter_tables.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();

                    jsonContents = JsonSerializer.Serialize(_lootTables, serializeOptions);
                    writer = File.Create(Path.Combine(current_project_path, "loot_tables.json"));
                    writer.Write(Encoding.UTF8.GetBytes(jsonContents));
                    writer.Close();

                    Directory.CreateDirectory(Path.Combine(current_project_path, "minimap_tiles"));
                    string png_path = Path.Combine(current_project_path, "minimap_tiles");
                    foreach (var entry in _minimap)
                    {
                        _minimapTiles[entry.name].Save(Path.Combine(png_path, entry.name.Replace(".tmx", ".png")));
                        TMXHandler.CreateTMX(_minimapTiles[entry.name], entry.name);
                        if (entry.multipleNames)
                        {
                            foreach (var name in entry.names)
                            {
                                _minimapTiles[name].Save(Path.Combine(png_path, name.Replace(".tmx", ".png")));
                                TMXHandler.CreateTMX(_minimapTiles[name], name);
                            }
                        }
                    }
                    BinaryWriter smapbin = new(File.OpenWrite(Path.Combine(current_project_path, "fld_smap.bin")));

                    TMXHandler.WriteSmap(smapbin);
                }
            }
        }
        private void LoadJSON(string path)
        {

            StreamReader jsonStream = new StreamReader(Path.Combine(path, "dungeon_floors.json"));
            string jsonContents = jsonStream.ReadToEnd();
            _floors = JsonSerializer.Deserialize<List<DungeonFloor>>(jsonContents)!;

            jsonStream = new StreamReader(Path.Combine(path, "dungeon_rooms.json"));
            jsonContents = jsonStream.ReadToEnd();
            _rooms = JsonSerializer.Deserialize<List<DungeonRoom>>(jsonContents)!;

            jsonStream = new StreamReader(Path.Combine(path, "dungeon_minimap.json"));
            jsonContents = jsonStream.ReadToEnd();
            _minimap = JsonSerializer.Deserialize<List<DungeonMinimap>>(jsonContents)!;

            jsonStream = new StreamReader(Path.Combine(path, "dungeon_templates.json"));
            jsonContents = jsonStream.ReadToEnd();
            _templates = JsonSerializer.Deserialize<List<DungeonTemplates>>(jsonContents)!;

            Dictionary<string, byte> temp;
            jsonStream = new StreamReader(Path.Combine(path, "dungeon_template_dict.json"));
            jsonContents = jsonStream.ReadToEnd();
            temp = JsonSerializer.Deserialize<Dictionary<string, byte>>(jsonContents)!;
            foreach (string key in temp.Keys)
            {
                _dungeon_template_dict.Add(Byte.Parse(key), temp[key]);
            }

            jsonStream = new StreamReader(Path.Combine(path, "encounters.json"));
            jsonContents = jsonStream.ReadToEnd();
            _enemyEncounters = JsonSerializer.Deserialize<List<EnemyEncounter>>(jsonContents)!;

            jsonStream = new StreamReader(Path.Combine(path, "encounter_tables.json"));
            jsonContents = jsonStream.ReadToEnd();
            _floorEncounters = JsonSerializer.Deserialize<List<FloorEncounter>>(jsonContents)!;

            jsonStream = new StreamReader(Path.Combine(path, "loot_tables.json"));
            jsonContents = jsonStream.ReadToEnd();
            _lootTables = JsonSerializer.Deserialize<List<LootTable>>(jsonContents)!;
        }
        private void SetupMinimapTileDictionaryNew()
        {
            string path;
            string file_name;
            Bitmap currentImage;
            _minimapTiles = new();
            // Loaded a new project file, use defaults
            path = "avares://P4G-Dungeon-Editor/Assets/minimap_tiles/";
            foreach (DungeonMinimap tile in _minimap)
            {
                if (tile.multipleNames)
                {
                    if (!_minimapTiles.ContainsKey(tile.name))
                    {
                        // smap00.png is a filler image, never accessed but needed (for now)
                        // to maintain order with how the game handles minimap textures
                        file_name = "smap00.png";
                        currentImage = new Bitmap(AssetLoader.Open(new Uri("avares://P4G-Dungeon-Editor/Assets/minimap_tiles/smap00.png")));
                        _minimapTiles.Add(tile.name, currentImage);
                    }
                    foreach (string name in tile.names)
                    {
                        if (!_minimapTiles.ContainsKey(name))
                        {
                            file_name = name.Replace(".tmx", ".png");
                            currentImage = new Bitmap(AssetLoader.Open(new Uri(path+file_name)));
                            _minimapTiles.Add(name, currentImage);
                        }
                    }
                }
                else
                {
                    if (!_minimapTiles.ContainsKey(tile.name))
                    {
                        file_name = tile.name.Replace(".tmx", ".png");
                        currentImage = new Bitmap(AssetLoader.Open(new Uri(path+file_name)));
                        _minimapTiles.Add(tile.name, currentImage);
                    }
                }
            }
        }
        private void SetupMinimapTileDictionaryLoad()
        {
            string path;
            string file_name;
            Bitmap currentImage;
            _minimapTiles = new();
            // Loaded a new project file, use defaults
            path = current_project_path+"/minimap_tiles/";
            foreach (DungeonMinimap tile in _minimap)
            {
                if (!_minimapTiles.ContainsKey(tile.name))
                {
                    // smap00.png is a filler image, never accessed but needed (for now)
                    // to maintain order with how the game handles minimap textures
                    currentImage = new Bitmap(AssetLoader.Open(new Uri("avares://P4G-Dungeon-Editor/Assets/minimap_tiles/smap00.png")));
                    _minimapTiles.Add(tile.name, currentImage);
                }
                if (tile.multipleNames)
                {
                    foreach (string name in tile.names)
                    {
                        if (!_minimapTiles.ContainsKey(name))
                        {
                            file_name = name.Replace(".tmx", ".png");
                            currentImage = new Bitmap(File.OpenRead(path+file_name));
                            _minimapTiles.Add(name, currentImage);
                        }
                    }
                }
                else
                {
                    if (!_minimapTiles.ContainsKey(tile.name))
                    {
                        file_name = tile.name.Replace(".tmx", ".png");
                        currentImage = new Bitmap(File.OpenRead(path+file_name));
                        _minimapTiles.Add(tile.name, currentImage);
                    }
                }
            }
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
            ActiveButton = (RadioButton)sender;
            if (sender == EncounterButton)
            {
                StackPanel PairContents;
                MaskedTextBox WritableTextbox;
                TextBlock PlainText;
                
                ActiveGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(5, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.SetValue(Grid.BackgroundProperty, palette[0]);


                InnerGrid = new();
                InnerGrid.SetValue(Grid.RowProperty, 0);
                InnerGrid.SetValue(Grid.ColumnProperty, 1);
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                ActiveGrid.Children.Add(InnerGrid);

                Button addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 0);
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 10 0 0"));
                addsub.Content = "+";
                addsub.Click += AddEncounterEntry;
                InnerGrid.Children.Add(addsub);
                addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 1);
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 0 0 10"));
                addsub.Content = "-";
                addsub.Click += RemoveEncounterEntry;
                InnerGrid.Children.Add(addsub);

                // https://docs.avaloniaui.net/docs/reference/controls/numericupdown
                NumericUpDown EncountSelector = new();
                ItemOutline = new();
                EncountSelector.SetValue(NumericUpDown.ValueProperty, lastEncountID);
                EncountSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                EncountSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                EncountSelector.SetValue(NumericUpDown.MaximumProperty, _enemyEncounters.Count-1);
                EncountSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);

                EncountSelector.ValueChanged+=ChangeActiveEncounter;
                _encounterMenu.EncounterID = EncountSelector;
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Grid.RowSpanProperty, 2);
                ItemOutline.Child = EncountSelector;
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[1]);
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10") );
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8") );
                InnerGrid.Children.Add(ItemOutline);

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
                ItemOutline.SetValue(Border.BackgroundProperty, palette[3]);
                ItemOutline.SetValue(Border.MarginProperty, Thickness.Parse("5 0 5 5"));
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
                            ItemOutline.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, palette[2]);
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3"));
                            ItemContainer.Children.Add(ItemOutline);

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, palette[1]);
                                Identifier.SetValue(TextBlock.TextProperty, "Unit "+(i+1));
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 64);
                                ItemOutline.Child = Identifier;

                            WritableTextbox = new();
                            WritableTextbox.SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].Units[i].ToString());
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
                ItemOutline.SetValue(Border.BackgroundProperty, palette[3]);
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
                            ItemOutline.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, palette[2]);
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, palette[1]);
                                Identifier.SetValue(TextBlock.TextProperty, "Flags");
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72);
                                ItemOutline.Child = Identifier;


                            ItemContainer.Children.Add(ItemOutline);

                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _enemyEncounters[lastEncountID].Flags.ToString());
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
                            ItemOutline.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, palette[2]);
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, palette[1]);
                                Identifier.SetValue(TextBlock.TextProperty, "Music ID");
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72);
                                ItemOutline.Child = Identifier;

                            ItemContainer.Children.Add(ItemOutline);

                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _enemyEncounters[lastEncountID].MusicID.ToString());
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
                            ItemOutline.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, palette[2]);
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, palette[1]);
                                Identifier.SetValue(TextBlock.TextProperty, "Field ID");
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72);
                                ItemOutline.Child = Identifier;


                            ItemContainer.Children.Add(ItemOutline);

                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _enemyEncounters[lastEncountID].FieldID.ToString());
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
                            ItemOutline.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, palette[2]);
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );
                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, palette[1]);
                                Identifier.SetValue(TextBlock.TextProperty, "Room ID");
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72);
                                ItemOutline.Child = Identifier;
                
                
                            ItemContainer.Children.Add(ItemOutline);

                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _enemyEncounters[lastEncountID].RoomID.ToString());
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
                            ItemOutline.SetValue(DockPanel.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, palette[2]);
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, palette[1]);
                                Identifier.SetValue(TextBlock.TextProperty, "Field04");
                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72);
                                ItemOutline.Child = Identifier;
                
                            ItemContainer.Children.Add(ItemOutline);
                

                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _enemyEncounters[lastEncountID].Field04.ToString());
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
                            ItemOutline.SetValue(DockPanel.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                            ItemOutline.SetValue(Border.BorderBrushProperty, palette[2]);
                            ItemOutline.SetValue(Border.BorderThicknessProperty, Thickness.Parse("3") );

                                Identifier = new();
                                Identifier.SetValue(TextBlock.PaddingProperty, Thickness.Parse("10 20 10 0"));
                                Identifier.SetValue(TextBlock.BackgroundProperty, palette[1]);
                                Identifier.SetValue(TextBlock.TextProperty, "Field06");

                                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                                Identifier.SetValue(TextBlock.WidthProperty, 72); 
                                ItemOutline.Child = Identifier;
                
                
                            ItemContainer.Children.Add(ItemOutline);
                            TextBoxInput = new();
                            
                            TextBoxInput.SetValue(TextBlock.TextProperty, _enemyEncounters[lastEncountID].Field06.ToString());
                            TextBoxInput.TextChanged += ChangeEncounterData;
                            _encounterMenu.Field06 = TextBoxInput;
                            ItemContainer.Children.Add(TextBoxInput);

            }
            else if (sender == EncounterTableButton)
            {

                StackPanel PairContents;
                MaskedTextBox WritableTextbox;
                ActiveGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(5, GridUnitType.Star));
                ActiveGrid.SetValue(Grid.BackgroundProperty, palette[0]);

                InnerGrid = new();
                InnerGrid.SetValue(Grid.RowProperty, 0);
                InnerGrid.SetValue(Grid.ColumnProperty, 1);
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));


                ToggleButton button = new();
                button.SetValue(Grid.ColumnProperty, 0);
                button.SetValue(ToggleButton.BackgroundProperty, palette[1]);
                button.SetValue(ToggleButton.CornerRadiusProperty, CornerRadius.Parse("8"));
                button.SetValue(ToggleButton.MarginProperty, Avalonia.Thickness.Parse("60 15"));
                button.SetValue(ToggleButton.VerticalAlignmentProperty, VerticalAlignment.Stretch);
                button.SetValue(ToggleButton.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                button.SetValue(ToggleButton.ContentProperty, "Is Raining");
                button.SetValue(ToggleButton.VerticalContentAlignmentProperty, VerticalAlignment.Center);
                button.SetValue(ToggleButton.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                button.IsCheckedChanged += ChangeEncounterTableData;
                _encounterTableMenu.IsRainy = button;
                InnerGrid.Children.Add(button);


                InnerGrid2 = new();
                InnerGrid2.SetValue(Grid.ColumnProperty, 1);
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                InnerGrid.Children.Add(InnerGrid2);

                Button addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 0);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 10 0 0"));
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.Content = "+";
                addsub.Click += AddFloorEncounterTable;
                InnerGrid2.Children.Add(addsub);
                addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 1);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 0 0 10"));
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.Content = "-";
                addsub.Click += RemoveFloorEncounterTable;
                InnerGrid2.Children.Add(addsub);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Grid.RowSpanProperty, 2);
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[1]);
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid2.Children.Add(ItemOutline);

                NumericUpDown EncountSelector = new();
                EncountSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                EncountSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                EncountSelector.SetValue(NumericUpDown.MaximumProperty, _floorEncounters.Count-1);
                EncountSelector.SetValue(NumericUpDown.ValueProperty, lastEncountTableID);
                EncountSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);
                ItemOutline.Child = EncountSelector;
                EncountSelector.ValueChanged += ChangeActiveEncounterTable;
                _encounterTableMenu.EncountTable = EncountSelector;

                ActiveGrid.Children.Add(InnerGrid);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Border.BackgroundProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("5"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("5"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                ActiveGrid.Children.Add(ItemOutline);

                InnerGrid = new();
                ItemOutline.Child = InnerGrid;

                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(4, GridUnitType.Star));

                InnerGrid.ColumnDefinitions.Add(new(2, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(2, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(2, GridUnitType.Star));

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[2]);
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                PairContents = new();
                ItemOutline.Child = PairContents;


                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                WritableTextbox = new();
                Identifier = new();
                Identifier.SetValue(TextBlock.TextProperty, "Weight (Normal)");
                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                PairContents.Children.Add(Identifier);


                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].NormalWeightRegular.ToString());

                WritableTextbox.TextChanged += ChangeEncounterTableData;
                _encounterTableMenu.NormalWeight = WritableTextbox;
                PairContents.Children.Add(WritableTextbox);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 2);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[2]);
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                PairContents = new();
                ItemOutline.Child = PairContents;


                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                WritableTextbox = new();
                Identifier = new();
                Identifier.SetValue(TextBlock.TextProperty, "Weight (Rare)");
                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                PairContents.Children.Add(Identifier);

                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RareWeightRegular.ToString());
                WritableTextbox.TextChanged += ChangeEncounterTableData;
                _encounterTableMenu.RareWeight = WritableTextbox;
                PairContents.Children.Add(WritableTextbox);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 4);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[2]);
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                PairContents = new();
                ItemOutline.Child = PairContents;
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);

                WritableTextbox = new();
                Identifier = new();
                Identifier.SetValue(TextBlock.TextProperty, "Weight (Gold)");
                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                PairContents.Children.Add(Identifier);

                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].GoldWeightRegular.ToString());

                _encounterTableMenu.GoldWeight = WritableTextbox;
                PairContents.Children.Add(WritableTextbox);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[2]);
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                PairContents = new();
                ItemOutline.Child = PairContents;


                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                WritableTextbox = new();
                Identifier = new();
                Identifier.SetValue(TextBlock.TextProperty, "Always 0xFF");
                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                PairContents.Children.Add(Identifier);


                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].AlwaysFF.ToString());

                WritableTextbox.TextChanged += ChangeEncounterTableData;
                _encounterTableMenu.AlwaysFF = WritableTextbox;
                PairContents.Children.Add(WritableTextbox);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Grid.ColumnProperty, 2);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[2]);
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                PairContents = new();
                ItemOutline.Child = PairContents;


                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                WritableTextbox = new();
                Identifier = new();
                Identifier.SetValue(TextBlock.TextProperty, "% of usage (Rare)");
                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                PairContents.Children.Add(Identifier);

                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].PercentRare.ToString());
                WritableTextbox.TextChanged += ChangeEncounterTableData;
                _encounterTableMenu.RarePercent = WritableTextbox;
                PairContents.Children.Add(WritableTextbox);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Grid.ColumnProperty, 4);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[2]);
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);

                PairContents = new();
                ItemOutline.Child = PairContents;
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);

                WritableTextbox = new();
                Identifier = new();
                Identifier.SetValue(TextBlock.TextProperty, "% of usage (Gold)");
                Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                PairContents.Children.Add(Identifier);

                WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                WritableTextbox.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].PercentGold.ToString());


                WritableTextbox.TextChanged += ChangeEncounterTableData;
                        _encounterTableMenu.GoldPercent = WritableTextbox;
                        PairContents.Children.Add(WritableTextbox);
                ScrollViewer EncountContainer = new();
                EncountContainer.SetValue(Grid.RowProperty, 2);
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
                    InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                    InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                    InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    if (counter < 20)
                    {
                        // Regular encounter selection
                        InnerGrid2.SetValue(Grid.BackgroundProperty, palette[4]);
                    }
                    else if (counter < 25)
                    {
                        // Rare encounter selection
                        InnerGrid2.SetValue(Grid.BackgroundProperty, Brush.Parse("#AD1C0F"));
                    }
                    else
                    {
                        // Gold hand encounter selection
                        InnerGrid2.SetValue(Grid.BackgroundProperty, Brush.Parse("#AA8708"));
                    }
                    InnerGrid2.SetValue(MarginProperty, Thickness.Parse("0 5"));
                    for (int j = 0; j < 5; j++)
                    {

                        PairContents = new();
                        PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                        PairContents.SetValue(Grid.RowProperty, 0);
                        PairContents.SetValue(Grid.ColumnProperty, j);
                        Identifier = new();
                        Identifier.SetValue(TextBlock.TextProperty, "Encounter "+(counter));
                        Identifier.SetValue(TextBlock.MarginProperty, Thickness.Parse("4, 0"));
                        Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                        Identifier.SetValue(TextBlock.FontStyleProperty, FontStyle.Oblique);
                        PairContents.Children.Add(Identifier);

                        WritableTextbox = new();

                        if (counter <= 20)
                        {
                            // Regular encounter selection
                            WritableTextbox.SetValue(TextBlock.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersNormal[(counter-1)%20][0].ToString());
                            _encounterTableMenu.NormalEncounters[(counter-1)%20] = WritableTextbox;
                        }
                        else if (counter <= 25)
                        {
                            // Rare encounter selection
                            WritableTextbox.SetValue(TextBlock.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersRare[(counter-1)%5][0].ToString());
                            _encounterTableMenu.RareEncounters[(counter-1)%5] = WritableTextbox;
                        }
                        else
                        {
                            // Gold hand encounter selection
                            WritableTextbox.SetValue(TextBlock.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersGold[(counter-1)%5][0].ToString());
                            _encounterTableMenu.GoldEncounters[(counter-1)%5] = WritableTextbox;
                        }
                        WritableTextbox.SetValue(TextBlock.MarginProperty, Thickness.Parse("4, 0"));
                        WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                        WritableTextbox.TextChanged += ChangeEncounterTableData;
                        PairContents.Children.Add(WritableTextbox);

                        InnerGrid2.Children.Add(PairContents);

                        PairContents = new();
                        PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                        PairContents.SetValue(Grid.RowProperty, 1);
                        PairContents.SetValue(Grid.ColumnProperty, j);
                        Identifier = new();
                        Identifier.SetValue(TextBlock.TextProperty, "Weight");
                        Identifier.SetValue(TextBlock.MarginProperty, Thickness.Parse("4, 0"));
                        Identifier.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                        Identifier.SetValue(TextBlock.FontStyleProperty, FontStyle.Oblique);
                        PairContents.Children.Add(Identifier);

                        WritableTextbox = new();
                        WritableTextbox.SetValue(TextBlock.MarginProperty, Thickness.Parse("4, 0"));
                        WritableTextbox.SetValue(MaskedTextBox.TextAlignmentProperty, TextAlignment.Center);
                        WritableTextbox.TextChanged += ChangeEncounterTableData;
                        if (counter <= 20)
                        {
                            // Regular encounter selection
                            WritableTextbox.SetValue(TextBlock.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersNormal[(counter-1)%20][1].ToString());
                            _encounterTableMenu.NormalEncounterWeights[(counter-1)%20] = WritableTextbox;
                        }
                        else if (counter <= 25)
                        {
                            // Rare encounter selection
                            WritableTextbox.SetValue(TextBlock.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersRare[(counter-1)%5][1].ToString());
                            _encounterTableMenu.RareEncounterWeights[(counter-1)%5] = WritableTextbox;
                        }
                        else
                        {
                            // Gold hand encounter selection
                            WritableTextbox.SetValue(TextBlock.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersGold[(counter-1)%5][1].ToString());
                            _encounterTableMenu.GoldEncounterWeights[(counter-1)%5] = WritableTextbox;
                        }
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
                ActiveGrid.SetValue(Grid.BackgroundProperty, palette[0]);

                InnerGrid = new();
                InnerGrid.SetValue(Grid.RowProperty, 0);
                InnerGrid.SetValue(Grid.ColumnProperty, 1);
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                ActiveGrid.Children.Add(InnerGrid);

                Button addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 0);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 10 0 0"));
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.Content = "+";
                addsub.Click += AddLootTable;
                InnerGrid.Children.Add(addsub);
                addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 1);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 0 0 10"));
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.Content = "-";
                addsub.Click += RemoveLootTable;
                InnerGrid.Children.Add(addsub);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Grid.RowSpanProperty, 2);
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[1]);
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                // https://docs.avaloniaui.net/docs/reference/controls/numericupdown
                NumericUpDown LootSelector = new();
                LootSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                LootSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                LootSelector.SetValue(NumericUpDown.MaximumProperty, _lootTables.Count-1);
                LootSelector.SetValue(NumericUpDown.ValueProperty, lastLootID);
                LootSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);
                LootSelector.ValueChanged+=ChangeActiveLootTable;
                _lootTableMenu.LootID = LootSelector;
                ItemOutline.Child = LootSelector;

                InnerGrid.Children.Add(ItemOutline);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Grid.ColumnSpanProperty, 2);
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("3"));
                ItemOutline.Child = EntryShell;
                EntryShell.Content = ActiveEntries;
                ActiveEntries.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Vertical);
                ActiveGrid.Children.Add(ItemOutline);
                _lootTableMenu.Entries = new List<LootTableEntry>();
                for (int i = 0; i < _lootTables[lastLootID].LootEntries.Count;  i++)
                {
                    LootTableMenu.LootTableEntry entry = new();
                    InnerGrid = new();
                    InnerGrid.ColumnDefinitions.Add(new(2, GridUnitType.Star));
                    InnerGrid.ColumnDefinitions.Add(new(2, GridUnitType.Star));
                    InnerGrid.ColumnDefinitions.Add(new(2, GridUnitType.Star));
                    InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                    if (i % 2 == 0)
                    {
                        InnerGrid.SetValue(Grid.BackgroundProperty, palette[1]);
                    }
                    else
                    {
                        InnerGrid.SetValue(Grid.BackgroundProperty, palette[2]);
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

                    WritableTextbox.Text = _lootTables[lastLootID].LootEntries[i].ItemWeight.ToString();
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


                    WritableTextbox.Text = _lootTables[lastLootID].LootEntries[i].ItemID.ToString();
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

                    WritableTextbox.Text = _lootTables[lastLootID].LootEntries[i].ChestFlags.ToString();
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
                    ChestTypeToggle.IsChecked = _lootTables[lastLootID].LootEntries[i].ChestModel > 0;
                    ChestTypeToggle.SetValue(ToggleButton.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                    ChestTypeToggle.SetValue(ToggleButton.VerticalAlignmentProperty, VerticalAlignment.Center);
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
                ActiveGrid.SetValue(Grid.BackgroundProperty, palette[0]);

                InnerGrid = new();
                InnerGrid.SetValue(Grid.ColumnProperty, 0);
                InnerGrid.SetValue(Grid.RowProperty, 0);
                InnerGrid.SetValue(Grid.ColumnSpanProperty, 3);
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.Children.Add(InnerGrid);

                InnerGrid2 = new();
                InnerGrid2.SetValue(Grid.ColumnProperty, 1);
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                InnerGrid.Children.Add(InnerGrid2);

                Button addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 0);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 10 0 0"));
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.Content = "+";
                addsub.Click += AddRoom;
                InnerGrid2.Children.Add(addsub);
                addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 1);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 0 0 10"));
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.Content = "-";
                addsub.Click += RemoveRoom;
                InnerGrid2.Children.Add(addsub);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Grid.RowSpanProperty, 2);
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[1]);
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid2.Children.Add(ItemOutline);

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
                PairContents.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
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
                PairContents.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                PairContents.SetValue(Panel.MarginProperty, Avalonia.Thickness.Parse("4"));
                PairContents2.Children.Add(PairContents);

                InnerGrid.Children.Add(PairContents2);

                PairContents = new();
                PairContents.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Horizontal);
                PairContents.SetValue(Grid.RowProperty, 2);
                PlainText = new();
                Toggle = new();
                PlainText.Text = "Has Doors";
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
                PairContents.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                PairContents.SetValue(Panel.MarginProperty, Avalonia.Thickness.Parse("4"));
                InnerGrid.Children.Add(PairContents);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 4);
                ItemOutline.SetValue(Border.BackgroundProperty, palette[3]);
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
                OutlineGrid.SetValue(UniformGrid.HorizontalAlignmentProperty, HorizontalAlignment.Center);
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
                ActiveGrid.SetValue(Grid.BackgroundProperty, palette[0]);

                InnerGrid = new();
                InnerGrid.SetValue(Grid.RowProperty, 0);
                InnerGrid.SetValue(Grid.ColumnProperty, 1);
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                ActiveGrid.Children.Add(InnerGrid);

                Button addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 0);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 10 0 0"));
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.Content = "+";
                addsub.Click += AddFloorEntry;
                InnerGrid.Children.Add(addsub);
                addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 1);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 0 0 10"));
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.Content = "-";
                addsub.Click += RemoveFloorEntry;
                InnerGrid.Children.Add(addsub);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowSpanProperty, 2);
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[1]);
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
                _floorMenu.FloorID = FloorSelector;
                InnerGrid.Children.Add(ItemOutline);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[1]);
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
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[2]);
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
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[2]);
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
                WritableTextbox.Text = _floors[lastFloorID].EncountTableLookup.ToString();
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
                WritableTextbox.Text = _floors[lastFloorID].LootTableLookup.ToString();
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
                WritableTextbox.Text = _floors[lastFloorID].MaxChestCount.ToString();
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
                WritableTextbox.Text = _floors[lastFloorID].InitialEncounterCount.ToString();
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
                WritableTextbox.Text = _floors[lastFloorID].MinEncounterCount.ToString();
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
                ActiveGrid.SetValue(Grid.BackgroundProperty, palette[0]);

                InnerGrid = new();
                InnerGrid.SetValue(Grid.RowProperty, 0);
                InnerGrid.SetValue(Grid.ColumnProperty, 1);
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                ActiveGrid.Children.Add(InnerGrid);

                Button addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 0);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 10 0 0"));
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.Content = "+";
                addsub.Click += AddTemplateEntry;
                InnerGrid.Children.Add(addsub);
                addsub = new();
                addsub.SetValue(Grid.ColumnProperty, 0);
                addsub.SetValue(Grid.RowProperty, 1);
                addsub.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                addsub.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                addsub.SetValue(Button.MarginProperty, Thickness.Parse("0 0 0 10"));
                addsub.SetValue(Button.BackgroundProperty, palette[3]);
                addsub.Content = "-";
                addsub.Click += RemoveTemplateEntry;
                InnerGrid.Children.Add(addsub);

                ItemOutline = new();
                InnerGrid.Children.Add(ItemOutline);
                ItemOutline.SetValue(Grid.RowSpanProperty, 2);
                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[1]);
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                NumericUpDown TemplateSelector = new();
                ItemOutline.Child = TemplateSelector;
                TemplateSelector.SetValue(NumericUpDown.ValueProperty, lastTemplateID);
                TemplateSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                TemplateSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                TemplateSelector.SetValue(NumericUpDown.MaximumProperty, _templates.Count-1);
                TemplateSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);
                TemplateSelector.ValueChanged+=ChangeActiveTemplate;
                _templateMenu.TemplateID = TemplateSelector;


                InnerGrid2 = new();
                InnerGrid2.SetValue(Grid.MarginProperty, Avalonia.Thickness.Parse("4"));
                InnerGrid2.SetValue(Grid.RowProperty, 1);
                InnerGrid2.SetValue(Grid.ColumnSpanProperty, 2);
                InnerGrid2.ColumnDefinitions.Add(new(10, GridUnitType.Star));
                InnerGrid2.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                ActiveGrid.Children.Add(InnerGrid2);

                ItemOutline = new();
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BackgroundProperty, palette[3]);
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
                panel1.SetValue(Panel.BackgroundProperty, palette[1]);
                panel1.SetValue(Panel.NameProperty, "UsedRooms");
                ScrollViewer view2 = new();
                WrapPanel panel2 = new();
                view2.SetValue(Grid.RowProperty, 1);
                view2.SetValue(Grid.ColumnProperty, 0);
                view2.SetValue(Grid.ColumnSpanProperty, 2);
                panel2.SetValue(Panel.BackgroundProperty, palette[1]);
                panel2.SetValue(Panel.NameProperty, "UnusedRooms");

                ScrollViewer view3 = new();
                WrapPanel panel3 = new();
                view3.SetValue(Grid.RowProperty, 0);
                view3.SetValue(Grid.ColumnProperty, 1);
                panel3.SetValue(Panel.BackgroundProperty, palette[1]);
                
                panel3.SetValue(Panel.NameProperty, "UsedRoomsEx");

                InnerGrid.Children.Add(view1);
                InnerGrid.Children.Add(view2);
                InnerGrid.Children.Add(view3);



                ItemOutline = new();
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Border.BackgroundProperty, palette[3]);
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
                panel5.SetValue(Panel.BackgroundProperty, palette[2]);
                panel5.SetValue(Panel.NameProperty, "UsedExit");
                ScrollViewer view6 = new();
                WrapPanel panel6 = new();
                view6.SetValue(Grid.RowProperty, 1);
                panel6.SetValue(Panel.BackgroundProperty, palette[2]);
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
                ItemOutline.SetValue(Border.BackgroundProperty, palette[3]);
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
                TemplateSelector = new();
                TemplateSelector.SetValue(NumericUpDown.NameProperty, "FieldSelected");
                TemplateSelector.SetValue(NumericUpDown.ValueProperty, 0);
                TemplateSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                TemplateSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                TemplateSelector.SetValue(NumericUpDown.MaximumProperty, 255);
                _templateMenu.FieldSelected = TemplateSelector;
                PairContents.Children.Add(TemplateSelector);
                StackPanel PairContents2 = new();
                button = new();
                button.Content = "+";
                button.SetValue(StackPanel.MarginProperty, Avalonia.Thickness.Parse("4"));
                button.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                button.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                button.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
                button.Click+=AddToTemplateDict;
                PairContents2.Children.Add(button);

                button = new();
                button.Content = "-";
                button.SetValue(StackPanel.MarginProperty, Avalonia.Thickness.Parse("4"));
                button.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                button.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                button.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
                button.Click+=RemoveFromTemplateDict;
                PairContents2.Children.Add(button);
                PairContents2.SetValue(StackPanel.OrientationProperty, Avalonia.Layout.Orientation.Horizontal);
                PairContents.Children.Add(PairContents2);
                PairContents.SetValue(Grid.ColumnProperty, 1);
                InnerGrid.Children.Add(PairContents);

                ActiveGrid.Children.Add(InnerGrid);

            }
            else if (sender == MinimapButton)
            {

                ActiveGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.RowDefinitions.Add(new(5, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                ActiveGrid.SetValue(Grid.BackgroundProperty, palette[0]);

                InnerGrid = new();
                InnerGrid.SetValue(Grid.RowProperty, 0);
                InnerGrid.SetValue(Grid.ColumnProperty, 1);
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                ActiveGrid.Children.Add(InnerGrid);

                Button LoadImage = new();
                LoadImage.SetValue(Grid.RowProperty, 0);
                LoadImage.SetValue(Grid.ColumnProperty, 0);
                LoadImage.SetValue(Button.BackgroundProperty, palette[3]);
                LoadImage.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
                LoadImage.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                LoadImage.SetValue(Button.ContentProperty, "Load new minimap image");
                LoadImage.Click += LoadNewMinimapImage;
                ActiveGrid.Children.Add(LoadImage);

                NumericUpDown MinimapTileSelector = new();
                ItemOutline = new();
                MinimapTileSelector.ValueChanged+=ChangeActiveMinimap;
                _minimapMenu.MinimapID = MinimapTileSelector;
                MinimapTileSelector.SetValue(NumericUpDown.ValueProperty, lastMinimapID+1);
                MinimapTileSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                MinimapTileSelector.SetValue(NumericUpDown.MinimumProperty, 1);
                MinimapTileSelector.SetValue(NumericUpDown.MaximumProperty, _minimap.Count);
                MinimapTileSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Right);

                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Grid.RowSpanProperty, 2);
                ItemOutline.Child = MinimapTileSelector;
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[1]);
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("10"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("8"));
                InnerGrid.Children.Add(ItemOutline);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("2"));
                ActiveGrid.Children.Add(ItemOutline);

                InnerGrid2 = new();
                ItemOutline.Child = InnerGrid2;
                InnerGrid2.RowDefinitions.Add(new(3, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));

                Panel roomBg = new();
                roomBg.SetValue(Panel.BackgroundProperty, palette[5]);
                InnerGrid2.Children.Add(roomBg);



                Bitmap currentImage;
                Image currentRoom = new();
                _minimapMenu.CurrentTileTexCoord = currentRoom;
                roomBg.Children.Add(currentRoom);
                if (_minimap[lastMinimapID].multipleNames)
                {

                    if (!_minimapTiles.TryGetValue(_minimap[lastMinimapID].names[lastMinimapNameID], out currentImage))
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    if (!_minimapTiles.TryGetValue(_minimap[lastMinimapID].name, out currentImage))
                    {
                        throw new Exception();
                    }
                }

                double img_width = currentImage.Size.Width;
                double img_height = currentImage.Size.Height;

                currentRoom.SetValue(Grid.RowProperty, 0);
                currentRoom.SetValue(Image.SourceProperty, currentImage);
                currentRoom.SetValue(Image.StretchProperty, Stretch.Uniform);
                currentRoom.SetValue(Grid.ZIndexProperty, 0);

                currentRoom.Loaded += LoadMinimapSectionOutlines;

                Grid minimapOptions = new();
                minimapOptions.SetValue(Grid.RowProperty, 1);
                minimapOptions.SetValue(Grid.BackgroundProperty, palette[2]);
                minimapOptions.RowDefinitions.Add(new(1, GridUnitType.Star));
                minimapOptions.RowDefinitions.Add(new(1, GridUnitType.Star));
                minimapOptions.ColumnDefinitions.Add(new(3, GridUnitType.Star));
                minimapOptions.ColumnDefinitions.Add(new(2, GridUnitType.Star));
                minimapOptions.ColumnDefinitions.Add(new(2, GridUnitType.Star));
                InnerGrid2.Children.Add(minimapOptions);

                MinimapTileSelector = new();
                MinimapTileSelector.ValueChanged+=ChangeMinimapNameID;
                _minimapMenu.MinimapTileNameSelection = MinimapTileSelector;
                ItemOutline = new();
                MinimapTileSelector.SetValue(NumericUpDown.ValueProperty, lastMinimapNameID);
                MinimapTileSelector.SetValue(NumericUpDown.IncrementProperty, 1);
                MinimapTileSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                MinimapTileSelector.SetValue(NumericUpDown.MaximumProperty, _minimap[lastMinimapID].multipleNames ? _minimap[lastMinimapID].names.Count-1 : 0);
                MinimapTileSelector.SetValue(NumericUpDown.IsEnabledProperty, _minimap[lastMinimapID].multipleNames);
                MinimapTileSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Center);
                MinimapTileSelector.SetValue(NumericUpDown.VerticalContentAlignmentProperty, VerticalAlignment.Center);

                ItemOutline.SetValue(Grid.RowProperty, 0);
                ItemOutline.SetValue(Grid.ColumnProperty, 0);
                ItemOutline.SetValue(Grid.RowSpanProperty, 2);
                ItemOutline.Child = MinimapTileSelector;
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("1"));
                ItemOutline.SetValue(Border.BackgroundProperty, palette[1]);
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("4 20"));
                ItemOutline.SetValue(Border.CornerRadiusProperty, CornerRadius.Parse("3"));
                minimapOptions.Children.Add(ItemOutline);

                double startx;
                double endx;
                double starty;
                double endy;
                if (_minimap[lastMinimapID].multipleNames)
                {
                    startx = _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0];
                    starty = _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1];
                    endx = _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][0];
                    endy = _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][1];
                }
                else
                {
                    startx = _minimap[lastMinimapID].texCoordSingle[0][0];
                    starty = _minimap[lastMinimapID].texCoordSingle[0][1];
                    endx = _minimap[lastMinimapID].texCoordSingle[1][0];
                    endy = _minimap[lastMinimapID].texCoordSingle[1][1];
                }

                MinimapTileSelector = new();
                MinimapTileSelector.ValueChanged+=ChangeMinimapTexCoord;
                _minimapMenu.MinimapTexCoordStartX = MinimapTileSelector;
                MinimapTileSelector.SetValue(Grid.RowProperty, 0);
                MinimapTileSelector.SetValue(Grid.ColumnProperty, 1);
                MinimapTileSelector.SetValue(NumericUpDown.ValueProperty, (decimal)startx);
                MinimapTileSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                MinimapTileSelector.SetValue(NumericUpDown.MaximumProperty, 1);
                MinimapTileSelector.SetValue(NumericUpDown.ShowButtonSpinnerProperty, false);
                MinimapTileSelector.SetValue(NumericUpDown.FontSizeProperty, 12);
                MinimapTileSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Center);
                MinimapTileSelector.SetValue(NumericUpDown.MarginProperty, Thickness.Parse("4 8"));
                minimapOptions.Children.Add(MinimapTileSelector);

                MinimapTileSelector = new();
                MinimapTileSelector.ValueChanged+=ChangeMinimapTexCoord;
                _minimapMenu.MinimapTexCoordEndX = MinimapTileSelector;
                MinimapTileSelector.SetValue(Grid.RowProperty, 1);
                MinimapTileSelector.SetValue(Grid.ColumnProperty, 1);
                MinimapTileSelector.SetValue(NumericUpDown.ValueProperty, (decimal)endx);
                MinimapTileSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                MinimapTileSelector.SetValue(NumericUpDown.MaximumProperty, 1);
                MinimapTileSelector.SetValue(NumericUpDown.ShowButtonSpinnerProperty, false);
                MinimapTileSelector.SetValue(NumericUpDown.FontSizeProperty, 12);
                MinimapTileSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Center);
                MinimapTileSelector.SetValue(NumericUpDown.MarginProperty, Thickness.Parse("4 8"));
                minimapOptions.Children.Add(MinimapTileSelector);

                MinimapTileSelector = new();
                MinimapTileSelector.ValueChanged+=ChangeMinimapTexCoord;
                _minimapMenu.MinimapTexCoordStartY = MinimapTileSelector;
                MinimapTileSelector.SetValue(Grid.RowProperty, 0);
                MinimapTileSelector.SetValue(Grid.ColumnProperty, 2);
                MinimapTileSelector.SetValue(NumericUpDown.ValueProperty, (decimal)starty);
                MinimapTileSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                MinimapTileSelector.SetValue(NumericUpDown.MaximumProperty, 1);
                MinimapTileSelector.SetValue(NumericUpDown.ShowButtonSpinnerProperty, false);
                MinimapTileSelector.SetValue(NumericUpDown.FontSizeProperty, 12);
                MinimapTileSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Center);
                MinimapTileSelector.SetValue(NumericUpDown.MarginProperty, Thickness.Parse("4 8"));
                minimapOptions.Children.Add(MinimapTileSelector);

                MinimapTileSelector = new();
                MinimapTileSelector.ValueChanged+=ChangeMinimapTexCoord;
                _minimapMenu.MinimapTexCoordEndY = MinimapTileSelector;
                MinimapTileSelector.SetValue(Grid.RowProperty, 1);
                MinimapTileSelector.SetValue(Grid.ColumnProperty, 2);
                MinimapTileSelector.SetValue(NumericUpDown.ValueProperty, (decimal)endy);
                MinimapTileSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                MinimapTileSelector.SetValue(NumericUpDown.MaximumProperty, 1);
                MinimapTileSelector.SetValue(NumericUpDown.ShowButtonSpinnerProperty, false);
                MinimapTileSelector.SetValue(NumericUpDown.FontSizeProperty, 12);
                MinimapTileSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Center);
                MinimapTileSelector.SetValue(NumericUpDown.MarginProperty, Thickness.Parse("4 8"));
                minimapOptions.Children.Add(MinimapTileSelector);


                ItemOutline = new();
                ItemOutline.SetValue(Grid.RowProperty, 1);
                ItemOutline.SetValue(Grid.ColumnProperty, 1);
                ItemOutline.SetValue(Border.BorderBrushProperty, palette[3]);
                ItemOutline.SetValue(Border.BorderThicknessProperty, Avalonia.Thickness.Parse("4"));
                ItemOutline.SetValue(Border.MarginProperty, Avalonia.Thickness.Parse("2"));
                ActiveGrid.Children.Add(ItemOutline);

                InnerGrid2 = new();
                ItemOutline.Child = InnerGrid2;
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(4, GridUnitType.Star));
                InnerGrid2.RowDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                InnerGrid2.ColumnDefinitions.Add(new(1, GridUnitType.Star));


                Grid ScaleVisual = new();
                _minimapMenu.ScaleGridVisual = ScaleVisual;
                ScaleVisual.SetValue(Grid.RowProperty, 1);
                ScaleVisual.SetValue(Grid.ColumnSpanProperty, 2);
                ScaleVisual.SetValue(Grid.ZIndexProperty, 2);
                ScaleVisual.SetValue(Grid.ShowGridLinesProperty, true);
                ScaleVisual.SetValue(Grid.WidthProperty, 240);
                ScaleVisual.SetValue(Grid.HeightProperty, 240);
                ScaleVisual.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                ScaleVisual.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                for (int i = 0; i < _rooms[lastMinimapID+1].sizeX; i++)
                {
                    ScaleVisual.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                }
                for (int i = 0; i < _rooms[lastMinimapID+1].sizeY; i++)
                {
                    ScaleVisual.RowDefinitions.Add(new(1, GridUnitType.Star));
                }
                InnerGrid2.Children.Add(ScaleVisual);

                Grid ScaleMap = new();
                _minimapMenu.TextureScaleGrid = ScaleMap;
                ScaleMap.SetValue(Grid.RowProperty, 1);
                ScaleMap.SetValue(Grid.ColumnSpanProperty, 2);
                ScaleMap.SetValue(Grid.ZIndexProperty, 1);
                InnerGrid2.Children.Add(ScaleMap);
                ScaleMap.SetValue(Grid.BackgroundProperty, palette[5]);
                ScaleMap.SetValue(Grid.WidthProperty, 240);
                ScaleMap.SetValue(Grid.HeightProperty, 240);
                ScaleMap.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                ScaleMap.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Bottom);

                for (int i = 0; i < _rooms[lastMinimapID+1].sizeX*18; i++)
                {
                    ScaleMap.ColumnDefinitions.Add(new(1, GridUnitType.Star));
                }
                for (int i = 0; i < _rooms[lastMinimapID+1].sizeY*18; i++)
                {
                    ScaleMap.RowDefinitions.Add(new(1, GridUnitType.Star));
                }

                cellsizeX = 240.0/(_rooms[lastMinimapID+1].sizeX*18);
                cellsizeY = 240.0/(_rooms[lastMinimapID+1].sizeY*18);
                rot = 0;

                PixelPoint start;
                PixelPoint end;
                double scale_x;
                double scale_y;
                if (_minimap[lastMinimapID].multipleNames)
                {
                    start = new((int)Math.Floor(img_width*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0]),
                                (int)Math.Floor(img_height*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1]));
                    end = new((int)Math.Floor(img_width*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][0]),
                                (int)Math.Floor(img_height*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][1]));
                    scale_x = _minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][0];
                    scale_y = _minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][1];
                }
                else
                {

                    start = new((int)Math.Floor(img_width*_minimap[lastMinimapID].texCoordSingle[0][0]),
                                (int)Math.Floor(img_height*_minimap[lastMinimapID].texCoordSingle[0][1]));

                    end = new((int)Math.Floor(img_width*_minimap[lastMinimapID].texCoordSingle[1][0]),
                                (int)Math.Floor(img_height*_minimap[lastMinimapID].texCoordSingle[1][1]));
                    scale_x = _minimap[lastMinimapID].texScaleSingle[0];
                    scale_y = _minimap[lastMinimapID].texScaleSingle[1];
                }

                CroppedBitmap currentImage_ToScale = new();
                PixelRect currentImageSelection = new(start, end);

                currentImage_ToScale.Source = currentImage;
                currentImage_ToScale.SourceRect = currentImageSelection;
;

                currentRoom = new();
                _minimapMenu.CurrentTileScale = currentRoom;
                /*
                Works how we want, but doesn't accept floats.
                Might not be a problem, floats may only be used due to variable resolution issues
                */
                currentRoom.SetValue(Grid.RowProperty, 0);
                currentRoom.SetValue(Grid.ColumnProperty, 0);
                currentRoom.SetValue(Grid.RowSpanProperty, (int)Math.Floor(scale_y-1));
                currentRoom.SetValue(Grid.ColumnSpanProperty, (int)Math.Floor(scale_x-1));
                currentRoom.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                currentRoom.SetValue(Image.VerticalAlignmentProperty, VerticalAlignment.Top);
                currentRoom.SetValue(Image.SourceProperty, currentImage_ToScale);
                currentRoom.SetValue(Image.StretchProperty, Stretch.Fill);

                currentRoom.SetValue(Image.RenderTransformOriginProperty, RelativePoint.TopLeft);
                Matrix renderTransform = Matrix.Identity,
                    rotation = Matrix.CreateRotation(rot),
                    translation = Matrix.CreateTranslation(new Vector(0*cellsizeX, 0*cellsizeY)),
                    scale = Matrix.CreateScale(1, 1);
                renderTransform *= rotation;
                renderTransform *= translation;
                renderTransform *= scale;

                currentRoom.SetValue(Image.RenderTransformProperty, new MatrixTransform(renderTransform));

                ScaleMap.Children.Add(currentRoom);

                Button rotate = new();
                rotate.Click += RotateScaleImage;
                _minimapMenu.RotateScale = rotate;
                rotate.SetValue(Grid.RowProperty, 0);
                rotate.SetValue(Grid.ColumnProperty, 1);
                rotate.SetValue(ToggleButton.MarginProperty, Thickness.Parse("5 10"));
                rotate.SetValue(ToggleButton.VerticalAlignmentProperty, VerticalAlignment.Center);
                rotate.SetValue(ToggleButton.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                rotate.SetValue(ToggleButton.ContentProperty, "Rotate");
                InnerGrid2.Children.Add(rotate);


                ToggleButton sizediff = new();
                if (_minimap[lastMinimapID].multipleNames)
                {
                    sizediff.IsChecked = _minimap[lastMinimapID].multiOrientBased[lastMinimapNameID];
                }
                else
                {
                    sizediff.IsChecked = _minimap[lastMinimapID].singleOrientBased;
                }
                _minimapMenu.UnevenSizeSplit = sizediff;
                sizediff.IsCheckedChanged += ToggleOrientationSettings;
                sizediff.SetValue(Grid.RowProperty, 0);
                sizediff.SetValue(Grid.ColumnProperty, 0);
                sizediff.SetValue(ToggleButton.MarginProperty, Thickness.Parse("5 10"));
                sizediff.SetValue(ToggleButton.VerticalAlignmentProperty, VerticalAlignment.Center);
                sizediff.SetValue(ToggleButton.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                sizediff.SetValue(ToggleButton.ContentProperty, "Rotate Scale");
                InnerGrid2.Children.Add(sizediff);

                MinimapTileSelector = new();
                _minimapMenu.MinimapScaleX = MinimapTileSelector;
                MinimapTileSelector.ValueChanged+=ChangeMinimapScale;
                MinimapTileSelector.SetValue(Grid.RowProperty, 2);
                MinimapTileSelector.SetValue(Grid.ColumnProperty, 0);
                MinimapTileSelector.SetValue(NumericUpDown.ValueProperty, (decimal)scale_x);
                MinimapTileSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                MinimapTileSelector.SetValue(NumericUpDown.MaximumProperty, (18*_rooms[lastMinimapID+1].sizeX)+1);
                MinimapTileSelector.SetValue(NumericUpDown.ShowButtonSpinnerProperty, false);
                MinimapTileSelector.SetValue(NumericUpDown.FontSizeProperty, 14);
                MinimapTileSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Center);
                MinimapTileSelector.SetValue(NumericUpDown.MarginProperty, Thickness.Parse("5 10"));
                InnerGrid2.Children.Add(MinimapTileSelector);

                MinimapTileSelector = new();
                _minimapMenu.MinimapScaleY = MinimapTileSelector;
                MinimapTileSelector.ValueChanged+=ChangeMinimapScale;
                MinimapTileSelector.SetValue(Grid.RowProperty, 2);
                MinimapTileSelector.SetValue(Grid.ColumnProperty, 1);
                MinimapTileSelector.SetValue(NumericUpDown.ValueProperty, (decimal)scale_y);
                MinimapTileSelector.SetValue(NumericUpDown.MinimumProperty, 0);
                MinimapTileSelector.SetValue(NumericUpDown.MaximumProperty, (18*_rooms[lastMinimapID+1].sizeY)+1);
                MinimapTileSelector.SetValue(NumericUpDown.ShowButtonSpinnerProperty, false);
                MinimapTileSelector.SetValue(NumericUpDown.FontSizeProperty, 14);
                MinimapTileSelector.SetValue(NumericUpDown.TextAlignmentProperty, TextAlignment.Center);
                MinimapTileSelector.SetValue(NumericUpDown.MarginProperty, Thickness.Parse("5 10"));
                InnerGrid2.Children.Add(MinimapTileSelector);

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
            ItemOutline.SetValue(Border.BackgroundProperty, palette[3]);
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
                        Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                        Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, VerticalAlignment.Top);
                        Toggle.SetValue(ToggleButton.MarginProperty, Avalonia.Thickness.Parse("20, 0, 20, 10"));
                        Toggle.SetValue(ToggleButton.HeightProperty, 10);
                        Toggle.SetValue(ToggleButton.NameProperty, "Up");
                        Toggle.IsChecked = (_rooms[lastRoomDataID].connectionValues[i/2][j/2] & 0x1) > 0;
                        Toggle.IsCheckedChanged+=ChangeTileDirectionData;
                        panel.Children.Add(Toggle);


                        Toggle = new();
                        Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                        Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, VerticalAlignment.Stretch);
                        Toggle.SetValue(ToggleButton.MarginProperty, Avalonia.Thickness.Parse("0, 20, 10, 20"));
                        Toggle.SetValue(ToggleButton.WidthProperty, 10);
                        Toggle.SetValue(ToggleButton.NameProperty, "Left");
                        Toggle.IsChecked = (_rooms[lastRoomDataID].connectionValues[i/2][j/2] & 0x2) > 0;
                        Toggle.IsCheckedChanged+=ChangeTileDirectionData;
                        panel.Children.Add(Toggle);

                        Toggle = new();
                        Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                        Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                        Toggle.SetValue(ToggleButton.MarginProperty, Avalonia.Thickness.Parse("20, 10, 20, 0"));
                        Toggle.SetValue(ToggleButton.HeightProperty, 10);
                        Toggle.SetValue(ToggleButton.NameProperty, "Down");
                        Toggle.IsChecked = (_rooms[lastRoomDataID].connectionValues[i/2][j/2] & 0x4) > 0;
                        Toggle.IsCheckedChanged+=ChangeTileDirectionData;
                        panel.Children.Add(Toggle);

                        Toggle = new();
                        Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                        Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, VerticalAlignment.Stretch);
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
                            panel.SetValue(MapTile.BackgroundProperty, Brush.Parse("#" + (tile_colors[panel.value].ToString("X").PadLeft(6))));
                        }
                        else
                        {
                            panel.SetValue(MapTile.BackgroundProperty, Brush.Parse("#FFFFFF"));
                        }
                        _roomDataMenu.RoomTiles.Children.Add(panel);
                    }
                    else
                    {
                        Toggle = new();
                        Toggle.SetValue(Grid.ColumnProperty, j);
                        Toggle.SetValue(Grid.RowProperty, i);
                        if ( (j % 2 == 1 && i % 2 == 0) )
                        {
                            Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                            Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, VerticalAlignment.Stretch);


                            var val1 = (_rooms[lastRoomDataID].connectionValues[i/2][(j-1)/2] >> 0x8);
                            var val2 = (_rooms[lastRoomDataID].connectionValues[i/2][(j+1)/2] >> 0x8);

                            Toggle.IsChecked = ((val1 & 0x8) > 0) && ((val2 & 0x2) > 0);

                            Toggle.IsVisible = _rooms[lastRoomDataID].hasDoor;
                            Toggle.IsCheckedChanged+=ChangeTileDoorData;
                            _roomDataMenu.RoomTiles.Children.Add(Toggle);
                        }
                        else if ( (i % 2 == 1 & j % 2 == 0) )
                        {


                            Toggle.SetValue(ToggleButton.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                            Toggle.SetValue(ToggleButton.VerticalAlignmentProperty, VerticalAlignment.Center);

                            var val1 = (_rooms[lastRoomDataID].connectionValues[(i-1)/2][j/2] >> 0x8);
                            var val2 = (_rooms[lastRoomDataID].connectionValues[(i+1)/2][j/2] >> 0x8);

                            Toggle.IsChecked = ((val1 & 0x4) > 0) && ((val2 & 0x1) > 0);

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
            if ( lastEncountID > _enemyEncounters.Count)
            {
                ((NumericUpDown)sender).Text = (_enemyEncounters.Count-1).ToString();
                lastEncountID = _enemyEncounters.Count-1;
            }
            else if (lastEncountID < 0)
            {
                ((NumericUpDown)sender).Text = "0";
                lastEncountID = 0;
            }

            _encounterMenu.Unit[0].SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].Units[0].ToString());
            _encounterMenu.Unit[1].SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].Units[1].ToString());
            _encounterMenu.Unit[2].SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].Units[2].ToString());
            _encounterMenu.Unit[3].SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].Units[3].ToString());
            _encounterMenu.Unit[4].SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].Units[4].ToString());


            _encounterMenu.Flags.SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].Flags.ToString());
            _encounterMenu.Field04.SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].Field04.ToString());
            _encounterMenu.Field06.SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].Field06.ToString());
            _encounterMenu.FieldID.SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].FieldID.ToString());
            _encounterMenu.MusicID.SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].MusicID.ToString());
            _encounterMenu.RoomID.SetValue(MaskedTextBox.TextProperty, _enemyEncounters[lastEncountID].RoomID.ToString());
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
                _enemyEncounters[lastEncountID].Units[0] = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Unit[1]))
            {
                _enemyEncounters[lastEncountID].Units[1] = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Unit[2]))
            {
                _enemyEncounters[lastEncountID].Units[2] = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Unit[3]))
            {
                _enemyEncounters[lastEncountID].Units[3] = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Unit[4]))
            {
                _enemyEncounters[lastEncountID].Units[4] = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Flags))
            {
                _enemyEncounters[lastEncountID].Flags = value;
            }
            else if (sender.Equals(_encounterMenu.Field04))
            {
                _enemyEncounters[lastEncountID].Field04 = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.Field06))
            {
                _enemyEncounters[lastEncountID].Field06 = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.FieldID))
            {
                _enemyEncounters[lastEncountID].FieldID = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.MusicID))
            {
                _enemyEncounters[lastEncountID].MusicID = (ushort)value;
            }
            else if (sender.Equals(_encounterMenu.RoomID))
            {
                _enemyEncounters[lastEncountID].RoomID = (ushort)value;
            }
            else
            {
                throw new Exception();
            }
        }
        private void AddEncounterEntry(object? sender, EventArgs e)
        {
            EnemyEncounter justAdded = new();
            justAdded.Units = new();
            for (int i = 0; i < 5; i++)
            {
                justAdded.Units.Add(0);
            }
            justAdded.Field04 = 0;
            justAdded.Field06 = 0;
            justAdded.Flags = 1;
            justAdded.FieldID = 0;
            justAdded.RoomID = 0;
            justAdded.MusicID = 0;
            _enemyEncounters.Add(justAdded);
            _encounterMenu.EncounterID.SetValue(NumericUpDown.MaximumProperty, _enemyEncounters.Count-1);
        }
        private void RemoveEncounterEntry(object? sender, EventArgs e)
        {
            _enemyEncounters.RemoveAt(lastEncountID);
            lastEncountID--;
            _encounterMenu.EncounterID.SetValue(NumericUpDown.MaximumProperty, _enemyEncounters.Count-1);
            _encounterMenu.EncounterID.SetValue(NumericUpDown.ValueProperty, lastEncountID);
        }
        private void ChangeActiveEncounterTable(object? sender, NumericUpDownValueChangedEventArgs e)  
        {
            lastEncountTableID = (int)((NumericUpDown)sender).Value;
            if (lastEncountTableID > _floorEncounters.Count)
            {
                ((NumericUpDown)sender).Text = "0";
                lastEncountTableID = 0;
            }
            else if (lastEncountTableID < 0)
            {
                ((NumericUpDown)sender).Text = (_floorEncounters.Count-1).ToString();
                lastEncountTableID = _floorEncounters.Count-1;
            }
            UpdateEncounterTableDisplay();
        }
        private void AddFloorEncounterTable(object? sender, EventArgs e)
        {
            FloorEncounter justAdded = new();
            justAdded.NormalWeightRegular = 100;
            justAdded.NormalWeightRain = 100;
            justAdded.RareWeightRegular = 0;
            justAdded.RareWeightRain = 0;
            justAdded.GoldWeightRegular = 0;
            justAdded.GoldWeightRain = 0;
            justAdded.AlwaysFF = 0xFF;
            justAdded.PercentRare = 0;
            justAdded.PercentGold = 0;

            justAdded.RegularEncountersNormal = new();
            justAdded.RainyEncountersNormal = new();
            for (int i = 0; i < 20; i++)
            {
                justAdded.RegularEncountersNormal.Add(new List<ushort> { 0, 0 });
                justAdded.RainyEncountersNormal.Add(new List<ushort> { 0, 0 });
            }

            justAdded.RegularEncountersRare = new();
            justAdded.RainyEncountersRare = new();
            for (int i = 0; i < 5; i++)
            {
                justAdded.RegularEncountersRare.Add(new List<ushort> { 0, 0 });
                justAdded.RainyEncountersRare.Add(new List<ushort> { 0, 0 });
            }

            justAdded.RegularEncountersGold = new();
            justAdded.RainyEncountersGold = new();
            for (int i = 0; i < 5; i++)
            {
                justAdded.RegularEncountersGold.Add(new List<ushort> { 0, 0 });
                justAdded.RainyEncountersGold.Add(new List<ushort> { 0, 0 });
            }

            _floorEncounters.Add(justAdded);
            _encounterTableMenu.EncountTable.SetValue(NumericUpDown.MaximumProperty, _floorEncounters.Count-1);
        }
        private void RemoveFloorEncounterTable(object? sender, EventArgs e)
        {
            _floorEncounters.RemoveAt(lastEncountTableID);
            lastEncountTableID--;
            _encounterTableMenu.EncountTable.SetValue(NumericUpDown.MaximumProperty, _floorEncounters.Count-1);
            _encounterTableMenu.EncountTable.SetValue(NumericUpDown.ValueProperty, lastEncountTableID);
        }
        private void UpdateEncounterTableDisplay()
        {
            if (_encounterTableMenu.IsRainy.IsChecked == true)
            {
                _encounterTableMenu.NormalWeight.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].NormalWeightRain.ToString());
                _encounterTableMenu.RareWeight.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RareWeightRain.ToString());
                _encounterTableMenu.GoldWeight.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].GoldWeightRain.ToString());


                for (int i = 0; i < _floorEncounters[lastEncountTableID].RainyEncountersNormal.Count; i++)
                {
                    _encounterTableMenu.NormalEncounters[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RainyEncountersNormal[i][0].ToString());
                    _encounterTableMenu.NormalEncounterWeights[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RainyEncountersNormal[i][1].ToString());
                }
                for (int i = 0; i < _floorEncounters[lastEncountTableID].RainyEncountersRare.Count; i++)
                {
                    _encounterTableMenu.RareEncounters[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RainyEncountersRare[i][0].ToString());
                    _encounterTableMenu.RareEncounterWeights[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RainyEncountersRare[i][1].ToString());
                }
                for (int i = 0; i < _floorEncounters[lastEncountTableID].RainyEncountersGold.Count; i++)
                {
                    _encounterTableMenu.GoldEncounters[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RainyEncountersGold[i][0].ToString());
                    _encounterTableMenu.GoldEncounterWeights[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RainyEncountersGold[i][1].ToString());
                }
            }
            else
            {

                _encounterTableMenu.NormalWeight.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].NormalWeightRegular.ToString());
                _encounterTableMenu.RareWeight.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RareWeightRegular.ToString());
                _encounterTableMenu.GoldWeight.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].GoldWeightRegular.ToString());

                for (int i = 0; i < _floorEncounters[lastEncountTableID].RegularEncountersNormal.Count; i++)
                {
                    _encounterTableMenu.NormalEncounters[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersNormal[i][0].ToString());
                    _encounterTableMenu.NormalEncounterWeights[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersNormal[i][1].ToString());
                }
                for (int i = 0; i < _floorEncounters[lastEncountTableID].RegularEncountersRare.Count; i++)
                {
                    _encounterTableMenu.RareEncounters[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersRare[i][0].ToString());
                    _encounterTableMenu.RareEncounterWeights[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersRare[i][1].ToString());
                }
                for (int i = 0; i < _floorEncounters[lastEncountTableID].RegularEncountersGold.Count; i++)
                {
                    _encounterTableMenu.GoldEncounters[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersGold[i][0].ToString());
                    _encounterTableMenu.GoldEncounterWeights[i].SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].RegularEncountersGold[i][1].ToString());
                }
            }

            _encounterTableMenu.AlwaysFF.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].AlwaysFF.ToString());
            _encounterTableMenu.RarePercent.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].PercentRare.ToString());
            _encounterTableMenu.GoldPercent.SetValue(MaskedTextBox.TextProperty, _floorEncounters[lastEncountTableID].PercentGold.ToString());
        }
        private void ChangeEncounterTableData(object? sender, EventArgs e)
        {
            if (sender == _encounterTableMenu.IsRainy)
            {
                UpdateEncounterTableDisplay();
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

            if (sender ==  _encounterTableMenu.NormalWeight)
            {
                if (_encounterTableMenu.IsRainy.IsChecked == true)
                {
                    _floorEncounters[lastEncountTableID].NormalWeightRain = (byte)value;
                }
                else
                {
                    _floorEncounters[lastEncountTableID].NormalWeightRegular = (byte)value;
                }
            }
            else if (sender ==  _encounterTableMenu.RareWeight)
            {
                if (_encounterTableMenu.IsRainy.IsChecked == true)
                {
                    _floorEncounters[lastEncountTableID].RareWeightRain = (byte)value;
                }
                else
                {
                    _floorEncounters[lastEncountTableID].RareWeightRegular = (byte)value;
                }
            }
            else if (sender ==  _encounterTableMenu.NormalWeight)
            {
                if (_encounterTableMenu.IsRainy.IsChecked == true)
                {
                    _floorEncounters[lastEncountTableID].GoldWeightRain = (byte)value;
                }
                else
                {
                    _floorEncounters[lastEncountTableID].GoldWeightRegular = (byte)value;
                }
            }
            else if (sender == _encounterTableMenu.AlwaysFF)
            {
                _floorEncounters[lastEncountTableID].AlwaysFF = (byte)value;
            }
            else if (sender == _encounterTableMenu.RarePercent)
            {
                _floorEncounters[lastEncountTableID].PercentRare = (byte)value;
            }
            else if (sender == _encounterTableMenu.GoldPercent)
            {
                _floorEncounters[lastEncountTableID].PercentGold = (byte)value;
            }
            else
            {
                for (int i = 0; i < _encounterTableMenu.NormalEncounters.Length; i++)
                {
                    if (_encounterTableMenu.NormalEncounters[i] == sender)
                    {
                        if (_encounterTableMenu.IsRainy.IsChecked == true)
                        {
                            _floorEncounters[lastEncountTableID].RainyEncountersNormal[i][0] = (byte)value;
                        }
                        else
                        {
                            _floorEncounters[lastEncountTableID].RegularEncountersNormal[i][0] = (ushort)value;
                        }
                        return;
                    }
                    else if (_encounterTableMenu.NormalEncounterWeights[i] == sender)
                    {
                        if (_encounterTableMenu.IsRainy.IsChecked == true)
                        {
                            _floorEncounters[lastEncountTableID].RainyEncountersNormal[i][1] = (byte)value;
                        }
                        else
                        {
                            _floorEncounters[lastEncountTableID].RegularEncountersNormal[i][1] = (ushort)value;
                        }
                        return;
                    }
                }
                for (int i = 0; i < _encounterTableMenu.RareEncounters.Length; i++)
                {
                    if (_encounterTableMenu.RareEncounters[i] == sender)
                    {
                        if (_encounterTableMenu.IsRainy.IsChecked == true)
                        {
                            _floorEncounters[lastEncountTableID].RainyEncountersRare[i][0] = (byte)value;
                        }
                        else
                        {
                            _floorEncounters[lastEncountTableID].RegularEncountersRare[i][0] = (ushort)value;
                        }
                        return;
                    }
                    else if (_encounterTableMenu.RareEncounterWeights[i] == sender)
                    {
                        if (_encounterTableMenu.IsRainy.IsChecked == true)
                        {
                            _floorEncounters[lastEncountTableID].RainyEncountersRare[i][1] = (byte)value;
                        }
                        else
                        {
                            _floorEncounters[lastEncountTableID].RegularEncountersRare[i][1] = (ushort)value;
                        }
                        return;
                    }
                }
                for (int i = 0; i < _encounterTableMenu.GoldEncounters.Length; i++)
                {
                    if (_encounterTableMenu.GoldEncounters[i] == sender)
                    {
                        if (_encounterTableMenu.IsRainy.IsChecked == true)
                        {
                            _floorEncounters[lastEncountTableID].RainyEncountersGold[i][0] = (byte)value;
                        }
                        else
                        {
                            _floorEncounters[lastEncountTableID].RegularEncountersGold[i][0] = (ushort)value;
                        }
                        return;
                    }
                    else if (_encounterTableMenu.GoldEncounterWeights[i] == sender)
                    {
                        if (_encounterTableMenu.IsRainy.IsChecked == true)
                        {
                            _floorEncounters[lastEncountTableID].RainyEncountersGold[i][1] = (byte)value;
                        }
                        else
                        {
                            _floorEncounters[lastEncountTableID].RegularEncountersGold[i][1] = (ushort)value;
                        }
                        return;
                    }
                }
                throw new Exception();
            }
        }
        private void ChangeActiveLootTable(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            lastLootID = (int)((NumericUpDown)sender).Value;
            if (lastLootID > _lootTables.Count-1)
            {
                ((NumericUpDown)sender).Text = "0";
                lastLootID = 0;
            }
            else if (lastLootID < 0)
            {
                ((NumericUpDown)sender).Text = (_lootTables.Count-1).ToString();
                lastLootID = _lootTables.Count-1;
            }
            for (int i = 0; i < _lootTableMenu.Entries.Count; i++)
            {
                _lootTableMenu.Entries[i].ItemWeight.SetValue(MaskedTextBox.TextProperty, _lootTables[lastLootID].LootEntries[i].ItemWeight.ToString());
                _lootTableMenu.Entries[i].ItemID.SetValue(MaskedTextBox.TextProperty, _lootTables[lastLootID].LootEntries[i].ItemID.ToString());
                _lootTableMenu.Entries[i].ChestFlags.SetValue(MaskedTextBox.TextProperty, _lootTables[lastLootID].LootEntries[i].ChestFlags.ToString());
                _lootTableMenu.Entries[i].ChestModel.SetValue(ToggleButton.IsCheckedProperty, _lootTables[lastLootID].LootEntries[i].ChestModel > 0);
            }
        }
        private void AddLootTable(object? sender, EventArgs e)
        {
            LootTable justAdded = new();
            justAdded.LootEntries = new();
            for (int i = 0; i < 29; i++)
            {
                LootTable.LootEntry entry = new();
                entry.ItemID = 0;
                entry.ItemWeight = 0;
                entry.ChestFlags = 0;
                entry.ChestModel = 0;
                justAdded.LootEntries.Add(entry);
            }
            _lootTables.Add(justAdded);
            _lootTableMenu.LootID.SetValue(NumericUpDown.MaximumProperty, _lootTables.Count-1);
        }
        private void RemoveLootTable(object? sender, EventArgs e)
        {
            _lootTables.RemoveAt(lastLootID);
            lastLootID--;
            _lootTableMenu.LootID.SetValue(NumericUpDown.MaximumProperty, _lootTables.Count-1);
            _lootTableMenu.LootID.SetValue(NumericUpDown.ValueProperty, lastLootID);
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
                        _lootTables[lastLootID].LootEntries[i].ItemWeight = (ushort)int.Parse(box.Text);
                        break;
                    }
                    else if (entry.ItemID == box)
                    {
                        _lootTables[lastLootID].LootEntries[i].ItemID = (ushort)int.Parse(box.Text);
                        break;
                    }
                    else if (entry.ChestFlags == box)
                    {
                        _lootTables[lastLootID].LootEntries[i].ChestFlags= (ushort)int.Parse(box.Text);
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
                            _lootTables[lastLootID].LootEntries[i].ChestModel = 0;
                        }
                        else
                        {
                            _lootTables[lastLootID].LootEntries[i].ChestModel = 1;
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

            _floorMenu.EncounterTableID.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].EncountTableLookup.ToString());
            _floorMenu.LootTableID.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].LootTableLookup.ToString());
            _floorMenu.MaxChestCount.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].MaxChestCount.ToString());
            _floorMenu.InitialEnemyCount.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].InitialEncounterCount.ToString());
            _floorMenu.MinEnemyCount.SetValue(MaskedTextBox.TextProperty, _floors[lastFloorID].MinEncounterCount.ToString());


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
                _floors[lastFloorID].EncountTableLookup = (ushort)value;
            }
            else if (sender  == _floorMenu.LootTableID)
            {
                _floors[lastFloorID].LootTableLookup = (ushort)value;
            }
            else if (sender  == _floorMenu.MaxChestCount)
            {
                _floors[lastFloorID].MaxChestCount = (byte)value;
            }
            else if (sender  == _floorMenu.InitialEnemyCount)
            {
                _floors[lastFloorID].InitialEncounterCount = (byte)value;
            }
            else if (sender  == _floorMenu.MinEnemyCount)
            {
                _floors[lastFloorID].MinEncounterCount = (byte)value;
            }
        }
        private void AddFloorEntry(object? sender, EventArgs e)
        {
            DungeonFloor justAdded = new();
            justAdded.floorName = "NULL";
            justAdded.ID = 0;
            justAdded.subID = 0;
            justAdded.Byte04 = 0;
            justAdded.Byte0A = 0;
            justAdded.tileCountMin = 0;
            justAdded.tileCountMax = 0;
            justAdded.dungeonScript = 0;
            justAdded.usedEnv = 0;
            justAdded.EncountTableLookup = 0;
            justAdded.LootTableLookup = 0;
            justAdded.InitialEncounterCount = 0;
            justAdded.MinEncounterCount = 0;
            justAdded.MaxChestCount = 0;
            _floors.Add(justAdded);
            _floorMenu.FloorID.SetValue(NumericUpDown.MaximumProperty, _floors.Count-1);
        }
        private void RemoveFloorEntry(object? sender, EventArgs e)
        {
            _floors.RemoveAt(lastFloorID);
            lastFloorID--;
            _floorMenu.FloorID.SetValue(NumericUpDown.MaximumProperty, _floors.Count-1);
            _floorMenu.FloorID.SetValue(NumericUpDown.ValueProperty, lastFloorID);
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
            if (_rooms.Count < 257)
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

                // Also need to add appropriate minimap data
                DungeonMinimap NewMap = new();
                NewMap.roomID = (byte)_rooms.Count-1;
                NewMap.name = $"smap{NewMap.roomID.ToString("D2")}.tmx";
                NewMap.multipleNames = false;
                NewMap.texCoordSingle = new List<List<float>>();
                List<float> intermediate = new();
                intermediate.Add(0.0f);
                intermediate.Add(0.0f);
                NewMap.texCoordSingle.Add(intermediate);
                intermediate = new();
                intermediate.Add(1.0f);
                intermediate.Add(1.0f);
                NewMap.texCoordSingle.Add(intermediate);
                NewMap.texScaleSingle = new() { 19.0f, 19.0f };


                NewMap.singleOrientBased = false;
                Bitmap defaultBitmap = new Bitmap(AssetLoader.Open(new Uri("avares://P4G-Dungeon-Editor/Assets/minimap_tiles/smap00.png")));
                _minimapTiles.Add(NewMap.name, defaultBitmap);
                _minimap.Add(NewMap);
            }
            else
            {
                // Maybe let them know ;)
            }
        }
        private void RemoveRoom(object? sender, EventArgs e)
        {
            _rooms.RemoveAt(lastRoomDataID);
            lastRoomDataID++;
            _roomDataMenu.RoomID.SetValue(NumericUpDown.MaximumProperty, _rooms.Count-1);
            _roomDataMenu.RoomID.SetValue(NumericUpDown.ValueProperty, lastRoomDataID);
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
            if (panel.value > 0)
            {
                Int32 color = 0xFFFFFF / 16;
                color = color * panel.value;
                panel.SetValue(MapTile.BackgroundProperty, Brush.Parse("#" + (tile_colors[panel.value].ToString("X").PadLeft(6))));
            }
            else
            {
                panel.SetValue(MapTile.BackgroundProperty, Brush.Parse("#FFFFFF") );
            }

            int sum = 0;
            int uniqueNums = 0;
            List<byte> seen = new();
            foreach (List<byte> row in _rooms[lastRoomDataID].revealProperties )
            {
                foreach (byte value in row)
                {
                    sum += value;
                    if (value != 0 && !seen.Contains(value))
                    {
                        seen.Add(value);
                    }
                }
            }
            seen.Sort();
            if (sum > 0)
            {
                _minimap[lastRoomDataID-1].multipleNames = true;

                if (_minimap[lastRoomDataID-1].names == null)
                    _minimap[lastRoomDataID-1].names = new();

                _minimap[lastRoomDataID-1].texCoordMulti = new();
                _minimap[lastRoomDataID-1].texScaleMulti = new();
                _minimap[lastRoomDataID-1].multiOrientBased = new();

                string original_name = _minimap[lastRoomDataID-1].name;
                int i = 1;
                int max_reveal_x = _rooms[lastRoomDataID].revealProperties.Count, 
                    max_reveal_y = _rooms[lastRoomDataID].revealProperties[0].Count;
                foreach (byte value in seen)
                {
                    int reveal_near_x = -1, reveal_near_y = -1;
                    int reveal_far_x = -1, reveal_far_y = -1;
                    int size_x = 0, size_y = 0;

                    string new_name = original_name.Split('.')[0] + "_" + i.ToString("D2") +".tmx";
                    if (!_minimap[lastRoomDataID-1].names.Contains(new_name))
                    {
                        _minimap[lastRoomDataID-1].names.Add(new_name);
                        Bitmap defaultBitmap = new Bitmap(AssetLoader.Open(new Uri("avares://P4G-Dungeon-Editor/Assets/minimap_tiles/smap00.png")));
                        _minimapTiles.Add(new_name, defaultBitmap);
                    }
                    for (int j = 0; j < _rooms[lastRoomDataID].revealProperties.Count; j++)
                    {
                        for (int k = 0; k < _rooms[lastRoomDataID].revealProperties[j].Count; k++)
                        {
                            if (_rooms[lastRoomDataID].revealProperties[j][k] == value)
                            {
                                if (reveal_far_x == -1 || reveal_far_x < k)
                                {
                                    reveal_far_x = k;
                                }
                                if (reveal_far_y == -1 || reveal_far_y < j)
                                {
                                    reveal_far_y = j;
                                }
                                if (reveal_near_x == -1 || reveal_near_x > k)
                                {
                                    reveal_near_x = k;
                                }
                                if (reveal_near_y == -1 || reveal_near_y > j)
                                {
                                    reveal_near_y = j;
                                }
                            }
                        }
                    }

                    size_x = reveal_far_x - reveal_near_x + 1;
                    size_y = reveal_far_y - reveal_near_y + 1;
                    List<List<float>> new_coord = new() { new() { (0.0f), (0.0f) },
                                                          new() { (1.0f*size_x)/(max_reveal_x), (1.0f*size_y)/(max_reveal_y) }};
                    List<float> new_scale = new() { (size_x*18.0f+1), (size_y*18.0f+1) };
                    _minimap[lastRoomDataID-1].texCoordMulti.Add(new_coord);
                    _minimap[lastRoomDataID-1].texScaleMulti.Add(new_scale);
                    _minimap[lastRoomDataID-1].multiOrientBased.Add(size_x != size_y);
                    i++;
                }
            }
            else
            {
                _minimap[lastRoomDataID-1].multipleNames = false;
                foreach (string name in _minimap[lastRoomDataID-1].names)
                { 
                    _minimapTiles.Remove(name);
                }
                _minimap[lastRoomDataID-1].names = null;
                _minimap[lastRoomDataID-1].texCoordMulti = null;
                _minimap[lastRoomDataID-1].texScaleMulti = null;
                _minimap[lastRoomDataID-1].multiOrientBased = null;
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
        private void AddTemplateEntry(object? sender, EventArgs e)
        {
            DungeonTemplates justAdded = new();
            justAdded.rooms = new();
            // Default is just going to be a duplicate of template 0
            justAdded.rooms.Add(1);
            justAdded.rooms.Add(2);
            justAdded.rooms.Add(3);
            justAdded.rooms.Add(5);
            justAdded.rooms.Add(6);
            justAdded.rooms.Add(7);
            justAdded.rooms.Add(9);
            justAdded.rooms.Add(10);
            justAdded.rooms.Add(4);
            justAdded.roomCount = 8;
            justAdded.roomExCount = 9;
            justAdded.exitNum = 10;
            _templates.Add(justAdded);
            _templateMenu.TemplateID.SetValue(NumericUpDown.MaximumProperty, _templates.Count-1);
        }
        private void RemoveTemplateEntry(object? sender, EventArgs e)
        {
            if (_dungeon_template_dict.ContainsValue((byte)lastTemplateID))
            {
                foreach (var item in _dungeon_template_dict.Where(kvp => kvp.Value == (byte)lastTemplateID).ToList())
                {
                    {
                        _dungeon_template_dict.Remove(item.Key);
                    }

                }
            }
            _templates.RemoveAt(lastTemplateID);
            lastTemplateID--;
            _templateMenu.TemplateID.SetValue(NumericUpDown.MaximumProperty, _templates.Count-1);
            _templateMenu.TemplateID.SetValue(NumericUpDown.ValueProperty, lastTemplateID);
        }
        private void LoadMinimapSectionOutlines(object? sender, EventArgs e)
        {
            if (sender is Image)
            {
                Image mapTile = (Image)sender;
                Panel parent = (Panel)mapTile.Parent;
                Rectangle test = new();
                double x = mapTile.Bounds.Right;
                double y = mapTile.Bounds.Bottom;
                double selection_width;
                double selection_height;
                double offset_x;
                double offset_y;
                if (_minimap[lastMinimapID].multipleNames)
                {
                    selection_width = x*((_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][0] - _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0]));
                    selection_height = y*((_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][1] - _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1]));

                    offset_x = x*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0] + mapTile.Bounds.Left;
                    offset_y = y*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1] + mapTile.Bounds.Top;
                }
                else
                {
                    selection_width = x*((_minimap[lastMinimapID].texCoordSingle[1][0] - _minimap[lastMinimapID].texCoordSingle[0][0]));
                    selection_height = y*((_minimap[lastMinimapID].texCoordSingle[1][1] - _minimap[lastMinimapID].texCoordSingle[0][1]));

                    offset_x = x*_minimap[lastMinimapID].texCoordSingle[0][0] + mapTile.Bounds.Left;
                    offset_y = y*_minimap[lastMinimapID].texCoordSingle[0][1] + mapTile.Bounds.Top;

                }
                TranslateTransform offset = new(offset_x, offset_y);


                test.SetValue(Rectangle.WidthProperty, selection_width);
                test.SetValue(Rectangle.HeightProperty, selection_height);
                test.SetValue(Rectangle.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                test.SetValue(Rectangle.VerticalAlignmentProperty, VerticalAlignment.Top);
                test.SetValue(Rectangle.RenderTransformProperty, offset);
                test.SetValue(Rectangle.OpacityProperty, 0.5);
                test.SetValue(Rectangle.FillProperty, Brush.Parse("#" + (tile_colors[0].ToString("X").PadLeft(6))));
                test.SetValue(Grid.ZIndexProperty, 1);
                _minimapMenu.TileTextCoordOutline = test;
                parent.Children.Add(test);

            }
        }
        private void ChangeActiveMinimap(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (_minimapMenu.CurrentTileTexCoord == null || _minimapMenu.CurrentTileScale == null)
            {
                return;
            }
            NumericUpDown MinimapPage = sender as NumericUpDown;
            lastMinimapID = (int)MinimapPage.Value-1;
            lastMinimapNameID = 0;

            cellsizeX = 240.0/(_rooms[lastMinimapID+1].sizeX*18);
            cellsizeY = 240.0/(_rooms[lastMinimapID+1].sizeY*18);
            rot = 0;

            _minimapMenu.ScaleGridVisual.RowDefinitions.Clear();
            _minimapMenu.ScaleGridVisual.ColumnDefinitions.Clear();
            for (int i = 0; i < _rooms[lastMinimapID+1].sizeX; i++)
            {
                _minimapMenu.ScaleGridVisual.ColumnDefinitions.Add(new(1, GridUnitType.Star));
            }
            for (int i = 0; i < _rooms[lastMinimapID+1].sizeY; i++)
            {
                _minimapMenu.ScaleGridVisual.RowDefinitions.Add(new(1, GridUnitType.Star));
            }

            _minimapMenu.TextureScaleGrid.RowDefinitions.Clear();
            _minimapMenu.TextureScaleGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < _rooms[lastMinimapID+1].sizeX*18; i++)
            {
                _minimapMenu.TextureScaleGrid.ColumnDefinitions.Add(new(1, GridUnitType.Star));
            }
            for (int i = 0; i < _rooms[lastMinimapID+1].sizeY*18; i++)
            {
                _minimapMenu.TextureScaleGrid.RowDefinitions.Add(new(1, GridUnitType.Star));
            };

            if (_minimap[lastMinimapID].multipleNames)
            {
                _minimapMenu.MinimapTileNameSelection.SetValue(NumericUpDown.IsEnabledProperty, true);
                _minimapMenu.MinimapTileNameSelection.SetValue(NumericUpDown.MaximumProperty, _minimap[lastMinimapID].names.Count-1);
 

                // Should only ever be called by rooms with multiple names associated with it
                Bitmap newTile;
                string name;
                if (_minimapTiles.TryGetValue(_minimap[lastMinimapID].names[lastMinimapNameID], out newTile))
                {
                    _minimapMenu.CurrentTileTexCoord.Source = newTile;

                    PixelPoint start;
                    PixelPoint end;

                    start = new((int)Math.Floor(newTile.Size.Width*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0]),
                                (int)Math.Floor(newTile.Size.Height*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1]));
                    end = new((int)Math.Floor(newTile.Size.Width*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][0]),
                                (int)Math.Floor(newTile.Size.Height*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][1]));

                    CroppedBitmap newTile_ToScale = new();
                    PixelRect currentImageSelection = new(start, end);
                    newTile_ToScale.Source = newTile;
                    newTile_ToScale.SourceRect = currentImageSelection;
                    _minimapMenu.CurrentTileScale.SetValue(Image.SourceProperty, newTile_ToScale);

                    _minimapMenu.MinimapTexCoordStartX.Value = (decimal)_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0];
                    _minimapMenu.MinimapTexCoordStartY.Value = (decimal)_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1];
                    _minimapMenu.MinimapTexCoordEndX.Value = (decimal)_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][0];
                    _minimapMenu.MinimapTexCoordEndY.Value = (decimal)_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][1];

                    _minimapMenu.MinimapScaleX.Value = (decimal)_minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][0];
                    _minimapMenu.MinimapScaleY.Value = (decimal)_minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][1];


                    _minimapMenu.CurrentTileScale.SetValue(Grid.RowSpanProperty, (int)Math.Floor(_minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][1]-1));
                    _minimapMenu.CurrentTileScale.SetValue(Grid.ColumnSpanProperty, (int)Math.Floor(_minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][0]-1));

                    _minimapMenu.UnevenSizeSplit.IsChecked = _minimap[lastMinimapID].multiOrientBased[lastMinimapNameID];
                }

            }
            else
            {
                _minimapMenu.MinimapTileNameSelection.SetValue(NumericUpDown.IsEnabledProperty, false);

                // Should only ever be called by rooms with multiple names associated with it
                NumericUpDown numericUpDown = sender as NumericUpDown;
                Bitmap newTile;
                string name;
                if (_minimapTiles.TryGetValue(_minimap[lastMinimapID].name, out newTile))
                {
                    _minimapMenu.CurrentTileTexCoord.Source = newTile;

                    PixelPoint start;
                    PixelPoint end;

                    start = new((int)Math.Floor(newTile.Size.Width*_minimap[lastMinimapID].texCoordSingle[0][0]),
                                (int)Math.Floor(newTile.Size.Height*_minimap[lastMinimapID].texCoordSingle[0][1]));

                    end = new((int)Math.Floor(newTile.Size.Width*_minimap[lastMinimapID].texCoordSingle[1][0]),
                                (int)Math.Floor(newTile.Size.Height*_minimap[lastMinimapID].texCoordSingle[1][1]));

                    CroppedBitmap newTile_ToScale = new();
                    PixelRect currentImageSelection = new(start, end);
                    newTile_ToScale.Source = newTile;
                    newTile_ToScale.SourceRect = currentImageSelection;
                    _minimapMenu.CurrentTileScale.Source = newTile_ToScale;

                    _minimapMenu.MinimapTexCoordStartX.Value = (decimal)_minimap[lastMinimapID].texCoordSingle[0][0];
                    _minimapMenu.MinimapTexCoordStartY.Value = (decimal)_minimap[lastMinimapID].texCoordSingle[0][1];
                    _minimapMenu.MinimapTexCoordEndX.Value = (decimal)_minimap[lastMinimapID].texCoordSingle[1][0];
                    _minimapMenu.MinimapTexCoordEndY.Value = (decimal)_minimap[lastMinimapID].texCoordSingle[1][1];
                    _minimapMenu.MinimapScaleX.Value = (decimal)_minimap[lastMinimapID].texScaleSingle[0];
                    _minimapMenu.MinimapScaleY.Value = (decimal)_minimap[lastMinimapID].texScaleSingle[1];


                    _minimapMenu.CurrentTileScale.SetValue(Grid.RowSpanProperty, (int)Math.Floor(_minimap[lastMinimapID].texScaleSingle[1]));
                    _minimapMenu.CurrentTileScale.SetValue(Grid.ColumnSpanProperty, (int)Math.Floor(_minimap[lastMinimapID].texScaleSingle[0]));
                    _minimapMenu.UnevenSizeSplit.IsChecked = _minimap[lastMinimapID].singleOrientBased;
                }

            }
            _minimapMenu.MinimapTileNameSelection.SetValue(NumericUpDown.ValueProperty, lastMinimapNameID);
        }
        private void ToggleOrientationSettings(object? sender, EventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            if (_minimap[lastMinimapID].multipleNames)
            {
                _minimap[lastMinimapID].multiOrientBased[lastMinimapNameID] = button.IsChecked.Value;
            }
            else
            {
                _minimap[lastMinimapID].singleOrientBased = button.IsChecked.Value;
            }
            RotateScaleImage(sender, null);
        }
        private void ChangeMinimapTexCoord(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            Image mapTile = _minimapMenu.CurrentTileTexCoord;
            if (mapTile.IsLoaded == false || _minimapMenu.CurrentTileScale == null)
            {
                return;
            }
            if (sender == _minimapMenu.MinimapTexCoordStartX)
            {
                if (_minimap[lastMinimapID].multipleNames)
                {
                    _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0] = (float)_minimapMenu.MinimapTexCoordStartX.Value;
                }
                else
                {
                    _minimap[lastMinimapID].texCoordSingle[0][0] = (float)_minimapMenu.MinimapTexCoordStartX.Value;
                }
            }
            else if (sender == _minimapMenu.MinimapTexCoordStartY)
            {
                if (_minimap[lastMinimapID].multipleNames)
                {
                    _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1] = (float)_minimapMenu.MinimapTexCoordStartY.Value;
                }
                else
                {
                    _minimap[lastMinimapID].texCoordSingle[0][1] = (float)_minimapMenu.MinimapTexCoordStartY.Value;
                }
            }
            else if (sender == _minimapMenu.MinimapTexCoordEndX)
            {
                if (_minimap[lastMinimapID].multipleNames)
                {
                    _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][0] = (float)_minimapMenu.MinimapTexCoordEndX.Value;
                }
                else
                {
                    _minimap[lastMinimapID].texCoordSingle[1][0] = (float)_minimapMenu.MinimapTexCoordEndX.Value;
                }
            }
            else if (sender == _minimapMenu.MinimapTexCoordEndY)
            {
                if (_minimap[lastMinimapID].multipleNames)
                {
                    _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][1] = (float)_minimapMenu.MinimapTexCoordEndY.Value;
                }
                else
                {
                    _minimap[lastMinimapID].texCoordSingle[1][1] = (float)_minimapMenu.MinimapTexCoordEndY.Value;
                }
            }
            double x = mapTile.Bounds.Right;
            double y = mapTile.Bounds.Bottom;
            double selection_width;
            double selection_height;
            double offset_x;
            double offset_y;
            if (_minimap[lastMinimapID].multipleNames)
            {
                selection_width = x*((_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][0] - _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0]));
                selection_height = y*((_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][1] - _minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1]));

                offset_x = x*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0] + mapTile.Bounds.Left;
                offset_y = y*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1] + mapTile.Bounds.Top;
            }
            else
            {
                selection_width = x*((_minimap[lastMinimapID].texCoordSingle[1][0] - _minimap[lastMinimapID].texCoordSingle[0][0]));
                selection_height = y*((_minimap[lastMinimapID].texCoordSingle[1][1] - _minimap[lastMinimapID].texCoordSingle[0][1]));

                offset_x = x*_minimap[lastMinimapID].texCoordSingle[0][0] + mapTile.Bounds.Left;
                offset_y = y*_minimap[lastMinimapID].texCoordSingle[0][1] + mapTile.Bounds.Top;

            }
            TranslateTransform offset = new(offset_x, offset_y);


            _minimapMenu.TileTextCoordOutline.SetValue(Rectangle.WidthProperty, selection_width);
            _minimapMenu.TileTextCoordOutline.SetValue(Rectangle.HeightProperty, selection_height);
            _minimapMenu.TileTextCoordOutline.SetValue(Rectangle.RenderTransformProperty, offset);


            // Since the selection for the scale map is drawn from the texture coordinates, gotta re-render
            CroppedBitmap scale_src = (CroppedBitmap)_minimapMenu.CurrentTileScale.Source;
            PixelPoint start;
            PixelPoint end;

            double img_width = mapTile.Source.Size.Width;
            double img_height = mapTile.Source.Size.Height;

            if (_minimap[lastMinimapID].multipleNames)
            {
                start = new((int)Math.Floor(img_width*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0]),
                            (int)Math.Floor(img_height*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1]));
                end = new((int)Math.Floor(img_width*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][0]),
                            (int)Math.Floor(img_height*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][1]));
            }
            else
            {

                start = new((int)Math.Floor(img_width*_minimap[lastMinimapID].texCoordSingle[0][0]),
                            (int)Math.Floor(img_height*_minimap[lastMinimapID].texCoordSingle[0][1]));

                end = new((int)Math.Floor(img_width*_minimap[lastMinimapID].texCoordSingle[1][0]),
                            (int)Math.Floor(img_height*_minimap[lastMinimapID].texCoordSingle[1][1]));
            }
            scale_src.SourceRect = new PixelRect(start, end);
        }
        private void ChangeMinimapScale(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (_minimapMenu.CurrentTileScale.IsLoaded == false)
            {
                return;
            }
            NumericUpDown ScaleUpDown = (NumericUpDown)sender;
            if (sender == _minimapMenu.MinimapScaleX)
            {
                if (_minimap[lastMinimapID].multipleNames)
                {
                    _minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][0] = (float)ScaleUpDown.Value;
                }
                else
                {
                    _minimap[lastMinimapID].texScaleSingle[0] = (float)ScaleUpDown.Value;
                }
            }
            else if (sender == _minimapMenu.MinimapScaleY)
            {
                if (_minimap[lastMinimapID].multipleNames)
                {
                    _minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][1] = (float)ScaleUpDown.Value;
                }
                else
                {
                    _minimap[lastMinimapID].texScaleSingle[1] = (float)ScaleUpDown.Value;
                }
            }

            double scale_x;
            double scale_y;
            if (_minimap[lastMinimapID].multipleNames)
            {
                scale_x = _minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][0];
                scale_y = _minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][1];
            }
            else
            {
                scale_x = _minimap[lastMinimapID].texScaleSingle[0];
                scale_y = _minimap[lastMinimapID].texScaleSingle[1];
            }

            _minimapMenu.CurrentTileScale.SetValue(Grid.RowProperty, 0);
            _minimapMenu.CurrentTileScale.SetValue(Grid.ColumnProperty, 0);
            _minimapMenu.CurrentTileScale.SetValue(Grid.RowSpanProperty, (int)Math.Floor(scale_y-1));
            _minimapMenu.CurrentTileScale.SetValue(Grid.ColumnSpanProperty, (int)Math.Floor(scale_x-1));
        }
        private void ChangeMinimapNameID(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (_minimapMenu.CurrentTileTexCoord.IsLoaded == false || _minimapMenu.CurrentTileScale == null)
            {
                return;
            }

            // Should only ever be called by rooms with multiple names associated with it
            NumericUpDown numericUpDown = sender as NumericUpDown;
            if (!numericUpDown.IsEnabled)
            {
                return;
            }
            Bitmap newTile;
            string name;
            if (_minimapTiles.TryGetValue(_minimap[lastMinimapID].names[(int)numericUpDown.Value], out newTile))
            {
                _minimapMenu.CurrentTileTexCoord.Source = newTile;
                lastMinimapNameID = (int)numericUpDown.Value;

                PixelPoint start;
                PixelPoint end;
                if (_minimap[lastMinimapID].multipleNames)
                {
                    start = new((int)Math.Floor(newTile.Size.Width*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0]),
                                (int)Math.Floor(newTile.Size.Height*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1]));
                    end = new((int)Math.Floor(newTile.Size.Width*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][0]),
                                (int)Math.Floor(newTile.Size.Height*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][1]));
                }
                else
                {

                    start = new((int)Math.Floor(newTile.Size.Width*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0]),
                                (int)Math.Floor(newTile.Size.Height*_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1]));

                    end = new((int)Math.Floor(newTile.Size.Width*_minimap[lastMinimapID].texCoordSingle[1][0]),
                                (int)Math.Floor(newTile.Size.Height*_minimap[lastMinimapID].texCoordSingle[1][1]));
                }

                CroppedBitmap newTile_ToScale = new();
                PixelRect currentImageSelection = new(start, end);
                newTile_ToScale.Source = newTile;
                newTile_ToScale.SourceRect = currentImageSelection;
                _minimapMenu.CurrentTileScale.Source = newTile_ToScale;

                _minimapMenu.MinimapTexCoordStartX.Value = (decimal)_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][0];
                _minimapMenu.MinimapTexCoordStartY.Value = (decimal)_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][0][1];
                _minimapMenu.MinimapTexCoordEndX.Value = (decimal)_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][0];
                _minimapMenu.MinimapTexCoordEndY.Value = (decimal)_minimap[lastMinimapID].texCoordMulti[lastMinimapNameID][1][1];
                _minimapMenu.MinimapScaleX.Value = (decimal)_minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][0];
                _minimapMenu.MinimapScaleY.Value = (decimal)_minimap[lastMinimapID].texScaleMulti[lastMinimapNameID][1];

            }
        }
        private void RotateScaleImage(object? sender, EventArgs e)
        {
            int useCellY = 0;
            int useCellX = 0;
            if (sender != _minimapMenu.UnevenSizeSplit)
            {
                rot++;
            }
            rot = rot % 4;
            if (rot > 1)
            {
                useCellY++;
            }
            if (0 < rot && rot < 3)
            {
                useCellX++;
            }
            Matrix renderTransform = Matrix.Identity,
                rotation = Matrix.CreateRotation(Math.PI*rot/2),
                translation = Matrix.CreateTranslation(new Vector((18*_rooms[lastMinimapID+1].sizeX)*cellsizeX*useCellX, 
                                                                  (18*_rooms[lastMinimapID+1].sizeY)*cellsizeY*useCellY)),
                scale = Matrix.CreateScale(1, 1);
            renderTransform *= rotation;
            renderTransform *= translation;
            renderTransform *= scale;

            _minimapMenu.CurrentTileScale.SetValue(Image.RenderTransformProperty, new MatrixTransform(renderTransform));
            if (_minimapMenu.UnevenSizeSplit.IsChecked == false && sender != _minimapMenu.UnevenSizeSplit)
            {
                // If this is not set, then the arrangement on the minimap will not be rotated
                int temporary = _minimapMenu.CurrentTileScale.GetValue(Grid.ColumnSpanProperty);
                _minimapMenu.CurrentTileScale.SetValue(Grid.ColumnSpanProperty, _minimapMenu.CurrentTileScale.GetValue(Grid.RowSpanProperty));
                _minimapMenu.CurrentTileScale.SetValue(Grid.RowSpanProperty, temporary);
            }

        }
        private async void LoadNewMinimapImage(object? sender, EventArgs e)
        {
            var file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select image to load",
                AllowMultiple=false,
                FileTypeFilter = new[] { MinimapTile }
            });

            if (file.Count > 0)
            {
                string name;
                if (_minimap[lastMinimapID].multipleNames)
                {
                    name = _minimap[lastMinimapID].names[lastMinimapNameID];
                }
                else
                {
                    name = _minimap[lastMinimapID].name;
                }
                await using var stream = await file[0].OpenReadAsync();
                Bitmap currentImage = new Bitmap(stream);
                _minimapTiles[name] = currentImage;

                // An ad hoc way to refresh the selected image, but it works
                ChangeActiveMinimap(_minimapMenu.MinimapID, null);
            }

        }
    }
}
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonBuilder.modules.window_objects
{
    public class MinimapMenu
    {
        public NumericUpDown MinimapID {  get; set; }
        public Image CurrentTileTexCoord { get; set; }
        public Rectangle TileTextCoordOutline { get; set; }
        public Image CurrentTileScale { get; set; }
        public Grid TextureScaleGrid { get; set; }
        public Grid ScaleGridVisual { get; set; }
        public NumericUpDown MinimapTileNameSelection { get; set; }
        public NumericUpDown MinimapTexCoordStartX { get; set; }
        public NumericUpDown MinimapTexCoordStartY { get; set; }
        public NumericUpDown MinimapTexCoordEndX { get; set; }
        public NumericUpDown MinimapTexCoordEndY { get; set; }
        public NumericUpDown MinimapScaleX { get; set; }
        public NumericUpDown MinimapScaleY { get; set; }
        public Button RotateScale { get; set; }
        public ToggleButton UnevenSizeSplit { get; set; }
    }
}

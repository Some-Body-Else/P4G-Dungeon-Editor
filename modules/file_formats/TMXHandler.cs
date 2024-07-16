using Avalonia.Media.Imaging;
using System;
using System.IO;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform;
using Colour = SkiaSharp.SKColor;

namespace DungeonBuilder.modules.file_formats
{
    /*
     * Tried using the converter designed by ltsophia, but the colors were getting messed up + haloing when treating as a bustup,
     * while trying to convert as a regular texture broke them when loading into the game. As such, I'm making my own solution for this.
     * 
     * I did look at ltsophia's converter for format references, shoutouts to them.
     */
    public static class TMXHandler
    {
        public static SMAP file;
        public struct SMAP
        {
            public List<TMX> tiles;
            public SMAP()
            {
                tiles = new List<TMX>();
            }
        }
        public struct TMX
        {
            public string Name;            // Name used with smap.bin

            public ushort FileID;          // Always 0x0002
            public ushort UserID;          // Always 0x0000 for minimap textures
            public uint FileSize;          // Calculated during runtime
            public uint MagicNum;          // Always 0x03584D5f (TMX0)
                                           // 4-byte spacer
            public ushort Flag;            // Always 0x0001 for minimap textures
            public ushort ImgWidth;        // Must be divisible by 8
            public ushort ImgHeight;       // Must be divisible by 2
                                           // 2-byte spacer
            public uint AlwaysFF000000;      // From the examples I've seen
                                           // Then space is filled until the header is 64-bytes large


            public Colour[] Palette;       // 16-color palette, each color being 4 bytes (r, g, b, a)

            public List<byte> Data;        // Our image data, 4bpp, each pixel referencing the color palette above

            public TMX(string name, ushort width, ushort height, List<Colour> colors, List<byte> image)
            {
                // TODO: Fill out data here
                Name = name;
                FileID = 0x0002;
                UserID = 0x0000;
                // 64-byte header, 64-bytes for palettes, image contains the rest of the data
                FileSize = (uint)(64 + 64 + image.Count);
                ImgWidth = width;
                ImgHeight = height;
                MagicNum = 0x30584D54;
                Flag = 0x00001;

                AlwaysFF000000 = 0xFF000000;
                Palette = new Colour[16];
                for (int i = 0; i < 16; i++)
                {
                    if (i <colors.Count)
                    {
                        Palette[i] = colors[i];
                    }
                    else
                    {
                        // Unused palette
                        Palette[i] = new Colour(0x00000080);
                    }
                }
                Data = image;
            }
        }

        public static void Initialize()
        {
            file = new();
        }

        public static void WriteSmap(BinaryWriter writer)
        {
            writer.Write(file.tiles.Count);
            foreach (TMX texture in file.tiles)
            {
                int name_size = 32;
                uint file_size = texture.FileSize;
                name_size -= texture.Name.Length;
                writer.Write(Encoding.ASCII.GetBytes(texture.Name));
                for (int i = 0; i < name_size; i++)
                {
                    writer.Write(false);
                }
                // Filesize used for .bin setuo
                writer.Write(texture.FileSize);
                // TMX writing starts here
                writer.Write(texture.FileID);
                writer.Write(texture.UserID);
                writer.Write(texture.FileSize);
                writer.Write(texture.MagicNum);
                writer.Write((int)0);
                writer.Write(texture.Flag);
                writer.Write(texture.ImgWidth);
                writer.Write(texture.ImgHeight);
                writer.Write((short)0x0014);
                writer.Write(texture.AlwaysFF000000);
                // used 28 of 64 header bytes, need to fill out the rest.
                for (int i = 0; i < 9; i++)
                {
                    writer.Write((int)0);
                }
                foreach(Colour color in texture.Palette)
                {
                    writer.Write(color.Red);
                    writer.Write(color.Green);
                    writer.Write(color.Blue);
                    writer.Write((byte)Math.Round((double)color.Alpha/2));
                }
                // Wrote out 2 64-byte sections
                file_size -= 128;
                foreach (byte entry in texture.Data)
                {
                    writer.Write(entry);
                    file_size--;
                }
                for (int i = 0; i < file_size; i++)
                {
                    writer.Write(false);
                }
            }
            writer.Close();
        }

        public static void CreateTMX(Bitmap image, string name)
        {
            Dictionary<Colour, int> paletteDict = new();
            List<Colour> palette = new();
            List<byte> img_contents = new();
            Stream img_data = new MemoryStream();
            ushort width = (ushort)image.PixelSize.Width;
            ushort height = (ushort)image.PixelSize.Height;
            int counter = 0;
            bool UpperBit = false;
            byte value = 0;
            image.Save(img_data);
            img_data.Seek(0, SeekOrigin.Begin);
            SkiaSharp.SKBitmap local_bitmap = SKBitmap.Decode(img_data, new SKImageInfo(image.PixelSize.Width, 
                                                              image.PixelSize.Height));
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!paletteDict.ContainsKey(local_bitmap.GetPixel(x, y)))
                    {
                        paletteDict.Add(local_bitmap.GetPixel(x, y), counter);
                        palette.Add(local_bitmap.GetPixel(x, y));
                        counter++;
                    }
                }
            }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (UpperBit)
                    {
                        value |= (byte)((paletteDict[local_bitmap.GetPixel(x, y)] << 4) & 0xF0);
                        img_contents.Add(value);
                        value = 0;
                        UpperBit = false;
                    }
                    else
                    {
                        value |= (byte)(paletteDict[local_bitmap.GetPixel(x, y)] & 0xF);
                        UpperBit = true;
                    }
                }
            }

            TMX newFile = new(name, width, height, palette, img_contents);
            file.tiles.Add(newFile);
        }
    }
}

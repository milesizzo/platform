﻿using CommonLibrary;
using GameEngine.Templates;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public enum MaterialType : int
    {
        Dirt,
        Water,
    }

    public interface ITile
    {
        ISpriteTemplate GetSprite(BlockStore store);

        string DebugString { get; }
    }

    public class Material : ITile
    {
        public MaterialType Type;

        public ISpriteTemplate GetSprite(BlockStore store)
        {
            var ids = store.Blocks[this.Type];
            if (!ids.Any())
            {
                return null;
            }
            var id = ids[this.GetHashCode() % ids.Count];
            return store.Tiles[id];
        }

        public string DebugString
        {
            get { return $"{this.Type.ToString().First()}"; }
        }
    }

    public class Block : ITile
    {
        public int Id;

        public ISpriteTemplate GetSprite(BlockStore store)
        {
            return this.Id < store.Tiles.Count ? store.Tiles[this.Id] : null;
        }

        public string DebugString
        {
            get { return $"{this.Id}"; }
        }
    }

    public class MapCell
    {
        public readonly HashSet<ITile> Background = new HashSet<ITile>();
        public readonly HashSet<ITile> Foreground = new HashSet<ITile>();
        public ITile Block = null;
    }

    public class MapRow
    {
        public readonly List<MapCell> Columns = new List<MapCell>();
    }

    public class BlockStore
    {
        public readonly DefaultDictionary<MaterialType, List<int>> Blocks = new DefaultDictionary<MaterialType, List<int>>(m => new List<int>());
        public readonly List<ISpriteTemplate> Tiles = new List<ISpriteTemplate>();

        public BlockStore()
        {
            //
        }
    }

    public class TileMap
    {
        public readonly List<MapRow> Rows = new List<MapRow>();
        public readonly int TileSize;
        public readonly int Width;
        public readonly int Height;
        public Color BackgroundColour = Color.CornflowerBlue;

        public TileMap(int width, int height, int tileSize)
        {
            this.Width = width;
            this.Height = height;
            for (var y = 0; y < height; y++)
            {
                var row = new MapRow();
                for (var x = 0; x < width; x++)
                {
                    row.Columns.Add(new MapCell());
                }
                this.Rows.Add(row);
            }
            this.TileSize = tileSize;
        }

        public MapCell this[int y, int x]
        {
            get { return this.Rows[y].Columns[x]; }
        }

        public MapCell this[Point p]
        {
            get { return this[p.Y, p.X]; }
        }

        public bool IsPassable(Point p)
        {
            return this.IsPassable(p.X, p.Y);
        }

        public bool IsPassable(int x, int y)
        {
            //return !this.BlockingTiles.Overlaps(this[y, x].Foreground);
            if (x < 0 || y < 0 || x >= this.Width || y >= this.Height) return false;
            return this[y, x].Block == null;
        }

        public bool IsPassable(Point first, Point second)
        {
            for (var y = first.Y; y <= second.Y; y++)
            {
                for (var x = first.X; x <= second.X; x++)
                {
                    if (!this.IsPassable(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void SaveToImage(GraphicsDevice graphics, string filename)
        {
            /*using (var bitmap = new System.Drawing.Bitmap(this.Width * 2, this.Height * 2))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(System.Drawing.Color.CornflowerBlue);
                }
                bitmap.Save(filename, ImageFormat.Png);
            }*/
            var texture = new Texture2D(graphics, this.Width, this.Height);
            var data = new Color[this.Width * this.Height];
            var offset = 0;
            for (var y = 0; y < this.Height; y++)
            {
                for (var x = 0; x < this.Width; x++)
                {
                    var block = this[y, x].Block;
                    if (block != null)
                    {
                        data[offset] = Color.Green;
                    }
                    else
                    {
                        data[offset] = Color.CornflowerBlue;
                    }
                    offset++;
                }
            }
            texture.SetData(data);
            using (var stream = File.OpenWrite(filename))
            {
                texture.SaveAsPng(stream, this.Width, this.Height);
            }
        }
    }

    public static class BinTileMapSerializer
    {
        public const byte TagTileNone = 0;
        public const byte TagTileMaterial = 1;
        public const byte TagTileBlock = 2;

        public static void Save(string filename, TileMap map)
        {
            using (var stream = File.OpenWrite(filename))
            using (var writer = new BinaryWriter(stream))
            {
                Save(writer, map);
            }
        }

        public static void Save(BinaryWriter writer, TileMap map)
        {
            writer.Write((Int32)map.Width);
            writer.Write((Int32)map.Height);
            writer.Write((byte)map.TileSize);
            writer.Write((UInt32)map.BackgroundColour.PackedValue);
            foreach (var row in map.Rows)
            {
                foreach (var cell in row.Columns)
                {
                    writer.Write((byte)cell.Foreground.Count);
                    foreach (var tile in cell.Foreground)
                    {
                        SaveTile(writer, tile);
                    }
                    writer.Write((byte)cell.Background.Count);
                    foreach (var tile in cell.Background)
                    {
                        SaveTile(writer, tile);
                    }
                    SaveTile(writer, cell.Block);
                }
            }
        }

        private static void SaveTile(BinaryWriter writer, ITile tile)
        {
            var asMaterial = tile as Material;
            var asBlock = tile as Block;
            if (asMaterial != null)
            {
                writer.Write((byte)TagTileMaterial);
                writer.Write((byte)asMaterial.Type);
            }
            else if (asBlock != null)
            {
                writer.Write((byte)TagTileBlock);
                writer.Write((Int32)asBlock.Id);
            }
            else
            {
                writer.Write((byte)TagTileNone);
            }
        }

        public static TileMap Load(string filename)
        {
            using (var stream = File.OpenRead(filename))
            using (var reader = new BinaryReader(stream))
            {
                return Load(reader);
            }
        }

        public static TileMap Load(BinaryReader reader)
        {
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var tileSize = reader.ReadByte();
            var result = new TileMap(width, height, tileSize);

            var bgcolour = new Color(reader.ReadUInt32());
            result.BackgroundColour = bgcolour;

            foreach (var row in result.Rows)
            {
                foreach (var cell in row.Columns)
                {
                    var foregroundCount = reader.ReadByte();
                    while (foregroundCount > 0)
                    {
                        foregroundCount--;
                        var tile = LoadTile(reader);
                        if (tile != null) cell.Foreground.Add(tile);
                    }
                    var backgroundCount = reader.ReadByte();
                    while (backgroundCount > 0)
                    {
                        var tile = LoadTile(reader);
                        if (tile != null) cell.Background.Add(tile);
                        backgroundCount--;
                    }
                    cell.Block = LoadTile(reader);
                }
            }
            return result;
        }

        private static ITile LoadTile(BinaryReader reader)
        {
            var tag = reader.ReadByte();
            switch (tag)
            {
                case TagTileNone:
                    return null;
                case TagTileMaterial:
                    var type = reader.ReadByte();
                    return new Material { Type = (MaterialType)type };
                case TagTileBlock:
                    var id = reader.ReadInt32();
                    return new Block { Id = id };
                default:
                    throw new InvalidOperationException($"Unknown tile type: {tag}");
            }
        }
    }
}
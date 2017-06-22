using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CommonLibrary;
using GameEngine.Content;
using GameEngine.Templates;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace Platform
{
    [Flags]
    public enum TileFlags : int
    {
        None = 0,
        Water = 1 << 0,
        OneWay = 1 << 1,
        Ladder = 1 << 2,
        SlopeLU = 1 << 3,
        SlopeUL = 1 << 4,
        SlopeLUReversed = 1 << 5,
        SlopeULReversed = 1 << 6,
        StepsLU = 1 << 7,
        StepsUL = 1 << 8,
    }

    public class BlockStore
    {
        public readonly int TileSize;
        public readonly int QuarterTileSize;
        public readonly DefaultDictionary<MaterialType, List<int>> Materials = new DefaultDictionary<MaterialType, List<int>>(m => new List<int>());
        public readonly List<ISpriteTemplate> Tiles = new List<ISpriteTemplate>();
        public readonly Dictionary<string, VisibleObjectPrefab> Prefabs = new Dictionary<string, VisibleObjectPrefab>();
        private readonly Dictionary<int, TileFlags> idToFlags = new Dictionary<int, TileFlags>();

        public BlockStore(int tileSize)
        {
            this.TileSize = tileSize;
            this.QuarterTileSize = tileSize / 4;
        }

        public void DrawTile(SpriteBatch sb, Vector2 pos, ITile tile, float depth, Color colour, Vector2? scale = null)
        {
            if (tile == null) return;
            if (!tile.Draw(this, sb, pos, colour, depth, scale))
            {
                sb.DrawRectangle(pos, new Size2(this.TileSize, this.TileSize), Color.Red);
                var font = Store.Instance.Fonts("Base", "debug.small");
                var s = $"{tile.DebugString}";
                font.DrawString(sb, pos + new Vector2(this.TileSize / 2) - font.Font.MeasureString(s) / 2, s, Color.Yellow);
            }
        }

        public void SetFlags(TileFlags type, params int[] ids)
        {
            foreach (var id in ids)
            {
                this[id] = type;
            }
        }

        public TileFlags this[int id]
        {
            get
            {
                TileFlags value;
                if (this.idToFlags.TryGetValue(id, out value))
                {
                    return value;
                }
                return TileFlags.None;
            }
            set
            {
                this.idToFlags[id] = value;
            }
        }

        // NOTE: this is for serialization only
        internal IEnumerable<KeyValuePair<int, TileFlags>> Flags
        {
            get { return this.idToFlags.OrderBy(kvp => kvp.Key); }
        }
    }

    public static class BinSpriteSerializer
    {
        public const byte TagSpriteFromSheet = 1;
        public const byte TagGeneralSprite = 2;

        public static void Save(BinaryWriter writer, ISpriteTemplate sprite)
        {
            var asSheet = sprite as SingleSpriteFromSheetTemplate;
            if (asSheet != null)
            {
                var index = asSheet.Parent.IndexOf(asSheet);
                writer.Write((byte)TagSpriteFromSheet);
                writer.Write(asSheet.Parent.Name);
                writer.Write((Int32)index);
            }
            else
            {
                writer.Write((byte)TagGeneralSprite);
                writer.Write(sprite.Name);
            }
        }

        public static ISpriteTemplate Load(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case TagSpriteFromSheet:
                    var parent = reader.ReadString();
                    var index = reader.ReadInt32();
                    return (Store.Instance.Sprites(parent) as SpriteSheetTemplate).Sprites[index];
                case TagGeneralSprite:
                    var name = reader.ReadString();
                    return Store.Instance.Sprites(name);
                default:
                    throw new InvalidOperationException("Unknown sprite type");
            }
        }
    }

    public static class BinBlockStoreSerializer
    {
        public const byte Version = 1;

        public static void Save(BinaryWriter writer, BlockStore store)
        {
            writer.Write((byte)Version);
            writer.Write((Int32)store.TileSize);
            writer.Write((Int32)store.Tiles.Count);
            foreach (var tile in store.Tiles)
            {
                BinSpriteSerializer.Save(writer, tile);
            }
            writer.Write((Int32)store.Materials.Count);
            foreach (var kvp in store.Materials)
            {
                writer.Write((Int32)kvp.Key);
                writer.Write((Int32)kvp.Value.Count);
                foreach (var item in kvp.Value)
                {
                    writer.Write((Int32)item);
                }
            }
            var flags = store.Flags.ToList();
            writer.Write((Int32)flags.Count);
            foreach (var kvp in flags)
            {
                writer.Write((Int32)kvp.Key);
                writer.Write((Int32)kvp.Value);
            }
        }

        public static BlockStore Load(BinaryReader reader)
        {
            var version = reader.ReadByte();
            if (version != Version)
            {
                throw new InvalidOperationException("Invalid blockstore version");
            }
            var tilesize = reader.ReadInt32();
            var result = new BlockStore(tilesize);
            var numTiles = reader.ReadInt32();
            while (numTiles-- > 0)
            {
                var sprite = BinSpriteSerializer.Load(reader);
                result.Tiles.Add(sprite);
            }
            var numMaterials = reader.ReadInt32();
            while (numMaterials-- > 0)
            {
                var material = (MaterialType)reader.ReadInt32();
                var numMaterialTiles = reader.ReadInt32();
                while (numMaterialTiles-- > 0)
                {
                    var id = reader.ReadInt32();
                    result.Materials[material].Add(id);
                }
            }
            var numFlags = reader.ReadInt32();
            while (numFlags-- > 0)
            {
                var id = reader.ReadInt32();
                var flags = (TileFlags)reader.ReadInt32();
                result[id] = flags;
            }
            return result;
        }
    }
}

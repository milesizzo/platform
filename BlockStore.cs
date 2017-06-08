using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CommonLibrary;
using GameEngine.Content;
using GameEngine.Templates;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Platform
{
    [Flags]
    public enum TileFlags
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
        public readonly DefaultDictionary<MaterialType, List<int>> Materials = new DefaultDictionary<MaterialType, List<int>>(m => new List<int>());
        public readonly List<ISpriteTemplate> Tiles = new List<ISpriteTemplate>();
        public readonly Dictionary<string, VisibleObjectPrefab> Prefabs = new Dictionary<string, VisibleObjectPrefab>();
        private readonly Dictionary<int, TileFlags> idToFlags = new Dictionary<int, TileFlags>();

        public BlockStore(int tileSize)
        {
            this.TileSize = tileSize;
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
}

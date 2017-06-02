using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CommonLibrary;
using GameEngine.Content;
using GameEngine.Templates;
using MonoGame.Extended;
using System.Collections.Generic;

namespace Platform
{
    public class BlockStore
    {
        public readonly int TileSize;
        public readonly DefaultDictionary<MaterialType, List<int>> Blocks = new DefaultDictionary<MaterialType, List<int>>(m => new List<int>());
        public readonly List<ISpriteTemplate> Tiles = new List<ISpriteTemplate>();
        public readonly Dictionary<string, VisibleObjectPrefab> Prefabs = new Dictionary<string, VisibleObjectPrefab>();

        public BlockStore(int tileSize)
        {
            this.TileSize = tileSize;
        }

        public void DrawTile(SpriteBatch sb, Vector2 pos, ITile tile, float depth, Color colour)
        {
            if (tile == null) return;
            if (!tile.Draw(this, sb, pos, colour, depth))
            {
                sb.DrawRectangle(pos, new Size2(this.TileSize - 1, this.TileSize - 1), Color.Red);
                var font = Store.Instance.Fonts("Base", "debug.small");
                var s = $"{tile.DebugString}";
                font.DrawString(sb, pos + new Vector2(this.TileSize / 2) - font.Font.MeasureString(s) / 2, s, Color.Yellow);
            }
        }
    }
}

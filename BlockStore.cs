using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CommonLibrary;
using GameEngine.Content;
using GameEngine.Templates;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Linq;

namespace Platform
{
    public class BlockStore
    {
        public readonly int TileSize;
        public readonly DefaultDictionary<MaterialType, List<int>> Materials = new DefaultDictionary<MaterialType, List<int>>(m => new List<int>());
        public readonly List<ISpriteTemplate> Tiles = new List<ISpriteTemplate>();
        public readonly Dictionary<string, VisibleObjectPrefab> Prefabs = new Dictionary<string, VisibleObjectPrefab>();
        private readonly Dictionary<int, MaterialType> idToMaterial = new Dictionary<int, MaterialType>();
        //private readonly DefaultDictionary<MaterialType, HashSet<int>> materialToId = new DefaultDictionary<MaterialType, HashSet<int>>(m => new HashSet<int>());

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

        public void SetMaterial(MaterialType type, params int[] ids)
        {
            foreach (var id in ids)
            {
                this[id] = type;
            }
        }

        public MaterialType this[int id]
        {
            get
            {
                MaterialType value;
                if (this.idToMaterial.TryGetValue(id, out value))
                {
                    return value;
                }
                return MaterialType.None;
            }
            set
            {
                this.idToMaterial[id] = value;
                //this.materialToId[value].Add(id);
            }
        }

        /*public IEnumerable<int> this[MaterialType material]
        {
            get
            {
                return this.materialToId[material];
            }
        }*/
    }
}

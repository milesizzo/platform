using GameEngine.Templates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Platform
{
    public enum MaterialType : int
    {
        Dirt,
        Water,
        Grass,
    }

    public interface ITile
    {
        bool Draw(BlockStore store, SpriteBatch sb, Vector2 pos, Color colour, float depth);

        string DebugString { get; }

        ITile Clone();
    }

    public class Material : ITile
    {
        public MaterialType Type;

        private ISpriteTemplate GetSprite(BlockStore store)
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

        public ITile Clone()
        {
            return new Material { Type = this.Type };
        }

        public bool Draw(BlockStore store, SpriteBatch sb, Vector2 pos, Color colour, float depth)
        {
            var sprite = this.GetSprite(store);
            if (sprite == null) return false;
            sprite.DrawSprite(sb, 0, pos, colour, 0, Vector2.One, SpriteEffects.None, depth);
            return true;
        }
    }

    public class Block : ITile
    {
        public int Id;

        public string DebugString
        {
            get { return $"{this.Id}"; }
        }

        public ITile Clone()
        {
            return new Block { Id = this.Id };
        }

        public bool Draw(BlockStore store, SpriteBatch sb, Vector2 pos, Color colour, float depth)
        {
            if (this.Id >= store.Tiles.Count) return false;
            var sprite = store.Tiles[this.Id];
            sprite.DrawSprite(sb, 0, pos, colour, 0, Vector2.One, SpriteEffects.None, depth);
            return true;
        }
    }
}

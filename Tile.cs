﻿using GameEngine.Templates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform
{
    public enum MaterialType
    {
        None,
        Dirt,
    }

    public interface ITile
    {
        bool Draw(BlockStore store, SpriteBatch sb, Vector2 pos, Color colour, float depth, Vector2? scale = null);

        string DebugString { get; }

        ITile Clone();
    }

    public class Material : ITile
    {
        public MaterialType Type;

        public Material(MaterialType type)
        {
            this.Type = type;
        }

        private ISpriteTemplate GetSprite(BlockStore store)
        {
            var ids = store.Materials[this.Type];
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
            return new Material(this.Type);
        }

        public bool Draw(BlockStore store, SpriteBatch sb, Vector2 pos, Color colour, float depth, Vector2? scale = null)
        {
            var sprite = this.GetSprite(store);
            if (sprite == null) return false;
            sprite.DrawSprite(sb, 0, pos, colour, 0, scale ?? Vector2.One, SpriteEffects.None, depth);
            return true;
        }
    }

    public class Tile : ITile
    {
        public int Id;

        public Tile(int id)
        {
            this.Id = id;
        }

        public string DebugString
        {
            get { return $"{this.Id}"; }
        }

        public ITile Clone()
        {
            return new Tile(this.Id);
        }

        public bool Draw(BlockStore store, SpriteBatch sb, Vector2 pos, Color colour, float depth, Vector2? scale = null)
        {
            if (this.Id >= store.Tiles.Count) return false;
            var sprite = store.Tiles[this.Id];
            sprite.DrawSprite(sb, 0, pos, colour, 0, scale ?? Vector2.One, SpriteEffects.None, depth);
            return true;
        }

        public static implicit operator Tile(int id)
        {
            return new Tile(id);
        }
    }
}

using GameEngine.Templates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using System;

namespace Platform.Editor
{
    public class TileStencil
    {
        public enum Layer
        {
            Background,
            Foreground,
            Blocking
        }

        internal readonly Dictionary<Point, ITile> tiles = new Dictionary<Point, ITile>();

        public ITile this[int x, int y]
        {
            set { this.tiles[new Point(x, y)] = value; }
        }

        public ITile this[Point p]
        {
            set { this[p.X, p.Y] = value; }
        }

        public void AddRow(int x, params int[] blocks)
        {
            var maxY = this.tiles.Any() ? this.tiles.Keys.Max(p => p.Y) : 0;
            foreach (var block in blocks)
            {
                this[x, maxY + 1] = new Block { Id = block };
                x++;
            }
        }

        public void Draw(SpriteBatch sb, Vector2 pos, BlockStore blocks)
        {
            foreach (var kvp in this.tiles)
            {
                var p = kvp.Key;
                var tile = kvp.Value;
                blocks.DrawTile(sb, pos + new Vector2(p.X * blocks.TileSize, p.Y * blocks.TileSize), tile, 0f, Color.White);
            }
        }

        public void Stamp(TileMap map, Point pos, Layer layer)
        {
            foreach (var kvp in this.tiles)
            {
                var p = kvp.Key;
                var tile = kvp.Value.Clone();
                var location = p + pos;
                if (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height)
                {
                    continue;
                }
                var cell = map[p + pos];
                switch (layer)
                {
                    case Layer.Background:
                        cell.Background.Add(tile);
                        break;
                    case Layer.Foreground:
                        cell.Foreground.Add(tile);
                        break;
                    case Layer.Blocking:
                        cell.Block = tile;
                        break;
                }
            }
        }

        // NOTE: this should only be used in serialization!
        public IReadOnlyList<KeyValuePair<Point, ITile>> Tiles
        {
            get { return this.tiles.ToList(); }
        }
    }

    /*public interface ISpriteTemplate : ITemplate
    {
        Texture2D Texture { get; set; }

        int NumberOfFrames { get; }

        int Width { get; }

        int Height { get; }

        int FPS { get; set; }

        Vector2 Origin { get; set; }

        void DrawSprite(SpriteBatch sb, int frame, Vector2 position, Color colour, float rotation, Vector2 scale, SpriteEffects effects, float depth);

        Shape Shape { get; set; }
    }*/

    public class TileStencilSprite : ISpriteTemplate
    {
        private readonly TileStencil stencil;
        private readonly BlockStore blocks;

        public TileStencilSprite(TileStencil stencil, BlockStore blocks)
        {
            this.stencil = stencil;
            this.blocks = blocks;
        }

        public int FPS
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Height
        {
            get
            {
                return this.stencil.tiles.Keys.Max(p => p.Y) * this.blocks.TileSize;
            }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public int NumberOfFrames
        {
            get { throw new NotImplementedException(); }
        }

        public Vector2 Origin
        {
            get { return Vector2.Zero; }
            set { throw new NotImplementedException(); }
        }

        public Shape Shape
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Texture2D Texture
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Width
        {
            get
            {
                return this.stencil.tiles.Keys.Max(p => p.X) * this.blocks.TileSize;
            }
        }

        public void DrawSprite(SpriteBatch sb, int frame, Vector2 position, Color colour, float rotation, Vector2 scale, SpriteEffects effects, float depth)
        {
            this.stencil.Draw(sb, position, this.blocks);
        }
    }
}

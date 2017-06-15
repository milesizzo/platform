using GameEngine.Templates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using System;

namespace Platform.Editor
{
    public interface Stampable
    {
        void Stamp(PlatformContext context, Vector2 world);
    }

    public class TileStencil
    {
        public enum Layer
        {
            Background,
            Foreground,
            Blocking,
        }

        internal readonly Dictionary<Point, ITile> tiles = new Dictionary<Point, ITile>();

        public Point Origin = Point.Zero;

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
                this[x, maxY + 1] = new Tile(block);
                x++;
            }
        }

        public void Draw(SpriteBatch sb, Vector2 pos, Vector2 scale, Color colour, BlockStore blocks)
        {
            foreach (var kvp in this.tiles)
            {
                var p = kvp.Key - this.Origin;
                var tile = kvp.Value;
                blocks.DrawTile(sb, pos + new Vector2(p.X * blocks.TileSize * scale.X, p.Y * blocks.TileSize * scale.Y), tile, 0f, colour, scale);
            }
        }

        public void Stamp(TileMap map, Point pos, Layer layer)
        {
            foreach (var kvp in this.tiles)
            {
                var p = kvp.Key - this.Origin;
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

        public ISpriteTemplate ToSprite(BlockStore blocks)
        {
            var sprite = new TileStencilSprite(this, blocks);
            return sprite;
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
        private Vector2 origin = Vector2.Zero;

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
                return (this.stencil.tiles.Keys.Max(p => p.Y) + 1) * this.blocks.TileSize;
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
            get { return this.origin; }
            set { this.origin = value; }
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
                return (this.stencil.tiles.Keys.Max(p => p.X) + 1) * this.blocks.TileSize;
            }
        }

        public void DrawSprite(SpriteBatch sb, int frame, Vector2 position, Color colour, float rotation, Vector2 scale, SpriteEffects effects, float depth)
        {
            foreach (var kvp in this.stencil.tiles)
            {
                var p = kvp.Key;
                var tile = kvp.Value;
                this.blocks.DrawTile(sb,
                    position + new Vector2(p.X * this.blocks.TileSize * scale.X, p.Y * this.blocks.TileSize * scale.Y) - this.origin,
                    tile,
                    0f,
                    colour,
                    scale);
            }
            //this.stencil.Draw(sb, position - this.origin, scale, colour, this.blocks);
        }
    }
}

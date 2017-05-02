using GameEngine.Content;
using GameEngine.GameObjects;
using GameEngine.Graphics;
using GameEngine.Templates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public class MapCell
    {
        public readonly HashSet<int> Background = new HashSet<int>();
        public readonly HashSet<int> Foreground = new HashSet<int>();
    }

    public class MapRow
    {
        public readonly List<MapCell> Columns = new List<MapCell>();
    }

    public class TileMap
    {
        public readonly HashSet<int> BlockingTiles = new HashSet<int>();
        public readonly List<MapRow> Rows = new List<MapRow>();
        public readonly List<SpriteTemplate> Sprites = new List<SpriteTemplate>();
        public readonly int TileSize;
        public readonly int Width;
        public readonly int Height;

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
            return !this[y, x].Foreground.Any();
        }
    }

    public class PlatformContext : GameContext
    {
        public readonly TileMap Map;
        public RectangleF VisibleBounds;
        private readonly Camera camera;

        public PlatformContext(Store store, Camera camera) : base(store)
        {
            this.Map = new TileMap(2048, 1024, 32);
            this.camera = camera;
        }

        public Point WorldToTile(Vector2 world)
        {
            return new Point((int)(world.X / this.Map.TileSize), (int)(world.Y / this.Map.TileSize));
        }

        public Vector2 TileToWorld(Point tile)
        {
            return this.TileToWorld(tile.X, tile.Y);
        }

        public Vector2 TileToWorld(int x, int y)
        {
            return new Vector2(x * this.Map.TileSize, y * this.Map.TileSize);
        }

        public static float ZToDepth(float z)
        {
            return z * 0.6f + 0.2f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            var topLeft = this.camera.ScreenToWorld(Vector2.Zero);
            var bottomRight = this.camera.ScreenToWorld(new Vector2(this.camera.Viewport.Width, this.camera.Viewport.Height));
            this.VisibleBounds = new RectangleF(topLeft, new Size2(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));

            base.Draw(renderer, gameTime);

            var topLeftTile = topLeft / new Vector2(this.Map.TileSize);
            var bottomRightTile = bottomRight / new Vector2(this.Map.TileSize);

            var minY = MathHelper.Max((int)topLeftTile.Y - 1, 0);
            var maxY = MathHelper.Min((int)bottomRightTile.Y + 1, this.Map.Height);
            var minX = MathHelper.Max((int)topLeftTile.X - 1, 0);
            var maxX = MathHelper.Min((int)bottomRightTile.X + 1, this.Map.Width);

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var cell = this.Map[y, x];
                    var pos = new Vector2(x * this.Map.TileSize, y * this.Map.TileSize);
                    foreach (var tileId in cell.Background)
                    {
                        this.Map.Sprites[tileId].DrawSprite(renderer.World, pos, 0.9f);
                    }
                    foreach (var tileId in cell.Foreground)
                    {
                        this.Map.Sprites[tileId].DrawSprite(renderer.World, pos, 0.1f);
                    }
                    //renderer.World.DrawRectangle(new RectangleF(pos, new Size2(this.Map.TileSize, this.Map.TileSize)), Color.White);
                }
            }
        }
    }
}

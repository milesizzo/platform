using CommonLibrary;
using GameEngine.Content;
using GameEngine.GameObjects;
using GameEngine.Graphics;
using GameEngine.Templates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public class PlatformContext : GameContext
    {
        public TileMap Map;
        public BlockStore BlockStore;
        public RectangleF VisibleBounds;
        private readonly Camera camera;
        private readonly List<Light> lights = new List<Light>();
        private bool lightsEnabled = false;
        private TimeSpan time = new TimeSpan(7, 0, 0); // start at 7am
        private Color[] ambientBackgroundAtHour = new Color[24]
        {
            Color.Black, // midnight
            Color.Black,
            new Color(0, 0, 0.1f),
            new Color(0, 0, 0.2f),
            Color.DarkBlue,
            Color.DarkBlue,
            Color.DarkBlue, // 6am
            new Color(50, 74, 246),
            Color.CornflowerBlue,
            Color.CornflowerBlue,
            Color.CornflowerBlue,
            Color.CornflowerBlue,
            Color.CornflowerBlue, // noon
            Color.CornflowerBlue,
            Color.CornflowerBlue,
            Color.CornflowerBlue,
            Color.CornflowerBlue,
            Color.CornflowerBlue,
            new Color(50, 74, 246), // 6pm
            Color.DarkBlue,
            Color.DarkBlue,
            new Color(0, 0, 0.2f),
            new Color(0, 0, 0.1f),
            Color.Black,
        };
        private Color[] ambientLightAtHour = new Color[24]
        {
            Color.Black, // midnight
            Color.Black,
            new Color(0, 0, 0.1f),
            new Color(0, 0, 0.2f),
            Color.DarkBlue,
            Color.DarkBlue,
            Color.DarkBlue, // 6am
            new Color(0.5f, 0.5f, 0.5f),
            Color.White,
            Color.White,
            Color.White,
            Color.White,
            Color.White, // noon
            Color.White,
            Color.White,
            Color.White,
            Color.White,
            Color.White,
            new Color(0.5f, 0.5f, 0.5f), // 6pm
            Color.DarkBlue,
            Color.DarkBlue,
            new Color(0, 0, 0.2f),
            new Color(0, 0, 0.1f),
            Color.Black,
        };
        private Color ambientBackground;
        private Color ambientLight;
        private bool enabled = true;

        public PlatformContext(Camera camera) : base()
        {
            this.camera = camera;
        }

        public PlatformContext(Camera camera, int width, int height) : base()
        {
            this.Map = new TileMap(width, height);
            this.camera = camera;
        }

        public bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        public Point WorldToTile(Vector2 world)
        {
            return new Point((int)(world.X / this.BlockStore.TileSize), (int)(world.Y / this.BlockStore.TileSize));
        }

        public Point WorldToTile(Vector2 world, out Vector2 offset)
        {
            var tile = this.WorldToTile(world);
            offset = new Vector2(world.X - tile.X * this.BlockStore.TileSize, world.Y - tile.Y * this.BlockStore.TileSize);
            return tile;
        }

        public Vector2 TileToWorld(Point tile)
        {
            return this.TileToWorld(tile.X, tile.Y);
        }

        public Vector2 TileToWorld(int x, int y)
        {
            return new Vector2(x * this.BlockStore.TileSize, y * this.BlockStore.TileSize);
        }

        public bool IsInBounds(Point tile)
        {
            return tile.X >= 0 && tile.X < this.Map.Width && tile.Y >= 0 && tile.Y < this.Map.Height;
        }

        public static float ZToDepth(float z)
        {
            return 0.7f - z * 0.6f;
        }

        public void AttachLightSource(IGameObject obj, Light light)
        {
            light.Owner = obj;
            this.lights.Add(light);
        }

        public void AttachLightSource(Light light)
        {
            this.lights.Add(light);
        }

        public bool LightsEnabled
        {
            get { return this.lightsEnabled; }
            set { this.lightsEnabled = value; }
        }

        public IEnumerable<Light> LightSources
        {
            get { return this.lights; }
        }

        public TimeSpan Time
        {
            get { return this.time; }
            set { this.time = value; }
        }

        private Color Lerp(Color curr, Color next, float scale)
        {
            var r = MathHelper.Lerp(curr.R, next.R, scale) / 255f;
            var g = MathHelper.Lerp(curr.G, next.G, scale) / 255f;
            var b = MathHelper.Lerp(curr.B, next.B, scale) / 255f;
            return new Color(r, g, b);
        }

        public Color AmbientBackground { get { return this.ambientBackground; } }

        public Color AmbientLight { get { return this.ambientLight; } }

        public Vector2 WorldToScreen(Vector2 pos)
        {
            return this.camera.WorldToScreen(pos);
        }

        public Vector2 ScreenToWorld(Vector2 pos)
        {
            return this.camera.ScreenToWorld(pos);
        }

        private TileFlags GetFlags(ITile tile)
        {
            var result = TileFlags.None;
            var asTile = tile as Tile;
            if (asTile != null)
            {
                result |= this.BlockStore[asTile.Id];
            }
            return result;
        }

        public TileFlags GetFlags(Point pos)
        {
            var cell = this.Map[pos];
            var result = cell.Foreground.Aggregate(TileFlags.None, (m, t) => m | this.GetFlags(t));
            result = cell.Background.Aggregate(result, (m, t) => m | this.GetFlags(t));
            result |= this.GetFlags(cell.Block);
            return result;
        }

        public TileFlags GetFlags(Point topLeft, Point bottomRight)
        {
            var result = TileFlags.None;
            for (var y = topLeft.Y; y <= bottomRight.Y; y++)
            {
                for (var x = topLeft.X; x <= bottomRight.X; x++)
                {
                    result |= this.GetFlags(new Point(x, y));
                }
            }
            return result;
        }

        public bool IsOneWayPlatform(int x, int y)
        {
            return this.GetFlags(new Point(x, y)).HasFlag(TileFlags.OneWay);
        }

        public bool IsOneWayPlatform(Point first, Point second)
        {
            return this.GetFlags(first, second).HasFlag(TileFlags.OneWay);
        }

        public bool IsLadder(int x, int y)
        {
            return this.GetFlags(new Point(x, y)).HasFlag(TileFlags.Ladder);
        }

        public bool IsLadder(Point first, Point second)
        {
            return this.GetFlags(first, second).HasFlag(TileFlags.Ladder);
        }

        public bool IsPassable(Point p)
        {
            return this.IsPassable(p.X, p.Y);
        }

        public bool IsPassable(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.Map.Width || y >= this.Map.Height) return false;
            return this.Map[y, x].Block == null;
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

        private float GetSurfaceSlope(float x)
        {
            return x;
        }

        private float GetSurfaceStep(float x)
        {
            return (float)Math.Floor(x / this.BlockStore.QuarterTileSize) * this.BlockStore.QuarterTileSize;
        }

        public bool IsPassable(Vector2 topLeft, Vector2 bottomRight)
        {
            Vector2 offsetBottomRight;
            if (this.IsPassable(this.WorldToTile(topLeft), this.WorldToTile(bottomRight, out offsetBottomRight)))
            {
                return true;
            }
            var slopeLU = false;
            var tile = this.Map[this.WorldToTile(bottomRight)].Block as Tile;
            if (tile != null)
            {
                if (this.BlockStore[tile.Id].HasFlag(TileFlags.SlopeLU))
                {
                    if (offsetBottomRight.Y - (this.BlockStore.TileSize - this.GetSurfaceSlope(offsetBottomRight.X)) > 0)
                    {
                        // we're in the slope
                        return false;
                    }
                    slopeLU = true;
                }
                if (this.BlockStore[tile.Id].HasFlag(TileFlags.StepsLU))
                {
                    if (offsetBottomRight.Y - (this.BlockStore.TileSize - this.GetSurfaceStep(offsetBottomRight.X)) > 0)
                    {
                        // we're in the step
                        return false;
                    }
                    slopeLU = true;
                }
            }
            Vector2 offsetBottomLeft;
            tile = this.Map[this.WorldToTile(new Vector2(topLeft.X, bottomRight.Y), out offsetBottomLeft)].Block as Tile;
            var slopeUL = false;
            if (tile != null)
            {
                if (this.BlockStore[tile.Id].HasFlag(TileFlags.SlopeUL))
                {
                    if (offsetBottomLeft.Y - this.GetSurfaceSlope(offsetBottomLeft.X) > 0)
                    {
                        // we're in the slope
                        return false;
                    }
                    slopeUL = true;
                }
                if (this.BlockStore[tile.Id].HasFlag(TileFlags.StepsUL))
                {
                    if (offsetBottomLeft.Y - this.GetSurfaceStep(offsetBottomLeft.X) > 0)
                    {
                        // we're in the step
                        return false;
                    }
                    slopeUL = true;
                }
            }
            Vector2 offsetTopLeft;
            tile = this.Map[this.WorldToTile(new Vector2(topLeft.X, topLeft.Y), out offsetTopLeft)].Block as Tile;
            var slopeLUReversed = false;
            if (tile != null)
            {
                if (this.BlockStore[tile.Id].HasFlag(TileFlags.SlopeLUReversed))
                {
                    if (this.GetSurfaceSlope(this.BlockStore.TileSize - offsetTopLeft.X) - offsetTopLeft.Y > 0)
                    {
                        // we're in the slope
                        return false;
                    }
                    slopeLUReversed = true;
                }
            }
            Vector2 offsetTopRight;
            tile = this.Map[this.WorldToTile(new Vector2(bottomRight.X, topLeft.Y), out offsetTopRight)].Block as Tile;
            var slopeULReversed = false;
            if (tile != null)
            {
                if (this.BlockStore[tile.Id].HasFlag(TileFlags.SlopeULReversed))
                {
                    if ((this.BlockStore.TileSize - this.GetSurfaceSlope(this.BlockStore.TileSize - offsetTopRight.X)) - offsetTopRight.Y > 0)
                    {
                        // we're in the slope
                        return false;
                    }
                    slopeULReversed = true;
                }
            }
            if (slopeLU || slopeUL || slopeULReversed || slopeLUReversed)
            {
                return true;
            }
            return false;
        }

        public float SlopeAmountRight(Vector2 bottomRight, int searchHeight)
        {
            // 1. check if bottomRight is in a slope tile
            // 2. check bit mask / function, return distance to slope
            Vector2 offset;
            var bottomRightTile = this.WorldToTile(bottomRight, out offset);
            var count = 0;
            while (count < searchHeight)
            {
                if (this.IsPassable(bottomRightTile))
                {
                    return count;
                }
                var tile = this.Map[bottomRightTile].Block as Tile;
                if (tile != null)
                {
                    if (this.BlockStore[tile.Id].HasFlag(TileFlags.SlopeLU))
                    {
                        return count + (offset.Y - (this.BlockStore.TileSize - this.GetSurfaceSlope(offset.X)));
                    }
                    if (this.BlockStore[tile.Id].HasFlag(TileFlags.StepsLU))
                    {
                        return count + (offset.Y - (this.BlockStore.TileSize - this.GetSurfaceStep(offset.X)));
                    }
                }
                count++;
                offset.Y -= 1;
                if (offset.Y < 0)
                {
                    offset.Y += this.BlockStore.TileSize;
                    bottomRightTile.Y--;
                }
            }
            return float.MaxValue;
        }

        public float SlopeAmountLeft(Vector2 bottomLeft, int searchHeight)
        {
            // 1. check if bottomLeft is in a slope tile
            // 2. check bit mask / function, return distance to slope
            Vector2 offset;
            var bottomLeftTile = this.WorldToTile(bottomLeft, out offset);
            var count = 0;
            while (count < searchHeight)
            {
                if (this.IsPassable(bottomLeftTile))
                {
                    return count;
                }
                var tile = this.Map[bottomLeftTile].Block as Tile;
                if (tile != null)
                {
                    if (this.BlockStore[tile.Id].HasFlag(TileFlags.SlopeUL))
                    {
                        return count + (offset.Y - this.GetSurfaceSlope(offset.X));
                    }
                    if (this.BlockStore[tile.Id].HasFlag(TileFlags.StepsUL))
                    {
                        return count + (offset.Y - this.GetSurfaceStep(offset.X));
                    }
                }
                count++;
                offset.Y -= 1;
                if (offset.Y < 0)
                {
                    offset.Y += this.BlockStore.TileSize;
                    bottomLeftTile.Y--;
                }
            }
            return float.MaxValue;
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Enabled)
            {
                base.Update(gameTime);
                this.time += TimeSpan.FromSeconds(gameTime.GetElapsedSeconds());
                foreach (var light in this.lights.Where(l => l.IsEnabled))
                {
                    light.Update(ref this.time, gameTime);
                }
            }

            var hour = this.time.Hours;
            var scale = (this.time.Minutes * 60f + this.time.Seconds + this.time.Milliseconds / 1000f) / 3600f;
            var nextHour = hour == 23 ? 0 : hour + 1;
            this.ambientLight = this.Lerp(this.ambientLightAtHour[hour], this.ambientLightAtHour[nextHour], scale);
            this.ambientBackground = this.Lerp(this.ambientBackgroundAtHour[hour], this.ambientBackgroundAtHour[nextHour], scale);
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            var topLeft = this.camera.ScreenToWorld(Vector2.Zero);
            var bottomRight = this.camera.ScreenToWorld(new Vector2(this.camera.Viewport.Width, this.camera.Viewport.Height));
            this.VisibleBounds = new RectangleF(topLeft, new Size2(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));

            base.Draw(renderer, gameTime);

            var topLeftTile = topLeft / new Vector2(this.BlockStore.TileSize);
            var bottomRightTile = bottomRight / new Vector2(this.BlockStore.TileSize);

            var minY = MathHelper.Max((int)topLeftTile.Y - 1, 0);
            var maxY = MathHelper.Min((int)bottomRightTile.Y + 1, this.Map.Height - 1);
            var minX = MathHelper.Max((int)topLeftTile.X - 1, 0);
            var maxX = MathHelper.Min((int)bottomRightTile.X + 1, this.Map.Width - 1);

            var pos = new Vector2(0, minY * this.BlockStore.TileSize);
            var startX = minX * this.BlockStore.TileSize;
            for (var y = minY; y <= maxY; y++)
            {
                pos.X = startX;
                for (var x = minX; x <= maxX; x++)
                {
                    var cell = this.Map[y, x];
                    if (cell.Background.Count > 0)
                    {
                        // (0.9, 0.8] is background
                        var diff = (1f / cell.Background.Count) * 0.01f;
                        var depth = 0.9f - diff;
                        foreach (var tile in cell.Background)
                        {
                            this.BlockStore.DrawTile(renderer.World, pos, tile, depth, Color.White);
                            depth -= diff;
                        }
                    }
                    this.BlockStore.DrawTile(renderer.World, pos, cell.Block, 0.25f, Color.White);
                    if (AbstractObject.DebugInfo && cell.Block != null)
                    {
                        Store.Instance.Sprites<ISpriteTemplate>("Base", "white-16x16").DrawSprite(renderer.World, 0, pos, Color.Red, 0, Vector2.One, SpriteEffects.None, 0.019f);
                    }
                    if (cell.Foreground.Count > 0)
                    {
                        // (0.2, 0.1] is foreground
                        var diff = (1f / cell.Foreground.Count) * 0.01f;
                        var depth = 0.2f - diff;
                        foreach (var tile in cell.Foreground)
                        {
                            this.BlockStore.DrawTile(renderer.World, pos, tile, depth, Color.White);
                            depth -= diff;
                        }
                    }
                    pos.X += this.BlockStore.TileSize;
                }
                pos.Y += this.BlockStore.TileSize;
            }
        }
    }
}

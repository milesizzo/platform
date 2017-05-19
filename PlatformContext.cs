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
        public readonly BlockStore BlockStore = new BlockStore();
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

        public PlatformContext(Store store, Camera camera) : base(store)
        {
            this.camera = camera;
        }

        public PlatformContext(Store store, Camera camera, int width, int height, int tilesize) : base(store)
        {
            this.Map = new TileMap(width, height, tilesize);
            this.camera = camera;
        }

        public bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
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

        public bool IsInBounds(Point tile)
        {
            return tile.X >= 0 && tile.X < this.Map.Width && tile.Y >= 0 && tile.Y < this.Map.Height;
        }

        public static float ZToDepth(float z)
        {
            return z * 0.6f + 0.2f;
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

        private void DrawTile(Renderer renderer, Vector2 pos, ITile tile, float depth, Color colour)
        {
            if (tile == null) return;

            var sprite = tile.GetSprite(this.BlockStore);
            if (sprite == null)
            {
                renderer.World.DrawRectangle(pos, new Size2(this.Map.TileSize - 1, this.Map.TileSize - 1), Color.Red);
                var font = this.Store.Fonts("Base", "debug.small");
                var s = $"{tile.DebugString}";
                font.DrawString(renderer.World, pos + new Vector2(this.Map.TileSize / 2) - font.Font.MeasureString(s) / 2, s, Color.Yellow);
            }
            else
            {
                sprite.DrawSprite(renderer.World, 0, pos, colour, 0, Vector2.One, SpriteEffects.None, depth);
            }
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
            var maxY = MathHelper.Min((int)bottomRightTile.Y + 1, this.Map.Height - 1);
            var minX = MathHelper.Max((int)topLeftTile.X - 1, 0);
            var maxX = MathHelper.Min((int)bottomRightTile.X + 1, this.Map.Width - 1);

            var pos = new Vector2(0, minY * this.Map.TileSize);
            var startX = minX * this.Map.TileSize;
            for (var y = minY; y <= maxY; y++)
            {
                pos.X = startX;
                for (var x = minX; x <= maxX; x++)
                {
                    var cell = this.Map[y, x];
                    foreach (var tile in cell.Background)
                    {
                        this.DrawTile(renderer, pos, tile, 0.9f, Color.White);
                    }
                    this.DrawTile(renderer, pos, cell.Block, 0.02f, Color.White);
                    foreach (var tile in cell.Foreground)
                    {
                        this.DrawTile(renderer, pos, tile, 0.01f, Color.White);
                    }
                    pos.X += this.Map.TileSize;
                }
                pos.Y += this.Map.TileSize;
            }
        }
    }
}

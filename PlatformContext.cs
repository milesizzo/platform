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

        public Vector2 WorldToScreen(Vector2 pos)
        {
            return this.camera.WorldToScreen(pos);
        }

        public Vector2 ScreenToWorld(Vector2 pos)
        {
            return this.camera.ScreenToWorld(pos);
        }

        private MaterialType GetMaterials(ITile tile)
        {
            var result = MaterialType.None;
            var asMaterial = tile as Material;
            var asTile = tile as Tile;
            if (asMaterial != null)
            {
                result |= asMaterial.Type;
            }
            else if (asTile != null)
            {
                result |= this.BlockStore[asTile.Id];
            }
            return result;
        }

        public MaterialType GetMaterials(Point pos)
        {
            var cell = this.Map[pos];
            var result = cell.Foreground.Aggregate(MaterialType.None, (m, t) => m | this.GetMaterials(t));
            result = cell.Background.Aggregate(result, (m, t) => m | this.GetMaterials(t));
            result |= this.GetMaterials(cell.Block);
            return result;
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
                        // (0.09, 0.08] is background
                        var diff = (1f / cell.Background.Count) * 0.01f;
                        var depth = 0.9f - diff;
                        foreach (var tile in cell.Background)
                        {
                            this.BlockStore.DrawTile(renderer.World, pos, tile, depth, Color.White);
                            depth -= diff;
                        }
                    }
                    // 0.02 is 'block'
                    this.BlockStore.DrawTile(renderer.World, pos, cell.Block, 0.02f, Color.White);
                    if (AbstractObject.DebugInfo && cell.Block != null)
                    {
                        Store.Instance.Sprites<ISpriteTemplate>("Base", "white-16x16").DrawSprite(renderer.World, 0, pos, Color.Red, 0, Vector2.One, SpriteEffects.None, 0.019f);
                    }
                    if (cell.Foreground.Count > 0)
                    {
                        // (0.02, 0.01] is foreground
                        var diff = (1f / cell.Foreground.Count) * 0.01f;
                        var depth = 0.02f - diff;
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

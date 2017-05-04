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
        public Color BackgroundColour = Color.CornflowerBlue;

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

    public class Light
    {
        public static Func<TimeSpan, bool> OperatingNightOnly = time => time.Hours < 8 || time.Hours > 17;

        private IGameObject owner;
        private Vector2 position = Vector2.Zero;
        private Color colour = Color.White;
        private Vector2 scale = Vector2.One;
        private bool enabled = true;
        private bool operating = false;
        private Func<TimeSpan, bool> operatingFunc = OperatingNightOnly;

        public Light()
        {
            this.owner = null;
        }
        
        public IGameObject Owner
        {
            get { return this.owner; }
            internal set { this.owner = value; }
        }

        public Vector2 AbsolutePosition
        {
            get { return this.owner == null ? this.position : this.owner.Position + this.position; }
        }

        public Vector2 RelativePosition
        {
            get { return this.position; }
            set { this.position = value; }
        }

        public Vector2 Size
        {
            get { return this.scale; }
            set { this.scale = value; }
        }

        public Color Colour
        {
            get { return this.colour; }
            set { this.colour = value; }
        }

        public bool IsEnabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        public bool IsOperating
        {
            get { return this.operating; }
        }

        public Func<TimeSpan, bool> OperatingFunction
        {
            set { this.operatingFunc = value; }
        }
        
        internal void Update(ref TimeSpan time, GameTime gameTime)
        {
            this.operating = this.operatingFunc == null ? true : this.operatingFunc(time);
        }
    }

    public class PlatformContext : GameContext
    {
        public readonly TileMap Map;
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
            base.Update(gameTime);
            this.time += TimeSpan.FromSeconds(gameTime.GetElapsedSeconds());

            foreach (var light in this.lights.Where(l => l.IsEnabled))
            {
                light.Update(ref this.time, gameTime);
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

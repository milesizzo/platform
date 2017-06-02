using GameEngine.Content;
using GameEngine.Scenes;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using GameEngine.Graphics;
using Microsoft.Xna.Framework.Input;
using GameEngine.Templates;
using GameEngine.Helpers;
using GameEngine.UI;
using CommonLibrary;
using MonoGame.Extended;
using System.IO;

namespace Platform
{
    public class PlatformEditorScene : BasePlatformGameScene
    {
        private enum Layer
        {
            Background,
            Foreground,
            Blocking,
        }
        private ITile currTile = null;
        private Layer layer = Layer.Blocking;
        private UIPanel palette = null;
        private Vector2? pan = null;

        private struct TilePlacement
        {
            public Point Location;
            public MouseButton Button;
        }
        private TilePlacement? lastPlacement = null;

        public PlatformEditorScene(string name, GraphicsDevice graphics) : base(name, graphics)
        {
        }

        protected override PlatformContext CreateContext()
        {
            var context = new PlatformContext(this.Camera, 256, 128);
            context.Enabled = false;
            return context;
        }

        public override void SetUp()
        {
            base.SetUp();

            var font = Store.Instance.Fonts("Base", "debug");
            if (!File.Exists("editor.map"))
            {
                File.Copy("Content\\Maps\\landscape.map", "editor.map", true);
            }
            this.Context.Map = BinTileMapSerializer.Load("editor.map");

            var panel = new UIPanel();

            var rows = new UIRowLayout(panel);

            // materials
            var materials = new UIColumnLayout(rows);
            var label = new UILabel(materials);
            label.Text = "Material:";
            label.TextColour = Color.Yellow;
            label.Font = font;
            foreach (var type in this.Context.BlockStore.Blocks.Keys)
            {
                var button = new UIButton(materials);
                button.Label.Text = type.ToString();
                button.Label.Font = font;
                button.ButtonClick += b =>
                {
                    this.currTile = new Material { Type = type };
                };
            }

            // tiles 
            var tiles = new UIColumnLayout(rows);
            label = new UILabel(tiles);
            label.Text = "Tiles:";
            label.TextColour = Color.Yellow;
            label.Font = font;
            var grid = new UIImageGridPicker(10, 20, tiles);
            grid.AddSprites(this.Context.BlockStore.Tiles);
            grid.GridClick += (b, p) =>
            {
                var index = grid.PointToIndex(p);
                this.currTile = new Block { Id = index };
            };

            // prefabs
            var prefabs = new UIColumnLayout(rows);
            label = new UILabel(prefabs);
            label.Text = "Objects:";
            label.TextColour = Color.Yellow;
            label.Font = font;
            foreach (var prefab in this.Context.BlockStore.Prefabs.Values)
            {
                var button = new UIIconButton(prefabs);
                button.Icon = prefab.Sprite;
                button.ButtonClick += b =>
                {
                    //
                };
            }

            // menu
            var menu = new UIColumnLayout(rows);
            var process = new UIButton(menu);
            process.Label.Text = "Process";
            process.Label.Font = font;
            process.ButtonClick += b =>
            {
                var processor = new UPPGKMapProcessor();
                processor.Process(this.Context.Map);
            };
            var save = new UIButton(menu);
            save.Label.Text = "Save";
            save.Label.Font = font;
            save.ButtonClick += b =>
            {
                BinTileMapSerializer.Save("editor.map", this.Context.Map);
            };
            var quit = new UIButton(menu);
            quit.Label.Text = "Quit";
            quit.Label.Font = font;
            quit.ButtonClick += b =>
            {
                this.SceneEnded = true;
            };

            panel.Size.X = 1400;
            panel.Size.Y = 800;
            panel.Placement.RelativeX = 0.5f;
            panel.Placement.RelativeY = 0.5f;
            panel.Origin = UIOrigin.Centre;
            this.palette = panel;
            this.UI.Add(panel);
            this.UI.Enabled = false;

            this.Context.Time = new TimeSpan(12, 0, 0);
            this.Camera.Position = Vector2.Zero;
        }

        private Point MouseToTile
        {
            get
            {
                var mouse = Mouse.GetState();
                var mouseWorld = this.Camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y));
                return this.Context.WorldToTile(mouseWorld);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var mouse = Mouse.GetState();
            if (this.UI.Enabled)
            {
                if (KeyboardHelper.KeyPressed(Keys.Escape))
                {
                    this.UI.Enabled = false;
                }
            }
            else
            {
                // show UI
                if (KeyboardHelper.KeyPressed(Keys.Escape))
                {
                    this.UI.Enabled = true;
                }

                // pick layer
                if (KeyboardHelper.KeyPressed(Keys.D1))
                {
                    this.layer = Layer.Background;
                }
                if (KeyboardHelper.KeyPressed(Keys.D2))
                {
                    this.layer = Layer.Foreground;
                }
                if (KeyboardHelper.KeyPressed(Keys.D3))
                {
                    this.layer = Layer.Blocking;
                }

                // pan camera
                if (MouseHelper.ButtonDown(MouseButton.Middle))
                {
                    var mousePos = this.Camera.ScreenToWorld(new Vector2(mouse.Position.X, mouse.Position.Y));
                    if (this.pan.HasValue)
                    {
                        this.Camera.Position = this.pan.Value - (mousePos - this.Camera.Position);
                    }
                    else
                    {
                        this.pan = mousePos;
                    }
                }
                else
                {
                    this.pan = null;
                }

                // zoom camera
                var scroll = MouseHelper.ScrollDirection;
                this.Camera.Zoom = MathHelper.Max(0.1f, this.Camera.Zoom + (float)(scroll * gameTime.ElapsedGameTime.TotalSeconds * 10));

                // tile placement
                var mouseTile = this.MouseToTile;
                if (this.Context.IsInBounds(mouseTile))
                {
                    var last = this.lastPlacement;
                    if (mouse.LeftButton == ButtonState.Pressed && (!last.HasValue || last.Value.Location != mouseTile || last.Value.Button != MouseButton.Left))
                    {
                        if (this.currTile != null)
                        {
                            var cell = this.Context.Map[mouseTile];
                            switch (this.layer)
                            {
                                case Layer.Background:
                                    cell.Background.Add(this.currTile.Clone());
                                    break;
                                case Layer.Blocking:
                                    cell.Block = this.currTile.Clone();
                                    break;
                                case Layer.Foreground:
                                    cell.Foreground.Add(this.currTile.Clone());
                                    break;
                            }
                        }
                        this.lastPlacement = new TilePlacement { Location = mouseTile, Button = MouseButton.Left };
                    }
                    if (mouse.RightButton == ButtonState.Pressed && (!last.HasValue || last.Value.Location != mouseTile || last.Value.Button != MouseButton.Right))
                    {
                        var cell = this.Context.Map[mouseTile];
                        cell.Background.Clear();
                        cell.Foreground.Clear();
                        cell.Block = null;
                        this.lastPlacement = new TilePlacement { Location = mouseTile, Button = MouseButton.Right };
                    }
                    if (!MouseHelper.ButtonDown(MouseButton.Left) && !MouseHelper.ButtonDown(MouseButton.Right))
                    {
                        this.lastPlacement = null;
                    }
                }
                else
                {
                    this.lastPlacement = null;
                }
            }
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            var font = Store.Instance.Fonts("Base", "debug");
            var mouseTile = this.MouseToTile;
            if (this.Context.IsInBounds(mouseTile) && !this.UI.Enabled)
            {
                if (this.currTile != null)
                {
                    var world = this.Context.TileToWorld(mouseTile);
                    this.Context.BlockStore.DrawTile(renderer.World, world, this.currTile, 0.1f, Color.White);
                    renderer.World.DrawRectangle(world, new Size2(this.Context.BlockStore.TileSize, this.Context.BlockStore.TileSize), Color.White);
                }
                var tileText = new StringBuilder();
                var cell = this.Context.Map[mouseTile];
                tileText.AppendLine($"  Position: {mouseTile}");
                tileText.AppendLine($"Foreground: " + string.Join(",", cell.Foreground.Select(t => t.DebugString)));
                tileText.AppendLine($"Background: " + string.Join(",", cell.Background.Select(t => t.DebugString)));
                tileText.AppendLine($"     Block: " + (cell.Block == null ? "null" : cell.Block.DebugString));
                font.DrawString(renderer.Screen, new Vector2(0, 800), tileText.ToString(), Color.Wheat);
            }

            var text = new StringBuilder();
            text.AppendLine($"Camera:");
            text.AppendLine($"    Bounds: {this.Camera.Viewport.Bounds}");
            text.AppendLine($"  Position: {this.Camera.Position}");
            text.AppendLine($"Time in game : {this.Context.Time}");
            text.AppendLine($"Layer        : {this.layer}");
            font.DrawString(renderer.Screen, new Vector2(0, 0), text.ToString(), Color.White);

            if (this.currTile != null)
            {
                font.DrawString(renderer.Screen, new Vector2(0, 600), "Current tile ", Color.White);
                var size = font.Font.MeasureString("Current tile ");
                this.Context.BlockStore.DrawTile(renderer.Screen, new Vector2(size.X, 600), this.currTile, 0f, Color.White);
            }

            renderer.World.DrawRectangle(
                new Vector2(-1, 1),
                new Size2(this.Context.Map.Width * this.Context.BlockStore.TileSize + 2, this.Context.Map.Height * this.Context.BlockStore.TileSize + 2),
                Color.White);

            base.Draw(renderer, gameTime);
        }
    }
}

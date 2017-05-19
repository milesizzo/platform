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

        public PlatformEditorScene(string name, GraphicsDevice graphics, Store store) : base(name, graphics, store)
        {
        }

        protected override PlatformContext CreateContext()
        {
            var context = new PlatformContext(this.Store, this.Camera, 2048, 1024, 16);
            context.Enabled = false;
            return context;
        }

        public override void SetUp()
        {
            base.SetUp();

            var font = this.Store.Fonts("Base", "debug");
            this.Context.Map = BinTileMapSerializer.Load("landscape.map");

            var panel = new UIPanel();

            var rows = new UIRowLayout(panel);

            // materials
            var materials = new UIColumnLayout(rows);
            var label = new UILabel(materials);
            label.Text = "Material:";
            label.TextColour = Color.Yellow;
            label.Font = font;
            UIElement last = label;
            foreach (var type in this.Context.BlockStore.Blocks.Keys)
            {
                var button = new UIButton(materials);
                button.Label.Text = type.ToString();
                button.Label.Font = font;
                button.ButtonClick += b =>
                {
                    this.currTile = new Material { Type = type };
                };
                last = button;
            }

            // tiles 
            var tiles = new UIColumnLayout(rows);
            label = new UILabel(tiles);
            label.Text = "Tiles:";
            label.TextColour = Color.Yellow;
            label.Font = font;
            last = label;
            var id = 0;
            foreach (var sprite in this.Context.BlockStore.Tiles)
            {
                var button = new UIIconButton(tiles);
                button.Icon = sprite;
                button.ButtonClick += this.ButtonDelegate(id);
                id++;
            }

            // menu
            var menu = new UIColumnLayout(rows);
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

            panel.Size.X = 400;
            panel.Size.Y = 200;
            panel.Placement.RelativeX = 0.5f;
            panel.Placement.RelativeY = 0.5f;
            panel.Origin = UIOrigin.Centre;
            this.palette = panel;
            this.UI.Add(panel);
            this.UI.Enabled = false;

            this.Context.Time = new TimeSpan(12, 0, 0);
            this.Camera.Position = Vector2.Zero;
        }

        private UIButtonClicked ButtonDelegate(int i)
        {
            return b => this.currTile = new Block { Id = i };
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
                if (KeyboardHelper.KeyPressed(Keys.F1))
                {
                    this.UI.Enabled = false;
                }
            }
            else
            {
                // show UI
                if (KeyboardHelper.KeyPressed(Keys.F1))
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
                this.Camera.Zoom += (float)(scroll * gameTime.ElapsedGameTime.TotalSeconds * 10);

                // tile placement
                var mouseTile = this.MouseToTile;
                if (this.Context.IsInBounds(mouseTile))
                {
                    var last = this.lastPlacement;
                    if (mouse.LeftButton == ButtonState.Pressed && (!last.HasValue || last.Value.Location != mouseTile || last.Value.Button != MouseButton.Left))
                    {
                        var cell = this.Context.Map[mouseTile];
                        switch (this.layer)
                        {
                            case Layer.Background:
                                cell.Background.Add(this.currTile);
                                break;
                            case Layer.Blocking:
                                cell.Block = this.currTile;
                                break;
                            case Layer.Foreground:
                                cell.Foreground.Add(this.currTile);
                                break;
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
            var font = this.Store.Fonts("Base", "debug");
            var mouseTile = this.MouseToTile;
            if (this.Context.IsInBounds(mouseTile))
            {
                if (this.currTile != null)
                {
                    var world = this.Context.TileToWorld(mouseTile);
                    this.currTile.GetSprite(this.Context.BlockStore).DrawSprite(renderer.World, world, 0.1f);
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
                this.currTile.GetSprite(this.Context.BlockStore).DrawSprite(renderer.Screen, new Vector2(size.X, 600), 0);
            }

            base.Draw(renderer, gameTime);
        }
    }
}

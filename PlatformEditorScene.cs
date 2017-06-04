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
using Platform.Editor;
using CommonLibrary.Serializing;
using Platform.Serializing;
using GameEngine.GameObjects;

namespace Platform
{
    public class PlatformEditorScene : BasePlatformGameScene
    {
        private TileStencil curr = null;
        private TileStencil.Layer layer = TileStencil.Layer.Blocking;
        private UIPanel palette = null;
        private Vector2? pan = null;
        private readonly List<TileStencil> stencils = new List<TileStencil>();

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

            #region Stencils
            this.stencils.Clear();
            var stencil = new TileStencil();
            stencil.AddRow(0, 500, 501, 502, 503, 504);
            stencil.AddRow(0, 510, 511, 512, 513, 514);
            stencil.AddRow(0, 520, 521, 522, 523, 524);
            stencil.AddRow(1,      531, 532);
            stencil.AddRow(1,      541, 542, 543);
            this.stencils.Add(stencil);

            stencil = new TileStencil();
            stencil.AddRow(2,           497, 498);
            stencil.AddRow(1,      506, 507, 508, 509);
            stencil.AddRow(0, 515, 516, 517, 518, 519);
            stencil.AddRow(0, 525, 526, 527, 528, 529);
            stencil.AddRow(2,           537, 538);
            stencil.AddRow(2,           547, 548);
            this.stencils.Add(stencil);

            stencil = new TileStencil();
            stencil.AddRow(0, 451, 452, 453);
            stencil.AddRow(0, 461, 462, 463);
            stencil.AddRow(0, 471, 472);
            stencil.AddRow(0, 481, 482, 483);
            this.stencils.Add(stencil);

            stencil = new TileStencil();
            stencil.AddRow(0, 456, 457);
            stencil.AddRow(0, 466, 467);
            stencil.AddRow(0, 476, 477);
            stencil.AddRow(0, 486, 487, 488);
            this.stencils.Add(stencil);

            stencil = new TileStencil();
            stencil.AddRow(0, 220, 221);
            stencil.AddRow(0, 232, 233);
            this.stencils.Add(stencil);

            stencil = new TileStencil();
            stencil.AddRow(0, 222, 223);
            stencil.AddRow(0, 232, 233);
            this.stencils.Add(stencil);

            stencil = new TileStencil();
            stencil.AddRow(0, 230, 231);
            stencil.AddRow(0, 234, 235);
            this.stencils.Add(stencil);

            stencil = new TileStencil();
            stencil.AddRow(0, 224, 225);
            stencil.AddRow(0, 234, 235);
            this.stencils.Add(stencil);

            stencil = new TileStencil();
            stencil.AddRow(0, 242, 243);
            stencil.AddRow(0, 252, 253);
            this.stencils.Add(stencil);

            stencil = new TileStencil();
            stencil.AddRow(0, 244, 245);
            stencil.AddRow(0, 254, 255);
            this.stencils.Add(stencil);

            stencil = new TileStencil();
            stencil.AddRow(0, 240, 241);
            stencil.AddRow(0, 250, 251);
            this.stencils.Add(stencil);

            // fence 1 (left)
            stencil = new TileStencil();
            stencil.AddRow(0, 95);
            stencil.AddRow(0, 105);
            this.stencils.Add(stencil);

            // fence 1 (middle)
            stencil = new TileStencil();
            stencil.AddRow(0, 96);
            stencil.AddRow(0, 106);
            this.stencils.Add(stencil);

            // fence 1 (right)
            stencil = new TileStencil();
            stencil.AddRow(0, 97);
            stencil.AddRow(0, 107);
            this.stencils.Add(stencil);

            // fence 2 (left)
            stencil = new TileStencil();
            stencil.AddRow(0, 115);
            stencil.AddRow(0, 125);
            this.stencils.Add(stencil);

            // fence 2 (middle)
            stencil = new TileStencil();
            stencil.AddRow(0, 116);
            stencil.AddRow(0, 126);
            this.stencils.Add(stencil);

            // fence 2 (right)
            stencil = new TileStencil();
            stencil.AddRow(0, 117);
            stencil.AddRow(0, 127);
            this.stencils.Add(stencil);

            using (var serializer = new MgiJsonSerializer("Stencils16x16.json", SerializerMode.Write))
            {
                serializer.Context.WriteList("stencils", this.stencils, PlatformSerialize.Write);
            }
            #endregion

            var panel = new UIPanel();

            var cols = new UIColumnLayout(panel, 0.8f, 0.2f);
            var rows = new UIRowLayout(cols);

            // materials
            /*var materials = new UIColumnLayout(rows);
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
                    this.curr = new TileStencil();
                    this.curr[0, 0] = new Material { Type = type };
                };
            }*/

            // tiles 
            /*var tiles = new UIColumnLayout(cols);
            label = new UILabel(tiles);
            label.Text = "Tiles:";
            label.TextColour = Color.Yellow;
            label.Font = font;*/
            var grid = new UIImageGridPicker(62, 10, cols);
            grid.AddSprites(this.Context.BlockStore.Tiles);
            grid.GridClick += (b, p) =>
            {
                var index = grid.PointToIndex(p);
                this.curr = new TileStencil();
                this.curr[0, 0] = new Tile(index);
            };

            /*
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
            */

            // stencils
            var stencils = new UIColumnLayout(rows);
            var label = new UILabel(stencils);
            label.Text = "Stencils:";
            label.TextColour = Color.Yellow;
            label.Font = font;
            foreach (var s in this.stencils)
            {
                var button = new UIIconButton(stencils);
                button.Icon = new TileStencilSprite(s, this.Context.BlockStore);
                button.ButtonClick += b =>
                {
                    this.curr = s;
                };
            }

            // menu
            var menu = new UIColumnLayout(rows);
            var process = new UIButton(menu);
            process.Label.Text = "Process";
            process.Label.Font = font;
            process.ButtonClick += b =>
            {
                var processor = new PFPTMapProcessor();
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
            this.UI.Clear();
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

                // debug mode
                if (KeyboardHelper.KeyPressed(Keys.F12))
                {
                    AbstractObject.DebugInfo = !AbstractObject.DebugInfo;
                }

                // pick layer
                if (KeyboardHelper.KeyPressed(Keys.D1))
                {
                    this.layer = TileStencil.Layer.Background;
                }
                if (KeyboardHelper.KeyPressed(Keys.D2))
                {
                    this.layer = TileStencil.Layer.Foreground;
                }
                if (KeyboardHelper.KeyPressed(Keys.D3))
                {
                    this.layer = TileStencil.Layer.Blocking;
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
                        if (this.curr != null)
                        {
                            this.curr.Stamp(this.Context.Map, mouseTile, this.layer);
                            /*var cell = this.Context.Map[mouseTile];
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
                            }*/
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
                if (this.curr != null)
                {
                    var world = this.Context.TileToWorld(mouseTile);
                    this.curr.Draw(renderer.World, world, this.Context.BlockStore);
                    //this.Context.BlockStore.DrawTile(renderer.World, world, this.currTile, 0.1f, Color.White);
                    renderer.World.DrawRectangle(world, new Size2(this.Context.BlockStore.TileSize, this.Context.BlockStore.TileSize), Color.White);
                }
                var tileText = new StringBuilder();
                var cell = this.Context.Map[mouseTile];
                var materials = this.Context.GetMaterials(mouseTile);
                tileText.AppendLine($"   Position: {mouseTile}");
                tileText.AppendLine($" Foreground: " + string.Join(",", cell.Foreground.Select(t => t.DebugString)));
                tileText.AppendLine($" Background: " + string.Join(",", cell.Background.Select(t => t.DebugString)));
                tileText.AppendLine($"      Block: " + (cell.Block == null ? "null" : cell.Block.DebugString));
                tileText.AppendLine($"Material(s): " + materials);
                font.DrawString(renderer.Screen, new Vector2(0, 800), tileText.ToString(), Color.Wheat);
            }

            var text = new StringBuilder();
            text.AppendLine($"Camera:");
            text.AppendLine($"    Bounds: {this.Camera.Viewport.Bounds}");
            text.AppendLine($"  Position: {this.Camera.Position}");
            text.AppendLine($"Time in game : {this.Context.Time}");
            text.AppendLine($"Layer        : {this.layer}");
            font.DrawString(renderer.Screen, new Vector2(0, 0), text.ToString(), Color.White);

            if (this.curr != null)
            {
                font.DrawString(renderer.Screen, new Vector2(0, 600), "Current: ", Color.White);
                var size = font.Font.MeasureString("Current: ");
                //this.Context.BlockStore.DrawTile(renderer.Screen, new Vector2(size.X, 600), this.currTile, 0f, Color.White);
                this.curr.Draw(renderer.Screen, new Vector2(size.X, 600), this.Context.BlockStore);
            }

            renderer.World.DrawRectangle(
                new Vector2(-1, 1),
                new Size2(this.Context.Map.Width * this.Context.BlockStore.TileSize + 2, this.Context.Map.Height * this.Context.BlockStore.TileSize + 2),
                Color.White);

            base.Draw(renderer, gameTime);
        }
    }
}

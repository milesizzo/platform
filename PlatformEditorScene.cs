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
using CommonLibrary;
using MonoGame.Extended;
using System.IO;
using Platform.Editor;
using CommonLibrary.Serializing;
using Platform.Serializing;
using GameEngine.GameObjects;
using GeonBit.UI.Entities;
using GeonBit.UI;
using GeonBit.UI.DataTypes;

namespace Platform
{
    public class PlatformEditorScene : BasePlatformGameScene
    {
        private const string DefaultMap = "editor.map";
        private TileStencil curr = null;
        private TileStencil.Layer layer = TileStencil.Layer.Blocking;
        private Vector2? pan = null;
        private readonly List<TileStencil> stencils = new List<TileStencil>();
        private Entity menu = null;

        private struct TilePlacement
        {
            public Point Location;
            public GameEngine.Helpers.MouseButton Button;
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

        private void CreateStencils()
        {
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
            stencil.Origin = new Point(2, 5);
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

            // square door with window
            stencil = new TileStencil();
            stencil.AddRow(0, 220, 221);
            stencil.AddRow(0, 232, 233);
            this.stencils.Add(stencil);

            // square door, no window
            stencil = new TileStencil();
            stencil.AddRow(0, 222, 223);
            stencil.AddRow(0, 232, 233);
            this.stencils.Add(stencil);

            // square door with hinges
            stencil = new TileStencil();
            stencil.AddRow(0, 242, 243);
            stencil.AddRow(0, 252, 253);
            this.stencils.Add(stencil);

            // square door - open
            stencil = new TileStencil();
            stencil.AddRow(0, 420, 421);
            stencil.AddRow(0, 430, 431);
            this.stencils.Add(stencil);

            // round door, window
            stencil = new TileStencil();
            stencil.AddRow(0, 230, 231);
            stencil.AddRow(0, 234, 235);
            this.stencils.Add(stencil);

            // round door, no window
            stencil = new TileStencil();
            stencil.AddRow(0, 224, 225);
            stencil.AddRow(0, 234, 235);
            this.stencils.Add(stencil);

            // round door with hinges
            stencil = new TileStencil();
            stencil.AddRow(0, 244, 245);
            stencil.AddRow(0, 254, 255);
            this.stencils.Add(stencil);

            // round door - open
            stencil = new TileStencil();
            stencil.AddRow(0, 433, 434);
            stencil.AddRow(0, 443, 444);
            this.stencils.Add(stencil);

            // stone frame door
            stencil = new TileStencil();
            stencil.AddRow(0, 008, 009);
            stencil.AddRow(0, 018, 019);
            this.stencils.Add(stencil);

            // stone frame bars?
            stencil = new TileStencil();
            stencil.AddRow(0, 015, 016);
            stencil.AddRow(0, 025, 026);
            this.stencils.Add(stencil);

            // stone frame door - open
            stencil = new TileStencil();
            stencil.AddRow(0, 400, 401);
            stencil.AddRow(0, 410, 411);
            this.stencils.Add(stencil);

            // green bush (near doors on tilesheet)
            stencil = new TileStencil();
            stencil.AddRow(0, 240, 241);
            stencil.AddRow(0, 250, 251);
            this.stencils.Add(stencil);

            // rock
            stencil = new TileStencil();
            stencil.AddRow(0, 70, 71);
            this.stencils.Add(stencil);

            // rock
            stencil = new TileStencil();
            stencil.AddRow(0, 75, 76);
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

            // fence 3 (stone - left)
            stencil = new TileStencil();
            stencil.AddRow(0, 310);
            stencil.AddRow(0, 320);
            this.stencils.Add(stencil);

            // fence 3 (stone - middle)
            stencil = new TileStencil();
            stencil.AddRow(0, 315);
            stencil.AddRow(0, 325);
            this.stencils.Add(stencil);

            // fence 3 (stone - right)
            stencil = new TileStencil();
            stencil.AddRow(0, 312);
            stencil.AddRow(0, 322);
            this.stencils.Add(stencil);

            // chimney 1 - left
            stencil = new TileStencil();
            stencil.AddRow(0, 354);
            stencil.AddRow(0, 364);
            this.stencils.Add(stencil);

            // chimney 1 - right
            stencil = new TileStencil();
            stencil.AddRow(0, 355);
            stencil.AddRow(0, 365);
            this.stencils.Add(stencil);

            // chimney 2 - left
            stencil = new TileStencil();
            stencil.AddRow(0, 356);
            stencil.AddRow(0, 366);
            this.stencils.Add(stencil);

            // chimney 2 - right
            stencil = new TileStencil();
            stencil.AddRow(0, 357);
            stencil.AddRow(0, 367);
            this.stencils.Add(stencil);

            // chimney 3 - left
            stencil = new TileStencil();
            stencil.AddRow(0, 358);
            stencil.AddRow(0, 368);
            this.stencils.Add(stencil);

            // chimney 3 - right
            stencil = new TileStencil();
            stencil.AddRow(0, 359);
            stencil.AddRow(0, 369);
            this.stencils.Add(stencil);

            // large crate
            stencil = new TileStencil();
            stencil.AddRow(0, 351);
            stencil.AddRow(0, 361);
            this.stencils.Add(stencil);

            // NOTE: gothic tiles start at 620
            // tombstone 1
            stencil = new TileStencil();
            stencil.AddRow(0, 624);
            stencil.AddRow(0, 634);
            this.stencils.Add(stencil);

            // tombstone 2
            stencil = new TileStencil();
            stencil.AddRow(0, 625);
            stencil.AddRow(0, 635);
            this.stencils.Add(stencil);

            // tombstone 3
            stencil = new TileStencil();
            stencil.AddRow(0, 626);
            stencil.AddRow(0, 636);
            this.stencils.Add(stencil);

            // bookshelf 1
            stencil = new TileStencil();
            stencil.AddRow(0, 990, 991);
            stencil.AddRow(0, 1000, 1001);
            this.stencils.Add(stencil);

            // bookshelf 2
            stencil = new TileStencil();
            stencil.AddRow(0, 994, 995);
            stencil.AddRow(0, 1004, 1005);
            this.stencils.Add(stencil);

            // bookshelf 3
            stencil = new TileStencil();
            stencil.AddRow(0, 996, 997);
            stencil.AddRow(0, 1006, 1007);
            this.stencils.Add(stencil);

            // tree 5
            stencil = new TileStencil();
            stencil.AddRow(1,       1181, 1182, 1183);
            stencil.AddRow(1,       1191, 1192, 1193);
            stencil.AddRow(0, 1200, 1201, 1202, 1203, 1204);
            stencil.AddRow(1,       1211, 1212, 1213);
            stencil.AddRow(1,       1221, 1222, 1223, 1224);
            this.stencils.Add(stencil);

            // tree 6
            stencil = new TileStencil();
            stencil.AddRow(2,             1187, 1188);
            stencil.AddRow(1,       1196, 1197, 1198, 1199);
            stencil.AddRow(1,       1206, 1207, 1208, 1209);
            stencil.AddRow(2,             1217, 1218);
            stencil.AddRow(0, 1225, 1226, 1227, 1228);
            this.stencils.Add(stencil);

            using (var serializer = new MgiJsonSerializer("Stencils16x16.json", SerializerMode.Write))
            {
                serializer.Context.WriteList("stencils", this.stencils, PlatformSerialize.Write);
            }
        }

        public override void SetUp()
        {
            base.SetUp();

            var font = Store.Instance.Fonts("Base", "debug");
            if (!File.Exists(DefaultMap))
            {
                File.Copy("Content\\Maps\\landscape.map", DefaultMap, true);
            }
            this.Context.Map = BinTileMapSerializer.Load(DefaultMap);

            this.CreateStencils();

            if (this.menu != null)
            {
                UserInterface.RemoveEntity(this.menu);
            }
            this.menu = new PanelTabs();
            this.menu.SetAnchor(Anchor.Center);
            this.menu.Size = new Vector2(1400, 1100);
            this.menu.Visible = false;

            // file menu
            var fileMenu = (this.menu as PanelTabs).AddTab("File", PanelSkin.Default);
            var processButton = new Button("Process", anchor: Anchor.AutoCenter, size: new Vector2(200, 100));
            processButton.OnClick += (b) =>
            {
                var processor = new PFPTMapProcessor();
                processor.Process(this.Context.Map);
            };
            fileMenu.panel.AddChild(processButton);

            // 'save map' area
            var savePanel = new Panel(new Vector2(600, 100), skin: PanelSkin.Simple, anchor: Anchor.BottomLeft);

            var filenameInput = new TextInput(false, anchor: Anchor.CenterLeft, size: new Vector2(300, 0));
            filenameInput.Value = DefaultMap;
            savePanel.AddChild(filenameInput);

            var saveButton = new Button("Save", anchor: Anchor.CenterRight, size: new Vector2(200, 0));
            saveButton.OnClick += (b) =>
            {
                var filename = filenameInput.Value.Trim();
                if (!filename.EndsWith(".map", StringComparison.InvariantCultureIgnoreCase))
                {
                    filename += ".map";
                    filenameInput.Value = filename;
                }
                BinTileMapSerializer.Save(filenameInput.TextParagraph.Text, this.Context.Map);
            };
            savePanel.AddChild(saveButton);
            fileMenu.panel.AddChild(savePanel);

            // quit button
            var quitButton = new Button("Quit", anchor: Anchor.BottomRight, size: new Vector2(200, 100));
            quitButton.OnClick += (b) =>
            {
                this.SceneEnded = true;
            };
            fileMenu.panel.AddChild(quitButton);

            // tiles menu
            var tilesMenu = (this.menu as PanelTabs).AddTab("Tiles", PanelSkin.Default);
            tilesMenu.panel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            tilesMenu.panel.Scrollbar.Max = (uint)(this.menu.Size.Y * 4);

            var tileIndex = 0;
            foreach (var sprite in this.Context.BlockStore.Tiles)
            {
                var tile = new Tile(tileIndex);

                var img = new SpriteImage(sprite, new Vector2(sprite.Width * 2, sprite.Height * 2), anchor: Anchor.AutoInline);
                img.SetStyleProperty("FillColor", new StyleProperty(Color.Red), EntityState.MouseHover);
                img.Padding = Vector2.Zero;
                img.Scale = 1f;
                img.OnClick += (e) =>
                {
                    this.curr = new TileStencil();
                    this.curr[0, 0] = tile;
                };
                tilesMenu.panel.AddChild(img);
                tileIndex++;
            }

            // stencil menu
            var stencilMenu = (this.menu as PanelTabs).AddTab("Stencils", PanelSkin.Default);
            stencilMenu.panel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            stencilMenu.panel.Scrollbar.Max = (uint)(this.menu.Size.Y * 4); // we have to guess at the maximum height...

            var stencilSprites = this.stencils.Select(s => s.ToSprite(this.Context.BlockStore)).ToList();
            var max = Vector2.Zero;
            max.X = stencilSprites.Max(s => s.Width);
            max.Y = stencilSprites.Max(s => s.Height);
            foreach (var stencil in this.stencils)
            {
                var sprite = stencil.ToSprite(this.Context.BlockStore);
                var outline = new ColoredRectangle(max * 2, anchor: Anchor.AutoInline);
                outline.SpaceAfter = new Vector2(10f, 10f);
                outline.OutlineColor = Color.White;
                outline.OutlineWidth = 1;
                outline.FillColor = Color.CornflowerBlue;
                outline.SetStyleProperty("FillColor", new StyleProperty(Color.Red), EntityState.MouseHover);
                outline.OnClick += (e) =>
                {
                    this.curr = stencil;
                };

                var img = new SpriteImage(sprite, new Vector2(sprite.Width * 2, sprite.Height * 2), anchor: Anchor.Center);
                img.Padding = Vector2.Zero;
                img.Scale = 1f;
                img.Locked = true;
                
                outline.AddChild(img);
                stencilMenu.panel.AddChild(outline);
            }

            UserInterface.AddEntity(this.menu);

            this.Context.Time = new TimeSpan(12, 0, 0);
            this.Camera.Position = Vector2.Zero;
        }

        private Vector2 MouseToWorld
        {
            get
            {
                var mouse = Mouse.GetState();
                var world = this.Camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y));
                return world;
            }
        }

        private Point MouseToTile
        {
            get
            {
                return this.Context.WorldToTile(this.MouseToWorld);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var mouse = Mouse.GetState();
            if (this.menu.Visible)
            {
                if (KeyboardHelper.KeyPressed(Keys.Escape))
                {
                    this.menu.Visible = false;
                }
            }
            else
            {
                // show UI
                if (KeyboardHelper.KeyPressed(Keys.Escape))
                {
                    this.menu.Visible = true;
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
                if (MouseHelper.ButtonDown(GameEngine.Helpers.MouseButton.Middle))
                {
                    var mousePos = this.MouseToWorld;
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
                    if (mouse.LeftButton == ButtonState.Pressed && (!last.HasValue || last.Value.Location != mouseTile || last.Value.Button != GameEngine.Helpers.MouseButton.Left))
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
                        this.lastPlacement = new TilePlacement { Location = mouseTile, Button = GameEngine.Helpers.MouseButton.Left };
                    }
                    if (mouse.RightButton == ButtonState.Pressed && (!last.HasValue || last.Value.Location != mouseTile || last.Value.Button != GameEngine.Helpers.MouseButton.Right))
                    {
                        var cell = this.Context.Map[mouseTile];
                        cell.Background.Clear();
                        cell.Foreground.Clear();
                        cell.Block = null;
                        this.lastPlacement = new TilePlacement { Location = mouseTile, Button = GameEngine.Helpers.MouseButton.Right };
                    }
                    if (!MouseHelper.ButtonDown(GameEngine.Helpers.MouseButton.Left) && !MouseHelper.ButtonDown(GameEngine.Helpers.MouseButton.Right))
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
            if (this.Context.IsInBounds(mouseTile) && !this.menu.Visible)
            {
                if (this.curr != null)
                {
                    var world = this.Context.TileToWorld(mouseTile);
                    this.curr.Draw(renderer.World, world, Vector2.One, Color.White, this.Context.BlockStore);
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

            var mouse = Mouse.GetState();
            var text = new StringBuilder();
            text.AppendLine($"Camera:");
            text.AppendLine($"    Bounds: {this.Camera.Viewport.Bounds}");
            text.AppendLine($"  Position: {this.Camera.Position}");
            text.AppendLine();
            text.AppendLine($"Time in game   : {this.Context.Time}");
            text.AppendLine($"Layer          : {this.layer}");
            text.AppendLine($"Mouse (screen) : {mouse.X}, {mouse.Y}");
            text.AppendLine($"Mouse (world)  : {this.MouseToWorld}");
            font.DrawString(renderer.Screen, new Vector2(0, 0), text.ToString(), Color.White);

            if (this.curr != null)
            {
                font.DrawString(renderer.Screen, new Vector2(0, 600), "Current: ", Color.White);
                var size = font.Font.MeasureString("Current: ");
                //this.Context.BlockStore.DrawTile(renderer.Screen, new Vector2(size.X, 600), this.currTile, 0f, Color.White);
                this.curr.Draw(renderer.Screen, new Vector2(size.X, 600), new Vector2(2f, 2f), Color.White, this.Context.BlockStore);
            }

            renderer.World.DrawRectangle(
                new Vector2(-1, 1),
                new Size2(this.Context.Map.Width * this.Context.BlockStore.TileSize + 2, this.Context.Map.Height * this.Context.BlockStore.TileSize + 2),
                Color.White);

            base.Draw(renderer, gameTime);
        }
    }
}

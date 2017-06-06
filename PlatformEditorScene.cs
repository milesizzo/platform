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

        private void SetUpUI()
        {
            if (this.menu != null)
            {
                UserInterface.RemoveEntity(this.menu);
            }
            this.menu = new PanelTabs();
            this.menu.SetAnchor(Anchor.Center);
            this.menu.Size = new Vector2(1400, 1000);
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
            var tilePicker = new TilePicker(this.Context.BlockStore);
            tilePicker.OnTileClick += (e, tile) =>
            {
                this.curr = new TileStencil();
                this.curr[0, 0] = tile;
            };
            tilePicker.Scrollbar.Max = (uint)(this.menu.Size.Y * 4); // we have to guess at the maximum height...
            tilePicker.Scrollbar.StepsCount = (uint)this.Context.BlockStore.TileSize;
            tilesMenu.panel.AddChild(tilePicker);

            // stencil menu
            var stencilMenu = (this.menu as PanelTabs).AddTab("Stencils", PanelSkin.Default);
            var stencilPicker = new StencilPicker(this.Context.BlockStore, this.stencils);
            stencilPicker.OnStencilClick += (e, stencil) =>
            {
                this.curr = stencil;
            };
            stencilPicker.Scrollbar.Max = (uint)(this.menu.Size.Y * 4); // we have to guess at the maximum height...
            stencilMenu.panel.AddChild(stencilPicker);

            UserInterface.AddEntity(this.menu);
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

            // stencils
            this.stencils.Clear();
#if STATICDATA
            this.stencils.AddRange(StaticData16x16.Instance.CreateStencils());
            using (var serializer = new MgiJsonSerializer("Stencils16x16.json", SerializerMode.Write))
            {
                serializer.Context.WriteList("stencils", this.stencils, PlatformSerialize.Write);
            }
#else
            using (var serializer = new MgiJsonSerializer("Stencils16x16.json", SerializerMode.Read))
            {
                this.stencils.AddRange(serializer.Context.ReadList<TileStencil>("stencils", PlatformSerialize.Read));
            }
#endif

            this.SetUpUI();

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

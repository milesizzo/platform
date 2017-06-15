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
        private Vector2? pan = null;
        private readonly List<TileStencil> stencils = new List<TileStencil>();
        private Entity mainMenu = null;
        private Entity contextMenu = null;
        private Entity currentUI = null;

        private IPlacementMode mode;

        private struct Placement
        {
            public Point Location;
            public GameEngine.Helpers.MouseButton Button;
        }
        private Placement? lastPlacement = null;
        private TileStencil.Layer lastLayer = TileStencil.Layer.Blocking;

        public PlatformEditorScene(string name, GraphicsDevice graphics) : base(name, graphics)
        {
        }

        protected override PlatformContext CreateContext()
        {
            PlatformContext context;
            if (File.Exists("default.ctx"))
            {
                context = BinPlatformContextSerializer.Load("default.ctx");
            }
            else
            {
                context = new PlatformContext();
                context.Map = BinTileMapSerializer.Load(DefaultMap);
            }
            context.Camera = this.Camera;
            context.Enabled = false;
            return context;
        }

        private void SetUpUI()
        {
            this.currentUI = null;
            if (this.mainMenu != null)
            {
                UserInterface.Active.RemoveEntity(this.mainMenu);
            }
            if (this.contextMenu != null)
            {
                UserInterface.Active.RemoveEntity(this.contextMenu);
            }
            this.mainMenu = new PanelTabs();
            this.mainMenu.SetAnchor(Anchor.Center);
            this.mainMenu.Size = new Vector2(1400, 1000);
            this.mainMenu.Visible = false;

            // file menu
            var fileMenu = (this.mainMenu as PanelTabs).AddTab("File", PanelSkin.Default);
            var processButton = new Button("Process", anchor: Anchor.AutoCenter, size: new Vector2(400, 100));
            processButton.OnClick += (b) =>
            {
                var processor = new PFPTMapProcessor();
                processor.Process(this.Context.Map);
            };
            fileMenu.panel.AddChild(processButton);
            var saveBlockStoreButton = new Button("Save BlockStore", anchor: Anchor.AutoCenter, size: new Vector2(400, 100));
            saveBlockStoreButton.OnClick += (b) =>
            {
                this.SaveBlockStore();
            };
            fileMenu.panel.AddChild(saveBlockStoreButton);

            var saveContextButton = new Button("Save context", anchor: Anchor.AutoCenter, size: new Vector2(400, 100));
            saveContextButton.OnClick += (b) =>
            {
                BinPlatformContextSerializer.Save("default.ctx", this.Context);
            };
            fileMenu.panel.AddChild(saveContextButton);

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
            var quitButton = new Button("Quit", anchor: Anchor.BottomRight, size: new Vector2(400, 100));
            quitButton.OnClick += (b) =>
            {
                this.SceneEnded = true;
            };
            fileMenu.panel.AddChild(quitButton);

            // tiles menu
            var tilesMenu = (this.mainMenu as PanelTabs).AddTab("Tiles", PanelSkin.Default);
            var tilePicker = new TilePicker(
                this.Context.BlockStore,
                this.Context.BlockStore.Tiles.Select((s, i) => new Tile(i)),
                anchor: Anchor.CenterLeft,
                size: new Vector2(1000, 0));
            tilesMenu.panel.AddChild(tilePicker);
            var tileSettingsPanel = new Panel(new Vector2(300, 0), skin: PanelSkin.Simple, anchor: Anchor.CenterRight);
            tilesMenu.panel.AddChild(tileSettingsPanel);
            tilePicker.OnItemClick += (e, tile) =>
            {
                var curr = new TileStencil();
                curr[0, 0] = tile;
                this.mode = new TilePlacement(curr, this.lastLayer);
                tileSettingsPanel.ClearChildren();
                var asTile = tile as Tile;
                if (asTile != null)
                {
                    var flagsLabel = new Label("Flags:", anchor: Anchor.AutoInline);
                    tileSettingsPanel.AddChild(flagsLabel);
                    foreach (var flag in EnumHelper.GetValues<TileFlags>())
                    {
                        if (flag == TileFlags.None) continue;
                        var checkBox = new CheckBox($"{flag}", anchor: Anchor.AutoCenter);
                        checkBox.Checked = this.Context.BlockStore[asTile.Id].HasFlag(flag);
                        checkBox.OnValueChange += (entity) =>
                        {
                            var currState = this.Context.BlockStore[asTile.Id];
                            if (checkBox.Checked)
                            {
                                currState |= flag;
                            }
                            else
                            {
                                currState &= ~flag;
                            }
                            this.Context.BlockStore[asTile.Id] = currState;
                        };
                        tileSettingsPanel.AddChild(checkBox);
                    }
                }
            };
            tilePicker.Scrollbar.Max = (uint)(this.mainMenu.Size.Y * 4); // we have to guess at the maximum height...

            // stencil menu
            var stencilMenu = (this.mainMenu as PanelTabs).AddTab("Stencils", PanelSkin.Default);
            var stencilPicker = new StencilPicker(this.Context.BlockStore, this.stencils);
            stencilPicker.OnStencilClick += (e, stencil) =>
            {
                this.mode = new TilePlacement(stencil, this.lastLayer);
            };
            stencilPicker.Scrollbar.Max = (uint)(this.mainMenu.Size.Y * 4); // we have to guess at the maximum height...
            stencilMenu.panel.AddChild(stencilPicker);

            // materials menu
            var materialsMenu = (this.mainMenu as PanelTabs).AddTab("Materials", PanelSkin.Default);
            var materialList = new SelectList(new Vector2(400, 300), anchor: Anchor.TopLeft);
            var settingsPanel = new Panel(new Vector2(600, 0), anchor: Anchor.CenterRight, skin: PanelSkin.Simple);
            materialsMenu.panel.AddChild(settingsPanel);

            foreach (var material in EnumHelper.GetValues<MaterialType>())
            {
                if (material != MaterialType.None) materialList.AddItem($"{material}");
            }
            materialList.OnValueChange += (e) =>
            {
                var material = (MaterialType)(materialList.SelectedIndex + 1);
                settingsPanel.ClearChildren();
                var materialTilePicker = new TilePicker(
                    this.Context.BlockStore,
                    this.Context.BlockStore.Materials[material].Select(id => new Tile(id)),
                    size: new Vector2(0, 700),
                    anchor: Anchor.AutoCenter);
                settingsPanel.AddChild(materialTilePicker);
                var deleteTile = new Button("Delete tile", anchor: Anchor.AutoCenter);
                deleteTile.OnClick += (entity) =>
                {
                    var asTile = materialTilePicker.SelectedItem as Tile;
                    if (asTile != null)
                    {
                        materialTilePicker.RemoveSelected();
                        this.Context.BlockStore.Materials[material].Remove(asTile.Id);
                    }
                };
                settingsPanel.AddChild(deleteTile);
                var addTile = new Button("Add tile", anchor: Anchor.AutoCenter);
                addTile.OnClick += (entity) =>
                {
                    var materialNewTilePicker = new TilePicker(
                        this.Context.BlockStore,
                        this.Context.BlockStore.Tiles.Select((s, i) => new Tile(i)),
                        size: new Vector2(1000, 900),
                        anchor: Anchor.TopCenter);
                    materialNewTilePicker.OnItemClick += (picker, tile) =>
                    {
                        var asTile = tile as Tile;
                        if (asTile != null)
                        {
                            this.Context.BlockStore.Materials[material].Add(asTile.Id);
                            materialTilePicker.AddItem(tile);
                        }
                        UserInterface.Active.RemoveEntity(materialNewTilePicker);
                    };
                    UserInterface.Active.AddEntity(materialNewTilePicker);
                };
                settingsPanel.AddChild(addTile);
            };
            materialsMenu.panel.AddChild(materialList);

            // lights menu
            var lightsMenu = (this.mainMenu as PanelTabs).AddTab("Lights", PanelSkin.Default);
            var colourPanel = new Panel(new Vector2(0, 250), skin: PanelSkin.Simple, anchor: Anchor.AutoCenter);
            var redLabel = new Label("Red", anchor: Anchor.AutoCenter);
            var redSlider = new Slider(min: 0, max: 255, skin: SliderSkin.Default, anchor: Anchor.AutoCenter);
            redSlider.Value = 255;
            var greenLabel = new Label("Green", anchor: Anchor.AutoCenter);
            var greenSlider = new Slider(min: 0, max: 255, skin: SliderSkin.Default, anchor: Anchor.AutoCenter);
            greenSlider.Value = 255;
            var blueLabel = new Label("Blue", anchor: Anchor.AutoCenter);
            var blueSlider = new Slider(min: 0, max: 255, skin: SliderSkin.Default, anchor: Anchor.AutoCenter);
            blueSlider.Value = 255;
            colourPanel.AddChild(redLabel);
            colourPanel.AddChild(redSlider);
            colourPanel.AddChild(greenLabel);
            colourPanel.AddChild(greenSlider);
            colourPanel.AddChild(blueLabel);
            colourPanel.AddChild(blueSlider);
            lightsMenu.panel.AddChild(colourPanel);
            var animationPanel = new Panel(new Vector2(0, 150), skin: PanelSkin.Simple, anchor: Anchor.AutoCenter);
            var animationLabel = new Label("Animation", anchor: Anchor.AutoCenter);
            var animationDropdown = new DropDown(Vector2.Zero, anchor: Anchor.AutoCenter);
            animationDropdown.AddItem("None");
            animationDropdown.AddItem("Candle");
            animationDropdown.SelectedIndex = 0;
            animationPanel.AddChild(animationLabel);
            animationPanel.AddChild(animationDropdown);
            lightsMenu.panel.AddChild(animationPanel);
            var lightTypeDropdown = new DropDown(new Vector2(0, 100), anchor: Anchor.AutoCenter);
            lightTypeDropdown.AddItem("Ambient");
            lightTypeDropdown.AddItem("Specular");
            lightTypeDropdown.SelectedIndex = 0;
            lightsMenu.panel.AddChild(lightTypeDropdown);

            var updateButton = new Button("Set Light", anchor: Anchor.BottomCenter);
            updateButton.OnClick += (e) =>
            {
                var light = new Light();
                switch (animationDropdown.SelectedIndex)
                {
                    case 0:
                        light.animation = null;
                        break;
                    case 1:
                        light.animation = Light.Candle;
                        break;
                    default:
                        light.animation = null;
                        break;
                }
                light.LightType = (Light.Type)lightTypeDropdown.SelectedIndex;
                light.Colour = new Color(redSlider.Value, greenSlider.Value, blueSlider.Value);
                this.mode = new LightPlacement(light);
            };
            lightsMenu.panel.AddChild(updateButton);

            UserInterface.Active.AddEntity(this.mainMenu);
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

        private void ShowUI(Entity entity)
        {
            if (this.currentUI != null)
            {
                this.currentUI.Visible = false;
                if (this.contextMenu != null)
                {
                    UserInterface.Active.RemoveEntity(this.contextMenu);
                    this.contextMenu = null;
                }
                this.currentUI = null;
            }
            if (entity != null)
            {
                this.currentUI = entity;
                this.currentUI.Visible = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (this.currentUI != null && this.currentUI.Visible)
            {
                if (KeyboardHelper.KeyPressed(Keys.Escape))
                {
                    this.ShowUI(null);
                }
            }
            else
            {
                // show UI
                if (KeyboardHelper.KeyPressed(Keys.Escape))
                {
                    this.ShowUI(this.mainMenu);
                }

                // debug mode
                if (KeyboardHelper.KeyPressed(Keys.F12))
                {
                    AbstractObject.DebugInfo = !AbstractObject.DebugInfo;
                }

                // pick layer
                var asTilePlacement = this.mode as TilePlacement;
                if (asTilePlacement != null)
                {
                    if (KeyboardHelper.KeyPressed(Keys.D1))
                    {
                        this.lastLayer = TileStencil.Layer.Background;
                    }
                    if (KeyboardHelper.KeyPressed(Keys.D2))
                    {
                        this.lastLayer = TileStencil.Layer.Foreground;
                    }
                    if (KeyboardHelper.KeyPressed(Keys.D3))
                    {
                        this.lastLayer = TileStencil.Layer.Blocking;
                    }
                    asTilePlacement.Layer = this.lastLayer;
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
                if (this.mode != null)
                {
                    var mouse = Mouse.GetState();
                    var mouseWorld = this.MouseToWorld;
                    var mouseTile = this.Context.WorldToTile(mouseWorld);
                    var last = this.lastPlacement;
                    if (mouse.LeftButton == ButtonState.Pressed && (!last.HasValue || last.Value.Location != mouseTile || last.Value.Button != GameEngine.Helpers.MouseButton.Left))
                    {
                        this.mode.Stamp(this.Context, mouseWorld);
                        this.lastPlacement = new Placement { Location = mouseTile, Button = GameEngine.Helpers.MouseButton.Left };
                    }
                    if (mouse.RightButton == ButtonState.Pressed && (!last.HasValue || last.Value.Location != mouseTile || last.Value.Button != GameEngine.Helpers.MouseButton.Right))
                    {
                        if (KeyboardHelper.KeyDown(Keys.LeftShift))
                        {
                            var menu = this.mode.ContextMenu(this.Context, mouseWorld);
                            UserInterface.Active.AddEntity(this.contextMenu);
                            this.ShowUI(menu);
                            this.contextMenu = menu;
                        }
                        else
                        {
                            this.mode.Clear(this.Context, mouseWorld);
                            this.lastPlacement = new Placement { Location = mouseTile, Button = GameEngine.Helpers.MouseButton.Right };
                        }
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
            var mouse = Mouse.GetState();
            var text = new StringBuilder();
            text.AppendLine($"Camera:");
            text.AppendLine($"    Bounds: {this.Camera.Viewport.Bounds}");
            text.AppendLine($"  Position: {this.Camera.Position}");
            text.AppendLine();
            text.AppendLine($"Time in game   : {this.Context.Time}");
            text.AppendLine($"Mouse (screen) : {mouse.X}, {mouse.Y}");
            text.AppendLine($"Mouse (world)  : {this.MouseToWorld}");
            font.DrawString(renderer.Screen, new Vector2(0, 0), text.ToString(), Color.White);

            if (this.mode != null && this.currentUI == null)
            {
                this.mode.DrawDebug(this.Context, this.MouseToWorld, renderer, font, new Vector2(0, 600));
            }

            // draw a circle around each light
            foreach (var light in this.Context.LightSources)
            {
                renderer.World.DrawCircle(light.AbsolutePosition, 16f, 16, light.Colour);
            }

            // draw a rectangle around the entire map
            renderer.World.DrawRectangle(
                new Vector2(-1, -1),
                new Size2(this.Context.Map.Width * this.Context.BlockStore.TileSize + 2, this.Context.Map.Height * this.Context.BlockStore.TileSize + 2),
                Color.White);

            base.Draw(renderer, gameTime);
        }
    }
}

using GameEngine.GameObjects;
using System;
using System.Text;
using GameEngine.Content;
using GameEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameEngine.Templates;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using GameEngine.Helpers;
using CommonLibrary;
using Platform.Controllers;

namespace Platform
{
    public class PlatformGameScene : BasePlatformGameScene
    {
        private CharacterObject player;
        private CharacterStore characters = new CharacterStore();
        private bool godMode = false;

        public PlatformGameScene(string name, GraphicsDevice graphics) : base(name, graphics)
        {
        }

        protected override PlatformContext CreateContext()
        {
            return new PlatformContext(this.Camera, 2048, 1024);
        }

        private IGameObject MakeTree(Point basePos, float z, string asset)
        {
            var random = new Random();
            var tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = Store.Instance.Sprites<SingleSpriteTemplate>("Base", asset);
            tree.Position3D = new Vector3(basePos.X * this.Context.BlockStore.TileSize, basePos.Y * this.Context.BlockStore.TileSize - tree.Sprite.Height - 5f, z);
            this.Context.AddObject(tree);
            return tree;
        }

        private void GenerateTerrain()
        {
            var random = new Random();
            var terrain = new TerrainGenerator(this.Context.Map);
            foreach (var point in terrain.Generate())
            {
                // tree pls
                this.MakeTree(point, (float)random.NextDouble(), random.Choice("tree1", "tree2", "tree3", "tree4"));
            }
        }

        public override void SetUp()
        {
            base.SetUp();

            this.Context.Map = BinTileMapSerializer.Load("editor.map");
            /*if (File.Exists("landscape.map"))
            {
                this.Context.Map = BinTileMapSerializer.Load("landscape.map");
            }
            else
            {
                this.GenerateTerrain();
                BinTileMapSerializer.Save("landscape.map", this.Context.Map);
            }*/
            this.Context.Map.SaveToImage(this.Graphics, "map.png");

            var startY = 160f * this.Context.BlockStore.TileSize;

            this.characters.GetOrAdd("Cat", (name) => new Character(name)
            {
                JumpPower = 500f,
                WalkSpeed = 400f,
                RunSpeed = 500f,
                ClimbSpeed = 300f,
                WalkMaxSpeed = 150f,
                RunMaxSpeed = 250f,
                ClimbMaxSpeed = 100f,
                WaterModifier = 0.4f,
                SwimPower = 200f,
                Sprite = Store.Instance.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player.cat"),
                Bounds = new RectangleF(Point2.Zero, new Size2(8, 16)),
            });

            this.characters.GetOrAdd("Bear", (name) => new Character(name)
            {
                JumpPower = 400f,
                WalkSpeed = 300f,
                RunSpeed = 400f,
                ClimbSpeed = 300f,
                WalkMaxSpeed = 100f,
                RunMaxSpeed = 150f,
                ClimbMaxSpeed = 100f,
                WaterModifier = 0.4f,
                SwimPower = 200f,
                Sprite = Store.Instance.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player.bear"),
                Bounds = new RectangleF(Point2.Zero, new Size2(8, 16)),
            });

            this.characters.GetOrAdd("Pig", (name) => new Character(name)
            {
                JumpPower = 450f,
                WalkSpeed = 350f,
                RunSpeed = 450f,
                ClimbSpeed = 300f,
                WalkMaxSpeed = 125f,
                RunMaxSpeed = 175f,
                ClimbMaxSpeed = 100f,
                WaterModifier = 0.4f,
                SwimPower = 200f,
                Sprite = Store.Instance.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player.pig"),
                Bounds = new RectangleF(Point2.Zero, new Size2(8, 16)),
            });

            var controller = new HumanCharacterController();
            controller[HumanActions.Jump] = new KeyboardAction(Keys.Space);
            controller[HumanActions.Swim] = new KeyboardAction(Keys.Space);
            controller[HumanActions.WalkLeft] = new KeyboardAction(Keys.A);
            controller[HumanActions.WalkRight] = new KeyboardAction(Keys.D);
            controller[HumanActions.RunModifier] = new OrAction(new KeyboardAction(Keys.LeftShift), new KeyboardAction(Keys.RightShift));
            controller[HumanActions.Squat] = new KeyboardAction(Keys.S);

            this.player = new CharacterObject(this.Context);
            this.player.Character = this.characters["Cat"];
            this.player.Controller = controller;
            this.player.Position3D = new Vector3(1280, startY, 0.5f);
            this.player.IsGravityEnabled = !this.godMode;
            this.Context.AddObject(this.player);
            this.Context.AttachLightSource(this.player, new Light
            {
                RelativePosition = new Vector2(this.player.Bounds.Width / 2, this.player.Bounds.Height / 2),
                Colour = Color.Yellow
            });
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();
            var elapsed = gameTime.GetElapsedSeconds();
            if (keyboard.IsKeyDown(Keys.Escape))
            {
                this.SceneEnded = true;
            }

            if (KeyboardHelper.KeyPressed(Keys.D1))
            {
                this.player.Character = this.characters["Cat"];
            }
            if (KeyboardHelper.KeyPressed(Keys.D2))
            {
                this.player.Character = this.characters["Bear"];
            }
            if (KeyboardHelper.KeyPressed(Keys.D3))
            {
                this.player.Character = this.characters["Pig"];
            }

            if (KeyboardHelper.KeyPressed(Keys.F12))
            {
                AbstractObject.DebugInfo = !AbstractObject.DebugInfo;
            }
            if (KeyboardHelper.KeyPressed(Keys.F11))
            {
                this.Context.LightsEnabled = !this.Context.LightsEnabled;
            }
            if (KeyboardHelper.KeyPressed(Keys.F10))
            {
                this.godMode = !this.godMode;
                this.player.IsGravityEnabled = !this.godMode;
            }
            if (KeyboardHelper.KeyPressed(Keys.OemPlus))
            {
                this.Context.Time += TimeSpan.FromHours(1);
            }
            if (KeyboardHelper.KeyPressed(Keys.OemMinus))
            {
                this.Context.Time -= TimeSpan.FromHours(1);
            }

            var animation = string.Empty;

            if (this.godMode)
            {
                if (keyboard.IsKeyDown(Keys.Down))
                {
                    if (this.player.Velocity.Y < 150f)
                    {
                        this.player.Velocity += new Vector2(0, 150f);
                    }
                }
                else if (keyboard.IsKeyDown(Keys.Up))
                {
                    if (this.player.Velocity.Y > -150f)
                    {
                        this.player.Velocity += new Vector2(0, -150f);
                    }
                }
                else if (this.player.Velocity.Y > 0)
                {
                    this.player.Velocity = new Vector2(this.player.Velocity.X, MathHelper.Max(this.player.Velocity.Y - 20f, 0));
                }
                else if (this.player.Velocity.Y < 0)
                {
                    this.player.Velocity = new Vector2(this.player.Velocity.X, MathHelper.Min(this.player.Velocity.Y + 20f, 0));
                }
            }

            this.Camera.LookAt(this.player.Position);
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            base.Draw(renderer, gameTime);
            var text = new StringBuilder();
            text.AppendLine($"Camera:");
            text.AppendLine($"       Bounds: {this.Camera.Viewport.Bounds}");
            text.AppendLine($"     Position: {this.Camera.Position}");
            text.AppendLine($"Ambient light: {this.Context.AmbientLight}");
            text.AppendLine($"   Background: {this.Context.AmbientBackground}");
            text.AppendLine($" Time in game: {this.Context.Time}");
            text.AppendLine($"    Character: {this.player.Character?.Name}");
            Store.Instance.Fonts("Base", "debug").DrawString(renderer.Screen, new Vector2(0, 0), text.ToString(), Color.White);
        }
    }
}

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

namespace Platform
{
    public class Character : ITemplate
    {
        private readonly string name;

        public float JumpPower;
        public float WalkSpeed;
        public float RunSpeed;
        public float ClimbSpeed;
        public float WalkMaxSpeed;
        public float RunMaxSpeed;
        public float ClimbMaxSpeed;
        public float WaterModifier;
        public float SwimPower;
        public NamedAnimatedSpriteSheetTemplate Sprite;

        public Character(string name)
        {
            this.name = name;
        }

        public string Name { get { return this.name; } }
    }

    public class CharacterStore : TemplateStore<Character>
    {
        //
    }

    public class PlatformGameScene : BasePlatformGameScene
    {
        private enum Facing
        {
            Left,
            Right
        }

        private VisiblePlatformObject player;
        private CharacterStore characters = new CharacterStore();
        private Character character;
        private string playerAnimation;
        private Facing playerFacing;
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
                Sprite = Store.Instance.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player.cat")
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
                Sprite = Store.Instance.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player.bear")
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
                Sprite = Store.Instance.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player.pig")
            });

            this.character = this.characters["Cat"];

            this.player = new VisiblePlatformObject(this.Context);
            this.player.Position3D = new Vector3(1280, startY, 0.5f);
            this.playerFacing = Facing.Right;
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
            /*if (keyboard.IsKeyDown(Keys.Right))
            {
                this.Camera.Position += new Vector2(elapsed * 100, 0);
            }
            if (keyboard.IsKeyDown(Keys.Left))
            {
                this.Camera.Position -= new Vector2(elapsed * 100, 0);
            }
            if (keyboard.IsKeyDown(Keys.Up))
            {
                this.Camera.Position -= new Vector2(0, elapsed * 100);
            }
            if (keyboard.IsKeyDown(Keys.Down))
            {
                this.Camera.Position += new Vector2(0, elapsed * 100);
            }*/

            if (KeyboardHelper.KeyPressed(Keys.D1))
            {
                this.character = this.characters["Cat"];
                this.playerAnimation = null;
            }
            if (KeyboardHelper.KeyPressed(Keys.D2))
            {
                this.character = this.characters["Bear"];
                this.playerAnimation = null;
            }
            if (KeyboardHelper.KeyPressed(Keys.D3))
            {
                this.character = this.characters["Pig"];
                this.playerAnimation = null;
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

            if (this.player.InWater)
            {
                if (KeyboardHelper.KeyPressed(Keys.Space))
                {
                    this.player.Velocity = new Vector2(this.player.Velocity.X, -this.character.SwimPower);
                }
            }
            else if (this.player.OnGround)
            {
                if (KeyboardHelper.KeyPressed(Keys.Space))
                {
                    this.player.Velocity = new Vector2(this.player.Velocity.X, -this.character.JumpPower);
                }
            }

            var squatting = false;
            if (keyboard.IsKeyDown(Keys.S))
            {
                squatting = true;
            }
            else
            {
                var modifier = this.player.InWater ? this.character.WaterModifier : 1.0f;
                if (keyboard.IsKeyDown(Keys.D))
                {
                    this.playerFacing = Facing.Right;
                    if (keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift))
                    {
                        if (this.player.Velocity.X < this.character.RunMaxSpeed * modifier)
                        {
                            var velocity = MathHelper.Min(this.player.Velocity.X + this.character.RunSpeed * elapsed, this.character.RunMaxSpeed);
                            this.player.Velocity = new Vector2(velocity, this.player.Velocity.Y);
                        }
                    }
                    else
                    {
                        if (this.player.Velocity.X < this.character.WalkMaxSpeed * modifier)
                        {
                            var velocity = MathHelper.Min(this.player.Velocity.X + this.character.WalkSpeed * elapsed, this.character.WalkMaxSpeed);
                            this.player.Velocity = new Vector2(velocity, this.player.Velocity.Y);
                        }
                    }
                }
                if (keyboard.IsKeyDown(Keys.A))
                {
                    this.playerFacing = Facing.Left;
                    if (keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift))
                    {
                        if (this.player.Velocity.X > -this.character.RunMaxSpeed * modifier)
                        {
                            var velocity = MathHelper.Max(this.player.Velocity.X - this.character.RunSpeed * elapsed, -this.character.RunMaxSpeed);
                            this.player.Velocity = new Vector2(velocity, this.player.Velocity.Y);
                        }
                    }
                    else
                    {
                        if (this.player.Velocity.X > -this.character.WalkMaxSpeed * modifier)
                        {
                            var velocity = MathHelper.Max(this.player.Velocity.X - this.character.WalkSpeed * elapsed, -this.character.WalkMaxSpeed);
                            this.player.Velocity = new Vector2(velocity, this.player.Velocity.Y);
                        }
                    }
                }
            }

            if (this.player.Velocity.Y < -(4f * this.character.JumpPower / 15f))
            {
                switch (this.playerFacing)
                {
                    case Facing.Left:
                        animation = "JumpLeft1";
                        break;
                    case Facing.Right:
                        animation = "JumpRight1";
                        break;
                }
            }
            else if (this.player.Velocity.Y > 4f * this.character.JumpPower / 15f)
            {
                switch (this.playerFacing)
                {
                    case Facing.Left:
                        animation = "JumpLeft3";
                        break;
                    case Facing.Right:
                        animation = "JumpRight3";
                        break;
                }
            }
            else if (this.player.Velocity.Y < 0 || this.player.Velocity.Y > 0)
            {
                switch (this.playerFacing)
                {
                    case Facing.Left:
                        animation = "JumpLeft2";
                        break;
                    case Facing.Right:
                        animation = "JumpRight2";
                        break;
                }
            }
            else
            {
                if (this.player.Velocity.X > this.character.WalkMaxSpeed)
                {
                    animation = squatting ? "SlideRight" : "RunRight";
                }
                else if (this.player.Velocity.X > 1f)
                {
                    animation = squatting ? "SlideRight" : "WalkRight";
                }
                else if (this.player.Velocity.X < -this.character.WalkMaxSpeed)
                {
                    animation = squatting ? "SlideLeft" : "RunLeft";
                }
                else if (this.player.Velocity.X < -1f)
                {
                    animation = squatting ? "SlideLeft" : "WalkLeft";
                }
                else
                {
                    if (squatting)
                    {
                        switch (this.playerFacing)
                        {
                            case Facing.Left:
                                animation = "SquatLeft";
                                break;
                            case Facing.Right:
                                animation = "SquatRight";
                                break;
                        }
                    }
                    else
                    {
                        switch (this.playerFacing)
                        {
                            case Facing.Left:
                                animation = "IdleLeft";
                                break;
                            case Facing.Right:
                                animation = "IdleRight";
                                break;
                        }
                    }
                }
            }

            // slow down
            if (this.player.Velocity.X > 0)
            {
                this.player.Velocity = new Vector2(MathHelper.Max(this.player.Velocity.X - 250f * elapsed, 0), this.player.Velocity.Y);
            }
            else if (this.player.Velocity.X < 0)
            {
                this.player.Velocity = new Vector2(MathHelper.Min(this.player.Velocity.X + 250f * elapsed, 0), this.player.Velocity.Y);
            }

            if (!string.IsNullOrEmpty(animation) && animation != this.playerAnimation)
            {
                this.player.Sprite = this.character.Sprite.GetAnimation(animation);
                this.playerAnimation = animation;
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
            text.AppendLine($"    Character: {this.character.Name}");
            Store.Instance.Fonts("Base", "debug").DrawString(renderer.Screen, new Vector2(0, 0), text.ToString(), Color.White);
        }
    }
}

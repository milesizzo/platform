using GameEngine.GameObjects;
using GameEngine.Scenes;
using System;
using System.Linq;
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
using System.IO;
using CommonLibrary.Serializing;
using Platform.Serializing;

namespace Platform
{
    public class PlatformGameScene : BasePlatformGameScene
    {
        private VisiblePlatformObject player;
        private string playerAnimation;
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

            this.player = new VisiblePlatformObject(this.Context);
            this.player.Position3D = new Vector3(1280, startY, 0.5f);
            this.player.Sprite = Store.Instance.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player.cat").GetAnimation("IdleRight");
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
            if (keyboard.IsKeyDown(Keys.Right))
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
                if (keyboard.IsKeyDown(Keys.S))
                {
                    if (this.player.Velocity.Y < 150f)
                    {
                        this.player.Velocity += new Vector2(0, 150f);
                    }
                }
                else if (keyboard.IsKeyDown(Keys.W))
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
            else if (keyboard.IsKeyDown(Keys.W) && this.player.OnGround)
            {
                this.player.Velocity += new Vector2(0, -750f);
            }

            if (keyboard.IsKeyDown(Keys.D))
            {
                animation = "WalkRight";
                if (this.player.Velocity.X < 150f)
                {
                    this.player.Velocity += new Vector2(20f, 0);
                }
            }
            if (keyboard.IsKeyDown(Keys.A))
            {
                animation = "WalkLeft";
                if (this.player.Velocity.X > -150f)
                {
                    this.player.Velocity -= new Vector2(20f, 0);
                }
            }

            if (!keyboard.IsKeyDown(Keys.A) && !keyboard.IsKeyDown(Keys.D))
            {
                // slow down
                if (this.player.Velocity.X > 0)
                {
                    this.player.Velocity = new Vector2(MathHelper.Max(this.player.Velocity.X - 20f, 0), this.player.Velocity.Y);
                }
                else if (this.player.Velocity.X < 0)
                {
                    this.player.Velocity = new Vector2(MathHelper.Min(this.player.Velocity.X + 20f, 0), this.player.Velocity.Y);
                }
                switch (this.playerAnimation)
                {
                    case "WalkRight":
                        animation = "IdleRight";
                        break;
                    case "WalkLeft":
                        animation = "IdleLeft";
                        break;
                }
            }

            if (KeyboardHelper.KeyPressed(Keys.F2))
            {
                BinTileMapSerializer.Save("landscape.map", this.Context.Map);
            }

            if (!string.IsNullOrEmpty(animation) && animation != this.playerAnimation)
            {
                this.player.Sprite = Store.Instance.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player.cat").GetAnimation(animation);
                this.playerAnimation = animation;
            }
            this.Camera.LookAt(this.player.Position);
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            base.Draw(renderer, gameTime);
            var text = new StringBuilder();
            text.AppendLine($"Camera:");
            text.AppendLine($"    Bounds: {this.Camera.Viewport.Bounds}");
            text.AppendLine($"  Position: {this.Camera.Position}");
            text.AppendLine($"Ambient light: {this.Context.AmbientLight}");
            text.AppendLine($"Background   : {this.Context.AmbientBackground}");
            text.AppendLine($"Time in game : {this.Context.Time}");
            Store.Instance.Fonts("Base", "debug").DrawString(renderer.Screen, new Vector2(0, 0), text.ToString(), Color.White);
        }
    }
}

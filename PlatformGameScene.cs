using GameEngine.GameObjects;
using GameEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameEngine.Content;
using GameEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameEngine.Templates;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using GameEngine.Helpers;

namespace Platform
{
    public class PlatformGameScene : GameScene<PlatformContext>
    {
        private VisiblePlatformObject player;
        private string playerAnimation;

        public PlatformGameScene(string name, GraphicsDevice graphics, Store store) : base(name, graphics, store)
        {
            //
        }

        public override void SetUp()
        {
            base.SetUp();
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "default"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone001"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone002"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone003"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone004"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone005"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone006"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone007"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone008"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone009"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone010"));
            for (var i = 0; i < this.Context.Map.Sprites.Count; i++)
            {
                this.Context.Map.BlockingTiles.Add(i);
            }

            var random = new Random();
            var x = 0;
            foreach (var cell in this.Context.Map.Rows[8].Columns)
            {
                if ((x + 1) % 5 != 0) cell.Foreground.Add(random.Next(1, 11));
                x++;
            }
            x = 0;
            foreach (var cell in this.Context.Map.Rows[11].Columns)
            {
                if ((x + 1) % 10 != 0) cell.Foreground.Add(random.Next(1, 11));
                x++;
            }

            /*var random = new Random();
            for (var i = 0; i < 100000; i++)
            {
                var col = random.Next(this.Context.Map.Width);
                var row = random.Next(this.Context.Map.Height);
                this.Context.Map[row, col].TileId = 1;
            }*/
            this.player = new VisiblePlatformObject(this.Context);
            this.player.Position3D = new Vector3(10, 100, 0.5f);
            this.player.Sprite = this.Store.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player").GetAnimation("IdleRight");
            this.Context.AddObject(this.player);

            var tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = this.Store.Sprites<SingleSpriteTemplate>("Base", "tree1");
            tree.Position3D = new Vector3(120, 100, 0f);
            this.Context.AddObject(tree);

            tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = this.Store.Sprites<SingleSpriteTemplate>("Base", "tree2");
            tree.Position3D = new Vector3(200, 100, 0.6f);
            this.Context.AddObject(tree);

            tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = this.Store.Sprites<SingleSpriteTemplate>("Base", "tree3");
            tree.Position3D = new Vector3(300, 100, 0f);
            this.Context.AddObject(tree);

            tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = this.Store.Sprites<SingleSpriteTemplate>("Base", "tree4");
            tree.Position3D = new Vector3(500, 100, 0f);
            this.Context.AddObject(tree);

            this.Camera.LookAt(new Vector2(0, 0));
            this.Camera.SamplerState = SamplerState.PointClamp;
            this.Camera.Zoom = 2f;
        }

        protected override PlatformContext CreateContext()
        {
            return new PlatformContext(this.Store, this.Camera);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();
            var elapsed = gameTime.GetElapsedSeconds();
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

            var animation = string.Empty;

            if (keyboard.IsKeyDown(Keys.W) && this.player.OnGround)
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

            if (!string.IsNullOrEmpty(animation) && animation != this.playerAnimation)
            {
                this.player.Sprite = this.Store.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player").GetAnimation(animation);
                this.playerAnimation = animation;
            }
            this.Camera.LookAt(this.player.Position);
        }

        public override void PreDraw(Renderer renderer)
        {
            renderer.Screen.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            renderer.World.Begin(sortMode: SpriteSortMode.BackToFront, blendState: BlendState.NonPremultiplied, transformMatrix: this.Camera.GetViewMatrix(), samplerState: this.Camera.SamplerState);
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            this.Camera.Clear(Color.CornflowerBlue);
            base.Draw(renderer, gameTime);
            var text = new StringBuilder();
            text.AppendLine($"Camera:");
            text.AppendLine($"    Bounds: {this.Camera.Viewport.Bounds}");
            text.AppendLine($"  Position: {this.Camera.Position}");
            this.Store.Fonts("Base", "debug").DrawString(renderer.Screen, new Vector2(0, 0), text.ToString(), Color.White);
        }
    }
}

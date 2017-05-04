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
        private RenderTarget2D lightsTarget;
        private RenderTarget2D mainTarget;
        private Effect effect1;
        private ISpriteTemplate lightMask;

        public PlatformGameScene(string name, GraphicsDevice graphics, Store store) : base(name, graphics, store)
        {
        }

        public override void SetUp()
        {
            base.SetUp();

            var pp = this.Graphics.PresentationParameters;
            this.lightsTarget = new RenderTarget2D(this.Graphics, pp.BackBufferWidth, pp.BackBufferHeight);
            this.mainTarget = new RenderTarget2D(this.Graphics, pp.BackBufferWidth, pp.BackBufferHeight);

            this.effect1 = this.Store.Content.Load<Effect>("lighteffect");
            this.lightMask = this.Store.Sprites<ISpriteTemplate>("Base", "lightmask");

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
            /*this.Context.AttachLightSource(this.player, new Light
            {
                RelativePosition = new Vector2(this.player.Bounds.Width / 2, this.player.Bounds.Height / 2)
            });*/

            var tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = this.Store.Sprites<SingleSpriteTemplate>("Base", "tree1");
            tree.Position3D = new Vector3(120, 100, 0f);
            this.Context.AddObject(tree);
            this.Context.AttachLightSource(tree, new Light
            {
                RelativePosition = new Vector2(tree.Bounds.Width / 2, tree.Bounds.Height / 2)
            });

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

            this.Context.LightsEnabled = true;

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
            if (KeyboardHelper.KeyPressed(Keys.F11))
            {
                this.Context.LightsEnabled = !this.Context.LightsEnabled;
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
            // draw the lights
            this.Graphics.SetRenderTarget(this.lightsTarget);
            this.Graphics.Clear(this.Context.AmbientLight);
            if (this.Context.LightsEnabled)
            {
                renderer.World.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.Additive, transformMatrix: this.Camera.GetViewMatrix());
                foreach (var light in this.Context.LightSources.Where(l => l.IsEnabled && l.IsOperating))
                {
                    this.Store.Sprites<ISpriteTemplate>("Base", "lightmask").DrawSprite(renderer.World, light.AbsolutePosition, light.Colour, 0, light.Size);
                }
                renderer.World.End();
            }

            // draw the world
            this.Graphics.SetRenderTarget(this.mainTarget);
            this.Graphics.Clear(this.Context.AmbientBackground);
            renderer.Screen.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            renderer.World.Begin(sortMode: SpriteSortMode.BackToFront, blendState: BlendState.NonPremultiplied, transformMatrix: this.Camera.GetViewMatrix(), samplerState: this.Camera.SamplerState);
        }

        public override void PostDraw(Renderer renderer)
        {
            // render the world (to current render target: mainTarget)
            renderer.World.End();

            // set render target to screen, clear it
            this.Graphics.SetRenderTarget(null);
            this.Graphics.Clear(Color.Black);

            // reuse the "World" sprite batch to combine the lightsTarget and mainTarget (using our lighting effect)
            renderer.World.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend);
            this.effect1.Parameters["t0"].SetValue(this.lightsTarget);
            this.effect1.CurrentTechnique.Passes[0].Apply();
            renderer.World.Draw(this.mainTarget, Vector2.Zero, Color.White);
            // render the combined targets
            renderer.World.End();

            // finally, render the screen layer
            renderer.Screen.End();
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
            this.Store.Fonts("Base", "debug").DrawString(renderer.Screen, new Vector2(0, 0), text.ToString(), Color.White);
        }
    }
}

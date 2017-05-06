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
using CommonLibrary;

namespace Platform
{
    public abstract class BasePlatformGameScene : GameScene<PlatformContext>
    {
        private RenderTarget2D lightsTarget;
        private RenderTarget2D mainTarget;
        private Effect effect1;
        private ISpriteTemplate lightMask;

        public BasePlatformGameScene(string name, GraphicsDevice graphics, Store store) : base(name, graphics, store)
        {
            //
        }

        public override void SetUp()
        {
            base.SetUp();

            var pp = this.Graphics.PresentationParameters;
            this.lightsTarget = new RenderTarget2D(this.Graphics, pp.BackBufferWidth, pp.BackBufferHeight);
            this.mainTarget = new RenderTarget2D(this.Graphics, pp.BackBufferWidth, pp.BackBufferHeight);

            this.effect1 = this.Store.Content.Load<Effect>("lighteffect");
            this.lightMask = this.Store.Sprites<ISpriteTemplate>("Base", "lightmask");

            this.Context.LightsEnabled = true;

            this.Camera.LookAt(new Vector2(0, 0));
            this.Camera.SamplerState = SamplerState.PointClamp;
            this.Camera.Zoom = 2f;
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
                    this.lightMask.DrawSprite(renderer.World, light.AbsolutePosition, light.Colour, 0, light.Size);
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
    }

    public class PlatformGameScene : BasePlatformGameScene
    {
        private VisiblePlatformObject player;
        private string playerAnimation;

        public PlatformGameScene(string name, GraphicsDevice graphics, Store store) : base(name, graphics, store)
        {
        }

        protected override PlatformContext CreateContext()
        {
            return new PlatformContext(this.Store, this.Camera, 2048, 1024, 16);
        }

        private void MakePlatform(Rectangle rect)
        {
            var map = this.Context.Map;
            map[rect.Top, rect.Left].Foreground.Add(0);
            map[rect.Top, rect.Right].Foreground.Add(2);
            for (var i = rect.Left + 1; i < rect.Right; i++)
            {
                map[rect.Top, i].Foreground.Add(1);
                map[rect.Bottom, i].Blocking.Add(10);
                for (var j = rect.Top + 1; j < rect.Bottom; j++)
                {
                    map[j, i].Blocking.Add(7);
                }
            }
            map[rect.Bottom, rect.Left].Blocking.Add(9);
            map[rect.Bottom, rect.Right].Blocking.Add(11);
            for (var j = rect.Top + 1; j < rect.Bottom; j++)
            {
                map[j, rect.Left].Blocking.Add(6);
                map[j, rect.Right].Blocking.Add(8);
            }
        }

        private IGameObject MakeTree(Point basePos, float z, string asset)
        {
            var random = new Random();
            var map = this.Context.Map;
            var tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = this.Store.Sprites<SingleSpriteTemplate>("Base", asset);
            tree.Position3D = new Vector3(basePos.X * map.TileSize, basePos.Y * map.TileSize - tree.Sprite.Height - 5f, z);
            this.Context.AddObject(tree);
            return tree;
        }

        public override void SetUp()
        {
            base.SetUp();

            /*this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "default"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone001"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone002"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone003"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone004"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone005"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone006"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone007"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone008"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone009"));
            this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", "stone010"));*/
            //this.Context.Map.Sprites.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.ground").Sprites);
            for (var i = 1; i <= 12; i++)
                this.Context.Map.Sprites.Add(this.Store.Sprites<SingleSpriteTemplate>("Base", $"rock0{i:00}"));

            for (var i = 0; i < this.Context.Map.Sprites.Count; i++)
            {
                this.Context.Map.BlockingTiles.Add(i);
            }

            foreach (var cell in this.Context.Map.Rows.Last().Columns)
            {
                cell.Blocking.Add(1);
            }

            var random = new Random();
            var x = 0;
            int y = 500;
            while (x < this.Context.Map.Width)
            {
                var width = random.Next(7, 20);
                var height = random.Next(3, width);
                var platform = new Rectangle(x, y, width, height);
                if (platform.Right >= this.Context.Map.Width)
                {
                    break;
                }
                this.MakePlatform(platform);
                for (var i = 0; i < random.Next(3) + 1; i++)
                {
                    var asset = random.Choice("tree1", "tree2", "tree3", "tree4");
                    this.MakeTree(new Point(random.Next(platform.Left, MathHelper.Clamp(platform.Right - 4, platform.Left + 2, platform.Right)), platform.Top), (float)random.NextDouble(), asset);
                }
                x += platform.Width + random.Next(3) + 4;
                y += random.Next(3) - 1;
                if (y < 0 || y >= this.Context.Map.Height)
                    break;
            }
            /*
            var x = 0;
            foreach (var cell in this.Context.Map.Rows[8].Columns)
            {
                if ((x + 1) % 5 != 0)
                {
                    cell.Blocking.Add(random.Next(0, 2));
                }
                x++;
            }
            x = 0;
            foreach (var cell in this.Context.Map.Rows[11].Columns)
            {
                if ((x + 1) % 10 != 0)
                {
                    cell.Blocking.Add(random.Next(0, 2));
                }
                x++;
            }
            */

            /*var random = new Random();
            for (var i = 0; i < 100000; i++)
            {
                var col = random.Next(this.Context.Map.Width);
                var row = random.Next(this.Context.Map.Height);
                this.Context.Map[row, col].TileId = 1;
            }*/
            var startY = 490f * this.Context.Map.TileSize;

            this.player = new VisiblePlatformObject(this.Context);
            this.player.Position3D = new Vector3(10, startY, 0.5f);
            this.player.Sprite = this.Store.Sprites<NamedAnimatedSpriteSheetTemplate>("Base", "player").GetAnimation("IdleRight");
            this.Context.AddObject(this.player);
            this.Context.AttachLightSource(this.player, new Light
            {
                RelativePosition = new Vector2(this.player.Bounds.Width / 2, this.player.Bounds.Height / 2),
                Colour = Color.Yellow
            });

            /*var tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = this.Store.Sprites<SingleSpriteTemplate>("Base", "tree1");
            tree.Position3D = new Vector3(120, startY, 0f);
            this.Context.AddObject(tree);
            this.Context.AttachLightSource(tree, new Light
            {
                RelativePosition = new Vector2(tree.Bounds.Width / 2, tree.Bounds.Height / 2)
            });

            tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = this.Store.Sprites<SingleSpriteTemplate>("Base", "tree2");
            tree.Position3D = new Vector3(200, startY, 0.6f);
            this.Context.AddObject(tree);

            tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = this.Store.Sprites<SingleSpriteTemplate>("Base", "tree3");
            tree.Position3D = new Vector3(300, startY, 0f);
            this.Context.AddObject(tree);

            tree = new VisiblePlatformObject(this.Context);
            tree.Sprite = this.Store.Sprites<SingleSpriteTemplate>("Base", "tree4");
            tree.Position3D = new Vector3(500, startY, 0f);
            this.Context.AddObject(tree);

            var torch = new VisiblePlatformObject(this.Context);
            torch.Sprite = this.Store.Sprites<ISpriteTemplate>("Base", "torch1");
            torch.Position3D = new Vector3(400, startY, 0.6f);
            torch.IsPhysicsEnabled = false;
            this.Context.AddObject(torch);
            this.Context.AttachLightSource(torch, new Light
            {
                RelativePosition = new Vector2(15, 19),
                Colour = Color.Yellow,
                Animation = Light.Candle,
            });*/
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

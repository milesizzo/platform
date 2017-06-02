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
    public abstract class BasePlatformGameScene : GameScene<PlatformContext>
    {
        private RenderTarget2D lightsTarget;
        private RenderTarget2D mainTarget;
        private Effect effect1;
        private ISpriteTemplate lightMask;

        public BasePlatformGameScene(string name, GraphicsDevice graphics) : base(name, graphics)
        {
            //
        }

        public override void SetUp()
        {
            base.SetUp();

            var pp = this.Graphics.PresentationParameters;
            this.lightsTarget = new RenderTarget2D(this.Graphics, pp.BackBufferWidth, pp.BackBufferHeight);
            this.mainTarget = new RenderTarget2D(this.Graphics, pp.BackBufferWidth, pp.BackBufferHeight);

            this.effect1 = Store.Instance.Content.Load<Effect>("lighteffect");
            this.lightMask = Store.Instance.Sprites<ISpriteTemplate>("Base", "lightmask");

            this.Context.LightsEnabled = true;

            /*var blockStore = new BlockStore(20);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.crusader").Sprites);
            blockStore.Blocks[MaterialType.Dirt].AddRange(new[] { 80, 81, 82, 83 });
            blockStore.Blocks[MaterialType.Water].AddRange(new[] { 243, 244, 245 });
            this.Context.BlockStore = blockStore;
            using (var serializer = new MgiJsonSerializer("blockstore.json", SerializerMode.Write))
            {
                serializer.Context.Write("blockstore", this.Context.BlockStore, PlatformSerialize.Write);
            }*/
            var blockStore = new BlockStore(32);
            /*blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.001").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.002").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.003").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.004").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.005").Sprites);*/
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.uppgk").Sprites);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.ppgk").Sprites);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.ssgt").Sprites);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.stonefence").Sprites);
            blockStore.Blocks[MaterialType.Dirt].AddRange(new[] { 8 });
            blockStore.Blocks[MaterialType.Water].AddRange(new[] { 74 });
            blockStore.Blocks[MaterialType.Grass].AddRange(new[] { 43, 44 });
            this.Context.BlockStore = blockStore;
            using (var serializer = new MgiJsonSerializer("blockstore.json", SerializerMode.Write))
            {
                serializer.Context.Write("blockstore", this.Context.BlockStore, PlatformSerialize.Write);
            }
            /*
            using (var serializer = new MgiJsonSerializer("BlockStore.json", SerializerMode.Read))
            {
                this.Context.BlockStore = serializer.Context.Read<BlockStore, Store>("blockstore", this.Store, PlatformSerialize.Read);
            }
            */
            this.Context.BlockStore.Prefabs.Add("tree1", new VisibleObjectPrefab(this.Context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree1")));
            this.Context.BlockStore.Prefabs.Add("tree2", new VisibleObjectPrefab(this.Context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree2")));
            this.Context.BlockStore.Prefabs.Add("tree3", new VisibleObjectPrefab(this.Context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree3")));
            this.Context.BlockStore.Prefabs.Add("tree4", new VisibleObjectPrefab(this.Context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree4")));

            this.Camera.LookAt(new Vector2(0, 0));
            this.Camera.SamplerState = SamplerState.PointClamp;
            this.Camera.Zoom = 4f;
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

#define MANUAL_BLOCKSTORE

using CommonLibrary.Serializing;
using GameEngine.Content;
using GameEngine.Graphics;
using GameEngine.Scenes;
using GameEngine.Templates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platform.Serializing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

#if MANUAL_BLOCKSTORE
            this.Context.BlockStore = this.Setup16x16BlockStore();
#else
            using (var serializer = new MgiJsonSerializer("Content\\BlockStore32x32.json", SerializerMode.Read))
            {
                this.Context.BlockStore = serializer.Context.Read<BlockStore, Store>("blockstore", this.Store, PlatformSerialize.Read);
            }
#endif
            this.Context.BlockStore.Prefabs.Add("tree1", new VisibleObjectPrefab(this.Context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree1")));
            this.Context.BlockStore.Prefabs.Add("tree2", new VisibleObjectPrefab(this.Context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree2")));
            this.Context.BlockStore.Prefabs.Add("tree3", new VisibleObjectPrefab(this.Context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree3")));
            this.Context.BlockStore.Prefabs.Add("tree4", new VisibleObjectPrefab(this.Context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree4")));

            this.Camera.LookAt(new Vector2(0, 0));
            this.Camera.SamplerState = SamplerState.PointClamp;
            this.Camera.Zoom = 4f;
        }

        private BlockStore Setup32x32BlockStore()
        {
            var blockStore = new BlockStore(32);
            /*blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.001").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.002").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.003").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.004").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.005").Sprites);*/
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.uppgk").Sprites);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.ppgk").Sprites);
            //blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.ssgt").Sprites);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.stonefence").Sprites);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.blocks").Sprites);
            blockStore.Blocks[MaterialType.Dirt].AddRange(new[] { 8 });
            blockStore.Blocks[MaterialType.Water].AddRange(new[] { 74 });
            blockStore.Blocks[MaterialType.Grass].AddRange(new[] { 43, 44 });
            using (var serializer = new MgiJsonSerializer("BlockStore32x32.json", SerializerMode.Write))
            {
                serializer.Context.Write("blockstore", blockStore, PlatformSerialize.Write);
            }
            return blockStore;
        }

        private BlockStore Setup16x16BlockStore()
        {
            var blockStore = new BlockStore(16);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.pfpt").Sprites);
            blockStore.Blocks[MaterialType.Dirt].Add(101);
            using (var serializer = new MgiJsonSerializer("BlockStore16x16.json", SerializerMode.Write))
            {
                serializer.Context.Write("blockstore", blockStore, PlatformSerialize.Write);
            }
            return blockStore;
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
}

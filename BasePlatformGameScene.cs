#define MANUAL_BLOCKSTORE

using CommonLibrary.Serializing;
using GameEngine.Content;
using GameEngine.Graphics;
using GameEngine.Scenes;
using GameEngine.Templates;
using GeonBit.UI;
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
        private SpriteBatch ui;
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

            this.ui = new SpriteBatch(this.Graphics);

            var pp = this.Graphics.PresentationParameters;
            this.lightsTarget = new RenderTarget2D(this.Graphics, pp.BackBufferWidth, pp.BackBufferHeight);
            this.mainTarget = new RenderTarget2D(this.Graphics, pp.BackBufferWidth, pp.BackBufferHeight);

            this.effect1 = Store.Instance.Content.Load<Effect>("lighteffect");
            this.lightMask = Store.Instance.Sprites<ISpriteTemplate>("Base", "lightmask");

            this.Context.LightsEnabled = true;

#if STATICDATA
            this.Context.BlockStore = StaticData16x16.Instance.SetupBlockStore();
            StaticData16x16.Instance.AddPrefabs(this.Context);
            using (var serializer = new MgiJsonSerializer($"BlockStore{StaticData16x16.Instance.Name}.json", SerializerMode.Write))
            {
                serializer.Context.Write("blockstore", this.Context.BlockStore, PlatformSerialize.Write);
            }
#else
            using (var serializer = new MgiJsonSerializer("Content\\BlockStore16x16.json", SerializerMode.Read))
            {
                this.Context.BlockStore = serializer.Context.Read<BlockStore, Store>("blockstore", Store.Instance, PlatformSerialize.Read);
            }
#endif

            this.Camera.LookAt(new Vector2(0, 0));
            this.Camera.SamplerState = SamplerState.PointClamp;
            this.Camera.Zoom = 4f;
        }

        public override void PreDraw(Renderer renderer)
        {
            UserInterface.Active.Draw(this.ui);

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

        public override void Update(GameTime gameTime)
        {
            UserInterface.Active.Update(gameTime);
            base.Update(gameTime);
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

            UserInterface.Active.DrawMainRenderTarget(this.ui);
        }
    }
}

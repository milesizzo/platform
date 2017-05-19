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

namespace Platform
{
    public class PlatformEditorScene : BasePlatformGameScene
    {
        private enum Layer
        {
            Background,
            Foreground,
            Blocking,
        }
        private int currTile = 0;
        private Layer layer = Layer.Blocking;

        public PlatformEditorScene(string name, GraphicsDevice graphics, Store store) : base(name, graphics, store)
        {
        }

        protected override PlatformContext CreateContext()
        {
            var context = new PlatformContext(this.Store, this.Camera, 2048, 1024, 16);
            context.Enabled = false;
            return context;
        }

        public override void SetUp()
        {
            base.SetUp();

            //this.Context.Map.Sprites.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.ground").Sprites);

            this.Context.Time = new TimeSpan(12, 0, 0);
            this.Camera.Position = Vector2.Zero;
        }

        private Point MouseToTile
        {
            get
            {
                var mouse = Mouse.GetState();
                var mouseWorld = this.Camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y));
                return this.Context.WorldToTile(mouseWorld);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var mouse = Mouse.GetState();
            if (KeyboardHelper.KeyPressed(Keys.D1))
            {
                this.layer = Layer.Background;
            }
            if (KeyboardHelper.KeyPressed(Keys.D2))
            {
                this.layer = Layer.Foreground;
            }
            if (KeyboardHelper.KeyPressed(Keys.D3))
            {
                this.layer = Layer.Blocking;
            }
            var mouseTile = this.MouseToTile;
            if (MouseHelper.ButtonPressed(MouseButton.Left) && this.Context.IsInBounds(mouseTile))
            {
                var cell = this.Context.Map[mouseTile];
                /*
                switch (this.layer)
                {
                    case Layer.Background:
                        cell.Background.Add(this.currTile);
                        break;
                    case Layer.Blocking:
                        cell.Blocking.Add(this.currTile);
                        break;
                    case Layer.Foreground:
                        cell.Foreground.Add(this.currTile);
                        break;
                }
                */
            }
            var scroll = MouseHelper.ScrollDirection;
            if (scroll < 0)
            {
                //this.currTile = MathHelper.Clamp(this.currTile - 1, 0, this.Context.Map.Sprites.Count - 1);
            }
            else if (scroll > 0)
            {
                //this.currTile = MathHelper.Clamp(this.currTile + 1, 0, this.Context.Map.Sprites.Count - 1);
            }
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            base.Draw(renderer, gameTime);

            var mouseTile = this.MouseToTile;
            if (this.Context.IsInBounds(mouseTile))
            {
                //this.Context.Map.Sprites[this.currTile].DrawSprite(renderer.World, new Vector2(mouseTile.X * this.Context.Map.TileSize, mouseTile.Y * this.Context.Map.TileSize), 0.1f);
            }

            var text = new StringBuilder();
            text.AppendLine($"Camera:");
            text.AppendLine($"    Bounds: {this.Camera.Viewport.Bounds}");
            text.AppendLine($"  Position: {this.Camera.Position}");
            text.AppendLine($"Time in game : {this.Context.Time}");
            text.AppendLine($"Layer        : {this.layer}");
            text.AppendLine($"Current tile : {this.currTile}");
            this.Store.Fonts("Base", "debug").DrawString(renderer.Screen, new Vector2(0, 0), text.ToString(), Color.White);
        }
    }
}

using GameEngine;
using GameEngine.Content;
using GameEngine.Graphics;
using GameEngine.Helpers;
using GameEngine.Scenes;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platform
{
    public class PlatformGame : SceneGame
    {
        public static GameWindow CurrentWindow;
        private SpriteBatch ui;
        private Panel panel;

        public PlatformGame()
        {
        }

        protected override void Initialize()
        {
            this.ui = new SpriteBatch(this.GraphicsDevice);
            UserInterface.Initialize(this.Content, BuiltinThemes.lowres);
            UserInterface.UseRenderTarget = true;
            UserInterface.GlobalScale = 1f;
            // TODO: Add your initialization logic here
            base.Initialize();
            this.IsMouseVisible = false;
        }

        protected override void LoadContent()
        {
            CurrentWindow = this.Window;
            Store.Instance.LoadFromJson("Content\\Base.json");
            this.Scenes.GetOrAdd<IScene>("Main", (key) =>
            {
                return new PlatformGameScene(key, this.GraphicsDevice);
            });
            this.Scenes.GetOrAdd<IScene>("Editor", (key) =>
            {
                return new PlatformEditorScene(key, this.GraphicsDevice);
            });
            this.SetCurrentScene("Editor");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
            UserInterface.Update(gameTime);

            if (this.CurrentScene == null)
            {
                Exit();
            }
            if (KeyboardHelper.KeyPressed(Keys.F1))
            {
                this.SetCurrentScene("Editor");
            }
            else if (KeyboardHelper.KeyPressed(Keys.F2))
            {
                this.SetCurrentScene("Main");
            }
            if (KeyboardHelper.KeyPressed(Keys.D0))
            {
                if (this.panel != null) UserInterface.RemoveEntity(this.panel);
                this.panel = new Panel(new Vector2(500, 500));
                UserInterface.AddEntity(this.panel);
            }
            if (KeyboardHelper.KeyPressed(Keys.D9))
            {
                UserInterface.RemoveEntity(this.panel);
                this.panel = null;
            }

            base.Update(gameTime);
        }

        protected override void Draw(Renderer renderer)
        {
            var font = Store.Instance.Fonts("Base", "debug");
            renderer.Screen.DrawString(font.Font, $"FPS: {this.FPS}", new Vector2(1024, 10), Color.White);
            base.Draw(renderer);
        }
    }
}

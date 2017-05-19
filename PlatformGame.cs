using GameEngine;
using GameEngine.Graphics;
using GameEngine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platform
{
    public class PlatformGame : SceneGame
    {
        public PlatformGame()
        {
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
            this.IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            this.Store.LoadFromJson("Content\\Base.json");
            this.Scenes.GetOrAdd<IScene>("Main", (key) =>
            {
                return new PlatformGameScene(key, this.GraphicsDevice, this.Store);
            });
            this.Scenes.GetOrAdd<IScene>("Editor", (key) =>
            {
                return new PlatformEditorScene(key, this.GraphicsDevice, this.Store);
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

            if (this.CurrentScene == null)
            {
                Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(Renderer renderer)
        {
            var font = this.Store.Fonts("Base", "debug");
            renderer.Screen.DrawString(font.Font, $"FPS: {this.FPS}", new Vector2(1024, 10), Color.White);
            base.Draw(renderer);
        }
    }
}

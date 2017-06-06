using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeonBit.UI.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GeonBit.UI.Entities;
using GameEngine.Templates;
using GeonBit.UI;

namespace Platform
{
    public class SpriteImage : Entity
    {
        public Vector2 FrameWidth = Vector2.One * 0.15f;
        public ISpriteTemplate Sprite;

        /// <summary>Default styling for images. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        public SpriteImage(ISpriteTemplate sprite, Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null) : base(size, anchor, offset)
        {
            this.Sprite = sprite;
            UpdateStyle(DefaultStyle);
        }

        override protected void DrawEntity(SpriteBatch spriteBatch)
        {
            // based on code in DrawUtils.DrawImage()
            var destination = UserInterface.DrawUtils.ScaleRect(this._destRectInternal, this.Scale);
            var scale = new Vector2(destination.Width / this.Sprite.Width, destination.Height / this.Sprite.Height);
            this.Sprite.DrawSprite(spriteBatch, new Vector2(destination.Location.X, destination.Location.Y), this.FillColor, 0f, scale);

            // call base draw function
            base.DrawEntity(spriteBatch);
        }
    }
}

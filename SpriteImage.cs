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
using MonoGame.Extended;

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
            var destination = UserInterface.Active.DrawUtils.ScaleRect(this._destRectInternal, this.Scale);
            var scale = new Vector2(destination.Width / this.Sprite.Width, destination.Height / this.Sprite.Height);
            this.Sprite.DrawSprite(spriteBatch, new Vector2(destination.Location.X, destination.Location.Y), this.FillColor, 0f, scale);

            // call base draw function
            base.DrawEntity(spriteBatch);
        }
    }

    public class TileImage : Panel
    {
        public Vector2 FrameWidth = Vector2.One * 0.15f;
        public ITile Tile;

        /// <summary>Default styling for images. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        private readonly BlockStore blocks;

        public Color BorderColour { get; set; }

        public TileImage(BlockStore blocks, ITile tile, Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null)
            : base(size, skin: PanelSkin.Simple, anchor: anchor, offset: offset)
        {
            this.blocks = blocks;
            this.Tile = tile;
            this.BorderColour = Color.Transparent;
            UpdateStyle(DefaultStyle);
        }

        override protected void DrawEntity(SpriteBatch spriteBatch)
        {
            base.DrawEntity(spriteBatch);

            var destination = this._destRectInternal;
            var offset = this.blocks.TileSize * this.Scale / 2f;
            var location = new Vector2(destination.Location.X + destination.Width / 2f - offset, destination.Location.Y + destination.Height / 2f - offset);
            if (this.BorderColour != Color.Transparent)
            {
                spriteBatch.DrawRectangle(destination, this.BorderColour);
            }
            this.blocks.DrawTile(spriteBatch, location, this.Tile, 0f, this.FillColor, new Vector2(this.Scale));
        }
    }
}

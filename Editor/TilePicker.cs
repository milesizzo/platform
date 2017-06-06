using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Platform.Editor
{
    public delegate void TileEventCallback(Entity entity, ITile tile);

    public class TilePicker : Panel
    {
        public TileEventCallback OnTileClick;

        public TilePicker(BlockStore blocks, Vector2? size = null, Anchor anchor = Anchor.Center, Vector2? offset = null) : base(size ?? Vector2.Zero, skin: PanelSkin.Simple, anchor: anchor, offset: offset)
        {
            var tileIndex = 0;
            foreach (var sprite in blocks.Tiles)
            {
                var tile = new Tile(tileIndex);

                var img = new SpriteImage(sprite, new Vector2(sprite.Width * 2, sprite.Height * 2), anchor: Anchor.AutoInline);
                img.SetStyleProperty("FillColor", new StyleProperty(Color.Red), EntityState.MouseHover);
                img.Padding = Vector2.Zero;
                img.Scale = 1f;
                img.OnClick += (e) =>
                {
                    this.OnTileClick?.Invoke(this, tile);
                };
                this.AddChild(img);
                tileIndex++;
            }
            this.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            //this.Scrollbar.Max = 2048;
        }
    }
}

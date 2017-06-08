using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Platform.Editor
{
    public delegate void TileEventCallback(Entity entity, ITile tile);

    public class TilePicker : Panel
    {
        public TileEventCallback OnTileClick;
        private Entity selectedTile = null;

        public TilePicker(BlockStore blocks, IEnumerable<ITile> tiles, Vector2? size = null, Anchor anchor = Anchor.Center, Vector2? offset = null, bool overflow = true) : base(size ?? Vector2.Zero, skin: PanelSkin.Simple, anchor: anchor, offset: offset)
        {
            var tileIndex = 0;
            foreach (var tile in tiles)
            {
                //var tile = new Tile(tileIndex);
                var img = new TileImage(blocks, tile, new Vector2(blocks.TileSize * 2, blocks.TileSize * 2), anchor: Anchor.AutoInline);
                img.SetStyleProperty("FillColor", new StyleProperty(Color.Red), EntityState.MouseHover);
                img.Padding = Vector2.Zero;
                img.Scale = 2f;
                img.OnClick += (e) =>
                {
                    if (this.selectedTile != null && this.GetChildren().Contains(this.selectedTile))
                    {
                        this.selectedTile.FillColor = Color.White;
                    }
                    this.selectedTile = e;
                    this.selectedTile.FillColor = Color.CornflowerBlue;
                    this.OnTileClick?.Invoke(e, tile);
                };
                img.Draggable = false;
                this.AddChild(img);
                tileIndex++;
            }
            if (overflow)
            {
                this.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            }
            //this.Scrollbar.Max = 2048;
        }
    }
}

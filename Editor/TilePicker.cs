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
        private Entity selectedEntity = null;
        private ITile selectedTile = null;
        private readonly BlockStore blocks;

        public TilePicker(BlockStore blocks, IEnumerable<ITile> tiles, Vector2? size = null, Anchor anchor = Anchor.Center, Vector2? offset = null, bool overflow = true) : base(size ?? Vector2.Zero, skin: PanelSkin.Simple, anchor: anchor, offset: offset)
        {
            this.blocks = blocks;
            foreach (var tile in tiles)
            {
                this.AddTile(tile);
            }
            if (overflow)
            {
                this.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            }
            //this.Scrollbar.Max = 2048;
        }

        public void AddTile(ITile tile)
        {
            var img = new TileImage(this.blocks, tile, new Vector2(this.blocks.TileSize * 2, this.blocks.TileSize * 2), anchor: Anchor.AutoInline);
            img.SetStyleProperty("FillColor", new StyleProperty(Color.Red), EntityState.MouseHover);
            img.Padding = Vector2.Zero;
            img.Scale = 2f;
            img.OnClick += (e) =>
            {
                if (this.selectedEntity != null && this.GetChildren().Contains(this.selectedEntity))
                {
                    this.selectedEntity.FillColor = Color.White;
                }
                this.selectedTile = tile;
                this.selectedEntity = e;
                this.selectedEntity.FillColor = Color.CornflowerBlue;
                this.OnTileClick?.Invoke(e, tile);
            };
            img.Draggable = false;
            this.AddChild(img);
        }

        public void RemoveSelected()
        {
            if (this.selectedEntity != null && this.GetChildren().Contains(this.selectedEntity))
            {
                this.RemoveChild(this.selectedEntity);
            }
            this.selectedEntity = null;
            this.selectedTile = null;
        }

        public ITile SelectedTile
        {
            get { return this.selectedTile; }
        }
    }
}

using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace Platform.Editor
{
    public delegate void TileEventCallback(Entity entity, ITile tile);

    public abstract class AbstractPicker<T> : Panel
    {
        public delegate void ItemEventCallback(Entity entity, T item);

        public ItemEventCallback OnItemClick;
        private Entity selectedEntity = null;
        private T selectedItem = default(T);

        protected AbstractPicker(Vector2? size = null, Anchor anchor = Anchor.Center, Vector2? offset = null, bool overflow = true) : base(size ?? Vector2.Zero, PanelSkin.Simple, anchor, offset)
        {
            if (overflow)
            {
                this.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            }
        }

        public void AddItem(T item)
        {
            var entity = this.CreateItemEntity(item);
            entity.SetStyleProperty("FillColor", new StyleProperty(Color.Red), EntityState.MouseHover);
            entity.Padding = Vector2.Zero;
            entity.OnClick += (e) =>
            {
                if (this.selectedEntity != null && this.GetChildren().Contains(this.selectedEntity))
                {
                    this.selectedEntity.FillColor = Color.White;
                }
                this.selectedItem = item;
                this.selectedEntity = e;
                this.selectedEntity.FillColor = Color.CornflowerBlue;
                this.OnItemClick?.Invoke(e, item);
            };
            this.AddChild(entity);
        }

        public void RemoveSelected()
        {
            if (this.selectedEntity != null && this.GetChildren().Contains(this.selectedEntity))
            {
                this.RemoveChild(this.selectedEntity);
            }
            this.selectedEntity = null;
            this.selectedItem = default(T);
        }

        public T SelectedItem
        {
            get { return this.selectedItem; }
        }

        protected abstract Entity CreateItemEntity(T item);
    }

    public class TilePicker : AbstractPicker<ITile>
    {
        private readonly BlockStore blocks;

        public TilePicker(BlockStore blocks, IEnumerable<ITile> tiles, Vector2? size = null, Anchor anchor = Anchor.Center, Vector2? offset = null, bool overflow = true) : base(size ?? Vector2.Zero, anchor: anchor, offset: offset, overflow: overflow)
        {
            this.blocks = blocks;
            foreach (var tile in tiles)
            {
                this.AddItem(tile);
            }
        }

        protected override Entity CreateItemEntity(ITile item)
        {
            var img = new TileImage(this.blocks, item, new Vector2(this.blocks.TileSize * 2, this.blocks.TileSize * 2), anchor: Anchor.AutoInline);
            img.Scale = 2f;
            return img;
        }
    }
}

using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Editor
{
    public delegate void StencilEventCallback(Entity entity, TileStencil stencil);

    public class StencilPicker : Panel
    {
        public StencilEventCallback OnStencilClick;

        public StencilPicker(BlockStore blocks, IReadOnlyList<TileStencil> stencils, Vector2? size = null, Anchor anchor = Anchor.Center, Vector2? offset = null) : base(size ?? Vector2.Zero, skin: PanelSkin.Simple, anchor: anchor, offset: offset)
        {
            var stencilSprites = stencils.Select(s => s.ToSprite(blocks)).ToList();
            var max = Vector2.Zero;
            max.X = stencilSprites.Max(s => s.Width);
            max.Y = stencilSprites.Max(s => s.Height);
            foreach (var stencil in stencils)
            {
                var sprite = stencil.ToSprite(blocks);
                var outline = new ColoredRectangle(max * 2, anchor: Anchor.AutoInline);
                outline.SpaceAfter = new Vector2(10f, 10f);
                outline.OutlineColor = Color.White;
                outline.OutlineWidth = 1;
                outline.FillColor = Color.CornflowerBlue;
                outline.SetStyleProperty("FillColor", new StyleProperty(Color.Red), EntityState.MouseHover);
                outline.OnClick += (e) =>
                {
                    this.OnStencilClick?.Invoke(this, stencil);
                };

                var img = new SpriteImage(sprite, new Vector2(sprite.Width * 2, sprite.Height * 2), anchor: Anchor.Center);
                img.Padding = Vector2.Zero;
                img.Scale = 1f;
                img.Locked = true;
                
                outline.AddChild(img);
                this.AddChild(outline);
            }
            this.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            //this.Scrollbar.Max = 2048;
        }
    }
}

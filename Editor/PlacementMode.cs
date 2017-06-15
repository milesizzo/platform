using GameEngine.Graphics;
using GameEngine.Templates;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Editor
{
    public interface IPlacementMode
    {
        void Stamp(PlatformContext context, Vector2 world);

        void Clear(PlatformContext context, Vector2 world);

        Entity ContextMenu(PlatformContext context, Vector2 world);

        void DrawDebug(PlatformContext context, Vector2 world, Renderer renderer, FontTemplate font, Vector2 position);
    }

    public abstract class PlacementMode<T> : IPlacementMode
    {
        public readonly T Current;

        protected PlacementMode(T current)
        {
            this.Current = current;
        }

        public void Stamp(PlatformContext context, Vector2 world)
        {
            if (context.IsInBounds(world))
            {
                this.StampImpl(context, world);
            }
        }

        public void Clear(PlatformContext context, Vector2 world)
        {
            if (context.IsInBounds(world))
            {
                this.ClearImpl(context, world);
            }
        }

        public Entity ContextMenu(PlatformContext context, Vector2 world)
        {
            if (!context.IsInBounds(world))
            {
                return null;
            }
            var contextMenu = new Panel(new Vector2(900, 1000));
            contextMenu.ClearChildren();
            this.ContextMenuImpl(contextMenu, context, world);
            return contextMenu;
        }

        public abstract void DrawDebug(PlatformContext context, Vector2 world, Renderer renderer, FontTemplate font, Vector2 position);

        protected abstract void StampImpl(PlatformContext context, Vector2 world);

        protected abstract void ClearImpl(PlatformContext context, Vector2 world);

        protected abstract void ContextMenuImpl(Entity parent, PlatformContext context, Vector2 world);
    }
}

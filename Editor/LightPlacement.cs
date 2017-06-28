using GameEngine.Graphics;
using GameEngine.Templates;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Editor
{
    public class LightPlacement : PlacementMode<Light>
    {
        public LightPlacement(Light light) : base(light)
        {
        }

        public override string Name
        {
            get { return "Lights"; }
        }

        protected override void StampImpl(PlatformContext context, Vector2 world)
        {
            var light = this.Current.Clone();
            light.RelativePosition = world;
            context.AttachLightSource(light);
        }

        protected override void ClearImpl(PlatformContext context, Vector2 world)
        {
            var tilePos = context.WorldToTile(world);
            var toRemove = new List<Light>();
            foreach (var light in context.LightSources)
            {
                var lightTilePos = context.WorldToTile(light.AbsolutePosition);
                if (lightTilePos == tilePos)
                {
                    toRemove.Add(light);
                }
            }
            foreach (var light in toRemove)
            {
                context.RemoveLightSource(light);
            }
        }

        protected override void ContextMenuImpl(Entity parent, PlatformContext context, Vector2 world)
        {
            throw new NotImplementedException();
        }

        public override void DrawDebug(PlatformContext context, Vector2 world, Renderer renderer, FontTemplate font, Vector2 position)
        {
            if (context.IsInBounds(world))
            {
                var tilePos = context.WorldToTile(world);
                var tileTopLeft = context.TileToWorld(tilePos);
                Light.DrawDebug(renderer, world, this.Current.Colour);
                renderer.World.DrawRectangle(tileTopLeft, new Size2(context.BlockStore.TileSize, context.BlockStore.TileSize), new Color(1f, 1f, 1f, 0.5f));

                var tileText = new StringBuilder();
                tileText.AppendLine($"   Position: {world} ({tilePos})");
                font.DrawString(renderer.Screen, new Vector2(position.X, position.Y + 200), tileText.ToString(), Color.Wheat);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameEngine.Graphics;
using GameEngine.Templates;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Platform.Editor
{
    public class SpawnPlacement : PlacementMode<Spawn>
    {
        public SpawnPlacement(Spawn spawn) : base(spawn)
        {
        }

        public override string Name
        {
            get { return "Spawn"; }
        }

        protected override void ClearImpl(PlatformContext context, Vector2 world)
        {
            var tile = context.WorldToTile(world);
            context.Spawn.RemoveAll(spawn => context.WorldToTile(spawn.World) == tile);
        }

        protected override void ContextMenuImpl(Entity parent, PlatformContext context, Vector2 world)
        {
        }

        public override void DrawDebug(PlatformContext context, Vector2 world, Renderer renderer, FontTemplate font, Vector2 position)
        {
            if (context.IsInBounds(world))
            {
                var tilePos = context.WorldToTile(world);
                var tileTopLeft = context.TileToWorld(tilePos);
                Spawn.DrawDebug(renderer, world);
                renderer.World.DrawRectangle(tileTopLeft, new Size2(context.BlockStore.TileSize, context.BlockStore.TileSize), new Color(1f, 1f, 1f, 0.5f));

                var text = new StringBuilder();
                text.AppendLine($"   Position: {world} ({tilePos})");
                text.AppendLine($"   # spawns: {context.Spawn.Count}");
                font.DrawString(renderer.Screen, new Vector2(position.X, position.Y + 200), text.ToString(), Color.Wheat);
            }
        }

        protected override void StampImpl(PlatformContext context, Vector2 world)
        {
            var spawn = this.Current.Clone();
            spawn.World = world;
            context.Spawn.Add(spawn);
        }
    }
}

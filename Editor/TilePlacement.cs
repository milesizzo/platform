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
    public class TilePlacement : PlacementMode<TileStencil>
    {
        private TileStencil.Layer layer = TileStencil.Layer.Blocking;

        public TilePlacement(TileStencil curr, TileStencil.Layer layer) : base(curr)
        {
            this.layer = layer;
        }

        public override string Name
        {
            get { return "Tiles"; }
        }

        public TileStencil.Layer Layer
        {
            get { return this.layer; }
            set { this.layer = value; }
        }

        protected override void StampImpl(PlatformContext context, Vector2 world)
        {
            var tilePos = context.WorldToTile(world);
            this.Current.Stamp(context.Map, tilePos, this.layer);
        }

        protected override void ClearImpl(PlatformContext context, Vector2 position)
        {
            var cell = context.Map[context.WorldToTile(position)];
            switch (this.layer)
            {
                case TileStencil.Layer.Background:
                    cell.Background.Clear();
                    break;
                case TileStencil.Layer.Foreground:
                    cell.Foreground.Clear();
                    break;
                case TileStencil.Layer.Blocking:
                    cell.Block = null;
                    break;
            }
        }

        protected override void ContextMenuImpl(Entity parent, PlatformContext context, Vector2 world)
        {
            var tilePos = context.WorldToTile(world);
            var cell = context.Map[tilePos];

            var background = new Panel(new Vector2(0, 256), skin: PanelSkin.Simple, anchor: Anchor.AutoCenter);
            var label = new Label("Background:", Anchor.TopCenter);
            background.AddChild(label);
            var backgroundPicker = new TilePicker(context.BlockStore, cell.Background, new Vector2(0, 128), Anchor.BottomCenter, overflow: false);
            backgroundPicker.OnItemClick += (e, tile) =>
            {
                backgroundPicker.RemoveChild(e);
                cell.Background.Remove(tile);
            };
            background.AddChild(backgroundPicker);
            parent.AddChild(background);

            var foreground = new Panel(new Vector2(0, 256), skin: PanelSkin.Simple, anchor: Anchor.AutoCenter);
            label = new Label("Foreground:", Anchor.TopCenter);
            foreground.AddChild(label);
            var foregroundPicker = new TilePicker(context.BlockStore, cell.Foreground, new Vector2(0, 128), Anchor.BottomCenter, overflow: false);
            foreground.AddChild(foregroundPicker);
            parent.AddChild(foreground);

            var blocking = new Panel(new Vector2(0, 256), skin: PanelSkin.Simple, anchor: Anchor.AutoCenter);
            label = new Label("Block:", Anchor.TopCenter);
            blocking.AddChild(label);
            var blocks = cell.Block == null ? new ITile[0] : new[] { cell.Block };
            var blockingPicker = new TilePicker(context.BlockStore, blocks, new Vector2(0, 128), Anchor.BottomCenter, overflow: false);
            blocking.AddChild(blockingPicker);
            parent.AddChild(blocking);

            var button = new Button("Clear", anchor: Anchor.BottomCenter);
            button.OnClick += (e) =>
            {
                context.Map[tilePos].Background.Clear();
                context.Map[tilePos].Foreground.Clear();
                context.Map[tilePos].Block = null;
            };
            parent.AddChild(button);
        }

        public override void DrawDebug(PlatformContext context, Vector2 world, Renderer renderer, FontTemplate font, Vector2 position)
        {
            var s = new StringBuilder();
            s.AppendLine($"Current tile : ");
            s.AppendLine($"{this.layer} ");
            font.DrawString(renderer.Screen, position, s.ToString(), Color.White);
            var size = font.Font.MeasureString(s.ToString());
            this.Current.Draw(renderer.Screen, new Vector2(size.X + position.X, position.Y), new Vector2(2f, 2f), Color.White, context.BlockStore);

            if (context.IsInBounds(world))
            {
                var tilePos = context.WorldToTile(world);
                var tileTopLeft = context.TileToWorld(tilePos);
                this.Current.Draw(renderer.World, tileTopLeft, Vector2.One, Color.White, context.BlockStore);
                renderer.World.DrawRectangle(tileTopLeft, new Size2(context.BlockStore.TileSize, context.BlockStore.TileSize), Color.White);

                var tileText = new StringBuilder();
                var cell = context.Map[tilePos];
                var flags = context.GetFlags(tilePos);
                tileText.AppendLine($"   Position: {tilePos}");
                tileText.AppendLine($" Foreground: " + string.Join(",", cell.Foreground.Select(t => t.DebugString)));
                tileText.AppendLine($" Background: " + string.Join(",", cell.Background.Select(t => t.DebugString)));
                tileText.AppendLine($"      Block: " + (cell.Block == null ? "null" : cell.Block.DebugString));
                tileText.AppendLine($"      Flags: " + flags);
                font.DrawString(renderer.Screen, new Vector2(position.X, position.Y + 200), tileText.ToString(), Color.Wheat);
            }
        }
    }
}

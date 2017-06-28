using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Editor
{
    public static class PaintOperations
    {
        private static readonly Point[] FloodFillTestPoints = new Point[]
        {
            new Point(1, 0),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(0, -1),
        };

        public static void FloodFill(PlatformContext context, Point location, TilePlacement placement)
        {
            if (!FloodFillValid(context, location, placement))
            {
                return;
            }
            var queue = new Queue<Point>();
            var set = new HashSet<Point>();
            queue.Enqueue(location);
            set.Add(location);
            while (queue.Any())
            {
                var point = queue.Dequeue();
                placement.Stamp(context, context.TileToWorld(point));
                foreach (var test in FloodFillTestPoints)
                {
                    var next = point + test;
                    if (!set.Contains(next) && FloodFillValid(context, next, placement))
                    {
                        queue.Enqueue(next);
                        set.Add(next);
                    }
                }
            }
        }

        private static bool FloodFillValid(PlatformContext context, Point location, TilePlacement placement)
        {
            if (!context.IsInBounds(location))
            {
                return false;
            }
            var cell = context.Map[location];
            switch (placement.Layer)
            {
                case TileStencil.Layer.Background:
                    return !cell.Background.Any();
                case TileStencil.Layer.Foreground:
                    return !cell.Foreground.Any();
                case TileStencil.Layer.Blocking:
                    return cell.Block == null;
            }
            return true;
        }
    }
}

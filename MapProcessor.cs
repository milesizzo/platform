using CommonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public abstract class MapProcessor
    {
        protected enum Direction
        {
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left
        }

        protected class DirtLookup : Dictionary<Direction, bool>
        {
            private readonly Func<int, int, bool> isDirtFunc;

            public DirtLookup(Func<int, int, bool> isDirtFunc)
            {
                this.isDirtFunc = isDirtFunc;
            }

            public bool Match(bool? topLeft, bool? top, bool? topRight, bool? right, bool? bottomRight, bool? bottom, bool? bottomLeft, bool? left)
            {
                if (topLeft.HasValue && this[Direction.TopLeft] != topLeft.Value) return false;
                if (top.HasValue && this[Direction.Top] != top.Value) return false;
                if (topRight.HasValue && this[Direction.TopRight] != topRight.Value) return false;
                if (right.HasValue && this[Direction.Right] != right.Value) return false;
                if (bottomRight.HasValue && this[Direction.BottomRight] != bottomRight.Value) return false;
                if (bottom.HasValue && this[Direction.Bottom] != bottom.Value) return false;
                if (bottomLeft.HasValue && this[Direction.BottomLeft] != bottomLeft.Value) return false;
                if (left.HasValue && this[Direction.Left] != left.Value) return false;
                return true;
            }

            public bool Build(TileMap map, int y, int x)
            {
                if (!this.isDirtFunc(y, x)) return false;
                // build a map of what's around us
                this[Direction.TopLeft] = this.isDirtFunc(y - 1, x - 1);
                this[Direction.Top] = this.isDirtFunc(y - 1, x);
                this[Direction.TopRight] = this.isDirtFunc(y - 1, x + 1);
                this[Direction.Right] = this.isDirtFunc(y, x + 1);
                this[Direction.BottomRight] = this.isDirtFunc(y + 1, x + 1);
                this[Direction.Bottom] = this.isDirtFunc(y + 1, x);
                this[Direction.BottomLeft] = this.isDirtFunc(y + 1, x - 1);
                this[Direction.Left] = this.isDirtFunc(y, x - 1);
                return true;
            }
        }

        public abstract void Process(TileMap map);
    }

    public class PPGKMapProcessor : MapProcessor
    {
        private static HashSet<int> DirtTypes = new HashSet<int>
        {
            49, 50, 51, 54, 55, 56, 59, 60, 64, 65, 69, 70
        };

        public override void Process(TileMap map)
        {
            var dirt = new DirtLookup((y, x) =>
            {
                if (y < 0 || y >= map.Height || x < 0 || x >= map.Width) return false;
                var cell = map[y, x];
                var material = cell.Block as Material;
                if (material != null && material.Type == MaterialType.Dirt) return true;
                var tile = cell.Block as Tile;
                if (tile != null && DirtTypes.Contains(tile.Id)) return true;
                return false;
            });

            var random = new Random();
            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                {
                    var cell = map[y, x];
                    if (dirt.Build(map, y, x))
                    {
                        var block = cell.Block;
                        if (dirt.Match(null, false, null, true, null, true, null, true))
                        {
                            // case #1
                            block = new Tile(random.Choice(50, 55));
                        }
                        else if (dirt.Match(null, false, null, false, null, true, null, true))
                        {
                            // case #2
                            block = new Tile(random.Choice(50, 55));
                        }
                        else if (dirt.Match(null, false, null, null, null, true, null, false))
                        {
                            // case #3
                            block = new Tile(60);
                        }
                        else if (dirt.Match(null, true, null, true, null, true, null, false))
                        {
                            // case #4
                            block = new Tile(random.Choice(54, 59));
                        }
                        else if (dirt.Match(false, true, null, true, null, true, null, true))
                        {
                            // case #5
                            block = new Tile(56);
                        }
                        else if (dirt.Match(null, true, null, true, null, false, null, false))
                        {
                            // case #6
                            block = new Tile(69);
                        }
                        else if (dirt.Match(null, false, null, null, null, false, null, false))
                        {
                            // case #7
                            block = new Tile(70);
                        }
                        else if (dirt.Match(null, false, null, null, null, false, null, true))
                        {
                            // case #8
                            block = new Tile(65);
                        }
                        else if (dirt.Match(null, false, null, null, null, true, false, true))
                        {
                            // case #9
                            block = new Tile(51);
                        }
                        else if (dirt.Match(null, true, null, null, null, false, null, true))
                        {
                            // case #10
                            block = new Tile(64);
                        }
                        cell.Block = block;

                        // add decorations
                        if (dirt.Match(null, false, null, null, null, null, null, null))
                        {
                            var above = map[y - 1, x];
                            // add grass etc.
                            var grass = random.Choice(43, 44);
                            above.Foreground.Add(new Tile(grass));
                            if (grass == 44)
                            {
                                // we can optionally add flowers
                                if (random.Next(5) == 0)
                                {
                                    above.Foreground.Add(new Tile(random.Choice(32, 39, 46)));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public class UPPGKMapProcessor : MapProcessor
    {
        /*private static HashSet<int> DirtTypes = new HashSet<int>
        {
            0, 1, 2, 7, 8, 9, 14, 15, 16, 21, 28, 35, 42
        };*/

        public override void Process(TileMap map)
        {
            var dirt = new DirtLookup((y, x) =>
            {
                if (y < 0 || y >= map.Height || x < 0 || x >= map.Width) return false;
                var cell = map[y, x];
                /*var material = cell.Block as Material;
                if (material != null && material.Type == MaterialType.Dirt) return true;
                var tile = cell.Block as Block;
                if (tile != null && DirtTypes.Contains(tile.Id)) return true;
                return false;*/
                return cell.Block != null;
            });

            var random = new Random();
            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                {
                    var cell = map[y, x];
                    if (dirt.Build(map, y, x))
                    {
                        var block = cell.Block;
                        if (dirt.Match(null, false, null, true, null, true, null, false))
                        {
                            block = new Tile(0);
                        }
                        else if (dirt.Match(null, false, null, false, null, true, null, true))
                        {
                            block = new Tile(14);
                        }
                        else if (dirt.Match(null, false, null, true, null, true, null, true))
                        {
                            block = new Tile(7);
                        }
                        else if (dirt.Match(null, true, null, true, null, true, null, false))
                        {
                            block = new Tile(1);
                        }
                        else if (dirt.Match(null, true, null, true, null, false, null, false))
                        {
                            block = new Tile(2);
                        }
                        else if (dirt.Match(null, true, null, false, null, false, null, true))
                        {
                            block = new Tile(16);
                        }
                        else if (dirt.Match(null, true, null, true, null, false, null, true))
                        {
                            block = new Tile(9);
                        }
                        else if (dirt.Match(null, true, null, false, null, true, null, true))
                        {
                            block = new Tile(15);
                        }
                        else if (dirt.Match(null, false, null, false, null, false, null, false))
                        {
                            block = new Tile(42);
                        }
                        else if (dirt.Match(null, false, null, true, null, false, null, false))
                        {
                            block = new Tile(21);
                        }
                        else if (dirt.Match(null, false, null, false, null, false, null, true))
                        {
                            block = new Tile(35);
                        }
                        else if (dirt.Match(null, false, null, true, null, false, null, true))
                        {
                            block = new Tile(28);
                        }
                        cell.Block = block;

                        // add decorations
                        if (dirt.Match(null, false, null, null, null, null, null, null))
                        {
                            var above = map[y - 1, x];
                            // add grass etc.
                            var grass = random.Choice(43, 44);
                            above.Foreground.Add(new Tile(grass));
                            if (random.Next(10) == 0 && grass == 43)
                            {
                                // randomly add background grass
                                above.Background.Add(new Tile(44));
                            }
                            if (grass == 44)
                            {
                                // we can optionally add flowers
                                if (random.Next(5) == 0)
                                {
                                    above.Foreground.Add(new Tile(random.Choice(32, 39, 46)));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public class PFPTMapProcessor : MapProcessor
    {
        public override void Process(TileMap map)
        {
            var dirt = new DirtLookup((y, x) =>
            {
                if (y < 0 || y >= map.Height || x < 0 || x >= map.Width) return false;
                var cell = map[y, x];
                return cell.Block != null;
            });

            var random = new Random();
            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                {
                    var cell = map[y, x];
                    if (dirt.Build(map, y, x))
                    {
                        var block = cell.Block;
                        if (dirt.Match(null, false, null, true, null, true, null, false))
                        {
                            block = new Tile(80);
                        }
                        else if (dirt.Match(null, false, null, false, null, true, null, true))
                        {
                            block = new Tile(84);
                        }
                        else if (dirt.Match(null, false, null, true, null, true, null, true))
                        {
                            block = new Tile(81);
                        }
                        else if (dirt.Match(null, true, null, true, null, true, null, false))
                        {
                            block = new Tile(90);
                        }
                        else if (dirt.Match(null, true, null, true, null, false, null, false))
                        {
                            block = new Tile(110);
                        }
                        else if (dirt.Match(null, true, null, false, null, false, null, true))
                        {
                            block = new Tile(114);
                        }
                        else if (dirt.Match(null, true, null, true, null, false, null, true))
                        {
                            block = new Tile(111);
                        }
                        else if (dirt.Match(null, true, null, false, null, true, null, true))
                        {
                            block = new Tile(94);
                        }
                        else if (dirt.Match(null, false, null, false, null, false, null, false))
                        {
                            block = new Tile(81);
                        }
                        else if (dirt.Match(null, false, null, true, null, false, null, false))
                        {
                            block = new Tile(80);
                        }
                        else if (dirt.Match(null, false, null, false, null, false, null, true))
                        {
                            block = new Tile(84);
                        }
                        else if (dirt.Match(null, false, null, true, null, false, null, true))
                        {
                            block = new Tile(81);
                        }
                        cell.Block = block;

                        // add decorations
                        /*if (dirt.Match(null, false, null, null, null, null, null, null))
                        {
                            var above = map[y - 1, x];
                            // add grass etc.
                            var grass = random.Choice(43, 44);
                            above.Foreground.Add(new Block { Id = grass });
                            if (random.Next(10) == 0 && grass == 43)
                            {
                                // randomly add background grass
                                above.Background.Add(new Block { Id = 44 });
                            }
                            if (grass == 44)
                            {
                                // we can optionally add flowers
                                if (random.Next(5) == 0)
                                {
                                    above.Foreground.Add(new Block { Id = random.Choice(32, 39, 46) });
                                }
                            }
                        }*/
                    }
                }
            }
        }
    }
}

using CommonLibrary.Serializing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Serializing
{
    public static partial class PlatformSerialize
    {
        public static void Write(ISerializer context, TileMap map)
        {
            context.Write("width", map.Width);
            context.Write("height", map.Height);
            context.Write("tilesize", map.TileSize);
            context.Write("bgcolour", map.BackgroundColour, CommonSerialize.Write);
            context.WriteList("map", map.Rows, Write);
        }

        public static void Write(ISerializer context, MapRow row)
        {
            context.WriteList("row", row.Columns, Write);
        }

        public static void Write(ISerializer context, MapCell cell)
        {
            context.WriteList("bg", cell.Background.ToList(), Write);
            context.WriteList("fg", cell.Foreground.ToList(), Write);
            if (cell.Block != null)
            {
                context.Write("block", cell.Block, Write);
            }
        }

        public static void Write(ISerializer context, ITile tile)
        {
            var material = tile as Material;
            var block = tile as Block;
            if (material != null)
            {
                context.Write("kind", "m");
                context.Write("type", (int)material.Type);
            }
            else if (block != null)
            {
                context.Write("kind", "b");
                context.Write("id", block.Id);
            }
        }

        public static void Read(IDeserializer context, out ITile tile)
        {
            var kind = context.Read<string>("kind");
            switch (kind)
            {
                case "m":
                    var typeString = context.Read<string>("type");
                    MaterialType type;
                    if (!Enum.TryParse(typeString, out type))
                    {
                        throw new InvalidOperationException("Unknown material type");
                    }
                    tile = new Material { Type = type };
                    break;
                case "b":
                    var id = context.Read<int>("id");
                    tile = new Block { Id = id };
                    break;
                default:
                    throw new InvalidOperationException("Unknown tile type");
            }
        }

        public static void Read(IDeserializer context, out MapCell cell)
        {
            cell = new MapCell();
            cell.Background.UnionWith(context.ReadList<ITile>("bg", Read));
            cell.Foreground.UnionWith(context.ReadList<ITile>("fg", Read));
            try
            {
                cell.Block = context.Read<ITile>("block", Read);
            }
            catch
            {
            }
        }

        public static void Read(IDeserializer context, out MapRow row)
        {
            row = new MapRow();
            row.Columns.AddRange(context.ReadList<MapCell>("row", Read));
        }

        public static void Read(IDeserializer context, out TileMap map)
        {
            var width = context.Read<int>("width");
            var height = context.Read<int>("height");
            var tilesize = context.Read<int>("tilesize");
            var bgcolour = context.Read<Color>("bgcolour", CommonSerialize.Read);
            map = new TileMap(width, height, tilesize);
            map.BackgroundColour = bgcolour;
            map.Rows.AddRange(context.ReadList<MapRow>("map", Read));
        }
    }
}

using CommonLibrary.Serializing;
using Microsoft.Xna.Framework;
using Platform.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Serializing
{
    public static partial class PlatformSerialize
    {
        public static void Write(ISerializer context, TileStencil stencil)
        {
            context.WriteList("tiles", stencil.Tiles, Write);
        }

        private static void Write(ISerializer context, KeyValuePair<Point, ITile> entry)
        {
            context.Write("x", entry.Key.X);
            context.Write("y", entry.Key.Y);
            context.Write("tile", entry.Value, Write);
        }

        public static void Read(IDeserializer context, out TileStencil stencil)
        {
            stencil = new TileStencil();
            foreach (var entry in context.ReadList<KeyValuePair<Point, ITile>>("tiles", Read))
            {
                stencil[entry.Key] = entry.Value;
            }
        }

        private static void Read(IDeserializer context, out KeyValuePair<Point, ITile> entry)
        {
            var x = context.Read<int>("x");
            var y = context.Read<int>("y");
            var tile = context.Read<ITile>("tile", Read);
            entry = new KeyValuePair<Point, ITile>(new Point(x, y), tile);
        }
    }
}

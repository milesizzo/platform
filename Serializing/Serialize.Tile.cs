using CommonLibrary.Serializing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Serializing
{
    public static partial class PlatformSerialize
    {
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
            else
            {
                throw new InvalidOperationException("Unknown tile type");
            }
        }

        public static void Read(IDeserializer context, out ITile tile)
        {
            var kind = context.Read<string>("kind");
            switch (kind)
            {
                case "m":
                    var type = (MaterialType)context.Read<int>("type");
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
    }
}

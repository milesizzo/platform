using CommonLibrary;
using CommonLibrary.Serializing;
using GameEngine.Content;
using GameEngine.Templates;
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
        public static void Write(ISerializer context, BlockStore store)
        {
            context.Write("tilesize", store.TileSize);
            context.WriteList("blocks", EnumHelper.GetValues<MaterialType>().Select(t => Tuple.Create(t, store.Blocks[t])).ToList(), Write);
            context.WriteList("tiles", store.Tiles, Write);
        }

        private static void Write(ISerializer context, ISpriteTemplate sprite)
        {
            var asSheet = sprite as SingleSpriteFromSheetTemplate;
            if (asSheet != null)
            {
                var index = asSheet.Parent.IndexOf(asSheet);
                context.Write("type", 0);
                context.Write("parent", asSheet.Parent.Name);
                context.Write("index", index);
            }
            else
            {
                context.Write("type", 1);
                context.Write("sprite", sprite.Name);
            }
        }

        private static void Write(ISerializer context, Tuple<MaterialType, List<int>> blocks)
        {
            context.Write("material", blocks.Item1.ToString());
            context.WriteList("tiles", blocks.Item2);
        }

        public static void Read(Store store, IDeserializer context, out BlockStore blockStore)
        {
            var tileSize = context.Read<int>("tilesize");
            blockStore = new BlockStore(tileSize);
            var blocks = context.ReadList<Tuple<MaterialType, IList<int>>>("blocks", Read);
            foreach (var block in blocks)
            {
                blockStore.Blocks[block.Item1].AddRange(block.Item2);
            }
            var tiles = context.ReadList<ISpriteTemplate, Store>("tiles", store, Read);
            blockStore.Tiles.AddRange(tiles);
        }

        private static void Read(IDeserializer context, out Tuple<MaterialType, IList<int>> blocks)
        {
            MaterialType material;
            if (!Enum.TryParse(context.Read<string>("material"), out material))
            {
                throw new InvalidOperationException("Unknown material type");
            }
            var tiles = context.ReadList<int>("tiles");
            blocks = Tuple.Create(material, tiles);
        }

        public static void Read(Store store, IDeserializer context, out ISpriteTemplate sprite)
        {
            var type = context.Read<int>("type");
            switch (type)
            {
                case 0:
                    var parent = context.Read<string>("parent");
                    var index = context.Read<int>("index");
                    sprite = (store.Sprites(parent) as SpriteSheetTemplate).Sprites[index];
                    break;
                case 1:
                    var name = context.Read<string>("sprite");
                    sprite = store.Sprites(name);
                    break;
                default:
                    throw new InvalidOperationException("Unknown sprite type");
            }
        }
    }
}

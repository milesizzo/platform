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
            context.WriteList("tiles", store.Tiles, Write);
            context.WriteList("flags", store.Flags.ToList(), Write);
            context.WriteList("materials", store.Materials.ToList(), Write);
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

        private static void Write(ISerializer context, KeyValuePair<int, TileFlags> flags)
        {
            context.Write("id", flags.Key);
            context.Write("flags", (int)flags.Value);
        }

        private static void Write(ISerializer context, KeyValuePair<MaterialType, List<int>> blocks)
        {
            context.Write("material", blocks.Key.ToString());
            context.WriteList("tiles", blocks.Value);
        }

        public static void Read(Store store, IDeserializer context, out BlockStore blockStore)
        {
            var tileSize = context.Read<int>("tilesize");
            blockStore = new BlockStore(tileSize);
            var materials = context.ReadList<KeyValuePair<MaterialType, IList<int>>>("materials", Read);
            foreach (var kvp in materials)
            {
                blockStore.Materials[kvp.Key].AddRange(kvp.Value);
            }
            var tiles = context.ReadList<ISpriteTemplate, Store>("tiles", store, Read);
            blockStore.Tiles.AddRange(tiles);
            var flags = context.ReadList<KeyValuePair<int, TileFlags>>("flags", Read);
            foreach (var flag in flags)
            {
                blockStore[flag.Key] = flag.Value;
            }
        }

        private static void Read(IDeserializer context, out KeyValuePair<int, TileFlags> flags)
        {
            flags = new KeyValuePair<int, TileFlags>(context.Read<int>("id"), (TileFlags)context.Read<int>("flags"));
        }

        private static void Read(IDeserializer context, out KeyValuePair<MaterialType, IList<int>> blocks)
        {
            MaterialType material;
            if (!Enum.TryParse(context.Read<string>("material"), out material))
            {
                throw new InvalidOperationException("Unknown material type");
            }
            var tiles = context.ReadList<int>("tiles");
            blocks = new KeyValuePair<MaterialType, IList<int>>(material, tiles);
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

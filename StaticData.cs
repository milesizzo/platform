using CommonLibrary.Serializing;
using GameEngine.Content;
using GameEngine.Templates;
using Microsoft.Xna.Framework;
using Platform.Editor;
using Platform.Serializing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public interface IStaticData
    {
        string Name { get; }

        IEnumerable<TileStencil> CreateStencils();

        BlockStore SetupBlockStore();

        void AddPrefabs(PlatformContext context);
    }

    public class StaticData16x16 : IStaticData
    {
        public static IStaticData Instance = new StaticData16x16();

        public string Name { get { return "16x16"; } }

        public IEnumerable<TileStencil> CreateStencils()
        {
            var stencil = new TileStencil();
            stencil.AddRow(0, 500, 501, 502, 503, 504);
            stencil.AddRow(0, 510, 511, 512, 513, 514);
            stencil.AddRow(0, 520, 521, 522, 523, 524);
            stencil.AddRow(1,      531, 532);
            stencil.AddRow(1,      541, 542, 543);
            yield return stencil;

            stencil = new TileStencil();
            stencil.AddRow(2,           497, 498);
            stencil.AddRow(1,      506, 507, 508, 509);
            stencil.AddRow(0, 515, 516, 517, 518, 519);
            stencil.AddRow(0, 525, 526, 527, 528, 529);
            stencil.AddRow(2,           537, 538);
            stencil.AddRow(2,           547, 548);
            stencil.Origin = new Point(2, 5);
            yield return stencil;

            stencil = new TileStencil();
            stencil.AddRow(0, 451, 452, 453);
            stencil.AddRow(0, 461, 462, 463);
            stencil.AddRow(0, 471, 472);
            stencil.AddRow(0, 481, 482, 483);
            yield return stencil;

            stencil = new TileStencil();
            stencil.AddRow(0, 456, 457);
            stencil.AddRow(0, 466, 467);
            stencil.AddRow(0, 476, 477);
            stencil.AddRow(0, 486, 487, 488);
            yield return stencil;

            // square door with window
            stencil = new TileStencil();
            stencil.AddRow(0, 220, 221);
            stencil.AddRow(0, 232, 233);
            yield return stencil;

            // square door, no window
            stencil = new TileStencil();
            stencil.AddRow(0, 222, 223);
            stencil.AddRow(0, 232, 233);
            yield return stencil;

            // square door with hinges
            stencil = new TileStencil();
            stencil.AddRow(0, 242, 243);
            stencil.AddRow(0, 252, 253);
            yield return stencil;

            // square door - open
            stencil = new TileStencil();
            stencil.AddRow(0, 420, 421);
            stencil.AddRow(0, 430, 431);
            yield return stencil;

            // round door, window
            stencil = new TileStencil();
            stencil.AddRow(0, 230, 231);
            stencil.AddRow(0, 234, 235);
            yield return stencil;

            // round door, no window
            stencil = new TileStencil();
            stencil.AddRow(0, 224, 225);
            stencil.AddRow(0, 234, 235);
            yield return stencil;

            // round door with hinges
            stencil = new TileStencil();
            stencil.AddRow(0, 244, 245);
            stencil.AddRow(0, 254, 255);
            yield return stencil;

            // round door - open
            stencil = new TileStencil();
            stencil.AddRow(0, 433, 434);
            stencil.AddRow(0, 443, 444);
            yield return stencil;

            // stone frame door
            stencil = new TileStencil();
            stencil.AddRow(0, 008, 009);
            stencil.AddRow(0, 018, 019);
            yield return stencil;

            // stone frame bars?
            stencil = new TileStencil();
            stencil.AddRow(0, 015, 016);
            stencil.AddRow(0, 025, 026);
            yield return stencil;

            // stone frame door - open
            stencil = new TileStencil();
            stencil.AddRow(0, 400, 401);
            stencil.AddRow(0, 410, 411);
            yield return stencil;

            // green bush (near doors on tilesheet)
            stencil = new TileStencil();
            stencil.AddRow(0, 240, 241);
            stencil.AddRow(0, 250, 251);
            yield return stencil;

            // rock
            stencil = new TileStencil();
            stencil.AddRow(0, 70, 71);
            yield return stencil;

            // rock
            stencil = new TileStencil();
            stencil.AddRow(0, 75, 76);
            yield return stencil;

            // fence 1 (left)
            stencil = new TileStencil();
            stencil.AddRow(0, 95);
            stencil.AddRow(0, 105);
            yield return stencil;

            // fence 1 (middle)
            stencil = new TileStencil();
            stencil.AddRow(0, 96);
            stencil.AddRow(0, 106);
            yield return stencil;

            // fence 1 (right)
            stencil = new TileStencil();
            stencil.AddRow(0, 97);
            stencil.AddRow(0, 107);
            yield return stencil;

            // fence 2 (left)
            stencil = new TileStencil();
            stencil.AddRow(0, 115);
            stencil.AddRow(0, 125);
            yield return stencil;

            // fence 2 (middle)
            stencil = new TileStencil();
            stencil.AddRow(0, 116);
            stencil.AddRow(0, 126);
            yield return stencil;

            // fence 2 (right)
            stencil = new TileStencil();
            stencil.AddRow(0, 117);
            stencil.AddRow(0, 127);
            yield return stencil;

            // fence 3 (stone - left)
            stencil = new TileStencil();
            stencil.AddRow(0, 310);
            stencil.AddRow(0, 320);
            yield return stencil;

            // fence 3 (stone - middle)
            stencil = new TileStencil();
            stencil.AddRow(0, 315);
            stencil.AddRow(0, 325);
            yield return stencil;

            // fence 3 (stone - right)
            stencil = new TileStencil();
            stencil.AddRow(0, 312);
            stencil.AddRow(0, 322);
            yield return stencil;

            // chimney 1 - left
            stencil = new TileStencil();
            stencil.AddRow(0, 354);
            stencil.AddRow(0, 364);
            yield return stencil;

            // chimney 1 - right
            stencil = new TileStencil();
            stencil.AddRow(0, 355);
            stencil.AddRow(0, 365);
            yield return stencil;

            // chimney 2 - left
            stencil = new TileStencil();
            stencil.AddRow(0, 356);
            stencil.AddRow(0, 366);
            yield return stencil;

            // chimney 2 - right
            stencil = new TileStencil();
            stencil.AddRow(0, 357);
            stencil.AddRow(0, 367);
            yield return stencil;

            // chimney 3 - left
            stencil = new TileStencil();
            stencil.AddRow(0, 358);
            stencil.AddRow(0, 368);
            yield return stencil;

            // chimney 3 - right
            stencil = new TileStencil();
            stencil.AddRow(0, 359);
            stencil.AddRow(0, 369);
            yield return stencil;

            // large crate
            stencil = new TileStencil();
            stencil.AddRow(0, 351);
            stencil.AddRow(0, 361);
            yield return stencil;

            // NOTE: gothic tiles start at 620
            // tombstone 1
            stencil = new TileStencil();
            stencil.AddRow(0, 624);
            stencil.AddRow(0, 634);
            yield return stencil;

            // tombstone 2
            stencil = new TileStencil();
            stencil.AddRow(0, 625);
            stencil.AddRow(0, 635);
            yield return stencil;

            // tombstone 3
            stencil = new TileStencil();
            stencil.AddRow(0, 626);
            stencil.AddRow(0, 636);
            yield return stencil;

            // bookshelf 1
            stencil = new TileStencil();
            stencil.AddRow(0, 990, 991);
            stencil.AddRow(0, 1000, 1001);
            yield return stencil;

            // bookshelf 2
            stencil = new TileStencil();
            stencil.AddRow(0, 994, 995);
            stencil.AddRow(0, 1004, 1005);
            yield return stencil;

            // bookshelf 3
            stencil = new TileStencil();
            stencil.AddRow(0, 996, 997);
            stencil.AddRow(0, 1006, 1007);
            yield return stencil;

            // tree 5
            stencil = new TileStencil();
            stencil.AddRow(1,       1181, 1182, 1183);
            stencil.AddRow(1,       1191, 1192, 1193);
            stencil.AddRow(0, 1200, 1201, 1202, 1203, 1204);
            stencil.AddRow(1,       1211, 1212, 1213);
            stencil.AddRow(1,       1221, 1222, 1223, 1224);
            yield return stencil;

            // tree 6
            stencil = new TileStencil();
            stencil.AddRow(2,             1187, 1188);
            stencil.AddRow(1,       1196, 1197, 1198, 1199);
            stencil.AddRow(1,       1206, 1207, 1208, 1209);
            stencil.AddRow(2,             1217, 1218);
            stencil.AddRow(0, 1225, 1226, 1227, 1228);
            yield return stencil;
        }

        public BlockStore SetupBlockStore()
        {
            var blockStore = new BlockStore(16);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.pfpt").Sprites);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.pfpt.gothic").Sprites);
            blockStore.SetFlags(TileFlags.Water, 313, 314, 318, 319, 326, 327, 328, 329, 336, 337, 338, 339, 345, 346, 347, 348, 349);
            blockStore.SetFlags(TileFlags.OneWay, 155, 156, 157, 158, 159, 165, 166, 167, 168, 169, 186);
            blockStore.SetFlags(TileFlags.Ladder, 378, 388, 398, 408, 418, 428);
            blockStore.Materials[MaterialType.Dirt].Add(101);


            return blockStore;
        }

        public void AddPrefabs(PlatformContext context)
        {
            context.BlockStore.Prefabs.Add("tree1", new VisibleObjectPrefab(context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree1")));
            context.BlockStore.Prefabs.Add("tree2", new VisibleObjectPrefab(context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree2")));
            context.BlockStore.Prefabs.Add("tree3", new VisibleObjectPrefab(context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree3")));
            context.BlockStore.Prefabs.Add("tree4", new VisibleObjectPrefab(context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree4")));
        }
    }

    public class StaticData32x32 : IStaticData
    {
        public static IStaticData Instance = new StaticData32x32();

        public string Name { get { return "32x32"; } }

        public IEnumerable<TileStencil> CreateStencils()
        {
            yield break;
        }

        public BlockStore SetupBlockStore()
        {
            var blockStore = new BlockStore(32);
            /*blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.001").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.002").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.003").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.004").Sprites);
            blockStore.Tiles.AddRange(this.Store.Sprites<SpriteSheetTemplate>("Base", "tiles.005").Sprites);*/
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.uppgk").Sprites);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.ppgk").Sprites);
            //blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.ssgt").Sprites);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.stonefence").Sprites);
            blockStore.Tiles.AddRange(Store.Instance.Sprites<SpriteSheetTemplate>("Base", "tiles.blocks").Sprites);

            //blockStore.Blocks[MaterialType.Dirt].AddRange(new[] { 8 });
            //blockStore.Blocks[MaterialType.Water].AddRange(new[] { 74 });
            //blockStore.Blocks[MaterialType.Grass].AddRange(new[] { 43, 44 });
            /*using (var serializer = new MgiJsonSerializer("BlockStore32x32.json", SerializerMode.Write))
            {
                serializer.Context.Write("blockstore", blockStore, PlatformSerialize.Write);
            }*/
            return blockStore;
        }

        public void AddPrefabs(PlatformContext context)
        {
            context.BlockStore.Prefabs.Add("tree1", new VisibleObjectPrefab(context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree1")));
            context.BlockStore.Prefabs.Add("tree2", new VisibleObjectPrefab(context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree2")));
            context.BlockStore.Prefabs.Add("tree3", new VisibleObjectPrefab(context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree3")));
            context.BlockStore.Prefabs.Add("tree4", new VisibleObjectPrefab(context, Store.Instance.Sprites<ISpriteTemplate>("Base", "tree4")));
        }
    }
}

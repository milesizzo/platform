using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platform
{
    public enum TriggerSource
    {
        Spawn,
        Use,
        Touch,
    }

    public enum TriggerAction
    {
        Map,
        Teleport,
        Item,
    }

    public interface ITrigger
    {
        void Execute();
    }

    public delegate void TriggerEventCallback();

    public class TileSwitch : ITrigger, ITile
    {
        private readonly ITile on;
        private readonly ITile off;

        public TileSwitch(ITile on, ITile off)
        {
            this.on = on;
            this.off = off;
        }

        public TriggerEventCallback OnTrigger;

        public bool Enabled { get; set; }

        public string DebugString
        {
            get { return "s"; }
        }

        public void Execute()
        {
            this.Enabled = !this.Enabled;
            this.OnTrigger?.Invoke();
        }

        public bool Draw(BlockStore store, SpriteBatch sb, Vector2 pos, Color colour, float depth, Vector2? scale = null)
        {
            var tile = this.Enabled ? this.on : this.off;
            return tile.Draw(store, sb, pos, colour, depth, scale);
        }

        public ITile Clone()
        {
            return new TileSwitch(this.on.Clone(), this.off.Clone());
        }
    }

    public class PlayerSpawn : ITrigger
    {
        private readonly PlatformContext context;
        private readonly Character character;
        private readonly Vector2 position;

        public PlayerSpawn(PlatformContext context, Character character, Vector2 position)
        {
            this.context = context;
            this.character = character;
            this.position = position;
        }

        public void Execute()
        {
            //
        }
    }
}

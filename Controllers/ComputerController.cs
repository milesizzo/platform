using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Platform.Controllers
{
    public class ComputerController : CharacterController
    {
        protected override void Brain(GameTime gameTime, CharacterObject obj)
        {
            var ts = obj.Context.BlockStore.TileSize;
            var map = obj.Context.Map;
            obj.Action = CharacterObject.Actions.None;
            if (obj.InWater)
            {
                obj.Action |= CharacterObject.Actions.Swim;
            }
            switch (obj.Direction)
            {
                case CharacterObject.Facing.Right:
                    // TODO: jump when the landscape is tending upwards, stop if the landscape is at a cliff
                    var bottomRight = obj.Bounds.BottomRight + Vector2.One;
                    obj.Action |= CharacterObject.Actions.Run;
                    /*if (!obj.Context.IsPassable(bottomRight, bottomRight + new Vector2(ts)))
                    {
                        obj.Action |= CharacterObject.Actions.Run;
                    }*/
                    if (!obj.Context.IsPassable(new Vector2(obj.Bounds.Right, obj.Bounds.Top + ts / 2f), new Vector2(obj.Bounds.Right + 4f * ts, obj.Bounds.Top + ts / 2f)))
                    {
                        obj.Action |= CharacterObject.Actions.Jump;
                    }
                    break;
                case CharacterObject.Facing.Left:
                    var bottomLeft = new Vector2(obj.Bounds.Left - 1, obj.Bounds.Bottom + 1);
                    if (!obj.Context.IsPassable(bottomLeft - new Vector2(obj.Context.BlockStore.TileSize), bottomLeft))
                    {
                        obj.Action |= CharacterObject.Actions.Run;
                    }
                    break;
            }
        }
    }
}

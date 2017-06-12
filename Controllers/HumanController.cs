using GameEngine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Controllers
{
    public enum HumanActions
    {
        WalkLeft,
        WalkRight,
        RunModifier,
        Squat,
        Jump,
        Swim,
    }

    public class KeyboardAction : IAction
    {
        private bool keyHeld = false;
        private readonly Keys key;
        
        public KeyboardAction(Keys key)
        {
            this.key = key;
        }

        public bool IsTapped
        {
            get
            {
                var held = this.IsHeld;
                if (held && !this.keyHeld)
                {
                    this.keyHeld = true;
                    return true;
                }
                else if (!held)
                {
                    this.keyHeld = false;
                }
                return false;
            }
        }

        public bool IsHeld
        {
            get { return KeyboardHelper.KeyDown(this.key); }
        }
    }

    public class HumanCharacterController : CharacterController
    {
        private readonly Dictionary<HumanActions, IAction> actions = new Dictionary<HumanActions, IAction>();

        public IAction this[HumanActions action]
        {
            get
            {
                IAction result;
                if (!this.actions.TryGetValue(action, out result))
                {
                    result = NoAction.Instance;
                }
                return result;
            }
            set
            {
                this.actions[action] = value;
            }
        }

        protected override void Brain(GameTime gameTime, CharacterObject obj)
        {
            if (this[HumanActions.WalkLeft].IsHeld)
            {
                obj.Direction = CharacterObject.Facing.Left;
            }
            if (this[HumanActions.WalkRight].IsHeld)
            {
                obj.Direction = CharacterObject.Facing.Right;
            }

            var action = CharacterObject.Actions.None;
            if (this[HumanActions.Jump].IsTapped)
            {
                action |= CharacterObject.Actions.Jump;
            }
            if (this[HumanActions.Swim].IsTapped)
            {
                action |= CharacterObject.Actions.Swim;
            }
            if (this[HumanActions.Squat].IsHeld)
            {
                action |= CharacterObject.Actions.Squat;
            }
            if (this[HumanActions.WalkLeft].IsHeld || this[HumanActions.WalkRight].IsHeld)
            {
                action |= CharacterObject.Actions.Walk;
            }
            if (this[HumanActions.RunModifier].IsHeld)
            {
                action |= CharacterObject.Actions.Run;
            }
            obj.Action = action;
        }
    }
}

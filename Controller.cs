using GameEngine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public interface IAction
    {
        bool IsTapped { get; }

        bool IsHeld { get; }
    }

    public class NoAction : IAction
    {
        public static readonly IAction Instance = new NoAction();

        public bool IsTapped { get { return false; } }

        public bool IsHeld { get { return false; } }
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

    public class AndAction : IAction
    {
        private readonly List<IAction> actions = new List<IAction>();

        public AndAction(params IAction[] actions)
        {
            this.actions.AddRange(actions);
        }

        public bool IsHeld
        {
            get { return this.actions.All(a => a.IsHeld); }
        }

        public bool IsTapped
        {
            get { return this.actions.All(a => a.IsTapped); }
        }
    }

    public class OrAction : IAction
    {
        private readonly List<IAction> actions = new List<IAction>();

        public OrAction(params IAction[] actions)
        {
            this.actions.AddRange(actions);
        }

        public bool IsHeld
        {
            get { return this.actions.Any(a => a.IsHeld); }
        }

        public bool IsTapped
        {
            get { return this.actions.Any(a => a.IsTapped); }
        }
    }

    public abstract class CharacterController
    {
        protected abstract CharacterObject.Actions GetAction(GameTime gameTime, CharacterObject.Actions input);

        protected abstract CharacterObject.Facing GetDirection(GameTime gameTime, CharacterObject.Facing input);

        public virtual void Update(GameTime gameTime, CharacterObject obj)
        {
            obj.Action = this.GetAction(gameTime, obj.Action);
            obj.Direction = this.GetDirection(gameTime, obj.Direction);
        }
    }

    public enum HumanActions
    {
        WalkLeft,
        WalkRight,
        RunModifier,
        Squat,
        Jump,
        Swim,
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

        protected override CharacterObject.Facing GetDirection(GameTime gameTime, CharacterObject.Facing input)
        {
            var facing = input;
            if (this[HumanActions.WalkLeft].IsHeld)
            {
                facing = CharacterObject.Facing.Left;
            }
            if (this[HumanActions.WalkRight].IsHeld)
            {
                facing = CharacterObject.Facing.Right;
            }
            return facing;
        }

        protected override CharacterObject.Actions GetAction(GameTime gameTime, CharacterObject.Actions input)
        {
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
            return action;
        }
    }
}

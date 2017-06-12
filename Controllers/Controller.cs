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
        protected abstract void Brain(GameTime gameTime, CharacterObject obj);

        public void Update(GameTime gameTime, CharacterObject obj)
        {
            this.Brain(gameTime, obj);
        }
    }
}

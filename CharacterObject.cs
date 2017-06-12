using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Platform.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public class CharacterObject : VisiblePlatformObject
    {
        [Flags]
        public enum Actions : byte
        {
            None = 0,
            Jump = 1 << 0,
            Swim = 1 << 1,
            Run = 1 << 2,
            Walk = 1 << 3,
            Squat = 1 << 4,
        }

        public enum Facing
        {
            Left,
            Right,
        }

        private Character character;
        private Facing facing;
        private Actions actions;
        private string currentAnimation;
        private CharacterController controller;

        public CharacterObject(PlatformContext context) : base(context)
        {
            this.facing = Facing.Right;
            this.actions = Actions.None;
            this.currentAnimation = null;
            this.character = null;
            this.controller = null;
        }

        public Character Character
        {
            get { return this.character; }
            set
            {
                this.character = value;
                if (this.character != null)
                {
                    this.bounds = new RectangleF(this.bounds.Position, this.character.Bounds.Size);
                }
                this.currentAnimation = null;
            }
        }

        public CharacterController Controller
        {
            get { return this.controller; }
            set { this.controller = value; }
        }

        public Actions Action
        {
            get { return this.actions; }
            set { this.actions = value; }
        }

        public Facing Direction
        {
            get { return this.facing; }
            set { this.facing = value; }
        }

        protected override StringBuilder DebugData()
        {
            var sb = base.DebugData();
            sb.AppendLine($"Facing: {this.facing}");
            sb.AppendLine($"Action: {this.actions}");
            return sb;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.controller?.Update(gameTime, this);

            var elapsed = gameTime.GetElapsedSeconds();

            if (this.InWater)
            {
                if (this.actions.HasFlag(Actions.Swim))
                {
                    this.Velocity = new Vector2(this.Velocity.X, -this.character.SwimPower);
                }
            }
            else if (this.OnGround)
            {
                if (this.actions.HasFlag(Actions.Jump))
                {
                    this.Velocity = new Vector2(this.Velocity.X, -this.character.JumpPower);
                }
            }

            var squatting = false;
            if (this.actions.HasFlag(Actions.Squat))
            {
                squatting = true;
            }
            else
            {
                var modifier = this.InWater ? this.character.WaterModifier : 1.0f;
                var walking = this.actions.HasFlag(Actions.Walk);
                var running = this.actions.HasFlag(Actions.Run);
                if (walking || running)
                {
                    var maxSpeed = modifier;
                    var speed = 1f;
                    if (running)
                    {
                        maxSpeed *= this.character.RunMaxSpeed;
                        speed *= this.character.RunSpeed;
                    }
                    else
                    {
                        maxSpeed *= this.character.WalkMaxSpeed;
                        speed *= this.character.WalkSpeed;
                    }

                    // move right
                    if (this.facing == Facing.Right)
                    {
                        if (this.Velocity.X < maxSpeed)
                        {
                            var velocity = MathHelper.Min(this.Velocity.X + speed * elapsed, maxSpeed);
                            this.Velocity = new Vector2(velocity, this.Velocity.Y);
                        }
                    }
                    // move left
                    else if (this.facing == Facing.Left)
                    {
                        if (this.Velocity.X > -maxSpeed)
                        {
                            var velocity = MathHelper.Max(this.Velocity.X - speed * elapsed, -maxSpeed);
                            this.Velocity = new Vector2(velocity, this.Velocity.Y);
                        }
                    }
                }
            }

            // apply horizontal drag
            if (this.Velocity.X > 0)
            {
                this.Velocity = new Vector2(MathHelper.Max(this.Velocity.X - 250f * elapsed, 0), this.Velocity.Y);
            }
            else if (this.Velocity.X < 0)
            {
                this.Velocity = new Vector2(MathHelper.Min(this.Velocity.X + 250f * elapsed, 0), this.Velocity.Y);
            }

            // update animation
            string animation = null;
            if (this.Velocity.Y < -(4f * this.character.JumpPower / 15f))
            {
                animation = $"Jump{this.facing}1";
            }
            else if (this.Velocity.Y > 4f * this.character.JumpPower / 15f)
            {
                animation = $"Jump{this.facing}3";
            }
            else if (this.Velocity.Y < 0 || this.Velocity.Y > 0)
            {
                animation = $"Jump{this.facing}2";
            }
            else
            {
                var xVelocity = Math.Abs(this.Velocity.X);
                if (xVelocity > this.character.WalkMaxSpeed)
                {
                    animation = squatting ? $"Slide{this.facing}" : $"Run{this.facing}";
                }
                else if (xVelocity > 1f)
                {
                    animation = squatting ? $"Slide{this.facing}" : $"Walk{this.facing}";
                }
                else
                {
                    animation = squatting ? $"Squat{this.facing}" : $"Idle{this.facing}";
                }
            }

            if (!string.IsNullOrEmpty(animation) && animation != this.currentAnimation)
            {
                this.Sprite = this.character.Sprite.GetAnimation(animation);
                this.currentAnimation = animation;
            }
        }
    }
}

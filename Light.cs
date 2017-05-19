using GameEngine.GameObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public delegate bool LightOperatingDelegate(TimeSpan time);
    public delegate void LightAnimationDelegate(Light light, GameTime gameTime);

    public class Light
    {
        public static LightOperatingDelegate OperatingNightOnly = time => time.Hours < 8 || time.Hours > 17;

        public static LightAnimationDelegate Candle = (light, gameTime) =>
        {
        };

        private IGameObject owner;
        private Vector2 position = Vector2.Zero;
        private Color colour = Color.White;
        private Vector2 scale = Vector2.One;
        private bool enabled = true;
        private bool operating = false;
        private LightOperatingDelegate operatingFunc = OperatingNightOnly;
        private LightAnimationDelegate animation = null;

        public Light()
        {
            this.owner = null;
        }

        public IGameObject Owner
        {
            get { return this.owner; }
            internal set { this.owner = value; }
        }

        public Vector2 AbsolutePosition
        {
            get { return this.owner == null ? this.position : this.owner.Position + this.position; }
        }

        public Vector2 RelativePosition
        {
            get { return this.position; }
            set { this.position = value; }
        }

        public Vector2 Size
        {
            get { return this.scale; }
            set { this.scale = value; }
        }

        public Color Colour
        {
            get { return this.colour; }
            set { this.colour = value; }
        }

        public bool IsEnabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        public bool IsOperating
        {
            get { return this.operating; }
        }

        public LightOperatingDelegate OperatingFunction
        {
            set { this.operatingFunc = value; }
        }

        public LightAnimationDelegate Animation
        {
            set { this.animation = value; }
        }

        internal void Update(ref TimeSpan time, GameTime gameTime)
        {
            this.operating = this.operatingFunc == null ? true : this.operatingFunc(time);
            this.animation?.Invoke(this, gameTime);
        }
    }
}

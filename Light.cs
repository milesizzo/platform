using GameEngine.GameObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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
        internal LightOperatingDelegate operatingFunc = OperatingNightOnly;
        internal LightAnimationDelegate animation = null;

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

        public Light Clone()
        {
            var light = new Light();
            light.owner = this.owner;
            light.position = this.position;
            light.colour = this.colour;
            light.scale = this.scale;
            light.enabled = this.enabled;
            light.operating = this.operating;
            light.operatingFunc = this.operatingFunc;
            light.animation = this.animation;
            return light;
        }

        internal void Update(ref TimeSpan time, GameTime gameTime)
        {
            this.operating = this.operatingFunc == null ? true : this.operatingFunc(time);
            this.animation?.Invoke(this, gameTime);
        }
    }

    public static class BinLightSerializer
    {
        public static void Save(BinaryWriter writer, Light light)
        {
            writer.Write(light.IsEnabled);
            writer.Write(light.AbsolutePosition.X);
            writer.Write(light.AbsolutePosition.Y);
            writer.Write((UInt32)light.Colour.PackedValue);
            writer.Write(light.Size.X);
            writer.Write(light.Size.Y);
            if (light.operatingFunc == Light.OperatingNightOnly)
            {
                writer.Write("nightonly");
            }
            else
            {
                writer.Write("custom");
            }
            if (light.animation == Light.Candle)
            {
                writer.Write("candle");
            }
            else
            {
                writer.Write("custom");
            }
        }

        public static Light Load(BinaryReader reader)
        {
            var light = new Light();
            light.IsEnabled = reader.ReadBoolean();
            light.RelativePosition = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            light.Colour = new Color(reader.ReadUInt32());
            light.Size = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            switch (reader.ReadString())
            {
                case "nightonly":
                    light.OperatingFunction = Light.OperatingNightOnly;
                    break;
                case "custom":
                    throw new InvalidOperationException("Can't deserialize custom");
            }
            switch (reader.ReadString())
            {
                case "candle":
                    light.Animation = Light.Candle;
                    break;
                case "custom":
                    throw new InvalidOperationException("Can't deserialize custom");
            }
            return light;
        }
    }
}

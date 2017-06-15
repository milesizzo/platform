using Microsoft.Xna.Framework;
using GameEngine.GameObjects;
using MonoGame.Extended;

namespace Platform
{
    public class PlatformObject : AbstractObject
    {
        protected RectangleF bounds = RectangleF.Empty;
        public float Z = 0f;

        public PlatformObject(PlatformContext context) : base(context)
        {
            //context.AddObject(this);
        }

        public override Vector2 Position
        {
            get { return this.bounds.Position; }
            set { this.bounds.Position = value; }
        }

        public override Vector3 Position3D
        {
            get { return new Vector3(this.Position, this.Z); }

            set
            {
                this.Position = new Vector2(value.X, value.Y);
                this.Z = value.Z;
            }
        }

        public RectangleF Bounds
        {
            get { return this.bounds; }
            set { this.bounds = value; }
        }

        public new PlatformContext Context
        {
            // TODO: store as local variable in native type?
            get { return base.Context as PlatformContext; }
        }
    }
}

using GameEngine.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using GameEngine.Templates;
using GameEngine.Graphics;
using GameEngine.Extensions;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Common;

namespace Platform
{
    public class PlatformObject : AbstractObject
    {
        protected RectangleF bounds;
        public float Z = 0f;

        public PlatformObject(PlatformContext context) : base(context)
        {
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

        public RectangleF Bounds { get { return this.bounds; } }

        public new PlatformContext Context
        {
            // TODO: store as local variable in native type?
            get { return base.Context as PlatformContext; }
        }
    }

    public class VisiblePlatformObject : PlatformObject
    {
        public bool IsPhysicsEnabled = true;
        private ISpriteTemplate sprite;
        private float frame;
        private Vector2 velocity;
        private bool onGround;

        public VisiblePlatformObject(PlatformContext context) : base(context)
        {
        }

        public Vector2 Velocity
        {
            get { return this.velocity; }
            set { this.velocity = value; }
        }

        public ISpriteTemplate Sprite
        {
            get { return this.sprite; }
            set
            {
                this.sprite = value;
                this.bounds = new RectangleF(this.Position, new Size2(value.Width, value.Height));
            }
        }

        public bool OnGround { get { return this.onGround; } }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.IsPhysicsEnabled)
            {
                var elapsed = gameTime.GetElapsedSeconds();
                if (!this.OnGround)
                {
                    // we only apply gravity if we weren't on the ground in the previous cycle
                    this.velocity.Y = MathHelper.Min(this.velocity.Y + 50f, 450f);
                }
                var dv = this.velocity * elapsed;

                // update the x axis, handle collision
                if (dv.X > 0)
                {
                    // test tile to the right
                    var xTileTopRight = this.Context.WorldToTile(new Vector2(this.bounds.Right - 1 + dv.X, this.bounds.Top));
                    var xTileBottomRight = this.Context.WorldToTile(new Vector2(this.bounds.Right - 1 + dv.X, this.bounds.Bottom - 1));
                    if (!this.Context.Map.IsPassable(xTileTopRight) || !this.Context.Map.IsPassable(xTileBottomRight))
                    {
                        this.velocity.X = 0;
                        this.bounds.X = this.Context.TileToWorld(xTileTopRight).X - this.bounds.Width;
                    }
                    else
                    {
                        this.bounds.X += dv.X;
                    }
                }
                else if (dv.X < 0)
                {
                    var xTileTopLeft = this.Context.WorldToTile(new Vector2(this.bounds.Left + dv.X, this.bounds.Top));
                    var xTileBottomLeft = this.Context.WorldToTile(new Vector2(this.bounds.Left + dv.X, this.bounds.Bottom - 1));
                    if (!this.Context.Map.IsPassable(xTileTopLeft) || !this.Context.Map.IsPassable(xTileBottomLeft))
                    {
                        this.velocity.X = 0;
                        this.bounds.X = this.Context.TileToWorld(xTileTopLeft.X + 1, xTileTopLeft.Y).X;
                    }
                    else
                    {
                        this.bounds.X += dv.X;
                    }
                }

                // update the y axis, handle collision
                if (dv.Y > 0)
                {
                    // test tile beneath us
                    var yTileBottomLeft = this.Context.WorldToTile(new Vector2(this.bounds.Left, this.bounds.Bottom - 1 + dv.Y));
                    var yTileBottomRight = this.Context.WorldToTile(new Vector2(this.bounds.Right - 1, this.bounds.Bottom - 1 + dv.Y));
                    if (!this.Context.Map.IsPassable(yTileBottomLeft) || !this.Context.Map.IsPassable(yTileBottomRight))
                    {
                        this.velocity.Y = 0;
                        this.bounds.Y = this.Context.TileToWorld(yTileBottomLeft).Y - this.bounds.Height;
                    }
                    else
                    {
                        this.bounds.Y += dv.Y;
                    }
                }
                else if (dv.Y < 0)
                {
                    // test tile above us
                    var yTileTopLeft = this.Context.WorldToTile(new Vector2(this.bounds.Left, this.bounds.Top + dv.Y));
                    var yTileTopRight = this.Context.WorldToTile(new Vector2(this.bounds.Right - 1, this.bounds.Top + dv.Y));
                    if (!this.Context.Map.IsPassable(yTileTopLeft) || !this.Context.Map.IsPassable(yTileTopRight))
                    {
                        this.velocity.Y = 0;// -this.velocity.Y;
                        this.bounds.Y = this.Context.TileToWorld(yTileTopLeft.X, yTileTopLeft.Y + 1).Y;
                    }
                    else
                    {
                        this.bounds.Y += dv.Y;
                    }
                }
            }

            var tileBottomLeft = this.Context.WorldToTile(new Vector2(this.bounds.Left, this.bounds.Bottom));
            var tileBottomRight = this.Context.WorldToTile(new Vector2(this.bounds.Right - 1, this.bounds.Bottom));
            if (!this.Context.Map.IsPassable(tileBottomLeft) || !this.Context.Map.IsPassable(tileBottomRight))
            {
                this.onGround = true;
            }
            else
            {
                this.onGround = false;
            }
        }
        
        public bool IsVisible()
        {
            return this.Context.VisibleBounds.Intersects(this.bounds);
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            this.frame += gameTime.GetElapsedSeconds() * this.Sprite.FPS;
            var intFrame = (int)Math.Floor(this.frame);
            if (intFrame >= this.Sprite.NumberOfFrames)
            {
                intFrame = 0;
                this.frame = 0;
            }

            if (this.IsVisible())
            {
                this.Sprite.DrawSprite(renderer.World, intFrame, this.Position, PlatformContext.ZToDepth(this.Z));

                if (AbstractObject.DebugInfo)
                {
                    renderer.World.DrawRectangle(this.bounds, Color.White);
                    renderer.World.DrawString(this.Context.Store.Fonts("Base", "debug"), $"{this.bounds}, {this.velocity}, {this.OnGround}", this.Position - new Vector2(0, 20), Color.White);
                }
            }

            base.Draw(renderer, gameTime);
        }
    }
}

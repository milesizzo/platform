using GameEngine.Content;
using GameEngine.Extensions;
using GameEngine.GameObjects;
using GameEngine.Graphics;
using GameEngine.Templates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public class VisiblePlatformObject : PlatformObject
    {
        public const float Gravity = 50f;
        public const float TerminalVelocity = 450f;
        public bool IsPhysicsEnabled = true;
        public bool IsGravityEnabled = true;
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
                if (!this.OnGround && this.IsGravityEnabled)
                {
                    // we only apply gravity if we weren't on the ground in the previous cycle
                    this.velocity.Y = MathHelper.Min(this.velocity.Y + Gravity, TerminalVelocity);
                }
                var dv = this.velocity * elapsed;

                // TODO: check when the magnitude of dv is > tilesize (we will need to check multiple tiles)

                // update the x axis, handle collision
                if (dv.X > 0)
                {
                    // test tile to the right
                    var xTileTopRight = this.Context.WorldToTile(new Vector2(this.bounds.Right - 1 + dv.X, this.bounds.Top));
                    var xTileBottomRight = this.Context.WorldToTile(new Vector2(this.bounds.Right - 1 + dv.X, this.bounds.Bottom - 1));
                    if (!this.Context.Map.IsPassable(xTileTopRight, xTileBottomRight))
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
                    if (!this.Context.Map.IsPassable(xTileTopLeft, xTileBottomLeft))
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
                    if (!this.Context.Map.IsPassable(yTileBottomLeft, yTileBottomRight))
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
                    if (!this.Context.Map.IsPassable(yTileTopLeft, yTileTopRight))
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
            if (!this.Context.Map.IsPassable(tileBottomLeft, tileBottomRight))
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
                var depth = PlatformContext.ZToDepth(this.Z);
                var colour = Color.White;
                var scale = Vector2.One;
                var offset = Vector2.Zero;
                if (this.Z > 0.5f)
                {
                    var alpha = (byte)((1f - (this.Z - 0.5f) * 2) * 255);
                    colour = new Color(alpha, alpha, alpha, alpha);
                    //scale = new Vector2(1f - (this.Z - 0.5f) * 2);
                    //offset = new Vector2((1f - scale.X) * this.sprite.Width / 2, (1f - scale.Y) * this.sprite.Height);
                }
                this.Sprite.DrawSprite(renderer.World, intFrame, this.Position + offset, colour, 0, scale, SpriteEffects.None, depth);

                if (AbstractObject.DebugInfo)
                {
                    renderer.World.DrawRectangle(this.bounds, Color.White);
                    renderer.World.DrawString(Store.Instance.Fonts("Base", "debug"), $"{this.bounds}, {this.velocity}, {this.OnGround}", this.Position - new Vector2(0, 20), Color.White);
                }
            }

            base.Draw(renderer, gameTime);
        }
    }
}

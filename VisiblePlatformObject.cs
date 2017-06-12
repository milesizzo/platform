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
        public const float Gravity = 2000f;
        public const float WaterGravity = 1000f;
        public const float TerminalVelocity = 450f;
        public const float TerminalWaterVelocity = 200f;
        public bool IsPhysicsEnabled = true;
        public bool IsGravityEnabled = true;
        private ISpriteTemplate sprite;
        private float frame;
        private Vector2 velocity;
        private bool onGround;
        private bool inWater;
        private bool onLadder;

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
                if (this.bounds == RectangleF.Empty)
                {
                    this.bounds = new RectangleF(this.Position, new Size2(value.Width, value.Height));
                }
            }
        }

        public bool OnGround { get { return this.onGround; } }

        public bool InWater { get { return this.inWater; } }

        public bool OnLadder { get { return this.onLadder; } }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.IsPhysicsEnabled)
            {
                var flags = this.Context.GetFlags(this.Context.WorldToTile(this.bounds.TopLeft), this.Context.WorldToTile(new Vector2(this.bounds.Right - 1, this.bounds.Bottom - 1)));
                if (!flags.HasFlag(TileFlags.Water) && this.InWater)
                {
                    this.velocity.Y *= 2;
                }
                this.inWater = flags.HasFlag(TileFlags.Water);
                this.onLadder = flags.HasFlag(TileFlags.Ladder);

                var elapsed = gameTime.GetElapsedSeconds();
                if (!this.OnGround && this.IsGravityEnabled)
                {
                    if (this.InWater)
                    {
                        this.velocity.Y = MathHelper.Min(this.velocity.Y + WaterGravity * elapsed, TerminalWaterVelocity);
                    }
                    else
                    {
                        // we only apply gravity if we weren't on the ground in the previous cycle
                        this.velocity.Y = MathHelper.Min(this.velocity.Y + Gravity * elapsed, TerminalVelocity);
                    }
                }
                var dv = this.velocity * elapsed;

                // TODO: check when the magnitude of dv is > tilesize (we will need to check multiple tiles)

                #region Collision detection with other tiles
                // update the x axis, handle collision
                if (dv.X > 0)
                {
                    // test tile to the right
                    var right = (float)Math.Ceiling(this.bounds.Right - 1 + dv.X);
                    var xTopRight = new Vector2(right, (float)Math.Floor(this.bounds.Top));
                    var xBottomRight = new Vector2(right, (float)Math.Ceiling(this.bounds.Bottom - 1));
                    var xTileTopRight = this.Context.WorldToTile(xTopRight);
                    var xTileBottomRight = this.Context.WorldToTile(xBottomRight);

                    if (!this.Context.IsPassable(xTileTopRight, xTileBottomRight))
                    {
                        // not passable, but are we on a slope?
                        var slope = this.Context.SlopeAmountRight(xBottomRight, 10);
                        if (slope < 10)
                        {
                            // we allow Y jumps of up to 4
                            this.bounds.Y -= MathHelper.Max(slope, 0);
                            this.bounds.X += dv.X;
                        }
                        else
                        {
                            this.velocity.X = 0;
                            this.bounds.X = this.Context.TileToWorld(xTileTopRight).X - this.bounds.Width;
                        }
                    }
                    else
                    {
                        this.bounds.X += dv.X;
                    }
                }
                else if (dv.X < 0)
                {
                    var left = (float)Math.Floor(this.bounds.Left + dv.X);
                    var xBottomLeft = new Vector2(left, (float)Math.Ceiling(this.bounds.Bottom - 1));
                    var xTileTopLeft = this.Context.WorldToTile(new Vector2(left, (float)Math.Floor(this.bounds.Top)));
                    var xTileBottomLeft = this.Context.WorldToTile(xBottomLeft);
                    if (!this.Context.IsPassable(xTileTopLeft, xTileBottomLeft))
                    {
                        // not passable, but are we on a slope?
                        var slope = this.Context.SlopeAmountLeft(xBottomLeft, 10);
                        if (slope < 10)
                        {
                            // we allow Y jumps of up to 4
                            this.bounds.Y -= MathHelper.Max(slope, 0);
                            this.bounds.X += dv.X;
                        }
                        else
                        {
                            this.velocity.X = 0;
                            this.bounds.X = this.Context.TileToWorld(xTileTopLeft.X + 1, xTileTopLeft.Y).X;
                        }
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
                    var bottom = (float)Math.Ceiling(this.bounds.Bottom - 1);
                    var bottomPlusDrop = (float)Math.Ceiling(this.bounds.Bottom - 1 + dv.Y);
                    var search = bottom;
                    while (search <= bottomPlusDrop)
                    {
                        var yBottomLeft = new Vector2((float)Math.Floor(this.bounds.Left), search);
                        var yBottomRight = new Vector2((float)Math.Ceiling(this.bounds.Right - 1), search);
                        var yTileBottomLeft = this.Context.WorldToTile(yBottomLeft);
                        var yTileBottomRight = this.Context.WorldToTile(yBottomRight);
                        if (!this.Context.IsPassable(yBottomLeft, yBottomRight))
                        {
                            this.velocity.Y = 0;
                            this.bounds.Y = search - this.bounds.Height;
                            break;
                        }
                        else if (this.Context.IsOneWayPlatform(yTileBottomLeft, yTileBottomRight))
                        {
                            this.velocity.Y = 0;
                            this.bounds.Y = search - this.bounds.Height;
                            break;
                        }
                        search++;
                    }
                    if (search > bottomPlusDrop)
                    {
                        // we didn't encounter an obstacle - apply Y velocity
                        this.bounds.Y += dv.Y;
                    }
                }
                else if (dv.Y < 0)
                {
                    // test tile above us
                    var top = (float)Math.Floor(this.bounds.Top);
                    var topPlusRaise = (float)Math.Floor(this.bounds.Top + dv.Y);
                    var search = top;
                    while (search >= topPlusRaise)
                    {
                        var yTopLeft = new Vector2((float)Math.Floor(this.bounds.Left), search);
                        var yTopRight = new Vector2((float)Math.Ceiling(this.bounds.Right - 1), search);
                        var yTileTopLeft = this.Context.WorldToTile(yTopLeft);
                        var yTileTopRight = this.Context.WorldToTile(yTopRight);
                        if (!this.Context.IsPassable(yTopLeft, yTopRight))
                        {
                            this.velocity.Y = 0;
                            this.bounds.Y = search + 1;
                            break;
                        }
                        search--;
                    }
                    if (search < topPlusRaise)
                    {
                        // we didn't encounter an obstacle - apply Y velocity
                        this.bounds.Y += dv.Y;
                    }
                }
                #endregion
            }

            var bottomLeft = new Vector2(this.bounds.Left, this.bounds.Bottom);
            var bottomRight = new Vector2((float)Math.Floor(this.bounds.Right), this.bounds.Bottom);
            var tileBottomLeft = this.Context.WorldToTile(bottomLeft);
            var tileBottomRight = this.Context.WorldToTile(bottomRight);
            if (!this.Context.IsPassable(bottomLeft, bottomRight) || this.Context.IsOneWayPlatform(tileBottomLeft, tileBottomRight))
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

        protected virtual StringBuilder DebugData()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"x = {this.bounds}");
            sb.AppendLine($"v = {this.velocity}");
            if (this.OnGround) sb.Append("OnGround ");
            if (this.OnLadder) sb.Append("OnLadder ");
            if (this.InWater) sb.Append("InWater ");
            sb.AppendLine();
            return sb;
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
                if (this.Z < 0.5f)
                {
                    var alpha = (byte)((1f - (0.5f - this.Z) * 2) * 255);
                    colour = new Color(alpha, alpha, alpha, alpha);
                    //scale = new Vector2(1f - (this.Z - 0.5f) * 2);
                    //offset = new Vector2((1f - scale.X) * this.sprite.Width / 2, (1f - scale.Y) * this.sprite.Height);
                }
                this.Sprite.DrawSprite(renderer.World, intFrame, this.Position + offset, colour, 0, scale, SpriteEffects.None, depth);

                if (AbstractObject.DebugInfo)
                {
                    renderer.World.DrawRectangle(this.bounds, Color.White);

                    var font = Store.Instance.Fonts("Base", "debug");
                    var sb = this.DebugData().ToString();
                    var size = font.Font.MeasureString(sb);
                    renderer.Screen.DrawString(font, sb, this.Context.WorldToScreen(this.Position) - new Vector2(0, size.Y), Color.White);
                    //renderer.World.DrawString(Store.Instance.Fonts("Base", "debug"), $"{this.bounds}, {this.velocity}, {this.OnGround}", this.Position - new Vector2(0, 20), Color.White);
                }
            }

            base.Draw(renderer, gameTime);
        }
    }
}

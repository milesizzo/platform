using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccidentalNoise;

namespace Platform
{
    public class TerrainGenerator
    {
        private readonly TileMap map;
        private readonly float halfWidth;
        private readonly float halfHeight;
        private readonly Random random;

        public TerrainGenerator(TileMap map)
        {
            this.map = map;
            this.halfWidth = this.map.Width / 2f;
            this.halfHeight = this.map.Height / 2f;
            this.random = new Random();
        }

        private static double IntegerNoise(int n)
        {
            n = (n >> 13) ^ n;
            int nn = (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
            return 1.0 - ((double)nn / 1073741824.0);
        }

        private static double CoherentNoise(double x)
        {
            var intX = (int)Math.Floor(x);
            var n0 = IntegerNoise(intX);
            var n1 = IntegerNoise(intX + 1);
            var weight = x - Math.Floor(x);
            //var noise = MathHelper.Lerp((float)n0, (float)n1, (float)weight);
            var noise = MathHelper.SmoothStep((float)n0, (float)n1, (float)weight);
            return noise;
        }

        private Vector2 Transform(Point tilePos)
        {
            // convert to range [-1, 1)
            //return new Vector2((tilePos.X - this.halfWidth) / this.halfWidth, (tilePos.Y - this.halfHeight) / this.halfHeight);

            // convert to range [0, 1)
            return new Vector2(tilePos.X * (float)this.map.TileSize, tilePos.Y * (float)this.map.TileSize);
        }

        /*private float Noise(IModule2D noise, float x)
        {
            return noise.GetValue(x, 0) / 2f + 0.5f;
        }*/

        /*public float Turbulence(int y, int x)
        {
            return (float)(this.Gradient(y, x) + this.random.NextDouble() * 0.25d - 0.25d);
        }*/

        /*private class Gradient : IModule2D
        {
            public Gradient()
            {
            }

            public float GetValue(float x, float y)
            {
                return y * 2 - 1f;
            }
        }*/

        public IEnumerable<Point> Generate()
        {
            /*var fractal = new MultiFractal();
            fractal.OctaveCount = 6f;
            fractal.Frequency = 2f;
            fractal.Primitive2D = new Gradient();*/
            var gradient = new Gradient(0, 0, 0, 1);
            var fractal = new Fractal(FractalType.FBM, BasisTypes.GRADIENT, InterpTypes.QUINTIC, octaves: 6, frequency: 2d, seed: null);
            var scale = new ScaleOffset(0.5d, 0, fractal);
            var perturb = new TranslatedDomain(gradient, null, scale);
            var select = new Select(perturb, 0, 1, 0.5d, null);

            for (var y = 0; y < this.map.Height; y++)
            {
                for (var x = 0; x < this.map.Width; x++)
                {
                    var value = select.Get((double)x / (this.map.Width / 2), (double)y / (this.map.Height / 2));
                    if (value > 0.5)
                    {
                        this.map[y, x].Block = new Material { Type = MaterialType.Dirt };
                    }
                }
            }
            yield break;
        }

        public IEnumerable<Point> Generate2()
        {
            /*
            var random = new Random();
            var gen1 = new SimplexPerlin(random.Next(), NoiseQuality.Best);
            //var gen1 = new ImprovedPerlin(random.Next(), NoiseQuality.Best);
            
            var flatCount = 0;
            int prevHeight = 0;
            var heightScale = 100f;
            var t = 0f;
            for (var x = 0; x < this.map.Width; x++)
            {
                var n = this.Noise(gen1, t) * heightScale;
                var height = MathHelper.Clamp((int)n + 300, 0, this.map.Height - 1);
                for (var y = height; y < this.map.Height; y++)
                {
                    var type = y > 400 ? MaterialType.Water : MaterialType.Dirt;
                    this.map[y, x].Block = new Material { Type = type };
                }
                t += 0.02f;
            }
            */
            yield break;
        }
    }
}

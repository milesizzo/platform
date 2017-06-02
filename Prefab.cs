using GameEngine.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public class Prefab<T>
    {
        private readonly Func<T> generator;

        public Prefab(Func<T> generator)
        {
            this.generator = generator;
        }

        public T Produce()
        {
            return this.generator();
        }
    }

    public class VisibleObjectPrefab : Prefab<VisiblePlatformObject>
    {
        private readonly ISpriteTemplate sprite;
        
        public VisibleObjectPrefab(PlatformContext context, ISpriteTemplate sprite) : base(() => Generate(context, sprite))
        {
            this.sprite = sprite;
        }

        private static VisiblePlatformObject Generate(PlatformContext context, ISpriteTemplate sprite)
        {
            var obj = new VisiblePlatformObject(context);
            obj.Sprite = sprite;
            context.AddObject(obj);
            return obj;
        }

        public ISpriteTemplate Sprite
        {
            get { return this.sprite; }
        }
    }
}

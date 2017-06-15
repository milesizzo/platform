using GameEngine.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public abstract class Prefab<T>
    {
        public Prefab()
        {
        }

        public abstract T Produce();
    }

    public class VisibleObjectPrefab : Prefab<VisiblePlatformObject>
    {
        private readonly PlatformContext context;
        private readonly ISpriteTemplate sprite;
        
        public VisibleObjectPrefab(PlatformContext context, ISpriteTemplate sprite)
        {
            this.context = context;
            this.sprite = sprite;
        }

        public override VisiblePlatformObject Produce()
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

    public class LightPrefab : Prefab<Light>
    {
        private readonly Light light;

        public LightPrefab(Light light)
        {
            this.light = light;
        }

        public override Light Produce()
        {
            return this.light;
        }
    }
}

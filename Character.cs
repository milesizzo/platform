using GameEngine.Templates;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public class Character : ITemplate
    {
        private readonly string name;

        public RectangleF Bounds;
        public float JumpPower;
        public float WalkSpeed;
        public float RunSpeed;
        public float ClimbSpeed;
        public float WalkMaxSpeed;
        public float RunMaxSpeed;
        public float ClimbMaxSpeed;
        public float WaterModifier;
        public float SwimPower;
        public NamedAnimatedSpriteSheetTemplate Sprite;

        public Character(string name)
        {
            this.name = name;
        }

        public string Name { get { return this.name; } }
    }

    public class CharacterStore : TemplateStore<Character>
    {
        //
    }
}

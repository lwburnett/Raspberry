using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class PrototypeCharacterComponent : Component//, IUpdatable
    {
        public override void OnAddedToEntity()
        {
            var texture = Entity.Scene.Content.LoadTexture(@"Characters/PrototypeCharacter");
            var sprites = Sprite.SpritesFromAtlas(texture, 16, 24);

            //_mover = Entity.AddComponent(new Mover())
            _animator = Entity.AddComponent<SpriteAnimator>();

            _animator.AddAnimation("Idle", new[] { sprites[0], sprites[1], sprites[2], sprites[3] });
            _animator.Play("Idle", SpriteAnimator.LoopMode.PingPong);
        }

        // public void Update()
        // {
        //
        // }

        private SpriteAnimator _animator;
    }
}

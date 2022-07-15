using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class RockObstacleEntity : Entity
    {
        public RockObstacleEntity(Vector2 iPosition)
        {
            Position = iPosition;
        }


        public int PhysicsLayer = 1;

        public override void OnAddedToScene()
        {
            var textureAtlas = Scene.Content.LoadTexture(Content.Content.LevelTileset);
            var spriteList = Sprite.SpritesFromAtlas(textureAtlas, 32, 32);
            var texture = spriteList[0];

            _renderer = AddComponent<SpriteRenderer>();
            _renderer.RenderLayer = 4;
            _renderer.Sprite = texture;

            _collider = AddComponent(new CircleCollider(texture.SourceRect.Width / 2f) {PhysicsLayer = PhysicsLayer, Entity = this});
            Physics.AddCollider(_collider);

            AddComponent(new WakeParticleEmitter(() => Vector2.Zero){RenderLayer = 4});

#if VERBOSE
            Verbose.RenderCollider(_collider);
#endif
        }

        public override void OnRemovedFromScene()
        {
            Physics.RemoveCollider(_collider);
        }

        private SpriteRenderer _renderer;
        private Collider _collider;
    }
}

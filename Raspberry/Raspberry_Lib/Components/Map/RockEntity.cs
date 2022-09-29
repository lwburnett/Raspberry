using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class RockObstacleEntity : Entity
    {
        private static class Settings
        {
            public static readonly RenderSetting DoNotUpdateWakeRightDistance = new(1000);
            public static readonly RenderSetting DoNotUpdateWakeLeftDistance = new(2000);
        }

        public RockObstacleEntity(Vector2 iPosition)
        {
            Position = iPosition;
        }


        public int PhysicsLayer = 1;

        public override void OnAddedToScene()
        {
            var textureAtlas = Scene.Content.LoadTexture(Content.ContentData.AssetPaths.ObjectsTileset, true);
            var spriteList = Sprite.SpritesFromAtlas(textureAtlas, 32, 32);
            var texture = spriteList[0];

            _renderer = AddComponent<SpriteRenderer>();
            _renderer.RenderLayer = 4;
            _renderer.Sprite = texture;

            _collider = AddComponent(new CircleCollider(texture.SourceRect.Width / 2f) {PhysicsLayer = PhysicsLayer, Entity = this});
            Physics.AddCollider(_collider);

            _character = Scene.FindEntity("character");

            _rightUpdateWakeBoundX = Position.X + Settings.DoNotUpdateWakeRightDistance.Value;
            _leftUpdateWakeBoundX = Position.X - Settings.DoNotUpdateWakeLeftDistance.Value;

            AddComponent(new WakeParticleEmitter(ShouldUpdate) {RenderLayer = 4});

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
        private Entity _character;
        private float _rightUpdateWakeBoundX;
        private float _leftUpdateWakeBoundX;

        private bool ShouldUpdate()
        {
            return _character.Position.X > _leftUpdateWakeBoundX && _character.Position.X < _rightUpdateWakeBoundX;
        }
    }
}

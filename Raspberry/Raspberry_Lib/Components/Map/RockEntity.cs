using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class RockObstacleEntity : Entity
    {
        private static class Settings
        {
            public static readonly RenderSetting DoNotUpdateWakeRightDistance = new(1000);
            public static readonly RenderSetting DoNotUpdateWakeLeftDistance = new(2000);

            public const float ScaleAdjustmentMultiplier = .75f;
        }

        public RockObstacleEntity(Vector2 iPosition)
        {
            Position = iPosition;
        }


        public int PhysicsLayer = 1;

        public override void OnAddedToScene()
        {
            Scale *= Settings.ScaleAdjustmentMultiplier;

            var textureAtlas = Scene.Content.LoadTexture(Content.ContentData.AssetPaths.ObjectsTileset, true);
            var spriteList = Sprite.SpritesFromAtlas(textureAtlas, 72, 72);
            var textureOutside = spriteList[0];
            var textureInside = spriteList[1];

            _character = Scene.FindEntity("character");
            var playerProximityComponent = _character.GetComponent<PlayerProximityComponent>();

            AddComponent(
                new ProximitySpriteRenderer(
                    textureInside,
                    textureOutside, 
                    () => _character.Position, 
                    () => playerProximityComponent.Radius)
                {
                    RenderLayer = 4
                });

            _collider = AddComponent(new CircleCollider(textureOutside.SourceRect.Width / 3f) {PhysicsLayer = PhysicsLayer, Entity = this});
            Physics.AddCollider(_collider);


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

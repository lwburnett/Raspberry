using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class EnergyEntity : Entity
    {
        private static class Settings
        {
            public const float ScaleAdjustmentMultiplier = .75f;
            public static readonly RenderSetting ColliderRadius = new(30);
        }

        public EnergyEntity(Vector2 iPosition)
        {
            Position = iPosition;
        }

        public int PhysicsLayer = 1;

        public override void OnAddedToScene()
        {
            Scale *= Settings.ScaleAdjustmentMultiplier;

            _animationComponent = AddComponent(new EnergyAnimationComponent());

            _collider = AddComponent(new CircleCollider(Settings.ColliderRadius.Value) { PhysicsLayer = PhysicsLayer, Entity = this });
            Physics.AddCollider(_collider);

#if VERBOSE
            Verbose.RenderCollider(_collider);
#endif
        }

        public override void OnRemovedFromScene()
        {
            Physics.RemoveCollider(_collider);
        }

        public void OnPlayerHit()
        {
            Physics.RemoveCollider(_collider);
            _animationComponent.Enabled = false;
        }

        private EnergyAnimationComponent _animationComponent;
        private Collider _collider;
    }
}

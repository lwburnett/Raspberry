using Nez;

namespace Raspberry_Lib.Components
{
    internal class CharacterCollisionComponent : Component
    {
        public CharacterCollisionComponent(System.Action iOnFatalCollision)
        {
            _onFatalCollision = iOnFatalCollision;
            _collider = new BoxCollider(24, 12);
        }

        public override void OnAddedToEntity()
        {
            Entity.AddComponent(_collider);
        }

        private readonly BoxCollider _collider;
        private readonly System.Action _onFatalCollision;

        public void HandleCollision(CollisionResult collisionResult)
        {
            if (collisionResult.Collider != null)
                _onFatalCollision();
        }
    }
}
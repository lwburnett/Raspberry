using Nez;

namespace Raspberry_Lib.Components
{
    internal class PrototypeCharacterComponent : Component
    {
        public PrototypeCharacterComponent(System.Action iOnMainMenu)
        {
            _collisionComponent = new CharacterCollisionComponent(iOnMainMenu);
            _playerProximityComponent = new PlayerProximityComponent(iOnMainMenu);
        }

        public override void OnAddedToEntity()
        {
            _movementComponent = Entity.AddComponent(new CharacterMovementComponent());
            Entity.AddComponent<CharacterAnimationComponent>();
            Entity.AddComponent(new CharacterInputController(OnPlayerInput));
            Entity.AddComponent(_collisionComponent);
            Entity.AddComponent(new WakeParticleEmitter(() => _movementComponent.CurrentVelocity, () => true, true){RenderLayer = 5});
            Entity.AddComponent(_playerProximityComponent);
            Entity.AddComponent(new OarPairComponent());
        }
        
        private CharacterMovementComponent _movementComponent;
        private readonly CharacterCollisionComponent _collisionComponent;
        private readonly PlayerProximityComponent _playerProximityComponent;

        private void OnPlayerInput(CharacterInputController.InputDescription iInputDescription)
        {
            _movementComponent.OnPlayerInput(iInputDescription);
        }
    }
}

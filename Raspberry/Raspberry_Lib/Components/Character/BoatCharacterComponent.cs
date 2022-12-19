using Nez;

namespace Raspberry_Lib.Components
{
    internal class BoatCharacterComponent : Component
    {
        public BoatCharacterComponent(System.Action iOnMainMenu)
        {
            _collisionComponent = new CharacterCollisionComponent();
            _playerProximityComponent = new PlayerProximityComponent(iOnMainMenu);
            _inputController = new CharacterInputController(OnPlayerInput);
        }

        public override void OnAddedToEntity()
        {
            _movementComponent = Entity.AddComponent(new CharacterMovementComponent());
            Entity.AddComponent<CharacterAnimationComponent>();
            Entity.AddComponent(_inputController);
            Entity.AddComponent(_collisionComponent);
            Entity.AddComponent(new WakeParticleEmitter(() => _movementComponent.CurrentVelocity, () => true, true){RenderLayer = 5});
            Entity.AddComponent(_playerProximityComponent);
            Entity.AddComponent(new OarPairComponent());
        }

        public void TogglePause(bool iIsPaused)
        {
            _movementComponent.IsPaused = iIsPaused;
            _playerProximityComponent.IsPaused = iIsPaused;
            _inputController.IsPaused = iIsPaused;
        }

        private CharacterMovementComponent _movementComponent;
        private readonly CharacterCollisionComponent _collisionComponent;
        private readonly PlayerProximityComponent _playerProximityComponent;
        private readonly CharacterInputController _inputController;

        private void OnPlayerInput(CharacterInputController.InputDescription iInputDescription)
        {
            _movementComponent.OnPlayerInput(iInputDescription);
        }
    }
}

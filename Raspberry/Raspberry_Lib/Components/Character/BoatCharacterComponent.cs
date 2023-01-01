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
            _animationComponent = new CharacterAnimationComponent();
            _wakeEmitter = new WakeParticleEmitter(() => 
                    _movementComponent.CurrentVelocity, () => true, true) { RenderLayer = 5 };
            _oarComponent = new OarPairComponent();
        }

        public override void OnAddedToEntity()
        {
            _movementComponent = Entity.AddComponent(new CharacterMovementComponent());
            Entity.AddComponent(_animationComponent);
            Entity.AddComponent(_inputController);
            Entity.AddComponent(_collisionComponent);
            Entity.AddComponent(_wakeEmitter);
            Entity.AddComponent(_playerProximityComponent);
            Entity.AddComponent(_oarComponent);
        }

        public void TogglePause(bool iIsPaused)
        {
            _movementComponent.IsPaused = iIsPaused;
            _playerProximityComponent.IsPaused = iIsPaused;
            _inputController.IsPaused = iIsPaused;
            _animationComponent.IsPaused = iIsPaused;
            _wakeEmitter.IsPaused = iIsPaused;
            _oarComponent.IsPaused = iIsPaused;
        }

        private CharacterMovementComponent _movementComponent;
        private readonly CharacterCollisionComponent _collisionComponent;
        private readonly PlayerProximityComponent _playerProximityComponent;
        private readonly CharacterInputController _inputController;
        private readonly CharacterAnimationComponent _animationComponent;
        private readonly WakeParticleEmitter _wakeEmitter;
        private readonly OarPairComponent _oarComponent;

        private void OnPlayerInput(CharacterInputController.InputDescription iInputDescription)
        {
            _movementComponent.OnPlayerInput(iInputDescription);
        }
    }
}

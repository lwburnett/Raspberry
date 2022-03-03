using Nez;

namespace Raspberry_Lib.Components
{
    internal class PrototypeCharacterComponent : Component
    {
        public enum State
        {
            Idle,
            RunLeft,
            RunRight
        }


        public override void OnAddedToEntity()
        {
            _animationComponent = Entity.AddComponent<CharacterAnimationComponent>();
            _movementComponent = Entity.AddComponent(new CharacterMovementComponent(OnCharacterStateChanged));
            Entity.AddComponent(new CharacterInputController(OnPlayerInput));
        }

        private CharacterAnimationComponent _animationComponent;
        private CharacterMovementComponent _movementComponent;

        private void OnPlayerInput(CharacterInputController.InputAction iInput)
        {
            _movementComponent.OnPlayerInput(iInput);
        }

        private void OnCharacterStateChanged(State iNewState)
        {
            _animationComponent.SetState(iNewState);
        }
    }
}

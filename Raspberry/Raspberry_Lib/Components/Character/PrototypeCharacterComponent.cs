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
            _animationComponent = Entity.AddComponent(new CharacterAnimationComponent());
            Entity.AddComponent(new CharacterInputController(OnCharacterStateChanged));
        }

        private CharacterAnimationComponent _animationComponent;

        private void OnCharacterStateChanged(State iNewState)
        {
            _animationComponent.SetState(iNewState);
        }
    }
}

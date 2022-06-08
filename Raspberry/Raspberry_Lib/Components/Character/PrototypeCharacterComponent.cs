using Nez;

namespace Raspberry_Lib.Components
{
    internal class PrototypeCharacterComponent :
#if DEBUG
    RenderableComponent
#else
        Component
#endif
    {
        private class Settings
        {
#if DEBUG
            public const bool DebugRenderHitBox = false;
#endif
        }

        public enum State
        {
            Idle,
            WalkLeft,
            WalkRight,
            RunLeft,
            RunRight
        }


        public override void OnAddedToEntity()
        {
            _animationComponent = Entity.AddComponent<CharacterAnimationComponent>();
            _movementComponent = Entity.AddComponent(new CharacterMovementComponent(OnCharacterStateChanged));
            Entity.AddComponent(new CharacterInputController(OnPlayerInput));
            Entity.AddComponent(new BoxCollider(24, 12));
        }

#if DEBUG
        public override float Width => 1000;
        public override float Height => 1000;

        public override void Render(Batcher batcher, Camera camera)
        {
            if (Settings.DebugRenderHitBox)
            {
                var collider = Entity.GetComponent<Collider>();
                collider?.DebugRender(batcher);
            }
        }
#endif

        private CharacterAnimationComponent _animationComponent;
        private CharacterMovementComponent _movementComponent;

        private void OnPlayerInput(CharacterInputController.InputDescription iInputDescription)
        {
            _movementComponent.OnPlayerInput(iInputDescription);
        }

        private void OnCharacterStateChanged(State iNewState)
        {
            _animationComponent.SetState(iNewState);
        }
    }
}

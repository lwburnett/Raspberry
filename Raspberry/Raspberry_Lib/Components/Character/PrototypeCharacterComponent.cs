﻿using Nez;

namespace Raspberry_Lib.Components
{
    internal class PrototypeCharacterComponent :
#if VERBOSE
        RenderableComponent
#else
        Component
#endif
    {
        public PrototypeCharacterComponent(System.Action iOnFatalCollision)
        {
            _collisionComponent = new CharacterCollisionComponent(iOnFatalCollision);
        }

        public enum State
        {
            Idle,
            TurnCw,
            TurnCcw,
            Row
        }


        public override void OnAddedToEntity()
        {
            _animationComponent = Entity.AddComponent<CharacterAnimationComponent>();
            _movementComponent = Entity.AddComponent(new CharacterMovementComponent(OnCharacterStateChanged));
            Entity.AddComponent(new CharacterInputController(OnPlayerInput));
            Entity.AddComponent(_collisionComponent);
        }

#if VERBOSE
        public override float Width => 1000;
        public override float Height => 1000;

        public override void Render(Batcher batcher, Camera camera)
        {
            var collider = Entity.GetComponent<Collider>();
            collider?.DebugRender(batcher);
        }
#endif

        private CharacterAnimationComponent _animationComponent;
        private CharacterMovementComponent _movementComponent;
        private readonly CharacterCollisionComponent _collisionComponent;

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

﻿using System;

namespace Raspberry_Lib.Components
{
    internal class BoatCharacterComponent : PausableComponent
    {
        public BoatCharacterComponent(Action iOnMainMenu)
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

            if (SettingsManager.GetGameSettings().Sfx)
                Entity.AddComponent<BoatAudioComponent>();
        }

        private CharacterMovementComponent _movementComponent;
        private readonly CharacterCollisionComponent _collisionComponent;
        private readonly PlayerProximityComponent _playerProximityComponent;
        private readonly CharacterInputController _inputController;
        private readonly CharacterAnimationComponent _animationComponent;
        private readonly WakeParticleEmitter _wakeEmitter;
        private readonly OarPairComponent _oarComponent;

        protected override void OnPauseSet(bool iVal)
        {
            _movementComponent.IsPaused = iVal;
            _playerProximityComponent.IsPaused = iVal;
            _inputController.IsPaused = iVal;
            _animationComponent.IsPaused = iVal;
            _wakeEmitter.IsPaused = iVal;
            _oarComponent.IsPaused = iVal;
        }

        private void OnPlayerInput(CharacterInputController.InputDescription iInputDescription)
        {
            _movementComponent.OnPlayerInput(iInputDescription);
        }
    }
}

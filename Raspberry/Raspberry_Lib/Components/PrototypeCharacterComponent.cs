using System;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Textures;

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

    internal class CharacterAnimationComponent : Component
    {
        public override void OnAddedToEntity()
        {
            var texture = Entity.Scene.Content.LoadTexture(@"Characters/PrototypeCharacter");
            var sprites = Sprite.SpritesFromAtlas(texture, 24, 24);

            _animator = Entity.AddComponent<SpriteAnimator>();

            _animator.AddAnimation(PrototypeCharacterComponent.State.Idle.ToString(), new[] { sprites[0], sprites[1], sprites[2], sprites[3] });
            _animator.AddAnimation(PrototypeCharacterComponent.State.RunLeft.ToString(), new []{sprites[5]});
            _animator.AddAnimation(PrototypeCharacterComponent.State.RunRight.ToString(), new []{sprites[4]});

            _currentState = PrototypeCharacterComponent.State.Idle;
            _animator.Play(PrototypeCharacterComponent.State.Idle.ToString(), SpriteAnimator.LoopMode.PingPong);
        }

        public void SetState(PrototypeCharacterComponent.State iNewState)
        {
            if (_currentState == iNewState)
                return;

            _currentState = iNewState;
            switch (iNewState)
            {
                case PrototypeCharacterComponent.State.Idle:
                    _animator.Play(PrototypeCharacterComponent.State.Idle.ToString(), SpriteAnimator.LoopMode.PingPong);
                    break;
                case PrototypeCharacterComponent.State.RunLeft:
                    _animator.Play(PrototypeCharacterComponent.State.RunLeft.ToString(), SpriteAnimator.LoopMode.ClampForever);
                    break;
                case PrototypeCharacterComponent.State.RunRight:
                    _animator.Play(PrototypeCharacterComponent.State.RunRight.ToString(), SpriteAnimator.LoopMode.ClampForever);
                    break;
                default:
                    System.Diagnostics.Debug.Fail($"Unknown value of enum {typeof(PrototypeCharacterComponent.State)}: {iNewState}");
                    throw new ArgumentOutOfRangeException(nameof(iNewState), iNewState, null);
            }
        }

        private PrototypeCharacterComponent.State _currentState;
        private SpriteAnimator _animator;
    }

    internal class CharacterInputController: Component, IUpdatable
    {
        public CharacterInputController(Action<PrototypeCharacterComponent.State> iOnStateChangedCallback)
        {
            _onStateChangedCallback = iOnStateChangedCallback;
        }

        public override void OnAddedToEntity()
        {
            _xAxisInput = new VirtualIntegerAxis(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right));
        }

        public void Update()
        {
            var inputX = _xAxisInput.Value;

            if (inputX == 0)
                _onStateChangedCallback(PrototypeCharacterComponent.State.Idle);
            else if (inputX > 0)
                _onStateChangedCallback(PrototypeCharacterComponent.State.RunRight);
            else
                _onStateChangedCallback(PrototypeCharacterComponent.State.RunLeft);
        }

        private VirtualIntegerAxis _xAxisInput;
        private readonly Action<PrototypeCharacterComponent.State> _onStateChangedCallback;
    }
}

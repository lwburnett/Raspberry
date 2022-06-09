using System;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class CharacterAnimationComponent : Component
    {
        public override void OnAddedToEntity()
        {
            var texture = Entity.Scene.Content.LoadTexture(@"Characters/PrototypeCharacter");
            var sprites = Sprite.SpritesFromAtlas(texture, 24, 24);

            _animator = Entity.AddComponent<SpriteAnimator>();

            _animator.AddAnimation(PrototypeCharacterComponent.State.Idle.ToString(), new[] { sprites[0] });
            _animator.AddAnimation(PrototypeCharacterComponent.State.Row.ToString(), new[] { sprites[1], sprites[2], sprites[3], sprites[0] });
            _animator.AddAnimation(PrototypeCharacterComponent.State.TurnCcw.ToString(), new []{ sprites[4] });
            _animator.AddAnimation(PrototypeCharacterComponent.State.TurnCw.ToString(), new []{sprites[5]});

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
                    _animator.Play(PrototypeCharacterComponent.State.Idle.ToString(), SpriteAnimator.LoopMode.ClampForever);
                    break;
                case PrototypeCharacterComponent.State.Row:
                    _animator.Play(PrototypeCharacterComponent.State.Row.ToString(), SpriteAnimator.LoopMode.ClampForever);
                    break;

                case PrototypeCharacterComponent.State.TurnCcw:
                    _animator.Play(PrototypeCharacterComponent.State.TurnCcw.ToString(), SpriteAnimator.LoopMode.ClampForever);
                    break;
                case PrototypeCharacterComponent.State.TurnCw:
                    _animator.Play(PrototypeCharacterComponent.State.TurnCw.ToString(), SpriteAnimator.LoopMode.ClampForever);
                    break;
                default:
                    System.Diagnostics.Debug.Fail($"Unknown value of enum {typeof(PrototypeCharacterComponent.State)}: {iNewState}");
                    throw new ArgumentOutOfRangeException(nameof(iNewState), iNewState, null);
            }
        }

        private PrototypeCharacterComponent.State _currentState;
        private SpriteAnimator _animator;
    }
}
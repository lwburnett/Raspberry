namespace Raspberry_Lib.Components
{
    internal class TutorialUiCanvasComponent : PlayUiCanvasComponent
    {
        public enum State
        {
            Welcome = 0,
            EndPlay
        }

        public TutorialUiCanvasComponent()
        {
            _currentState = 0;
        }

        private State _currentState;
        private PlayerProximityComponent _characterProximity;
        private CharacterInputController _characterInputController;
        private TutorialNavigationInputController _navigationInputController;

        protected sealed override void OnAddedToEntityInternal()
        {
            base.OnAddedToEntityInternal();
            
            _navigationInputController = Entity.AddComponent(new TutorialNavigationInputController(OnNavigation));
        }

        protected sealed override void BeginPlayInternal()
        {
            base.BeginPlayInternal();

            var characterEntity = Entity.Scene.FindEntity("character");
            _characterProximity = characterEntity.GetComponent<PlayerProximityComponent>();
            _characterInputController = characterEntity.GetComponent<CharacterInputController>();

            System.Diagnostics.Debug.Assert(_characterProximity != null);

            OnNavigationChanged();
        }

        // protected sealed override void UpdateInternal()
        // {
        //     base.UpdateInternal();
        //
        //
        // }

        private void OnNavigation()
        {
            _currentState++;
            OnNavigationChanged();
        }

        private void OnNavigationChanged()
        {
            void SetPauseStuff(bool iValue)
            {
                MovementComponent.IsPaused = iValue;
                _characterProximity.IsPaused = iValue;
                _characterInputController.IsPaused = iValue;
                _navigationInputController.IsPaused = !iValue;
            }

            switch (_currentState)
            {
                case State.Welcome:
                    SetPauseStuff(true);
                    break;
                case State.EndPlay:
                    SetPauseStuff(false);
                    break;
                default:
                    System.Diagnostics.Debug.Fail($"Unknown type of {nameof(State)}");
                    return;
            }
        }
    }
}

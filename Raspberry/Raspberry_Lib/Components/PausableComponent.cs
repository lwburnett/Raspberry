using Nez;

namespace Raspberry_Lib.Components
{
    internal abstract class PausableComponent : Component, IUpdatable
    {
        public void Update()
        {
            if (IsPaused)
            {
                TimeSpentPaused += Time.DeltaTime;
                return;
            }

            OnUpdate(Time.TotalTime - TimeSpentPaused);
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused == value)
                    return;

                _isPaused = value;
                OnPauseSet(_isPaused);
            }
        }
        private bool _isPaused;

        protected virtual void OnUpdate(float iTotalPlayableTime) { }

        protected virtual void OnPauseSet(bool iVal) { }

        protected float TimeSpentPaused { get; private set; }
    }
}

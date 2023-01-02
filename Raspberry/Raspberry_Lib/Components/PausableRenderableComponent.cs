using Nez;

namespace Raspberry_Lib.Components
{
    internal abstract class PausableRenderableComponent : RenderableComponent, IUpdatable
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

        public bool IsPaused { get; set; }

        protected abstract void OnUpdate(float iTotalPlayableTime);

        protected float TimeSpentPaused { get; private set; }
    }
}

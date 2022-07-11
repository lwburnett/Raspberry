using System.Collections.Generic;
using System.Linq;
using Nez;
using Raspberry_Lib.Components;

namespace Raspberry_Lib.Scenes
{
    internal class SceneBase : Scene
    {
        public SceneBase()
        {
            AddRenderer(new DefaultRenderer());
            AddRenderer(new ScreenSpaceRenderer(2, -1));
            _isFirstUpdate = true;
        }

        public override void Update()
        {
            base.Update();

            if (_isFirstUpdate)
            {
                OnBeginPlay();
                _isFirstUpdate = false;
            }
        }

        private bool _isFirstUpdate;

        public void OnBeginPlay()
        {
            for (var ii = 0; ii < Entities.Count; ii++)
            {
                var thisEntity = Entities[ii];
                thisEntity.Update();

                var startableComponents = new List<IBeginPlay>();
                for (var jj = 0; jj < thisEntity.Components.Count; jj++)
                {
                    var thisComponent = thisEntity.Components[jj];

                    if (thisComponent is IBeginPlay startableComponent)
                        startableComponents.Add(startableComponent);
                }

                foreach (var startableComponent in startableComponents.OrderBy(s => s.BeginPlayOrder))
                {
                    startableComponent.OnBeginPlay();
                }
            }
        }
    }
}
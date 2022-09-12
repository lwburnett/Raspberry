using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Nez;
using Nez.Tweens;
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

        public override void OnStart()
        {
            if (BackgroundSong != null)
            {
                MediaPlayer.Play(BackgroundSong);
                MediaPlayer.IsRepeating = true;
            }
        }

        public override void End()
        {
            MediaPlayer.Stop();
        }

        protected Song BackgroundSong;
        private bool _isFirstUpdate;
    }
}
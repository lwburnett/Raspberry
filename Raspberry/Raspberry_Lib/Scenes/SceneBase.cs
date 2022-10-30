using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Media;
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
            if (_backgroundSong != null)
            {
                MediaPlayer.Play(_backgroundSong);
                MediaPlayer.IsRepeating = true;
            }
        }

        public override void End()
        {
            MediaPlayer.Stop();
        }

        protected void SetBackgroundSong(string iPath)
        {
            var uri = new Uri(iPath, UriKind.Relative);
            // Need to make the first argument iPath because Android is bugged and
            //    uses that instead of the Uri to load the asset.
            // Reference found here: https://github.com/MonoGame/MonoGame/issues/3935
            //_backgroundSong = Song.FromUri(iPath, uri);
        }

        private Song _backgroundSong;
        private bool _isFirstUpdate;
    }
}
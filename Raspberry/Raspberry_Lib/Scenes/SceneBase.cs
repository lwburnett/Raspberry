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
            PlayBackgroundSong();

            base.OnStart();
        }

        public override void End()
        {
            StopBackgroundSong();

            if (_backgroundSong.HasValue)
                AudioManager.Unload(_backgroundSong.Value);

            base.End();
        }

        public void PlayBackgroundSong()
        {
            if (SettingsManager.GetGameSettings().Music && _backgroundSong.HasValue)
            {
                AudioManager.PlaySound(_backgroundSong.Value, true, _backgroundSongVolume, SoundStrategy.Overwrite);
            }
        }

        public void StopBackgroundSong()
        {
            if (_backgroundSong.HasValue)
                AudioManager.StopSound(_backgroundSong.Value);
        }

        // Volume's domain is 0f to 1f
        protected void SetBackgroundSong(string iPath, float iVolume)
        {
            _backgroundSong = AudioManager.Load(Content, iPath);
            _backgroundSongVolume = iVolume;
        }

        private int? _backgroundSong;
        private float _backgroundSongVolume;
        private bool _isFirstUpdate;
    }
}
using Microsoft.Xna.Framework.Audio;
using Raspberry_Lib.Content;

namespace Raspberry_Lib.Components
{
    internal class StreamSoundComponent : PausableComponent
    {
        private static class Settings
        {
            public const float Volume = 1;
        }

        public override void Initialize()
        {
            _streamSound = SoundEffect.FromFile(ContentData.AssetPaths.StreamSound);
            
            base.Initialize();
        }

        public void StartStreamSound()
        {
            System.Diagnostics.Debug.Assert(_streamSound != null);
            System.Diagnostics.Debug.Assert(_instance == null);

            _instance = _streamSound.CreateInstance();
            _instance.IsLooped = true;
            _instance.Volume = Settings.Volume;
            _instance.Play();
        }

        public void StopStreamSound()
        {
            System.Diagnostics.Debug.Assert(_instance != null);

            _instance.Stop();
            _instance.Dispose();
            _instance = null;
        }
        
        private SoundEffect _streamSound;
        private SoundEffectInstance _instance;

        protected override void OnPauseSet(bool iVal)
        {
            System.Diagnostics.Debug.Assert(_instance != null);

            if (iVal)
            {
                _instance.Pause();
            }
            else
            {
                _instance.Resume();
            }
        }
    }
}

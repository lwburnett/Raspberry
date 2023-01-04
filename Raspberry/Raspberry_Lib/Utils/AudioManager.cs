using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Nez.Systems;

namespace Raspberry_Lib
{
    public enum SoundStrategy
    {
        Overwrite, // Will stop an existing instance of the same sound to play a new one
        Overlap, // Will play both sound instances on top of each other
        Bounce // Won't play the current instance if one already exists
    }

    internal static class AudioManager
    {
        static AudioManager()
        {
            sIdCounter = -1;
            sSounds = new Dictionary<int, SoundEffect>();
            sSoundInstances = new Dictionary<int, SoundEffectInstance>();
        }

        public static int Load(NezContentManager iContentManager, string iSoundPath)
        {
            sIdCounter++;
            sSounds.Add(sIdCounter, iContentManager.LoadSoundEffect(iSoundPath));
            return sIdCounter;
        }

        public static void Unload(int iId)
        {
            System.Diagnostics.Debug.Assert(iId <= sIdCounter);

            if (sSoundInstances.TryGetValue(iId, out var instance))
            {
                instance.Stop();
                instance.Dispose();
                sSoundInstances.Remove(iId);
            }

            if (sSounds.TryGetValue(iId, out var sound))
            {
                sound.Dispose();
                sSounds.Remove(iId);
            }
        }

        public static void PlaySound(int iId, bool iLooping, float iVolume, SoundStrategy iStrategy)
        {
            System.Diagnostics.Debug.Assert(iId <= sIdCounter);

            if (iStrategy == SoundStrategy.Overwrite)
            {
                if (sSoundInstances.TryGetValue(iId, out var instance))
                {
                    instance.Stop();
                }
                else
                {
                    instance = sSounds[iId].CreateInstance();
                    sSoundInstances.Add(iId, instance);
                }

                PlaySoundEffect(ref instance, iLooping, iVolume);
            }
            else if (iStrategy == SoundStrategy.Overlap)
            {
                var newInstance = sSounds[iId].CreateInstance();
                PlaySoundEffect(ref newInstance, iLooping, iVolume);
                if (sSoundInstances.ContainsKey(iId))
                {
                    sSoundInstances[iId] = newInstance;
                }
                else
                {
                    sSoundInstances.Add(iId, newInstance);
                }
            }
            else if (iStrategy == SoundStrategy.Bounce)
            {
                if (sSoundInstances.ContainsKey(iId))
                {
                    var instance = sSoundInstances[iId];
                    if (instance.State == SoundState.Stopped)
                    {
                        PlaySoundEffect(ref instance, iLooping, iVolume);
                    }
                }
                else
                {
                    var instance = sSounds[iId].CreateInstance();
                    PlaySoundEffect(ref instance, iLooping, (int)iVolume);
                    sSoundInstances.Add(iId, instance);
                }
            }
            else
            {
                System.Diagnostics.Debug.Fail($"Unknown value {iStrategy} of type {nameof(SoundStrategy)}");
            }
        }

        public static void StopSound(int iId)
        {
            System.Diagnostics.Debug.Assert(iId <= sIdCounter);

            if (sSoundInstances.TryGetValue(iId, out var instance))
            {
                instance.Stop();
            }
        }

        public static void PauseSound(int iId)
        {
            System.Diagnostics.Debug.Assert(iId <= sIdCounter);

            if (sSoundInstances.TryGetValue(iId, out var instance))
            {
                instance.Pause();
            }
        }

        public static void ResumeSound(int iId)
        {
            System.Diagnostics.Debug.Assert(iId <= sIdCounter);

            if (sSoundInstances.TryGetValue(iId, out var instance))
            {
                instance.Resume();
            }
        }

        private static readonly Dictionary<int, SoundEffect> sSounds;
        private static readonly Dictionary<int, SoundEffectInstance> sSoundInstances;
        private static int sIdCounter;

        private static void PlaySoundEffect(ref SoundEffectInstance iInstance, bool iLooping, float iVolume)
        {
            iInstance.IsLooped = iLooping;
            iInstance.Volume = iVolume;
            iInstance.Play();
        }
    }
}

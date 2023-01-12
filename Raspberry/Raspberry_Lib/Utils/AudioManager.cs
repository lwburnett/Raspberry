using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Nez.Systems;
using StbVorbisSharp;

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

        public static int Load(NezContentManager iContent, string iSoundPath)
        {
            SoundEffect soundEffect;
            var extension = Path.GetExtension(iSoundPath);
            switch (extension)
            {
                case ".wav":
                    soundEffect = iContent.LoadSoundEffect(iSoundPath);
                    break;
                case ".ogg":
                    soundEffect = LoadOggSoundEffect(iSoundPath);
                    break;
                default:
                    System.Diagnostics.Debug.Fail($"Unsupported audio extension {extension}");
                    return -1;
            }

            sIdCounter++;

            sSounds.Add(sIdCounter, soundEffect);
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

        // https://github.com/StbSharp/StbVorbisSharp/blob/master/samples/StbVorbisSharp.MonoGame.Test.Prerender/Game1.cs
        private static SoundEffect LoadOggSoundEffect(string iPath)
        {
            byte[] bytes;
            using (var stream = Path.IsPathRooted(iPath) ? File.OpenRead(iPath) : TitleContainer.OpenStream(iPath))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            var audioShort = StbVorbis.decode_vorbis_from_memory(bytes, out var sampleRate, out var channels);
            var audioData = new byte[audioShort.Length * 2];
            for (var i = 0; i < audioShort.Length; ++i)
            {
                if (i * 2 >= audioData.Length)
                {
                    break;
                }

                var b1 = (byte)(audioShort[i] >> 8);
                var b2 = (byte)(audioShort[i] & 256);

                audioData[i * 2 + 0] = b2;
                audioData[i * 2 + 1] = b1;
            }

            return new SoundEffect(audioData, sampleRate, (AudioChannels)channels);
        }
    }
}

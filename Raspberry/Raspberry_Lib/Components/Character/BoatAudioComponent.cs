using System;
using System.Collections.Generic;
using Nez;
using Raspberry_Lib.Content;

namespace Raspberry_Lib.Components;

internal class BoatAudioComponent : PausableComponent
{
    private static class Settings
    {
        public const float RowTransition0 = .4f;
        public const float RowTransition1 = .7f;
        public const float RowTransition2 = .9f;
        public const float RowTransition3 = 1.25f;

        public const float RowVolumeBad = .25f;
        public const float RowVolumeMedium = .50f;
        public const float RowVolumeGood = 1.00f;
        public const float RowVolumeNeutral = .75f;

        public const float SfxDuration = 1.0f;
    }

    public BoatAudioComponent()
    {
        _sfxIds = new List<int>();
        _sfxInstances = new List<Tuple<int, float>>();
        _secondsSinceLastRow = Settings.RowTransition3;
        _rng = new System.Random();
    }

    public override void OnAddedToEntity()
    {
        var paths = new[]
        {
            ContentData.AssetPaths.Row1,
            ContentData.AssetPaths.Row2,
            ContentData.AssetPaths.Row3,
            ContentData.AssetPaths.Row4,
            ContentData.AssetPaths.Row5,
            ContentData.AssetPaths.Row6,
            ContentData.AssetPaths.Row7,
        };

        foreach (var path in paths)
        {
            _sfxIds.Add(AudioManager.Load(Entity.Scene.Content, path));
        }

        _characterMovementComponent = Entity.GetComponent<CharacterMovementComponent>();
        System.Diagnostics.Debug.Assert(_characterMovementComponent != null);

        base.Initialize();
    }

    public override void OnRemovedFromEntity()
    {
        foreach (var sfxId in _sfxIds)
        {
            AudioManager.Unload(sfxId);
        }

        base.OnRemovedFromEntity();
    }

    private readonly List<int> _sfxIds;
    private readonly List<Tuple<int, float>> _sfxInstances;
    private CharacterMovementComponent _characterMovementComponent;

    private float _secondsSinceLastRow;

    private readonly System.Random _rng;

    protected override void OnUpdate(float iTotalPlayableTime)
    {
        if (IsPaused)
            return;

        _secondsSinceLastRow += Time.DeltaTime;

        var currentInput = _characterMovementComponent.CurrentInput;

        HandleRowSfx(currentInput.Row, iTotalPlayableTime);

        CleanSfxInstanceList(iTotalPlayableTime);

        base.OnUpdate(iTotalPlayableTime);
    }

    protected override void OnPauseSet(bool iVal)
    {
        foreach (var sfxInstance in _sfxInstances)
        {
            if (iVal)
            {
                AudioManager.PauseSound(sfxInstance.Item1);
            }
            else
            {
                AudioManager.ResumeSound(sfxInstance.Item1);
            }
        }

        base.OnPauseSet(iVal);
    }

    private int GetRandomRowSfx() => _sfxIds[_rng.Next(_sfxIds.Count)];

    private void HandleRowSfx(bool iRowInput, float iTime)
    {
        if (iRowInput)
        {
            int? id;
            float? volume;

            if (_secondsSinceLastRow < Settings.RowTransition0)
            {
                id = null;
                volume = null;
            }
            else
            {
                id = GetRandomRowSfx();

                if (_secondsSinceLastRow < Settings.RowTransition1)
                {
                    volume = Settings.RowVolumeBad;
                }
                else if (_secondsSinceLastRow < Settings.RowTransition2)
                {
                    volume = Settings.RowVolumeMedium;
                }
                else if (_secondsSinceLastRow < Settings.RowTransition3)
                {
                    volume = Settings.RowVolumeGood;
                }
                else
                {
                    volume = Settings.RowVolumeNeutral;
                }
            }

            if (id.HasValue)
            {
                   _sfxInstances.Add(new Tuple<int, float>(id.Value, iTime)); 
                   AudioManager.PlaySound(id.Value, false, volume.Value, SoundStrategy.Overlap);
                   _secondsSinceLastRow = 0;
            }
        }
    }

    private void CleanSfxInstanceList(float iTotalPlayableTime)
    {
        var numInstances = _sfxInstances.Count;

        for (var ii = numInstances - 1; ii >= 0; ii--)
        {
            var thisInstance = _sfxInstances[ii];
            var timeDiff = iTotalPlayableTime - thisInstance.Item2;

            if (timeDiff >= Settings.SfxDuration)
            {
                _sfxInstances.RemoveAt(ii);
            }
            else
                break; // Assuming the list is in chronological order
        }
    }
}
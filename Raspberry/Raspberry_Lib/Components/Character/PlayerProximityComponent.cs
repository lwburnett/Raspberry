using System;
using Microsoft.Xna.Framework;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class PlayerProximityComponent : Component, IUpdatable, IBeginPlay, IPausable
    {
        private static class Settings
        {
            public static readonly RenderSetting StartingRadius = new(300);
            public static readonly RenderSetting BranchHitIncrease = new(100);
            public const float BranchHitIncreaseOverTimeSeconds = 1f;
            public static readonly RenderSetting RadiusDecayPerSecondLower = new(7.5f);
            public static readonly RenderSetting RadiusDecayPerSecondUpper = new(15.0f);
            public static readonly RenderSetting MaximumRadius = new(500);
            public static readonly RenderSetting MinimumRadius = new(100);

            public static readonly RenderSetting MinimumImpactSpeed = new(10);
            public static readonly RenderSetting MaximumImpactSpeed = new(150);
            public const float MaximumImpactDecayMultiplierLower = 6.0f;
            public const float MaximumImpactDecayMultiplierUpper = 18.0f;
            public const float ImpactDecayTimespanSeconds = .5f;
        }

        public PlayerProximityComponent(Action iOnRadiusTooLow)
        {
            Radius = Settings.StartingRadius.Value;
            _onRadiusTooLow = iOnRadiusTooLow;
            _lastBranchHitTime = 0;
            _lastObstacleHitTime = null;
            _decayMultiplier = null;
            _timeSpentPaused = 0;

#if VERBOSE
            _radiusDecayPerSecond = Settings.RadiusDecayPerSecondLower.Value;

            Verbose.TrackMetric(() => _radiusDecayPerSecond, v => $"Radius Decay Per Second: {v:G6}");
            Verbose.TrackMetric(() => _decayMultiplier ?? 0.0f, v => $"Radius Decay Multiplier: {v:G6}");
#endif
        }

        public int BeginPlayOrder => 95;
        public void OnBeginPlay()
        {
            var mapEntity = Entity.Scene.FindEntity("map");

            System.Diagnostics.Debug.Assert(mapEntity != null);

            _proceduralGenerator = mapEntity.GetComponent<ProceduralGeneratorComponent>();

            System.Diagnostics.Debug.Assert(_proceduralGenerator != null);
        }

        public bool IsPaused { get; set; }

        public float Radius { get; private set; }

        public void OnBranchHit()
        {
            _lastBranchHitTime = Time.TotalTime - _timeSpentPaused;

            _decayMultiplier = null;
            _lastObstacleHitTime = null;
        }

        public void Update()
        {
            if (IsPaused)
            {
                _timeSpentPaused += Time.DeltaTime;
                return;
            }

            if (_proceduralGenerator == null)
            {
                OnBeginPlay();
                return;
            }

            var adjustedTime = Time.TotalTime - _timeSpentPaused;

            if (adjustedTime - _lastBranchHitTime > Settings.BranchHitIncreaseOverTimeSeconds)
            {
                var radiusDecayPerSecond = MathHelper.Lerp(
                    Settings.RadiusDecayPerSecondLower.Value,
                    Settings.RadiusDecayPerSecondUpper.Value,
                    _proceduralGenerator.PlayerScoreRating / _proceduralGenerator.MaxPlayerScoreRating);

#if VERBOSE
                _radiusDecayPerSecond = radiusDecayPerSecond;
#endif

                if (!_decayMultiplier.HasValue || !_lastObstacleHitTime.HasValue)
                {
                    Radius -= radiusDecayPerSecond * Time.DeltaTime;
                }
                else
                {
                    if (adjustedTime - _lastObstacleHitTime.Value < Settings.ImpactDecayTimespanSeconds)
                    {
                        Radius -= _decayMultiplier.Value * radiusDecayPerSecond * Time.DeltaTime;
                    }
                    else
                    {
                        Radius -= radiusDecayPerSecond * Time.DeltaTime;
                        _decayMultiplier = null;
                        _lastObstacleHitTime = null;
                    }
                }

                if (Radius < Settings.MinimumRadius.Value)
                    _onRadiusTooLow();
            }
            else
            {
                Radius += Settings.BranchHitIncrease.Value * Time.DeltaTime / Settings.BranchHitIncreaseOverTimeSeconds;
                if (Radius > Settings.MaximumRadius.Value)
                    Radius = Settings.MaximumRadius.Value;
            }
        }

        public void OnObstacleHit(float iImpactSpeed)
        {
            var adjustedTime = Time.TotalTime - _timeSpentPaused;

            var impactSpeedMag = Math.Abs(iImpactSpeed);
            if (impactSpeedMag <= Settings.MinimumImpactSpeed.Value)
                return;

            var lerpValue = (impactSpeedMag - Settings.MinimumImpactSpeed.Value) / 
                            (Settings.MaximumImpactSpeed.Value - Settings.MinimumImpactSpeed.Value);

            var clampedLerpValue = MathHelper.Clamp(lerpValue, 0.0f, 1.0f);

            var maximumImpactDecayMultiplier = MathHelper.Lerp(
                Settings.MaximumImpactDecayMultiplierLower,
                Settings.MaximumImpactDecayMultiplierUpper,
                _proceduralGenerator.PlayerScoreRating / _proceduralGenerator.MaxPlayerScoreRating);

            var newDecayMultiplier = MathHelper.Lerp(1.0f, maximumImpactDecayMultiplier, clampedLerpValue);

            if (_decayMultiplier.HasValue && _lastObstacleHitTime.HasValue)
            {
                var oldDecayTimeRemaining = Settings.ImpactDecayTimespanSeconds - (adjustedTime - _lastObstacleHitTime.Value);

                var oldDecayEffect = _decayMultiplier.Value * oldDecayTimeRemaining;
                var newDecayEffect = newDecayMultiplier * Settings.ImpactDecayTimespanSeconds;

                if (newDecayEffect >= oldDecayEffect || oldDecayTimeRemaining <= 0)
                {
                    _lastObstacleHitTime = adjustedTime;
                    _decayMultiplier = newDecayMultiplier;
                }
            }
            else
            {
                _lastObstacleHitTime = adjustedTime;
                _decayMultiplier = newDecayMultiplier;
            }
        }

        private readonly Action _onRadiusTooLow;
        private float _lastBranchHitTime;

        private float? _lastObstacleHitTime;
        private float? _decayMultiplier;

        private float _timeSpentPaused;

        private ProceduralGeneratorComponent _proceduralGenerator;

#if VERBOSE
        private float _radiusDecayPerSecond;
#endif
    }
}

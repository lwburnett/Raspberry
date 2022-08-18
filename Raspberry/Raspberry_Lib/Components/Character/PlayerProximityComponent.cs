using System;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class PlayerProximityComponent : Component, IUpdatable
    {
        private static class Settings
        {
            public static readonly RenderSetting StartingRadius = new(300);
            public static readonly RenderSetting BranchHitIncrease = new(300);
            public const float BranchHitIncreaseOverTimeSeconds = .5f;
            public static readonly RenderSetting RadiusDecayPerSecond = new(10);
            public static readonly RenderSetting MaximumRadius = new(900);
            public static readonly RenderSetting MinimumRadius = new(50);
        }

        public PlayerProximityComponent(Action iOnRadiusTooLow)
        {
            Radius = Settings.StartingRadius.Value;
            _onRadiusTooLow = iOnRadiusTooLow;
            _lastBranchHitTime = 0;
        }

        public float Radius { get; private set; }

        public void OnBranchHit()
        {
            _lastBranchHitTime = Time.TotalTime;
        }

        public void Update()
        {
            if (Time.TotalTime - _lastBranchHitTime > Settings.BranchHitIncreaseOverTimeSeconds)
            {
                Radius -= Settings.RadiusDecayPerSecond.Value * Time.DeltaTime;

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

        private readonly Action _onRadiusTooLow;
        private float _lastBranchHitTime;
    }
}

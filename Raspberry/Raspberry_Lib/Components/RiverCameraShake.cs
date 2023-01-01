using Microsoft.Xna.Framework;
using Nez;
using System;

namespace Raspberry_Lib.Components
{
    internal class RiverCameraShake : CameraShake
    {
        private static class Settings
        {
            public static readonly RenderSetting MinimumImpactSpeed = new(10);
            public static readonly RenderSetting MaximumImpactSpeed = new(150);

            public const float ShakeMin = 5f;
            public const float ShakeMax = 200f;
        }

        public void OnObstacleHit(Vector2 iImpactVelocity)
        {
            if (!SettingsManager.GetGameSettings().ScreenShake)
                return;

            var impactSpeed = Math.Abs(iImpactVelocity.Length());
            if (impactSpeed <= Settings.MinimumImpactSpeed.Value)
                return;

            var lerpValue = (impactSpeed - Settings.MinimumImpactSpeed.Value) /
                            (Settings.MaximumImpactSpeed.Value - Settings.MinimumImpactSpeed.Value);

            var clampedLerpValue = MathHelper.Clamp(lerpValue, 0.0f, 1.0f);

            var shakeIntensity = MathHelper.Lerp(
                Settings.ShakeMin, Settings.ShakeMax, clampedLerpValue);

            iImpactVelocity.Normalize();
            Shake(shakeIntensity, 0.02f, iImpactVelocity);
        }
    }
}

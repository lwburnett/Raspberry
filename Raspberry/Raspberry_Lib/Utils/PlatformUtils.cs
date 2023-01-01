using System;

namespace Raspberry_Lib
{
    public static class PlatformUtils
    {
        /// <summary>
        /// Sets the Render Scale
        /// </summary>
        /// <param name="iRenderScale">The ratio of the actual screen width to the target screen width</param>
        public static void SetRenderScale(float iRenderScale)
        {
            System.Diagnostics.Debug.Assert(iRenderScale > 0);
            System.Diagnostics.Debug.Assert(!sIsRenderScaleSet);

            sRenderScale = iRenderScale;
            sIsRenderScaleSet = true;

        }

        /// <summary>
        /// Gets the Render Scale
        /// </summary>
        /// <returns>The ratio of the actual screen width to the target screen width</returns>
        public static float GetRenderScale()
        {
            System.Diagnostics.Debug.Assert(sIsRenderScaleSet);
            System.Diagnostics.Debug.Assert(sRenderScale > 0);

            return sRenderScale;
        }

        private static bool sIsRenderScaleSet;
        private static float sRenderScale = 1;

        /// <summary>
        /// Set the platform-specific callback to be used when requesting a vibration effect
        /// </summary>
        /// <param name="iVibrateAction">The callback</param>
        public static void SetVibrateCallback(Action<long, byte?> iVibrateAction)
        {
            System.Diagnostics.Debug.Assert(!sIsVibrateCallbackSet);

            sVibrateCallback = iVibrateAction;
            sIsVibrateCallbackSet = true;
        }

        /// <summary>
        /// Vibrates the device
        /// </summary>
        /// <param name="iDurationMilliseconds">Duration of the vibration in milliseconds</param>
        /// <param name="iAmplitude">Intensity of the vibration. Null is device default. 1 is least intense. 255 is most intense</param>
        public static void Vibrate(long iDurationMilliseconds, byte? iAmplitude)
        {
            System.Diagnostics.Debug.Assert(sIsVibrateCallbackSet);

            sVibrateCallback?.Invoke(iDurationMilliseconds, iAmplitude);
        }

        /// <summary>
        /// Common helper function to vibrate for a touch input
        /// </summary>
        public static void VibrateForUiNavigation() => Vibrate(50, 60);

        private static bool sIsVibrateCallbackSet;
        private static Action<long, byte?> sVibrateCallback;
    }
}

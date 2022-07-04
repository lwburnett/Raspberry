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
    }
}

using System.Diagnostics;

namespace Raspberry_Lib
{
    internal class RenderSetting
    {
        #region Scale

        public static void SetRenderScale(float iRenderScale)
        {
            Debug.Assert(iRenderScale > 0);
            Debug.Assert(!sIsRenderScaleSet);

            sRenderScale = iRenderScale;
            sIsRenderScaleSet = true;

        }

        private static bool sIsRenderScaleSet;
        private static float sRenderScale = 1;

        #endregion

        public RenderSetting(float iValue)
        {
            _value = iValue;
        }

        public float Value => _value * sRenderScale;
        private readonly float _value;

    }
}

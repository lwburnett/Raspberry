namespace Raspberry_Lib
{
    /// <summary>
    /// A simple wrapper of a float that scales itself by PlatformUtils.GetRenderScale()
    /// </summary>
    internal class RenderSetting
    {
        public RenderSetting(float iRawValue)
        {
            RawValue = iRawValue;
        }

        public float Value => RawValue * PlatformUtils.GetRenderScale();
        public float RawValue { get; }
    }
}

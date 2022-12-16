using System;

namespace Raspberry_Lib
{
    internal static class SeedUtils
    {
        public static int GetDistanceChallengeSeedForToday()
        {
            return GetSeedForDateTime(DateTime.Today);
        }

        public static int GetTimeChallengeSeedForToday()
        {
            return GetSeedForDateTime(DateTime.Today + TimeSpan.FromHours(12));
        }

        private static int GetSeedForDateTime(DateTime iDateTime)
        {
            var ticks = iDateTime.Ticks;
            var divisor = ticks / MaxIntValue;
            var remainder = ticks - divisor * MaxIntValue;

            return (int)remainder + (int)divisor;
        }

        private const long MaxIntValue = 2000000000;
    }
}

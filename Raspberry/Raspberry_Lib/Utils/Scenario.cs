using System;
using System.Collections.Generic;

namespace Raspberry_Lib
{
    internal class Scenario
    {
        private Scenario(
            int? iSeed,
            string iTitle,
            IEnumerable<string> iIntroLines,
            Func<float, float, bool, bool> iHaveLost,
            IEnumerable<string> iLoseLines,
            Func<float, float, bool, bool> iHaveEnded,
            IEnumerable<string> iEndLines,
            Action<float, float> iRegisterStats)
        {
            Seed = iSeed;
            Title = iTitle;
            IntroLines = iIntroLines;
            _haveLost = iHaveLost;
            LoseLines = iLoseLines;
            _haveEnded = iHaveEnded;
            EndLines = iEndLines;
            _registerStats = iRegisterStats;
        }

        public int? Seed { get; }
        public string Title { get; }
        public IEnumerable<string> IntroLines { get; }
        public IEnumerable<string> LoseLines { get; }
        public IEnumerable<string> EndLines { get; }

        private readonly Func<float, float, bool, bool> _haveLost;
        private readonly Func<float, float, bool, bool> _haveEnded;
        private readonly Action<float, float> _registerStats;

        public bool HaveLost(float iDistance, float iTime, bool iStillHaveEnergy)
        {
            return _haveLost(iDistance, iTime, iStillHaveEnergy);
        }

        public bool HaveEnded(float iDistance, float iTime, bool iStillHaveEnergy)
        {
            return _haveEnded(iDistance, iTime, iStillHaveEnergy);
        }

        public void RegisterStats(float iDistance, float iTime)
        {
            _registerStats(iDistance, iTime);
        }

        #region Static Constructors

        public static Scenario CreateDistanceChallengeScenario()
        {
            return new Scenario(
                SeedUtils.GetDistanceChallengeSeedForToday(),
                "Distance Challenge",
                new []{"Make it as far as", "you can in 2:00 minutes!" },
                DistanceModeIsLost,
                new[] {string.Empty},
                DistanceModeIsEnded,
                new[] {"Your time has ended."},
                DataManager.OnNewDistanceChallengeData);
        }

        private static bool DistanceModeIsLost(float iDistance, float iTime, bool iStillHaveEnergy)
        {
            return false;
        }

        private static bool DistanceModeIsEnded(float iDistance, float iTime, bool iStillHaveEnergy)
        {
            return !iStillHaveEnergy || iTime >= 120f;
        }

        public static Scenario CreateTimeChallengeScenario()
        {
            return new Scenario(
                SeedUtils.GetTimeChallengeSeedForToday(),
                "Time Challenge",
                new[] { "Make to 750 meters", "as quickly as possible!" },
                TimeModeIsLost,
                new[] { string.Empty },
                TimeModeIsEnded,
                new[] { "You traversed through the desert." },
                DataManager.OnNewTimeChallengeData);
        }

        private static bool TimeModeIsLost(float iDistance, float iTime, bool iStillHaveEnergy)
        {
            return !iStillHaveEnergy;
        }

        private static bool TimeModeIsEnded(float iDistance, float iTime, bool iStillHaveEnergy)
        {
            return iDistance >= 750f;
        }

        public static Scenario CreateEndlessChallengeScenario()
        {
            return new Scenario(
                null,
                "Endless Challenge",
                new[] { "Traverse as far as possible", "before your energy runs out!" },
                EndlessModeIsLost,
                new[] { string.Empty },
                EndlessModeIsEnded,
                new[] { "You traversed through the desert." },
                DataManager.OnNewEndlessData);
        }

        public static Scenario CreateTutorialScenario()
        {
            return new Scenario(
                null,
                "Tutorial",
                new string[]{},
                EndlessModeIsLost,
                new[] { string.Empty },
                EndlessModeIsEnded,
                new[] { "You traversed through the desert." },
                DataManager.OnNewEndlessData);
        }

        private static bool EndlessModeIsLost(float iDistance, float iTime, bool iStillHaveEnergy)
        {
            return false;
        }

        private static bool EndlessModeIsEnded(float iDistance, float iTime, bool iStillHaveEnergy)
        {
            return !iStillHaveEnergy;
        }

        #endregion
    }
}

using System;

namespace Raspberry_Lib
{
    internal struct GameData
    {
        public GameData()
        {
            DistChallengeRecordDateTime = null;
            DistChallengeRecord = null;
            TimeChallengeRecordDateTime = null;
            TimeChallengeRecord = null;
            EndlessChallengeRecord = null;
        }

        public GameData(
            DateTime? iDistChallengeRecordDateTime, 
            float? iDistChallengeRecord, 
            DateTime? iTimeChallengeRecordDateTime, 
            TimeSpan? iTimeChallengeRecord, 
            float? iEndlessChallengeRecord)
        {
            DistChallengeRecordDateTime = iDistChallengeRecordDateTime;
            DistChallengeRecord = iDistChallengeRecord;
            TimeChallengeRecordDateTime = iTimeChallengeRecordDateTime;
            TimeChallengeRecord = iTimeChallengeRecord;
            EndlessChallengeRecord = iEndlessChallengeRecord;
        }

        public DateTime? DistChallengeRecordDateTime { get; }
        public float? DistChallengeRecord { get; }
        public DateTime? TimeChallengeRecordDateTime { get; }
        public TimeSpan? TimeChallengeRecord { get; }
        public float? EndlessChallengeRecord { get; }
    }
}

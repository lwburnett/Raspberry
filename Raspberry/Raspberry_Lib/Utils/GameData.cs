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
            DateTime iDistChallengeRecordDateTime, 
            TimeSpan iDistChallengeRecord, 
            DateTime iTimeChallengeRecordDateTime, 
            TimeSpan iTimeChallengeRecord, 
            TimeSpan iEndlessChallengeRecord)
        {
            DistChallengeRecordDateTime = iDistChallengeRecordDateTime;
            DistChallengeRecord = iDistChallengeRecord;
            TimeChallengeRecordDateTime = iTimeChallengeRecordDateTime;
            TimeChallengeRecord = iTimeChallengeRecord;
            EndlessChallengeRecord = iEndlessChallengeRecord;
        }

        public DateTime? DistChallengeRecordDateTime { get; }
        public TimeSpan? DistChallengeRecord { get; }
        public DateTime? TimeChallengeRecordDateTime { get; }
        public TimeSpan? TimeChallengeRecord { get; }
        public TimeSpan? EndlessChallengeRecord { get; }
    }
}

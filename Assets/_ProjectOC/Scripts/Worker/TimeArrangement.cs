namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct TimeArrangement
    {
        public TimeStatus[] Status;
        public TimeArrangement(int hour=24)
        {
            Status = new TimeStatus[hour];
        }
        public TimeStatus GetTimeStatus(int hour)
        {
            if (0 <= hour && hour < 24)
            {
                return Status[hour];
            }
            return TimeStatus.Relax;
        }
        public void SetTimeStatus(int hour, TimeStatus status)
        {
            if (0 <= hour && hour < 24)
            {
                Status[hour] = status;
            }
        }
        public void SetTimeStatusAll(TimeStatus newStatus)
        {
            for (int i = 0; i < 24 ; i++)
            {
                Status[i] = newStatus;
            }
        }
        public void ReverseTimeAll()
        {
            for (int i = 0; i < 24; i++)
            {
                Status[i] = Status[i] == TimeStatus.Relax ? TimeStatus.Work_OnDuty : TimeStatus.Relax;
            }
        }
    }
}

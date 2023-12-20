namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public class TimeArrangement
    {
        private TimeStatus[] Status = new TimeStatus[24];
        public TimeStatus this[int index]
        {
            get
            {
                if (index >= 0 && index < 24)
                {
                    return Status[index];
                }
                else
                {
                    return TimeStatus.Relax;
                }
            }
            set
            {
                if (index >= 0 && index < 24)
                {
                    Status[index] = value;
                }
            }
        }

        public void SetTimeStatusAll(TimeStatus newStatus)
        {
            for (int i = 0; i < 24 ; i++)
            {
                Status[i] = newStatus;
            }
        }
        public void SetTimeArrangement(TimeArrangement timeArrangement)
        {
            if (timeArrangement != null)
            {
                for (int i = 0; i < 24; i++)
                {
                    Status[i] = timeArrangement[i];
                }
            }
        }
    }
}

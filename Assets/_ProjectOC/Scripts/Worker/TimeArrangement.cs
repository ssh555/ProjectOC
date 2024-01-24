namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public class TimeArrangement
    {
        public TimeStatus[] Status = new TimeStatus[24];
        public TimeArrangement()
        {
        }
        public TimeArrangement(TimeArrangement arrangement)
        {
            for (int i = 0; i < 24; i++)
            {
                Status[i] = arrangement[i];
            }
        }

        public TimeStatus this[int index]
        {
            get
            {
                if (0 <= index && index < 24)
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
                if (0 <= index && index < 24)
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
            for (int i = 0; i < 24; i++)
            {
                Status[i] = timeArrangement[i];
            }
        }
    }
}

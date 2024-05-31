using ExcelToJson;
using System;


namespace ProjectOC.RestaurantNS
{
    [Serializable]
    public struct WorkerFoodTableData : IGenData
    {
        public string ID;
        public string ItemID;
        public int AlterAP;
        public float AlterMoodOddsProb;
        public int AlterMoodOddsValue;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> ItemID
            this.ItemID = Program.ParseString(row[1]);
            // 2 -> AlterAP
            this.AlterAP = Program.ParseInt(row[2]);
            // 3 4 -> AlterMoodOdds
            var alterMoodOdds = Program.ParseStringList(row[3]);
            if (alterMoodOdds.Count == 2)
            {
                this.AlterMoodOddsProb = Program.ParseFloat(alterMoodOdds[0]);
                this.AlterMoodOddsValue = Program.ParseInt(alterMoodOdds[1]);
            }
            return true;
        }
    }
}

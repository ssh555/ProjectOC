using ExcelToJson;
using System;


namespace ProjectOC.RestaurantNS
{
    [Serializable]
    public struct WorkerFoodTableData : IGenData
    {
        public string ID;
        public string ItemID;
        public int EatTime;
        public int AlterAP;
        public Tuple<float, int> AlterMoodOdds;

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
            // 2 -> EatTime
            this.EatTime = Program.ParseInt(row[2]);
            // 3 -> AlterAP
            this.AlterAP = Program.ParseInt(row[3]);
            // 4 5 -> AlterMoodOdds
            this.AlterMoodOdds = Tuple.Create(Program.ParseFloat(row[4]), Program.ParseInt(row[5]));
            return true;
        }
    }
}

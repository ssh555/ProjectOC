using ExcelToJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProjectOC.Order
{
    [System.Serializable]
    public struct OrderTableData : IGenData
    {
        public string ID;
        public OrderType OrderType;
        public string OrderName;
        public string OrderDescription;
        public List<OrderMap> RequireList;
        public List<OrderMap> ItemReward;
        public List<OrderMap> ClanReward;
        public List<OrderMap> CharaReward;
        public bool IsFirstOrder;
        public int CD;
        public int ReceiveDDL;
        public int DeliverDDL;
        public List<OrderMap> PayBack;
        public string[] Contacter;

        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> Type
            this.OrderType = Program.ParseEnum<OrderType>(row[1], "Normal");
            // 2 -> Name
            this.OrderName = Program.ParseString(row[2]);
            // 3 -> ItemDescription
            this.OrderDescription = Program.ParseString(row[3]);
            // 4 -> RequireList
            this.RequireList = Program.ParseOrderMap(row[4]);
            // 5 -> ItemReward
            this.ItemReward = Program.ParseOrderMap(row[5]);
            // 6 -> ClanReward
            this.ClanReward = Program.ParseOrderMap(row[6]);
            // 7 -> CharaReward
            this.CharaReward = Program.ParseOrderMap(row[7]);
            // 8 -> IsFirstOrder
            this.IsFirstOrder = Program.ParseBool(row[8]);
            // 9 -> CD
            this.CD = Program.ParseInt(row[9]);
            // 10 -> ReceiveDDL
            this.ReceiveDDL = Program.ParseInt(row[10]);
            // 11 -> DeliverDDL
            this.DeliverDDL = Program.ParseInt(row[11]);
            // 12 -> PayBack
            this.PayBack = Program.ParseOrderMap(row[12]);
            // 13 -> Contacter
            this.Contacter = Program.ParseStringList(row[13]).ToArray();
            return true;
        }
    }

    [System.Serializable]
    public enum OrderType
    {
        Urgent = 0,
        Special,
        Normal
    }

    [System.Serializable]
    public struct OrderMap
    {
        public string id;
        public int num;
    }
}

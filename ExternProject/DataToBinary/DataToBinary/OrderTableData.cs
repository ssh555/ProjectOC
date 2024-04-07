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
            if (row[0] == null || row[0] == "")
            {
                return false;
            }
            this.ID = row[0];
            this.OrderType = (OrderType)Enum.Parse(typeof(OrderType), row[1]);
            this.OrderName = row[2];
            this.OrderDescription = row[3];
            this.RequireList = Program.ParseOrderMap(row[4]);
            this.ItemReward = Program.ParseOrderMap(row[5]);
            this.ClanReward = Program.ParseOrderMap(row[6]);
            this.CharaReward = Program.ParseOrderMap(row[7]);
            this.IsFirstOrder = int.Parse(row[8]) != 0;
            if (!string.IsNullOrEmpty(row[9]))
            {
                this.CD = int.Parse(row[9]);
            }
            if (!string.IsNullOrEmpty(row[10]))
            {
                this.ReceiveDDL = int.Parse(row[10]);
            }
            if (!string.IsNullOrEmpty(row[11]))
            {
                this.DeliverDDL = int.Parse(row[11]);
            }
            this.PayBack = Program.ParseOrderMap(row[12]);
            this.Contacter = new List<string>().ToArray();
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

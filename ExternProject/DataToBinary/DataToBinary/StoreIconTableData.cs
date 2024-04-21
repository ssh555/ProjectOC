using ExcelToJson;
using System;
using System.Collections.Generic;

namespace ProjectOC.StoreNS
{
    [System.Serializable]
    public struct StoreIconTableData : IGenData
    {
        public string ID;
        public string Icon;
        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = Program.ParseString(row[0]);
            // 1 -> Icon
            this.Icon = Program.ParseString(row[1]);
            return true;
        }
    }
}

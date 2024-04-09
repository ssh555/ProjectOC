using ExcelToJson;
using System;
using System.Collections.Generic;

namespace ProjectOC.StoreNS
{
    public struct StoreIconTableData : IGenData
    {
        public string ID;
        public string Icon;
        public bool GenData(string[] row)
        {
            if (row[0] == null || row[0] == "")
            {
                return false;
            }
            // 0 -> ID
            this.ID = row[0];
            // 1 -> Icon
            this.Icon = row[1];
            return true;
        }
    }
}

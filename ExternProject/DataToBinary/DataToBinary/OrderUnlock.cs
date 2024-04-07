using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using System;


namespace ProjectOC.Order
{
    /// <summary>
    /// TODO
    /// </summary>
    public struct OrderUnlock
    {
        public string ID;
        public string[] UnlockOrders_LV0;
        public string[] UnlockOrders_LV1;
        public string[] UnlockOrders_LV2;
        public string[] UnlockOrders_LV3;
        public string[] UnlockOrders_LV4;
        public string[] UnlockOrders_LV5;
        public bool GenData(string[] row)
        {
            if (row[0] == null || row[0] == "")
            {
                return false;
            }
            return true;
        }
    }
}

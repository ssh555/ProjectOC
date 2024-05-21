namespace ProjectOC.MissionNS
{
    public class ItemTransport : Transport<ML.Engine.InventorySystem.Item>
    {
        public ItemTransport(MissionTransport<ML.Engine.InventorySystem.Item> mission, int missionNum,
            IMissionObj<ML.Engine.InventorySystem.Item> source, IMissionObj<ML.Engine.InventorySystem.Item> target, WorkerNS.Worker worker)
            : base(mission, missionNum, source, target, worker) { }
        protected override int GetWeight() { return Data.Weight; }
        protected override bool WorkerAddData(int num)
        {
            if (Worker != null && num == 1 && !Worker.TransportItems.Contains(Data))
            {
                Worker.TransportItems.Add(Data);
                return true;
            }
            return false;
        }
        protected override int WorkerRemoveData(int num)
        {
            if (num == 1 && Worker.TransportItems.Contains(Data))
            {
                return Worker.TransportItems.Remove(Data) ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }
        protected override void DataToWorldItem(int num)
        {
            if (num == 1)
            {
                ML.Engine.InventorySystem.ItemManager.Instance?.SpawnWorldItem(Data, Worker.transform.position, Worker.transform.rotation);
            }
        }
    }
}
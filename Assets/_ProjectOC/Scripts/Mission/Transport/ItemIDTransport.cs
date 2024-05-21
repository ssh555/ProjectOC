using System.Collections.Generic;

namespace ProjectOC.MissionNS
{
    public class ItemIDTransport : Transport<string>
    {
        public ItemIDTransport(MissionTransport<string> mission, int missionNum, 
            IMissionObj<string> source, IMissionObj<string> target, WorkerNS.Worker worker) 
            : base(mission, missionNum, source, target, worker) { }
        protected override int GetWeight() { return ML.Engine.InventorySystem.ItemManager.Instance.GetWeight(Data); }
        protected override bool WorkerAddData(int num)
        {
            if (Worker != null)
            {
                if (!Worker.TransportDict.ContainsKey(Data))
                {
                    Worker.TransportDict[Data] = 0;
                }
                Worker.TransportDict[Data] += num;
                return true;
            }
            return false;
        }
        protected override int WorkerRemoveData(int num)
        {
            int remove = 0;
            if (Worker.TransportDict.ContainsKey(Data))
            {
                remove = Worker.TransportDict[Data] <= num ? Worker.TransportDict[Data] : num;
                Worker.TransportDict[Data] -= remove;
                if (Worker.TransportDict[Data] == 0)
                {
                    Worker.TransportDict.Remove(Data);
                }
            }
            return remove;
        }
        protected override void DataToWorldItem(int num)
        {
            if (ML.Engine.InventorySystem.ItemManager.Instance != null)
            {
                List<ML.Engine.InventorySystem.Item> items = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(Data, num);
                foreach (var item in items)
                {
#pragma warning disable CS4014
                    ML.Engine.InventorySystem.ItemManager.Instance.SpawnWorldItem(item, Worker.transform.position, Worker.transform.rotation);
#pragma warning restore CS4014
                }
            }
        }
    }
}
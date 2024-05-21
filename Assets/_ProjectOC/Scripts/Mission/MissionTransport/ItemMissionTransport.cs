namespace ProjectOC.MissionNS
{
    public class ItemMissionTransport : MissionTransport<ML.Engine.InventorySystem.Item>
    {
        public ItemMissionTransport(MissionTransportType type, ML.Engine.InventorySystem.Item data, int missionNum,
            IMissionObj<ML.Engine.InventorySystem.Item> imission, MissionInitiatorType initiatorType)
            : base(type, data, missionNum, imission, initiatorType) { }
        public override int GetWeight() { return Data.Weight; }
    }
}
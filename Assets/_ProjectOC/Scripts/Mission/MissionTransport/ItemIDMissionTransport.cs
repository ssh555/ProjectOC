namespace ProjectOC.MissionNS
{
    public class ItemIDMissionTransport : MissionTransport<string>
    {
        public ItemIDMissionTransport(MissionTransportType type, string data, int missionNum, 
            IMissionObj<string> imission, MissionInitiatorType initiatorType) 
            : base(type, data, missionNum, imission, initiatorType) { }
        public override int GetWeight() { return ML.Engine.InventorySystem.ItemManager.Instance.GetWeight(Data); }
    }
}
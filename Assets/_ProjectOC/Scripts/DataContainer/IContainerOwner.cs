using Sirenix.OdinInspector;

namespace ProjectOC.DataNS
{
    public interface IContainerOwner : MissionNS.IMissionObj, ML.Engine.InventorySystem.IInventory { }
    public interface IContainerOwner<T> : IContainerOwner
    {
        [LabelText("�洢����"), ReadOnly]
        public DataContainer<T> DataContainer { get; set; }

        public void InitData(int capacity, int dataCapacity)
        {
            DataContainer = new DataContainer<T>(capacity, dataCapacity);
        }
    }
}

using Sirenix.OdinInspector;

namespace ProjectOC.DataNS
{
    public interface IContainerOwner : ML.Engine.InventorySystem.IInventory { }
    public interface IContainerOwner<T> : MissionNS.IMissionObj<T>, IContainerOwner
    {
        [LabelText("�洢����"), ReadOnly]
        public DataContainer<T> DataContainer { get; set; }

        public void InitData(int capacity, int dataCapacity)
        {
            DataContainer = new DataContainer<T>(capacity, dataCapacity);
        }
    }
}

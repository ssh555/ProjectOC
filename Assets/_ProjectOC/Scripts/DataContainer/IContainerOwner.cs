using Sirenix.OdinInspector;

namespace ProjectOC.DataNS
{
    public interface IContainerOwner : MissionNS.IMissionObj, ML.Engine.InventorySystem.IInventory { }
    public interface IContainerOwner<T> : IContainerOwner
    {
        [LabelText("´æ´¢Êý¾Ý"), ReadOnly]
        public DataContainer<T> DataContainer { get; set; }

        public void InitData(int capacity, int dataCapacity)
        {
            DataContainer = new DataContainer<T>(capacity, dataCapacity);
        }
    }
}

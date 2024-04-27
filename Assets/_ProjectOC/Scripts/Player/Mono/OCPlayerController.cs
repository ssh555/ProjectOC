using System.Collections.Generic;
using System.Linq;
using ML.PlayerCharacterNS;

namespace ProjectOC.Player
{
    [System.Serializable]
    public class OCPlayerController : PlayerController
    {
        public PlayerCharacter currentCharacter = null;

        private List<string> ICharacterABResourcePath = new List<string>();
        public OCPlayerControllerState OCState => State as OCPlayerControllerState;

        public OCPlayerController()
        {
            State = new OCPlayerControllerState(this);
            //ABResource Path Initʱ����
            ICharacterABResourcePath.Add("Prefab_Character/Prefab_PlayerCharacter.prefab");
        }

        public override ICharacter SpawnCharacter(int _index = 0, IStartPoint _startPoint = null)
        {
            IPlayerCharacter playerCharacter = null;
            ML.Engine.Manager.GameManager.Instance.ABResourceManager
                .InstantiateAsync(ICharacterABResourcePath[_index], isGlobal: true).Completed += (handle) =>
            {
                var playerCharacter = handle.Result.GetComponent<PlayerCharacter>();
                SetCharacterTransform(playerCharacter.transform, _startPoint);
                currentCharacter = playerCharacter;
                SpawnedCharacters.Add(playerCharacter);
                playerCharacter.OnSpawn(this);
            };
            return playerCharacter as ICharacter;
        }

        #region Inventory
        /// <summary>
        /// ��ȡ��ұ���
        /// </summary>
        public ML.Engine.InventorySystem.IInventory GetInventory()
        {
            return OCState.Inventory;
        }
        /// <summary>
        /// ��ȡ��ҺͲֿ��Inventory
        /// </summary>
        /// <param name="containStore">�Ƿ�����ֿ�</param>
        /// <param name="priority">���ĵ����ȼ���0��ʾû�����ȼ���1��ʾ�Ӹ����ȼ����ģ�-1��ʾ�ӵ����ȼ�����</param>
        public List<ML.Engine.InventorySystem.IInventory> GetInventorys(bool containStore = true, int priority = 0)
        {
            List<ML.Engine.InventorySystem.IInventory> result = new List<ML.Engine.InventorySystem.IInventory>();
            result.Add(OCState.Inventory);
            if (containStore)
            {
                result.AddRange(ManagerNS.LocalGameManager.Instance.StoreManager.GetStores(priority));
            }
            return result;
        }

        /// <summary>
        /// �����Ͳֿ����Ƿ��ж�Ӧ��������Ʒ��
        /// </summary>
        public bool InventoryHaveItem(string itemID, int amount, bool containStore = true)
        {
            if (!string.IsNullOrEmpty(itemID) && amount >= 0)
            {
                int num = OCState.Inventory.GetItemAllNum(itemID);
                if (containStore && num < amount)
                {
                    List<StoreNS.Store> stores = ManagerNS.LocalGameManager.Instance.StoreManager.GetStores();
                    foreach (StoreNS.Store store in stores)
                    {
                        num += store.GetAmount(itemID, DataNS.DataOpType.Storage);
                        if (num >= amount)
                        {
                            return true;
                        }
                    }
                }
                return num >= amount;
            }
            return false;
        }

        public bool InventoryHaveItems(List<ML.Engine.InventorySystem.CompositeSystem.Formula> formulas, bool containStore = true)
        {
            if (formulas == null) { return false; }
            Dictionary<string, int> needs = new Dictionary<string, int>();
            foreach (var formula in formulas)
            {
                if (!needs.ContainsKey(formula.id))
                {
                    needs[formula.id] = 0;
                }
                needs[formula.id] += formula.num;
            }
            return InventoryHaveItems(needs, containStore);
        }

        public bool InventoryHaveItems(Dictionary<string, int> needs, bool containStore = true)
        {
            if (needs == null) { return false; }
            Dictionary<string, int> curs = new Dictionary<string, int>();
            HashSet<string> reamin = new HashSet<string>();

            foreach (var kv in needs)
            {
                curs[kv.Key] = OCState.Inventory.GetItemAllNum(kv.Key);
                if (curs[kv.Key] < kv.Value)
                {
                    reamin.Add(kv.Key);
                }
            }

            if (containStore && reamin.Count > 0)
            {
                List<StoreNS.Store> stores = ManagerNS.LocalGameManager.Instance.StoreManager.GetStores();
                foreach (StoreNS.Store store in stores)
                {
                    foreach (string id in reamin.ToArray())
                    {
                        curs[id] += store.GetAmount(id, DataNS.DataOpType.Storage);
                        if (curs[id] >= needs[id])
                        {
                            reamin.Remove(id);
                        }
                    }
                    if (reamin.Count == 0)
                    {
                        break;
                    }
                }
            }
            return reamin.Count == 0;
        }

        /// <summary>
        /// �����Ͳֿ��и���Ʒ��������
        /// </summary>
        public int InventoryItemAmount(string itemID, bool containStore = true)
        {
            int current = OCState.Inventory.GetItemAllNum(itemID);
            if (containStore)
            {
                foreach (var store in ManagerNS.LocalGameManager.Instance.StoreManager.GetStores())
                {
                    current += store.GetAmount(itemID, DataNS.DataOpType.Storage);
                }
            }
            return current;
        }

        public void InventoryAddItems(string itemID, int amount)
        {
            if (!string.IsNullOrEmpty(itemID) && amount > 0)
            {
                OCState.Inventory.AddItem(ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(itemID, amount));
            }
        }
        public void InventoryAddItems(Dictionary<string, int> itemDict)
        {
            if (itemDict != null)
            {
                List<ML.Engine.InventorySystem.Item> items = new List<ML.Engine.InventorySystem.Item>();
                foreach (var kv in itemDict)
                {
                    items.AddRange(ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(kv.Key, kv.Value));
                }
                OCState.Inventory.AddItem(items);
            }
        }
        public void InventoryRemoveItems(Dictionary<string, int> itemDict)
        {
            if (itemDict != null)
            {
                foreach (var kv in itemDict)
                {
                    OCState.Inventory.RemoveItem(kv.Key, kv.Value);
                }
            }
        }

        /// <summary>
        /// �ӱ����Ͳֿ������Ķ�Ӧ��������Ʒ���������ĵ�����
        /// </summary>
        /// <param name="containStore">�Ƿ�����ֿ�</param>
        /// <param name="needJudgeNum">�Ƿ���Ҫ�ж�����</param>
        /// <param name="priority">���ĵ����ȼ���0��ʾû�����ȼ���1��ʾ�Ӹ����ȼ����ģ�-1��ʾ�ӵ����ȼ�����</param>
        public int InventoryCostItems(string itemID, int amount, bool containStore = true, bool needJudgeNum = false, int priority = 0)
        {
            if (!needJudgeNum || InventoryHaveItem(itemID, amount, containStore))
            {
                int num = OCState.Inventory.GetItemAllNum(itemID);
                num = num <= amount ? num : amount;
                int current = OCState.Inventory.RemoveItem(itemID, num) ? num : 0;
                if (containStore && amount - current > 0)
                {
                    Dictionary<StoreNS.Store, int> dict = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutOutStore(itemID, amount - current, priority);
                    foreach (var kv in dict)
                    {
                        current += kv.Key.RemoveItem(itemID, kv.Value) ? kv.Value : 0;
                    }
                }
                return current;
            }
            return 0;
        }

        public Dictionary<string, int> InventoryCostItems(List<ML.Engine.InventorySystem.CompositeSystem.Formula> formulas, bool containStore = true, bool needJudgeNum = false, int priority = 0)
        {
            if (formulas == null)
            {
                return null;
            }
            if (!needJudgeNum || InventoryHaveItems(formulas, containStore))
            {
                Dictionary<string, int> needs = new Dictionary<string, int>();

                foreach (var formula in formulas)
                {
                    if (needs.ContainsKey(formula.id))
                    {
                        needs[formula.id] += formula.num;
                    }
                    else
                    {
                        needs[formula.id] = formula.num;
                    }
                }
                return InventoryCostItems(needs, containStore, needJudgeNum, priority);
            }
            return null;
        }

        public Dictionary<string, int> InventoryCostItems(Dictionary<string, int> needs, bool containStore = true, bool needJudgeNum = false, int priority = 0)
        {
            if (needs == null) { return null; }
            if (!needJudgeNum || InventoryHaveItems(needs, containStore))
            {
                Dictionary<string, int> results = new Dictionary<string, int>();
                foreach (var kv in needs)
                {
                    results.Add(kv.Key, InventoryCostItems(kv.Key, kv.Value, containStore, needJudgeNum, priority));
                }
                return results;
            }
            return null;
        }
        #endregion
    }
}

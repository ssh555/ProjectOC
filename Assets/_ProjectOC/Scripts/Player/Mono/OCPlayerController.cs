using System.Collections;
using System.Collections.Generic;
using ML.PlayerCharacterNS;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;

namespace ProjectOC.Player
{
    [System.Serializable]
    public class OCPlayerController : PlayerController
    {
        public PlayerCharacter currentCharacter = null;

        private List<string> ICharacterABResourcePath = new List<string>();
        public OCPlayerControllerState OCState =>State as OCPlayerControllerState;

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
        /// ��ȡ��ҵ�Inventory
        /// </summary>
        /// <param name="containStore">�Ƿ�����ֿ�</param>
        /// <param name="priority">���ĵ����ȼ���0��ʾû�����ȼ���1��ʾ�Ӹ����ȼ����ģ�-1��ʾ�ӵ����ȼ�����</param>
        public List<ML.Engine.InventorySystem.IInventory> GetInventorys(bool containStore = true, int priority = 0)
        {
            List<ML.Engine.InventorySystem.IInventory> result = new List<ML.Engine.InventorySystem.IInventory>();
            result.Add(OCState.Inventory);
            if (containStore)
            {
                var stores = ManagerNS.LocalGameManager.Instance.StoreManager.GetStores(priority);
                result.AddRange(stores);
            }
            return result;
        }

        /// <summary>
        /// �����Ͳֿ����Ƿ��ж�Ӧ��������Ʒ��
        /// </summary>
        public bool InventoryHasItems(string itemID, int amount, bool containStore = true)
        {
            int current = OCState.Inventory.GetItemAllNum(itemID);
            if (containStore && amount - current > 0)
            {
                Dictionary<StoreNS.Store, int> dict = ManagerNS.LocalGameManager.Instance.StoreManager.GetPutOutStore(itemID, amount - current, -1);
                foreach (var kv in dict)
                {
                    current += kv.Value;
                }
            }
            return current >= amount;
        }

        /// <summary>
        /// �����Ͳֿ����Ƿ��ж�Ӧ��������Ʒ��
        /// </summary>
        public bool InventoryHasItems(List<Formula> formulas, bool containStore = true)
        {
            if (formulas == null)
            {
                return false;
            }
            Dictionary<string, int> curs = new Dictionary<string, int>();
            Dictionary<string, int> needs = new Dictionary<string, int>();

            foreach (Formula formula in formulas)
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

            foreach (var kv in needs)
            {
                int num = OCState.Inventory.GetItemAllNum(kv.Key);
                num = num <= kv.Value ? num : kv.Value;
                curs[kv.Key] = num;
            }

            if (containStore)
            {
                List<StoreNS.Store> stores = ManagerNS.LocalGameManager.Instance.StoreManager.GetStores(-1);
                foreach (StoreNS.Store store in stores)
                {
                    foreach (var kv in needs)
                    {
                        int need = kv.Value - curs[kv.Key];
                        if (need > 0 && store.IsStoreHaveItem(kv.Key))
                        {
                            int num = store.GetDataNum(kv.Key, StoreNS.Store.DataType.Storage);
                            num = num <= need ? num : need;
                            curs[kv.Key] += num;
                        }
                    }
                }
            }

            foreach (var kv in needs)
            {
                if (curs[kv.Key] < kv.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public int InventoryItemAmount(string itemID, bool containStore = true)
        {
            int current = OCState.Inventory.GetItemAllNum(itemID);
            if (containStore)
            {
                foreach (var store in ManagerNS.LocalGameManager.Instance.StoreManager.GetStores(-1))
                {
                    current += store.GetDataNum(itemID, StoreNS.Store.DataType.Storage);
                }
            }
            return current;
        }

        /// <summary>
        /// �ӱ����Ͳֿ������Ķ�Ӧ��������Ʒ���������ĵ�����
        /// </summary>
        /// <param name="containStore">�Ƿ�����ֿ�</param>
        /// <param name="needJudgeNum">�Ƿ���Ҫ�ж�����</param>
        /// <param name="priority">���ĵ����ȼ���0��ʾû�����ȼ���1��ʾ�Ӹ����ȼ����ģ�-1��ʾ�ӵ����ȼ�����</param>
        public int InventoryCostItems(string itemID, int amount, bool containStore = true, bool needJudgeNum = false, int priority = 0)
        {
            if (!needJudgeNum || InventoryHasItems(itemID, amount, containStore))
            {
                int num = OCState.Inventory.GetItemAllNum(itemID);
                num = num <= amount ?  num : amount;
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

        public List<Formula> InventoryCostItems(List<Formula> formulas, bool containStore = true, bool needJudgeNum = false, int priority = 0)
        {
            if (formulas == null)
            {
                return null;
            }
            if (!needJudgeNum || InventoryHasItems(formulas, containStore))
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

                List<Formula> results = new List<Formula>();
                foreach (var kv in needs)
                {
                    int num = InventoryCostItems(kv.Key, kv.Value, containStore, needJudgeNum, priority);
                    results.Add(new Formula() { id = kv.Key, num = num });
                }
                return results;
            }
            return null;
        }
        #endregion
    }
}

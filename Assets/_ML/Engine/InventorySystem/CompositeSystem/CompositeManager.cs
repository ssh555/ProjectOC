using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ML.Engine.InventorySystem.CompositeSystem
{
    [System.Serializable]
    public class CompositionTableData
    {
        public string id;
        public string compositionid;
        public int compositionnum;
        public Formula[] formula;
        public TextContent.TextContent name;
        public string[] tag;
        public string texture2d;
        public List<string> usage;
    }

    [System.Serializable]
    public sealed class CompositeManager : Manager.LocalManager.ILocalManager
    {
        public CompositeManager() { }

        public static CompositeManager Instance { get { return instance; } }

        private static CompositeManager instance;
        public void OnRegister()
        {
            if (instance == null)
            {
                instance = this;
                LoadTableData();
            }
        }

        public void OnUnregister()
        {
            if(instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// �Ƿ��Ѽ���������
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;


        [System.Serializable]
        public enum CompositionObjectType
        {
            Error,
            Item,
            BuildingPart
        }


        public ML.Engine.ABResources.ABJsonAssetProcessor<CompositionTableData[]> ABJAProcessor;
        public void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<CompositionTableData[]>("OCTableData", "Composition", (datas) =>
                
            {
                foreach (var data in datas)
                {
                    if (data.id == null || data == null)
                    {
                        Debug.Log($"{data?.id} {data == null} {data.name}");
                    }
                    this.CompositeData.Add(data.id, data);
                }

                // to-do : ��ʱȡ��Usage����
                //// ���� Usage
                //foreach (var data in this.CompositeData.Values)
                //{
                //    if (data.formula != null)
                //    {
                //        foreach (var usage in data.formula)
                //        {
                //            if (this.CompositeData[usage.id].usage == null)
                //            {
                //                this.CompositeData[usage.id].usage = new List<string>();
                //            }
                //            this.CompositeData[usage.id].usage.Add(data.id);
                //        }

                //    }
                //}
            }, "�ϳ�ϵͳ������");
            ABJAProcessor.StartLoadJsonAssetData();
        }

        /// <summary>
        /// �ϳɱ�����
        /// </summary>
        private Dictionary<string, CompositionTableData> CompositeData = new Dictionary<string, CompositionTableData>();
        public CompositionTableData[] GetCompositionData()
        {
            return this.CompositeData.Values.ToArray();
        }

        /// <summary>
        /// �ܷ�ϳɶ�Ӧ��Ʒ
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CanComposite(IInventory resource, string id)
        {
            if (string.IsNullOrEmpty(id) || !this.CompositeData.ContainsKey(id) || this.CompositeData[id].formula == null)
            {
                return false;
            }
            foreach (var formula in this.CompositeData[id].formula)
            {
                // ��������
                if (resource.GetItemAllNum(formula.id) < formula.num)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanComposite(List<IInventory> resources, string id)
        {
            if (string.IsNullOrEmpty(id) || !this.CompositeData.ContainsKey(id) || this.CompositeData[id].formula == null || resources == null)
            {
                return false;
            }

            foreach (var formula in this.CompositeData[id].formula)
            {
                int curNum = 0;
                foreach (IInventory inventory in resources)
                {
                    curNum += inventory.GetItemAllNum(formula.id);
                    if (curNum >= formula.num)
                    {
                        break;
                    }
                }
                if (curNum < formula.num)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// �ϳ���Ʒ
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public CompositionObjectType Composite(IInventory resource, string id, out IComposition composition)
        {
            composition = null;
            // �Ƴ����ĵ���Դ
            OnlyCostResource(resource, id);
            return OnlyReturnComposition(id, out composition);
        }

        public CompositionObjectType OnlyReturnComposition(string id, out IComposition composition)
        {
            composition = null;
            // �� Item
            if (ItemManager.Instance.IsValidItemID(id))
            {
                Item item = ItemManager.Instance.SpawnItem(id);
                item.Amount = this.CompositeData[id].compositionnum;
                composition = item as IComposition;
                return CompositionObjectType.Item;
            }
            else if (ProjectOC.ManagerNS.LocalGameManager.Instance.RecipeManager.IsValidID(id))
            {
                Item item = ItemManager.Instance.SpawnItem(CompositeData[id].compositionid);
                item.Amount = this.CompositeData[id].compositionnum;
                composition = item as IComposition;
                return CompositionObjectType.Item;
            }
            // �� BuildingPart
            else if (BuildingSystem.BuildingManager.Instance.IsValidBPartID(id))
            {
                composition = BuildingSystem.BuildingManager.Instance.GetOneBPartInstance(id) as IComposition;
                return CompositionObjectType.BuildingPart;
            }
            return CompositionObjectType.Error;
        }

        /// <summary>
        /// ����Item �������ɺϳ���
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool OnlyCostResource(IInventory resource, string id)
        {
            // �Ƴ����ĵ���Դ
            if (!this.CanComposite(resource, id))
            {
                return false;
            }
            foreach (var formula in this.CompositeData[id].formula)
            {
                resource.RemoveItem(formula.id, formula.num);
            }
            return true;
        }
        public bool OnlyCostResource(List<IInventory> resources, string id)
        {
            if (!this.CanComposite(resources, id))
            {
                return false;
            }

            Dictionary<IInventory, List<Item>> dict = new Dictionary<IInventory, List<Item>>();
            foreach (var formula in this.CompositeData[id].formula)
            {
                int amount = formula.num;
                foreach (IInventory inventory in resources)
                {
                    if (amount <= 0)
                    {
                        break;
                    }
                    int num = inventory.GetItemAllNum(formula.id);
                    num = num <= amount ? num : amount;
                    num = inventory.RemoveItem(formula.id, num) ? num : 0;
                    amount -= num;
                    if (!dict.ContainsKey(inventory))
                    {
                        dict[inventory] = new List<Item>();
                    }
                    dict[inventory].AddRange(ItemManager.Instance.SpawnItems(formula.id, num));
                }
                if (amount > 0)
                {
                    foreach (var kv in dict)
                    {
                        kv.Key.AddItem(kv.Value);
                    }
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// ������Ҫ���ĵ���Դ
        /// </summary>
        public bool OnlyReturnResource(IInventory resource, string id)
        {
            // ������Ҫ���ĵ���Դ
            if (!string.IsNullOrEmpty(id) && this.CompositeData.ContainsKey(id) && this.CompositeData[id].formula != null)
            {
                foreach (var formula in this.CompositeData[id].formula)
                {
                    List<Item> items = ItemManager.Instance.SpawnItems(formula.id, formula.num);
                    foreach (var item in items)
                    {
                        resource.AddItem(item);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// ��ȡָ�� id �ɺϳ���Ʒ�� IDList
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string[] GetCompositionUsage(string id)
        {
            if (!this.CompositeData.ContainsKey(id))
                return null;
            return this.CompositeData[id].usage.ToArray();
        }

        public string[] GetCompositonTag(string id)
        {
            if (!this.CompositeData.ContainsKey(id))
                return null;
            return this.CompositeData[id].tag;
        }

        public string GetCompositonName(string id)
        {
            if (!this.CompositeData.ContainsKey(id))
                return null;
            return this.CompositeData[id].name;
        }

        public Formula[] GetCompositonFomula(string id)
        {
            if (!this.CompositeData.ContainsKey(id) || this.CompositeData[id].formula == null)
                return null;
            return this.CompositeData[id].formula;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    [System.Serializable]
    public class CompositionTableData
    {
        public string id;
        public int compositionnum;
        public Formula[] formula;
        public TextContent.TextContent name;
        public string[] tag;
        public string texture2d;
        public List<string> usage;
    }
    public sealed class CompositeManager : Manager.GlobalManager.IGlobalManager
    {
        private CompositeManager() { }

        private static CompositeManager instance;

        public static CompositeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CompositeManager();
                    Manager.GameManager.Instance.RegisterGlobalManager(instance);
                }
                return instance;
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
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<CompositionTableData[]>("OC/Json/TableData", "Composition", (datas) =>
                
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
        /// <param name="compositonID"></param>
        /// <returns></returns>
        public bool CanComposite(IInventory resource, string compositonID)
        {
            
            if (string.IsNullOrEmpty(compositonID) || !this.CompositeData.ContainsKey(compositonID) || this.CompositeData[compositonID].formula == null)
            {
                return false;
            }
            foreach(var formula in this.CompositeData[compositonID].formula)
            {
                // ��������
                if(resource.GetItemAllNum(formula.id) < formula.num)
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
        /// <param name="compositonID"></param>
        /// <returns></returns>
        public CompositionObjectType Composite(IInventory resource, string compositonID, out IComposition composition)
        {
            composition = null;

            // �Ƴ����ĵ���Դ
            lock (resource)
            {
                if (!this.CanComposite(resource, compositonID))
                {
                    return CompositionObjectType.Error;
                }
                foreach (var formula in this.CompositeData[compositonID].formula)
                {
                    resource.RemoveItem(formula.id, formula.num);
                }
            }

            // �� Item
            if (ItemManager.Instance.IsValidItemID(compositonID))
            {
                Item item = ItemManager.Instance.SpawnItem(compositonID);
                item.Amount = this.CompositeData[compositonID].compositionnum;
                composition = item as IComposition;
                return CompositionObjectType.Item;
            }
            // �� BuildingPart
            else if(BuildingSystem.BuildingManager.Instance.IsValidBPartID(compositonID))
            {
                composition = BuildingSystem.BuildingManager.Instance.GetOneBPartInstance(compositonID) as IComposition;
                return CompositionObjectType.BuildingPart;
            }
            return CompositionObjectType.Error;
        }
    
        /// <summary>
        /// ����Item �������ɺϳ���
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="compositonID"></param>
        /// <returns></returns>
        public bool OnlyCostResource(IInventory resource, string compositonID)
        {
            // �Ƴ����ĵ���Դ
            lock (resource)
            {
                if (!this.CanComposite(resource, compositonID))
                {
                    return false;
                }
                foreach (var formula in this.CompositeData[compositonID].formula)
                {
                    resource.RemoveItem(formula.id, formula.num);
                }
            }
            return true;
        }
        /// <summary>
        /// ������Ҫ���ĵ���Դ
        /// </summary>
        public bool OnlyReturnResource(IInventory resource, string compositonID)
        {
            // ������Ҫ���ĵ���Դ
            lock (resource)
            {
                if (!string.IsNullOrEmpty(compositonID) && this.CompositeData.ContainsKey(compositonID) && this.CompositeData[compositonID].formula != null)
                {
                    foreach (var formula in this.CompositeData[compositonID].formula)
                    {
                        List<Item> items = ItemManager.Instance.SpawnItems(formula.id, formula.num);
                        foreach (var item in items)
                        {
                            resource.AddItem(item);
                        }
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

        public Texture2D GetCompositonTexture2D(string id)
        {
            if (!this.CompositeData.ContainsKey(id))
            {
                return null;
            }
            return InventorySystem.ItemManager.Instance.GetItemTexture2D(this.CompositeData[id].texture2d);
        }

        public Sprite GetCompositonSprite(string id)
        {
            if (!this.CompositeData.ContainsKey(id))
            {
                return null;
            }
            return InventorySystem.ItemManager.Instance.GetItemSprite(this.CompositeData[id].texture2d);
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

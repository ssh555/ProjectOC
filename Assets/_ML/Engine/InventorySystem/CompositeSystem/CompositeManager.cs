using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    [System.Serializable]
    public class CompositionTableData
    {
        /// <summary>
        /// �ϳɶ��� -> Item | ������ ID ����
        /// </summary>
        public string id;

        /// <summary>
        /// �ϳ�������
        /// </summary>
        public TextContent.TextContent name;

        /// <summary>
        /// ��ǩ�ּ�
        /// 1��|2��|3��
        /// </summary>
        public string[] tag; // Category

        /// <summary>
        /// �ϳɹ�ʽ
        /// û�� num ��Ĭ��Ϊ 1
        /// num Ŀǰ���� item
        /// num = <1,2> | <1,2>
        /// 1 -> ID 
        /// 2 -> Num
        /// </summary>
        public Formula[] formula;
        /// <summary>
        /// һ�οɺϳ�����
        /// </summary>
        public int compositionnum;

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
            BuildingPart,
            Worker
        }



        public static ML.Engine.ABResources.ABJsonAssetProcessor<CompositionTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<CompositionTableData[]>("Binary/TableData", "CompositionTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
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
                }, null, "�ϳ�ϵͳ������");
                ABJAProcessor.StartLoadJsonAssetData();
            }
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
            if (!this.CompositeData.ContainsKey(compositonID) || this.CompositeData[compositonID].formula == null)
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

            return Manager.GameManager.Instance.ABResourceManager.LoadLocalAB(ItemManager.Texture2DPath).LoadAsset<Texture2D>(this.CompositeData[id].texture2d);
        }

        public Sprite GetCompositonSprite(string id)
        {
            var tex = this.GetCompositonTexture2D(id);
            if (tex == null)
            {
                return null;
            }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
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

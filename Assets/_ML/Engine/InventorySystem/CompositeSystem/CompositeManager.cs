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
        /// 合成对象 -> Item | 建筑物 ID 引用
        /// </summary>
        public string id;

        /// <summary>
        /// 合成物名称
        /// </summary>
        public TextContent.TextContent name;

        /// <summary>
        /// 标签分级
        /// 1级|2级|3级
        /// </summary>
        public string[] tag; // Category

        /// <summary>
        /// 合成公式
        /// 没有 num 则默认为 1
        /// num 目前仅限 item
        /// num = <1,2> | <1,2>
        /// 1 -> ID 
        /// 2 -> Num
        /// </summary>
        public Formula[] formula;
        /// <summary>
        /// 一次可合成数量
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
        /// 是否已加载完数据
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

                    // to-do : 暂时取消Usage功能
                    //// 加入 Usage
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
                }, null, "合成系统表数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }

        /// <summary>
        /// 合成表数据
        /// </summary>
        private Dictionary<string, CompositionTableData> CompositeData = new Dictionary<string, CompositionTableData>();
        public CompositionTableData[] GetCompositionData()
        {
            return this.CompositeData.Values.ToArray();
        }

        /// <summary>
        /// 能否合成对应物品
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
                // 数量不够
                if(resource.GetItemAllNum(formula.id) < formula.num)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 合成物品
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="compositonID"></param>
        /// <returns></returns>
        public CompositionObjectType Composite(IInventory resource, string compositonID, out IComposition composition)
        {
            composition = null;

            // 移除消耗的资源
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

            // 是 Item
            if (ItemManager.Instance.IsValidItemID(compositonID))
            {
                Item item = ItemManager.Instance.SpawnItem(compositonID);
                item.Amount = this.CompositeData[compositonID].compositionnum;
                composition = item as IComposition;
                return CompositionObjectType.Item;
            }
            // 是 BuildingPart
            else if(BuildingSystem.BuildingManager.Instance.IsValidBPartID(compositonID))
            {
                composition = BuildingSystem.BuildingManager.Instance.GetOneBPartInstance(compositonID) as IComposition;
                return CompositionObjectType.BuildingPart;
            }
            return CompositionObjectType.Error;
        }
    
        /// <summary>
        /// 消耗Item 但不生成合成物
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="compositonID"></param>
        /// <returns></returns>
        public bool OnlyCostResource(IInventory resource, string compositonID)
        {
            // 移除消耗的资源
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
        /// 获取指定 id 可合成物品的 IDList
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

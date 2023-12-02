using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    public sealed class CompositeSystem : Manager.GlobalManager.IGlobalManager
    {
        private CompositeSystem() { }

        private static CompositeSystem instance;

        public static CompositeSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CompositeSystem();
                    Manager.GameManager.Instance.RegisterGlobalManager(instance);
                }
                return instance;
            }
        }

        /// <summary>
        /// 合成表数据
        /// </summary>
        private Dictionary<string, CompositionJsonData> CompositeData = new Dictionary<string, CompositionJsonData>();
        public CompositionJsonData[] GetCompositionData()
        {
            return this.CompositeData.Values.ToArray();
        }


        [System.Serializable]
        public struct Formula
        {
            public string id;
            public int num;
        }
        [System.Serializable]
        public class CompositionJsonData
        {
            /// <summary>
            /// 合成对象 -> Item | 建筑物 ID 引用
            /// </summary>
            public string id;

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

        [System.Serializable]
        private struct CompositionJsonDatas
        {
            public CompositionJsonData[] table;
        }

        public const string CompositionTableDataABPath = "Json/TabelData";
        public const string TableName = "CompositionTableData";

        /// <summary>
        /// 载入合成表数据
        /// </summary>
        public IEnumerator LoadTableData(MonoBehaviour mono)
        {
            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(CompositionTableDataABPath, null, out ab);
            yield return crequest;
            ab = crequest.assetBundle;


            var request = ab.LoadAssetAsync<TextAsset>(TableName);
            yield return request;
            CompositionJsonDatas datas = JsonUtility.FromJson<CompositionJsonDatas>((request.asset as TextAsset).text);

            foreach(var data in datas.table)
            {
                this.CompositeData.Add(data.id, data);
            }

            // 加入 Usage
            foreach (var data in this.CompositeData.Values)
            {
                if (data.formula != null)
                {
                    foreach (var usage in data.formula)
                    {
                        if(this.CompositeData[usage.id].usage == null)
                        {
                            this.CompositeData[usage.id].usage = new List<string>();
                        }
                        this.CompositeData[usage.id].usage.Add(data.id);
                    }

                }
            }

            abmgr.UnLoadLocalABAsync(CompositionTableDataABPath, false, null);

        }

        /// <summary>
        /// 能否合成对应物品
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="compositonID"></param>
        /// <returns></returns>
        public bool CanComposite(Inventory resource, string compositonID)
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
        public IComposition Composite(Inventory resource, string compositonID)
        {
            // 移除消耗的资源
            lock (resource)
            {
                if (!this.CanComposite(resource, compositonID))
                {
                    return null;
                }
                foreach (var formula in this.CompositeData[compositonID].formula)
                {
                    resource.RemoveItem(formula.id, formula.num);
                }
            }


            // 是 Item
            if (ItemSpawner.Instance.IsValidItemID(compositonID))
            {
                Item item = ItemSpawner.Instance.SpawnItem(compositonID);
                item.Amount = this.CompositeData[compositonID].compositionnum;
                return item as IComposition;
            }
            return null;
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

            return Manager.GameManager.Instance.ABResourceManager.LoadLocalAB(ItemSpawner.Texture2DPath).LoadAsset<Texture2D>(this.CompositeData[id].texture2d);
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

        public Formula[] GetCompositonFomula(string id)
        {
            if (!this.CompositeData.ContainsKey(id) || this.CompositeData[id].formula == null)
                return null;
            return this.CompositeData[id].formula;
        }
    }



}

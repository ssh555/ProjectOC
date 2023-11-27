using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    /// <summary>
    /// 合成数据
    /// </summary>
    public struct CompositionData
    {
        /// <summary>
        /// 合成对象 -> Item | 建筑物 ID 引用
        /// </summary>
        public int ID;

        /// <summary>
        /// 标签分级
        /// 1级|2级|3级
        /// </summary>
        public string Tag; // Category

        /// <summary>
        /// 合成公式
        /// 没有 num 则默认为 1
        /// num 目前仅限 item
        /// num = <1,2> | <1,2>
        /// 1 -> ID 
        /// 2 -> Num
        /// </summary>
        public List<int[]> Formula;
        /// <summary>
        /// 一次可合成数量
        /// </summary>
        public int CompositionNum;

        /// <summary>
        /// 可合成目标
        /// </summary>
        public List<int> Usage;

        public Sprite sprite;
    }


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
        private Dictionary<int, CompositionData> CompositeData = new Dictionary<int, CompositionData>();
        public CompositionData[] GetCompositionData()
        {
            CompositionData[] ans = new CompositionData[this.CompositeData.Count];
            this.CompositeData.Values.CopyTo(ans, 0);
            return ans;
        }


        /// <summary>
        /// 载入合成表数据
        /// </summary>
        public IEnumerator LoadTableData(MonoBehaviour mono)
        {
            var datas = Utility.CSVUtils.ParseCSV("CSV/CompositeTableData", 3);

            Coroutine[] coroutines = new Coroutine[datas.Count];

            for (int i = 0; i < coroutines.Length; ++i)
            {
                coroutines[i] = mono.StartCoroutine(LoadRowData(datas[i]));
            }

            // to-do : 需优化资源加载
            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
            }

            // 加入 Usage
            foreach (var data in this.CompositeData.Values)
            {
                if (data.Formula != null)
                {
                    foreach (var usage in data.Formula)
                    {
                        this.CompositeData[usage[0]].Usage.Add(data.ID);
                    }

                }
            }
        }

        private IEnumerator LoadRowData(List<string> row)
        {
            CompositionData data = new CompositionData();
            // ID-> 0
            data.ID = int.Parse(row[0]);
            // Tag-> 1
            string[] tags = row[1].Split('|', StringSplitOptions.RemoveEmptyEntries);
            string tag = tags[0].Trim();
            for (int i = 1; i < tags.Length; ++i)
            {
                tag += '|' + tags[i].Trim();
            }
            data.Tag = tag;
            // Formula-> 2
            row[2] = (row[2].Trim()).Trim('\"');
            // 有合成数量
            if (row[2].Contains('='))
            {
                data.CompositionNum = int.Parse(row[2].Substring(0, row[2].IndexOf('=')).Trim());
#if UNITY_EDITOR
                if(data.CompositionNum < 1)
                {
                    Debug.LogError("CompositeTableData ID = " + data.ID + " : CompositionNum < 1");
                }
#endif
            }
            else
            {
                data.CompositionNum = 1;
            }
            string[] formulas = row[2].Split('|', StringSplitOptions.RemoveEmptyEntries);
            // 有合成公式
            if (formulas[0] != "null")
            {
                data.Formula = new List<int[]>();
                foreach (var formula in formulas)
                {
                    string[] kv = formula.Trim().Split('-');
                    int[] id_num = new int[2];
                    id_num[0] = int.Parse(kv[0].Trim());
                    id_num[1] = int.Parse(kv[1].Trim());
                    data.Formula.Add(id_num);
                }
            }

            // SpritePath => 3
            var request = Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<Texture2D>("ui/texture2d", row[3].Trim(), null);

            yield return request;
            Texture2D tex = request.asset as Texture2D;
            data.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            // Usage
            data.Usage = new List<int>();
            lock (this.CompositeData)
            {
                this.CompositeData.Add(data.ID, data);
            }
        }

        /// <summary>
        /// 能否合成对应物品
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="compositonID"></param>
        /// <returns></returns>
        public bool CanComposite(Inventory resource, int compositonID)
        {
            if (!this.CompositeData.ContainsKey(compositonID) || this.CompositeData[compositonID].Formula == null)
            {
                return false;
            }
            foreach(var formula in this.CompositeData[compositonID].Formula)
            {
                // 数量不够
                if(resource.GetItemAllNum(formula[0]) < formula[1])
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
        public IComposition Composite(Inventory resource, int compositonID)
        {
            // 移除消耗的资源
            lock (resource)
            {
                if (!this.CanComposite(resource, compositonID))
                {
                    return null;
                }
                foreach (var formula in this.CompositeData[compositonID].Formula)
                {
                    resource.RemoveItem(formula[0], formula[1]);
                }
            }


            // 是 Item
            if (ItemSpawner.Instance.IsValidItemID(compositonID))
            {
                Item item = ItemSpawner.Instance.SpawnItem(compositonID);
                item.Amount = this.CompositeData[compositonID].CompositionNum;
                return item as IComposition;
            }
            return null;
        }
    
        /// <summary>
        /// 获取指定 id 可合成物品的 IDList
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int[] GetCompositionUsage(int id)
        {
            if (!this.CompositeData.ContainsKey(id))
                return null;
            return this.CompositeData[id].Usage.ToArray();
        }

        public string GetCompositonTag(int id)
        {
            if (!this.CompositeData.ContainsKey(id))
                return null;
            return this.CompositeData[id].Tag;
        }

        public Sprite GetCompositonSprite(int id)
        {
            if (!this.CompositeData.ContainsKey(id))
                return null;
            return this.CompositeData[id].sprite;
        }

        public int[][] GetCompositonFomula(int id)
        {
            if (!this.CompositeData.ContainsKey(id) || this.CompositeData[id].Formula == null)
                return null;
            return this.CompositeData[id].Formula.ToArray();
        }
    }



}

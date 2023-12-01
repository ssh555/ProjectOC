using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    /// <summary>
    /// �ϳ�����
    /// </summary>
    public struct CompositionData
    {
        /// <summary>
        /// �ϳɶ��� -> Item | ������ ID ����
        /// </summary>
        public int ID;

        /// <summary>
        /// ��ǩ�ּ�
        /// 1��|2��|3��
        /// </summary>
        public string Tag; // Category

        /// <summary>
        /// �ϳɹ�ʽ
        /// û�� num ��Ĭ��Ϊ 1
        /// num Ŀǰ���� item
        /// num = <1,2> | <1,2>
        /// 1 -> ID 
        /// 2 -> Num
        /// </summary>
        public List<int[]> Formula;
        /// <summary>
        /// һ�οɺϳ�����
        /// </summary>
        public int CompositionNum;

        /// <summary>
        /// �ɺϳ�Ŀ��
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
        /// �ϳɱ�����
        /// </summary>
        private Dictionary<int, CompositionData> CompositeData = new Dictionary<int, CompositionData>();
        public CompositionData[] GetCompositionData()
        {
            CompositionData[] ans = new CompositionData[this.CompositeData.Count];
            this.CompositeData.Values.CopyTo(ans, 0);
            return ans;
        }


        [System.Serializable]
        public struct Formula
        {
            public int id;
            public int num;
        }
        [System.Serializable]
        public struct CompositionJsonData
        {
            /// <summary>
            /// �ϳɶ��� -> Item | ������ ID ����
            /// </summary>
            public int id;

            /// <summary>
            /// ��ǩ�ּ�
            /// 1��|2��|3��
            /// </summary>
            public string tag; // Category

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
        }

        [System.Serializable]
        private struct CompositionJsonDatas
        {
            public CompositionJsonData[] table;
        }

        public const string CompositionTableDataABPath = "Json/TabelData";
        public const string TableName = "CompositionTableData";

        /// <summary>
        /// ����ϳɱ�����
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


            Coroutine[] coroutines = new Coroutine[datas.table.Length];

            for (int i = 0; i < coroutines.Length; ++i)
            {
                coroutines[i] = mono.StartCoroutine(LoadRowData(datas.table[i]));
            }

            // to-do : ���Ż���Դ����
            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
            }

            // ���� Usage
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

            abmgr.UnLoadLocalABAsync(CompositionTableDataABPath, false, null);

        }

        private IEnumerator LoadRowData(CompositionJsonData row)
        {
            CompositionData data = new CompositionData();
            // ID-> 0
            data.ID = row.id;
            // Tag-> 1
            string[] tags = row.tag.Split('|', StringSplitOptions.RemoveEmptyEntries);
            string tag = tags[0].Trim();
            for (int i = 1; i < tags.Length; ++i)
            {
                tag += '|' + tags[i].Trim();
            }
            data.Tag = tag;
            // Formula-> 2
            foreach(var f in row.formula)
            {
                int[] d = new int[2];
                d[0] = f.id;
                d[1] = f.num;
                data.Formula.Add(d);
            }
            data.CompositionNum = row.compositionnum;

            // SpritePath => 3
            var request = Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<Texture2D>("ui/texture2d", row.texture2d.Trim(), null);

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
        /// �ܷ�ϳɶ�Ӧ��Ʒ
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
                // ��������
                if(resource.GetItemAllNum(formula[0]) < formula[1])
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
        public IComposition Composite(Inventory resource, int compositonID)
        {
            // �Ƴ����ĵ���Դ
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


            // �� Item
            if (ItemSpawner.Instance.IsValidItemID(compositonID))
            {
                Item item = ItemSpawner.Instance.SpawnItem(compositonID);
                item.Amount = this.CompositeData[compositonID].CompositionNum;
                return item as IComposition;
            }
            return null;
        }
    
        /// <summary>
        /// ��ȡָ�� id �ɺϳ���Ʒ�� IDList
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

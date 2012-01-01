using Newtonsoft.Json;
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
        /// �Ƿ��Ѽ���������
        /// </summary>
        public bool IsLoadOvered = false;


        [System.Serializable]
        public enum CompositionObjectType
        {
            Error,
            Item,
            BuildingPart,
        }

        [System.Serializable]
        public class CompositionJsonData
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

        public const string CompositionTableDataABPath = "Json/TableData";
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
            if(crequest != null)
                ab = crequest.assetBundle;


            var request = ab.LoadAssetAsync<TextAsset>(TableName);
            yield return request;
            CompositionJsonData[] datas = JsonConvert.DeserializeObject<CompositionJsonData[]>((request.asset as TextAsset).text);

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

            //abmgr.UnLoadLocalABAsync(CompositionTableDataABPath, false, null);

            IsLoadOvered = true;
        }



        /// <summary>
        /// �ϳɱ�����
        /// </summary>
        private Dictionary<string, CompositionJsonData> CompositeData = new Dictionary<string, CompositionJsonData>();
        public CompositionJsonData[] GetCompositionData()
        {
            return this.CompositeData.Values.ToArray();
        }

        /// <summary>
        /// �ܷ�ϳɶ�Ӧ��Ʒ
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
        public CompositionObjectType Composite(Inventory resource, string compositonID, out IComposition composition)
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
            if (ItemSpawner.Instance.IsValidItemID(compositonID))
            {
                Item item = ItemSpawner.Instance.SpawnItem(compositonID);
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
    
        public bool OnlyCostResource(Inventory resource, string compositonID)
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

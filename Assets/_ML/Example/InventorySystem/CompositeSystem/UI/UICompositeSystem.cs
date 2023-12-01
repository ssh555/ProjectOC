using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ML.Example.InventorySystem.CompositeSystem.UI
{
    /// <summary>
    /// �ϳ�ϵͳ��UIManager
    /// </summary>
    public class UICompositeSystem : MonoBehaviour
    {
        protected CompositeAbility owner;
        public CompositeAbility Owner
        {
            get => this.owner;
            set
            {
                this.owner = value;
            }
        }

        #region ClassificationContainer


        /// <summary>
        /// ��� ClassificationSelection �� parent
        /// </summary>
        [ShowInInspector, ReadOnly]
        protected RectTransform classificationContainer;
        
        /// <summary>
        /// ClassificationSelection ģ��
        /// </summary>
        [ShowInInspector, ReadOnly]
        protected ClassificationSelectionBtn classificationSelectionTemplate;

        protected Dictionary<string, ClassificationSelectionBtn> CSBtnDict = new Dictionary<string, ClassificationSelectionBtn>();

        public ClassificationSelectionBtn CreateAndAttachClassificationSelection(string tag)
        {
            ClassificationSelectionBtn ans = GameObject.Instantiate<ClassificationSelectionBtn>(this.classificationSelectionTemplate, this.classificationContainer);
            ans.gameObject.SetActive(true);
            ans.PrimaryTag = tag;
            this.CSBtnDict.Add(tag, ans);
            return ans;
        }

        #endregion

        #region CatalogueContainer
        /// <summary>
        /// ��� CompositionCatalogue �� parent
        /// </summary>
        [ShowInInspector, ReadOnly]
        protected RectTransform catalogueContainer;

        /// <summary>
        /// CompositionSelectionTemplate ģ��
        /// </summary>
        [ShowInInspector, ReadOnly]
        protected CompositionSelectionBtn compositionSelectionTemplate;

        /// <summary>
        /// CompositionDescribeTemplate ģ��
        /// </summary>
        [ShowInInspector, ReadOnly]
        protected Text compositionDescribeTemplate;

        public CompositionSelectionBtn CreateAndAttachCompositionSelection(int id)
        {
            CompositionSelectionBtn ans = GameObject.Instantiate<CompositionSelectionBtn>(this.compositionSelectionTemplate, this.catalogueContainer);
            ans.gameObject.SetActive(true);
            ans.ID = id;

            if (ans.ID == this.previewPanel.ID)
            {
                ans.SetActived(ans, typeof(CompositionSelectionBtn));
            }

            ans.OnActiveListener += (pre, post) =>
            {
                this.previewPanel.ID = (post as CompositionSelectionBtn).ID;
            };
            return ans;
        }
        
        public Text CreateAndAttachCompositionDescribe(string description)
        {
            Text ans = GameObject.Instantiate<Text>(this.compositionDescribeTemplate, this.catalogueContainer);
            ans.gameObject.SetActive(true);
            ans.text = description;
            return ans;
        }
        #endregion

        #region PreviewPanel
        protected PreviewPanel previewPanel;
        #endregion

        /// <summary>
        /// <PrimaryTag, <Tag, IDList>>
        /// </summary>
        protected Dictionary<string, Dictionary<string, List<int>>> CatalogueDatas = new Dictionary<string, Dictionary<string, List<int>>>();

        private void Awake()
        {
            #region InitData
            var datas = ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.GetCompositionData();
            foreach (var data in datas)
            {
                string[] tags = data.tag;
                // 1����ǩ
                if (!this.CatalogueDatas.ContainsKey(tags[0]))
                {
                    this.CatalogueDatas.Add(tags[0], new Dictionary<string, List<int>>());
                }
                // 2����ǩ
                if (!this.CatalogueDatas[tags[0]].ContainsKey(tags[1]))
                {
                    this.CatalogueDatas[tags[0]].Add(tags[1], new List<int>());
                }
                // ��������
                this.CatalogueDatas[tags[0]][tags[1]].Add(data.id);
            }
            #endregion


            #region ClassificationContainer & CatalogueContainer
            Transform _DirectoryPanel = this.transform.Find("DirectoryPanel");

            this.classificationContainer = _DirectoryPanel.Find("ClassificationContainer").GetChild(0).GetChild(0) as RectTransform;
            this.classificationSelectionTemplate = this.classificationContainer.GetChild(0).GetComponent<ClassificationSelectionBtn>();

            this.catalogueContainer = _DirectoryPanel.Find("CatalogueContainer").GetChild(0).GetChild(0) as RectTransform;
            this.compositionSelectionTemplate = this.catalogueContainer.Find("CompositionSelectionTemplate").GetComponent<CompositionSelectionBtn>();
            this.compositionDescribeTemplate = this.catalogueContainer.Find("CompositionDescribeTemplate").GetComponent<Text>();
            #endregion

            #region PreviewPanel
            this.previewPanel = this.transform.Find("PreviewPanel").GetComponent<PreviewPanel>();
            this.previewPanel.Owner = this;
            #endregion


            #region ClassificationSelection ��ʼ��
            foreach (string tag in this.CatalogueDatas.Keys)
            {
                var value = this.CreateAndAttachClassificationSelection(tag);
                value.OnActiveListener += RefreshCatalogueContainer;
                value.CataloguePair = this.CatalogueDatas[tag];
            }
            #endregion
        }

        private void Start()
        {
            this.owner.ResourceInventory.OnItemListChanged += OnInventoryChanged;
        }

        private void OnDestroy()
        {
            this.owner.ResourceInventory.OnItemListChanged -= OnInventoryChanged;
        }

        private void OnInventoryChanged(Inventory inv)
        {
            this.previewPanel.RefreshPreviewPanel();
        }

        protected void RefreshCatalogueContainer(ML.Example.UI.SingletonActivedButton pre, ML.Example.UI.SingletonActivedButton post)
        {
            foreach (Transform child in this.catalogueContainer)
            {
                // ��ɾ��ģ��
                if (child == this.compositionDescribeTemplate.transform || child == this.compositionSelectionTemplate.transform) continue;
                Destroy(child.gameObject);
            }
            
            foreach(var kv in (post as ClassificationSelectionBtn).CataloguePair)
            {
                this.CreateAndAttachCompositionDescribe(kv.Key);
                foreach(var id in kv.Value)
                {
                    this.CreateAndAttachCompositionSelection(id);
                }
            }
        }
    
        /// <summary>
        /// �����Ӧ ID �ĺϳ���Ʒ
        /// </summary>
        /// <param name="id"></param>
        public void ActiveCompositionID(int id)
        {
            this.previewPanel.ID = id;

            string[] tags = ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.GetCompositonTag(id);
            // 1����ǩ -> �ҵ���Ӧ �������
            var csbtn = this.CSBtnDict[tags[0]];
            csbtn.SetActived(null, typeof(ClassificationSelectionBtn));
            csbtn.SetActived(csbtn, typeof(ClassificationSelectionBtn));

            // 2����ǩ -> �ҵ���Ӧ�ĺϳ���

        }
    
    
    }
}

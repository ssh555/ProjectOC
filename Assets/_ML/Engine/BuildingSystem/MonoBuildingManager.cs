using System.Collections.Generic;
using UnityEngine;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.UI;
using Sirenix.OdinInspector;
using ML.Engine.TextContent;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
namespace ML.Engine.BuildingSystem
{

    [System.Serializable]
    public class MonoBuildingManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public void OnRegister()
        {
            Init();
        }

        #region Property|Field
        [SerializeField]
        public BuildingManager BM;
        private int IsInit = 5;
        /// <summary>
        /// 是否初始化完成所有数据的载入
        /// </summary>
        public bool IsLoadOvered => IsInit == 0;
        #endregion

        #region TextContent

        #region Category|Type
        public Dictionary<string, TextContent.TextTip> Category1Dict = new Dictionary<string, TextContent.TextTip>();
        public Dictionary<string, TextContent.TextTip> Category2Dict = new Dictionary<string, TextContent.TextTip>();

        #endregion
       
        public void InitUITextContents()
        {
            var Category_ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TextTip[]>("OCTextContent/BuildingSystem/UI", "Category", (datas) =>
            {
                foreach (var category in datas)
                {
                    this.Category1Dict.Add(category.name, category);
                }
                IsInit--;
            }, "建造系统Category1");
            Category_ABJAProcessor.StartLoadJsonAssetData();

            var Type_ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TextTip[]>("OCTextContent/BuildingSystem/UI", "Type", (datas) =>
            {
                foreach (var type in datas)
                {
                    this.Category2Dict.Add(type.name, type);
                }
                IsInit--;
            }, "建造系统Category2");
            Type_ABJAProcessor.StartLoadJsonAssetData();
        }

        #endregion

        #region Internal
        public const string UIPanelABPath = "Prefab_UIPanel";

        public const string BPartABPath = "BS_Prefab_BuildingPart";

        private RectTransform Canvas { get => Manager.GameManager.Instance.UIManager.NormalPanel.GetComponent<RectTransform>(); }
        public async UniTask<T> GetPanel<T>() where T : UIBasePanel
        {
            string _path = UIPanelABPath + "/Prefab_BS_" + typeof(T).Name + ".prefab";
            var handle = Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(_path);
            await handle.Task;
            T panel = handle.Result.GetComponent<T>();
            return panel;
        }

        public async void PushPanel<T>() where T : UIBasePanel
        {
            var panel = await this.GetPanel<T>();
            Manager.GameManager.Instance.UIManager.PushPanel(panel);
            panel.transform.SetParent(this.Canvas, false);
        }

        public void PopPanel()
        {
            Manager.GameManager.Instance.UIManager.PopPanel();
        }

        public async void PopAndPushPanel<T>() where T : UIBasePanel
        {
            var panel = await this.GetPanel<T>();
            Manager.GameManager.Instance.UIManager.PopPanel();
            Manager.GameManager.Instance.UIManager.PushPanel(panel);
            panel.transform.SetParent(this.Canvas, false);
        }

        private UIBasePanel GetPeekPanel()
        {
            return Manager.GameManager.Instance.UIManager.GetTopUIPanel();
        }

        public void Init()
        {
            ML.Engine.Manager.GameManager.Instance.RegisterLocalManager(BM);
            RegisterBPartPrefab();
            InitUITextContents();
        }
        [ShowInInspector]
        public Dictionary<BuildingPartClassification, IBuildingPart> LoadedBPart = new Dictionary<BuildingPartClassification, IBuildingPart>();

        private AsyncOperationHandle BPartHandle;
        private void RegisterBPartPrefab()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<GameObject>(BPartABPath, (obj) =>
            {
                lock(this.LoadedBPart)
                {
                    var bpart = obj.GetComponent<IBuildingPart>();
                    if (bpart != null)
                    {
                        if(!this.LoadedBPart.TryAdd(bpart.Classification, bpart))
                        {
                            Debug.LogError($"{bpart.Classification.ToString()} 重复配置: {bpart.gameObject.name}");
                        }

                        // to-do : 仅测试用，后续接入科技树后需注释掉
#if UNITY_EDITOR||DEVELOPMENT_BUILD
                        if (IsAddBPartOnRegister)
                        {
                            BM.RegisterBPartPrefab(bpart);
                        }
#endif
                    }
                }

            }).Completed += (handle) =>
            {
                BPartHandle = handle;
                IsInit--;
            };
        }

        public void OnUnregister()
        {
            if (Manager.GameManager.Instance != null)
                Manager.GameManager.Instance.ABResourceManager.Release(BPartHandle);
            if(this.BM != null)
            {
                this.BM.UnregisterAllBPartPrefab();
            }
        }
        #endregion

#if UNITY_EDITOR||DEVELOPMENT_BUILD
        [LabelText("加载建筑物时是否标记为可用"), SerializeField]
        private bool IsAddBPartOnRegister = true;
#endif

        ///// <summary>
        ///// TODO : 正式打包需要删掉
        ///// </summary>
        //public void AddAllBPartPrefab()
        //{

        //}
    }

}

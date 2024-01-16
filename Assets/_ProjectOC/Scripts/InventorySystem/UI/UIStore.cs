using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ProjectOC.StoreNS;
using ProjectOC.TechTree.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.tvOS;
using UnityEngine.UI;

namespace ProjectOC.InventorySystem.UI
{
    public class UIStore : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            InitUITextContents();

            #region TopTitle
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            KT_ChangeStoreData = new UIKeyTip();
            KT_ChangeStoreData.img = transform.Find("TopTitle").Find("KT_ChangeStoreData").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_ChangeStoreData.keytip = KT_ChangeStoreData.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();

            #region Priority
            Transform priority = transform.Find("TopTitle").Find("Priority");
            Text_Priority = priority.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            KT_NextPriority = new UIKeyTip();
            KT_NextPriority.img = priority.Find("KT_NextPriority").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_NextPriority.keytip = KT_NextPriority.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            PriorityUrgency = priority.Find("Urgency");
            PriorityNormal = priority.Find("Normal");
            PriorityAlternative = priority.Find("Alternative");
            #endregion
            #endregion

            #region Store
            Transform content = transform.Find("Store").Find("Viewport").Find("Content");
            StoreGridLayout = content.GetComponent<GridLayoutGroup>();
            Transform StoreDataTemplate = content.Find("UIStoreDataTemplate");
            StoreDataTemplate.gameObject.SetActive(false);
            #endregion

            #region BotKeyTips
            Transform kt = this.transform.Find("BotKeyTips").Find("KeyTips");
            KT_Remove1 = new UIKeyTip();
            KT_Remove1.img = kt.Find("KT_Remove1").Find("Image").GetComponent<Image>();
            KT_Remove1.keytip = KT_Remove1.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Remove1.description = KT_Remove1.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Remove10 = new UIKeyTip();
            KT_Remove10.img = kt.Find("KT_Remove10").Find("Image").GetComponent<Image>();
            KT_Remove10.keytip = KT_Remove10.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Remove10.description = KT_Remove10.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_FastAdd = new UIKeyTip();
            KT_FastAdd.img = kt.Find("KT_FastAdd").Find("Image").GetComponent<Image>();
            KT_FastAdd.keytip = KT_FastAdd.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_FastAdd.description = KT_FastAdd.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();
            #endregion

            CurPriority = MissionNS.TransportPriority.Normal;
            Store.OnStoreDataChange += Refresh;
            IsInit = true;
            Refresh();
        }
        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.Enter();
        }

        public override void OnExit()
        {
            base.OnExit();
            this.Exit();
            ClearTemp();
        }

        public override void OnPause()
        {
            base.OnPause();
            this.Exit();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.Enter();
        }
        #endregion

        #region Internal
        /// <summary>
        /// 对应的逻辑仓库
        /// </summary>
        public Store Store;
        /// <summary>
        /// 当前的Priority
        /// </summary>
        private MissionNS.TransportPriority curPriority;
        /// <summary>
        /// 封装的Priority，便于在更新值时一并更新其他数据并Refresh
        /// </summary>
        private MissionNS.TransportPriority CurPriority
        {
            get => curPriority;
            set
            {
                if (Priority != null)
                {
                    Priority.Find("Selected").gameObject.SetActive(false);
                }
                curPriority = value;
                switch (curPriority)
                {
                    case MissionNS.TransportPriority.Urgency:
                        Priority = PriorityUrgency;
                        Text_Priority.text = PanelTextContent.text_PriorityUrgency.GetText();
                        break;
                    case MissionNS.TransportPriority.Normal:
                        Priority = PriorityNormal;
                        Text_Priority.text = PanelTextContent.text_PriorityNormal.GetText();
                        break;
                    case MissionNS.TransportPriority.Alternative:
                        Priority = PriorityAlternative;
                        Text_Priority.text = PanelTextContent.text_PriorityAlternative.GetText();
                        break;
                }
                Priority.Find("Selected").gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// UIPanel.Store区域控制处理的StoreData列表
        /// </summary>
        [ShowInInspector]
        private List<StoreData> StoreDatas = new List<StoreData>();
        /// <summary>
        /// 上一次选中的DataIndex，用于移动滑动窗口
        /// </summary>
        private int lastDataIndex = 0;
        /// <summary>
        /// 当前选中的DataIndex
        /// </summary>
        private int currentDataIndex = 0;
        /// <summary>
        /// 封装，方便更新数据和Refresh
        /// </summary>
        private int CurrentDataIndex
        {
            get => currentDataIndex;
            set
            {
                int last = currentDataIndex;
                if(StoreDatas.Count > 0)
                {
                    currentDataIndex = value;
                    if(currentDataIndex == -1)
                    {
                        currentDataIndex = StoreDatas.Count - 1;
                    }
                    else if(currentDataIndex == StoreDatas.Count)
                    {
                        currentDataIndex = 0;
                    }
                    else
                    {
                        var grid = StoreGridLayout.GetGridSize();
                        if (currentDataIndex < 0)
                        {
                            currentDataIndex += (grid.x * grid.y);
                        }
                        else if (currentDataIndex >= StoreDatas.Count)
                        {
                            currentDataIndex -= (grid.x * grid.y);
                            if (currentDataIndex < 0)
                            {
                                currentDataIndex += grid.y;
                            }
                        }
                        // 不计算隐藏的模板
                        while (this.currentDataIndex >= StoreDatas.Count)
                        {
                            this.currentDataIndex -= grid.y;
                        }
                    }
                }
                else
                {
                    currentDataIndex = 0;
                }
                if(last != currentDataIndex)
                {
                    lastDataIndex = last;
                }
                this.Refresh();
            }
        }
        /// <summary>
        /// 当前选中的StoreData
        /// </summary>
        private StoreData CurrentStoreData
        {
            get
            {
                if(CurrentDataIndex < StoreDatas.Count)
                {
                    return StoreDatas[CurrentDataIndex];
                }
                return null;
            }
        }
        private Player.PlayerCharacter Player => GameObject.Find("PlayerCharacter")?.GetComponent<Player.PlayerCharacter>();

        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Enable();
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(0);
            Store.IsInteracting = true;
            this.Refresh();
        }

        private void Exit()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIInventory.Disable();
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(1);
            Store.IsInteracting = false;
            this.UnregisterInput();
        }

        private void UnregisterInput()
        {
            // 切换Priority
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed -= NextPriority_performed;
            // 上下切换StoreData
            ProjectOC.Input.InputManager.PlayerInput.UIStore.AlterStoreData.performed -= AlterStoreData_performed;
            // 改变StoreData存储的Item
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeStoreData.performed -= ChangeStoreData_performed;
            // 快捷放入
            ProjectOC.Input.InputManager.PlayerInput.UIStore.FastAdd.performed -= FastAdd_performed;
            // 取出1个
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove1.performed -= Remove1_performed;
            // 取出10个
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove10.performed -= Remove10_performed;
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        private void RegisterInput()
        {
            // 切换Priority
            ProjectOC.Input.InputManager.PlayerInput.UIStore.NextPriority.performed += NextPriority_performed;
            // 上下切换StoreData
            ProjectOC.Input.InputManager.PlayerInput.UIStore.AlterStoreData.performed += AlterStoreData_performed;
            // 改变StoreData存储的Item
            ProjectOC.Input.InputManager.PlayerInput.UIStore.ChangeStoreData.performed += ChangeStoreData_performed;
            // 快捷放入
            ProjectOC.Input.InputManager.PlayerInput.UIStore.FastAdd.performed += FastAdd_performed;
            // 取出1个
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove1.performed += Remove1_performed;
            // 取出10个
            ProjectOC.Input.InputManager.PlayerInput.UIStore.Remove10.performed += Remove10_performed;
            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }

        private void NextPriority_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            MissionNS.TransportPriority temp = CurPriority;
            switch (temp)
            {
                case MissionNS.TransportPriority.Urgency:
                    CurPriority = MissionNS.TransportPriority.Normal;
                    break;
                case MissionNS.TransportPriority.Normal:
                    CurPriority = MissionNS.TransportPriority.Alternative;
                    break;
                case MissionNS.TransportPriority.Alternative:
                    CurPriority = MissionNS.TransportPriority.Urgency;
                    break;
            }
        }
        private void AlterStoreData_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var f_offset = obj.ReadValue<Vector2>();
            var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
            var grid = StoreGridLayout.GetGridSize();
            this.CurrentDataIndex += -offset.y * grid.y + offset.x;
        }
        private void ChangeStoreData_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            // TODO: 打开UI界面
            //var panel = GameObject.Instantiate(uITechPointPanel);
            //panel.transform.SetParent(this.transform.parent, false);
            //panel.inventory = this.player.Inventory;
            //ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
            //var changePanel = Instantiate(StoreDataTemplate, StoreGridLayout.transform, false);
        }
        private void Add1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Store.UIAdd(Player, CurrentStoreData, 1);
            Refresh();
        }
        private void Add10_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Store.UIAdd(Player, CurrentStoreData, 10);
            Refresh();
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Store.UIFastAdd(Player, CurrentStoreData);
            Refresh();
        }
        private void Remove1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Store.UIRemove(Player, CurrentStoreData, 1);
            Refresh();
        }
        private void Remove10_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Store.UIRemove(Player, CurrentStoreData, 10);
            Refresh();
        }
        private void FastRemove_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Store.UIFastRemove(Player, CurrentStoreData);
            Refresh();
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }
        #endregion

        #region UI
        #region Temp
        private List<Sprite> tempSprite = new List<Sprite>();
        private List<GameObject> tempUIStoreDatas = new List<GameObject>();
        private void ClearTemp()
        {
            foreach(var s in tempSprite)
            {
                Destroy(s);
            }
            foreach (var s in tempUIStoreDatas)
            {
                Destroy(s);
            }
        }
        #endregion

        #region UI对象引用
        private TMPro.TextMeshProUGUI Text_Title;
        private TMPro.TextMeshProUGUI Text_Priority;

        private UIKeyTip KT_NextPriority;
        private UIKeyTip KT_ChangeStoreData;
        private UIKeyTip KT_Remove1;
        private UIKeyTip KT_Remove10;
        private UIKeyTip KT_FastAdd;

        private Transform StoreDataTemplate;
        private GridLayoutGroup StoreGridLayout;
        private Transform Priority;
        private Transform PriorityUrgency;
        private Transform PriorityNormal;
        private Transform PriorityAlternative;
        #endregion

        public void Refresh()
        {
            // 加载完成JSON数据 & 查找完所有引用
            if(ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
            {
                return;
            }
            StoreDatas = Store.StoreDatas;
            #region TopTitle
            Text_Title.text = PanelTextContent.text_Title.GetText();
            KT_ChangeStoreData.ReWrite(PanelTextContent.kt_ChangeStoreData);
            KT_NextPriority.ReWrite(PanelTextContent.kt_NextPriority);
            #endregion

            #region Store
            // 临时内存生成的UIStoreData数量(只增不减，多的隐藏掉即可) - 当前筛选出来的UIStoreData数量
            int delta = tempUIStoreDatas.Count - StoreDatas.Count;
            // > 0 => 有多余，隐藏
            if(delta > 0)
            {
                for(int i = 0; i < delta; ++i)
                {
                    tempUIStoreDatas[tempUIStoreDatas.Count - 1 - i].SetActive(false);
                }
            }
            // < 0 => 不够， 增加
            else if (delta < 0)
            {
                delta = -delta;
                for (int i = 0; i < delta; ++i)
                {
                    var uiitem = Instantiate(StoreDataTemplate, StoreGridLayout.transform, false);
                    tempUIStoreDatas.Add(uiitem.gameObject);
                }
            }

            // 用于更新滑动窗口
            // 当前选中的UIStoreData
            GameObject cur = null;
            // 上一个UIStoreData
            GameObject last = null;

            // 遍历筛选的StoreDataList
            for (int i = 0; i < StoreDatas.Count; ++i)
            {
                var uiStoreData = tempUIStoreDatas[i];
                var storeData = StoreDatas[i];
                // Active
                uiStoreData.SetActive(true);
                // 更新Icon
                var img = uiStoreData.transform.Find("Icon").GetComponent<Image>();
                // 查找临时存储的Sprite
                var sprite = tempSprite.Find(s => s.texture == ManagerNS.LocalGameManager.Instance.StoreManager.GetTexture2D());
                // 不存在则生成
                if (sprite == null)
                {
                    sprite = ManagerNS.LocalGameManager.Instance.StoreManager.GetSprite();
                    tempSprite.Add(sprite);
                }
                img.sprite = sprite;
                // Name
                var nametext = uiStoreData.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
                nametext.text = ItemManager.Instance.GetItemName(storeData.ItemID);
                // Amount
                int amount = storeData.StorageAll;
                int amountMax = storeData.MaxCapacity;
                var AmountCur = uiStoreData.transform.Find("Amount").Find("Cur").GetComponent<TMPro.TextMeshProUGUI>();
                var AmountMax = uiStoreData.transform.Find("Amount").Find("Max").GetComponent<TMPro.TextMeshProUGUI>();
                AmountCur.text = amount.ToString();
                AmountMax.text = amountMax.ToString();
                // ProgressBar
                int level = Store.Level;
                int amountEachLevel = amountMax / (level + 1);
                Transform progressBar1 = uiStoreData.transform.Find("ProgressBar1");
                RectTransform progressBarRectCur1 = progressBar1.Find("Cur").GetComponent<RectTransform>();
                RectTransform progressBarRectMax1 = progressBar1.Find("Max").GetComponent<RectTransform>();
                Transform progressBar2 = uiStoreData.transform.Find("ProgressBar2");
                RectTransform progressBarRectCur2 = progressBar2.Find("Cur").GetComponent<RectTransform>();
                RectTransform progressBarRectMax2 = progressBar2.Find("Max").GetComponent<RectTransform>();
                Transform progressBar3 = uiStoreData.transform.Find("ProgressBar3");
                RectTransform progressBarRectCur3 = progressBar3.Find("Cur").GetComponent<RectTransform>();
                RectTransform progressBarRectMax3 = progressBar3.Find("Max").GetComponent<RectTransform>();
                if (level == 0)
                {
                    progressBar1.Find("None").gameObject.SetActive(false);
                    progressBar2.Find("None").gameObject.SetActive(true);
                    progressBar3.Find("None").gameObject.SetActive(true);
                }
                else if (level == 1)
                {
                    progressBar1.Find("None").gameObject.SetActive(false);
                    progressBar2.Find("None").gameObject.SetActive(false);
                    progressBar3.Find("None").gameObject.SetActive(true);
                }
                else if(level == 2)
                {
                    progressBar1.Find("None").gameObject.SetActive(false);
                    progressBar2.Find("None").gameObject.SetActive(false);
                    progressBar3.Find("None").gameObject.SetActive(false);
                }
                float width1 = amount >= amountEachLevel ? 1 : amount / amountEachLevel;
                float width2 = amount >= 2 * amountEachLevel ? 1 : amount / amountEachLevel - 1;
                width2 = width2 >= 0 ? width2 : 0;
                float width3 = amount >= 3 * amountEachLevel ? 1 : amount / amountMax - 2;
                width3 = width3 >= 0 ? width3 : 0;
                progressBarRectCur1.sizeDelta = new Vector2(progressBarRectMax1.sizeDelta.x * width1, progressBarRectCur1.sizeDelta.y);
                progressBarRectCur2.sizeDelta = new Vector2(progressBarRectMax2.sizeDelta.x * width2, progressBarRectCur2.sizeDelta.y);
                progressBarRectCur3.sizeDelta = new Vector2(progressBarRectMax3.sizeDelta.x * width3, progressBarRectCur3.sizeDelta.y);

                // Add and Remove
                uiStoreData.transform.Find("Add").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_Add.GetText();
                uiStoreData.transform.Find("Remove").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.text_Remove.GetText();
                // Selected
                var selected = uiStoreData.transform.Find("Selected");
                if(CurrentStoreData == StoreDatas[i])
                {
                    selected.gameObject.SetActive(true);
                    cur = uiStoreData;
                }
                else
                {
                    selected.gameObject.SetActive(false);
                }
                if(i == lastDataIndex)
                {
                    last = uiStoreData;
                }
            }

            #region 更新滑动窗口
            if(cur != null && last != null)
            {
                // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                // 获取 ScrollRect 组件
                ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                // 获取 Content 的 RectTransform 组件
                RectTransform contentRect = scrollRect.content;

                // 获取 UI 元素的四个角点
                Vector3[] corners = new Vector3[4];
                uiRectTransform.GetWorldCorners(corners);
                bool allCornersVisible = true;
                for (int i = 0; i < 4; ++i)
                {
                    // 将世界空间的点转换为屏幕空间的点
                    Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                    // 判断 ScrollRect 是否包含这个点
                    if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                    {
                        allCornersVisible = false;
                        break;
                    }
                }
                
                // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                if (!allCornersVisible)
                {
                    // 将当前选中的这个放置于上一个激活TP的位置

                    // 设置滑动位置

                    // 获取点 A 和点 B 在 Content 中的位置
                    Vector2 positionA = (last.transform as RectTransform).anchoredPosition;
                    Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;

                    // 计算点 B 相对于点 A 的偏移量
                    Vector2 offset = positionB - positionA;

                    // 根据偏移量更新 ScrollRect 的滑动位置
                    Vector2 normalizedPosition = scrollRect.normalizedPosition;
                    normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                    scrollRect.normalizedPosition = normalizedPosition;
                }
            }
            else
            {
                StoreGridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
            }
            #endregion
            
            // 强制立即更新 VerticalLayoutGroup 的布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(StoreGridLayout.GetComponent<RectTransform>());
            #endregion

            #region BotKeyTips
            KT_Remove1.ReWrite(PanelTextContent.kt_Remove1);
            KT_Remove10.ReWrite(PanelTextContent.kt_Remove10);
            KT_FastAdd.ReWrite(PanelTextContent.kt_FastAdd);
            #endregion
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct StorePanel
        {
            public TextContent text_Title;
            public TextContent text_PriorityUrgency;
            public TextContent text_PriorityNormal;
            public TextContent text_PriorityAlternative;
            public TextContent text_Add;
            public TextContent text_Remove;

            public KeyTip kt_NextPriority;
            public KeyTip kt_ChangeStoreData;
            public KeyTip kt_Remove1;
            public KeyTip kt_Remove10;
            public KeyTip kt_FastAdd;
        }

        public static StorePanel PanelTextContent => ABJAProcessor.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<StorePanel> ABJAProcessor;

        private void InitUITextContents()
        {
            if(ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<StorePanel>("Binary/TextContent/Store", "StorePanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI仓库Panel数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion
    }
}

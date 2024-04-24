using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.InventorySystem.UI.UIRestaurant;
using ML.Engine.UI;
using ProjectOC.RestaurantNS;
using System.Linq;


namespace ProjectOC.InventorySystem.UI
{
    public class UIRestaurant : UIBasePanel<RestaurantPanel>
    {
        #region 数据
        public enum Mode
        {
            Restaurant = 0,
            ChangeFood = 1,
        }
        public Mode CurMode = Mode.Restaurant;

        public Restaurant Restaurant;

        private TMPro.TextMeshProUGUI Text_Title;
        private Sprite EmptySprite;
        private Transform Food_Desc;
        public Transform RestaurantUITransform;
        public Transform ChangeFoodUITransform;
        private Transform BotKeyTips;
        private Transform BotKeyTips1;

        protected void SetBotKeyTips()
        {
            Transform data = DataBtnList.GetCurSelected()?.transform;
            bool hasSetFood = false;
            if (data != null)
            {
                hasSetFood = Restaurant.GetRestaurantData(DataBtnList.GetCurSelectedPos1()).HaveSetFood;
            }
            BotKeyTips.gameObject.SetActive(CurMode == Mode.Restaurant);
            BotKeyTips1.gameObject.SetActive(CurMode == Mode.ChangeFood);
            BotKeyTips.Find("KT_Remove1").gameObject.SetActive(hasSetFood);
            BotKeyTips.Find("KT_Remove10").gameObject.SetActive(hasSetFood);
            BotKeyTips.Find("KT_FastAdd").gameObject.SetActive(hasSetFood);
            LayoutRebuilder.ForceRebuildLayoutImmediate(BotKeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
        }

        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();
        private void ClearTemp()
        {
            foreach (var s in tempSprite)
            {
                if (s.Value != EmptySprite)
                {
                    ML.Engine.Manager.GameManager.DestroyObj(s.Value);
                }
            }
        }

        private UIBtnList DataBtnList;
        List<string> FoodItemIDs = new List<string>() { "" };
        private UIBtnList FoodBtnList;
        public bool IsBtnAllInit1 = true;
        public bool IsBtnAllInit2 = true;

        protected override void InitBtnInfo()
        {
            DataBtnList = new UIBtnList(transform.Find("Restaurant").Find("Food").Find("Viewport").Find("UIBtnList").GetComponentInChildren<UIBtnListInitor>());
            DataBtnList.OnSelectButtonChanged += () =>
            {
                Refresh();
            };
            IsBtnAllInit1 = false;
            DataBtnList.ChangBtnNum(ManagerNS.LocalGameManager.Instance.RestaurantManager.DataNum, "Assets/_ProjectOC/OCResources/UI/Restaurant/Prefabs/UIRestaurantData.prefab", () => { IsBtnAllInit1 = true;  Refresh(); });

            FoodBtnList = new UIBtnList(transform.Find("ChangeFood").Find("Select").Find("Viewport").Find("UIBtnList").GetComponentInChildren<UIBtnListInitor>());
            FoodBtnList.OnSelectButtonChanged += () =>
            {
                Refresh();
            };
            List<string> itemIDs = ItemManager.Instance.GetAllItemID().ToList();
            foreach (var itemID in itemIDs)
            {
                if (ItemManager.Instance.GetItemType(itemID) == ItemType.Feed)
                {
                    FoodItemIDs.Add(itemID);
                }
            }
            IsBtnAllInit2 = false;
            FoodBtnList.ChangBtnNum(FoodItemIDs.Count, "Assets/_ProjectOC/OCResources/UI/Restaurant/Prefabs/UIRestaurantFood.prefab", () => { IsBtnAllInit2 = true; Refresh(); });
        }

        [System.Serializable]
        public struct RestaurantPanel
        {
            public TextContent textTitleRestaurant;
            public TextContent textTitleChangeFood;
            public TextContent textEmpty;
            public TextContent textNo1;

            public KeyTip ChangeFood;
            public KeyTip Remove1;
            public KeyTip Remove10;
            public KeyTip FastAdd;
            public KeyTip Confirm;
            public KeyTip Back;
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/Restaurant";
            this.abname = "RestaurantPanel";
            this.description = "RestaurantPanel数据加载完成";
        }
        #endregion

        #region Unity
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();
            InitTextContentPathData();
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            EmptySprite = transform.Find("Restaurant").Find("Food").Find("Viewport").Find("UIBtnList").Find("Container").Find("UIRestaurantData").Find("Icon").GetComponent<Image>().sprite;
            Food_Desc = transform.Find("ChangeFood").Find("Desc");
            BotKeyTips = transform.Find("BotKeyTips").Find("KeyTips");
            BotKeyTips1 = transform.Find("BotKeyTips").Find("KeyTips1");
            BotKeyTips1.gameObject.SetActive(false);

            RestaurantUITransform = transform.Find("Restaurant");
            ChangeFoodUITransform = transform.Find("ChangeFood");
            IsInit = true;
        }
        #endregion

        #region Override
        protected override void Enter()
        {
            Restaurant.OnDataChangeEvent += Refresh;
            base.Enter();
        }

        protected override void Exit()
        {
            Restaurant.OnDataChangeEvent -= Refresh;
            ClearTemp();
            base.Exit();
        }
        #endregion

        #region Internal
        private bool ItemIsDestroyed = false;
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIRestaurant.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIRestaurant.Remove.canceled -= Remove_canceled;
            ProjectOC.Input.InputManager.PlayerInput.UIRestaurant.Remove.performed -= Remove_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIRestaurant.FastAdd.performed -= FastAdd_performed;
        }

        protected override void RegisterInput()
        {
            DataBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            DataBtnList.EnableBtnList();
            FoodBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);

            ProjectOC.Input.InputManager.PlayerInput.UIRestaurant.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIRestaurant.Remove.canceled += Remove_canceled;
            ProjectOC.Input.InputManager.PlayerInput.UIRestaurant.Remove.performed += Remove_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIRestaurant.FastAdd.performed += FastAdd_performed;
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Restaurant)
            {
                CurMode = Mode.ChangeFood;
                int index = DataBtnList.GetCurSelectedPos1();
                if (0 <= index && index < ManagerNS.LocalGameManager.Instance.RestaurantManager.DataNum)
                {
                    var data = Restaurant.GetRestaurantData(index);
                    for (int i = 0; i < FoodItemIDs.Count; i++)
                    {
                        if (data.ItemID == FoodItemIDs[i] && i < FoodBtnList.BtnCnt && IsBtnAllInit2)
                        {
                            FoodBtnList.MoveIndexIUISelected(i);
                            break;
                        }
                    }
                }
                DataBtnList.DisableBtnList();
                FoodBtnList.EnableBtnList();
            }
            else if (CurMode == Mode.ChangeFood)
            {
                CurMode = Mode.Restaurant;
                int index = FoodBtnList.GetCurSelectedPos1();
                if (0 <= index && index < FoodItemIDs.Count && IsBtnAllInit1)
                {
                    Restaurant.UIChangeFood(DataBtnList.GetCurSelectedPos1(), FoodItemIDs[FoodBtnList.GetCurSelectedPos1()]);
                }
                DataBtnList.EnableBtnList();
                FoodBtnList.DisableBtnList();
            }
            Refresh();
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Restaurant)
            {
                UIMgr.PopPanel();
            }
            else
            {
                CurMode = Mode.Restaurant;
                DataBtnList.EnableBtnList();
                FoodBtnList.DisableBtnList();
                Refresh();
            }
        }

        private void Remove_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Restaurant)
            {
                if (this.ItemIsDestroyed)
                {
                    this.ItemIsDestroyed = false;
                }
                else
                {
                    Restaurant.UIRemove(DataBtnList.GetCurSelectedPos1(), 1);
                }
            }
        }
        private void Remove_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Restaurant)
            {
                this.ItemIsDestroyed = true;
                int index = DataBtnList.GetCurSelectedPos1();
                int amount = Restaurant.GetRestaurantData(index).Amount;
                if (amount < 10)
                {
                    Restaurant.UIRemove(index, amount);
                }
                else
                {
                    Restaurant.UIRemove(index, 10);
                }
            }
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Restaurant)
            {
                Restaurant.UIFastAdd(DataBtnList.GetCurSelectedPos1());
            }
        }
        #endregion

        #region UI
        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit) { return; }
            
            Text_Title.text = CurMode == Mode.Restaurant ? PanelTextContent.textTitleRestaurant : PanelTextContent.textTitleChangeFood;
            SetBotKeyTips();
            RestaurantUITransform.gameObject.SetActive(CurMode == Mode.Restaurant);
            ChangeFoodUITransform.gameObject.SetActive(CurMode == Mode.ChangeFood);

            if (CurMode == Mode.Restaurant)
            {
                int dataNum = ManagerNS.LocalGameManager.Instance.RestaurantManager.DataNum;
                int maxCapacity = ManagerNS.LocalGameManager.Instance.RestaurantManager.MaxCapacity;
                
                for (int i = 0; i < dataNum; i++)
                {
                    if (i < DataBtnList.BtnCnt && IsBtnAllInit1)
                    {
                        var uidata = DataBtnList.GetBtn(i).transform;
                        var data = Restaurant.GetRestaurantData(i);

                        string itemID = data.ItemID;
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            var sprite = ItemManager.Instance.GetItemSprite(itemID);
                            tempSprite[itemID] = sprite ?? EmptySprite;
                        }
                        uidata.Find("Icon").GetComponent<Image>().sprite = tempSprite[itemID];
                        uidata.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = data.Amount.ToString();
                        uidata.Find("MaxCapacity").GetComponent<TMPro.TextMeshProUGUI>().text = maxCapacity.ToString();
                        var bar = uidata.Find("Bar").Find("Cur").GetComponent<RectTransform>();
                        float sizeDeltaX = uidata.Find("Bar").GetComponent<RectTransform>().sizeDelta.x * data.Amount / maxCapacity;
                        bar.sizeDelta = new Vector2(sizeDeltaX, bar.sizeDelta.y);

                        string name = ItemManager.Instance.GetItemName(itemID);
                        uidata.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = !string.IsNullOrEmpty(name) ? name : PanelTextContent.textEmpty;
                        uidata.Find("Priority1").gameObject.SetActive(data.Priority == FoodPriority.No1);
                        uidata.Find("Priority1").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textNo1;
                        uidata.Find("Priority2").gameObject.SetActive(data.Priority == FoodPriority.No2);
                    }
                }
            }
            else if (CurMode == Mode.ChangeFood)
            {
                for (int i = 0; i < FoodItemIDs.Count; i++)
                {
                    if (i < FoodBtnList.BtnCnt && IsBtnAllInit2)
                    {
                        string itemID = FoodItemIDs[i];
                        var uidata = FoodBtnList.GetBtn(i).transform;
                        if (!tempSprite.ContainsKey(itemID))
                        {
                            var sprite = ItemManager.Instance.GetItemSprite(itemID);
                            tempSprite[itemID] = sprite ?? EmptySprite;
                        }
                        uidata.Find("Icon").GetComponent<Image>().sprite = tempSprite[itemID];
                    }
                }
                int index = FoodBtnList.GetCurSelectedPos1();
                if (0 <= index && index < FoodBtnList.BtnCnt && IsBtnAllInit2)
                {
                    string curItemID = FoodItemIDs[index];
                    Food_Desc.Find("Icon").GetComponent<Image>().sprite = FoodBtnList.GetBtn(index).transform.Find("Icon").GetComponent<Image>().sprite;
                    Food_Desc.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(curItemID);
                    Food_Desc.Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemDescription(curItemID) ?? "";
                    Food_Desc.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetEffectDescription(curItemID) ?? "";
                    int weight = ItemManager.Instance.GetWeight(curItemID);
                    weight = weight > 0 ? weight : 0;
                    Food_Desc.Find("Weight").GetComponent<TMPro.TextMeshProUGUI>().text = weight.ToString();
                }
            }
        }
        #endregion
    }
}
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.RestaurantNS.UI.UIRestaurant;
using ML.Engine.UI;
using System.Linq;

namespace ProjectOC.RestaurantNS.UI
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
            bool hasSetFood = (CurMode == Mode.Restaurant && Restaurant.DataContainer.HaveSetData(DataIndex));
            BotKeyTips.gameObject.SetActive(CurMode == Mode.Restaurant);
            BotKeyTips1.gameObject.SetActive(CurMode == Mode.ChangeFood);
            BotKeyTips.Find("KT_Remove1").gameObject.SetActive(hasSetFood);
            BotKeyTips.Find("KT_Remove10").gameObject.SetActive(hasSetFood);
            BotKeyTips.Find("KT_FastAdd").gameObject.SetActive(hasSetFood);
            LayoutRebuilder.ForceRebuildLayoutImmediate(BotKeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
        }
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();
        private UIBtnList DataBtnList;
        private int DataIndex => DataBtnList?.GetCurSelectedPos1() ?? 0;
        List<string> FoodItemIDs = new List<string>() { "" };
        private UIBtnList FoodBtnList;
        private int FoodIndex => FoodBtnList?.GetCurSelectedPos1() ?? 0;
        private bool IsInitBtnList = false;
        protected override void InitBtnInfo()
        {
            UIBtnList.Synchronizer synchronizer = new UIBtnList.Synchronizer(2, () => { IsInitBtnList = true; Refresh(); });
            DataBtnList = new UIBtnList(transform.Find("Restaurant").Find("Food").Find("Viewport").GetComponentInChildren<UIBtnListInitor>());
            DataBtnList.OnSelectButtonChanged += () => { Refresh(); };

            DataBtnList.ChangBtnNum(ManagerNS.LocalGameManager.Instance.RestaurantManager.DataNum, "Prefab_Restaurant_UI/Prefab_Restaurant_UI_DataTemplate.prefab", () => { synchronizer.Check(); });

            FoodBtnList = new UIBtnList(transform.Find("ChangeFood").Find("Select").Find("Viewport").GetComponentInChildren<UIBtnListInitor>());
            FoodBtnList.OnSelectButtonChanged += () => { Refresh(); };
            List<string> itemIDs = ItemManager.Instance.GetAllItemID().ToList();
            foreach (var itemID in itemIDs)
            {
                if (ItemManager.Instance.GetItemType(itemID) == ItemType.Feed)
                {
                    FoodItemIDs.Add(itemID);
                }
            }
            itemIDs = ItemManager.Instance.SortItemIDs(itemIDs);
            FoodBtnList.ChangBtnNum(FoodItemIDs.Count, "Prefab_Restaurant_UI/Prefab_Restaurant_UI_FoodTemplate.prefab", () => { synchronizer.Check(); });
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
            abpath = "OCTextContent/Restaurant";
            abname = "RestaurantPanel";
            description = "RestaurantPanel数据加载完成";
        }
        #endregion

        #region Unity
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();
            InitTextContentPathData();
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            EmptySprite = transform.Find("Restaurant").Find("Food").Find("Viewport").Find("Container").Find("UIRestaurantData").Find("Icon").GetComponent<Image>().sprite;
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
            Restaurant.DataContainer.OnDataChangeEvent += Refresh;
            base.Enter();
        }
        protected override void Exit()
        {
            Restaurant.DataContainer.OnDataChangeEvent -= Refresh;
            foreach (var s in tempSprite)
            {
                if (s.Value != EmptySprite)
                {
                    ML.Engine.Manager.GameManager.DestroyObj(s.Value);
                }
            }
            base.Exit();
        }
        #endregion

        #region Internal
        private bool ItemIsDestroyed = false;
        protected override void UnregisterInput()
        {
            DataBtnList.DisableBtnList();
            FoodBtnList.DisableBtnList();
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
                string itemID = Restaurant.DataContainer.GetID(DataIndex);
                for (int i = 0; i < FoodItemIDs.Count; i++)
                {
                    if (itemID == FoodItemIDs[i])
                    {
                        FoodBtnList.MoveIndexIUISelected(i);
                        break;
                    }
                }
                DataBtnList.DisableBtnList();
                FoodBtnList.EnableBtnList();
            }
            else if (CurMode == Mode.ChangeFood)
            {
                CurMode = Mode.Restaurant;
                Restaurant.ChangeData(DataIndex, FoodItemIDs[FoodIndex]);
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
                if (ItemIsDestroyed)
                {
                    ItemIsDestroyed = false;
                }
                else
                {
                    Restaurant.Remove(DataIndex, 1);
                }
            }
        }
        private void Remove_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Restaurant)
            {
                ItemIsDestroyed = true;
                int num = Restaurant.DataContainer.GetAmount(DataIndex, DataNS.DataOpType.Storage);
                num = num < 10 ? num : 10;
                Restaurant.Remove(DataIndex, num);
            }
        }
        private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Restaurant)
            {
                Restaurant.FastAdd(DataBtnList.GetCurSelectedPos1());
            }
        }
        #endregion

        #region UI
        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }
            
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
                    var uidata = DataBtnList.GetBtn(i).transform;
                    string itemID = Restaurant.DataContainer.GetID(i);
                    int amount = Restaurant.DataContainer.GetAmount(i, DataNS.DataOpType.Storage);
                    if (!tempSprite.ContainsKey(itemID))
                    {
                        var sprite = ItemManager.Instance.GetItemSprite(itemID);
                        tempSprite[itemID] = sprite ?? EmptySprite;
                    }
                    uidata.Find("Icon").GetComponent<Image>().sprite = tempSprite[itemID];
                    uidata.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = amount.ToString();
                    uidata.Find("MaxCapacity").GetComponent<TMPro.TextMeshProUGUI>().text = maxCapacity.ToString();
                    var bar = uidata.Find("Bar").Find("Cur").GetComponent<RectTransform>();
                    float sizeDeltaX = uidata.Find("Bar").GetComponent<RectTransform>().sizeDelta.x * amount / maxCapacity;
                    bar.sizeDelta = new Vector2(sizeDeltaX, bar.sizeDelta.y);

                    string name = ItemManager.Instance.GetItemName(itemID);
                    uidata.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = !string.IsNullOrEmpty(name) ? name : PanelTextContent.textEmpty;
                    uidata.Find("Priority1").gameObject.SetActive(i == 0);
                    uidata.Find("Priority1").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textNo1;
                    uidata.Find("Priority2").gameObject.SetActive(i == 1);
                }
            }
            else if (CurMode == Mode.ChangeFood)
            {
                for (int i = 0; i < FoodItemIDs.Count; i++)
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
                string curItemID = FoodItemIDs[FoodIndex];
                Food_Desc.Find("Icon").GetComponent<Image>().sprite = FoodBtnList.GetBtn(FoodIndex).transform.Find("Icon").GetComponent<Image>().sprite;
                Food_Desc.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(curItemID);
                Food_Desc.Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemDescription(curItemID) ?? "";
                Food_Desc.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetEffectDescription(curItemID) ?? "";
                int weight = ItemManager.Instance.GetWeight(curItemID);
                weight = weight > 0 ? weight : 0;
                Food_Desc.Find("Weight").GetComponent<TMPro.TextMeshProUGUI>().text = weight.ToString();
            }
        }
        #endregion
    }
}
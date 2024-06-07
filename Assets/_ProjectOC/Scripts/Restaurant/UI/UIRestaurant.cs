using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.RestaurantNS.UI.UIRestaurant;
using System.Linq;
using ML.Engine.Utility;

namespace ProjectOC.RestaurantNS.UI
{
    public class UIRestaurant : ML.Engine.UI.UIBasePanel<RestaurantPanel>
    {
        #region Str
        private const string str = "";
        private const string strRestaurant = "Restaurant";
        private const string strFood = "Food";
        private const string strViewport = "Viewport";
        private const string strChangeFood = "ChangeFood";
        private const string strSelect = "Select";
        private const string strTopTitle = "TopTitle";
        private const string strText = "Text";
        private const string strIcon = "Icon";
        private const string strDesc = "Desc";
        private const string strBotKeyTips = "BotKeyTips";
        private const string strKeyTips = "KeyTips";
        private const string strKeyTips1 = "KeyTips1";
        private const string strContainer = "Container";
        private const string strUIRestaurantData = "UIRestaurantData";
        private const string strKT_Remove1 = "KT_Remove1";
        private const string strKT_Remove10 = "KT_Remove10";
        private const string strKT_FastAdd = "KT_FastAdd";
        private const string strAmount = "Amount";
        private const string strMaxCapacity = "MaxCapacity";
        private const string strBar = "Bar";
        private const string strCur = "Cur";
        private const string strName = "Name";
        private const string strPriority1 = "Priority1";
        private const string strPriority2 = "Priority2";
        private const string strItemDesc = "ItemDesc";
        private const string strEffectDesc = "EffectDesc";
        private const string strWeight = "Weight";

        private const string strPrefab_Restaurant_UI_DataTemplate = "Prefab_Restaurant_UI/Prefab_Restaurant_UI_DataTemplate.prefab";
        private const string strPrefab_Restaurant_UI_FoodTemplate = "Prefab_Restaurant_UI/Prefab_Restaurant_UI_FoodTemplate.prefab";
        #endregion

        #region Data
        #region Mode
        public enum Mode
        {
            Restaurant = 0,
            ChangeFood = 1,
        }
        private Mode curMode;
        public Mode CurMode 
        {
            get => curMode;
            set
            {
                DataBtnList.DisableBtnList();
                FoodBtnList.DisableBtnList();
                curMode = value;
                if (curMode == Mode.Restaurant)
                {
                    DataBtnList.EnableBtnList();
                }
                else
                {
                    string itemID = Restaurant.DataContainer.GetID(DataIndex);
                    for (int i = 0; i < FoodItemIDs.Count; i++)
                    {
                        if (itemID == FoodItemIDs[i])
                        {
                            FoodBtnList.MoveIndexIUISelected(i);
                            break;
                        }
                    }
                    FoodBtnList.EnableBtnList();
                }
            }
        }
        #endregion

        public Restaurant Restaurant;
        private TMPro.TextMeshProUGUI Text_Title;
        private Transform Food_Desc;
        private Transform BotKeyTips;
        private Transform BotKeyTips1;
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();

        #region BtnList
        private ML.Engine.UI.UIBtnList DataBtnList;
        private int DataIndex => DataBtnList?.GetCurSelectedPos1() ?? 0;
        List<string> FoodItemIDs = new List<string>();
        private ML.Engine.UI.UIBtnList FoodBtnList;
        private int FoodIndex => FoodBtnList?.GetCurSelectedPos1() ?? 0;
        private bool IsInitBtnList = false;
        protected override void InitBtnInfo()
        {
            Synchronizer synchronizer = new Synchronizer(2, () => { IsInitBtnList = true; Refresh(); });
            DataBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strRestaurant).Find(strFood).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            DataBtnList.OnSelectButtonChanged += () => { Refresh(); };
            DataBtnList.ChangBtnNum(ManagerNS.LocalGameManager.Instance.RestaurantManager.Config.DataNum,
                strPrefab_Restaurant_UI_DataTemplate, () => { synchronizer.Check(); });

            FoodBtnList = new ML.Engine.UI.UIBtnList(transform.Find(strChangeFood).Find(strSelect).Find(strViewport).GetComponentInChildren<ML.Engine.UI.UIBtnListInitor>());
            FoodBtnList.OnSelectButtonChanged += () => { Refresh(); };
            List<string> itemIDs = ML.Engine.InventorySystem.ItemManager.Instance.GetAllItemID().ToList();
            foreach (var itemID in itemIDs)
            {
                if (ML.Engine.InventorySystem.ItemManager.Instance.GetItemType(itemID) == ML.Engine.InventorySystem.ItemType.Feed)
                {
                    FoodItemIDs.Add(itemID);
                }
                FoodItemIDs = ML.Engine.InventorySystem.ItemManager.Instance.SortItemIDs(FoodItemIDs);
            }
            FoodItemIDs.Insert(0, str);
            itemIDs = ML.Engine.InventorySystem.ItemManager.Instance.SortItemIDs(itemIDs);
            FoodBtnList.ChangBtnNum(FoodItemIDs.Count, strPrefab_Restaurant_UI_FoodTemplate, () => { synchronizer.Check(); });
        }
        #endregion

        #region TextContent
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
        #endregion

        #region Unity
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();
            Text_Title = transform.Find(strTopTitle).Find(strText).GetComponent<TMPro.TextMeshProUGUI>();
            Food_Desc = transform.Find(strChangeFood).Find(strDesc);
            BotKeyTips = transform.Find(strBotKeyTips).Find(strKeyTips);
            BotKeyTips1 = transform.Find(strBotKeyTips).Find(strKeyTips1);
            IsInit = true;
        }
        #endregion

        #region Override
        protected override void Enter()
        {
            Restaurant.DataContainer.OnDataChangeEvent += Refresh;
            tempSprite.Add(str, transform.Find(strRestaurant).Find(strFood).Find(strViewport).Find(strContainer).Find(strUIRestaurantData).Find(strIcon).GetComponent<Image>().sprite);
            base.Enter();
        }
        protected override void Exit()
        {
            Restaurant.DataContainer.OnDataChangeEvent -= Refresh;
            tempSprite.Remove(str);
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s.Value);
            }
            tempSprite.Clear();
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
            DataBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.UI.UIBtnListContainer.BindType.started);
            DataBtnList.EnableBtnList();
            FoodBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, ML.Engine.UI.UIBtnListContainer.BindType.started);
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
            }
            else if (CurMode == Mode.ChangeFood)
            {
                CurMode = Mode.Restaurant;
                Restaurant.ChangeData(DataIndex, new DataNS.ItemIDDataObj(FoodItemIDs[FoodIndex]));
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
                Restaurant.FastAdd(DataIndex);
            }
        }
        #endregion

        #region UI
        protected void SetUIActive()
        {
            transform.Find(strRestaurant).gameObject.SetActive(CurMode == Mode.Restaurant);
            transform.Find(strChangeFood).gameObject.SetActive(CurMode == Mode.ChangeFood);
            bool hasSetFood = (CurMode == Mode.Restaurant && Restaurant.DataContainer.HaveSetData(DataIndex));
            BotKeyTips.gameObject.SetActive(CurMode == Mode.Restaurant);
            BotKeyTips1.gameObject.SetActive(CurMode == Mode.ChangeFood);
            BotKeyTips.Find(strKT_Remove1).gameObject.SetActive(hasSetFood);
            BotKeyTips.Find(strKT_Remove10).gameObject.SetActive(hasSetFood);
            BotKeyTips.Find(strKT_FastAdd).gameObject.SetActive(hasSetFood);
            LayoutRebuilder.ForceRebuildLayoutImmediate(BotKeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
        }
        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit || !IsInitBtnList) { return; }
            Text_Title.text = CurMode == Mode.Restaurant ? PanelTextContent.textTitleRestaurant : PanelTextContent.textTitleChangeFood;
            SetUIActive();
            if (CurMode == Mode.Restaurant)
            {
                int maxCapacity = ManagerNS.LocalGameManager.Instance.RestaurantManager.Config.MaxCapacity;
                for (int i = 0; i < ManagerNS.LocalGameManager.Instance.RestaurantManager.Config.DataNum; i++)
                {
                    var uidata = DataBtnList.GetBtn(i).transform;
                    string itemID = Restaurant.DataContainer.GetID(i);
                    int amount = Restaurant.DataContainer.GetAmount(i, DataNS.DataOpType.Storage);
                    if (!tempSprite.ContainsKey(itemID))
                    {
                        tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                    }
                    uidata.Find(strIcon).GetComponent<Image>().sprite = tempSprite[itemID];
                    uidata.Find(strAmount).GetComponent<TMPro.TextMeshProUGUI>().text = amount.ToString();
                    uidata.Find(strMaxCapacity).GetComponent<TMPro.TextMeshProUGUI>().text = maxCapacity.ToString();
                    var bar = uidata.Find(strBar).Find(strCur).GetComponent<RectTransform>();
                    float percent = ((float)amount) / ((float)maxCapacity);
                    percent = percent <= 1 ? percent : 1;
                    float sizeDeltaX = uidata.Find(strBar).GetComponent<RectTransform>().sizeDelta.x * percent;
                    bar.sizeDelta = new Vector2(sizeDeltaX, bar.sizeDelta.y);

                    string name = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(itemID);
                    uidata.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = !string.IsNullOrEmpty(name) ? name : PanelTextContent.textEmpty;
                    uidata.Find(strPriority1).gameObject.SetActive(i == 0);
                    uidata.Find(strPriority1).Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textNo1;
                    uidata.Find(strPriority2).gameObject.SetActive(i == 1);
                }
            }
            else if (CurMode == Mode.ChangeFood)
            {
                for (int i = 0; i < FoodItemIDs.Count; i++)
                {
                    string itemID = FoodItemIDs[i];
                    if (!tempSprite.ContainsKey(itemID))
                    {
                        tempSprite[itemID] = ML.Engine.InventorySystem.ItemManager.Instance.GetItemSprite(itemID);
                    }
                    FoodBtnList.GetBtn(i).transform.Find(strIcon).GetComponent<Image>().sprite = tempSprite[itemID];
                }
                string curItemID = FoodItemIDs[FoodIndex];
                Food_Desc.Find(strIcon).GetComponent<Image>().sprite = FoodBtnList.GetBtn(FoodIndex).transform.Find(strIcon).GetComponent<Image>().sprite;
                Food_Desc.Find(strName).GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemName(curItemID);
                Food_Desc.Find(strItemDesc).GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetItemDescription(curItemID) ?? str;
                Food_Desc.Find(strEffectDesc).GetComponent<TMPro.TextMeshProUGUI>().text = ML.Engine.InventorySystem.ItemManager.Instance.GetEffectDescription(curItemID) ?? str;
                int weight = ML.Engine.InventorySystem.ItemManager.Instance.GetWeight(curItemID);
                weight = weight > 0 ? weight : 0;
                Food_Desc.Find(strWeight).GetComponent<TMPro.TextMeshProUGUI>().text = weight.ToString();
            }
        }
        #endregion
    }
}
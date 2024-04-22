using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.TextContent;
using ProjectOC.ManagerNS;
using ProjectOC.ProNodeNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.InventorySystem.UI.UIRestaurant;
using ML.Engine.UI;


namespace ProjectOC.InventorySystem.UI
{
    public class UIRestaurant : ML.Engine.UI.UIBasePanel<RestaurantPanel>
    {
        //    #region Input
        //    private bool ItemIsDestroyed = false;
        //    #endregion

        //    #region Unity
        //    public bool IsInit = false;
        //    protected override void Start()
        //    {
        //        base.Start();

        //        this.InitTextContentPathData();

        //        #region TopTitle
        //        Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
        //        #endregion

        //        #region ProNode
        //        ProNode_UI = transform.Find("ProNode");
        //        Transform recipe = transform.Find("ProNode").Find("Recipe");
        //        ProNode_Product = recipe.Find("Product");
        //        EmptySprite = ProNode_Product.Find("Icon").GetComponent<Image>().sprite;
        //        ProNode_Raw_GridLayout = recipe.Find("Raw").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
        //        ProNode_Raw_UIItemTemplate = ProNode_Raw_GridLayout.transform.Find("UIItemTemplate");
        //        ProNode_Raw_UIItemTemplate.gameObject.SetActive(false);
        //        ProNode_Worker = transform.Find("ProNode").Find("Worker");
        //        ProNode_Eff = transform.Find("ProNode").Find("Eff");
        //        #endregion

        //        #region ChangeRecipe
        //        Recipe_UI = transform.Find("ChangeRecipe");
        //        Recipe_UI.gameObject.SetActive(false);
        //        Recipe_GridLayout = Recipe_UI.Find("Select").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
        //        Recipe_UIItemTemplate = Recipe_GridLayout.transform.Find("UIItemTemplate");
        //        Recipe_UIItemTemplate.gameObject.SetActive(false);
        //        Recipe_Raw_GridLayout = Recipe_UI.Find("Recipe").Find("Raw").Find("Viewport").Find("Content").GetComponent<GridLayoutGroup>();
        //        Recipe_Raw_UIItemTemplate = Recipe_Raw_GridLayout.transform.Find("UIItemTemplate");
        //        Recipe_Raw_UIItemTemplate.gameObject.SetActive(false);
        //        Recipe_Desc = Recipe_UI.Find("Desc");
        //        #endregion

        //        #region BotKeyTips
        //        BotKeyTips = this.transform.Find("BotKeyTips").Find("KeyTips");
        //        BotKeyTips1 = this.transform.Find("BotKeyTips").Find("KeyTips1");
        //        BotKeyTips1.gameObject.SetActive(false);
        //        #endregion

        //        IsInit = true;
        //    }
        //    #endregion

        //    #region Override
        //    protected override void Enter()
        //    {
        //        ProNode.OnProduceEnd += Refresh;
        //        base.Enter();
        //    }

        //    protected override void Exit()
        //    {
        //        ProNode.OnProduceEnd -= Refresh;
        //        ClearTemp();
        //        base.Exit();
        //    }
        //    #endregion

        //    #region Internal
        //    public enum Mode
        //    {
        //        ProNode = 0,
        //        ChangeRecipe = 1,
        //    }
        //    public Mode CurMode = Mode.ProNode;

        //    public ProNodeNS.ProNode ProNode;

        //    public bool IsInitCurRecipe;

        //    [ShowInInspector]
        //    private List<string> Recipes = new List<string>();


        //    protected override void UnregisterInput()
        //    {
        //        ProjectOC.Input.InputManager.PlayerInput.UIProNode.Disable();
        //        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
        //        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        //        ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.canceled -= Remove_canceled;
        //        ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.performed -= Remove_performed;
        //        ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed -= FastAdd_performed;
        //        ProjectOC.Input.InputManager.PlayerInput.UIProNode.Alter.started -= Alter_started;
        //    }

        //    protected override void RegisterInput()
        //    {
        //        TestBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);

        //        ProjectOC.Input.InputManager.PlayerInput.UIProNode.Enable();
        //        ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
        //        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        //        ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.canceled += Remove_canceled;
        //        ProjectOC.Input.InputManager.PlayerInput.UIProNode.Remove.performed += Remove_performed;
        //        ProjectOC.Input.InputManager.PlayerInput.UIProNode.FastAdd.performed += FastAdd_performed;
        //        ProjectOC.Input.InputManager.PlayerInput.UIProNode.Alter.started += Alter_started;
        //    }

        //    private void Alter_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        //    {

        //    }
        //    private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        //    {
        //        if (CurMode == Mode.ProNode)
        //        {
        //            CurMode = Mode.ChangeRecipe;
        //        }
        //        else if (CurMode == Mode.ChangeRecipe)
        //        {
        //            ProNode.ChangeRecipe(CurrentRecipe);
        //            IsInitCurRecipe = false;
        //            CurMode = Mode.ProNode;
        //        }
        //        Refresh();
        //    }

        //    private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        //    {
        //        if (CurMode == Mode.ProNode)
        //        {
        //            if (ProNode.HasRecipe)
        //            {
        //                ItemManager.Instance.AddItemIconObject(ProNode.Recipe.Product.id,
        //                                                       ProNode.WorldProNode.transform,
        //                                                       new Vector3(0, ProNode.WorldProNode.transform.GetComponent<BoxCollider>().size.y * 1.5f, 0),
        //                                                       Quaternion.Euler(Vector3.zero),
        //                                                       Vector3.one,
        //                                                       (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).currentCharacter.transform);
        //            }
        //            UIMgr.PopPanel();
        //        }
        //        else
        //        {
        //            CurMode = Mode.ProNode;
        //            Refresh();
        //        }
        //    }

        //    private void Remove_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        //    {
        //        if (CurMode == Mode.ProNode)
        //        {
        //            if (this.ItemIsDestroyed)
        //            {
        //                this.ItemIsDestroyed = false;
        //            }
        //            else
        //            {
        //                ProNode.UIRemove(1);
        //                Refresh();
        //            }
        //        }
        //    }
        //    private void Remove_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        //    {
        //        if (CurMode == Mode.ProNode)
        //        {
        //            this.ItemIsDestroyed = true;
        //            if (ProNode.Stack < 10)
        //            {
        //                ProNode.UIRemove(ProNode.Stack);
        //            }
        //            else
        //            {
        //                ProNode.UIRemove(10);
        //            }
        //            Refresh();
        //        }
        //    }
        //    private void FastAdd_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        //    {
        //        if (CurMode == Mode.ProNode)
        //        {
        //            ProNode.UIFastAdd();
        //        }
        //        Refresh();
        //    }
        //    #endregion

        //    #region UI
        //    private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();
        //    private void ClearTemp()
        //    {
        //        foreach (var s in tempSprite)
        //        {
        //            ML.Engine.Manager.GameManager.DestroyObj(s.Value);
        //        }
        //    }

        //    #region UI对象引用
        //    private TMPro.TextMeshProUGUI Text_Title;
        //    private Sprite EmptySprite;

        //    private Transform ProNode_UI;
        //    private Transform ProNode_Priority;
        //    private Transform ProNode_Product;
        //    private GridLayoutGroup ProNode_Raw_GridLayout;
        //    private Transform ProNode_Raw_UIItemTemplate;
        //    private Transform ProNode_Worker;
        //    private Transform ProNode_Eff;

        //    private Transform Recipe_UI;
        //    private GridLayoutGroup Recipe_GridLayout;
        //    private Transform Recipe_UIItemTemplate;
        //    private GridLayoutGroup Recipe_Raw_GridLayout;
        //    private Transform Recipe_Raw_UIItemTemplate;
        //    private Transform Recipe_Desc;

        //    private Transform BotKeyTips;
        //    private Transform BotKeyTips1;
        //    #endregion
        //    public override void Refresh()
        //    {
        //        // 加载完成JSON数据 & 查找完所有引用
        //        if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
        //        {
        //            return;
        //        }

        //        this.ProNode_UI.gameObject.SetActive(true);
        //        this.Recipe_UI.gameObject.SetActive(CurMode == Mode.ChangeRecipe);
        //        this.BotKeyTips.gameObject.SetActive(CurMode == Mode.ProNode);
        //        this.BotKeyTips1.gameObject.SetActive(CurMode != Mode.ProNode);
        //        this.BotKeyTips1.Find("KT_Confirm").gameObject.SetActive(CurMode != Mode.Upgrade);

        //        if (CurMode == Mode.ProNode)
        //        {
        //            #region ProNode
        //            #region TopTitle

        //            #endregion

        //            #region Product
        //            ProNode_Product.Find("Selected").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
        //            bool hasRecipe = ProNode.HasRecipe;
        //            ProNode_Product.Find("Mask").gameObject.SetActive(hasRecipe);
        //            ProNode_Product.transform.Find("Name").gameObject.SetActive(hasRecipe);
        //            ProNode_Product.transform.Find("Back").gameObject.SetActive(hasRecipe);
        //            ProNode_Product.transform.Find("Back1").gameObject.SetActive(hasRecipe);
        //            ProNode_Product.transform.Find("Amount").gameObject.SetActive(hasRecipe);

        //            if (!hasRecipe || ProNode.State != ProNodeState.Production)
        //            {
        //                ProNode_Product.Find("Mask").GetComponent<Image>().fillAmount = 0;
        //            }

        //            if (hasRecipe)
        //            {
        //                #region Product
        //                string productID = ProNode.Recipe.ProductID;
        //                var imgProduct = ProNode_Product.transform.Find("Icon").GetComponent<Image>();
        //                if (ItemManager.Instance.IsValidItemID(productID))
        //                {
        //                    if (!tempSprite.ContainsKey(productID))
        //                    {
        //                        var sprite = ItemManager.Instance.GetItemSprite(productID);
        //                        tempSprite[productID] = sprite;
        //                    }
        //                    imgProduct.sprite = tempSprite[productID];
        //                }
        //                else
        //                {
        //                    imgProduct.sprite = EmptySprite;
        //                }
        //                ProNode_Product.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(productID);
        //                ProNode_Product.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = ProNode.Stack.ToString();
        //                #endregion

        //                #region Raw
        //                int delta = tempUIItems.Count - Raws.Count;
        //                if (delta > 0)
        //                {
        //                    for (int i = 0; i < delta; ++i)
        //                    {
        //                        tempUIItems[tempUIItems.Count - 1 - i].SetActive(false);
        //                    }
        //                }
        //                else if (delta < 0)
        //                {
        //                    delta = -delta;
        //                    for (int i = 0; i < delta; ++i)
        //                    {
        //                        var uiitem = Instantiate(ProNode_Raw_UIItemTemplate, ProNode_Raw_GridLayout.transform, false);
        //                        tempUIItems.Add(uiitem.gameObject);
        //                    }
        //                }
        //                for (int i = 0; i < Raws.Count; ++i)
        //                {
        //                    var itemID = Raws[i].id;
        //                    var item = tempUIItems[i];
        //                    // Active
        //                    item.SetActive(true);
        //                    // Icon
        //                    var img = item.transform.Find("Icon").GetComponent<Image>();
        //                    if (ItemManager.Instance.IsValidItemID(itemID))
        //                    {
        //                        if (!tempSprite.ContainsKey(itemID))
        //                        {
        //                            var sprite = ItemManager.Instance.GetItemSprite(itemID);
        //                            tempSprite[itemID] = sprite;
        //                        }
        //                        img.sprite = tempSprite[itemID];
        //                    }
        //                    else
        //                    {
        //                        img.sprite = EmptySprite;
        //                    }
        //                    // NeedAmount
        //                    var needAmount = item.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
        //                    needAmount.text = ProNode.Recipe.GetRawNum(itemID).ToString();
        //                    // Amount
        //                    var amount = item.transform.Find("Amount").GetComponent<TMPro.TextMeshProUGUI>();
        //                    amount.text = ProNode.GetItemAllNum(itemID).ToString();
        //                    // Name
        //                    item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(itemID);
        //                    if (int.Parse(amount.text) < int.Parse(needAmount.text))
        //                    {
        //                        item.transform.Find("Back").GetComponent<Image>().color = Color.red;
        //                    }
        //                    else
        //                    {
        //                        item.transform.Find("Back").GetComponent<Image>().color = Color.black;
        //                    }
        //                }
        //                LayoutRebuilder.ForceRebuildLayoutImmediate(ProNode_Raw_GridLayout.GetComponent<RectTransform>());
        //                #endregion
        //            }
        //            else
        //            {
        //                ProNode_Product.transform.Find("Icon").GetComponent<Image>().sprite = EmptySprite;
        //                for (int i = 0; i < tempUIItems.Count; ++i)
        //                {
        //                    tempUIItems[tempUIItems.Count - 1 - i].SetActive(false);
        //                }
        //            }
        //            #endregion
        //            #endregion

        //            #region BotKeyTips
        //            BotKeyTips.Find("KT_ChangeRecipe").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
        //            BotKeyTips.Find("KT_Remove1").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && ProNode.HasRecipe);
        //            BotKeyTips.Find("KT_Remove10").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && ProNode.HasRecipe);
        //            BotKeyTips.Find("KT_FastAdd").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe && Raws != null && Raws.Count > 0);
        //            BotKeyTips.Find("KT_Return").gameObject.SetActive(CurProNodeMode == ProNodeSelectMode.Recipe);
        //            LayoutRebuilder.ForceRebuildLayoutImmediate(BotKeyTips.GetComponent<GridLayoutGroup>().GetComponent<RectTransform>());
        //            #endregion
        //        }
        //        else if (CurMode == Mode.ChangeRecipe)
        //        {
        //            Recipes = new List<string>() { "" };
        //            Recipes.AddRange(ProNode.GetCanProduceRecipe());
        //            Formula product;
        //            #region Select

        //            for (int i = 0; i < Recipes.Count; ++i)
        //            {
        //                var recipeID = Recipes[i];
        //                product = LocalGameManager.Instance.RecipeManager.GetProduct(recipeID);
        //                var item = tempUIItemsRecipe[i];
        //                // Active
        //                item.SetActive(true);
        //                var img = item.transform.Find("Icon").GetComponent<Image>();
        //                if (ItemManager.Instance.IsValidItemID(product.id))
        //                {
        //                    if (!tempSprite.ContainsKey(product.id))
        //                    {
        //                        var sprite = ItemManager.Instance.GetItemSprite(product.id);
        //                        tempSprite[product.id] = sprite;
        //                    }
        //                    img.sprite = tempSprite[product.id];
        //                }
        //                else
        //                {
        //                    img.sprite = EmptySprite;
        //                }
        //                // Selected
        //                bool isSelected = CurrentRecipe == recipeID;
        //                item.transform.Find("Selected").gameObject.SetActive(isSelected);
        //            }
        //            #endregion

        //            #region Product
        //            product = LocalGameManager.Instance.RecipeManager.GetProduct(CurrentRecipe);
        //            if (ItemManager.Instance.IsValidItemID(product.id))
        //            {
        //                if (!tempSprite.ContainsKey(product.id))
        //                {
        //                    var sprite = ItemManager.Instance.GetItemSprite(product.id);
        //                    tempSprite[product.id] = sprite;
        //                }
        //                Recipe_UI.Find("Recipe").Find("Product").GetComponent<Image>().sprite = tempSprite[product.id];
        //                Recipe_UI.Find("Recipe").Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textTime + ":" + LocalGameManager.Instance.RecipeManager.GetTimeCost(CurrentRecipe).ToString() + "s";
        //                Recipe_Desc.Find("Icon").GetComponent<Image>().sprite = tempSprite[product.id];
        //                Recipe_Desc.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(product.id);
        //                Recipe_Desc.Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemDescription(product.id);
        //                Recipe_Desc.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetEffectDescription(product.id);
        //                Recipe_Desc.Find("WeightIcon").gameObject.SetActive(true);
        //                Recipe_Desc.Find("Weight").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetWeight(product.id).ToString();
        //            }
        //            else
        //            {
        //                Recipe_UI.Find("Recipe").Find("Product").GetComponent<Image>().sprite = EmptySprite;
        //                Recipe_UI.Find("Recipe").Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textTime + ": 0s";
        //                Recipe_Desc.Find("Icon").GetComponent<Image>().sprite = EmptySprite;
        //                Recipe_Desc.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
        //                Recipe_Desc.Find("ItemDesc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
        //                Recipe_Desc.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text = PanelTextContent.textEmpty;
        //                Recipe_Desc.Find("WeightIcon").gameObject.SetActive(false);
        //                Recipe_Desc.Find("Weight").GetComponent<TMPro.TextMeshProUGUI>().text = "";
        //            }
        //            #endregion

        //            #region Raw
        //            List<Formula> recipeRaws = LocalGameManager.Instance.RecipeManager.GetRaw(CurrentRecipe);
        //            for (int i = 0; i < recipeRaws.Count; ++i)
        //            {
        //                var itemID = recipeRaws[i].id;
        //                var item = tempUIItemsRecipeRaw[i];
        //                item.SetActive(true);
        //                // Name
        //                var name = item.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
        //                name.text = ItemManager.Instance.GetItemName(itemID);
        //                if (name.text == "")
        //                {
        //                    name.text = PanelTextContent.textEmpty;
        //                }
        //                // NeedAmount
        //                var amount = item.transform.Find("NeedAmount").GetComponent<TMPro.TextMeshProUGUI>();
        //                amount.text = recipeRaws[i].num.ToString();
        //                // Icon
        //                if (ItemManager.Instance.IsValidItemID(itemID))
        //                {
        //                    var img = item.transform.Find("Icon").GetComponent<Image>();
        //                    if (!tempSprite.ContainsKey(itemID))
        //                    {
        //                        var sprite = ItemManager.Instance.GetItemSprite(itemID);
        //                        tempSprite[itemID] = sprite;
        //                    }
        //                    img.sprite = tempSprite[itemID];
        //                }
        //            }
        //            Recipe_Raw_GridLayout.transform.parent.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        //            LayoutRebuilder.ForceRebuildLayoutImmediate(Recipe_Raw_GridLayout.GetComponent<RectTransform>());
        //            #endregion
        //        }
        //    }
        //    #endregion

        //    #region TextContent
        //    private UIBtnList TestBtnList;
        //    protected override void InitBtnInfo()
        //    {
        //        TestBtnList = new UIBtnList(transform.Find("Restaurant").Find("Food").Find("Viewport").Find("UIBtnList").GetComponentInChildren<UIBtnListInitor>());
        //        TestBtnList.OnSelectButtonChanged += () =>
        //        {
        //            var template = TestBtnList.GetCurSelected()?.transform;
        //            if (template != null)
        //            {
        //                //(var x) => int.Parse(x.Find("Amount")) == 0;
        //            }
        //        };
        //    }

        [System.Serializable]
        public struct RestaurantPanel
        {
            public TextContent textTitleRestaurant;
            public TextContent textTitleChangeFood;
            public TextContent textEmpty;
            public TextContent textNo1;

            public KeyTip ChangeRecipe;
            public KeyTip Remove1;
            public KeyTip Remove10;
            public KeyTip FastAdd;
            public KeyTip Confirm;
            public KeyTip Back;
        }
        //    protected override void InitTextContentPathData()
        //    {
        //        this.abpath = "OC/Json/TextContent/Restaurant";
        //        this.abname = "RestaurantPanel";
        //        this.description = "RestaurantPanel数据加载完成";
        //    }
        //    #endregion
    }
}
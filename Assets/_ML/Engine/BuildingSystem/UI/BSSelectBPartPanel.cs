using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ML.Engine.BuildingSystem.UI.BSSelectBPartPanel;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ML.Engine.Utility;

namespace ML.Engine.BuildingSystem.UI
{
    /// <summary>
    /// 放置的一级
    /// </summary>
    public class BSSelectBPartPanel : Engine.UI.UIBasePanel<BSSelectBPartPanelStruct>
    {
        #region 载入资源
        public const string TCategorySpriteAtlasPath = "SA_UI_Category1";
        public const string TTypeABPath = "SA_UI_Category2";

        public int IsInit = -1;

        private SpriteAtlas typeAtlas = null,categoryAtlas = null;
        private MonoBuildingManager monoBM;

        //家具图集
        public const string FurnitureSpriteAtlasPath = "SA_UI_BuildIcon";
        public const string FurnitureThemeSpriteAtlasPath = "SA_UI_FurnitureTheme";
        private SpriteAtlas FurnitureSpriteAtlas = null, FurnitureThemeSpriteAtlas = null;

        private UICameraImage FurnitureUICameraImage = null;
        private UICameraImage ProNodeUICameraImage = null;
        private UICameraImage InteractUICameraImage = null;
        public Sprite GetCategorySprite(BuildingCategory1 category)
        {
            Sprite sprite = categoryAtlas.GetSprite(category.ToString());;
            if (sprite == null)
            {
                sprite = categoryAtlas.GetSprite("None");
            }
            return sprite;
        }
        public Sprite GetTypeSprite(BuildingCategory2 type)
        {
            Sprite sprite = typeAtlas.GetSprite(type.ToString());;
            if (sprite == null)
            {
                sprite = typeAtlas.GetSprite("None");
            }
            return sprite;
        }


        private void InitCategoryTexture2D()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(TCategorySpriteAtlasPath).Completed += (handle) =>
            {
                categoryAtlas = handle.Result as SpriteAtlas;
                //CanSelectCategory1 = BuildingManager.Instance.GetRegisteredCategory();
                //Array.Sort(CanSelectCategory1);
                //更改顺序为 交互建筑，生产节点，房屋，家具
                CanSelectCategory1 = new BuildingCategory1[4] {BuildingCategory1.Interact, BuildingCategory1 .ProNode,BuildingCategory1.House,BuildingCategory1.Furniture};
                ++IsInit;
                if (IsInit >= 1)
                {
                    InitAsset();
                }
            };
        }

        private void InitTypeTexture2D()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(TTypeABPath).Completed += (handle) =>
            {
                typeAtlas = handle.Result as SpriteAtlas;
                CanSelectCategory2 = BuildingManager.Instance.GetRegisteredType();
                Array.Sort(CanSelectCategory2);

                ++IsInit;
                if (IsInit >= 1)
                {
                    InitAsset();
                }
            };
        }

        private void InitFurnitureTexture2D()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(FurnitureSpriteAtlasPath).Completed += (handle) =>
            {
                FurnitureSpriteAtlas = handle.Result as SpriteAtlas;
            };
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(FurnitureThemeSpriteAtlasPath).Completed += (handle) =>
            {
                FurnitureThemeSpriteAtlas = handle.Result as SpriteAtlas;
            };
        }


        private void UnloadAsset()
        {
            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            abmgr.Release(this.categoryAtlas);
            abmgr.Release(this.typeAtlas);
        }

        private void InitAsset()
        {
            if (S_LastSelectedCategory1Index != -1)
            {
                this.SelectedCategory1Index = S_LastSelectedCategory1Index;
            }
            this.UpdatePlaceBuildingType(this.CanSelectCategory1[this.SelectedCategory1Index]);
            if (S_LastSelectedCategory2Index != -1)
            {
                this.SelectedCategory2Index = S_LastSelectedCategory2Index;
            }

            this.ClearInstance();

            foreach (var category in this.CanSelectCategory1)
            {
                var go = Instantiate<GameObject>(this.templateCategory.gameObject, this.categoryParent, false);
                go.GetComponentInChildren<Image>().sprite = GetCategorySprite(category);
                go.GetComponentInChildren<TextMeshProUGUI>().text = monoBM.Category1Dict[category.ToString()].GetDescription();
                go.SetActive(true);
                this.categoryInstance.Add(category, go.transform as RectTransform);
            }
            this.FurniturePanel.gameObject.SetActive(false);
            this.Refresh();
        }
        #endregion

        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;

        #region UIGO引用
        private Dictionary<BuildingCategory1, RectTransform> categoryInstance = new Dictionary<BuildingCategory1, RectTransform>();
        private Dictionary<BuildingCategory2, RectTransform> typeInstance = new Dictionary<BuildingCategory2, RectTransform>();

        private RectTransform categoryParent;
        private RectTransform templateCategory;
        private RectTransform typeParent;
        private RectTransform templateType;


        private Transform ChangeFurnitureDisplay;
        private TextMeshProUGUI TipText;
        private Transform FurniturePanel;
        private Transform SelectType;
        private Transform FurnitureCategoryBtnListTransform;
        private Transform FurnitureThemeBtnListTransform;

        private Transform SwitchPanel;

        #endregion

        #endregion

        #region Unity
        protected override void Awake()
        {
            InitCategoryTexture2D();
            InitTypeTexture2D();
            InitFurnitureTexture2D();
            monoBM = GameManager.Instance.GetLocalManager<MonoBuildingManager>();
            this.categoryParent = this.transform.Find("SelectCategory").Find("Content") as RectTransform;
            this.templateCategory = this.categoryParent.Find("CategoryTemplate") as RectTransform;
            this.templateCategory.gameObject.SetActive(false);

            this.typeParent = this.transform.Find("SelectType").Find("Content") as RectTransform;
            this.templateType = this.typeParent.Find("TypeTemplate") as RectTransform;
            this.templateType.gameObject.SetActive(false);

            this.ChangeFurnitureDisplay = this.transform.Find("SelectType").Find("ChangeFurnitureDisplay");
            this.TipText = this.ChangeFurnitureDisplay.Find("TipText").GetComponent<TextMeshProUGUI>();
            this.FurniturePanel = this.transform.Find("FurniturePanel");
            this.SelectType = this.transform.Find("SelectType");
            this.FurnitureCategoryBtnListTransform = this.SelectType.Find("FurnitureCategoryBtnList");
            this.FurnitureThemeBtnListTransform = this.SelectType.Find("FurnitureThemeBtnList");
            this.SwitchPanel = this.transform.Find("SwitchPanel");

            this.FurnitureUICameraImage = this.transform.Find("FurniturePanel").GetComponentInChildren<UICameraImage>(true);
            this.FurnitureUICameraImage.Init();
            this.ProNodeUICameraImage = this.transform.Find("SwitchPanel").Find("ProNodePanel").GetComponentInChildren<UICameraImage>(true);
            this.ProNodeUICameraImage.Init();
            this.InteractUICameraImage = this.transform.Find("SwitchPanel").Find("InteractPanel").GetComponentInChildren<UICameraImage>(true);
            this.InteractUICameraImage.Init();
        }

        #endregion

        #region Refresh
        public override void Refresh()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.templateCategory.parent.GetComponent<RectTransform>());
            if (IsInit < 1 || this.FurnitureDisplayBtnList == null || !this.objectPool.IsLoadFinish())
            {
                return;
            }
            this.ClearCategory2Instance();
            // 更换 Category1
            foreach (var instance in this.categoryInstance)
            {
                var img = instance.Value.GetComponentInChildren<Image>();
                if(instance.Key != this.SelectedCategory1)
                {
                    Disactive(img);
                }
                else
                {
                    Active(img);
                }
            }

            this.SwitchPanel.gameObject.SetActive(this.SelectedCategory1 != BuildingCategory1.Furniture);
            if (this.SelectedCategory1 != BuildingCategory1.Furniture)
            {
                this.SwitchUIBtnListContainer.SetIsEnableTrue();
                int index = (int)this.SelectedCategory1;
                for (int i = 0; i < SwitchPanel.childCount; i++)
                {
                    SwitchPanel.GetChild(i).gameObject.SetActive(i == index);
                }

                this.ProNodeUICameraImage.CameraParent.SetActive(this.SelectedCategory1 == BuildingCategory1.ProNode);
                this.InteractUICameraImage.CameraParent.SetActive(this.SelectedCategory1 == BuildingCategory1.Interact);

                UIBtnList uIBtnList = SwitchUIBtnListContainer.UIBtnLists[index];

                Synchronizer synchronizer = new Synchronizer(this.CanSelectCategory2.Length, () =>
                {
                    SwitchUIBtnListContainer.MoveToBtnList(uIBtnList);
                });

                uIBtnList.DeleteAllButton(() =>
                {
                    foreach (var category2 in this.CanSelectCategory2)
                    {
                        string classificationString = BuildingManager.Instance.GetOneBPartBuildingPartClassificationString(this.SelectedCategory1, category2);
                        uIBtnList.AddBtn("Prefab_BuildingSystem/Prefab_BS_FurnitureBtn.prefab", BtnAction: () =>
                        {
                            this.Placer.SelectedPartInstance = BuildingManager.Instance.GetOneBPartInstance(classificationString);
                            monoBM.PopAndPushPanel<BSPlaceModePanel>();
                        }, BtnSettingAction:
                        (btn) =>
                        {
                            btn.transform.Find("Image2").GetComponent<Image>().sprite = GetTypeSprite(category2);
                            btn.name = classificationString;
                        },
                        OnFinishAdd: () => { synchronizer.Check(); },
                        OnSelectEnter: () => {

                            this.RefreshBuildingInfo(classificationString);
                        },
                        BtnText: monoBM.Category2Dict[category2.ToString()].GetDescription());
                    }
                });
                
            }
            //家具特殊处理
            if (this.SelectedCategory1 == BuildingCategory1.Furniture)
            {
                this.SwitchUIBtnListContainer.SetIsEnableFalse();
                this.ProNodeUICameraImage.CameraParent.SetActive(false);
                this.InteractUICameraImage.CameraParent.SetActive(false);

                this.SelectType.Find("Content").gameObject.SetActive(false);
                //默认先用类别排序
                this.FurnitureCategoryBtnListTransform.gameObject.SetActive(true);
                this.ChangeFurnitureDisplay.gameObject.SetActive(true);
                this.TipText.text = this.PanelTextContent.ChangeFurnitureDisplay[0].GetText();

                this.FurnitureCategoryBtnList.OnSelectEnter();
                this.FurnitureCategoryBtnList.BindNavigationInputAction(this.Placer.BInput.BuildSelection.SwichBtn, UIBtnListContainer.BindType.started);
                this.FurnitureCategoryBtnList.BindButtonInteractInputAction(this.Placer.comfirmInputAction, UIBtnListContainer.BindType.started);

                this.FurnitureThemeBtnList.BindNavigationInputAction(this.Placer.BInput.BuildSelection.SwichBtn, UIBtnListContainer.BindType.started);
                this.FurnitureThemeBtnList.BindButtonInteractInputAction(this.Placer.comfirmInputAction, UIBtnListContainer.BindType.started);
            }
            else if(this.SelectedCategory1 != BuildingCategory1.Furniture)
            {
                this.SelectType.Find("Content").gameObject.SetActive(true);
                this.FurnitureCategoryBtnListTransform.gameObject.SetActive(false);
                this.FurnitureThemeBtnListTransform.gameObject.SetActive(false);

                this.ChangeFurnitureDisplay.gameObject.SetActive(false);
                
                this.FurnitureCategoryBtnList.OnSelectExit();
                this.FurnitureCategoryBtnList.DeBindInputAction();
                this.FurnitureCategoryBtnList.RemoveAllListener();
                this.FurnitureDisplayBtnList.OnSelectExit();
                this.FurnitureDisplayBtnList.DeleteAllButton(() => { this.FurniturePanel.gameObject.SetActive(false); });
                this.FurnitureDisplayBtnList.DeBindInputAction();
                this.FurnitureDisplayBtnList.RemoveAllListener();
            }
        }

        private void RefreshBuildingInfo(string CID)
        {
            Transform Content = null;
            if (this.SelectedCategory1 == BuildingCategory1.ProNode)
            {
                var ProNodePanel = this.SwitchPanel.Find("ProNodePanel");
                Content = ProNodePanel.Find("Info").Find("Content");
                Content.Find("Name").GetComponent<TextMeshProUGUI>().text = BuildingManager.Instance.GetName(CID);
                Content.Find("Description").GetComponent<TextMeshProUGUI>().text = BuildingManager.Instance.GetItemDescription(CID);
                Content.Find("Effect").GetComponent<TextMeshProUGUI>().text = BuildingManager.Instance.GetEffectDescription(CID);
                string ProNodeID = BuildingManager.Instance.GetActorID(CID);
                bool canCharge = LocalGameManager.Instance.ProNodeManager.GetCanCharge(ProNodeID);
                bool isAuto = LocalGameManager.Instance.ProNodeManager.GetIsAuto(ProNodeID);
                ProNodePanel.Find("Info").Find("PowerSupply").gameObject.SetActive(canCharge);
                ProNodePanel.Find("Info").Find("Auto").gameObject.SetActive(isAuto);
            }
            else if(this.SelectedCategory1 == BuildingCategory1.Interact)
            {
                var InteractPanel = this.SwitchPanel.Find("InteractPanel");
                Content = InteractPanel.Find("Info").Find("Content");
                Content.Find("Name").GetComponent<TextMeshProUGUI>().text = BuildingManager.Instance.GetName(CID);
                Content.Find("Description").GetComponent<TextMeshProUGUI>().text = BuildingManager.Instance.GetItemDescription(CID);
                Content.Find("Effect").GetComponent<TextMeshProUGUI>().text = BuildingManager.Instance.GetEffectDescription(CID);
            }
        }

        /// <summary>
        /// to-yl
        /// </summary>
        /// <param name="img"></param>
        private void Active(Image img)
        {
            img.transform.localScale = Vector3.one * 1.2f;
        }

        /// <summary>
        /// to-yl
        /// </summary>
        /// <param name="img"></param>
        private void Disactive(Image img)
        {
            img.transform.localScale = Vector3.one;
        }

        private void ClearInstance()
        {
            foreach (var instance in this.categoryInstance.Values)
            {
                Manager.GameManager.DestroyObj(instance.GetComponentInChildren<Image>().sprite);
                Manager.GameManager.DestroyObj(instance.gameObject);
            }
            this.categoryInstance.Clear();
            foreach (var instance in this.typeInstance.Values)
            {
                Manager.GameManager.DestroyObj(instance.GetComponentInChildren<Image>().sprite);
                Manager.GameManager.DestroyObj(instance.gameObject);
            }
            this.typeInstance.Clear();
        }

        private void ClearCategory2Instance()
        {
            foreach (var instance in this.typeInstance.Values)
            {
                Manager.GameManager.DestroyObj(instance.GetComponentInChildren<Image>().sprite);
                Manager.GameManager.DestroyObj(instance.gameObject);
            }
            this.typeInstance.Clear();
        }
        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.Placer.Mode = BuildingMode.Place;
            this.Placer.InteractBPartList.Clear();
            this.Placer.SelectedPartInstance = null;


        }
        public override void OnExit()
        {
            S_LastSelectedCategory1Index = this.SelectedCategory1Index;
            S_LastSelectedCategory2Index = this.SelectedCategory2Index;
            base.OnExit();
            this.ClearInstance();
            this.UnloadAsset();
            this.FurnitureUICameraImage.DisableUICameraImage();
            this.ProNodeUICameraImage.DisableUICameraImage();
            this.InteractUICameraImage.DisableUICameraImage();
        }

        protected override void Exit()
        {
            base.Exit();
            this.FurnitureCategoryBtnList.DeBindInputAction();
            this.FurnitureCategoryBtnList.RemoveAllListener();

            this.FurnitureDisplayBtnList.DeBindInputAction();
            this.FurnitureDisplayBtnList.RemoveAllListener();

            this.FurnitureThemeBtnList.DeBindInputAction();
            this.FurnitureThemeBtnList.RemoveAllListener();
        }

        #endregion

        #region KeyFunction
        protected override void UnregisterInput()
        {
            this.Placer.BInput.BuildSelection.Disable();

            this.Placer.comfirmInputAction.performed -= Placer_ComfirmSelection;
            this.Placer.backInputAction.performed -= Placer_CancelSelection;
            this.Placer.BInput.BuildSelection.AlterCategory.performed -= Placer_AlterCategory1;

            this.Placer.BInput.BuildSelection.AlternativeType.started -= Placer_AlterCategory2;
            this.Placer.BInput.BuildSelection.ChangeFurnitureDisplay.performed -= ChangeFurnitureSortWay;

            this.SwitchUIBtnListContainer.DeBindNavigationInputAction();
        }

        protected override void RegisterInput()
        {
            this.Placer.BInput.BuildSelection.Enable();

            this.Placer.comfirmInputAction.performed += Placer_ComfirmSelection;
            this.Placer.backInputAction.performed += Placer_CancelSelection;
            this.Placer.BInput.BuildSelection.AlterCategory.performed += Placer_AlterCategory1;

            this.Placer.BInput.BuildSelection.AlternativeType.started += Placer_AlterCategory2;
            this.Placer.BInput.BuildSelection.ChangeFurnitureDisplay.performed += ChangeFurnitureSortWay;

            this.SwitchUIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
        }

        private void Placer_AlterCategory2(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            /*if (this.SelectedCategory1 == BuildingCategory1.Furniture)
            {
                return;
            }
            int offset = obj.ReadValue<float>() > 0 ? 1 : -1;
            this.SelectedCategory2Index = (this.SelectedCategory2Index + offset + this.CanSelectCategory2.Length) % this.CanSelectCategory2.Length;

            this.Refresh();*/
        }

        private void Placer_AlterCategory1(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(CanSelectCategory1 != null)
            {
                int offset = obj.ReadValue<float>() > 0 ? 1 : -1;
                this.SelectedCategory1Index = (this.SelectedCategory1Index + this.CanSelectCategory1.Length + offset) % this.CanSelectCategory1.Length;
                this.UpdatePlaceBuildingType(this.CanSelectCategory1[this.SelectedCategory1Index]);
                
            }
        }

        private void Placer_CancelSelection(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(this.FurniturePanel.gameObject.activeInHierarchy == false)
            {
                this.Placer.Mode = BuildingMode.Interact;
                monoBM.PopPanel();
            }
            else
            {
                this.FurnitureDisplayBtnList.OnSelectExit();
                this.FurnitureDisplayBtnList.DeleteAllButton(() => { this.FurniturePanel.gameObject.SetActive(false); });
       
                if (this.FurnitureCategoryBtnListTransform.gameObject.activeInHierarchy == true)
                {
                    this.FurnitureCategoryBtnList.EnableBtnList();
                }
                else
                {
                    this.FurnitureThemeBtnList.EnableBtnList();
                }
            }
        }
        private void Placer_ComfirmSelection(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.SelectedCategory1 == BuildingCategory1.Furniture) 
            {
                if(this.FurniturePanel.gameObject.activeInHierarchy == false)
                {
                    if (this.FurnitureCategoryBtnListTransform.gameObject.activeInHierarchy == true)
                    {
                        this.FurnitureCategoryBtnList.DisableBtnList();
                        //当前为类别排序


                        //当前所选类别对应的家具列表
                        List<(int, IBuildingPart)> furnitureList = new List<(int, IBuildingPart)>();

                        var IBuildingPartList = BuildingManager.Instance.GetFurnitureIBuildingParts((BuildingCategory2)Enum.Parse(typeof(BuildingCategory2), this.FurnitureCategoryBtnList.GetCurSelected().gameObject.name));

                        foreach ( var ib in IBuildingPartList)
                        {
                            int s = BuildingManager.Instance.GetSort(ib.Classification.ToString());
                            if (s == -1) continue;
                            furnitureList.Add((s, ib));
                        }
                        
                        furnitureList.Sort((x, y) => x.Item1.CompareTo(y.Item1));

                        Synchronizer synchronizer = new Synchronizer(furnitureList.Count, () => 
                        { 
                            this.FurniturePanel.gameObject.SetActive(true);
                            this.FurnitureDisplayBtnList.OnSelectEnter();
                            this.FurnitureDisplayBtnList.BindNavigationInputAction(this.Placer.BInput.BuildSelection.SwichBtn, UIBtnListContainer.BindType.started);
                            this.FurnitureDisplayBtnList.BindButtonInteractInputAction(this.Placer.comfirmInputAction, UIBtnListContainer.BindType.started);
                        });

                        foreach (var item in furnitureList)
                        {
                            string BuildingName = BuildingManager.Instance.GetName(item.Item2.Classification.ToString());
                            this.FurnitureDisplayBtnList.AddBtn("Prefab_BuildingSystem/Prefab_BS_FurnitureBtn.prefab", BtnAction: () =>
                            {
                                this.Placer.SelectedPartInstance = BuildingManager.Instance.GetOneBPartInstance(item.Item2.Classification);
                                monoBM.PopAndPushPanel<BSPlaceModePanel>();
                            }, BtnSettingAction:
                            (btn) =>
                            {
                                btn.transform.Find("Image2").GetComponent<Image>().sprite = FurnitureSpriteAtlas.GetSprite(BuildingManager.Instance.GetFurtureBuildingIcon(item.Item2.Classification.ToString()));
                                btn.name = item.Item2.Classification.ToString();
                            }
                            , BtnText: BuildingName, OnFinishAdd: () => { synchronizer.Check(); });
                        }
                    }
                    else
                    {
                        this.FurnitureThemeBtnList.DisableBtnList();
                        //当前为主题排序

                        //当前选中的主题ID
                        var curSelectedThemeID = this.FurnitureThemeBtnList.GetCurSelected().gameObject.name;
                        List<string> Buildings = BuildingManager.Instance.GetThemeContainBuildings(curSelectedThemeID);
                        Synchronizer synchronizer = new Synchronizer(Buildings.Count, () => 
                        { 
                            this.FurniturePanel.gameObject.SetActive(true);
                            this.FurnitureDisplayBtnList.OnSelectEnter();
                            this.FurnitureDisplayBtnList.BindNavigationInputAction(this.Placer.BInput.BuildSelection.SwichBtn, UIBtnListContainer.BindType.started);
                            this.FurnitureDisplayBtnList.BindButtonInteractInputAction(this.Placer.comfirmInputAction, UIBtnListContainer.BindType.started);
                        });
                        foreach (var buildingID in Buildings)
                        {
                            
                            string classification = BuildingManager.Instance.GetClassification(buildingID);
                            string BuildingName = BuildingManager.Instance.GetName(classification);
                            this.FurnitureDisplayBtnList.AddBtn("Prefab_BuildingSystem/Prefab_BS_FurnitureBtn.prefab", BtnAction: () =>
                            {
                                this.Placer.SelectedPartInstance = BuildingManager.Instance.GetOneBPartInstance(classification);
                                monoBM.PopAndPushPanel<BSPlaceModePanel>();
                            }, BtnSettingAction:
                            (btn) =>
                            {
                                btn.transform.Find("Image2").GetComponent<Image>().sprite = FurnitureSpriteAtlas.GetSprite(BuildingManager.Instance.GetFurtureBuildingIcon(classification));
                                btn.name = classification;
                            }
                            , BtnText: BuildingName, OnFinishAdd: () => { synchronizer.Check(); });
                        }
                    }

                    
                }
                else
                {
                    //this.FurnitureDisplayBtnList.GetCurSelected()?.Interact();
                }
            }
            /*else
            {
                this.Placer.SelectedPartInstance = BuildingManager.Instance.GetOneBPartInstance(this.CanSelectCategory1[this.SelectedCategory1Index], this.CanSelectCategory2[this.SelectedCategory2Index]);
                monoBM.PopAndPushPanel<BSPlaceModePanel>();
            }*/
            
        }


        private bool isFirstAddThemeBtn = true;
        private void ChangeFurnitureSortWay(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.SelectedCategory1 != BuildingCategory1.Furniture)
            {
                return;
            }

            if (this.FurniturePanel.gameObject.activeInHierarchy == true)
            {
                this.FurnitureDisplayBtnList.DeleteAllButton(() => { this.FurniturePanel.gameObject.SetActive(false); });
                this.FurnitureDisplayBtnList.DisableBtnList();
            }

            if(this.FurnitureCategoryBtnListTransform.gameObject.activeInHierarchy == true)
            {
                this.TipText.text = this.PanelTextContent.ChangeFurnitureDisplay[1].GetText();
                this.FurnitureCategoryBtnListTransform.gameObject.SetActive(false);
                this.FurnitureCategoryBtnList.DisableBtnList();
                this.FurnitureThemeBtnList.EnableBtnList();
                if (isFirstAddThemeBtn)
                {
                    Synchronizer synchronizer = new Synchronizer(BuildingManager.Instance.FurnitureThemeTableDataDic.Count, () => {
                        this.FurnitureThemeBtnListTransform.gameObject.SetActive(true);
                    });
                    //给主题btnlist加入按钮
                    foreach (var (id, data) in BuildingManager.Instance.FurnitureThemeTableDataDic)
                    {
                        this.FurnitureThemeBtnList.AddBtn("Prefab_BuildingSystem/Prefab_BS_ThemeBtn.prefab",BtnSettingAction: (btn) =>
                        {
                            btn.gameObject.name = data.ID;
                            btn.transform.Find("Image").Find("Image1").GetComponent<Image>().sprite = FurnitureThemeSpriteAtlas.GetSprite(data.Icon);
                        }, BtnText: data.Name,OnFinishAdd: () => { synchronizer.Check(); });
                    }
                    isFirstAddThemeBtn = false;
                }
                else
                {
                    this.FurnitureThemeBtnListTransform.gameObject.SetActive(true);
                }
            }
            else
            {
                this.TipText.text = this.PanelTextContent.ChangeFurnitureDisplay[0].GetText();
                this.FurnitureThemeBtnListTransform.gameObject.SetActive(false);
                this.FurnitureThemeBtnList.DisableBtnList();
                this.FurnitureCategoryBtnListTransform.gameObject.SetActive(true);
                this.FurnitureCategoryBtnList.EnableBtnList();
                
            }
        }
        #endregion

        #region Select
        public int S_LastSelectedCategory1Index
        {
            get => BSInteractModePanel.S_LastSelectedCategory1Index;
            set => BSInteractModePanel.S_LastSelectedCategory1Index = value;
        }
        public int S_LastSelectedCategory2Index
        {
            get => BSInteractModePanel.S_LastSelectedCategory2Index;
            set => BSInteractModePanel.S_LastSelectedCategory2Index = value;
        }

        /// <summary>
        /// 当前选中的Category1Index
        /// </summary>
        [ShowInInspector, LabelText("Category1Index"), FoldoutGroup("PlaceMode")]
        protected int SelectedCategory1Index = 0;
        /// <summary>
        /// 当前选中的Category2Index
        /// </summary>
        [ShowInInspector, LabelText("Category2Index"), FoldoutGroup("PlaceMode")]
        protected int SelectedCategory2Index = 0;
        /// <summary>
        /// 当前可选的Category1
        /// </summary>
        [ShowInInspector, LabelText("Category1Array"), FoldoutGroup("PlaceMode")]
        protected BuildingCategory1[] CanSelectCategory1;
        /// <summary>
        /// 当前可选的Category2
        /// </summary>
        [ShowInInspector, LabelText("Category2Array"), FoldoutGroup("PlaceMode")]
        protected BuildingCategory2[] CanSelectCategory2;
        /// <summary>
        /// 选择的Category1
        /// </summary>
        public BuildingCategory1 SelectedCategory1 => this.CanSelectCategory1[this.SelectedCategory1Index];
        /// <summary>
        /// 选择的Category2
        /// </summary>
        public BuildingCategory2 SelectedCategory2 => this.CanSelectCategory2[this.SelectedCategory2Index];

        protected void UpdatePlaceBuildingType(BuildingCategory1 category)
        {
            this.CanSelectCategory2 = BuildingManager.Instance.GetRegisteredType().Where(type => (int)type >= ((int)category * 100) && (int)type < ((int)category * 100 + 100)).ToArray();
            Array.Sort(CanSelectCategory2);

            this.SelectedCategory2Index = 0;
            if (this.SwitchUIBtnListContainer != null && this.SwitchUIBtnListContainer.CurSelectUIBtnList != null)
            {
                this.SwitchUIBtnListContainer.CurSelectUIBtnList.DeleteAllButton(() => { this.Refresh(); });
            }
            else
            {
                this.Refresh();
            }
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct BSSelectBPartPanelStruct
        {
            public KeyTip[] KeyTips;

            public TextTip[] FurnitureCategoryBtns;

            public ML.Engine.TextContent.TextContent[] ChangeFurnitureDisplay;
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/BuildingSystem/UI";
            this.abname = "BSSelectBPartPanel";
            this.description = "BSSelectBPartPanel数据加载完成";
        }
        #endregion

        #region ButtonList
        [ShowInInspector]
        private UIBtnList FurnitureDisplayBtnList = null;
        [ShowInInspector]
        private UIBtnList FurnitureCategoryBtnList = null;
        [ShowInInspector]
        private UIBtnList FurnitureThemeBtnList = null;
        [ShowInInspector]
        private UIBtnListContainer SwitchUIBtnListContainer = null;
        protected override void InitBtnInfo()
        {
            this.FurnitureDisplayBtnList = new UIBtnList(this.FurniturePanel.Find("ButtonList").GetComponent<UIBtnListInitor>());
            this.FurnitureCategoryBtnList = new UIBtnList(this.FurnitureCategoryBtnListTransform.GetComponent<UIBtnListInitor>());
            this.FurnitureThemeBtnList = new UIBtnList(this.FurnitureThemeBtnListTransform.GetComponent<UIBtnListInitor>());
            this.SwitchUIBtnListContainer = new UIBtnListContainer(this.transform.Find("SwitchPanel").GetComponent<UIBtnListContainerInitor>());
            this.Refresh();
        }
        protected override void OnLoadJsonAssetComplete(BSSelectBPartPanelStruct datas)
        {
            base.OnLoadJsonAssetComplete(datas);
            InitBtnData(datas);
        }
        private void InitBtnData(BSSelectBPartPanelStruct datas)
        {
            foreach (var tt in datas.FurnitureCategoryBtns)
            {
                this.FurnitureCategoryBtnList.SetBtnText(tt.name, tt.description.GetText());
            }

            this.FurnitureDisplayBtnList.OnSelectButtonChanged += () => {

                var btn = this.FurnitureDisplayBtnList.GetCurSelected();
                string curSelectedBuilding = btn != null ? btn.name : "";
                if (curSelectedBuilding == "") return;
                GameObject buildingPart = BuildingManager.Instance.GetOneBPartInstanceGO(curSelectedBuilding);
                this.FurnitureUICameraImage.LookAtGameObject(buildingPart);
            };

            //ProNode
            this.SwitchUIBtnListContainer.UIBtnLists[0].OnSelectButtonChanged += () => {

                var btn = this.SwitchUIBtnListContainer.CurSelectUIBtnList?.GetCurSelected();
                string curSelectedBuilding = btn != null ? btn.name : "";
                if (curSelectedBuilding == "") return;
                GameObject buildingPart = BuildingManager.Instance.GetOneBPartInstanceGO(curSelectedBuilding);
                this.ProNodeUICameraImage.LookAtGameObject(buildingPart);
            };
            //Interact
            this.SwitchUIBtnListContainer.UIBtnLists[1].OnSelectButtonChanged += () => {

                var btn = this.SwitchUIBtnListContainer.CurSelectUIBtnList?.GetCurSelected();
                string curSelectedBuilding = btn != null ? btn.name : "";
                if (curSelectedBuilding == "") return;
                GameObject buildingPart = BuildingManager.Instance.GetOneBPartInstanceGO(curSelectedBuilding);
                this.InteractUICameraImage.LookAtGameObject(buildingPart);
            };

            foreach (var uIBtnList in this.SwitchUIBtnListContainer.UIBtnLists)
            {
                uIBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.performed);
            }

        }
        #endregion
    }

}

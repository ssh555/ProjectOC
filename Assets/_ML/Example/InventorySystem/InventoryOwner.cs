using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Example.InventorySystem.CompositeSystem.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Example.InventorySystem
{
    [RequireComponent(typeof(Collider))]
    public class InventoryOwner : MonoBehaviour, CompositeAbility
    {
        #region 杂项
        /// <summary>
        /// 是否打开
        /// </summary>
        public bool IsOpen 
        { 
            get
            {
                return this.hudChildInventoryUI != null && hudChildInventoryUI.gameObject.activeSelf;
            } 
        }

        /// <summary>
        /// MainHUD.Parent => 寻找场景中的 "Canvas" 物体
        /// </summary>
        public Transform Canvas;

        /// <summary>
        /// MainHUD Prefab
        /// </summary>
        [ShowInInspector, SerializeField]
        private GameObject MainHUD;

        /// <summary>
        /// MainHUD Instance
        /// </summary>
        private RectTransform openMainHUD;
        /// <summary>
        /// Inventory
        /// </summary>
        private RectTransform hudChildInventoryUI;
        private Transform hudSocket;


#if !ENABLE_INPUT_SYSTEM
        [LabelText("背包按键")]
        public KeyCode SHIKey;
#endif

        private GameObject _curObject = null;
        /// <summary>
        /// 当前处于激活态的窗口控件
        /// </summary>
        protected GameObject CurWidget
        {
            get => this._curObject;
            set
            {
                if(this._curObject != null)
                {
                    this._curObject.SetActive(false);
                }
                this._curObject = value;
                this._curObject.SetActive(true);
            }
        }
        #endregion
        
        #region Inventory
        private Inventory inventory;

        public int MaxSize = 65;

        [ShowInInspector, SerializeField]
        private GameObject uIInventory;


        [ShowInInspector, SerializeField, ReadOnly]
        private UIInventory openInventory;
#if !ENABLE_INPUT_SYSTEM
        [LabelText("拾取按键")]
        public KeyCode PickUpKey;
#endif

        private WorldItem _pickUpItem = null;
        #endregion

        #region CompositeAbility
        public Inventory ResourceInventory { get => inventory;}
        public CompositeAbility compositeAbility { get => (this as CompositeAbility); }


        [ShowInInspector, SerializeField]
        private GameObject uICompositeSystem;

        [ShowInInspector, SerializeField, ReadOnly]
        private UICompositeSystem openCompositeSystem;
        #endregion

        #region Unity_Method
        /// <summary>
        /// 实例化背包
        /// </summary>
        private void Awake()
        {
            inventory = new Inventory(this.MaxSize, this.transform);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Start()
        {
            this.Init();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Input.InputManager.GetInputs.PlayerUI.OpenUI.WasPressedThisFrame() || Input.InputManager.GetInputs.PlayerUI.CloseUI.WasPressedThisFrame())
#else
            if (Input.GetKeyDown(this.SHIKey))
#endif
            {
                this.hudChildInventoryUI.gameObject.SetActive(!this.hudChildInventoryUI.gameObject.activeSelf);

#if ENABLE_INPUT_SYSTEM
                if (this.IsOpen)
                {
                    Input.InputManager.GetInputs.Player.Disable();
                }
                else
                {
                    Input.InputManager.GetInputs.Player.Enable();
                }
#endif
            }

            if (this._pickUpItem)
            {
#if ENABLE_INPUT_SYSTEM
                if (Input.InputManager.GetInputs.Player.PickUp.WasPressedThisFrame())
#else
            if (Input.GetKeyDown(this.PickUpKey))
#endif
                {
                    this._pickUpItem.PickUp(this.inventory);
                    this._pickUpItem = null;
                }
            }

        }
        
        private void OnTriggerStay(Collider other)
        {
            if(this._pickUpItem != null)
            {
                return;
            }
            this._pickUpItem = other.GetComponent<WorldItem>();
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.GetComponent<WorldItem>() == this._pickUpItem)
            {
                this._pickUpItem = null;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            if (Canvas == null)
            {
                Canvas = GameObject.Find("Canvas").transform;
            }

            this.MainHUD = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<GameObject>("ui/prefabs", "MainHUD", (asyncOpt) =>
            {
                this.openMainHUD = UnityEngine.Object.Instantiate(this.MainHUD.transform, Canvas) as RectTransform;
                this.hudChildInventoryUI = this.openMainHUD.Find("UIClassificationContainer") as RectTransform;

                this.hudChildInventoryUI.gameObject.SetActive(false);
                this.hudSocket = this.openMainHUD.Find("UIClassificationContainer").Find("Socket");
            }).asset as GameObject;

            var btnList = this.openMainHUD.GetChild(0).GetComponent<InventorySystem.MainHUD.UI.UIClassificationContainer>();
            btnList.inventoryBtn.gameObject.SetActive(false);
            btnList.compositeBtn.gameObject.SetActive(false);
            this.uIInventory = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<GameObject>("ui/prefabs", "UI_Inventory", (asyncOpt) =>
            {
                btnList.inventoryBtn.OnActiveListener += (pre, post) =>
                {
                    this.CreateUIInventory();
                };
                btnList.inventoryBtn.gameObject.SetActive(true);
            }).asset as GameObject;
            this.uICompositeSystem = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<GameObject>("ui/prefabs", "UICompositePanel", (asyncOpt) =>
            {
                btnList.compositeBtn.OnActiveListener += (pre, post) =>
                {
                    this.CreateUICompositeSystem();
                };
                btnList.compositeBtn.gameObject.SetActive(true);
            }).asset as GameObject;
        }

        public void CloseInventory()
        {
            Input.InputManager.GetInputs.Player.Enable();
            this.hudChildInventoryUI.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void CreateUIInventory()
        {
            if(openInventory == null)
            {
                openInventory = UnityEngine.Object.Instantiate(this.uIInventory.transform, this.openMainHUD).GetComponent<UIInventory>();
                openInventory.SetInventory(this.inventory);
                openInventory.transform.SetParent(this.hudSocket);
                RectTransform rect = openInventory.transform as RectTransform;
                rect.offsetMax = Vector2.zero;
                rect.offsetMin = Vector2.zero;
            }
            CurWidget = openInventory.gameObject;
        }

        private void CreateUICompositeSystem()
        {
            if(openCompositeSystem == null)
            {
                openCompositeSystem = Instantiate(this.uICompositeSystem.transform, this.openMainHUD).GetComponent<UICompositeSystem>();
                openCompositeSystem.Owner = this;
                openCompositeSystem.transform.SetParent(this.hudSocket);
                RectTransform rect = openCompositeSystem.transform as RectTransform;
                rect.offsetMax = Vector2.zero;
                rect.offsetMin = Vector2.zero;
            }
            CurWidget = openCompositeSystem.gameObject;
        }

        public IComposition Composite(string compositonID)
        {
            // 一次性合成
            IComposition ret = ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.Composite(this.ResourceInventory, compositonID);

            if(ret is Item)
            {
                this.ResourceInventory.AddItem(ret as Item);
            }
            return ret;
        }
    
        /// <summary>
        /// 根据合成物ID 消耗对应资源
        /// </summary>
        public void CostResourcesFromCompID(string compositonID)
        {
            // 移除消耗的资源
            lock (this.inventory)
            {
                foreach (var formula in ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.GetCompositonFomula(compositonID))
                {
                    this.inventory.RemoveItem(formula.id, formula.num);
                }
            }
        }
        #endregion
    }
}


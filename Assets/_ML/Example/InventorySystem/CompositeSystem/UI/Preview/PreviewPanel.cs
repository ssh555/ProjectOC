using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ML.Example.Input;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.InventorySystem;

namespace ML.Example.InventorySystem.CompositeSystem.UI
{
    public class PreviewPanel : MonoBehaviour
    {
        protected int _id = -1;
        public int ID
        {
            get => this._id;
            set
            {
                this._id = value;
                this.RefreshPreviewPanel();
            }
        }
        
        public UICompositeSystem Owner;

        #region PreviewContainer
        protected RectTransform previewContainer;
        #endregion

        #region FormulaContainer
        protected FormulaItemContainer formulaContainer;
        #endregion

        #region UsageContainer
        protected UsageContainer usageContainer;
        #endregion

        #region ButtonContainer
        protected RectTransform buttonContainer;

        protected Button compositeOneBtn;
        #endregion

#if !ENABLE_INPUT_SYSTEM
        [LabelText("ºÏ³É¿ì½Ý¼ü")]
        public KeyCode compositeKey;
#endif

        private void Awake()
        {
            Transform content = this.transform.GetChild(0).GetChild(0);

            this.previewContainer = content.Find("PreviewContainer") as RectTransform;
            this.formulaContainer = content.Find("FormulaContainer").GetComponent<FormulaItemContainer>();
            this.formulaContainer.Owner = this;
            this.usageContainer = content.Find("UsageContainer").GetComponent<UsageContainer>();
            this.usageContainer.Owner = this;
            this.buttonContainer = content.Find("ButtonContainer") as RectTransform;

            this.compositeOneBtn = this.buttonContainer.Find("CompositeOne").GetComponent<Button>();
            this.compositeOneBtn.onClick.AddListener(this.Composite);

            this.RefreshPreviewPanel();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (InputManager.GetInputs.PlayerUI.Composite.WasPressedThisFrame() && this.Owner.Owner.CanComposite(this.ID))
#else
            if(Input.GetKeyDown(this.compositeKey) && this.Owner.Owner.CanComposite(this.ID))
#endif
            {
                this.Composite();
            }
        }

        protected void Composite()
        {
            IComposition composition = this.Owner.Owner.Composite(this.ID);
            this.RefreshPreviewPanel();
        }


        public void RefreshPreviewPanel()
        {
            // Preview : None
            if(this.ID < 0)
            {
                this.previewContainer.gameObject.SetActive(false);
                this.RefreshFormulaContainer(null);
                this.usageContainer.gameObject.SetActive(false);
                this.buttonContainer.gameObject.SetActive(false);
                return;
            }
            // PreViewContainer
            this.previewContainer.GetChild(0).GetComponent<Image>().sprite = ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.GetCompositonSprite(this._id);
            this.previewContainer.GetChild(1).GetComponent<Text>().text = ItemSpawner.Instance.GetItemName(this.ID) + "   Own: " + this.Owner.Owner.ResourceInventory.GetItemAllNum(this.ID);
            this.previewContainer.gameObject.SetActive(true);

            // FormulaContainer
            this.RefreshFormulaContainer(ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.GetCompositonFomula(this.ID));

            // UsageContainer
            this.RefreshUsageContainer(ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.GetCompositionUsage(this.ID));

            // ButtonContainer
            this.RefreshButtonContainer();
        }

        protected void RefreshFormulaContainer(int[][] formulas)
        {
            this.formulaContainer.RefreshFormulaContainer(formulas);
        }

        protected void RefreshUsageContainer(int[] usage)
        {
            this.usageContainer.RefreshUsageContainer(usage);
        }

        protected void RefreshButtonContainer()
        {
            this.compositeOneBtn.interactable = this.Owner.Owner.CanComposite(this.ID);

            this.buttonContainer.gameObject.SetActive(true);
        }
    }
}

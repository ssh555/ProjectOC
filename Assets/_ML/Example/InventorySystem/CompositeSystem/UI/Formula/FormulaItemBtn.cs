using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ML.Example.InventorySystem.CompositeSystem.UI
{
    public class FormulaItemBtn : ML.Example.UI.SingletonActivedButton, IPointerClickHandler
    {
        public FormulaItemContainer Owner;

        protected string _id;
        public string ID
        {
            get => this._id;
            set
            {
                this._id = value;
                // id
                this.transform.GetChild(0).GetComponent<Image>().sprite = ML.Engine.InventorySystem.CompositeSystem.CompositeManager.Instance.GetCompositonSprite(this._id);
                // own num
                this.transform.GetChild(2).GetComponent<Text>().text = this.Owner.Owner.Owner.Owner.ResourceInventory.GetItemAllNum(value).ToString();
            }
        }
        protected int _num;
        public int Num
        {
            get => this._num;
            set
            {
                this._num = value;
                // need num
                this.transform.GetChild(1).GetComponent<Text>().text = value.ToString();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            this.PointerDownForActivedListener += (eventData) =>
            {
                SetActived(this, this.GetType());
            };
            // ClickDown
            this.OnActiveListener += (ML.Example.UI.SingletonActivedButton pre, ML.Example.UI.SingletonActivedButton post) =>
            {
                this.rectTransform.localScale = new Vector3(0.9f, 0.9f, 1f);
                this.GetComponent<Image>().color = new Color(80f / 256, 80f / 256, 80f / 256, 150f / 256);
            };

            this.OnDisactiveListener += (ML.Example.UI.SingletonActivedButton pre, ML.Example.UI.SingletonActivedButton post) =>
            {
                this.GetComponent<Image>().color = Color.white;
                this.rectTransform.localScale = Vector3.one;
            };

            // PointerEnter
            this.PointerEnterForActivedListener += (PointerEventData eventData) =>
            {
                this.rectTransform.localScale = new Vector3(1.1f, 1.1f, 1f);
            };
            // PointerExit
            this.PointerExitForActivedListener += (PointerEventData eventData) =>
            {
                this.rectTransform.localScale = Vector3.one;
            };
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                this.Owner.Owner.Owner.ActiveCompositionID(this.ID);
            }
        }
    }
}

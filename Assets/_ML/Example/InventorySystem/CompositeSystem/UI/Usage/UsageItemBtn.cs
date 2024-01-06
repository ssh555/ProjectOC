using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ML.Example.InventorySystem.CompositeSystem.UI
{
    public class UsageItemBtn : ML.Example.UI.SingletonActivedButton, IPointerClickHandler
    {
        public UsageContainer Owner;
        protected string _id;
        public string ID
        {
            get => this._id;
            set
            {
                this._id = value;
                // id
                this.transform.GetComponent<Image>().sprite = ML.Engine.InventorySystem.CompositeSystem.CompositeManager.Instance.GetCompositonSprite(this._id);
            }
        }
        
        protected override void Awake()
        {
            base.Awake();

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

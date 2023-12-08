using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ML.Example.InventorySystem.CompositeSystem.UI
{
    public class CompositionSelectionBtn : ML.Example.UI.SingletonActivedButton
    {
        protected string _id;
        public string ID
        {
            get => this._id;
            set
            {
                this._id = value;
                this.GetComponent<Image>().sprite = ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.GetCompositonSprite(this._id);
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


    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ML.Example.InventorySystem.CompositeSystem.UI
{
    public class ClassificationSelectionBtn : ML.Example.UI.SingletonActivedButton
    {
        protected string _primaryTag;
        public string PrimaryTag
        {
            get => this._primaryTag;
            set
            {
                this._primaryTag = value;
                Texture2D tex = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAsset<Texture2D>("UI/Sprite", value, false);

                this.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }

        /// <summary>
        /// <tag, IDList>
        /// </summary>
        public Dictionary<string, List<string>> CataloguePair;

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
                this.GetComponent<Image>().color = new Color(80f/256, 80f/256, 80f/256, 150f/256);
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


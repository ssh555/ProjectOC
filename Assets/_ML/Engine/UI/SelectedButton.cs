using ML.Engine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ML.Engine.UI
{
    [System.Serializable]
    public class SelectedButton : Button
    {
        public event System.Action OnSelectedEnter;
        public event System.Action OnSelectedExit;

        public System.Action PreInteract;
        public System.Action PostInteract;
        private Transform Selected = null;
        protected UIBtnList UIBtnList = null;
        protected override void Start()
        {
            
        }

        // 定义按钮是否可交互的属性
        public bool Interactable
        {
            get { return interactable; }
            set
            {
                // 如果新值与旧值相同，则不执行任何操作
                if (interactable == value)
                    return;

                // 更新按钮是否可交互的状态
                interactable = value;

                // 根据按钮是否可交互执行不同的逻辑
                if (interactable)
                {
                    OnInteractableEnabled();
                }
                else
                {
                    OnInteractableDisabled();
                }
            }
        }
        public override void OnSelect(BaseEventData eventData)
        {
            if(Selected != null) 
            {
                this.Selected.gameObject.SetActive(true);
            }
            else
            {
                base.OnSelect(eventData);
            }
            this.OnSelectedEnter?.Invoke();
        }
        public override void OnDeselect(BaseEventData eventData)
        {
            if (Selected != null)
            {
                this.Selected.gameObject.SetActive(false);
            }
            else
            {
                base.OnDeselect(eventData);
            }
            this.OnSelectedExit?.Invoke();
        }
        private void OnInteractableEnabled()
        {
            var Texts = this.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in Texts)
            {
                text.color = Color.white;
            }
        }

        private void OnInteractableDisabled()
        {
            var Texts = this.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in Texts)
            {
                text.color = Color.grey;
            }
        }

        public void Init()
        {
            image = this.GetComponentInChildren<UnityEngine.UI.Image>();
            Selected = this.transform.Find("Selected");
            if(Selected != null) { Selected.gameObject.SetActive(false); }
            this.targetGraphic = image;
        }

        public void Interact()
        {
            
            if (this.Interactable)
            {
                this.PreInteract?.Invoke();
                this.onClick.Invoke();
                this.PostInteract?.Invoke();
            }
        }

        public void SetUIBtnList(UIBtnList uIBtnList)
        {
            this.UIBtnList = uIBtnList;
        }

        public UIBtnList GetUIBtnList()
        {
            return this.UIBtnList;
        }

        public void SetPreAndPostInteract(System.Action preAction, System.Action postAction)
        {
            this.PreInteract = preAction; 
            this.PostInteract = postAction;
        }

        // 统一鼠标与按键
        public override void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            this.UIBtnList?.RefreshSelected(this);
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            this.UIBtnList?.ButtonInteract(new InputAction.CallbackContext());
        }

        public void SetOnSelectEnter(System.Action action)
        {
            if (action != null)
            {
                this.OnSelectedEnter += action;
            }
        }

        public void SetOnSelectExit(System.Action action)
        {
            if (action != null)
            {
                this.OnSelectedExit += action;
            }
        }

    }

}



using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ML.Engine.UI
{
    public class SelectedButton : Button
    {
        public event System.Action OnSelectedEnter;
        public event System.Action OnSelectedExit;

        private Transform Selected = null;

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
            Debug.Log("OnInteractableEnabled");
            var Texts = this.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in Texts)
            {
                text.color = Color.white;
            }
        }

        private void OnInteractableDisabled()
        {
            Debug.Log("OnInteractableDisabled");
            var Texts = this.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in Texts)
            {
                text.color = Color.grey;
            }
        }

        public void Init(System.Action OnSelectedEnter, System.Action OnSelectedExit)
        {
            image = this.GetComponentInChildren<UnityEngine.UI.Image>();
            Selected = this.transform.Find("Selected");
            this.targetGraphic = image;
            this.OnSelectedEnter = OnSelectedEnter;
            this.OnSelectedExit = OnSelectedExit;   
        }

        public void Interact()
        {
            Debug.Log(this.name + " " + this.Interactable);
            if(this.Interactable)
            {
                this.onClick.Invoke();
            }
        }
    }

}

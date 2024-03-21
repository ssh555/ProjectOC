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

        // ���尴ť�Ƿ�ɽ���������
        public bool Interactable
        {
            get { return interactable; }
            set
            {
                // �����ֵ���ֵ��ͬ����ִ���κβ���
                if (interactable == value)
                    return;

                // ���°�ť�Ƿ�ɽ�����״̬
                interactable = value;

                // ���ݰ�ť�Ƿ�ɽ���ִ�в�ͬ���߼�
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

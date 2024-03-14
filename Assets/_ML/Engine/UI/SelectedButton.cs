using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
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

        public void Init(System.Action OnSelectedEnter, System.Action OnSelectedExit)
        {
            image = this.GetComponentInChildren<UnityEngine.UI.Image>();
            Selected = this.transform.Find("Selected");
            this.targetGraphic = image;
            this.OnSelectedEnter = OnSelectedEnter;
            this.OnSelectedExit = OnSelectedExit;   
        }
    }

}

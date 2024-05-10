using System;
using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace ML.Engine.UI
{
    [RequireComponent(typeof(Slider))]
    public class CustomSelectedSlider :MonoBehaviour
    {
        
        #region CustomPart

        private TextMeshProUGUI headText, numText;
        public Slider slider;
        public float value => slider.value;
        private Action<float> OnSliderValueChange;
        public void InitData(string _str,int _value)
        {
            if(headText!=null)
                headText.text = _str;
            // slider.value = _value;
        }
        
        protected void Awake()
        {
            slider = GetComponent<Slider>();
            headText = transform.parent.Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            numText = transform.parent.Find("Num").GetComponentInChildren<TextMeshProUGUI>();
            slider.onValueChanged.AddListener(HandleSliderValueChange);

            this.enabled = false;
        }

        private void HandleSliderValueChange(float _value)
        {
            numText.text = _value.ToString();
        }

        public void SetSliderConfig(string _str,UnityAction<float> sliderAction)
        {
            headText.text = _str;
            slider.onValueChanged.AddListener(sliderAction);
        }

        public void SetValueWithoutNotify(float _value)
        {
            slider.SetValueWithoutNotify(_value);
            HandleSliderValueChange(value);
        }
        #endregion

        

    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityLight;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ML.Engine.UI
{
    public class HSVColorSettingController : MonoBehaviour
    {
        public Action<Color> ColorChangeAction;

        //0-2 RGB
        //3-5 hsv
        private CustomSelectedSlider[] sliders;

        private void Awake()
        {
            sliders = GetComponentsInChildren<CustomSelectedSlider>();
            BindColorAction();
            this.enabled = false;
        }

        private void OnDestroy()
        {
            //UnBind，但好像也不用写
        }

        private void BindColorAction()
        {
            for (int i = 0; i < 3; i++)
            {
                sliders[i].slider.onValueChanged.AddListener((_value)=>
                {
                    Color _color = ReadColor(true);
                    SetColorValue(_color,false);
                    ColorChangeAction(_color);
                }); 
                
            }

            for (int i = 3; i < 6; i++)
            {
                sliders[i].slider.onValueChanged.AddListener((_value)=>
                {
                    Color _color = ReadColor(false);
                    SetColorValue(_color,true);
                    ColorChangeAction(_color);
                }); 
            }
            
        }
        
        public Color ReadColor(bool left = true)
        {
            Color res;
            if (left)
            {
                Vector3 colorVec = new Vector3(sliders[0].value, sliders[1].value, sliders[2].value) / 255f;
                res = new Color(colorVec.x, colorVec.y, colorVec.z, 1.0f);
            }
            else
            {
                Vector3 colorVec = new Vector3(sliders[3].value, sliders[4].value, sliders[5].value) / 255f;
                res = Color.HSVToRGB(colorVec.x, colorVec.y, colorVec.z);
            }
            return res;
        }

        public void SetColorValue(Color _colorVaue, bool left = true)
        {
            if (left)
            {
                Vector3 _values = new Vector3(_colorVaue.r, _colorVaue.g, _colorVaue.b) * 255f + Vector3.one*0.5f;
                sliders[0].SetValueWithoutNotify(_values.x);
                sliders[1].SetValueWithoutNotify(_values.y);
                sliders[2].SetValueWithoutNotify(_values.z);
            }
            else
            {
                float _h, _s, _v;
                Color.RGBToHSV(_colorVaue, out _h, out _s, out _v);
                Vector3 _values = new Vector3(_h,_s,_v) * 255f+ Vector3.one *0.5f;
                sliders[3].SetValueWithoutNotify(_values.x);
                sliders[4].SetValueWithoutNotify(_values.y);
                sliders[5].SetValueWithoutNotify(_values.z);
            }
        }
        
        

    }
}
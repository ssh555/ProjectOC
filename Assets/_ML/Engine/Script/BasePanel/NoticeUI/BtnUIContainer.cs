using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ML.Engine.UI
{
    public class BtnUIContainer 
    {
        private UIBtnList UIBtnList;
        private Transform parent;

        public BtnUIContainer(Transform parent) 
        {
            this.parent = parent;
            this.UIBtnList = new UIBtnList(parent);
            this.UIBtnList.EnableBtnList();
        }

        public void AddBtn(SelectedButton selectedButton)
        {
            UIBtnList.AddBtn(selectedButton);
        }

        public void SetBtnAction(string btnName,UnityAction action)
        {
            this.UIBtnList.SetBtnAction(btnName, action);
        }
    }
}



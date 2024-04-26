using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

namespace ML.Engine.UI
{
    public class UIBtnListInitor : UIBehaviour,ISelected
    {
        #region ISelected
        public ISelected LeftUI { get; set; }
        public ISelected RightUI { get; set; }
        public ISelected UpUI { get; set; }
        public ISelected DownUI { get; set; }
        #endregion

        [Serializable]
        public struct BtnListInitData
        {
            [LabelText("����")]
            public int limitNum;
            [LabelText("�Ƿ��г�ʼѡ��")]
            public bool hasInitSelect;
            [LabelText("�Ƿ���ѭ��")]
            public bool isLoop;
            [LabelText("�Ƿ�Ϊ��ת�б�")]
            public bool isWheel;
            [LabelText("�Ƿ���BtnListContainer��")]
            public bool hasBtnListContainer;
            [LabelText("�Ƿ��ȡUnActive Button")] 
            public bool readUnActiveButton;
            
            public static BtnListInitData defaultTemplate = new BtnListInitData()
            {
                limitNum = 1,
                hasInitSelect = false,
                isLoop = false,
                isWheel = false,
                hasBtnListContainer = false,
                readUnActiveButton = true
            };
        }

        [LabelText("���������")]
        public BtnListInitData btnListInitData = BtnListInitData.defaultTemplate;
    }
}



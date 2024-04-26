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
            [LabelText("列数")]
            public int limitNum;
            [LabelText("是否有初始选中")]
            public bool hasInitSelect;
            [LabelText("是否有循环")]
            public bool isLoop;
            [LabelText("是否为轮转列表")]
            public bool isWheel;
            [LabelText("是否在BtnListContainer里")]
            public bool hasBtnListContainer;
            [LabelText("是否读取UnActive Button")] 
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

        [LabelText("面板配置项")]
        public BtnListInitData btnListInitData = BtnListInitData.defaultTemplate;
    }
}



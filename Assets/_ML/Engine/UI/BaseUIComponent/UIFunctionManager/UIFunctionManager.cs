using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ML.Engine.UI
{
    public class UIFunctionManager : UIBehaviour
    {

        [LabelText("元素之间选中是否互斥")]
        public bool IsMutex = false;

        private Dictionary<string, Transform> transformDic = new Dictionary<string, Transform>();

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < this.transform.childCount; i++)
            {
                transformDic.Add(this.transform.GetChild(i).name, this.transform.GetChild(i));
            }
        }

        public void SelectTransform(string name)
        {

        }

    }
}



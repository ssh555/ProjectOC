using Cysharp.Threading.Tasks;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ML.Engine.UI.OptionPanel;
using static ML.Engine.UI.UIBtnListContainer;

namespace ML.Engine.UI
{
    public class TestPanel : ML.Engine.UI.UIBasePanel
    {
        private Transform BtnList1, BtnList2, BtnList3;
        private List<Transform> transforms = new List<Transform>();
        private UIBtnListContainer UIBtnListContainer;
        protected override void Awake()
        {
            base.Awake();
            BtnList1 = transform.Find("ButtonList1");
            BtnList2 = transform.Find("ButtonList2");
            BtnList3 = transform.Find("ButtonList3");
            transforms.Add(BtnList1);
            transforms.Add(BtnList2);
            transforms.Add(BtnList3);
            UIBtnListContainer = new UIBtnListContainer(new List<UIBtnListContainer.BtnListinitData2> {
                new UIBtnListContainer.BtnListinitData2(BtnList1, 6, false, false) ,
            new UIBtnListContainer.BtnListinitData2(BtnList2, 5, false, false),
            new UIBtnListContainer.BtnListinitData2(BtnList3, 3, false, false)

            },UIBtnListContainer.GridNavagationType.B


            );

            UIBtnListContainer.LinkTwoEdge(UIBtnListContainer.GetEdge(UIBtnListContainer.UIBtnLists[0], UIBtnListContainer.EdgeType.RP), UIBtnListContainer.GetEdge(UIBtnListContainer.UIBtnLists[1], UIBtnListContainer.EdgeType.LN), UIBtnListContainer.LinkType.LTR);
            UIBtnListContainer.LinkTwoEdge(UIBtnListContainer.GetEdge(UIBtnListContainer.UIBtnLists[0], UIBtnListContainer.EdgeType.DN), UIBtnListContainer.GetEdge(UIBtnListContainer.UIBtnLists[2], UIBtnListContainer.EdgeType.UN), UIBtnListContainer.LinkType.UTD);

            UIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, BindType.started);
        }


        protected override void Exit()
        {
            this.UIBtnListContainer.DisableUIBtnListContainer();
            base.Exit();
        }

        #region Internal
        protected override void UnregisterInput()
        {
            // ·µ»Ø
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        protected override void RegisterInput()
        {
            // ·µ»Ø
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }

        #endregion



       



    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ML.Engine.UI
{
    public class UIBtnList
    {
        private SelectedButton[] OneDimSelectedButtons;//一维列表
        private SelectedButton[,] TwoDimSelectedButtons;//二维列表
        private int limitNum = 1;
        private int OneDimCnt = 0;
        private int TwoDimH = 0;
        private int TwoDimW = 0;

        private SelectedButton CurSelected;

        //Index
        private int OneDimI = 0;
        private int TwoDimI = 0;
        private int TwoDimJ = 0;

        private Dictionary<string, SelectedButton> SBDic = new Dictionary<string, SelectedButton>();
        /// <summary>
        /// parent: 按钮父物体 limitNum：一行多少个按钮 
        /// </summary>
        public UIBtnList(Transform parent, int limitNum = 1, bool hasInitSelect = true, Action OnSelectedEnter = null, Action OnSelectedExit = null)
        {
            if (parent != null)
            {
                this.OneDimSelectedButtons = parent.GetComponentsInChildren<SelectedButton>(true);
                this.limitNum = limitNum;
                this.OneDimCnt = this.OneDimSelectedButtons.Length;

                foreach(var btn in OneDimSelectedButtons)
                {
                    btn.Init(OnSelectedEnter, OnSelectedExit);
                    Navigation navigation = btn.navigation;
                    navigation.mode = Navigation.Mode.Explicit;
                    SBDic.Add(btn.gameObject.name, btn);
                }

                if(limitNum > 1)//二维
                {
                    this.TwoDimW = limitNum;
                    this.TwoDimH = OneDimCnt % TwoDimW == 0 ? OneDimCnt / TwoDimW : OneDimCnt / TwoDimW + 1;
                    TwoDimSelectedButtons = new SelectedButton[TwoDimH, TwoDimW];
                    int cnt = 0;
                    for(int i = 0;i< TwoDimH; i++)
                    {
                        for(int j = 0;j< TwoDimW; j++)
                        {
                            if(cnt > OneDimCnt)
                            {
                                TwoDimSelectedButtons[i, j] = null;
                            }
                            else
                            {
                                TwoDimSelectedButtons[i, j] = OneDimSelectedButtons[cnt++];
                            }

                        }
                    }
                    //设置二维按钮相对位置
                    for (int i = 0; i < OneDimSelectedButtons.Length; ++i)
                    {
                        int last = (i - 1 + OneDimSelectedButtons.Length) % OneDimSelectedButtons.Length;
                        int next = (i + 1 + OneDimSelectedButtons.Length) % OneDimSelectedButtons.Length;
                        Navigation navigation = OneDimSelectedButtons[i].navigation;
                        navigation.selectOnUp = i - limitNum >= 0 ? OneDimSelectedButtons[i - limitNum] : OneDimSelectedButtons[i - limitNum + OneDimSelectedButtons.Length];
                        navigation.selectOnDown = i + limitNum < OneDimSelectedButtons.Length ? OneDimSelectedButtons[i + limitNum] : OneDimSelectedButtons[i + limitNum - OneDimSelectedButtons.Length];
                        navigation.selectOnRight = i % limitNum + 1 < limitNum ? OneDimSelectedButtons[i + 1] : OneDimSelectedButtons[i / limitNum * limitNum];
                        navigation.selectOnLeft = i % limitNum - 1 >= 0 ? OneDimSelectedButtons[i - 1] : OneDimSelectedButtons[i / limitNum * limitNum + limitNum - 1];
                    }

                    //初始化选择对象
                    this.TwoDimI = 0;
                    this.TwoDimJ = 0;
                    this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
                }
                else//一维
                {
                    //设置一维按钮相对位置
                    for (int i = 0; i < OneDimSelectedButtons.Length; ++i)
                    {
                        int last = (i - 1 + OneDimSelectedButtons.Length) % OneDimSelectedButtons.Length;
                        int next = (i + 1 + OneDimSelectedButtons.Length) % OneDimSelectedButtons.Length;
                        Navigation navigation = OneDimSelectedButtons[i].navigation;
                        navigation.selectOnUp = OneDimSelectedButtons[last];
                        navigation.selectOnDown = OneDimSelectedButtons[next];
                    }

                    //初始化选择对象
                    this.OneDimI = 0;
                    this.CurSelected = OneDimSelectedButtons[OneDimI];
                }
                if(hasInitSelect)
                    this.CurSelected.OnSelect(null);
            }   
        }

        /// <summary>
        /// 传入下标设置一维按钮列表action
        /// </summary>
        public void SetBtnAction(int i,UnityAction action)
        {
            this.OneDimSelectedButtons[i].onClick.AddListener(action);
        }
        /// <summary>
        /// 传入下标设置二维按钮列表action
        /// </summary>
        public void SetBtnAction(int i,int j, UnityAction action)
        {
            this.TwoDimSelectedButtons[i,j].onClick.AddListener(action);
        }
        /// <summary>
        /// 传入字符串设置按钮列表action
        /// </summary>
        public void SetBtnAction(string BtnName, UnityAction action)
        {
            if (this.SBDic.ContainsKey(BtnName))
            {
                SBDic[BtnName].onClick.AddListener(action);
            }
        }
        /// <summary>
        /// 获取指定按钮
        /// </summary>
        public SelectedButton GetBtn(string BtnName)
        {
            if (this.SBDic.ContainsKey(BtnName))
            {
                return SBDic[BtnName];
            }
            return null;
        }
        /// <summary>
        /// 获取当前选中按钮
        /// </summary>
        public SelectedButton GetCurSelected()
        {
            return this.CurSelected;
        }
        /// <summary>
        /// 置空当前选中按钮
        /// </summary>
        public void SetCurSelectedNull()
        {
            this.CurSelected.OnDeselect(null);
            this.CurSelected = null;
        }
        /// <summary>
        /// 向上
        /// </summary>
        public SelectedButton MoveUPIUISelected()
        {
            if(limitNum > 1)
            {
                int i = TwoDimI - 1 >= 0 ? TwoDimI - 1 : TwoDimI - 1 + TwoDimH;

                if (TwoDimSelectedButtons[i, TwoDimJ] == null) return null;
                TwoDimI = i;
                this.CurSelected?.OnDeselect(null);
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
            }
            else
            {
                int i = OneDimI - 1 >= 0 ? OneDimI - 1 : OneDimI - 1 + OneDimCnt;

                if (OneDimSelectedButtons[i] == null) return null;
                OneDimI = i;
                this.CurSelected?.OnDeselect(null);
                this.CurSelected = OneDimSelectedButtons[OneDimI];
            }
            this.CurSelected.OnSelect(null);
            return CurSelected;
        }
        /// <summary>
        /// 向下
        /// </summary>
        public SelectedButton MoveDownIUISelected()
        {
            if (limitNum > 1)
            {
                int i = TwoDimI + 1 < TwoDimH ? TwoDimI + 1 : TwoDimI + 1 - TwoDimH;

                if (TwoDimSelectedButtons[i, TwoDimJ] == null) return null;
                TwoDimI = i;
                this.CurSelected?.OnDeselect(null);
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
            }
            else
            {
                int i = OneDimI + 1 < OneDimCnt ? OneDimI + 1 : OneDimI + 1 - OneDimCnt;

                if (OneDimSelectedButtons[i] == null) return null;
                OneDimI = i;
                this.CurSelected?.OnDeselect(null);
                this.CurSelected = OneDimSelectedButtons[OneDimI];
            }
            this.CurSelected.OnSelect(null);
            return CurSelected;
        }
        /// <summary>
        /// 向左
        /// </summary>
        public SelectedButton MoveLeftIUISelected()
        {
            if (limitNum > 1)
            {
                int j = TwoDimJ - 1 >= 0 ? TwoDimJ - 1 : TwoDimJ - 1 + TwoDimW;

                if (TwoDimSelectedButtons[TwoDimI, j] == null) return null;
                TwoDimJ = j;
                this.CurSelected?.OnDeselect(null);
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
            }
            else
            {
                int i = OneDimI - 1 >= 0 ? OneDimI - 1 : OneDimI - 1 + OneDimCnt;

                if (OneDimSelectedButtons[i] == null) return null;
                OneDimI = i;
                this.CurSelected?.OnDeselect(null);
                this.CurSelected = OneDimSelectedButtons[OneDimI];
            }
            this.CurSelected.OnSelect(null);
            return CurSelected;
        }
        /// <summary>
        /// 向右
        /// </summary>
        public SelectedButton MoveRightIUISelected()
        {
            
            if (limitNum > 1)
            {
                int j = TwoDimJ + 1 < TwoDimW ? TwoDimJ + 1 : TwoDimJ + 1 - TwoDimW;
                if (TwoDimSelectedButtons[TwoDimI, j] == null) return null;
                TwoDimJ = j;
                this.CurSelected?.OnDeselect(null);
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
            }
            else
            {
                int i = OneDimI + 1 < OneDimCnt ? OneDimI + 1 : OneDimI + 1 - OneDimCnt;

                if (OneDimSelectedButtons[i] == null) return null;
                OneDimI = i;
                this.CurSelected?.OnDeselect(null);
                this.CurSelected = OneDimSelectedButtons[OneDimI];
            }
            this.CurSelected.OnSelect(null);
            return CurSelected;
        }
        /// <summary>
        /// 向指定坐标移动
        /// </summary>
        public SelectedButton MoveIndexIUISelected(int i)
        {

            
            this.CurSelected?.OnDeselect(null);
            this.CurSelected = OneDimSelectedButtons[i];

            this.CurSelected.OnSelect(null);
            return CurSelected;
        }

        /// <summary>
        /// 设置按钮按键提示文本
        /// </summary>
        public void SetBtnText(string btnName,string showText)
        {
            SBDic[btnName].transform.Find("BtnText").GetComponent<TextMeshProUGUI>().text = showText;
        }
    }

}



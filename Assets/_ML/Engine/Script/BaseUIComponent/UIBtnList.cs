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
        private SelectedButton[] OneDimSelectedButtons;//һά�б�
        private SelectedButton[,] TwoDimSelectedButtons;//��ά�б�
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
        /// parent: ��ť������ limitNum��һ�ж��ٸ���ť 
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

                if(limitNum > 1)//��ά
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
                    //���ö�ά��ť���λ��
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

                    //��ʼ��ѡ�����
                    this.TwoDimI = 0;
                    this.TwoDimJ = 0;
                    this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
                }
                else//һά
                {
                    //����һά��ť���λ��
                    for (int i = 0; i < OneDimSelectedButtons.Length; ++i)
                    {
                        int last = (i - 1 + OneDimSelectedButtons.Length) % OneDimSelectedButtons.Length;
                        int next = (i + 1 + OneDimSelectedButtons.Length) % OneDimSelectedButtons.Length;
                        Navigation navigation = OneDimSelectedButtons[i].navigation;
                        navigation.selectOnUp = OneDimSelectedButtons[last];
                        navigation.selectOnDown = OneDimSelectedButtons[next];
                    }

                    //��ʼ��ѡ�����
                    this.OneDimI = 0;
                    this.CurSelected = OneDimSelectedButtons[OneDimI];
                }
                if(hasInitSelect)
                    this.CurSelected.OnSelect(null);
            }   
        }

        /// <summary>
        /// �����±�����һά��ť�б�action
        /// </summary>
        public void SetBtnAction(int i,UnityAction action)
        {
            this.OneDimSelectedButtons[i].onClick.AddListener(action);
        }
        /// <summary>
        /// �����±����ö�ά��ť�б�action
        /// </summary>
        public void SetBtnAction(int i,int j, UnityAction action)
        {
            this.TwoDimSelectedButtons[i,j].onClick.AddListener(action);
        }
        /// <summary>
        /// �����ַ������ð�ť�б�action
        /// </summary>
        public void SetBtnAction(string BtnName, UnityAction action)
        {
            if (this.SBDic.ContainsKey(BtnName))
            {
                SBDic[BtnName].onClick.AddListener(action);
            }
        }
        /// <summary>
        /// ��ȡָ����ť
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
        /// ��ȡ��ǰѡ�а�ť
        /// </summary>
        public SelectedButton GetCurSelected()
        {
            return this.CurSelected;
        }
        /// <summary>
        /// �ÿյ�ǰѡ�а�ť
        /// </summary>
        public void SetCurSelectedNull()
        {
            this.CurSelected.OnDeselect(null);
            this.CurSelected = null;
        }
        /// <summary>
        /// ����
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
        /// ����
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
        /// ����
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
        /// ����
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
        /// ��ָ�������ƶ�
        /// </summary>
        public SelectedButton MoveIndexIUISelected(int i)
        {

            
            this.CurSelected?.OnDeselect(null);
            this.CurSelected = OneDimSelectedButtons[i];

            this.CurSelected.OnSelect(null);
            return CurSelected;
        }

        /// <summary>
        /// ���ð�ť������ʾ�ı�
        /// </summary>
        public void SetBtnText(string btnName,string showText)
        {
            SBDic[btnName].transform.Find("BtnText").GetComponent<TextMeshProUGUI>().text = showText;
        }
    }

}



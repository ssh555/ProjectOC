using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        private IUISelected CurSelected;

        //Index
        private int OneDimI = 0;
        private int TwoDimI = 0;
        private int TwoDimJ = 0;

        private Dictionary<string, SelectedButton> SBDic = new Dictionary<string, SelectedButton>();
        /// <summary>
        /// parent: ��ť������ limitNum��һ�ж��ٸ���ť BtnType��0Ϊ�� 1Ϊѡ���˳������ʽ 2Ϊѡ���˳�selected���Ըı� 
        /// </summary>
        public UIBtnList(Transform parent,int limitNum = 1,int BtnType = 0)
        {
            if (parent != null)
            {
                this.OneDimSelectedButtons = parent.GetComponentsInChildren<SelectedButton>(true);
                this.limitNum = limitNum;
                this.OneDimCnt = this.OneDimSelectedButtons.Length;

                foreach(var btn in OneDimSelectedButtons)
                {
                    btn.Init();
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

                        OneDimSelectedButtons[i].UpUI = i - limitNum >= 0 ? OneDimSelectedButtons[i - limitNum] : OneDimSelectedButtons[i - limitNum + OneDimSelectedButtons.Length];
                        OneDimSelectedButtons[i].DownUI = i + limitNum < OneDimSelectedButtons.Length ? OneDimSelectedButtons[i + limitNum] : OneDimSelectedButtons[i + limitNum - OneDimSelectedButtons.Length];
                        OneDimSelectedButtons[i].RightUI = i % limitNum + 1 < limitNum ? OneDimSelectedButtons[i + 1] : OneDimSelectedButtons[i / limitNum * limitNum];
                        OneDimSelectedButtons[i].LeftUI = i % limitNum - 1 >= 0 ? OneDimSelectedButtons[i - 1] : OneDimSelectedButtons[i / limitNum * limitNum + limitNum - 1];
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

                        OneDimSelectedButtons[i].UpUI = OneDimSelectedButtons[last];
                        OneDimSelectedButtons[i].DownUI = OneDimSelectedButtons[next];
                    }

                    //��ʼ��ѡ�����
                    this.OneDimI = 0;
                    this.CurSelected = OneDimSelectedButtons[OneDimI];
                }

                foreach (var btn in OneDimSelectedButtons)
                {
                    btn.OnSelectedEnter += () =>
                    {
                        if(BtnType == 0)
                        {
                            
                        }
                        else if(BtnType == 1)
                        {
                            btn.image.color = Color.red;
                        }
                        else if(BtnType == 2)
                        {
                            btn.transform.Find("Selected").gameObject.SetActive(true);
                        }
                        
                    };
                    btn.OnSelectedExit += () =>
                    {
                        if (BtnType == 0)
                        {

                        }
                        else if (BtnType == 1)
                        {
                            btn.image.color = Color.white;
                        }
                        else if (BtnType == 2)
                        {
                            btn.transform.Find("Selected").gameObject.SetActive(false);
                        }
                    };
                }
                
                
                this.CurSelected.SelectedEnter();
            }   
        }

        /// <summary>
        /// �����±�����һά��ť�б�action
        /// </summary>
        public void SetBtnAction(int i,Action action)
        {
            this.OneDimSelectedButtons[i].OnInteract += action;
        }
        /// <summary>
        /// �����±����ö�ά��ť�б�action
        /// </summary>
        public void SetBtnAction(int i,int j, Action action)
        {
            this.TwoDimSelectedButtons[i,j].OnInteract += action;
        }
        /// <summary>
        /// �����ַ������ð�ť�б�action
        /// </summary>
        public void SetBtnAction(string BtnName, Action action)
        {
            if (this.SBDic.ContainsKey(BtnName))
            {
                SBDic[BtnName].OnInteract += action;
            }
        }
        /// <summary>
        /// ��ȡ��ǰѡ�а�ť
        /// </summary>
        public IUISelected GetCurSelected()
        {
            return this.CurSelected;
        }
        /// <summary>
        /// ����
        /// </summary>
        public IUISelected MoveUPIUISelected()
        {
            if(limitNum > 1)
            {
                int i = TwoDimI - 1 >= 0 ? TwoDimI - 1 : TwoDimI - 1 + TwoDimH;

                if (TwoDimSelectedButtons[i, TwoDimJ] == null) return null;
                TwoDimI = i;
                this.CurSelected.SelectedExit();
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
            }
            else
            {
                int i = OneDimI - 1 >= 0 ? OneDimI - 1 : OneDimI - 1 + OneDimCnt;

                if (OneDimSelectedButtons[i] == null) return null;
                OneDimI = i;
                this.CurSelected.SelectedExit();
                this.CurSelected = OneDimSelectedButtons[OneDimI];
            }
            this.CurSelected.SelectedEnter();
            return CurSelected;
        }
        /// <summary>
        /// ����
        /// </summary>
        public IUISelected MoveDownIUISelected()
        {
            if (limitNum > 1)
            {
                int i = TwoDimI + 1 < TwoDimH ? TwoDimI + 1 : TwoDimI + 1 - TwoDimH;

                if (TwoDimSelectedButtons[i, TwoDimJ] == null) return null;
                TwoDimI = i;
                this.CurSelected.SelectedExit();
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
            }
            else
            {
                int i = OneDimI + 1 < OneDimCnt ? OneDimI + 1 : OneDimI + 1 - OneDimCnt;

                if (OneDimSelectedButtons[i] == null) return null;
                OneDimI = i;
                this.CurSelected.SelectedExit();
                this.CurSelected = OneDimSelectedButtons[OneDimI];
            }
            this.CurSelected.SelectedEnter();
            return CurSelected;
        }
        /// <summary>
        /// ����
        /// </summary>
        public IUISelected MoveLeftIUISelected()
        {
            if (limitNum > 1)
            {
                int j = TwoDimJ - 1 >= 0 ? TwoDimJ - 1 : TwoDimJ - 1 + TwoDimW;

                if (TwoDimSelectedButtons[TwoDimI, j] == null) return null;
                TwoDimJ = j;
                this.CurSelected.SelectedExit();
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
            }
            else
            {
                int i = OneDimI - 1 >= 0 ? OneDimI - 1 : OneDimI - 1 + OneDimCnt;

                if (OneDimSelectedButtons[i] == null) return null;
                OneDimI = i;
                this.CurSelected.SelectedExit();
                this.CurSelected = OneDimSelectedButtons[OneDimI];
            }
            this.CurSelected.SelectedEnter();
            return CurSelected;
        }
        /// <summary>
        /// ����
        /// </summary>
        public IUISelected MoveRightIUISelected()
        {
            
            if (limitNum > 1)
            {
                int j = TwoDimJ + 1 < TwoDimW ? TwoDimJ + 1 : TwoDimJ + 1 - TwoDimW;
                if (TwoDimSelectedButtons[TwoDimI, j] == null) return null;
                TwoDimJ = j;
                this.CurSelected.SelectedExit();
                this.CurSelected = TwoDimSelectedButtons[TwoDimI, TwoDimJ];
            }
            else
            {
                int i = OneDimI + 1 < OneDimCnt ? OneDimI + 1 : OneDimI + 1 - OneDimCnt;

                if (OneDimSelectedButtons[i] == null) return null;
                OneDimI = i;
                this.CurSelected.SelectedExit();
                this.CurSelected = OneDimSelectedButtons[OneDimI];
            }
            this.CurSelected.SelectedEnter();
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



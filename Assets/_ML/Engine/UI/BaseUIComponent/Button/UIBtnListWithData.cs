using ML.Engine.UI;
using System;
using System.Collections.Generic;

public class UIBtnListWithData<T> : UIBtnList where T : class
{
    private List<T> BtnDatas;
    private string prefabpath;
    public UIBtnListWithData(UIBtnListInitor uIBtnListInitor,List<T> datas,string prefabpath) : base(uIBtnListInitor)
    {
        this.BtnDatas = datas;
        this.prefabpath = prefabpath;
    }

    public T GetData(int index)
    {
        if (GetBtn(index) == null) return default(T);
        return BtnDatas[index];
    }

    public T GetData(int index1, int index2)
    {
        if (GetBtn(index1, index2) == null) return default(T);
        return BtnDatas[index1 * TwoDimW + index2];
    }

    public void ChangBtnNum(int newNum, Action OnAllBtnChanged = null)
    {
        base.ChangBtnNum(newNum, prefabpath, OnAllBtnChanged);
        int cnt = BtnDatas.Count;
        if (OneDimCnt > cnt)
        {
            for(int i = 0; i < OneDimCnt - cnt; i++)
            {
                BtnDatas.Add(default(T));
            }
        }
        else
        {
            for (int i = 0; i < cnt - OneDimCnt; i++)
            {
                BtnDatas.RemoveAt(BtnDatas.Count - 1);
            }
        }
    }

    public void ChangBtnListData(List<T> datas, Action OnAllBtnChanged = null)
    {
        if (datas == null) return;

        base.ChangBtnNum(datas.Count, prefabpath, OnAllBtnChanged);
        this.BtnDatas = datas;
    }

}

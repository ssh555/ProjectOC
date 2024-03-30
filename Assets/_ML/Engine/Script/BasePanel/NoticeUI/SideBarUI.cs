using ML.Engine.Manager;
using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SideBarUI : UIBasePanel, INoticeUI
{
    private TMPro.TextMeshProUGUI Msg1, Msg2;
    private Animator animator;
    protected override void Awake()
    {
        base.Awake();
        this.Msg1 = this.transform.Find("TextGroup").Find("Text1").GetComponent<TextMeshProUGUI>();
        this.Msg2 = this.transform.Find("TextGroup").Find("Text2").GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();    
    }

    public void DestroySelf()
    {
        GameManager.DestroyObj(gameObject);
    }

    #region INoticeUI
    public void SaveAsInstance()
    {
        this.gameObject.SetActive(false);
        this.animator.enabled = false;

    }

    public void CopyInstance<D>(D data)
    {
        this.gameObject.SetActive(true);
        this.animator.enabled = true;

        if (data is UIManager.SideBarUIData sideBarUIData)
        {
            this.Msg1.text = sideBarUIData.msg1;
            this.Msg2.text = sideBarUIData.msg2;
        }
    }
    #endregion

}

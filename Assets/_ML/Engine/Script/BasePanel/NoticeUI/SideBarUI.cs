using ML.Engine.Manager;
using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SideBarUI : UIBasePanel
{
    public TextMeshProUGUI Text;
    private Animator animator;
    protected override void Awake()
    {
        base.Awake();
        Text = transform.Find("Image1").Find("Text").GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();    
    }

    public void DestroySelf()
    {
        GameManager.DestroyObj(gameObject);
    }

    public void SaveAsInstance()
    {
        this.gameObject.SetActive(false);
        this.animator.enabled = false;
        
    }

    public void CopyInstance()
    {
        this.gameObject.SetActive(true);
        this.animator.enabled = true;
    }
}

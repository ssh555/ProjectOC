using ML.Engine.Manager;
using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FloatTextUI : UIBasePanel
{
    public TextMeshProUGUI Text;
    protected override void Awake()
    {
        base.Awake();
        Text = transform.Find("Image").Find("Text").GetComponent<TextMeshProUGUI>();
    }

    public void DestroySelf()
    {
        GameManager.DestroyObj(gameObject);
    }
}

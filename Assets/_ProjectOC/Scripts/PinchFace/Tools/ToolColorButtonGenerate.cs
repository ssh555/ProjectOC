using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using ML.Engine.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ToolColorButtonGenerate : MonoBehaviour
{
    //To-Delete
    public List<Vector2Int> SV;
    [Button("生成颜色")]
    public void GenerateColorButton()
    {
        SelectedButton[] buttons = GetComponentsInChildren<SelectedButton>();
        //30个btn
        
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                int _btnIndex = i * 10 + j;
                Color _color = Color.HSVToRGB(0.1f*j,SV[i].x/255f,SV[i].y/255f);
                buttons[_btnIndex].GetComponent<Image>().color = _color;
            }
        }
    }

    public void Awake()
    {
        this.enabled = false;
    }
}

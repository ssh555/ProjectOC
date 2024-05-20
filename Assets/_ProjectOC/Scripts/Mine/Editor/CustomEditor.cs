using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

public class OCCustomEditor : Editor
{

    
    public virtual void OnEnable()
    {
        
        scalelabelStyle = new GUIStyle();
        scalelabelStyle.normal.textColor = Color.red; // �����ı���ɫΪ��ɫ
        scalelabelStyle.fontSize = 20; // ���������СΪ20
        //StyleCheck();
        if ((EditorExpand & SceneExpand.DrawScale) != 0)
        {
            SceneView.duringSceneGui += DrawScale;
        }
    }

    public virtual void OnDisable()
    {
        if ((EditorExpand & SceneExpand.DrawScale) != 0)
        {
            SceneView.duringSceneGui -= DrawScale;
        }
    }


    #region Make GUIStyle
    protected GUIStyle scalelabelStyle,blackLabelStyle;
    protected GUIStyle defaultTextStyle,defaultSliderStyle;
    
    //����DrawInspect���棬������DrawInspect�п�������OnEnable ����
    protected void StyleCheck()
    {
        if (blackLabelStyle == null)
        {
            blackLabelStyle = new GUIStyle(GUI.skin.label);
            blackLabelStyle.normal.textColor = Color.white; // ����������ɫΪ��ɫ
            blackLabelStyle.fontSize = 20; // ���������С
            blackLabelStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 1f)); // ���ñ�����ɫΪ����ɫ
        }
        
        if (defaultTextStyle == null)
        {
            defaultTextStyle = new GUIStyle(GUI.skin.label);
            defaultTextStyle.fontSize = 15;
        }
        
        if (defaultSliderStyle == null)
        {
            defaultSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            defaultSliderStyle.fontSize = 20;
        }
    }
    private Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = color;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    
    int selectBordSize = 3;
    public void ToggleButton(string labelText, Action buttonAction, bool SelectCondition, int _buttonSize = -1,GUIStyle style = null)
    {
        if (_buttonSize == -1)
        {
            _buttonSize = 100;
        }

        if (style == null)
        {
            if (GUILayout.Button(labelText, GUILayout.Height(_buttonSize), GUILayout.Width(_buttonSize)))
            {
                buttonAction();
            }
        }
        else
        {
            if (GUILayout.Button(labelText, style,GUILayout.Height(_buttonSize), GUILayout.Width(_buttonSize)))
            {
                buttonAction();
            }
        }
            

        if (SelectCondition)
        {
            DrawBorder(selectBordSize, new Color(1, 0, 0, 0.25f));
        }

        GUILayout.Space(20);
    }
    private void DrawBorder(float size, Color color)
    {
        Rect rect = GUILayoutUtility.GetLastRect();
        rect.width += size * 2;
        rect.height += size * 2;
        rect.x -= size;
        rect.y -= size;

        EditorGUI.DrawRect(rect, color);
    }
    #endregion

    #region Scale

    [System.Flags]
    public enum SceneExpand
    {
        [LabelText("All")]
        All = int.MaxValue,
        [LabelText("None")]
        None = 0,
        [LabelText("���ƿ̶�")]
        DrawScale = 1 << 0,
    }
    [SerializeField]
    SceneExpand EditorExpand = SceneExpand.DrawScale;
    
    protected int scaleWidth = 100, scaleHeight = 100;
    void DrawScale(SceneView sceneView)
    {
        Handles.Label( new Vector3(0.5f,0.5f,0f), $"0,0",scalelabelStyle);
        float scaleLineLength = 0.5f;
        float scaleLineThickness = 0.01f; // ���������Ĵ�ϸ
        Color rectangleColor = Color.red;
        Vector3 originPos = Vector3.zero;
        float stepSize = 1.0f;
        
        
        // Draw ˮƽ scale
        for (int i = 0; i < scaleWidth; i++)
        {
            Vector3 start = originPos + new Vector3(i * stepSize, 0, 0);
            Vector3 end = start + new Vector3(0, scaleLineLength, 0);

            Vector3 thinkBuffer = Vector3.right * scaleLineThickness;
            Handles.DrawSolidRectangleWithOutline(
                new Vector3[] { start + thinkBuffer, start - thinkBuffer, end - thinkBuffer, end + thinkBuffer}, 
                rectangleColor, rectangleColor);
            // Label the row index
            Handles.Label(start + new Vector3(scaleLineLength, 0, 0), i.ToString());
        }

        // Draw ��ֱ scale
        for (int j = 0; j < scaleHeight; j++)
        {
            Vector3 start = originPos + new Vector3(0, j * stepSize, 0);
            Vector3 end = start + new Vector3(scaleLineLength, 0, 0);
            //Handles.DrawLine(start, end);
            Vector3 thinkBuffer = Vector3.up * scaleLineThickness;
            Handles.DrawSolidRectangleWithOutline(
                new Vector3[] { start + thinkBuffer, start - thinkBuffer, end - thinkBuffer, end + thinkBuffer}, 
                rectangleColor, rectangleColor);
            Handles.Label(start + new Vector3(0, scaleLineLength, 0), j.ToString());
        }
    }
    #endregion
}

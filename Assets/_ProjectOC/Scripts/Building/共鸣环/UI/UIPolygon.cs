using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using ML.Engine.Timer;
using Unity.VisualScripting;

/// <summary>
/// UI多边形
/// 继承 MaskableGraphic -> 遮罩图形
/// </summary>
public class UIPolygon : MaskableGraphic
{
    /// <summary>
    /// 不为null -> 在自己的Tex2D上Mask
    /// </summary>
    [SerializeField]
    protected Texture m_Texture;

    /// <summary>
    /// 填充
    /// true -> 透明外侧，显示内侧
    /// false -> 透明内侧
    /// </summary>
    public bool fill = true;

    /// <summary>
    /// 边数
    /// </summary>
    [Range(3, 360)]
    public int sides = 3;
    /// <summary>
    /// 旋转角度
    /// </summary>
    [Range(0, 360)]
    public float rotation = 0;
    /// <summary>
    /// 顶点数组 -> rect.pivot * size * VerticesDistances[index]
    /// </summary>
    [Range(0, 1)]
    public float[] VerticesDistances = new float[3];
    /// <summary>
    /// 尺寸 -> max(rect.height, rect.width)
    /// 可能需要修改一下 -> 因为自身的rect可能会发生变化，发生变化时需要更新值
    /// </summary>
    private float size = 0;

    /// <summary>
    /// 用于Mask的Texture
    /// </summary>
    public override Texture mainTexture
    {
        get
        {
            return m_Texture == null ? s_WhiteTexture : m_Texture;
        }
    }

    /// <summary>
    /// 更新自己的Texture
    /// </summary>
    public Texture texture
    {
        get
        {
            return m_Texture;
        }
        set
        {
            if (m_Texture == value) return;
            m_Texture = value;
            // 脏标记 -> 用于进行显示更新
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }

    #region 提供外部的接口
    public void DrawPolygon(int _sides)
    {
        sides = _sides;
        VerticesDistances = new float[_sides + 1];
        for (int i = 0; i < _sides; i++) VerticesDistances[i] = 1;
        // 触发重绘?
    }

    public void DrawPolygon(List<float> datas)
    {
        List<float> finalDatas = new List<float>(datas);
        sides = finalDatas.Count;
        // 加上最后一个点，最后一个点与第一个点重合
        finalDatas.Add(finalDatas[0]);
        VerticesDistances = finalDatas.ToArray();
        // 触发重绘
        SetVerticesDirty();
    }
    #endregion


    public List<float> datas = new List<float>();
    protected override void Start()
    {
        // 不是UI
        if (rectTransform == null) return;
        // 根据宽高适配尺寸
        size = rectTransform.rect.width;
        if (rectTransform.rect.width > rectTransform.rect.height)
            size = rectTransform.rect.height;
        else
            size = rectTransform.rect.width;
    }

    /// <summary>
    /// 设置遮罩的4个UI顶点
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="uvs"></param>
    /// <returns></returns>
    protected UIVertex[] SetVertexs(Vector2[] vertices, Vector2[] uvs)
    {
        UIVertex[] vbo = new UIVertex[4];
        for (int i = 0; i < vertices.Length; i++)
        {
            // 规范化vert -> default
            var vert = UIVertex.simpleVert;
            // 设置顶点颜色
            vert.color = color;
            // 设置顶点位置
            vert.position = vertices[i];
            // 设置uv
            vert.uv0 = uvs[i];
            vbo[i] = vert;
        }
        return vbo;
    }

    /// <summary>
    /// 重写OnPopulateMesh方法
    /// </summary>
    /// <param name="vh"></param>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        // 前一个点X
        Vector2 prevX = Vector2.zero;
        // 前一个点Y
        Vector2 prevY = Vector2.zero;
        Vector2 uv0 = new Vector2(0, 0);
        Vector2 uv1 = new Vector2(0, 1);
        Vector2 uv2 = new Vector2(1, 1);
        Vector2 uv3 = new Vector2(1, 0);
        Vector2 pos0;
        Vector2 pos1;
        Vector2 pos2;
        Vector2 pos3;
        // 绘制正多边形的顶点旋转角
        float degrees = 360f / sides;
        // 多边形顶点数
        int vertices = sides + 1;
        // 不一致则直接绘制正多边形
        if (VerticesDistances.Length != vertices)
        {
            VerticesDistances = new float[vertices];
            for (int i = 0; i < vertices - 1; i++) VerticesDistances[i] = 1;
        }
        // 最后一个顶点，也即是第一个顶点
        VerticesDistances[vertices - 1] = VerticesDistances[0];
        // 遍历顶点数组加入待绘制的UI顶点 -> 顶点数据变换操作
        for (int i = 0; i < vertices; i++)
        {
            float outer = -rectTransform.pivot.x * size * VerticesDistances[i];
            // 为什么要多算一遍
            //float inner = -rectTransform.pivot.x * size * VerticesDistances[i];
            float inner = outer;
            // 绕正轴的旋转
            float rad = Mathf.Deg2Rad * (i * degrees + rotation);
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);
            // 展UV
            uv0 = new Vector2(0, 1);
            uv1 = new Vector2(1, 1);
            uv2 = new Vector2(1, 0);
            uv3 = new Vector2(0, 0);

            // 计算绘制的四边形的四个顶点
            pos0 = prevX;
            pos1 = new Vector2(outer * c, outer * s);

            #region 绘制圆
            //// 绘制圆
            //float radius = pos1.magnitude;
            //int EdgeNum = 100;
            //for (int m = 0; m < EdgeNum-1; m++) 
            //{
            //    int startIndex = vh.currentVertCount;
            //    float rad1 = Mathf.Deg2Rad * m * (360f/ EdgeNum);
            //    float x1 = Mathf.Cos(rad1) * radius;
            //    float y1 = Mathf.Sin(rad1) * radius;

            //    float rad2 = Mathf.Deg2Rad * (m+1) * (360f / EdgeNum);
            //    float x2 = Mathf.Cos(rad2) * radius;
            //    float y2 = Mathf.Sin(rad2) * radius;



            //    var vert = UIVertex.simpleVert;

            //    vert.color = Color.white;
            //    vert.position = new Vector3(x1,y1,0);
            //    //vert.uv0 = new Vector4(0,0,0,0);
            //    vh.AddVert(vert);

            //    vert.position = new Vector3(x2, y2, 0);
            //    //vert.uv0 = new Vector4(0, 1, 0, 0);
            //    vh.AddVert(vert);

            //    vert.position = new Vector3(x1, y1, 0);
            //    //vert.uv0 = new Vector4(0, 1, 0, 0);
            //    vh.AddVert(vert);

            //    vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            //}
            #endregion

            // 填充内侧，透明外侧
            if (fill)
            {
                pos2 = Vector2.zero;
                pos3 = Vector2.zero;
            }
            // 透明内侧，填充外侧
            else
            {
                pos2 = new Vector2(inner * c, inner * s);
                pos3 = prevY;
            }

            prevX = pos1;
            prevY = pos2;
            // 绘制四边形
            vh.AddUIVertexQuad(SetVertexs(new[] { pos0, pos1, pos2, pos3 }, new[] { uv0, uv1, uv2, uv3 }));
        }
    }

}

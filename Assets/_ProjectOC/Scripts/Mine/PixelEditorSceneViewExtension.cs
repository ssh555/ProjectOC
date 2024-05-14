using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class PixelEditorSceneViewExtension
{
    private static Texture2D canvasTexture;
    private static Color[] pixels;
    private static Vector2Int canvasSize = new Vector2Int(200, 200);
    private static bool[] selectedPixels;

    private static Vector2 canvasOffset = Vector2.zero;
    private static float zoomScale = 1f;
    private static float minZoomScale = 0.1f;
    private static float maxZoomScale = 3f;

    static PixelEditorSceneViewExtension()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.name == "MineConfigScene")
        {
            SceneView.duringSceneGui += OnSceneGUI;
            InitializeCanvas();
        }
        else
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }

    private static void InitializeCanvas()
    {
        canvasTexture = new Texture2D(canvasSize.x, canvasSize.y);
        pixels = new Color[canvasSize.x * canvasSize.y];
        selectedPixels = new bool[canvasSize.x * canvasSize.y];

        ResetCanvas();
    }

    private static void ResetCanvas()
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
            selectedPixels[i] = false;
        }
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();

        // 绘制缩放和拖动功能
        HandleZoomAndPan();

        // 绘制画布
        GUI.DrawTexture(new Rect(10 + canvasOffset.x, 10 + canvasOffset.y, canvasSize.x * zoomScale, canvasSize.y * zoomScale), canvasTexture);

        // 绘制像素绘制和删除功能
        DrawPixelEditing();

        Handles.EndGUI();
    }

    private static void HandleZoomAndPan()
    {
        Event e = Event.current;

        // 缩放
        if (e.type == EventType.ScrollWheel)
        {
            float delta = e.delta.y;
            float zoomDelta = -delta * 0.01f;
            float prevZoomScale = zoomScale;
            zoomScale = Mathf.Clamp(zoomScale + zoomDelta, minZoomScale, maxZoomScale);

            // 根据鼠标位置缩放画布
            Vector2 mousePosition = Event.current.mousePosition;
            Vector2 offset = mousePosition - new Vector2(10 + canvasOffset.x, 10 + canvasOffset.y);
            Vector2 scaledOffset = offset / prevZoomScale;
            Vector2 newOffset = mousePosition - scaledOffset * zoomScale;
            canvasOffset = newOffset;
        }

        // 拖动
        if (e.button == 2)
        {
            if (e.type == EventType.MouseDrag)
            {
                canvasOffset += e.delta;
            }
        }

        // 防止事件被 Scene 视图处理
        if (e.type == EventType.ScrollWheel || e.type == EventType.MouseDrag)
        {
            e.Use();
        }
    }

    private static void DrawPixelEditing()
    {
        Event e = Event.current;

        // 在画布上绘制像素
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            // 省略绘制像素的逻辑
            
        }

        // 删除选中像素
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
        {
            // 省略删除选中像素的逻辑
            
        }

        // 导出像素数据按钮
        if (GUI.Button(new Rect(220, 10, 100, 30), "Export Pixels"))
        {
            // 省略导出像素数据的逻辑
            
        }
    }
}

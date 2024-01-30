using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ML.Engine.Extension
{
    public static class GridLayoutGroupExtension
    {
        public static Vector2Int GetGridSize(this GridLayoutGroup gridLayout)
        {
            RectTransform rectTransform = gridLayout.GetComponent<RectTransform>();
            Vector2 size = rectTransform.rect.size;
            Vector2 cellSize = gridLayout.cellSize;
            Vector2 spacing = gridLayout.spacing;
            Vector2 padding = new Vector2(gridLayout.padding.horizontal, gridLayout.padding.vertical);

            Vector2 gridSize = (size - padding + spacing) / (cellSize + spacing);

            int rows = Mathf.FloorToInt(gridSize.y);
            int columns = Mathf.FloorToInt(gridSize.x);
            return new Vector2Int(rows, columns);
        }

        public static int GetElementCount(this GridLayoutGroup gridLayout, bool IsCountHide = true)
        {
            if (IsCountHide)
            {
                return gridLayout.transform.childCount;
            }
            int res = 0;
            for (int i = 0; i < gridLayout.transform.childCount; ++i)
            {
                if (gridLayout.transform.GetChild(i).gameObject.activeSelf == true)
                {
                    ++res;
                }
            }
            return res;
        }

    }

}

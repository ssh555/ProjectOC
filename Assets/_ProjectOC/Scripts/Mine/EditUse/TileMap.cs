using Sirenix.OdinInspector;
using UnityEngine;

namespace MineSystem
{
    public class TileMap : MonoBehaviour
    {
        public int TileWidth
        {
            get
            {
                return SmallMapEditData.width;
            }
            set
            {
                SmallMapEditData.width = value;
            }
        }
        public int TileHeight
        {
            get
            {
                return SmallMapEditData.height;
            }
            set
            {
                SmallMapEditData.height = value;
            }
        }

        public float mineScale = 1;
        [HideInInspector] public GameObject[,] tiles;
        [HideInInspector] public GameObject TilePrefab, MinePrefab, selectOutline, brushIcon;
        [HideInInspector] public Transform gridParentTransf, mineParentTransf;
        public MineSmallMapEditData SmallMapEditData;

        public int EditOption = 0;
        public MineBigMapEditData.MineBrushData curMineBrush = null;
        [Tooltip("0�� 1 Ȧ")] public bool brushTypeIsCircle = false;
        public bool isShiftPressed = false;

        public float brushSizeScale = 0.1f; //sprite.scale Ϊ1ʱ����Χ��2,�뾶��1

        public void SetTileSprite(int x, int y, bool pen)
        {
            float _offset = 0.5f;
            if (x >= 0 && x < TileWidth && y >= 0 && y < TileHeight)
            {
                // ��ָ��λ��ʵ�����µ���Ƭ
                if (TilePrefab != null)
                {
                    GameObject tile = tiles[x, y];
                    if (tiles[x, y] == null)
                    {
                        tile = Instantiate(TilePrefab, new Vector3(x + _offset, y + _offset, 0), Quaternion.identity);
                        tile.name = $"{x}_{y}";
                        tile.transform.parent = gridParentTransf;
                        tile.SetActive(true);
                        tiles[x, y] = tile;
                    }

                    if (pen)
                    {
                        tile.GetComponent<SpriteRenderer>().color = Color.black;
                    }
                    else
                    {
                        tile.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Tile position out of bounds.");
            }
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public int Width = 10;
    public int Height = 10;
    public GameObject[,] tiles = new GameObject[10,10];
    [ShowInInspector]
    public bool[,] gridData = new bool[10,10];  //true 有实体
    public GameObject TilePrefab;
    public Transform gridParentTransf,blockParentTransf;
    
    #region EditorProperty
    public GameObject selectOutline;
    private GameObject selectGo;
    public GameObject SelectGo
    {
        get
        {
            return selectGo;
        }
        set
        {
            selectGo = value;
            if (selectGo == null)
            {
                selectOutline.SetActive(false);
            }
            else
            {
                selectOutline.SetActive(true);
                selectOutline.transform.position = selectGo.transform.position;
            }
        }
    }

    #endregion
    public void Awake()
    {
        //tiles = new GameObject[Width, Height];
    }

    public void SetTileSprite(int x, int y,bool pen)
    {
        
        float _offset =  0.5f;
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            // 在指定位置实例化新的瓦片
            if (TilePrefab != null)
            {
                GameObject tile = tiles[x, y];
                if (tiles[x, y] == null)
                {
                    tile = Instantiate(TilePrefab, new Vector3(x + _offset, y + _offset, 0), Quaternion.identity);
                    tile.name = $"{x}_{y}";
                    tile.transform.parent = gridParentTransf;
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
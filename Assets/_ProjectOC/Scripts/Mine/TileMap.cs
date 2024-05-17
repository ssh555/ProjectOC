using Sirenix.OdinInspector;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    
    public int Width = 10;
    public int Height = 10;
    
    public float mineScale = 1;
    [HideInInspector]
    public GameObject[,] tiles;
    [HideInInspector]
    public bool[,] gridData;  //true 有实体
    [HideInInspector]
    public GameObject TilePrefab,MinePrefab,selectOutline,brushIcon;
    [HideInInspector]
    public Transform gridParentTransf,mineParentTransf;
    
    
    public int selectedOption = 0;
    [Tooltip("0点 1 圈")]
    public int brushType = 0;
    public bool isShiftPressed = false;
    
    public float brushSizeScale = 0.1f;  //sprite.scale 为1时，范围是2,半径是1
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
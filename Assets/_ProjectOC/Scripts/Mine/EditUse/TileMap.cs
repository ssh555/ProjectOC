using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectOC.MineSystem
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

        [HideInInspector] public GameObject[,] tiles;
        [HideInInspector] public GameObject TilePrefab, MinePrefab, selectOutline, brushIcon;
        [HideInInspector] public Transform gridParentTransf, mineParentTransf,textMeshTransf;
        public MineSmallMapEditData SmallMapEditData;

        public int EditOption = 0;
        public MineBigMapEditData.MineBrushData curMineBrush = null;
        [Tooltip("0点 1 圈")] public bool brushTypeIsCircle = false;
        public bool isShiftPressed = false;

        public float brushSizeScale = 0.1f; //sprite.scale 为1时，范围是2,半径是1
        public BigMap bigMap;

        public void SceneInit()
        {
            gridParentTransf = GameObject.Find("Editor/GridTransform").transform;
            mineParentTransf = GameObject.Find("Editor/MineTransform").transform;
            selectOutline = GameObject.Find("Editor/Others").transform.GetChild(0).gameObject;
            TilePrefab = GameObject.Find("Editor/Others").transform.GetChild(1).gameObject;
            MinePrefab = GameObject.Find("Editor/Others").transform.GetChild(2).gameObject;
            brushIcon = GameObject.Find("Editor/Others").transform.GetChild(3).gameObject;
            textMeshTransf = GameObject.Find("Editor/Others").transform.GetChild(5);
            bigMap = transform.GetComponentInParent<BigMap>();
            if (SmallMapEditData == null)
            {
                string[] nameStr = gameObject.name.Split("_");
                int _index = int.Parse(nameStr[1]);
                SmallMapEditData = bigMap.SmallMapEditDatas[_index];
            }
            
            curMineBrush = bigMap.BigMapEditDatas.MineBrushDatas[0];
        }
        
        public void SetTileSprite(int x, int y, bool pen)
        {
            float _offset = 0.5f;
            if (x >= 0 && x < TileWidth && y >= 0 && y < TileHeight)
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

                    BigMap _bigMap = GetComponentInParent<BigMap>();
                    if (pen)
                    {
                        tile.GetComponent<SpriteRenderer>().color = _bigMap.dataTileColor;
                    }
                    else
                    {
                        tile.GetComponent<SpriteRenderer>().color = _bigMap.emptyTileColor;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Tile position out of bounds.");
            }
        }

        public void ReGenerateTileGo()
        {
            tiles = new GameObject[TileWidth, TileHeight];
            //生成可视化方块
            DestroyTransformChild(gridParentTransf);

            for (int i = 0; i < TileWidth; i++)
            {
                for (int j = 0; j < TileHeight; j++)
                {
                    SetTileSprite(i, j, SmallMapEditData.gridData[i + j *TileWidth]);
                }
            }
        }
        public void ReGenerateMine()
        {
            DestroyTransformChild(mineParentTransf);
            foreach (var _singleMineData in SmallMapEditData.mineData)
            {
                MineBigMapEditData.MineBrushData _mineBrush =
                    GetComponentInParent<BigMap>().BigMapEditDatas.IDToMineBrushData(_singleMineData.MineID);
                for (int i = 0; i < _singleMineData.MinePoses.Count; i++)
                {
                    GameObject _mine = Instantiate(MinePrefab, 
                        new Vector3(_singleMineData.MinePoses[i].x,_singleMineData.MinePoses[i].y,0), Quaternion.identity,
                        mineParentTransf);
                    
                    ProcessMine(_mine, _mineBrush,i);
                }
            }
        }
        public void ProcessMine(GameObject _mineGo,MineBigMapEditData.MineBrushData _mineBrushData,int _index)
        {                
            _mineGo.SetActive(true);
            _mineGo.GetComponent<SpriteRenderer>().sprite = _mineBrushData.mineIcon;
            _mineGo.transform.localScale = Vector3.one * bigMap.mineToTileScale;
            _mineGo.name = $"{_mineBrushData.mineID}|{_index}";
        }
        public static void DestroyTransformChild(Transform _transf)
        {
            for (int i = _transf.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_transf.GetChild(i).gameObject);
            }
        }
    }
}
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    public class WorldItemSpawner : MonoBehaviour
    {
        [Button("生成 WorldItem")]
        public WorldItem SpawnerWorldItem(int id)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                ItemSpawner.ItemTabelData data = new ItemSpawner.ItemTabelData();
                var datas = Utility.CSVUtils.ParseCSV("CSV/ItemTableData", 1, true, true);
                foreach(var row in datas)
                {
                    // ID -> 0
                    if(int.Parse(row[0]) != id)
                    {
                        continue;
                    }
                    // Name -> 1
                    data.name = row[1];
                    // Type -> 2
                    data.type = ItemSpawner.typePath + row[2];
                    // bCanStack -> 3
                    data.bCanStack = row[3] == "1" ? true : false;
                    // MaxAmount -> 4
                    data.maxAmount = int.Parse(row[4]);

                    // SpritePath -> 5
                    Texture2D tex = Resources.Load<Texture2D>("Items/Sprite/" + row[5].Trim());
                    if(tex == null)
                    {
                        var t = Resources.LoadAll<Texture2D>("Items/Sprite/");
                        foreach(var tt in t)
                        {
                            if(tt.name == row[5].Trim())
                            {
                                tex = tt;
                                break;
                            }
                        }
                    }
                    data.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

                    // WorldObjectPath -> 6
                    data.worldObject = Resources.Load<GameObject>("Items/WorldItem/" + row[6].Trim());
                    if (data.worldObject == null)
                    {
                        var t = Resources.LoadAll<GameObject>("Items/WorldItem/");
                        foreach (var g in t)
                        {
                            if(g.name == row[6].Trim())
                            {
                                data.worldObject = g;
                                break;
                            }
                        }
                    }
                    System.Type type = ItemSpawner.GetTypeByName(data.type);

                    Item item = System.Activator.CreateInstance(type, id) as Item;

                    item.Init(data);
                    var ans = ItemSpawner.SpawnWorldItemInEditor(data.worldObject, item, this.transform.position, this.transform.rotation);
                    //ans.transform.SetParent(this.transform);
                    return ans;
                }
                Debug.LogError("没有对应ID为 " + id + " 的Item");
                return null;
            }
#endif

            var ret = ItemSpawner.Instance.SpawnWorldItem(ItemSpawner.Instance.SpawnItem(id), this.transform.position, this.transform.rotation);
            ret.transform.SetParent(this.transform);
            return ret;
        }

    }
}


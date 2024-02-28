using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using ML.Engine.Manager;
using ProjectOC.LandMassExpand;
using Sirenix.OdinInspector;

public class NavMeshManager : MonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
{
    private static NavMeshManager Instance = null;
    
    [SerializeField]
    private List<NavMeshSurface> surfacesToReBake;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.RegisterLocalManager(this);
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            if(ML.Engine.Manager.GameManager.Instance != null)
                ML.Engine.Manager.GameManager.Instance.UnregisterLocalManager<BuildPowerIslandManager>();
            Instance = null;
        }    
    }
    
    public void UpdateNaveMeshSurfaces()
    {
        float _time = Time.realtimeSinceStartup;
        foreach (var _nvs in surfacesToReBake)
        {
            _nvs.UpdateNavMesh(_nvs.navMeshData);
        }
        surfacesToReBake.Clear();
        Debug.Log($"time cost: {Time.realtimeSinceStartup-_time}");
    }

    public void JudgeNVSPosition(Vector3 _pos)
    {
        IslandBase currentIsland = GameManager.Instance.GetLocalManager<IslandManager>().currentIsland;
        foreach (var _islandFieldPart in currentIsland.islandFieldParts)
        {
            foreach (var _bounds in _islandFieldPart.fieldBounds)
            {
                Bounds tempBounds = new Bounds(_bounds.center + currentIsland.transform.position, _bounds.size);
                if (tempBounds.Contains(_pos))
                {
                    AddNVSToUpdate(_islandFieldPart.navMeshSurface);
                    return;
                }
            }
        }   
    }
    
    public void AddNVSToUpdate(NavMeshSurface navMeshSurface)
    {
        if (!surfacesToReBake.Contains(navMeshSurface))
        {
            surfacesToReBake.Add(navMeshSurface);
        }
    }
    
    // [Button()]
    // private void GEN()
    // {
    //     Debug.Log($"Start: {Time.realtimeSinceStartup}");
    //     var ao = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
    //     ao.completed += (operation =>
    //     {
    //         Debug.Log($"End: {Time.realtimeSinceStartup}");
    //     });
    //     Debug.Log("QWQ");
    // }
    

}

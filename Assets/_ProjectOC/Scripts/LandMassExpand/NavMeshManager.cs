using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPlacer;
using UnityEngine;
using Unity.AI.Navigation;
using ML.Engine.Manager;
using ProjectOC.LandMassExpand;
using Sirenix.OdinInspector;

public class NavMeshManager : MonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
{
    private static NavMeshManager Instance = null;
    
    private List<NavMeshSurface> surfacesToReBake = new List<NavMeshSurface>();

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
        //BuildingPlacer BP = BuildingManager.Instance.Placer;
        // BuildingPlacer BP = FindObjectOfType<BuildingPlacer>();
        // BP.OnPlaceModeSuccess += (bpart) =>
        // {
        //     JudgeNVSPosition(BP.SelectedPartInstance.transform);
        // };
        // BP.OnEditModeExit += (bpart) =>
        // {
        //     JudgeNVSPosition(BP.SelectedPartInstance.transform);
        // };
        // BP.OnBuildingModeExit += () =>
        // {
        //     UpdateNaveMeshSurfaces();
        // };
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            if(ML.Engine.Manager.GameManager.Instance != null)
                ML.Engine.Manager.GameManager.Instance.UnregisterLocalManager<NavMeshManager>();
            Instance = null;
        }    
    }
    
    public void UpdateNaveMeshSurfaces()
    {
        float startT = Time.realtimeSinceStartup;
        foreach (var _nvs in surfacesToReBake)
        {
            _nvs.UpdateNavMesh(_nvs.navMeshData);
        }
        surfacesToReBake.Clear();
        Debug.Log($"time cost: {Time.realtimeSinceStartup-startT}");
    }
    
    public void JudgeNVSPosition(Transform _transf)
    {
        {
            NavMeshSurface _navMeshSurface = _transf.GetComponentInParent<NavMeshSurface>();
            //如果没有父节点，说明是新建，不用管老位置
            if (_navMeshSurface != null)
            {
                AddNVSToUpdate(_navMeshSurface);
            }
        }
        
        //设新的父节点
        IslandBase currentIsland = GameManager.Instance.GetLocalManager<IslandManager>().currentIsland;
        foreach (var _islandFieldPart in currentIsland.islandFieldParts)
        {
            foreach (var _bounds in _islandFieldPart.fieldBounds)
            {
                Bounds tempBounds = new Bounds(_bounds.center + currentIsland.transform.position, _bounds.size);
                if (tempBounds.Contains(_transf.position))
                {
                    _transf.SetParent(_islandFieldPart.buildingPartsTransf);
                    AddNVSToUpdate(_islandFieldPart.navMeshSurface);
                    return;
                }
            }
        } 
        
        Debug.LogWarning("Can't find islandArea");
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

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

public class NavMeshManager :ML.Engine.Manager.LocalManager.ILocalManager
{
    private List<NavMeshSurface> surfacesToReBake = new List<NavMeshSurface>();

    public NavMeshManager()
    {
         BuildingPlacer BP = BuildingManager.Instance.Placer;
         //BuildingPlacer BP = FindObjectOfType<BuildingPlacer>();
         BP.OnPlaceModeSuccess += (bpart) =>
         {
             JudgeNVSPosition(bpart.transform);
         };
         BP.OnEditModeSuccess += (bpart,pos1,pos2) =>
         {
             JudgeNVSPosition(bpart.transform);
         };
         BP.OnBuildingModeExit += () =>
         {
             UpdateNaveMeshSurfaces();
         };
    }
    
    public void UpdateNaveMeshSurfaces()
    {
        GameManager.Instance.GetLocalManager<IslandManager>().currentIsland.SetSurfacesTrigger(surfacesToReBake,false);
#if UNITY_EDITOR
        float startT = Time.realtimeSinceStartup;
#endif
        foreach (var _nvs in surfacesToReBake)
        {
            _nvs.UpdateNavMesh(_nvs.navMeshData);
        }

        GameManager.Instance.GetLocalManager<IslandManager>().currentIsland.SetSurfacesTrigger(surfacesToReBake);
        surfacesToReBake.Clear();
#if  UNITY_EDITOR
        Debug.Log($"navmesh bake time cost: {Time.realtimeSinceStartup-startT}");
#endif
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
        
        Debug.LogWarning(_transf.name+ "Can't find islandArea");
    }
    
    public void AddNVSToUpdate(NavMeshSurface navMeshSurface)
    {
        if (!surfacesToReBake.Contains(navMeshSurface))
        {
            surfacesToReBake.Add(navMeshSurface);
        }
    }


}

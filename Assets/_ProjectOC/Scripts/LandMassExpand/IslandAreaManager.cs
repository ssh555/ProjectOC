using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.BuildingSystem.BuildingPlacer;
using UnityEngine;
using Unity.AI.Navigation;
using ML.Engine.Manager;
using ProjectOC.LandMassExpand;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;

[System.Serializable]
public class IslandAreaManager :ML.Engine.Manager.LocalManager.ILocalManager
{
#region 岛屿区域管理

        public List<Transform> updatedFieldTransforms = new List<Transform>();

        /// <summary>
        /// 区域建筑发生变化事件，退出建造模式的时候调用一次
        /// </summary>
        private BuildingPlacer buildingPlacer;
        public Action UpdatedFieldTransformsAction;

        private Action<IBuildingPart> OnPlaceModeSuccessHandler;
        private Action<IBuildingPart,Vector3,Vector3> OnEditModeSuccessHandler;
        private Action OnBuildingModeExitHandler;
        private Action<IBuildingPart> OnDestroySelectedBPartHandler;
        public void OnRegister()
        {
            OnPlaceModeSuccessHandler += (bpart) =>
            {
                JudgeUpdateField(bpart.transform);
            };
            
            OnEditModeSuccessHandler += (bpart, pos1, pos2) =>
            {
                JudgeUpdateField(bpart.transform);
                UpdatedFieldTransformsAction?.Invoke();
            };
            OnBuildingModeExitHandler += () =>
            {
                UpdateNaveMeshSurfaces();
                UpdatedFieldTransformsAction?.Invoke();
                ClearUpdateTransform();
            };
            OnDestroySelectedBPartHandler += (bpart) =>
            {
                JudgeUpdateField(bpart.transform);
                UpdatedFieldTransformsAction?.Invoke();
            };
            
            buildingPlacer = BuildingManager.Instance.Placer;
            buildingPlacer.OnPlaceModeSuccess += OnPlaceModeSuccessHandler;
            buildingPlacer.OnEditModeSuccess += OnEditModeSuccessHandler;
            buildingPlacer.OnBuildingModeExit += OnBuildingModeExitHandler;
            buildingPlacer.OnDestroySelectedBPart += OnDestroySelectedBPartHandler;
        }
        //UpdateFieldTransform List控制
        public void JudgeUpdateField(Transform _transf)
        {
            //如果是编辑更换位置，原区域也会标记
            if (_transf.parent != null)
            {
                AddTransformToUpdate(_transf.parent); 
            }
            
            //设新的父节点
            IslandBase currentIsland = ML.Engine.Manager.GameManager.Instance.GetLocalManager<IslandModelManager>().currentIsland;
            foreach (var _islandFieldPart in currentIsland.islandFieldParts)
            {
                foreach (var _bounds in _islandFieldPart.fieldBounds)
                {
                    Bounds tempBounds = new Bounds(_bounds.center + currentIsland.transform.position, _bounds.size);
                    if (tempBounds.Contains(_transf.position))
                    {
                        _transf.SetParent(_islandFieldPart.buildingPartsTransf);

                        AddTransformToUpdate(_islandFieldPart.buildingPartsTransf);
                        return;
                    }
                }
            }

            Debug.LogWarning(_transf.name + "Can't find islandArea");
        }

        void AddTransformToUpdate(Transform _transf)
        {
            if (!updatedFieldTransforms.Contains(_transf))
            {
                updatedFieldTransforms.Add(_transf);
            }
        }

        public void ClearUpdateTransform()
        {
            updatedFieldTransforms.Clear();
        }
        
        //NavmeshSurface控制
        public void UpdateNaveMeshSurfaces()
        {
            List<NavMeshSurface> navMeshSurfacesToReBake = new List<NavMeshSurface>();
            foreach (var _transf in updatedFieldTransforms)
            {
                navMeshSurfacesToReBake.Add(_transf.GetComponentInParent<NavMeshSurface>());
            }
            //BuildingArea 后续不需要设为Trigger
            //SetSurfacesTrigger(navMeshSurfacesToReBake,false);

            foreach (var _nms in navMeshSurfacesToReBake)
            {
                _nms.UpdateNavMesh(_nms.navMeshData);
            }
            //SetSurfacesTrigger(navMeshSurfacesToReBake);
        }
        
        //设置NavmeshSurface collider Trigger为碰撞，碰撞时才能有效烘焙
        public void SetSurfacesTrigger(List<NavMeshSurface> _NMSes,bool _isTrue = true)
        {
            foreach (var _nms in _NMSes)
            {
                MeshCollider[] meshColliders = _nms.transform.Find("TerrainPartCollider").GetComponentsInChildren<MeshCollider>();
                foreach (var meshCollider in meshColliders)
                {
                    meshCollider.isTrigger = _isTrue;
                }
            }
        }

        public void OnUnregister()
        {
            buildingPlacer.OnPlaceModeSuccess -= OnPlaceModeSuccessHandler;
            buildingPlacer.OnEditModeSuccess -= OnEditModeSuccessHandler;
            buildingPlacer.OnBuildingModeExit -= OnBuildingModeExitHandler;
            buildingPlacer.OnDestroySelectedBPart -= OnDestroySelectedBPartHandler;
        }

        #endregion
    }

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
#region �����������

        public List<Transform> updatedFieldTransforms = new List<Transform>();
        /// <summary>
        /// �����������仯�¼����˳�����ģʽ��ʱ�����һ��
        /// </summary>
        public Action UpdatedFieldTransformsAction;

        public void Init()
        {
            BuildingPlacer buildingPlacer = BuildingManager.Instance.Placer;
            buildingPlacer.OnPlaceModeSuccess += (bpart) =>
            {
                JudgeUpdateField(bpart.transform);
            };
            buildingPlacer.OnEditModeSuccess += (bpart, pos1, pos2) =>
            {
                JudgeUpdateField(bpart.transform);
                UpdatedFieldTransformsAction?.Invoke();
            };
            buildingPlacer.OnBuildingModeExit += () =>
            {
                UpdateNaveMeshSurfaces();
                UpdatedFieldTransformsAction?.Invoke();
                ClearUpdateTransform();
            };
            buildingPlacer.OnDestroySelectedBPart += (bpart) =>
            {
                JudgeUpdateField(bpart.transform);
                UpdatedFieldTransformsAction?.Invoke();
            };
        }
        //UpdateFieldTransform List����
        public void JudgeUpdateField(Transform _transf)
        {
            //����Ǳ༭����λ�ã�ԭ����Ҳ����
            if (_transf.parent != null)
            {
                AddTransformToUpdate(_transf.parent); 
            }
            
            //���µĸ��ڵ�
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
        
        //NavmeshSurface����
        public void UpdateNaveMeshSurfaces()
        {
            List<NavMeshSurface> navMeshSurfacesToReBake = new List<NavMeshSurface>();
            foreach (var _transf in updatedFieldTransforms)
            {
                navMeshSurfacesToReBake.Add(_transf.GetComponentInParent<NavMeshSurface>());
            }
            SetSurfacesTrigger(navMeshSurfacesToReBake,false);

            foreach (var _nms in navMeshSurfacesToReBake)
            {
                _nms.UpdateNavMesh(_nms.navMeshData);
            }
            SetSurfacesTrigger(navMeshSurfacesToReBake);
        }
        
        //����NavmeshSurface collider TriggerΪ��ײ����ײʱ������Ч�決
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
        #endregion
    }

using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem
{
    [CreateAssetMenu(fileName = "BSBPartMatPackage", menuName = "ML/BuildingSystem/BSBPartMatPackage", order = 1)]
    public class BSBPartMatPackage : SerializedScriptableObject
    {
        [Serializable]
        public struct MatPackage
        {
            public Material[] Mats;
        }

        [Serializable]
        public struct MatPackageDict
        {
            public Texture2D Tex;
            [LabelText("����")]
            public MatPackage MPs;
        }


        public BuildingPartClassification Classification;

        [LabelText("���ʽṹ"), PropertyTooltip("-1 ��ӦRootRenderer, �����ӦTranform.GetChild(i)")]
        public int[] MatStruct;

        [LabelText("���ʰ�")]
        public List<MatPackageDict> MatPackages = new List<MatPackageDict>();

        public Dictionary<Texture2D, BuildingCopiedMaterial> ToMatPackage()
        {
            Dictionary<Texture2D, BuildingCopiedMaterial> ret = new Dictionary<Texture2D, BuildingCopiedMaterial>();

            for(int i = 0; i < MatPackages.Count; ++i)
            {
                BuildingCopiedMaterial mat = new BuildingCopiedMaterial();

                foreach (int index in MatStruct)
                {
                    if(index != -1)
                    {
                        mat.ChildrenMat.Add(index, MatPackages[i].MPs.Mats);
                    }
                    else
                    {
                        mat.ParentMat = MatPackages[i].MPs.Mats;
                    }
                }
                ret.Add(MatPackages[i].Tex, mat);
            }

            return ret;
        }
    }
}


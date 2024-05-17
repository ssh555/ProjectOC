using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using ML.Engine.Animation;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(AnimationAssetBase), true)]
    public class AnimationAssetBaseEditor : UnityEditor.Editor
    {
        [OnOpenAsset(0)]
        public static bool ClickAction(int instanceID, int line)
        {
            AnimationAssetBase asset = EditorUtility.InstanceIDToObject(instanceID) as AnimationAssetBase;
            if (asset == null)
            {
                return false;
            }

            AnimancerTransitionAssetBaseWindow.OpenWithAsset(asset);

            return true;
        }

        public virtual void DrawInEditorWindow()
        {
            Debug.LogWarning($"{this.target.GetType()} 没有实现Editor");
            OnInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }

}

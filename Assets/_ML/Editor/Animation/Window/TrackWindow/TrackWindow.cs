using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ML.Editor.Animation
{
    public class TrackWindow : EditorWindow
    {
        public static TrackWindow Instance
        {
            get;
            private set;
        }

        private void OnGUI()
        {
            if (AnimationWindow.Instance != null && AnimationWindow.Instance.AssetEditor != null)
            {
                AnimationWindow.Instance.AssetEditor.DrawTrack();
            }
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }
    }


}
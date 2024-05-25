using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(ClipTransitionAsset), true)]
    public class ClipTransitionAssetEditor : AnimationAssetBaseEditor
    {
        protected EventTrack eventTrack;


        public override void Init()
        {
            base.Init();
            ClipTransitionAsset tmp = (ClipTransitionAsset)target;
            eventTrack = new EventTrack(tmp);
            
            eventTrack.End = tmp.clipTransition.Clip.frameRate * tmp.clipTransition.Clip.length;
        }

        public override void DrawTrackGUI()
        {
            eventTrack.DrawTrackGUI();
        }
    }

}

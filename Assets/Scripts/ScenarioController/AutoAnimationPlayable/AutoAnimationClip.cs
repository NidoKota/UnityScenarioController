using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;

namespace ScenarioController.AutoAnimationPlayable
{
    /// <summary>
    /// AutoAnimationPlayableのClip
    /// </summary>
    [Serializable]
    public class AutoAnimationClip : PlayableAsset, ITimelineClipAsset
    {
        public AnimationClip animationClip;

#if UNITY_EDITOR
        //Editor上で使う情報
        [HideInInspector]
        public AutoAnimationTrack autoAnimationTrack;
#endif

        public ClipCaps clipCaps
        {
            get { return ClipCaps.All; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return AnimationClipPlayable.Create(graph, animationClip);
        }
    }
}


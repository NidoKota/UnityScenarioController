using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

#if UNITY_EDITOR
    /// <summary>
    /// ScenarioEventStopPlayableClipのEditor拡張
    /// </summary>
    [CustomEditor(typeof(AutoAnimationClip))]
    public class AutoAnimationClipEditor : Editor
    {
        AutoAnimationClip autoAnimationClip;
        void OnEnable()
        {
            autoAnimationClip = target as AutoAnimationClip;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            int clipIndex = autoAnimationClip.autoAnimationTrack.GetClipIndex(autoAnimationClip);
            GUILayout.Label($"ClipIndex : {clipIndex}");
            if (GUILayout.Button("SetClipLengthForAnimationLength"))
            {
                float length = autoAnimationClip.animationClip.length;
                autoAnimationClip.autoAnimationTrack.SetClipLength(clipIndex, length == 0 ? 1 : length);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}


using System;
using UnityEngine;
using UnityEditor;

namespace ScenarioController.AutoAnimationPlayable.Editor
{
    /// <summary>
    /// AutoAnimationClipのEditor拡張
    /// </summary>
    [CustomEditor(typeof(AutoAnimationClip))]
    public class AutoAnimationClipEditor : UnityEditor.Editor
    {
        AutoAnimationClip autoAnimationClip;

        void OnEnable()
        {
            autoAnimationClip = (AutoAnimationClip)target;
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
}
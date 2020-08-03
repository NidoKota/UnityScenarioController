using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScenarioController.ScenarioEventPlayable.Editor
{
    /// <summary>
    /// ScenarioEventMovePlayableClipのEditor拡張
    /// </summary>
    [CustomEditor(typeof(ScenarioEventMovePlayableClip))]
    public class ScenarioEventMovePlayableClipEditor : UnityEditor.Editor
    {
        ScenarioEventMovePlayableClip scenarioEventMovePlayableClip;
        void OnEnable()
        {
            scenarioEventMovePlayableClip = target as ScenarioEventMovePlayableClip;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (scenarioEventMovePlayableClip.clipIn) EditorGUILayout.HelpBox("発火完了", MessageType.Info);
            else EditorGUILayout.HelpBox("発火未完了", MessageType.Info);
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}

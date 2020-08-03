using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScenarioController.ScenarioEventPlayable.Editor
{
    /// <summary>
    /// ScenarioEventStopPlayableClipのEditor拡張
    /// </summary>
    [CustomEditor(typeof(ScenarioEventStopPlayableClip))]
    public class ScenarioEventStopPlayableClipEditor : UnityEditor.Editor
    {
        ScenarioEventStopPlayableClip scenarioEventStopPlayableClip;
        void OnEnable()
        {
            scenarioEventStopPlayableClip = target as ScenarioEventStopPlayableClip;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (scenarioEventStopPlayableClip.finishFiring) EditorGUILayout.HelpBox("発火完了", MessageType.Info);
            else EditorGUILayout.HelpBox("発火未完了", MessageType.Info);
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}

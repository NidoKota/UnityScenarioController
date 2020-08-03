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

            if (scenarioEventStopPlayableClip.finishFiring) EditorGUILayout.HelpBox("Fired", MessageType.Info);
            else EditorGUILayout.HelpBox("NotFired", MessageType.Info);

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }
    }
}

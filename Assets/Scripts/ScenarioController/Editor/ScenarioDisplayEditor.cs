using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScenarioController.Editor
{
    /// <summary>
    /// ScenarioDisplayのEditor拡張
    /// </summary>
    [CustomEditor(typeof(ScenarioDisplay))]
    public class ScenarioDisplayEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
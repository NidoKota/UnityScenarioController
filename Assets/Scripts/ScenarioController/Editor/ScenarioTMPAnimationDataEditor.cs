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
    [CustomEditor(typeof(ScenarioTMPAnimationData))]
    public class ScenarioTMPAnimationDataEditor : UnityEditor.Editor
    {
        ScenarioTMPAnimationData scenarioTMPAnimationData;
        List<string> excluding = new List<string>(5);

        int flag;
        int beforeFlag;

        public void OnEnable()
        {
            scenarioTMPAnimationData = (ScenarioTMPAnimationData)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            flag = 0;
            if (scenarioTMPAnimationData.usePositionYAnimation) flag |= 1 << 0;
            if (scenarioTMPAnimationData.useRotationZAnimation) flag |= 1 << 1;
            if (scenarioTMPAnimationData.useScaleAllAnimation) flag |= 1 << 2;
            if (scenarioTMPAnimationData.useColorAlphaAnimation) flag |= 1 << 3;

            if(flag != beforeFlag)
            {
                excluding.Clear();
                excluding.Add("m_Script");

                if (!scenarioTMPAnimationData.usePositionYAnimation) excluding.Add("positionYAnimation");
                if (!scenarioTMPAnimationData.useRotationZAnimation) excluding.Add("rotationZAnimation");
                if (!scenarioTMPAnimationData.useScaleAllAnimation) excluding.Add("scaleAllAnimation");
                if (!scenarioTMPAnimationData.useColorAlphaAnimation) excluding.Add("colorAlphaAnimation");
            }

            DrawPropertiesExcluding(serializedObject, excluding.ToArray());
            serializedObject.ApplyModifiedProperties();

            beforeFlag = flag;
        }
    }
}
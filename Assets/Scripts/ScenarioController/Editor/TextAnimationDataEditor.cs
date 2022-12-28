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
    [CustomEditor(typeof(TextAnimationData))]
    public class TextAnimationDataEditor : UnityEditor.Editor
    {
        private TextAnimationData _scenarioTMPAnimationData;
        private readonly List<string> _excludes = new List<string>(5);
        private int _beforeFlag;

        public void OnEnable()
        {
            _scenarioTMPAnimationData = target as TextAnimationData;
            CalcExcludes();
        }

        public override void OnInspectorGUI()
        {
            if (_scenarioTMPAnimationData == null) return;
            
            serializedObject.Update();
            {
                int flag = 0;
                if (_scenarioTMPAnimationData.UsePositionXAnimation) flag |= 1 << 0;
                if (_scenarioTMPAnimationData.UsePositionYAnimation) flag |= 1 << 1;
                if (_scenarioTMPAnimationData.UseRotationZAnimation) flag |= 1 << 2;
                if (_scenarioTMPAnimationData.UseScaleAllAnimation) flag |= 1 << 3;
                if (_scenarioTMPAnimationData.UseColorAlphaAnimation) flag |= 1 << 4;

                if (flag != _beforeFlag)
                {
                    CalcExcludes();
                    _beforeFlag = flag;
                }

                DrawPropertiesExcluding(serializedObject, _excludes.ToArray());
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void CalcExcludes()
        {
            if (_scenarioTMPAnimationData == null) return;
            
            _excludes.Clear();
            _excludes.Add("m_Script");

            if (!_scenarioTMPAnimationData.UsePositionXAnimation) _excludes.Add("_positionXAnimation");
            if (!_scenarioTMPAnimationData.UsePositionYAnimation) _excludes.Add("_positionYAnimation");
            if (!_scenarioTMPAnimationData.UseRotationZAnimation) _excludes.Add("_rotationZAnimation");
            if (!_scenarioTMPAnimationData.UseScaleAllAnimation) _excludes.Add("_scaleAllAnimation");
            if (!_scenarioTMPAnimationData.UseColorAlphaAnimation) _excludes.Add("_colorAlphaAnimation");
        }
    }
}
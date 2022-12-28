using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditorInternal;
using UnityEngine;

namespace ScenarioController.Editor
{
    /// <summary>
    /// Scenarioの配列を表示するReorderableListを操作するラッパー
    /// </summary>
    public class ScenariosReorderableListWrapper
    {
        private readonly SerializedObject _serializedObject;
        private readonly SerializedProperty _scenariosProp;
        private readonly ReorderableList _scenarioReorderableList;
        private readonly Action<int> _onSelect;
        private const string ListName = "Scenarios";

        public ScenariosReorderableListWrapper(SerializedObject serializedObject, SerializedProperty scenariosProp, Action<int> onSelect)
        {
            _serializedObject = serializedObject;
            _scenariosProp = scenariosProp;
            _scenarioReorderableList = GetSpritePropReorderableList();
            _onSelect = onSelect;
        }

        public Rect Draw(Rect lineRect)
        {
            if (_scenariosProp.isExpanded)
            {
                lineRect = new Rect(lineRect) {yMax = lineRect.yMin + GetHeight()};
                _scenarioReorderableList.DoList(lineRect);
            }
            else _scenariosProp.isExpanded = EditorGUI.Foldout(lineRect, _scenariosProp.isExpanded, ListName, true);

            return Utilities.LineRectUpdate(lineRect);
        }

        private float GetHeight()
        {
            float height = EditorGUIUtility.singleLineHeight;
            SerializedProperty scenariosProp = _scenariosProp;
            if (scenariosProp.isExpanded)
            {
                height += 30;
                if (scenariosProp.arraySize == 0) height += 20;
                for (int i = 0; i < scenariosProp.arraySize; i++) height += 80;
            }
            else height += EditorGUIUtility.singleLineHeight;

            return height;
        }

        private ReorderableList GetSpritePropReorderableList()
        {
            ReorderableList rl = new ReorderableList(_serializedObject, _scenariosProp);

            rl.onAddCallback = OnAdd;
            rl.drawHeaderCallback = DrawHeader;
            rl.elementHeightCallback = GetElementHeight;
            rl.drawElementCallback = DrawElement;
            rl.onSelectCallback = OnSelect;

            return rl;
        }

        private void OnAdd(ReorderableList reorderableList)
        {
            _scenariosProp.arraySize++;
            SerializedProperty maxProp = _scenariosProp.GetArrayElementAtIndex(_scenariosProp.arraySize - 1);

            new SerializedWrapper(maxProp).SetScenarioData(default);
        }

        private void DrawHeader(Rect rect)
        {
            rect = new Rect(rect) {xMin = rect.xMin - 8, xMax = rect.xMax - 50};
            _scenariosProp.isExpanded = EditorGUI.Foldout(rect, _scenariosProp.isExpanded, ListName, true);
        }

        private float GetElementHeight(int index)
        {
            return 80;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.PropertyField(rect, _scenariosProp.GetArrayElementAtIndex(index));
        }

        private void OnSelect(ReorderableList reorderableList)
        {
            _onSelect.Invoke(reorderableList.index);
        }
    }
}
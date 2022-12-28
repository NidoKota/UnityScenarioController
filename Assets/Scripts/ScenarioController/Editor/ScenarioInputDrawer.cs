using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UObject = UnityEngine.Object;

namespace ScenarioController.Editor
{
    /// <summary>
    /// ScenarioInputの描画
    /// </summary>
    [CustomPropertyDrawer(typeof(ScenarioInput))]
    public class ScenarioInputDrawer : PropertyDrawer
    {
        private static ScenarioInputDrawer _currentPreviewScenarioInputDrawer;
        private static bool _isPreviewScenario;
        private static int _currentPreviewScenarioIndex;

        private readonly List<ScenariosReorderableListWrapper> _scenarioReorderableListWrappers = new List<ScenariosReorderableListWrapper>();
        private SerializedProperty _scenariosProp;
        private SerializedProperty _scenarioDisplayBaseProp;
        private int _endElementInitializeOnceCount;
        private int _prevIndex = -1;
        private bool _initializeOnce;
        private int _selectMinValue;
        private int _selectMaxValue;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            _scenariosProp = property.FindPropertyRelative("<Scenarios>k__BackingField");
            if (_scenariosProp.isExpanded)
            {
                height += 60;
                if (_scenariosProp.arraySize == 0) height += 20;
                for (int i = 0; i < _scenariosProp.arraySize; i++) height += 80;
                height += EditorGUIUtility.singleLineHeight;
                if (property.isExpanded) height += EditorGUIUtility.singleLineHeight * 4;
            }
            else height += EditorGUIUtility.singleLineHeight * 2;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using var _ = new EditorGUI.PropertyScope(position, label, property);
            CallEvents(position, property, label);
        }

        private void CallEvents(Rect position, SerializedProperty property, GUIContent label)
        {
            int index = Utilities.GetElementIndex(property);
            index = index < 0 ? 0 : index;

            if (!_initializeOnce)
            {
                Initialize(position, property, label, index);
                _initializeOnce = true;
            }

            if (_endElementInitializeOnceCount <= index)
            {
                ElementInitializeOnce(position, property, label, index);
                _endElementInitializeOnceCount++;
            }

            if (index != _prevIndex)
            {
                ElementInitialize(position, property, label, index);
                _prevIndex = index;
            }

            Draw(position, property, label, index);
        }

        /// <summary>
        /// 始めに1度だけ呼ばれる
        /// </summary>
        private void Initialize(Rect position, SerializedProperty property, GUIContent label, int index)
        {
            bool.TryParse(EditorUserSettings.GetConfigValue("PreviewScenario"), out _isPreviewScenario);
        }

        /// <summary>
        /// 1要素につき1回、OnGUIの1フレーム目に呼ばれる
        /// </summary>
        private void ElementInitializeOnce(Rect position, SerializedProperty property, GUIContent label, int index)
        {
            _scenariosProp = property.FindPropertyRelative("<Scenarios>k__BackingField");

            ScenariosReorderableListWrapper scenariosReorderableListWrapper = new ScenariosReorderableListWrapper(property.serializedObject, _scenariosProp, i =>
            {
                _currentPreviewScenarioInputDrawer = this;
                _currentPreviewScenarioIndex = i;
            });
            _scenarioReorderableListWrappers.Add(scenariosReorderableListWrapper);
        }

        /// <summary>
        /// OnGUIが違う要素を描画し始めた時、その最初の1フレーム目に呼ばれる
        /// </summary>
        private void ElementInitialize(Rect position, SerializedProperty property, GUIContent label, int index)
        {
            _scenariosProp = property.FindPropertyRelative("<Scenarios>k__BackingField");
            _scenarioDisplayBaseProp = property.FindPropertyRelative("<ScenarioDisplayBase>k__BackingField");
        }

        /// <summary>
        /// ここで描画と値の更新を行う
        /// </summary>
        private void Draw(Rect position, SerializedProperty property, GUIContent label, int index)
        {
            position = new Rect(position) {yMin = position.yMin + 4, yMax = position.yMax - 4};

            ScenarioDisplayBase scenarioDisplayBase = _scenarioDisplayBaseProp.objectReferenceValue ? _scenarioDisplayBaseProp.objectReferenceValue as ScenarioDisplayBase : null;
            bool isToolsExpanded = property.isExpanded;
            {
                DrawBackBox(position);
                Rect lineRect = Utilities.LineRectUpdate(new Rect(position) {yMin = position.yMin + 4, yMax = position.yMin + 4});
                lineRect = DrawScenarioDisplayBase(lineRect, ref scenarioDisplayBase);
                lineRect = DrawScenariosReorderableList(lineRect, _scenarioReorderableListWrappers[index]);
                lineRect = DrawTools(lineRect, _scenariosProp, ref isToolsExpanded, ref _selectMinValue, ref _selectMaxValue);
            }
            property.isExpanded = isToolsExpanded;
            _scenarioDisplayBaseProp.objectReferenceValue = scenarioDisplayBase;

            if (_isPreviewScenario) PreviewScenario(scenarioDisplayBase);
            else EndPreviewScenario(scenarioDisplayBase);
        }

        /// <summary>
        /// Scenarioをプレビューする
        /// </summary>
        /// <param name="scenarioDisplayBase">プレビューする先のScenarioDisplayBase</param>
        private void PreviewScenario(ScenarioDisplayBase scenarioDisplayBase)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (!scenarioDisplayBase || _currentPreviewScenarioInputDrawer != this) return;

            SerializedWrapper previewScenarioWrapper = new SerializedWrapper(_scenariosProp.GetArrayElementAtIndex(_currentPreviewScenarioIndex));
            ScenarioData previewScenario = previewScenarioWrapper.GetScenarioData();

            bool doUpdate = scenarioDisplayBase.tmp.text != previewScenario.Text;

            scenarioDisplayBase.GetComponent<CanvasGroup>().alpha = 1;
            scenarioDisplayBase.tmp.text = previewScenario.Text;

            if (doUpdate) EditorApplication.QueuePlayerLoopUpdate();
        }

        /// <summary>
        /// Scenarioをプレビューをやめる
        /// </summary>
        /// <param name="scenarioDisplayBase">プレビューをやめるScenarioDisplayBase</param>
        private void EndPreviewScenario(ScenarioDisplayBase scenarioDisplayBase)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (!scenarioDisplayBase || _currentPreviewScenarioInputDrawer != this) return;

            bool doUpdate = !string.IsNullOrEmpty(scenarioDisplayBase.tmp.text);

            scenarioDisplayBase.GetComponent<CanvasGroup>().alpha = 1;
            scenarioDisplayBase.tmp.text = null;

            if (doUpdate) EditorApplication.QueuePlayerLoopUpdate();
        }

        /// <summary>
        /// 指定した範囲にBoxスキンを適応する
        /// </summary>
        /// <param name="scope">Boxスキンを適応する範囲</param>
        private static void DrawBackBox(Rect scope)
        {
            GUI.Label(scope, GUIContent.none, GUI.skin.box);
        }

        /// <summary>
        /// ScenarioDisplayBaseを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenarioDisplayBase">インスタンスを格納する場所</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawScenarioDisplayBase(Rect lineRect, ref ScenarioDisplayBase scenarioDisplayBase)
        {
            scenarioDisplayBase = EditorGUI.ObjectField(lineRect, "ScenarioDisplayBase", scenarioDisplayBase, typeof(ScenarioDisplayBase), true) as ScenarioDisplayBase;
            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// ScenariosReorderableListを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenariosReorderableListWrapper">表示するScenarioReorderableListWrapper</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawScenariosReorderableList(Rect lineRect, ScenariosReorderableListWrapper scenariosReorderableListWrapper)
        {
            return scenariosReorderableListWrapper.Draw(lineRect);
        }

        /// <summary>
        /// Toolsを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenariosProp">scenarioの配列を格納したSerializedProperty</param>
        /// <param name="isToolsExpanded">Toolsが展開されているかどうか</param>
        /// <param name="selectMinValue">範囲編集の最小値を格納する変数</param>
        /// <param name="selectMaxValue">範囲編集の最大値を格納する変数</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawTools(Rect lineRect, SerializedProperty scenariosProp, ref bool isToolsExpanded, ref int selectMinValue, ref int selectMaxValue)
        {
            if (!scenariosProp.isExpanded) return lineRect;

            lineRect.xMin += (EditorGUI.indentLevel + 1) * 15;
            {
                isToolsExpanded = EditorGUI.Foldout(lineRect, isToolsExpanded, "Tools", true);
                lineRect = Utilities.LineRectUpdate(lineRect);

                if (!isToolsExpanded) return lineRect;

                lineRect = DrawPreviewScenarioToggle(lineRect);
                lineRect = DrawRangeEditDescription(lineRect);
                lineRect = DrawRangeEditSelector(lineRect, scenariosProp, ref selectMinValue, ref selectMaxValue);
                lineRect = DrawRangeEditButtons(lineRect, scenariosProp, selectMinValue, selectMaxValue);
            }
            lineRect.xMin -= (EditorGUI.indentLevel + 1) * 15;
            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// ScenarioDataをプレビューするかどうかを決めるToggleを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawPreviewScenarioToggle(Rect lineRect)
        {
            bool isPreviewScenario = _isPreviewScenario;
            isPreviewScenario = EditorGUI.Toggle(lineRect, "PreviewScenario", isPreviewScenario);
            if (isPreviewScenario != _isPreviewScenario)
            {
                EditorUserSettings.SetConfigValue("PreviewScenario", isPreviewScenario.ToString());
                _isPreviewScenario = isPreviewScenario;
            }

            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// 範囲編集の説明を表示
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawRangeEditDescription(Rect lineRect)
        {
            EditorGUI.HelpBox(lineRect, "Deletes or Copies the specified area.", MessageType.None);
            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// 範囲編集のセレクターの表示
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenariosProp">scenarioの配列を格納したSerializedProperty</param>
        /// <param name="selectMinValue">範囲編集の最小値を格納する変数</param>
        /// <param name="selectMaxValue">範囲編集の最大値を格納する変数</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawRangeEditSelector(Rect lineRect, SerializedProperty scenariosProp, ref int selectMinValue, ref int selectMaxValue)
        {
            Rect leftRect = new Rect(lineRect) {xMax = lineRect.xMin + 70};
            Rect rightRect = new Rect(lineRect) {xMin = lineRect.xMax - 70};
            Rect centerRect = new Rect(lineRect) {xMin = leftRect.xMax, xMax = rightRect.xMin};

            float min = selectMinValue;
            float max = selectMaxValue;
            if (scenariosProp.arraySize == 0)
            {
                EditorGUI.IntField(leftRect, -1);
                EditorGUI.IntField(rightRect, -1);
                EditorGUI.MinMaxSlider(centerRect, ref min, ref max, 0, 0);
            }
            else
            {
                min = EditorGUI.IntField(leftRect, (int) Mathf.Clamp(min, 0, max));
                max = EditorGUI.IntField(rightRect, (int) Mathf.Clamp(max, min, scenariosProp.arraySize - 1));
                EditorGUI.MinMaxSlider(centerRect, ref min, ref max, 0, scenariosProp.arraySize - 1);
            }

            selectMinValue = (int) min;
            selectMaxValue = (int) max;

            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// 範囲編集のボタンの表示
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenariosProp">scenarioの配列を格納したSerializedProperty</param>
        /// <param name="selectMinValue">範囲編集の最小値を格納する変数</param>
        /// <param name="selectMaxValue">範囲編集の最大値を格納する変数</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawRangeEditButtons(Rect lineRect, SerializedProperty scenariosProp, int selectMinValue, int selectMaxValue)
        {
            if (GUI.Button(Utilities.GetHorizonLayout(lineRect, 0, 1, 1), "Delete"))
            {
                if (selectMinValue <= selectMaxValue && selectMinValue >= 0 && scenariosProp.arraySize > selectMaxValue)
                {
                    for (int i = selectMaxValue; i >= selectMinValue; i--) scenariosProp.DeleteArrayElementAtIndex(i);
                }
            }

            if (GUI.Button(Utilities.GetHorizonLayout(lineRect, 1, 1, 1), "Copy"))
            {
                if (selectMinValue <= selectMaxValue && selectMinValue >= 0 && scenariosProp.arraySize > selectMaxValue)
                {
                    for (int i = selectMinValue; selectMaxValue >= i; i++)
                    {
                        scenariosProp.arraySize++;

                        SerializedWrapper currentScenarioWrapper = new SerializedWrapper(scenariosProp.GetArrayElementAtIndex(i));
                        SerializedWrapper newScenarioWrapper = new SerializedWrapper(scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1));
                        newScenarioWrapper.SetScenarioData(currentScenarioWrapper.GetScenarioData());
                    }
                }
            }

            return Utilities.LineRectUpdate(lineRect);
        }
    }
}
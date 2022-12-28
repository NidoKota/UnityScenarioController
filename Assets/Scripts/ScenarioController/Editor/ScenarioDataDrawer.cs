using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ScenarioController.Editor
{
    /// <summary>
    /// Scenarioの描画
    /// </summary>
    [CustomPropertyDrawer(typeof(ScenarioData))]
    public class ScenarioDataDrawer : PropertyDrawer
    {
        private static readonly GUIContent TextEditorWindowIcon = new GUIContent(EditorGUIUtility.IconContent("winbtn_win_restore_a@2x").image);
        private readonly List<TextEditorWindow> _textEditorWindows = new List<TextEditorWindow>();
        private int _endElementInitializeOnceCount;
        private int _prevIndex = -1;
        private bool _initializeOnce;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 80;

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
        }

        /// <summary>
        /// 1要素につき1回、OnGUIの1フレーム目に呼ばれる
        /// </summary>
        private void ElementInitializeOnce(Rect position, SerializedProperty property, GUIContent label, int index)
        {
            _textEditorWindows.Add(null);
        }

        /// <summary>
        /// OnGUIが違う要素を描画し始めた時、その最初の1フレーム目に呼ばれる
        /// </summary>
        private void ElementInitialize(Rect position, SerializedProperty property, GUIContent label, int index)
        {
        }

        /// <summary>
        /// ここで描画と値の更新を行う
        /// </summary>
        private void Draw(Rect position, SerializedProperty property, GUIContent label, int index)
        {
            position = new Rect(position) {yMin = position.yMin + 4, yMax = position.yMax - 4};

            SerializedWrapper serializedWrapperProp = new SerializedWrapper(property);

            ScenarioData scenario = serializedWrapperProp.GetScenarioData();
            Vector2 textScroll = GetTextScroll(property);
            TextEditorWindow currentTextEditorWindow = _textEditorWindows[index];
            {
                Rect stateDataSpriteRect = new Rect(position) {width = 70, height = 70};
                DrawSprite(stateDataSpriteRect, scenario);

                Rect lineRect = Utilities.LineRectUpdate(new Rect(position) {xMin = stateDataSpriteRect.xMax + 4, yMax = position.yMin});
                lineRect = DrawText(lineRect, ref scenario, ref textScroll, ref currentTextEditorWindow);

                DrawOptions(lineRect, ref scenario);
            }
            _textEditorWindows[index] = currentTextEditorWindow;
            SetTextScroll(property, textScroll);
            serializedWrapperProp.SetScenarioData(scenario);
        }

        /// <summary>
        /// ScrollableTextAreaInternalで使用するVector2の値を取得する
        /// </summary>
        /// <param name="property">ScenarioDataを格納したSerializedProperty</param>
        /// <returns>ScrollableTextAreaInternalで使用するVector2</returns>
        private static Vector2 GetTextScroll(SerializedProperty property)
        {
            SerializedProperty textScrollProp = property.FindPropertyRelative("_textScroll");
            return textScrollProp.vector2Value;
        }

        /// <summary>
        /// ScrollableTextAreaInternalで使用したVector2の値を適応する
        /// </summary>
        /// <param name="property">ScenarioDataを格納したSerializedProperty</param>
        /// <param name="textScroll">ScrollableTextAreaInternalで使用したVector2</param>
        private static void SetTextScroll(SerializedProperty property, Vector2 textScroll)
        {
            SerializedProperty textScrollProp = property.FindPropertyRelative("_textScroll");
            textScrollProp.vector2Value = textScroll;
        }

        /// <summary>
        /// CharacterStateDataに設定された画像を表示する
        /// </summary>
        /// <param name="rect">画像を表示する範囲</param>
        /// <param name="scenario">対象のScenarioデータ</param>
        private static void DrawSprite(Rect rect, ScenarioData scenario)
        {
            CharacterStateData stateData = scenario.StateData;

            Texture2D texture = Texture2D.whiteTexture;
            if (stateData && stateData.StateSprite) texture = AssetPreview.GetAssetPreview(stateData.StateSprite);
            GUIStyle style = new GUIStyle {imagePosition = ImagePosition.ImageOnly, normal = {background = texture}};

            GUI.Label(rect, (string) null, style);
        }

        /// <summary>
        /// ScenarioDataのTextの編集画面を表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenario">対象のScenarioデータ</param>
        /// <param name="scroll">ScrollableTextAreaInternalで使用するVector2</param>
        /// <param name="textEditorWindow">編集ウィンドウのインスタンスを格納する場所</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawText(Rect lineRect, ref ScenarioData scenario, ref Vector2 scroll, ref TextEditorWindow textEditorWindow)
        {
            lineRect = new Rect(lineRect) {yMax = lineRect.yMax + EditorGUIUtility.singleLineHeight};

            Rect leftRect = new Rect(lineRect) {xMax = lineRect.xMin + 40};
            Rect rightRect = new Rect(lineRect) {xMin = leftRect.xMax};

            if (GUI.Button(leftRect, TextEditorWindowIcon)) textEditorWindow = TextEditorWindow.Open(scenario.Text, "ScenarioTextEditor");
            if (textEditorWindow != null) scenario.SetText(textEditorWindow.Text);

            string text = Utilities.ScrollableTextAreaInternal(rightRect, scenario.Text, ref scroll, EditorStyles.textArea);
            scenario.SetText(text);

            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// オプションを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenario">対象のScenarioデータ</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawOptions(Rect lineRect, ref ScenarioData scenario)
        {
            float[] ratios = {0.6f, 0.02f, 0.6f, 0.02f, 1};

            Rect leftRect = Utilities.GetHorizonLayout(lineRect, 0, ratios);
            leftRect = DrawCharacterData(leftRect, ref scenario);
            leftRect = DrawCharacterStateData(leftRect, ref scenario);

            Rect centerRect = Utilities.GetHorizonLayout(lineRect, 2, ratios);
            centerRect = DrawOverrideName(centerRect, ref scenario);
            centerRect = DrawAnimationData(centerRect, ref scenario);

            Rect rightRect = Utilities.GetHorizonLayout(lineRect, 4, ratios);
            rightRect = DrawCharTime(rightRect, ref scenario);
            rightRect = DrawAnimState(rightRect, ref scenario);

            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// AnimationDataを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenario">対象のScenarioデータ</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawAnimationData(Rect lineRect, ref ScenarioData scenario)
        {
            scenario.SetAnimationData(EditorGUI.ObjectField(lineRect, scenario.AnimationData, typeof(TextAnimationData), false) as TextAnimationData);
            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// CharacterDataを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenario">対象のScenarioデータ</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawCharacterData(Rect lineRect, ref ScenarioData scenario)
        {
            CharacterStateData stateData = scenario.StateData;
            CharacterData characterData = stateData ? stateData.ParentAsset : null;

            CharacterData newCharacterData = EditorGUI.ObjectField(lineRect, characterData, typeof(CharacterData), false) as CharacterData;
            if (characterData != newCharacterData) scenario.SetStateData(newCharacterData != null ? newCharacterData.SubAssets[0] : null);

            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// CharacterStateDataを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenario">対象のScenarioデータ</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawCharacterStateData(Rect lineRect, ref ScenarioData scenario)
        {
            CharacterStateData stateData = scenario.StateData;
            CharacterData characterData = stateData ? stateData.ParentAsset : null;

            if (!characterData)
            {
                scenario.SetStateData(null);
                EditorGUI.LabelField(lineRect, "NoData");
                return Utilities.LineRectUpdate(lineRect);
            }

            int stateDataInt = 0;
            if (stateData)
            {
                var contents = characterData.SubAssets
                    .Select((sd, index) => new {statedata = sd, index})
                    .FirstOrDefault(x => x.statedata == stateData);

                stateDataInt = contents?.index ?? 0;
            }

            stateDataInt = EditorGUI.Popup(lineRect, stateDataInt, characterData.SubAssets.Select(x => x.name).ToArray());
            stateData = characterData.SubAssets[stateDataInt];
            scenario.SetStateData(stateData);

            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// CharTimeを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenario">対象のScenarioデータ</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawCharTime(Rect lineRect, ref ScenarioData scenario)
        {
            Rect leftRect = new Rect(lineRect) {xMax = lineRect.xMin + 60};
            Rect rightRect = new Rect(lineRect) {xMin = leftRect.xMax};

            EditorGUI.LabelField(leftRect, "CharTime");
            float charTime = EditorGUI.FloatField(rightRect, scenario.CharTime);

            charTime = Mathf.Clamp(charTime, 0.001f, Mathf.Infinity);

            scenario.SetCharTime(charTime);

            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// AnimStateを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenario">対象のScenarioデータ</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawAnimState(Rect lineRect, ref ScenarioData scenario)
        {
            Rect leftRect = new Rect(lineRect) {xMax = lineRect.xMin + 60};
            Rect rightRect = new Rect(lineRect) {xMin = leftRect.xMax};

            EditorGUI.LabelField(leftRect, "AnimState");
            string animStateName = EditorGUI.TextField(rightRect, scenario.AnimStateName);

            scenario.SetAnimStateName(animStateName);

            return Utilities.LineRectUpdate(lineRect);
        }

        /// <summary>
        /// OverrideNameを表示する
        /// </summary>
        /// <param name="lineRect">最初の1行を表示する範囲</param>
        /// <param name="scenario">対象のScenarioデータ</param>
        /// <returns>次の要素の最初の1行を表示する範囲</returns>
        private static Rect DrawOverrideName(Rect lineRect, ref ScenarioData scenario)
        {
            CharacterStateData stateData = scenario.StateData;
            CharacterData characterData = stateData ? stateData.ParentAsset : null;

            Rect leftRect = new Rect(lineRect) {xMax = lineRect.xMin + 15};
            Rect rightRect = new Rect(lineRect) {xMin = leftRect.xMax};

            bool isOverrideName = scenario.IsOverrideName;
            isOverrideName = EditorGUI.Toggle(leftRect, isOverrideName);

            if (characterData)
            {
                if (isOverrideName) scenario.SetOverrideName(EditorGUI.TextField(rightRect, scenario.OverrideName));
                else
                {
                    scenario.SetDefaultName();
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.TextField(rightRect, characterData.DisplayName);
                }
            }
            else
            {
                scenario.SetDefaultName();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(rightRect, null);
            }

            EditorGUI.EndDisabledGroup();

            return Utilities.LineRectUpdate(lineRect);
        }
    }
}
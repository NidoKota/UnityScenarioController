//変数を使っていない警告を無理やり切る
#pragma warning disable 0414
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using ScenarioController;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace ScenarioController
{
    /// <summary>
    /// ScenarioをEditor上で入力する時に使うClass
    /// </summary>
    [Serializable]
    public class ScenarioInput
    {
        public ScenarioDisplayBase scenarioDisplayBase;

        public List<Scenario> scenarios = new List<Scenario>();

        /// <summary>
        /// 登録されたScenarioDisplayBaseでScenarioの再生を始める(PlayableAssetの場合は使えない)
        /// </summary>
        public void PlayScenario()
        {
            scenarioDisplayBase.PlayScenario(scenarios);
        }

#if UNITY_EDITOR
        //Editor上で使う情報
        //Property全体を表示するか
        [SerializeField] bool display = true;
#endif
    }
}

#if UNITY_EDITOR
namespace ScenarioControllerEditor
{
    /// <summary>
    /// ScenarioInputの描画
    /// </summary>
    [CustomPropertyDrawer(typeof(ScenarioInput))]
    public class ScenarioInputDrawer : PropertyDrawer
    {
        SerializedProperty scenariosProp;
        SerializedProperty displayProp;
        ReorderableList scenariosReorderableList;      //Scenarioを表示するReorderableList
        float propertyHight;                           //現在のPropertyの高さ
        float selectMinValue;                          //削除やコピーを行う範囲の最小値
        float selectMaxValue;                          //削除やコピーを行う範囲の最大値
        bool tools;                                    //ツールを表示しているか
        bool once;                                     //OnGUIで1度だけ実行するためのbool
        bool isPlayableAsset;                          //PlayableAssetかどうか
        static bool scenarioPreview;                   //Scenarioをプレビューするか
        static object nowPreviewScenarioInput;         //現在プレビューされているScenarioInput

        /// <summary>
        /// ScenarioのReorderableListを生成して取得
        /// </summary>
        ReorderableList GetScenariosReorderableList()
        {
            ReorderableList srl = null;
            //ReorderableListで表示するのがscenariosPropertyの時
            srl = new ReorderableList(scenariosProp.serializedObject, scenariosProp);
            //ReorderableListの要素の描画設定
            srl.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                EditorGUI.PropertyField(rect, scenariosProp.GetArrayElementAtIndex(index));
            };
            //新しい要素が追加された時の初期値
            srl.onAddCallback = (list) =>
            {
                scenariosProp.arraySize++;
                scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("data").objectReferenceValue = null;
                scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("stateData").objectReferenceValue = null;
                scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("text").stringValue = null;
                scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("charTime").floatValue = 0.05f;
                scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("isOverrideName").boolValue = false;
                scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("overrideName").stringValue = null;
                scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("animationData").objectReferenceValue = null;
            };
            srl.onSelectCallback = (list) =>
            {
                if (scenarioPreview) nowPreviewScenarioInput = this;
            };
            if (srl != null)
            {
                //ReorderableListのその他の設定
                srl.elementHeight = 80;
                srl.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Scenarios");
            }
            return srl;
        }

        /// <summary>
        /// ScenariosReorderableListの高さを取得
        /// </summary>
        float GetScenariosReorderableListHight(SerializedProperty property)
        {
            float height = 40;
            int arraySize = property.FindPropertyRelative("scenarios").arraySize;

            if (arraySize == 0) height += 80;
            else
            {
                for (int i = 0; i < arraySize; i++)
                {
                    height += 80;
                }
            }
            return height;
        }

        //ScenarioInputのPropertyの長さを渡す
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.FindPropertyRelative("display").boolValue ? GetScenariosReorderableListHight(property) + (tools ? 113 : 53) + (isPlayableAsset ? 0 : 20) : 32;
        }

        //様々なPropertyの描画
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //元は1つのPropertyなのでPropertyScopeで囲む
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                //SerializedProperty格納
                scenariosProp = property.FindPropertyRelative("scenarios");
                displayProp = property.FindPropertyRelative("display");

                //始めに1度だけ実行
                if (!once)
                {
                    isPlayableAsset = property.serializedObject.targetObject is PlayableAsset;

                    try
                    {
                        //Scenarioのプレビュー設定を取得
                        scenarioPreview = bool.Parse(EditorUserSettings.GetConfigValue("scenarioPreview"));
                    }
                    catch { }

                    scenariosReorderableList = GetScenariosReorderableList();

                    once = true;
                }

                position.y += 3;
                position.yMax -= 6;
                propertyHight = position.y + 2;

                //全体を四角で囲む
                GUI.Label(position, GUIContent.none, GUI.skin.box);

                position.xMax -= 5;

                Rect displayRect = new Rect()
                {
                    xMin = position.x,
                    xMax = position.xMax,
                    yMin = propertyHight,
                    height = EditorGUIUtility.singleLineHeight
                };
                //displayFoldout描画
                displayProp.boolValue = EditorGUI.Foldout(displayRect, displayProp.boolValue, property.name, true);
                //描画位置の高さ更新
                propertyHight = displayRect.yMax + 2;

                position.x += 10;
                position.xMax -= 10;

                if (displayProp.boolValue)
                {
                    if (!isPlayableAsset)
                    {
                        Rect scenarioDisplayBaseRect = new Rect()
                        {
                            xMin = position.x,
                            xMax = position.xMax,
                            yMin = propertyHight,
                            height = EditorGUIUtility.singleLineHeight
                        };
                        EditorGUI.PropertyField(scenarioDisplayBaseRect, property.FindPropertyRelative("scenarioDisplayBase"));
                        propertyHight = scenarioDisplayBaseRect.yMax + 2;
                    }

                    Rect scenariosReorderableListRect = new Rect()
                    {
                        xMin = position.x,
                        xMax = position.xMax,
                        yMin = propertyHight,
                        height = GetScenariosReorderableListHight(property)
                    };
                    Rect scenarioFileRect = new Rect()
                    {
                        xMin = position.x,
                        xMax = position.xMax,
                        yMin = propertyHight,
                        height = EditorGUIUtility.singleLineHeight
                    };

                    //scenariosReorderableList描画
                    scenariosReorderableList.DoList(scenariosReorderableListRect);
                    propertyHight += GetScenariosReorderableListHight(property) + 2;

                    //tools描画
                    Rect toolsRect = new Rect()
                    {
                        xMin = position.x + 10,
                        xMax = position.xMax,
                        yMin = propertyHight,
                        height = EditorGUIUtility.singleLineHeight
                    };
                    tools = EditorGUI.Foldout(toolsRect, tools, "Tools", true);
                    propertyHight = toolsRect.yMax + 2;

                    //ツールを表示している時
                    if (tools)
                    {
                        //scenarioPreview描画
                        Rect scenarioPreviewRect = new Rect()
                        {
                            xMin = toolsRect.xMin,
                            xMax = position.xMax,
                            yMin = propertyHight,
                            height = EditorGUIUtility.singleLineHeight
                        };
                        bool scenarioPreviewChecker = scenarioPreview;
                        scenarioPreview = EditorGUI.Toggle(scenarioPreviewRect, "ScenarioPreview", scenarioPreview);
                        //scenarioPreviewの値が変更されたら値を保存する
                        if (scenarioPreview != scenarioPreviewChecker)
                        {
                            EditorUserSettings.SetConfigValue("scenarioPreview", scenarioPreview.ToString());
                            nowPreviewScenarioInput = null;
                        }
                        propertyHight = scenarioPreviewRect.yMax + 2;

                        //helpBox描画
                        Rect helpBoxRect = new Rect()
                        {
                            xMin = toolsRect.xMin,
                            xMax = position.xMax,
                            yMin = propertyHight,
                            height = EditorGUIUtility.singleLineHeight
                        };
                        EditorGUI.HelpBox(helpBoxRect, "指定した範囲を一括削除またはコピーします。", MessageType.None);
                        propertyHight = helpBoxRect.yMax + 2;

                        Rect minValueRect = new Rect()
                        {
                            xMin = toolsRect.xMin,
                            width = helpBoxRect.width * 1 / 10,
                            yMin = propertyHight,
                            height = EditorGUIUtility.singleLineHeight
                        };
                        Rect selectPointRect = new Rect()
                        {
                            xMin = minValueRect.xMax + 2,
                            width = helpBoxRect.width * 4 / 10,
                            yMin = propertyHight,
                            height = EditorGUIUtility.singleLineHeight
                        };
                        Rect maxValueRect = new Rect()
                        {
                            xMin = selectPointRect.xMax + 2,
                            width = helpBoxRect.width * 1 / 10,
                            yMin = propertyHight,
                            height = EditorGUIUtility.singleLineHeight
                        };
                        Rect deleteButtonRect = new Rect()
                        {
                            xMin = maxValueRect.xMax + 2,
                            width = helpBoxRect.width * 2 / 10,
                            yMin = propertyHight,
                            height = EditorGUIUtility.singleLineHeight
                        };
                        Rect copyButtonRect = new Rect()
                        {
                            xMin = deleteButtonRect.xMax + 2,
                            xMax = position.xMax,
                            yMin = propertyHight,
                            height = EditorGUIUtility.singleLineHeight
                        };
                        propertyHight = selectPointRect.yMax;

                        //selectPoint描画
                        if (scenariosProp.arraySize == 0)
                        {
                            EditorGUI.IntField(minValueRect, -1);
                            EditorGUI.IntField(maxValueRect, -1);
                            EditorGUI.MinMaxSlider(selectPointRect, ref selectMinValue, ref selectMaxValue, 0, 0);
                        }
                        else
                        {
                            selectMinValue = EditorGUI.IntField(minValueRect, Mathf.Clamp((int)selectMinValue, 0, (int)selectMaxValue));
                            selectMaxValue = EditorGUI.IntField(maxValueRect, Mathf.Clamp((int)selectMaxValue, (int)selectMinValue, scenariosProp.arraySize - 1));
                            EditorGUI.MinMaxSlider(selectPointRect, ref selectMinValue, ref selectMaxValue, 0, scenariosProp.arraySize - 1);
                        }
                        if (selectMinValue % 1 != 0) selectMinValue = (int)selectMinValue;
                        if (selectMaxValue % 1 != 0) selectMaxValue = (int)selectMaxValue;

                        //deleteButton描画
                        if (GUI.Button(deleteButtonRect, "Delete"))
                        {
                            if (selectMinValue <= selectMaxValue && selectMinValue >= 0 && scenariosProp.arraySize > selectMaxValue)
                            {
                                //selectPointで指定した個所を削除する
                                for (int i = (int)selectMaxValue; i >= selectMinValue; i--) scenariosProp.DeleteArrayElementAtIndex(i);
                            }
                        }
                        //CopyButton描画
                        if (GUI.Button(copyButtonRect, "Copy"))
                        {
                            if (selectMinValue <= selectMaxValue && selectMinValue >= 0 && scenariosProp.arraySize > selectMaxValue)
                            {
                                //selectPointで指定した個所をコピーする
                                for (int i = (int)selectMinValue; selectMaxValue >= i; i++)
                                {
                                    scenariosProp.arraySize++;
                                    scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("data").objectReferenceValue = scenariosProp.GetArrayElementAtIndex(i).FindPropertyRelative("data").objectReferenceValue;
                                    scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("stateData").objectReferenceValue = scenariosProp.GetArrayElementAtIndex(i).FindPropertyRelative("stateData").objectReferenceValue;
                                    scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("text").stringValue = scenariosProp.GetArrayElementAtIndex(i).FindPropertyRelative("text").stringValue;
                                    scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("charTime").floatValue = scenariosProp.GetArrayElementAtIndex(i).FindPropertyRelative("charTime").floatValue;
                                    scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("isOverrideName").boolValue = scenariosProp.GetArrayElementAtIndex(i).FindPropertyRelative("isOverrideName").boolValue;
                                    scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("overrideName").stringValue = scenariosProp.GetArrayElementAtIndex(i).FindPropertyRelative("overrideName").stringValue;
                                    scenariosProp.GetArrayElementAtIndex(scenariosProp.arraySize - 1).FindPropertyRelative("animationData").objectReferenceValue = scenariosProp.GetArrayElementAtIndex(i).FindPropertyRelative("animationData").objectReferenceValue;
                                }
                            }
                        }
                    }
                    //自身がプレビュー対象かつ、範囲外をクリックされた時、プレビューをやめる
                    if (nowPreviewScenarioInput == this && !scenariosReorderableListRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 0) ScenarioPreviewOut();

                    //自身がプレビュー対象かつ、scenarioPreviewがtrueの時、プレビューをする
                    if (nowPreviewScenarioInput == this && scenarioPreview) ScenarioPreview();
                }
                //displayがfalseの時プレビューをやめる
                else if (nowPreviewScenarioInput == this) ScenarioPreviewOut();

                if (EditorApplication.isPlayingOrWillChangePlaymode || !scenarioPreview) ScenarioPreviewOut();


                /// <summary>
                /// 選択されたScenarioをプレビューする
                /// </summary>
                void ScenarioPreview()
                {
                    if (scenariosReorderableList.index >= 0 && scenariosReorderableList.index < scenariosProp.arraySize && !EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        try
                        {
                            ScenarioDisplayBase scenarioDisplayBase = default;
                            if (isPlayableAsset) scenarioDisplayBase = (ScenarioDisplayBase)property.serializedObject.FindProperty("scenarioDisplayBase").objectReferenceValue;
                            else scenarioDisplayBase = (ScenarioDisplayBase)property.FindPropertyRelative("scenarioDisplayBase").objectReferenceValue;

                            scenarioDisplayBase.GetComponent<CanvasGroup>().alpha = 1;
                            scenarioDisplayBase.tmp.text = scenariosProp.GetArrayElementAtIndex(scenariosReorderableList.index).FindPropertyRelative("text").stringValue;
                            //GameViewとSceneViewを更新
                            EditorApplication.QueuePlayerLoopUpdate();
                        }
                        catch { }
                    }
                }

                /// <summary>
                /// Scenarioをプレビューをやめる
                /// </summary>
                void ScenarioPreviewOut()
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        nowPreviewScenarioInput = null;
                        try
                        {
                            ScenarioDisplayBase scenarioDisplayBase = default;
                            if (isPlayableAsset) scenarioDisplayBase = (ScenarioDisplayBase)property.serializedObject.FindProperty("scenarioDisplayBase").objectReferenceValue;
                            else scenarioDisplayBase = (ScenarioDisplayBase)property.FindPropertyRelative("scenarioDisplayBase").objectReferenceValue;

                            scenarioDisplayBase.GetComponent<CanvasGroup>().alpha = 0;
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
#endif
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScenarioController.Editor
{
    /// <summary>
    /// Scenarioの描画
    /// </summary>
    [CustomPropertyDrawer(typeof(Scenario))]
    public class ScenarioDrawer : PropertyDrawer
    {
        GUIStyle stateDataSpriteStyle;

        ScenarioCharacterData[] datas;

        int dataInt;
        int stateDataInt;

        bool once;

        //Propertyの高さ
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 80;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //元は1つのPropertyなのでPropertyScopeで囲む
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                position.yMin += 3;
                //Rectの計算
                Rect stateDataSpritRect = new Rect()
                {
                    xMin = position.x,
                    width = 73,
                    yMin = position.y,
                    height = 73
                };
                Rect scenarioTextRect = new Rect()
                {
                    xMin = stateDataSpritRect.xMax + 2,
                    xMax = position.xMax,
                    yMin = position.y,
                    height = EditorGUIUtility.singleLineHeight * 3,
                };
                Rect charTimeRect = new Rect()
                {
                    xMin = position.xMax - 32,
                    xMax = scenarioTextRect.xMax,
                    yMin = scenarioTextRect.yMax + 2,
                    height = EditorGUIUtility.singleLineHeight,
                };
                Rect charTimeLabelRect = new Rect()
                {
                    xMin = charTimeRect.xMin - 17,
                    xMax = charTimeRect.xMin,
                    yMin = scenarioTextRect.yMax + 2,
                    height = EditorGUIUtility.singleLineHeight,
                };
                Rect overrideNameRect = new Rect()
                {
                    xMin = charTimeLabelRect.xMin - 50,
                    xMax = charTimeLabelRect.xMin - 2,
                    yMin = scenarioTextRect.yMax + 2,
                    height = EditorGUIUtility.singleLineHeight,
                };
                Rect isOverrideNameBoolRect = new Rect()
                {
                    xMin = overrideNameRect.xMin - 15,
                    xMax = overrideNameRect.xMin - 2,
                    yMin = scenarioTextRect.yMax + 2,
                    height = EditorGUIUtility.singleLineHeight,
                };
                Rect animationDataRect = new Rect()
                {
                    xMin = isOverrideNameBoolRect.xMin - 50,
                    xMax = isOverrideNameBoolRect.xMin - 2,
                    yMin = scenarioTextRect.yMax + 2,
                    height = EditorGUIUtility.singleLineHeight,
                };
                Rect dataRect = new Rect()
                {
                    xMin = stateDataSpritRect.xMax + 2,
                    width = 50,
                    yMin = scenarioTextRect.yMax + 2,
                    height = EditorGUIUtility.singleLineHeight,
                };
                Rect stateDataRect = new Rect()
                {
                    xMin = dataRect.xMax + 2,
                    width = 50,
                    yMin = scenarioTextRect.yMax + 2,
                    height = EditorGUIUtility.singleLineHeight,
                };
                Rect textEditorWindowButtonRect = new Rect()
                {
                    xMin = stateDataRect.xMax + 2,
                    width = 20,
                    yMin = scenarioTextRect.yMax + 2,
                    height = EditorGUIUtility.singleLineHeight,
                };

                //SerializedProperty格納
                SerializedProperty stateDataProp = property.FindPropertyRelative("stateData");
                SerializedProperty textProp = property.FindPropertyRelative("text");
                SerializedProperty charTimeProp = property.FindPropertyRelative("charTime");
                SerializedProperty overrideNameProp = property.FindPropertyRelative("overrideName");
                SerializedProperty isOverrideNameProp = property.FindPropertyRelative("isOverrideName");
                SerializedProperty dataProp = property.FindPropertyRelative("data");
                SerializedProperty animationDataProp = property.FindPropertyRelative("animationData");
                SerializedProperty scroll = property.FindPropertyRelative("scroll");

                //ずらさないようにする
                EditorGUI.indentLevel = 0;

                //Textを描画
                Vector2 v = scroll.vector2Value;
                textProp.stringValue = ScenarioControllerUtility.ScrollableTextAreaInternal(scenarioTextRect, textProp.stringValue, ref v, EditorStyles.textArea);
                scroll.vector2Value = v;

                //CharTimeの描画
                EditorGUI.LabelField(charTimeLabelRect, "CT");
                charTimeProp.floatValue = Mathf.Clamp(EditorGUI.FloatField(charTimeRect, Mathf.Clamp(charTimeProp.floatValue, 0, Mathf.Infinity)), 0.001f, Mathf.Infinity);

                //Nameの描画
                isOverrideNameProp.boolValue = EditorGUI.Toggle(isOverrideNameBoolRect, isOverrideNameProp.boolValue);

                animationDataProp.objectReferenceValue = EditorGUI.ObjectField(animationDataRect, animationDataProp.objectReferenceValue, typeof(ScenarioTMPAnimationData), false);

                if (!once)
                {
                    //共有データなので1度だけの処理でも可
                    datas = AssetDatabase.FindAssets("t:ScenarioCharacterData").Select(x => AssetDatabase.LoadAssetAtPath<ScenarioCharacterData>(AssetDatabase.GUIDToAssetPath(x))).ToArray();
                    once = true;
                }

                //アセットにDataがある時
                if (datas.Length > 0)
                {
                    //dataがあればdatasの中のdataのindexを調べる(dataが無かったら0を入れる)
                    dataInt = dataProp.objectReferenceValue ? datas.Select((data, index) => (data, index)).FirstOrDefault(con => con.data == dataProp.objectReferenceValue).index : 0;

                    int dataIntChecker = dataInt;

                    //ScenarioCharacterDataの描画
                    dataIntChecker = EditorGUI.Popup(dataRect, dataIntChecker, datas.Select(x => x.name).ToArray());

                    //別のDataに変えた時
                    if (dataInt != dataIntChecker)
                    {
                        //Dataを変えたらStateDataは一番最初にする
                        stateDataInt = 0;
                        dataInt = dataIntChecker;
                    }
                    //dataを保存
                    dataProp.objectReferenceValue = datas[dataInt];
                }
                else
                {
                    dataProp.objectReferenceValue = null;
                    EditorGUI.LabelField(dataRect, "NoData");
                }

                //dataがある時
                if (dataProp.objectReferenceValue)
                {
                    //dataのSubAssetsにStateDataがある時
                    if (((ScenarioCharacterData)dataProp.objectReferenceValue).SubAssets.Length > 0)
                    {
                        //stateDataがあればdata.SubAssetsの中のstateDataのindexを調べる
                        if (stateDataProp.objectReferenceValue)
                        {
                            var contents = ((ScenarioCharacterData)dataProp.objectReferenceValue).SubAssets.Select((statedata, index) => new { statedata, index }).FirstOrDefault(con => con.statedata == stateDataProp.objectReferenceValue);
                            stateDataInt = contents == null ? 0 : contents.index;
                        }
                        else stateDataInt = 0;

                        //ScenarioCharacterStateDataの描画
                        stateDataInt = EditorGUI.Popup(stateDataRect, stateDataInt, ((ScenarioCharacterData)dataProp.objectReferenceValue).SubAssets.Select(x => x.name).ToArray());

                        //stateDataを保存
                        stateDataProp.objectReferenceValue = datas[dataInt].SubAssets[stateDataInt];
                    }
                }

                //dataがないまたは、dataのSubAssetsにStateDataがない時
                if (!dataProp.objectReferenceValue || ((ScenarioCharacterData)dataProp.objectReferenceValue).SubAssets.Length == 0)
                {
                    stateDataProp.objectReferenceValue = null;
                    EditorGUI.LabelField(stateDataRect, "NoData");
                }

                //nameをデフォルトの名前にする処理
                if (dataProp.objectReferenceValue)
                {
                    if (isOverrideNameProp.boolValue) overrideNameProp.stringValue = EditorGUI.TextField(overrideNameRect, overrideNameProp.stringValue);
                    else
                    {
                        //編集不可にする
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.TextField(overrideNameRect, dataProp.objectReferenceValue.name);
                    }
                }
                else
                {
                    //編集不可にする
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.TextField(overrideNameRect, null);
                }
                //編集可能にする
                EditorGUI.EndDisabledGroup();

                //指定されたStateDataのSpriteの描画
                Sprite sp = default;
                if (dataProp.objectReferenceValue && stateDataProp.objectReferenceValue) sp = ((ScenarioCharacterStateData)stateDataProp.objectReferenceValue).stateSprite;
                if (stateDataSpriteStyle == null)
                {
                    stateDataSpriteStyle = new GUIStyle();
                    stateDataSpriteStyle.imagePosition = ImagePosition.ImageOnly;
                }
                //Spriteがなければ白にする
                if (sp == null) stateDataSpriteStyle.normal.background = Texture2D.whiteTexture;
                //Textureの取得
                else stateDataSpriteStyle.normal.background = AssetPreview.GetAssetPreview(sp);
                //Spriteの描画
                GUI.Label(stateDataSpritRect, "", stateDataSpriteStyle);

                if (GUI.Button(textEditorWindowButtonRect, "↑")) ScenarioTextEditorWindow.Open(property);
            }
        }
    }
}

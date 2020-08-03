using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScenarioController.Editor
{
    [CustomEditor(typeof(ScenarioCharacterData))]
    public class ScenarioCharacterDataEditor : UnityEditor.Editor
    {
        SerializedProperty _SubAssetsProp;

        void OnEnable()
        {
            _SubAssetsProp = serializedObject.FindProperty("_SubAssets");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //SubAssetを取得
            ScenarioCharacterStateData[] subAssets = GetSubAssets();

            //SubAssetの数を表示
            EditorGUILayout.LabelField($"SubAsset ({subAssets.Count()})");

            //SubAssetを作成
            if (GUILayout.Button("CreateNewScenarioCharacterStateData"))
            {
                ScenarioCharacterStateData stateData = CreateInstance<ScenarioCharacterStateData>();
                AssetDatabase.AddObjectToAsset(stateData, target);

                //アセットを再読み込み
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stateData));
                ScenarioControllerUtility.ProjectWindowReset();
                AssetDatabase.SaveAssets();

                //ParentAssetのコピーを保存
                SerializedObject stateDataObject = new SerializedObject(stateData);
                stateDataObject.Update();
                stateDataObject.FindProperty("_ParentAsset").objectReferenceValue = (ScenarioCharacterData)target;
                stateDataObject.ApplyModifiedProperties();

                //SubAssetのコピーを保存
                int createIndex = _SubAssetsProp.arraySize;
                _SubAssetsProp.InsertArrayElementAtIndex(createIndex);
                _SubAssetsProp.GetArrayElementAtIndex(createIndex).objectReferenceValue = stateData;
            }

            //SubAssetの再確認
            if (GUILayout.Button("ReinportSubAssets"))
            {
                subAssets = GetSubAssets();
                _SubAssetsProp.ClearArray();
                for (int i = 0; i < subAssets.Length; i++)
                {
                    _SubAssetsProp.InsertArrayElementAtIndex(i);
                    _SubAssetsProp.GetArrayElementAtIndex(i).objectReferenceValue = subAssets[i];
                }
            }

            ScenarioCharacterStateData[] GetSubAssets()
            {
                return AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target)).Where(x => AssetDatabase.IsSubAsset(x)).Select(x => (ScenarioCharacterStateData)x).ToArray();
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }

    }
}
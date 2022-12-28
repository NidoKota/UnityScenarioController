using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScenarioController.Editor
{
    /// <summary>
    /// CharacterDataの拡張
    /// </summary>
    [CustomEditor(typeof(CharacterData))]
    public class CharacterDataEditor : UnityEditor.Editor
    {
        private CharacterData _characterData;
        private CharacterStateData[] _subAssets;

        private void OnEnable()
        {
            _characterData = target as CharacterData;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                //SubAssetを取得
                _subAssets = CharacterDataUtilities.GetSubAssets(_characterData);

                //SubAssetの数を表示
                EditorGUILayout.LabelField($"SubAsset ({_subAssets.Length})");

                using (new EditorGUILayout.HorizontalScope())
                {
                    //SubAssetを作成
                    if (GUILayout.Button("CreateStateData")) CharacterDataUtilities.CreateSubAsset(_characterData);
                    //SubAssetの再確認
                    if (GUILayout.Button("ReimportSubAssets")) CharacterDataUtilities.ReimportSubAssets(_characterData);
                }

                GUILayout.Space(EditorGUIUtility.singleLineHeight);
                DrawPropertiesExcluding(serializedObject, "m_Script");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
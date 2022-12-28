using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UObject = UnityEngine.Object;

namespace ScenarioController.Editor
{
    /// <summary>
    /// CharacterStateDataの拡張
    /// </summary>
    [CustomEditor(typeof(CharacterStateData))]
    public class CharacterStateDataEditor : UnityEditor.Editor
    {
        private SerializedProperty _stateSpriteProp;
        private SerializedProperty _parentAssetProp;
        private SerializedObject _parentSerializedObject;
        private Sprite _editedStateSprite;
        private string _editedAssetName;
        private bool _isDeleteButtonClicked;

        void OnEnable()
        {
            _stateSpriteProp = serializedObject.FindProperty("<StateSprite>k__BackingField");
            _parentAssetProp = serializedObject.FindProperty("<ParentAsset>k__BackingField");

            _parentSerializedObject = new SerializedObject(_parentAssetProp.objectReferenceValue);

            _editedStateSprite = _stateSpriteProp.objectReferenceValue as Sprite;
            _editedAssetName = target.name;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CharacterData characterData = _parentAssetProp.objectReferenceValue as CharacterData;
            {
                DrawName(ref _editedAssetName);
                DrawParentAssetPreview(characterData);
                DrawStateSprite(ref _editedStateSprite);
                DrawSpace();

                if (IsSameName(characterData, _editedAssetName)) DrawSameNameError();
                else if (string.IsNullOrEmpty(_editedAssetName)) DrawNullNameError();
                else if (ChangeCheck(_editedAssetName, _editedStateSprite) && DrawApplyButton()) Apply(_editedAssetName, _editedStateSprite);

                _isDeleteButtonClicked = DrawDeleteButton(_parentSerializedObject);
                DrawSpace();
                DrawAddedProperties();
            }
            serializedObject.ApplyModifiedProperties();

            // すべての変更を適応した後で削除処理する
            if (_isDeleteButtonClicked) Delete();
        }

        private bool ChangeCheck(string newAssetName, Sprite newStateSprite)
        {
            return target.name != newAssetName | _stateSpriteProp.objectReferenceValue != newStateSprite;
        }

        private void Apply(string newAssetName, Sprite newStateSprite)
        {
            target.name = newAssetName;
            _stateSpriteProp.objectReferenceValue = newStateSprite;

            //アセットをセーブして再読み込み
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
            Utilities.ProjectWindowReset();
        }

        private void Delete()
        {
            bool isLast;

            //Parentに保存されているコピーも削除する
            _parentSerializedObject.Update();
            {
                SerializedProperty parentSubAssetsProp = _parentSerializedObject.FindProperty("<SubAssets>k__BackingField");
                isLast = parentSubAssetsProp.arraySize <= 1;
                for (int i = 0; i < parentSubAssetsProp.arraySize; i++)
                {
                    if (parentSubAssetsProp.GetArrayElementAtIndex(i).objectReferenceValue == target)
                    {
                        parentSubAssetsProp.GetArrayElementAtIndex(i).objectReferenceValue = null;
                        parentSubAssetsProp.DeleteArrayElementAtIndex(i);
                    }
                }
            }
            _parentSerializedObject.ApplyModifiedProperties();

            //Assetを削除して再読み込み
            DestroyImmediate(target, true);
            AssetDatabase.SaveAssets();
            if (!isLast) AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_parentSerializedObject.targetObject));
            Utilities.ProjectWindowReset();
        }

        private static void DrawName(ref string assetName)
        {
            assetName = EditorGUILayout.TextField("Name", assetName);
        }

        private static void DrawParentAssetPreview(CharacterData parentAsset)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("ParentAsset");
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(parentAsset, typeof(CharacterData), false);
                EditorGUI.EndDisabledGroup();
            }
        }

        private static void DrawStateSprite(ref Sprite stateSprite)
        {
            string spriteName = stateSprite ? stateSprite.name : "Null";
            stateSprite = EditorGUILayout.ObjectField($"StateSprite : {spriteName}", stateSprite, typeof(Sprite), false, GUILayout.MinHeight(75), GUILayout.MinWidth(75)) as Sprite;
        }

        private static void DrawSpace()
        {
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
        }

        private static bool DrawDeleteButton(SerializedObject parentSerializedObject)
        {
            parentSerializedObject.Update();
            SerializedProperty parentSubAssetsProp = parentSerializedObject.FindProperty("<SubAssets>k__BackingField");
            bool isLast = parentSubAssetsProp.arraySize <= 1;

            return !isLast && GUILayout.Button("Delete");
        }

        private static bool IsSameName(CharacterData parentAsset, string newName)
        {
            return parentAsset.name == newName;
        }

        private static void DrawSameNameError()
        {
            EditorGUILayout.HelpBox("Don't use ParentAsset name", MessageType.Error);
        }

        private static void DrawNullNameError()
        {
            EditorGUILayout.HelpBox("Don't use Null name", MessageType.Error);
        }

        private static bool DrawApplyButton()
        {
            return GUILayout.Button("Apply");
        }

        private void DrawAddedProperties()
        {
            DrawPropertiesExcluding(serializedObject, "m_Script");
        }
    }
}
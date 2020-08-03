using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScenarioController.Editor
{
    [CustomEditor(typeof(ScenarioCharacterStateData))]
    public class ScenarioCharacterStateDataEditor : UnityEditor.Editor
    {
        SerializedProperty stateSpriteProp;
        SerializedProperty _ParentAssetProp;

        SerializedObject parentObject;
        SerializedProperty parent_SubAssetsProp;

        UnityEngine.Object stateSpriteChecker;

        string nameChecker;
        bool delete;
        bool edit;

        void OnEnable()
        {
            stateSpriteProp = serializedObject.FindProperty("stateSprite");
            _ParentAssetProp = serializedObject.FindProperty("_ParentAsset");

            nameChecker = target.name;

            stateSpriteChecker = stateSpriteProp.objectReferenceValue;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //Nameの描画
            nameChecker = EditorGUILayout.TextField("Name", nameChecker);

            //ParentAssetの描画
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("ParentAsset");
                //編集不可にする
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(_ParentAssetProp.objectReferenceValue, typeof(ScenarioCharacterData), false);
                EditorGUI.EndDisabledGroup();
            }

            //StateSpriteの描画
            string name = stateSpriteChecker ? stateSpriteChecker.name : "Null";
            stateSpriteChecker = EditorGUILayout.ObjectField($"StateSprite : {name}", stateSpriteChecker, typeof(Sprite), false, GUILayout.MinHeight(75), GUILayout.MinWidth(75));

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            //変更があったか調べる
            if (target.name != nameChecker) edit = true;
            if (stateSpriteProp.objectReferenceValue != stateSpriteChecker) edit = true;

            //同じ名前だとエラー
            if (_ParentAssetProp.objectReferenceValue.name == nameChecker)
            {
                EditorGUILayout.HelpBox("Don't use ParentAsset name!", MessageType.Error);
                edit = false;
            }

            //ApplyButtonの描画
            if (edit)
            {
                if (GUILayout.Button("Apply"))
                {
                    target.name = nameChecker;
                    stateSpriteProp.objectReferenceValue = stateSpriteChecker;

                    //アセットをセーブして再読み込み
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();

                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                    ScenarioControllerUtility.ProjectWindowReset();
                    edit = false;
                }
            }
            else GUILayout.Space(EditorGUIUtility.singleLineHeight);

            //DeleteButtonの描画
            if (GUILayout.Button("Delete")) delete = true;

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();

            //エラーが出るので最後に削除処理する
            if (delete)
            {
                //Parentに保存されているコピーも削除する
                parentObject = new SerializedObject(_ParentAssetProp.objectReferenceValue);
                parentObject.Update();
                parent_SubAssetsProp = parentObject.FindProperty("_SubAssets");
                for (int i = 0; i < parent_SubAssetsProp.arraySize; i++)
                {
                    if (parent_SubAssetsProp.GetArrayElementAtIndex(i).objectReferenceValue == target)
                    {
                        parent_SubAssetsProp.GetArrayElementAtIndex(i).objectReferenceValue = null;
                        parent_SubAssetsProp.DeleteArrayElementAtIndex(i);
                    }
                }
                parentObject.ApplyModifiedProperties();

                //Assetを削除して再読み込み
                string path = AssetDatabase.GetAssetPath(target);
                DestroyImmediate(target, true);
                AssetDatabase.ImportAsset(path);
                ScenarioControllerUtility.ProjectWindowReset();
            }
        }
    }
}
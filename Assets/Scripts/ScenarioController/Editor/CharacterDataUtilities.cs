using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ScenarioController.Editor
{
    /// <summary>
    /// CharacterDataに関する便利関数群
    /// </summary>
    public static class CharacterDataUtilities
    {
        /// <summary>
        /// CharacterDataに含まれるCharacterStateDataを取得する
        /// </summary>
        public static CharacterStateData[] GetSubAssets(CharacterData characterData)
        {
            AssetDatabase.SaveAssets();

            return AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(characterData))
                .Where(AssetDatabase.IsSubAsset)
                .Select(x => (CharacterStateData) x)
                .ToArray();
        }

        /// <summary>
        /// CharacterDataに新しいCharacterStateDataを追加する
        /// </summary>
        public static void CreateSubAsset(CharacterData characterData)
        {
            SerializedObject serializedObject = new SerializedObject(characterData);
            {
                SerializedProperty subAssetsProp = serializedObject.FindProperty("<SubAssets>k__BackingField");

                int createIndex = subAssetsProp.arraySize;

                CharacterStateData stateData = ScriptableObject.CreateInstance<CharacterStateData>();
                stateData.name = createIndex.ToString();
                AssetDatabase.AddObjectToAsset(stateData, characterData);

                //先にセーブする必要がある
                AssetDatabase.SaveAssets();

                //アセットを再読み込み
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stateData));
                Utilities.ProjectWindowReset();
                AssetDatabase.SaveAssets();

                //ParentAssetを保存
                SerializedObject stateDataObject = new SerializedObject(stateData);
                stateDataObject.Update();
                stateDataObject.FindProperty("<ParentAsset>k__BackingField").objectReferenceValue = characterData;
                stateDataObject.ApplyModifiedProperties();

                //SubAssetを保存
                subAssetsProp.InsertArrayElementAtIndex(createIndex);
                subAssetsProp.GetArrayElementAtIndex(createIndex).objectReferenceValue = stateData;
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// <para>CharacterDataとCharacterStateDataの紐づけを再度行う</para>
        /// <para>CharacterStateDataが0個の時は1つ追加する</para>
        /// </summary>
        public static void ReimportSubAssets(CharacterData characterData)
        {
            SerializedObject serializedObject = new SerializedObject(characterData);
            {
                CharacterStateData[] subAssets = GetSubAssets(characterData);

                if (subAssets.Length == 0)
                {
                    CreateSubAsset(characterData);
                    subAssets = GetSubAssets(characterData);
                }

                SerializedProperty subAssetsProp = serializedObject.FindProperty("<SubAssets>k__BackingField");
                subAssetsProp.ClearArray();

                for (int i = 0; i < subAssets.Length; i++)
                {
                    subAssetsProp.InsertArrayElementAtIndex(i);
                    subAssetsProp.GetArrayElementAtIndex(i).objectReferenceValue = subAssets[i];
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
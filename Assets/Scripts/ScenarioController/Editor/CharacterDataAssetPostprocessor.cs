using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScenarioController.Editor
{
    /// <summary>
    /// CharacterStateDataを1つも持たないCharacterDataを作らせないようにする
    /// </summary>
    public class CharacterDataAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                CharacterData characterData = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
                if (!characterData) return;

                if (characterData.SubAssets == null || characterData.SubAssets.Length == 0) CharacterDataUtilities.ReimportSubAssets(characterData);
            }
        }
    }
}
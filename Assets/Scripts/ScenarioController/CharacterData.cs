using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScenarioController
{
    /// <summary>
    /// Scenarioで指定するCharacterの情報
    /// </summary>
    [CreateAssetMenu(menuName = "CharacterData", fileName = "CharacterData", order = 1000)]
    public class CharacterData : ScriptableObject
    {
        /// <summary>
        /// ScenarioCharacterStateDataの参照
        /// </summary>
        [field: SerializeField, HideInInspector]
        public CharacterStateData[] SubAssets { get; private set; }

        //↓自由にデータを追加可能

        [field: SerializeField] public string DisplayName { get; private set; }
    }
}
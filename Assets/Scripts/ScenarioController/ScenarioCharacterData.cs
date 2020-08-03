using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScenarioController
{
    /// <summary>
    /// Scenarioで指定するCharacterの情報
    /// </summary>
    [CreateAssetMenu(menuName = "ScenarioCharacterData", fileName = "ScenarioCharacterData", order = 1000)]
    public class ScenarioCharacterData : ScriptableObject
    {
        /// <summary>
        /// ScenarioCharacterStateDataの参照
        /// </summary>
        public ScenarioCharacterStateData[] SubAssets { get { return _SubAssets; } }
        [SerializeField, HideInInspector] ScenarioCharacterStateData[] _SubAssets = new ScenarioCharacterStateData[0];

        //↓自由にデータを追加可能
    }
}
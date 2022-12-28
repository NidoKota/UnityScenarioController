using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ScenarioController;

namespace ScenarioController
{
    /// <summary>
    /// Scenarioで指定するCharacterのStateの情報
    /// </summary>
    public class CharacterStateData : ScriptableObject
    {
        /// <summary>
        /// ScenarioCharacterDataの参照
        /// </summary>
        [field: SerializeField, HideInInspector]
        public CharacterData ParentAsset { get; private set; }

        [field: SerializeField, HideInInspector]
        public Sprite StateSprite { get; private set; }

        //↓自由にデータを追加可能
    }
}
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ScenarioController;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace ScenarioController
{
    /// <summary>
    /// Scenarioで指定するCharacterのStateの情報
    /// </summary>
    public class ScenarioCharacterStateData : ScriptableObject
    {
        /// <summary>
        /// ScenarioCharacterDataの参照
        /// </summary>
        public ScenarioCharacterData ParentAsset { get { return _ParentAsset; } }
        [SerializeField, HideInInspector] ScenarioCharacterData _ParentAsset = default;
        [HideInInspector] public Sprite stateSprite;

        //↓自由にデータを追加可能
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScenarioController
{
    /// <summary>
    /// 誰が何を話すのかといった情報
    /// </summary>
    [Serializable]
    public struct Scenario
    {
        /// <summary>
        /// 指定したStateの情報
        /// </summary>
        public ScenarioCharacterStateData stateData;

        /// <summary>
        /// 台詞
        /// </summary>
        public string text;

        /// <summary>
        /// 1文字を表示する時間
        /// </summary>
        public float charTime;

        /// <summary>
        /// 任意でScenarioに紐づけられる文字のアニメーションのデータ
        /// </summary>
        public ScenarioTMPAnimationData animationData;

        /// <summary>
        /// 名前を上書きしているか
        /// </summary>
        public bool isOverrideName;

        [SerializeField] string overrideName;

        /// <summary>
        /// Characterの名前(代入する際Overrideしていないか注意)
        /// </summary>
        public string Name
        {
            get { return isOverrideName ? overrideName : stateData ? stateData.ParentAsset.name : null; }
            set { overrideName = Name; }
        }

        public Scenario(Scenario scenario) : this()
        {
            stateData = scenario.stateData;
            text = scenario.text;
            charTime = scenario.charTime;
            animationData = scenario.animationData;
            isOverrideName = scenario.isOverrideName;
            Name = scenario.Name;
        }

#if UNITY_EDITOR
        //Editor上で使う情報
        [SerializeField] ScenarioCharacterData data;
        [SerializeField] Vector2 scroll;
#endif
    }
}
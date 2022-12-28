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
    public struct ScenarioData
    {
        /// <summary>
        /// 指定したStateの情報
        /// </summary>
        [field: SerializeField]
        public CharacterStateData StateData { get; private set; }

        /// <summary>
        /// 台詞
        /// </summary>
        [field: SerializeField]
        public string Text { get; private set; }

        /// <summary>
        /// 1文字を表示する時間
        /// </summary>
        [field: SerializeField]
        public float CharTime { get; private set; }

        /// <summary>
        /// 任意でScenarioに紐づけられる文字のアニメーションのデータ
        /// </summary>
        [field: SerializeField]
        public TextAnimationData AnimationData { get; private set; }

        /// <summary>
        /// 再生するキャラクターアニメーションのステートの名前
        /// </summary>
        [field: SerializeField]
        public string AnimStateName { get; private set; }

        /// <summary>
        /// 名前を上書きしているか
        /// </summary>
        [field: SerializeField]
        public bool IsOverrideName { get; private set; }

        /// <summary>
        /// 上書きした名前
        /// </summary>
        [field: SerializeField]
        public string OverrideName { get; private set; }

        /// <summary>
        /// Characterの名前
        /// </summary>
        public string Name
        {
            get => IsOverrideName ? OverrideName : StateData ? StateData.ParentAsset.DisplayName : null;
            set
            {
                if (IsOverrideName) OverrideName = value;
            }
        }

        public ScenarioData(ScenarioData scenario) : this()
        {
            StateData = scenario.StateData;
            Text = scenario.Text;
            CharTime = scenario.CharTime;
            AnimationData = scenario.AnimationData;
            IsOverrideName = scenario.IsOverrideName;
            Name = scenario.Name;
            AnimStateName = scenario.AnimStateName;
        }

        public ScenarioData(CharacterStateData stateData, string text, float charTime, TextAnimationData animationData, string animStateName, string overrideName) : this()
        {
            StateData = stateData;
            Text = text;
            CharTime = charTime;
            AnimationData = animationData;
            IsOverrideName = true;
            Name = overrideName;
            AnimStateName = animStateName;
        }

        public ScenarioData(CharacterStateData stateData, string text, float charTime, TextAnimationData animationData, string animStateName) : this()
        {
            StateData = stateData;
            Text = text;
            CharTime = charTime;
            AnimationData = animationData;
            IsOverrideName = false;
            Name = default;
            AnimStateName = animStateName;
        }

        public void SetStateData(CharacterStateData stateData)
        {
            StateData = stateData;
        }

        public void SetText(string text)
        {
            Text = text;
        }

        public void SetCharTime(float charTime)
        {
            CharTime = charTime;
        }

        public void SetAnimationData(TextAnimationData animationData)
        {
            AnimationData = animationData;
        }

        public void SetDefaultName()
        {
            IsOverrideName = false;
        }

        public void SetOverrideName(string overrideName)
        {
            IsOverrideName = true;
            OverrideName = overrideName;
        }

        public void SetAnimStateName(string animStateName)
        {
            AnimStateName = animStateName;
        }

#if UNITY_EDITOR
        [SerializeField] private Vector2 _textScroll;
#endif
    }
}
//変数を使っていない警告を無理やり切る
#pragma warning disable 0414
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScenarioController
{
    /// <summary>
    /// ScenarioをEditor上で入力する時に使うClass
    /// </summary>
    [Serializable]
    public class ScenarioInput
    {
        public ScenarioDisplayBase scenarioDisplayBase;

        public List<Scenario> scenarios = new List<Scenario>();

        /// <summary>
        /// 登録されたScenarioDisplayBaseでScenarioの再生を始める(PlayableAssetの場合は使えない)
        /// </summary>
        public void PlayScenario()
        {
            scenarioDisplayBase.PlayScenario(scenarios);
        }

#if UNITY_EDITOR
        //Editor上で使う情報
        //Property全体を表示するか
        [SerializeField] bool display = true;
#endif
    }
}

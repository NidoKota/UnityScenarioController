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
        [field: SerializeField] public ScenarioDisplayBase ScenarioDisplayBase { get; private set; }

        [field: SerializeField] public List<ScenarioData> Scenarios { get; private set; }

        /// <summary>
        /// 登録されたScenarioDisplayBaseでScenarioの再生を始める
        /// </summary>
        public void PlayScenario()
        {
            ScenarioDisplayBase.PlayScenario(Scenarios);
        }
    }
}
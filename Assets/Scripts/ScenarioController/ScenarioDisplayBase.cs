using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ScenarioController
{
    // TODO リファクタリング

    /// <summary>
    /// ScenarioDisplayとScenarioDisplayLightの基底クラス
    /// </summary>
    public abstract class ScenarioDisplayBase : MonoBehaviour
    {
        /// <summary>
        /// Scenarioを表示するText
        /// </summary>
        public TextMeshProUGUI tmp;

        /// <summary>
        /// 文字が表示されたときに流れるSE
        /// </summary>
        public AudioSource charSE;

        /// <summary>
        /// 文字表示をカットしたときに流れるSE
        /// </summary>
        public AudioSource cutScenarioSE;

        /// <summary>
        /// 次のScenarioに移る時に流れるSE
        /// </summary>
        public AudioSource nextScenarioSE;

        /// <summary>
        /// 次のScenarioに移行する時にフェードさせるか
        /// </summary>
        public bool nextScenarioFadeOut = true;

        /// <summary>
        /// 次のScenarioに移行するフェードの時間
        /// </summary>
        public float nextScenarioFadeTime = 0.5f;

        /// <summary>
        /// 次のScenarioに移行するまでの待機時間
        /// </summary>
        public float nextScenarioWaitTime;

#if UNITY_EDITOR
        [SerializeField, Space] bool _InfoDebug = false;

        protected bool InfoDebug
        {
            get => _InfoDebug;
            set => _InfoDebug = value;
        }
#endif

        /// <summary>
        /// ScenarioのStateが変わったときに呼び出されるEvent
        /// </summary>
        public event Action<ScenarioDisplayState> ScenarioStateChangeEvent;


        /// <summary>
        /// Scenarioを途中でカットしたときに呼び出されるEvent
        /// </summary>
        public event Action ScenarioCutEvent;

        /// <summary>
        /// 現在の状態
        /// </summary>
        public ScenarioDisplayState State { get; protected set; } = ScenarioDisplayState.Hide;

        /// <summary>
        /// 現在処理されているScenario
        /// </summary>
        public ScenarioData currentScenario { get; protected set; }

        public IEnumerable<ScenarioData> scenarios { get; protected set; }

        /// <summary>
        /// scenarios中のcurrentScenarioのIndex
        /// </summary>
        public int scenarioIndex { get; protected set; }

        /// <summary>
        /// 複数のScenarioの表示を開始
        /// </summary>
        public abstract void PlayScenario(IEnumerable<ScenarioData> scenarios);

        /// <summary>
        /// 複数のScenarioの表示を開始
        /// </summary>
        public abstract void PlayScenario(params ScenarioData[] scenarios);

        /// <summary>
        /// Scenarioの表示を強制停止する
        /// </summary>
        public abstract void ForceStop();

        /// <summary>
        /// Scenarioを次に進める 最後のScenarioの場合ScenarioDisplayを非表示にする
        /// </summary>
        public abstract bool Next();

        protected void ScenarioStateChanged(ScenarioDisplayState state)
        {
            ScenarioStateChangeEvent?.Invoke(state);
        }

        protected void ScenarioCut()
        {
            ScenarioCutEvent?.Invoke();
        }
    }

    public enum ScenarioDisplayState
    {
        /// <summary>
        /// 表示されていない状態
        /// </summary>
        Hide,

        /// <summary>
        /// 表示されている状態
        /// </summary>
        Work,

        /// <summary>
        /// 次への移行を開始した状態
        /// </summary>
        Next,

        /// <summary>
        /// 次のScenarioへの待機状態
        /// </summary>
        Wait
    }
}
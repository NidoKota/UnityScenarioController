using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ScenarioController.ScenarioEventPlayable
{
    /// <summary>
    /// Scenarioの再生時、最初からTimelineを止めて終了を待つClip
    /// </summary>
    [Serializable]
    public class ScenarioEventStopPlayableClip : PlayableAsset, ITimelineClipAsset
    {
        public ScenarioInput scenarioInput; //Scenarioの入力
        [NonSerialized]
        public bool finishFiring; //発火したかどうか

#if UNITY_EDITOR
        //Editor上で使う情報
        [HideInInspector]
        public ScenarioDisplayBase scenarioDisplayBase;
#endif

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<ScenarioEventPlayableBehaviour>.Create(graph, new ScenarioEventPlayableBehaviour());
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ScenarioController.ScenarioEventPlayable
{
    /// <summary>
    /// Scenarioの再生時、終了時間になるまでTimelineを再生しながら終了を待つClip
    /// </summary>
    [Serializable]
    public class ScenarioEventMovePlayableClip : PlayableAsset, ITimelineClipAsset
    {
        public ScenarioInput scenarioInput; //Scenarioの入力
        [NonSerialized]
        public bool clipIn;  //Clipの最初に到達した
        [NonSerialized]
        public bool clipOut; //Clipの最後に到達した

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
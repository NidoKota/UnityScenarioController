using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using ScenarioController.ScenarioEventPlayable;
using UnityEditor;
#endif

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

#if UNITY_EDITOR
namespace ScenarioControllerEditor
{
    /// <summary>
    /// ScenarioEventStopPlayableClipのEditor拡張
    /// </summary>
    [CustomEditor(typeof(ScenarioEventStopPlayableClip))]
    public class ScenarioEventStopPlayableClipEditor : Editor
    {
        ScenarioEventStopPlayableClip scenarioEventStopPlayableClip;
        void OnEnable()
        {
            scenarioEventStopPlayableClip = target as ScenarioEventStopPlayableClip;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (scenarioEventStopPlayableClip.finishFiring) EditorGUILayout.HelpBox("発火完了", MessageType.Info);
            else EditorGUILayout.HelpBox("発火未完了", MessageType.Info);
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

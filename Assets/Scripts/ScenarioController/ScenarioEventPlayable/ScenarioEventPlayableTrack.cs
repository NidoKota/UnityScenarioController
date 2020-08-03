using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ScenarioController.ScenarioEventPlayable
{
    /// <summary>
    /// ScenarioEventPlayableのTrackAsset
    /// </summary>
    [TrackColor(1f, 0.925f, 0.3f)]
    [TrackClipType(typeof(ScenarioEventStopPlayableClip))]
    [TrackClipType(typeof(ScenarioEventMovePlayableClip))]
    [TrackBindingType(typeof(ScenarioDisplayBase))]
    public class ScenarioEventPlayableTrack : TrackAsset
    {
        //主に初期化処理を行う
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            PlayableDirector director = go.GetComponent<PlayableDirector>();
            ScenarioDisplayBase scenarioDisplayBase = (ScenarioDisplayBase)director.GetGenericBinding(this);

#if UNITY_EDITOR
            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset.GetType() == typeof(ScenarioEventStopPlayableClip)) ((ScenarioEventStopPlayableClip)clip.asset).scenarioDisplayBase = scenarioDisplayBase;
                else ((ScenarioEventMovePlayableClip)clip.asset).scenarioDisplayBase = scenarioDisplayBase;
            }
#endif
            //Mixerを生成
            ScriptPlayable<ScenarioEventPlayableMixer> mixer = ScriptPlayable<ScenarioEventPlayableMixer>.Create(graph, inputCount);
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
#endif
            {
                //Timelineの再生が終了したらScenarioを強制停止させる
                //???たまにエラー出る(開放していないから?)
               if (director) director.stopped += (pd) => scenarioDisplayBase.ForceStop();

                //Mixerの処理で必要なものを渡す
                ScenarioEventPlayableMixer scenarioEventPlayableMixer = mixer.GetBehaviour();
                scenarioEventPlayableMixer.director = director;
                IEnumerable<TimelineClip> clips = GetClips();
                scenarioEventPlayableMixer.clips = clips;
                
                //Timelineの再生が開始されたとき
                if (director) director.played += TimelinePlayed;
                void TimelinePlayed(PlayableDirector pd)
                {
                    //Clipの初期化
                    foreach (TimelineClip clip in clips)
                    {
                        if (clip.asset.GetType() == typeof(ScenarioEventStopPlayableClip))
                        {
                            ScenarioEventStopPlayableClip scenarioEventStopPlayableClip = (ScenarioEventStopPlayableClip)clip.asset;
                            scenarioEventStopPlayableClip.finishFiring = false;
                        }
                        else
                        {
                            ScenarioEventMovePlayableClip scenarioEventMovePlayableClip = (ScenarioEventMovePlayableClip)clip.asset;
                            scenarioEventMovePlayableClip.clipIn = scenarioEventMovePlayableClip.clipOut = false;
                        }
                    }
                    director.played -= TimelinePlayed;
                }
            }
            return mixer;
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            //Clip生成時の長さ
            if (clip.asset.GetType() == typeof(ScenarioEventStopPlayableClip)) clip.duration = 0.166666666666667;
            else clip.duration = 2.5;
        }
    }
}

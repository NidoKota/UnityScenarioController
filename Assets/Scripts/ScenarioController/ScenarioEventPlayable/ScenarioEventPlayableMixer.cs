using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ScenarioController.ScenarioEventPlayable
{
    /// <summary>
    /// ScenarioEventPlayableのMixer
    /// </summary>
    public class ScenarioEventPlayableMixer : PlayableBehaviour
    {
        public IEnumerable<TimelineClip> clips; //登録されている全てのClip
        public PlayableDirector director;       //このPlayableを再生しているPlayableDirector
        ScenarioDisplayBase scenarioDisplayBase;

        //1コマごとに呼ばれる
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
#endif
            {
                scenarioDisplayBase = playerData as ScenarioDisplayBase;

                foreach (TimelineClip clip in clips)
                {
                    if (clip.asset.GetType() == typeof(ScenarioEventStopPlayableClip))
                    {
                        //Timeline再生時間がのClipの開始時間以降になった時
                        if (director.time >= clip.start)
                        {
                            ScenarioEventStopPlayableClip scenarioEventStopPlayableClip = (ScenarioEventStopPlayableClip)clip.asset;
                            if (!scenarioEventStopPlayableClip.finishFiring)
                            {
                                //一応警告しているがさほど問題はない
                                //if (director.time >= clip.end) Debug.LogWarning("Timelineで処理落ちが発生しました");

                                //Timelineの再生を一時停止させる(停止させるとCinemachineの動作が停止してしまうため)
                                director.playableGraph.GetRootPlayable(0).SetSpeed(0);
                                //Clipの終わりまでTimelineを飛ばす
                                director.time = clip.end;
                                //Scenarioを表示させる
                                scenarioDisplayBase.PlayScenario(scenarioEventStopPlayableClip.scenarioInput.scenarios);
                                scenarioDisplayBase.ScenarioStateChangeEvent += OnStateChange;
                                scenarioEventStopPlayableClip.finishFiring = true;
                                break;
                            }
                        }

                        //???他のScenarioが再生されたときに進めたと勘違いする
                        void OnStateChange(ScenarioDisplayState state)
                        {
                            //Scenarioの再生が終了した時
                            if (state == ScenarioDisplayState.Hide)
                            {
                                try
                                {
                                    //再生を再開
                                    director.playableGraph.GetRootPlayable(0).SetSpeed(1);
                                }
                                catch { }
                                //登録を解除
                                scenarioDisplayBase.ScenarioStateChangeEvent -= OnStateChange;
                            }
                        }
                    }
                    else
                    {
                        ScenarioEventMovePlayableClip scenarioEventMovePlayableClip = (ScenarioEventMovePlayableClip)clip.asset;
                        //Timeline再生時間がのClipの開始時間以降になった時
                        if (director.time >= clip.start)
                        {
                            if (!scenarioEventMovePlayableClip.clipIn)
                            {
                                //Clipの始めにTimelineを戻す
                                director.time = clip.start;
                                //Scenarioを表示させる
                                scenarioDisplayBase.PlayScenario(scenarioEventMovePlayableClip.scenarioInput.scenarios);
                                scenarioDisplayBase.ScenarioStateChangeEvent += OnStateChange;
                                scenarioEventMovePlayableClip.clipIn = true;
                                break;
                            }
                        }
                        //Timeline再生時間がのClipの終了時間以降になった時
                        if (director.time >= clip.end)
                        {
                            if (!scenarioEventMovePlayableClip.clipOut)
                            {
                                //Clipの終わりにTimelineを戻す
                                director.time = clip.end;
                                //Timelineの再生を一時停止させる(停止させるとCinemachineの動作が停止してしまうため)
                                director.playableGraph.GetRootPlayable(0).SetSpeed(0);
                                scenarioEventMovePlayableClip.clipOut = true;
                                break;
                            }
                        }

                        //???カットが微妙におかしい
                        void OnStateChange(ScenarioDisplayState state)
                        {
                            //Scenarioの再生が終了した時
                            if (state == ScenarioDisplayState.Hide)
                            {
                                //Clipの最後に到達した後にScenarioの再生が終了した
                                if (scenarioEventMovePlayableClip.clipOut)
                                {
                                    try
                                    {
                                        //再生を再開
                                        director.playableGraph.GetRootPlayable(0).SetSpeed(1);
                                    }
                                    catch { }
                                }
                                //Clipの最後に到達する前にScenarioの再生が終了した
                                else
                                {
                                    //Clipの終わりまでTimelineを飛ばす
                                    director.time = clip.end;
                                    scenarioEventMovePlayableClip.clipOut = true;
                                }
                                //登録を解除
                                scenarioDisplayBase.ScenarioStateChangeEvent -= OnStateChange;
                            }
                        }
                    }
                }
            }
        }
    }
}

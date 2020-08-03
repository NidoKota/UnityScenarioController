using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ScenarioController.AutoAnimationPlayable
{
    /// <summary>
    /// AutoAnimationPlayableのTrackAsset
    /// </summary>
    [TrackColor(0.1f, 0.39f, 0.63f)]
    [TrackClipType(typeof(AutoAnimationClip))]
    [TrackBindingType(typeof(Animator))]
    public class AutoAnimationTrack : TrackAsset
    {
        /// <summary>
        /// 再生し続けるために使うTimelineの制御下にないPlayableGraph
        /// </summary>
        [NonSerialized]
        public PlayableGraph aapGraph;
        [NonSerialized]
        public int cashedNowClipIndex;
        RuntimeAnimatorController saveRuntimeAnimatorController = default;
        Playable playable;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            IEnumerable<TimelineClip> clips = GetClips();

#if UNITY_EDITOR
            foreach (TimelineClip clip in clips)
            {
                AutoAnimationClip autoAnimationClip = (AutoAnimationClip)clip.asset;
                autoAnimationClip.autoAnimationTrack = this;
            }

            if (EditorApplication.isPlaying)
#endif
            {
                //Timelineの制御下にないGraphの設定
                aapGraph = PlayableGraph.Create("AutoAnimationPlayable");
                IEnumerator<TimelineClip> clipsEnumerator = clips.GetEnumerator();
                PlayableDirector director = go.GetComponent<PlayableDirector>();
                Animator animator = (Animator)director.GetGenericBinding(this);

                //PlayableDirectorと同じTimeUpdateModeにする
                aapGraph.SetTimeUpdateMode(director.timeUpdateMode);

                //Unityの通常のAnimatorPlayableを流用
                AnimationPlayableOutput animOutput = AnimationPlayableOutput.Create(aapGraph, "AutoAnimationPlayableOutput", animator);
                AnimationMixerPlayable animationMixerPlayable = AnimationMixerPlayable.Create(aapGraph, inputCount, false);

                //ClipとMixerを接続
                for (int i = 0; clipsEnumerator.MoveNext(); i++) aapGraph.Connect(AnimationClipPlayable.Create(aapGraph, ((AutoAnimationClip)clipsEnumerator.Current.asset).animationClip), 0, animationMixerPlayable, i);
                animOutput.SetSourcePlayable(animationMixerPlayable);

                //Timelineの制御下にないGraphの再生を開始
                aapGraph.Play();

                //Timelineの再生が開始されたとき
                if (director) director.played += TimelinePlayed;
                void TimelinePlayed(PlayableDirector pd)
                {
                    //???AnimatorのRuntimeAnimatorControllerを外す以外にPlayabeを止める方法がわからない(PauseとかSetSpeed(0)でもダメ)
                    /*animator.playableGraph.GetRootPlayable(0).SetSpeed(0);
                    animator.playableGraph.GetRootPlayable(0).Pause();
                    animator.playableGraph.Stop();*/
                    saveRuntimeAnimatorController = animator.runtimeAnimatorController;
                    animator.runtimeAnimatorController = null;
                    director.played -= TimelinePlayed;
                }

                //Timelineの再生が終了したとき
                if (director) director.stopped += TimelineStopped;
                void TimelineStopped(PlayableDirector pd)
                {
                    //Timelineの制御下にないGraphを開放
                    if (aapGraph.IsValid()) aapGraph.Destroy();
                    //AnimatorのRuntimeAnimatorControllerを元に戻す
                    if (animator) animator.runtimeAnimatorController = saveRuntimeAnimatorController;
                    director.stopped -= TimelineStopped;
                }

                //Mixerの処理で必要なものを渡す
                ScriptPlayable<AutoAnimationMixer> p = ScriptPlayable<AutoAnimationMixer>.Create(graph, inputCount);
                AutoAnimationMixer autoAnimationPlayableMixer = p.GetBehaviour();
                autoAnimationPlayableMixer.animationMixerPlayable = animationMixerPlayable;
                autoAnimationPlayableMixer.clips = clips;
                autoAnimationPlayableMixer.director = director;
                autoAnimationPlayableMixer.autoAnimationTrack = this;
                playable = p;
            }
#if UNITY_EDITOR
            else
            {
                //ゲームを実行していない時はUnityの通常のAnimatorPlayableと同じ処理をしてプレビューさせる
                AnimationMixerPlayable p = AnimationMixerPlayable.Create(graph, inputCount, false);
                playable = p;
            }
#endif
            return playable;
        }

#if UNITY_EDITOR

        public override IEnumerable<PlayableBinding> outputs
        {
            get
            {
                //Timelineの出力は使わないので適当に生成
                if (EditorApplication.isPlaying) yield return ScriptPlayableBinding.Create(name, this, typeof(UnityEngine.Object));
                //ゲームを実行していない時はUnityの通常のAnimatorPlayableと同じ処理をしてプレビューさせる
                else yield return AnimationPlayableBinding.Create(name, this);
            }
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var animator = (Animator)director.GetGenericBinding(this);
            if (!animator) return;

            ScenarioControllerUtility.PreviewFromCurves(animator, GetClips().Select(x => ((AutoAnimationClip)x.asset).animationClip).ToArray());
        }

        public int GetClipIndex(AutoAnimationClip clip)
        {
            IEnumerator clips = GetClips().GetEnumerator();

            int index = -1;
            for (int i = 0; clips.MoveNext(); i++)
            {
                if (clip == (AutoAnimationClip)((TimelineClip)clips.Current).asset)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public void SetClipLength(int clipIndex, double duration)
        {
            GetClips().Select((data, index) => (data, index)).First(x => x.index == clipIndex).data.duration = duration;
        }
#endif
    }
}
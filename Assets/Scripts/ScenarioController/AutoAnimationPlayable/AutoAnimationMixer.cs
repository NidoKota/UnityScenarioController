using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;

namespace ScenarioController.AutoAnimationPlayable
{
    /// <summary>
    /// AutoAnimationPlayaleのMixer
    /// </summary>
    public class AutoAnimationMixer : PlayableBehaviour
    {
        public AutoAnimationTrack autoAnimationTrack;
        public IEnumerable<TimelineClip> clips;               //登録されている全てのClip
        public PlayableDirector director;                     //このPlayableを再生しているPlayableDirector
        public AnimationMixerPlayable animationMixerPlayable; //Animation制御で使用しているAnimationMixer
        int nowClipIndex;                                     //現在再生しているClipのclips中のindex

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            IEnumerator<TimelineClip> clipsEnumerator = clips.GetEnumerator();
            nowClipIndex = -1;

            for (int i = 0; clipsEnumerator.MoveNext(); i++)
            {
                //Timelineで設定されたウエイトやスピードをそのままanimationMixerPlayableに渡す
                animationMixerPlayable.SetInputWeight(i, playable.GetInputWeight(i));
                animationMixerPlayable.GetInput(i).SetSpeed(playable.GetInput(i).GetSpeed());
                
                //再生しているClipを取得
                if (director.time >= clipsEnumerator.Current.start && director.time < clipsEnumerator.Current.end) nowClipIndex = i;
            }

            //再生しているClipが変わった場合
            if (nowClipIndex != autoAnimationTrack.cashedNowClipIndex)
            {
                //今再生しているClipがある場合
                if (nowClipIndex != -1)
                {
                    //Clipの再生時間を初期化
                    TimelineClip nowTimelineClip = clips.Select((data, index) => new { data, index }).First(x => x.index == nowClipIndex).data;
                    animationMixerPlayable.GetInput(nowClipIndex).SetTime(nowTimelineClip.clipIn);

                }
                autoAnimationTrack.cashedNowClipIndex = nowClipIndex;
            }
        }
    }
}
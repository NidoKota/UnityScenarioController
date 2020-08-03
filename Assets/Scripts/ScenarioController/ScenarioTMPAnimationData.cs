using System.Collections;
using UnityEngine;

namespace ScenarioController
{
    [CreateAssetMenu(menuName = "ScenarioTMPAnimationData", fileName = "ScenarioTMPAnimationData", order = 1000)]
    public class ScenarioTMPAnimationData : ScriptableObject
    {
        public bool usePositionYAnimation;
        public AnimationCurve positionYAnimation;

        public bool useRotationZAnimation;
        public AnimationCurve rotationZAnimation;

        public bool useScaleAllAnimation;
        public AnimationCurve scaleAllAnimation;

        public bool useColorAlphaAnimation;
        public AnimationCurve colorAlphaAnimation;

        [Space(15)]
        public Vector3 addPosition;
        public Vector3 addRotation;
        public Vector3 addScale = Vector3.one;

        float[] maxAnimationTimes = new float[4];

        /// <summary>
        /// 登録されているAnimationCurveの中から一番長いCurveの時間の長さを返す
        /// </summary>
        public float GetMaxAnimationTime()
        {
            if (usePositionYAnimation) maxAnimationTimes[0] = positionYAnimation.keys[positionYAnimation.length - 1].time;
            if (useRotationZAnimation) maxAnimationTimes[1] = rotationZAnimation.keys[rotationZAnimation.length - 1].time;
            if (useScaleAllAnimation) maxAnimationTimes[2] = scaleAllAnimation.keys[scaleAllAnimation.length - 1].time;
            if (useColorAlphaAnimation) maxAnimationTimes[3] = colorAlphaAnimation.keys[colorAlphaAnimation.length - 1].time;
            return Mathf.Max(maxAnimationTimes);
        }
    }
}
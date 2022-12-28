using System.Collections;
using UnityEngine;

namespace ScenarioController
{
    /// <summary>
    /// テキストのアニメーション情報
    /// </summary>
    [CreateAssetMenu(menuName = "TextAnimationData", fileName = "TextAnimationData", order = 1000)]
    public class TextAnimationData : ScriptableObject
    {
        [SerializeField] private bool _usePositionXAnimation;
        public bool UsePositionXAnimation => _usePositionXAnimation;
        [SerializeField] private AnimationCurve _positionXAnimation;
        public AnimationCurve PositionXAnimation => _positionXAnimation;

        [SerializeField] private bool _usePositionYAnimation;
        public bool UsePositionYAnimation => _usePositionYAnimation;
        [SerializeField] private AnimationCurve _positionYAnimation;
        public AnimationCurve PositionYAnimation => _positionYAnimation;

        [SerializeField] private bool _useRotationZAnimation;
        public bool UseRotationZAnimation => _useRotationZAnimation;
        [SerializeField] private AnimationCurve _rotationZAnimation;
        public AnimationCurve RotationZAnimation => _rotationZAnimation;

        [SerializeField] private bool _useScaleAllAnimation;
        public bool UseScaleAllAnimation => _useScaleAllAnimation;
        [SerializeField] private AnimationCurve _scaleAllAnimation;
        public AnimationCurve ScaleAllAnimation => _scaleAllAnimation;

        [SerializeField] private bool _useColorAlphaAnimation;
        public bool UseColorAlphaAnimation => _useColorAlphaAnimation;
        [SerializeField] private AnimationCurve _colorAlphaAnimation;
        public AnimationCurve ColorAlphaAnimation => _colorAlphaAnimation;

        [Space(15)] [SerializeField] private Vector3 _addPosition;
        public Vector3 AddPosition => _addPosition;
        [SerializeField] private Vector3 _addRotation;
        public Vector3 AddRotation => _addRotation;
        [SerializeField] private Vector3 _addScale = Vector3.one;
        public Vector3 AddScale => _addScale;

        private bool _isInitialized;
        private float[] _maxAnimationTimes;

        /// <summary>
        /// 登録されているAnimationCurveの中から一番長いCurveの時間の長さを返す
        /// </summary>
        public float GetMaxAnimationTime()
        {
            if (!_isInitialized)
            {
                _maxAnimationTimes = new float[5];
                _isInitialized = true;
            }

            if (_usePositionXAnimation) _maxAnimationTimes[0] = _positionXAnimation.keys[_positionXAnimation.length - 1].time;
            if (_usePositionYAnimation) _maxAnimationTimes[1] = _positionYAnimation.keys[_positionYAnimation.length - 1].time;
            if (_useRotationZAnimation) _maxAnimationTimes[2] = _rotationZAnimation.keys[_rotationZAnimation.length - 1].time;
            if (_useScaleAllAnimation) _maxAnimationTimes[3] = _scaleAllAnimation.keys[_scaleAllAnimation.length - 1].time;
            if (_useColorAlphaAnimation) _maxAnimationTimes[4] = _colorAlphaAnimation.keys[_colorAlphaAnimation.length - 1].time;
            return Mathf.Max(_maxAnimationTimes);
        }
    }
}
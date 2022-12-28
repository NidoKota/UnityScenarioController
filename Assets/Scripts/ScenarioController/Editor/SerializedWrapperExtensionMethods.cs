using UnityEditor;
using UnityEngine;

namespace ScenarioController.Editor
{
    /// <summary>
    /// SerializedWrapperの拡張メソッド
    /// </summary>
    public static class SerializedWrapperExtensionMethods
    {
        public static ScenarioData GetScenarioData(this SerializedWrapper serializedWrapper)
        {
            SerializedProperty stateDataProp = serializedWrapper.FindProperty("<StateData>k__BackingField");
            SerializedProperty textProp = serializedWrapper.FindProperty("<Text>k__BackingField");
            SerializedProperty charTimeProp = serializedWrapper.FindProperty("<CharTime>k__BackingField");
            SerializedProperty animationDataProp = serializedWrapper.FindProperty("<AnimationData>k__BackingField");
            SerializedProperty animStateNameProp = serializedWrapper.FindProperty("<AnimStateName>k__BackingField");
            SerializedProperty isOverrideNameProp = serializedWrapper.FindProperty("<IsOverrideName>k__BackingField");
            SerializedProperty overrideNameProp = serializedWrapper.FindProperty("<OverrideName>k__BackingField");

            if (isOverrideNameProp.boolValue)
                return new ScenarioData(
                    stateDataProp.objectReferenceValue as CharacterStateData,
                    textProp.stringValue,
                    charTimeProp.floatValue,
                    animationDataProp.objectReferenceValue as TextAnimationData,
                    animStateNameProp.stringValue,
                    overrideNameProp.stringValue);

            return new ScenarioData(
                stateDataProp.objectReferenceValue as CharacterStateData,
                textProp.stringValue,
                charTimeProp.floatValue,
                animationDataProp.objectReferenceValue as TextAnimationData,
                animStateNameProp.stringValue);
        }

        public static void SetScenarioData(this SerializedWrapper serializedWrapper, ScenarioData scenario)
        {
            SerializedProperty stateDataProp = serializedWrapper.FindProperty("<StateData>k__BackingField");
            SerializedProperty textProp = serializedWrapper.FindProperty("<Text>k__BackingField");
            SerializedProperty charTimeProp = serializedWrapper.FindProperty("<CharTime>k__BackingField");
            SerializedProperty animationDataProp = serializedWrapper.FindProperty("<AnimationData>k__BackingField");
            SerializedProperty animStateNameProp = serializedWrapper.FindProperty("<AnimStateName>k__BackingField");
            SerializedProperty isOverrideNameProp = serializedWrapper.FindProperty("<IsOverrideName>k__BackingField");
            SerializedProperty overrideNameProp = serializedWrapper.FindProperty("<OverrideName>k__BackingField");

            stateDataProp.objectReferenceValue = scenario.StateData;
            textProp.stringValue = scenario.Text;
            charTimeProp.floatValue = scenario.CharTime;
            animationDataProp.objectReferenceValue = scenario.AnimationData;
            animStateNameProp.stringValue = scenario.AnimStateName;
            isOverrideNameProp.boolValue = scenario.IsOverrideName;
            overrideNameProp.stringValue = scenario.OverrideName;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace ScenarioController
{
    /// <summary>
    /// ScenarioControllerで使う便利な何か
    /// </summary>
    public static class ScenarioControllerUtility
    {
#if UNITY_EDITOR
        /// <summary>
        /// 全てのProjectWindowをリセットする(UnityEditor専用)
        /// </summary>
        public static void ProjectWindowReset()
        {
            Type typePrjctWindow = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.ProjectBrowser");
            MethodInfo method = typePrjctWindow.GetMethod("ResetViews", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (EditorWindow window in (EditorWindow[])Resources.FindObjectsOfTypeAll(typePrjctWindow)) method.Invoke(window, null);
        }

        /// <summary>
        /// TextAreaに入りきらなければスクロールバーが出てくる奴(UnityEditor専用)
        /// </summary>
        public static string ScrollableTextAreaInternal(Rect position, string text, ref Vector2 scrollPosition, GUIStyle style)
        {
            MethodInfo method = typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.NonPublic | BindingFlags.Static);
            object[] obj = new object[] { position, text, scrollPosition, style };
            object result = method.Invoke(null, obj);
            scrollPosition = (Vector2)obj[2];
            return (string)result;
        }

        /// <summary>
        /// AnimationClipで変更された値を元に戻す(Humanoidは考慮していない)(UnityEditor専用)
        /// </summary>
        public static void PreviewFromCurves(Animator animator, AnimationClip[] animationClips)
        {
            //AnimationPreviewUtilitiesはTimeline\Runtime\Utilitysの中に入っている
            Type animationPreviewUtilities = Assembly.Load("Unity.Timeline").GetType("UnityEngine.Timeline.AnimationPreviewUtilities");
            MethodInfo getBindingsMethod = animationPreviewUtilities.GetMethod("GetBindings", BindingFlags.Public | BindingFlags.Static);
            MethodInfo previewFromCurvesMethod = animationPreviewUtilities.GetMethod("PreviewFromCurves", BindingFlags.Public | BindingFlags.Static);

            previewFromCurvesMethod.Invoke(null, new object[] { animator.gameObject, getBindingsMethod.Invoke(null, new object[] { animator.gameObject, animationClips }) });
        }
    }
#endif
}
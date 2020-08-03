#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ScenarioController;
using UnityEditor;
using UnityEditorInternal;

namespace ScenarioControllerEditor
{
    public class ScenarioControllerEditorUtility
    {
        /// <summary>
        /// 全てのProjectWindowをリセットする
        /// </summary>
        public static void ProjectWindowReset()
        {
            Type typePrjctWindow = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.ProjectBrowser");
            MethodInfo method = typePrjctWindow.GetMethod("ResetViews", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (EditorWindow window in (EditorWindow[])Resources.FindObjectsOfTypeAll(typePrjctWindow)) method.Invoke(window, null);
        }

        public static string ScrollableTextAreaInternal(Rect position, string text, ref Vector2 scrollPosition, GUIStyle style)
        {
            MethodInfo method = typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.NonPublic | BindingFlags.Static);
            object[] obj = new object[] { position, text, scrollPosition, style };
            object result = method.Invoke(null, obj);
            scrollPosition = (Vector2)obj[2];
            return (string)result;
        }
    }
}
#endif
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ScenarioController.Editor
{
    /// <summary>
    /// 便利関数群
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// 次の行のRectの取得を繰り返しながらSerializedPropertyの要素を表示する
        /// </summary>
        public static Rect LineRectUpdatePropertyField(Rect lineRect, SerializedProperty propIterator, bool isSerializedProps, GUIContent label = null, params string[] propertyToExclude)
        {
            if (isSerializedProps) propIterator.NextVisible(true);
            do
            {
                if (propertyToExclude.Any(x => x == propIterator.name)) continue;
                EditorGUI.PropertyField(lineRect, propIterator, label, false);
                lineRect = LineRectUpdate(lineRect);
            } while (isSerializedProps && propIterator.NextVisible(false));

            return lineRect;
        }

        /// <summary>
        /// 次の行のRectを取得する
        /// </summary>
        public static Rect LineRectUpdate(Rect lineRect)
        {
            return new Rect(lineRect) {yMin = lineRect.yMax, yMax = lineRect.yMax + EditorGUIUtility.singleLineHeight};
        }

        /// <summary>
        /// Rectを横方向にratiosの比率で分割後、左からindex番目のRectを返す
        /// </summary>
        public static Rect GetHorizonLayout(Rect rect, int index, params float[] ratios)
        {
            float sum = ratios.Sum();
            float leftRatios = ratios.Where((x, i) => i < index).Sum() / sum;
            float rightRatios = ratios.Where((x, i) => i > index).Sum() / sum;
            float width = rect.width;
            return new Rect(rect) {xMin = rect.xMin + width * leftRatios, xMax = rect.xMin + width * (1f - rightRatios)};
        }

        /// <summary>
        /// 全てのProjectWindowをリセットする
        /// </summary>
        public static void ProjectWindowReset()
        {
            Type projectWindowType = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.ProjectBrowser");
            MethodInfo method = projectWindowType.GetMethod("ResetViews", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (EditorWindow window in (EditorWindow[]) Resources.FindObjectsOfTypeAll(projectWindowType)) method.Invoke(window, null);
        }

        /// <summary>
        /// TextAreaに入りきらなければスクロールバーが出るEditorGUI
        /// </summary>
        public static string ScrollableTextAreaInternal(Rect position, string text, ref Vector2 scrollPosition, GUIStyle style)
        {
            MethodInfo method = typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.NonPublic | BindingFlags.Static);
            object[] obj = new object[] {position, text, scrollPosition, style};
            object result = method.Invoke(null, obj);
            scrollPosition = (Vector2) obj[2];
            return (string) result;
        }

        /// <summary>
        /// <para>配列の要素番号を取得する</para>
        /// <para>https://www.urablog.xyz/entry/2017/02/12/165706</para>
        /// </summary>
        public static int GetElementIndex(SerializedProperty property)
        {
            string propertyPath = property.propertyPath;

            var properties = propertyPath.Split('.');

            if (properties.Length < 3) return -1;

            string arrayProperty = properties[properties.Length - 2];
            if (arrayProperty != "Array") return -1;

            var paths = propertyPath.Split('.');
            var lastPath = paths[properties.Length - 1];
            if (!lastPath.StartsWith("data[")) return -1;

            var regex = new System.Text.RegularExpressions.Regex(@"[^0-9]");
            var countText = regex.Replace(lastPath, "");
            if (!int.TryParse(countText, out var index)) return -1;

            return index;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEditor;

namespace ScenarioController.Editor
{
    /// <summary>
    /// Textを別ウィンドウで編集する
    /// </summary>
    public class TextEditorWindow : EditorWindow
    {
        public string Text { get; private set; }

        private SerializedProperty _textProp;
        private SerializedObject _serializedObject;
        private Vector2 _textScroll;

        public static TextEditorWindow Open(SerializedObject serializedObject, SerializedProperty textProp, string title)
        {
            if (serializedObject == null || textProp == null || string.IsNullOrEmpty(title)) throw new ArgumentException();

            TextEditorWindow window = OpenInternal(title);
            window._textProp = textProp;
            window._serializedObject = serializedObject;

            return window;
        }

        public static TextEditorWindow Open(string text, string title)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException();

            TextEditorWindow window = OpenInternal(title);
            window.Text = text;

            return window;
        }

        private static TextEditorWindow OpenInternal(string title)
        {
            TextEditorWindow window = CreateInstance<TextEditorWindow>();
            window.titleContent = new GUIContent(title);
            window.ShowAuxWindow();
            return window;
        }

        private void OnGUI()
        {
            _serializedObject?.Update();
            {
                Rect textRect = new Rect()
                {
                    xMin = 2,
                    width = position.width - 2,
                    yMin = 2,
                    height = position.height - 2
                };

                if (_textProp != null) Text = _textProp.stringValue;
                Text = Utilities.ScrollableTextAreaInternal(textRect, Text, ref _textScroll, EditorStyles.textArea);
                if (_textProp != null) _textProp.stringValue = Text;
            }
            _serializedObject?.ApplyModifiedProperties();
        }
    }
}
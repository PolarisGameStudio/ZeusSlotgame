// EditorUtils.cs
using UnityEditor;
using UnityEngine;

public static class EditorUtils2
{
    private class InputDialog : EditorWindow
    {
        private string input = "";
        private string title = "";
        private string message = "";
        private string defaultValue = "";
        private System.Action<string> callback;

        public static void Show(string title, string message, string defaultValue, System.Action<string> callback)
        {
            InputDialog window = CreateInstance<InputDialog>();
            window.titleContent = new GUIContent(title);
            window.input = defaultValue;
            window.message = message;
            window.callback = callback;
            window.minSize = new Vector2(300, 80);
            window.ShowModalUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(message);
            input = EditorGUILayout.TextField(input);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("OK", GUILayout.Width(60)))
            {
                callback?.Invoke(input);
                Close();
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(60)))
            {
                callback?.Invoke(null); // 或 string.Empty，根据你的逻辑
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    public static string DisplayDialogEditText(string title, string message, string defaultValue)
    {
        
        Debug.Log($"[EditorUtils2] DisplayDialogEditText called: {title}"); // 👈 加这一行
        string result = null;
        bool done = false;

        InputDialog.Show(title, message, defaultValue, s =>
        {
            result = s;
            done = true;
        });

        // 等待用户操作（模态窗口阻塞直到关闭）
        while (!done)
        {
            System.Threading.Thread.Sleep(100);
        }

        return result;
    }
}
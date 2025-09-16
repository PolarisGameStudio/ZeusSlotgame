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

    // 不再返回 string，而是接受一个 Action<string> 作为回调
    public static void DisplayDialogEditText(string title, string message, string defaultValue, System.Action<string> onConfirm)
    {
        Debug.Log($"[EditorUtils2] DisplayDialogEditText called: {title}");

        // InputDialog.Show 现在接收一个额外的回调，当用户取消或关闭窗口时调用
        // (假设 InputDialog.Show 也可以接受一个 onCancel 回调，如果它支持的话)
        InputDialog.Show(title, message, defaultValue, (userInput) => 
        {
            // 当用户点击确认时，我们调用从外部传进来的 onConfirm 回调
            if (onConfirm != null)
            {
                onConfirm(userInput);
            }
        });

        // 移除了致命的 while 循环！
        // 这个方法会立即返回，不会阻塞主线程。
    }
}
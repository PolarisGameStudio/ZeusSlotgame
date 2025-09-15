using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class PlistColumnEditorWindow : EditorWindow
{
    private string xmlPath = "";
    private XmlDocument xmlDoc;
    private PlistTreeNode rootNode;
    private Vector2 scrollPos;

    private List<List<PlistTreeNode>> columns = new List<List<PlistTreeNode>>();
    private List<int> selectedIndices = new List<int>();

    // 用于重命名时临时存储
    private PlistTreeNode nodeToRename;
    private string originalName;
    
    private PlistTreeNode renamingNode; // 当前正在重命名的节点
    private string renamingInput = "";  // 输入框内容
    private int renamingColumnIndex = -1;
    private int renamingItemIndex = -1;

    [MenuItem("Tools/Plist Column Editor")]
    public static void ShowWindow()
    {
        GetWindow<PlistColumnEditorWindow>("Plist Column Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        xmlPath = EditorGUILayout.TextField("Plist Path:", xmlPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string selected = EditorUtility.OpenFilePanel("Select Plist File", "", "plist,xml");
            if (!string.IsNullOrEmpty(selected))
            {
                xmlPath = selected;
                LoadPlist();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (string.IsNullOrEmpty(xmlPath))
        {
            EditorGUILayout.HelpBox("Please select a .plist file.", MessageType.Info);
            return;
        }

        if (rootNode == null)
        {
            if (GUILayout.Button("Load Plist"))
            {
                LoadPlist();
            }
            return;
        }

        if (GUILayout.Button("Save to Plist"))
        {
            SavePlist();
        }

        EditorGUILayout.Space();

        if (columns.Count == 0 && rootNode != null)
        {
            InitializeColumns();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 80), GUILayout.ExpandWidth(true));

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        for (int colIndex = 0; colIndex < columns.Count; colIndex++)
        {
            DrawColumn(colIndex);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }
    private void InitializeColumns()
    {
        columns.Clear();
        selectedIndices.Clear();
        if (rootNode != null)
        {
            columns.Add(new List<PlistTreeNode> { rootNode });
            selectedIndices.Add(-1);
        }
    }

    private void CommitRename()
    {
        if (renamingNode != null)
        {
            if (string.IsNullOrWhiteSpace(renamingInput))
            {
                EditorUtility.DisplayDialog("Invalid Name", "Name cannot be empty.", "OK");
                return; // 保持重命名状态
            }

            if (renamingInput != renamingNode.Name)
            {
                // 更新树节点
                renamingNode.Name = renamingInput;

                // 更新 XML（找到对应的 <key> 节点）
                if (renamingNode.XmlKeyNode != null)
                {
                    renamingNode.XmlKeyNode.InnerText = renamingInput;
                }

                // 刷新当前列及右侧列
                RefreshColumnsFrom(renamingColumnIndex);
            }
        }

        CancelRename();
        Repaint();
        
        GUI.FocusControl(null);
    }
    
    

    private void CancelRename()
    {
        renamingNode = null;
        renamingInput = "";
        renamingColumnIndex = -1;
        renamingItemIndex = -1;
        GUI.FocusControl(null);
    }

    private void DrawColumn(int colIndex)
    {
        var column = columns[colIndex];
        int selectedIndex = selectedIndices[colIndex];

        GUILayout.BeginVertical("box", GUILayout.Width(220), GUILayout.ExpandHeight(true));

        EditorGUILayout.LabelField($"Level {colIndex + 1}", EditorStyles.boldLabel);

        for (int i = 0; i < column.Count; i++)
        {
            var node = column[i];
            bool isSelected = (i == selectedIndex);

            GUIStyle style = isSelected ? EditorStyles.label : GUI.skin.GetStyle("Label");
            Color defaultColor = GUI.color;
            if (isSelected) GUI.color = new Color(0.8f, 0.9f, 1f);

            Rect rect = EditorGUILayout.GetControlRect(false, 22);

            
            if (renamingNode == node)
            {
                EditorGUI.BeginChangeCheck();
                renamingInput = EditorGUI.TextField(rect, renamingInput);
                if (EditorGUI.EndChangeCheck())
                {
                    // 用户输入时暂不处理，等提交
                }

                Event e = Event.current;
                if (e.type == EventType.KeyDown)
                {
                    if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                    {
                        CommitRename();
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.Escape)
                    {
                        CancelRename();
                        e.Use();
                    }
                }else if (e.type == EventType.MouseDown && !rect.Contains(e.mousePosition))
                {
                    CommitRename(); // 或 CancelRename()
                    e.Use();
                }
            }
            else
            {
                // 绘制节点背景（可选：用按钮样式或自定义绘制）
                EditorGUI.LabelField(rect, node.Name, style);

                // ✅ 手动检测鼠标事件
                Event e = Event.current;
                if (rect.Contains(e.mousePosition))
                {
                    // 左键点击
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        HandleColumnClick(colIndex, i);
                        e.Use(); // 阻止事件冒泡
                    }
                    // 右键点击
                    else if (e.type == EventType.ContextClick || (e.type == EventType.MouseDown && e.button == 1))
                    {
                        ShowContextMenu(colIndex, i);
                        e.Use();
                    }
                }

                // 如果是叶子节点且被选中，显示编辑器
                if (isSelected && !node.IsContainer)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Value");
                    string newValue = EditorGUILayout.TextField(node.Value);
                    if (newValue != node.Value)
                    {
                        node.Value = newValue;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            GUI.color = defaultColor;
        }

        GUILayout.EndVertical();
    }

    private void ShowContextMenu(int colIndex, int itemIndex)
    {
        var node = columns[colIndex][itemIndex];
        GenericMenu menu = new GenericMenu();

        // ✅ 只有拥有 <key> 节点的项才能重命名（即：是 dict 的子项）
        if (node.XmlKeyNode != null)
        {
            menu.AddItem(new GUIContent("重命名"), false, () => RenameNode(node));
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("重命名"));
        }

        // 添加子节点（仅容器节点或根节点）
        if (node.IsContainer)
        {
            menu.AddItem(new GUIContent("添加"), false, () => ShowAddMenu(node));
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("添加"));
        }

        // 删除（不能删除根节点）
        if (node != rootNode)
        {
            menu.AddItem(new GUIContent("删除"), false, () => DeleteNode(node, colIndex, itemIndex));
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("删除"));
        }

        menu.ShowAsContext();
    }

    private void RenameNode(PlistTreeNode node)
    {
        if (node.XmlKeyNode == null) return;

        for (int col = 0; col < columns.Count; col++)
        {
            int idx = columns[col].IndexOf(node);
            if (idx >= 0)
            {
                renamingNode = node;
                renamingInput = node.Name;
                renamingColumnIndex = col;
                renamingItemIndex = idx;

                // ✅ 下一帧自动聚焦并选中文本
                EditorApplication.delayCall += () =>
                {
                    EditorGUI.FocusTextInControl($"rename_{col}_{idx}");
                };
                break;
            }
        }
    }

    private void ShowAddMenu(PlistTreeNode parentNode)
    {
        GenericMenu menu = new GenericMenu();

        // 添加不同类型的子节点
        menu.AddItem(new GUIContent("添加 String"), false, () => AddChildNode(parentNode, PlistNodeType.String, "string"));
        menu.AddItem(new GUIContent("添加 Integer"), false, () => AddChildNode(parentNode, PlistNodeType.Integer, "integer"));
        menu.AddItem(new GUIContent("添加 Boolean"), false, () => AddChildNode(parentNode, PlistNodeType.Boolean, "true"));
        menu.AddItem(new GUIContent("添加 Dict"), false, () => AddChildNode(parentNode, PlistNodeType.Dict, "dict"));
        menu.AddItem(new GUIContent("添加 Array"), false, () => AddChildNode(parentNode, PlistNodeType.Array, "array"));

        menu.ShowAsContext();
    }

    private void AddChildNode(PlistTreeNode parentNode, PlistNodeType type, string xmlType)
    {
        string name = "NewKey";
        string value = "";

        // 对于 dict/array，需要先添加 <key>，再添加值节点
        if (type == PlistNodeType.Dict || type == PlistNodeType.Array)
        {
            //name = EditorUtility.DisplayDialogEditText("添加子项", "输入键名：", "NewKey");
            if (string.IsNullOrEmpty(name)) return;

            // 创建 <key> 节点
            XmlElement keyElement = xmlDoc.CreateElement("key");
            keyElement.InnerText = name;

            // 创建值节点（dict/array）
            XmlElement valueElement = xmlDoc.CreateElement(xmlType);

            // 插入到父节点（必须是 dict）
            if (parentNode.XmlValueNode?.Name == "dict")
            {
                parentNode.XmlValueNode.AppendChild(keyElement);
                parentNode.XmlValueNode.AppendChild(valueElement);

                // 创建树节点
                var newNode = new PlistTreeNode
                {
                    Name = name,
                    NodeType = type,
                    Children = new List<PlistTreeNode>(),
                    XmlValueNode = valueElement,
                    XmlKeyNode = keyElement,
                    Parent = parentNode,
                    Path = $"{parentNode.Path}/{name}"
                };

                parentNode.Children.Add(newNode);

                // 如果当前列正在显示 parentNode，则刷新
                int colIndex = columns.FindIndex(col => col.Contains(parentNode));
                if (colIndex >= 0)
                {
                    RefreshColumnsFrom(colIndex);
                    // 自动选中新节点并进入重命名模式
                    int newIndex = columns[colIndex].Count - 1;
                    selectedIndices[colIndex] = newIndex;
                    RenameNode(newNode); // 自动进入重命名
                }
            }
        }
        else
        {
            // 叶子节点：string/integer/boolean
            if (parentNode.NodeType == PlistNodeType.Dict)
            {
                //name = EditorUtility.DisplayDialogEditText("添加子项", "输入键名：", "NewKey");
                if (string.IsNullOrEmpty(name)) return;

                value =  type == PlistNodeType.Integer ? "0" : "NewValue";

                // 创建 <key> + <value> 节点
                XmlElement keyElement = xmlDoc.CreateElement("key");
                keyElement.InnerText = name;

                XmlElement valueElement = xmlDoc.CreateElement(xmlType);
                valueElement.InnerText = value;

                parentNode.XmlValueNode?.AppendChild(keyElement);
                parentNode.XmlValueNode?.AppendChild(valueElement);

                // 创建树节点
                var newNode = new PlistTreeNode
                {
                    Name = name,
                    NodeType = type,
                    Value = value,
                    XmlValueNode = valueElement,
                    XmlKeyNode = keyElement,
                    Parent = parentNode,
                    Path = $"{parentNode.Path}/{name}"
                };

                parentNode.Children.Add(newNode);

                int colIndex = columns.FindIndex(col => col.Contains(parentNode));
                if (colIndex >= 0)
                {
                    RefreshColumnsFrom(colIndex);
                    int newIndex = columns[colIndex].Count - 1;
                    selectedIndices[colIndex] = newIndex;
                    RenameNode(newNode); // 自动重命名 key
                }
            }
            else if (parentNode.NodeType == PlistNodeType.Array)
            {
                value =  type == PlistNodeType.Integer ? "0" : "NewValue";

                XmlElement valueElement = xmlDoc.CreateElement(xmlType);
                valueElement.InnerText = value;

                parentNode.XmlValueNode?.AppendChild(valueElement);

                // 数组项名称为 [index]
                string itemName = $"[{parentNode.Children.Count}]";

                var newNode = new PlistTreeNode
                {
                    Name = itemName,
                    NodeType = type,
                    Value = value,
                    XmlValueNode = valueElement,
                    Parent = parentNode,
                    Path = $"{parentNode.Path}[{parentNode.Children.Count}]"
                };

                parentNode.Children.Add(newNode);

                int colIndex = columns.FindIndex(col => col.Contains(parentNode));
                if (colIndex >= 0)
                {
                    RefreshColumnsFrom(colIndex);
                    int newIndex = columns[colIndex].Count - 1;
                    selectedIndices[colIndex] = newIndex;
                    // 自动选中并聚焦值编辑器（你可以在 DrawColumn 中检测选中叶子节点时自动聚焦 TextField）
                }
            }
        }
    }

    private void DeleteNode(PlistTreeNode node, int colIndex, int itemIndex)
    {
        if (EditorUtility.DisplayDialog("确认删除", $"确定要删除 '{node.Name}' 吗？", "删除", "取消"))
        {
            // 从父节点移除
            if (node.Parent != null)
            {
                node.Parent.Children.Remove(node);

                // 从 XML 移除（移除 key + value 节点）
                if (node.XmlKeyNode != null && node.XmlValueNode != null)
                {
                    node.XmlKeyNode.ParentNode?.RemoveChild(node.XmlKeyNode);
                    node.XmlKeyNode.ParentNode?.RemoveChild(node.XmlValueNode);
                }
                else if (node.XmlValueNode != null) // array item
                {
                    node.XmlValueNode.ParentNode?.RemoveChild(node.XmlValueNode);
                }

                // 刷新从当前列开始
                RefreshColumnsFrom(colIndex);

                // 清除当前选中
                selectedIndices[colIndex] = -1;
            }
        }
    }

    // 从指定列开始刷新（重新构建该列及右侧所有列）
    private void RefreshColumnsFrom(int startColIndex)
    {
        // 清除 startColIndex 右侧所有列
        while (columns.Count > startColIndex + 1)
        {
            columns.RemoveAt(columns.Count - 1);
            selectedIndices.RemoveAt(selectedIndices.Count - 1);
        }

        // 重新构建 startColIndex 列（如果存在）
        if (startColIndex < columns.Count)
        {
            var parent = GetParentNodeOfColumn(startColIndex);
            if (parent != null)
            {
                columns[startColIndex] = new List<PlistTreeNode>(parent.Children);
                selectedIndices[startColIndex] = -1;
            }
        }

        Repaint();
    }

    // 获取某列的“父节点”——即上一列选中的节点
    private PlistTreeNode GetParentNodeOfColumn(int colIndex)
    {
        if (colIndex <= 0) return rootNode;

        for (int i = colIndex - 1; i >= 0; i--)
        {
            int selectedIndex = selectedIndices[i];
            if (selectedIndex >= 0 && selectedIndex < columns[i].Count)
            {
                return columns[i][selectedIndex];
            }
        }
        return rootNode;
    }

    private void HandleColumnClick(int colIndex, int itemIndex)
    {
        selectedIndices[colIndex] = itemIndex;

        while (columns.Count > colIndex + 1)
        {
            columns.RemoveAt(columns.Count - 1);
            selectedIndices.RemoveAt(selectedIndices.Count - 1);
        }

        var clickedNode = columns[colIndex][itemIndex];

        if (clickedNode.IsContainer && clickedNode.Children != null && clickedNode.Children.Count > 0)
        {
            columns.Add(new List<PlistTreeNode>(clickedNode.Children));
            selectedIndices.Add(-1);
        }
    }

    private void LoadPlist()
    {
        if (string.IsNullOrEmpty(xmlPath) || !File.Exists(xmlPath))
        {
            EditorUtility.DisplayDialog("Error", "Invalid file path.", "OK");
            return;
        }

        try
        {
            xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            XmlNode plistDict = xmlDoc.SelectSingleNode("//plist/dict") ?? xmlDoc.DocumentElement;

            rootNode = new PlistTreeNode
            {
                Name = "Root",
                NodeType = PlistNodeType.Root,
                Children = new List<PlistTreeNode>(),
                XmlValueNode = plistDict
            };

            ParsePlistNode(plistDict, rootNode, "Root");

            columns.Clear();
            selectedIndices.Clear();
            InitializeColumns();

            Repaint();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LoadPlist Error: {ex}");
            EditorUtility.DisplayDialog("Load Error", ex.Message, "OK");
        }
    }

    private void ParsePlistNode(XmlNode xmlNode, PlistTreeNode parent, string path)
    {
        if (xmlNode.Name == "dict")
        {
            ParseDict(xmlNode, parent, path);
        }
        else if (xmlNode.Name == "array")
        {
            ParseArray(xmlNode, parent, path);
        }
    }

    private void ParseDict(XmlNode dictNode, PlistTreeNode parent, string parentPath)
    {
        for (int i = 0; i < dictNode.ChildNodes.Count; i++)
        {
            XmlNode child = dictNode.ChildNodes[i];
            if (child.Name == "key")
            {
                string key = child.InnerText;
                XmlNode valueNode = GetNextSibling(child);
                if (valueNode == null) continue;

                string currentPath = $"{parentPath}/{key}";

                if (IsValueNode(valueNode))
                {
                    var node = new PlistTreeNode
                    {
                        Name = key,
                        NodeType = GetNodeType(valueNode.Name),
                        Value = valueNode.InnerText,
                        XmlValueNode = valueNode,
                        XmlKeyNode = child,
                        Parent = parent,
                        Path = currentPath
                    };
                    parent.Children.Add(node);
                }
                else if (valueNode.Name == "dict" || valueNode.Name == "array")
                {
                    var containerNode = new PlistTreeNode
                    {
                        Name = key,
                        NodeType = valueNode.Name == "dict" ? PlistNodeType.Dict : PlistNodeType.Array,
                        Children = new List<PlistTreeNode>(),
                        XmlValueNode = valueNode,
                        XmlKeyNode = child,
                        Parent = parent,
                        Path = currentPath
                    };
                    parent.Children.Add(containerNode);
                    ParsePlistNode(valueNode, containerNode, currentPath);
                }
            }
        }
    }

    private void ParseArray(XmlNode arrayNode, PlistTreeNode parent, string parentPath)
    {
        for (int i = 0; i < arrayNode.ChildNodes.Count; i++)
        {
            XmlNode itemNode = arrayNode.ChildNodes[i];
            string currentPath = $"{parentPath}[{i}]";

            if (IsValueNode(itemNode))
            {
                var node = new PlistTreeNode
                {
                    Name = $"[{i}]",
                    NodeType = GetNodeType(itemNode.Name),
                    Value = itemNode.InnerText,
                    XmlValueNode = itemNode,
                    Parent = parent,
                    Path = currentPath
                };
                parent.Children.Add(node);
            }
            else if (itemNode.Name == "dict")
            {
                var dictNode = new PlistTreeNode
                {
                    Name = $"[{i}]",
                    NodeType = PlistNodeType.Dict,
                    Children = new List<PlistTreeNode>(),
                    XmlValueNode = itemNode,
                    Parent = parent,
                    Path = currentPath
                };
                parent.Children.Add(dictNode);
                ParsePlistNode(itemNode, dictNode, currentPath);
            }
        }
    }

    private PlistNodeType GetNodeType(string xmlName)
    {
        switch (xmlName)
        {
            case "string": return PlistNodeType.String;
            case "integer": return PlistNodeType.Integer;
            case "real": return PlistNodeType.Real;
            case "true":
            case "false": return PlistNodeType.Boolean;
            case "date": return PlistNodeType.Date;
            case "dict": return PlistNodeType.Dict;
            case "array": return PlistNodeType.Array;
            default: return PlistNodeType.Unknown;
        }
    }

    private bool IsValueNode(XmlNode node)
    {
        return node.Name == "string" || node.Name == "integer" || node.Name == "real" ||
               node.Name == "true" || node.Name == "false" || node.Name == "date";
    }

    private XmlNode GetNextSibling(XmlNode node)
    {
        if (node?.ParentNode == null) return null;

        int index = -1;
        for (int i = 0; i < node.ParentNode.ChildNodes.Count; i++)
        {
            if (node.ParentNode.ChildNodes[i] == node)
            {
                index = i;
                break;
            }
        }

        if (index == -1 || index + 1 >= node.ParentNode.ChildNodes.Count)
        {
            return null;
        }

        return node.ParentNode.ChildNodes[index + 1];
    }

    private void SavePlist()
    {
        if (xmlDoc == null)
        {
            EditorUtility.DisplayDialog("Error", "No plist loaded.", "OK");
            return;
        }

        try
        {
            UpdateXmlFromTree(rootNode);
            SaveWithPlistHeader();
            EditorUtility.DisplayDialog("Success", "Plist saved successfully!", "OK");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SavePlist Error: {ex}");
            EditorUtility.DisplayDialog("Save Error", ex.Message, "OK");
        }
    }

    private void UpdateXmlFromTree(PlistTreeNode node)
    {
        if (node.XmlValueNode != null && !node.IsContainer)
        {
            node.XmlValueNode.InnerText = node.Value;
        }

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                UpdateXmlFromTree(child);
            }
        }
    }

    private void SaveWithPlistHeader()
    {
        StringBuilder sb = new StringBuilder();
        using (StringWriter sw = new StringWriter(sb))
        using (XmlTextWriter writer = new XmlTextWriter(sw))
        {
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 1;
            writer.IndentChar = '\t';
            xmlDoc.WriteTo(writer);
        }

        string xmlContent = sb.ToString();

        if (!xmlContent.Contains("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"))
        {
            xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + xmlContent;
        }

        if (!xmlContent.Contains("<!DOCTYPE plist"))
        {
            int insertIndex = xmlContent.IndexOf("?>") + 2;
            string doctype = "\n<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">";
            xmlContent = xmlContent.Insert(insertIndex, doctype);
        }

        File.WriteAllText(xmlPath, xmlContent, new UTF8Encoding(false));
    }
}

public enum PlistNodeType
{
    Root,
    Dict,
    Array,
    String,
    Integer,
    Real,
    Boolean,
    Date,
    Unknown
}

[System.Serializable]
public class PlistTreeNode
{
    public string Name;
    public PlistNodeType NodeType;
    public string Value;
    public List<PlistTreeNode> Children;
    public PlistTreeNode Parent; // 新增：指向父节点，便于删除和刷新
    public XmlNode XmlValueNode; // 值节点：<string>, <dict>, <array> 等
    public XmlNode XmlKeyNode;   // key 节点（仅 dict 子项有）
    public string Path;

    public bool IsContainer => NodeType == PlistNodeType.Dict || NodeType == PlistNodeType.Array || NodeType == PlistNodeType.Root;
}
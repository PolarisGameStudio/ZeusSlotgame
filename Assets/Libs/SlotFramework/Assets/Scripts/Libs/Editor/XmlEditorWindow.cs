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

    // 列数据：每列是一个节点列表
    private List<List<PlistTreeNode>> columns = new List<List<PlistTreeNode>>();
    // 每列选中的索引（-1 表示无选择）
    private List<int> selectedIndices = new List<int>();

    [MenuItem("Tools/Plist Column Editor")]
    public static void ShowWindow()
    {
        GetWindow<PlistColumnEditorWindow>("Plist Column Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // 文件选择
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

        // 初始化第一列
        if (columns.Count == 0 && rootNode != null)
        {
            InitializeColumns();
        }

        // 横向滚动区域
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 80), GUILayout.ExpandWidth(true));

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        // 绘制每一列
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

        // 第一列：根节点的子节点（隐藏 Root 本身）
        if (rootNode.Children != null && rootNode.Children.Count > 0)
        {
            columns.Add(new List<PlistTreeNode>(rootNode.Children));
            selectedIndices.Add(-1);
        }
    }

    private void DrawColumn(int colIndex)
    {
        var column = columns[colIndex];
        int selectedIndex = selectedIndices[colIndex];

        // 列容器
        GUILayout.BeginVertical("box", GUILayout.Width(200), GUILayout.ExpandHeight(true));

        EditorGUILayout.LabelField($"Level {colIndex + 1}", EditorStyles.boldLabel);

        for (int i = 0; i < column.Count; i++)
        {
            var node = column[i];
            bool isSelected = (i == selectedIndex);

            // 用不同样式表示选中
            GUIStyle style = isSelected ? EditorStyles.label : GUI.skin.GetStyle("Label");
            Color defaultColor = GUI.color;
            if (isSelected) GUI.color = new Color(0.8f, 0.9f, 1f); // 淡蓝色选中背景

            Rect rect = EditorGUILayout.GetControlRect(false, 22);
            EditorGUI.LabelField(rect, node.Name, style);
            GUI.color = defaultColor;

            // 点击处理
            if (GUI.Button(rect, "", GUIStyle.none))
            {
                HandleColumnClick(colIndex, i);
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

        GUILayout.EndVertical();
    }

    private void HandleColumnClick(int colIndex, int itemIndex)
    {
        // 选中当前项
        selectedIndices[colIndex] = itemIndex;

        // 清除右侧所有列
        while (columns.Count > colIndex + 1)
        {
            columns.RemoveAt(columns.Count - 1);
            selectedIndices.RemoveAt(selectedIndices.Count - 1);
        }

        var clickedNode = columns[colIndex][itemIndex];

        // 如果是容器节点，添加新列
        if (clickedNode.IsContainer && clickedNode.Children != null && clickedNode.Children.Count > 0)
        {
            columns.Add(new List<PlistTreeNode>(clickedNode.Children));
            selectedIndices.Add(-1); // 新列默认无选中
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
                Children = new List<PlistTreeNode>()
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
                    parent.Children.Add(new PlistTreeNode
                    {
                        Name = key,
                        NodeType = GetNodeType(valueNode.Name),
                        Value = valueNode.InnerText,
                        XmlValueNode = valueNode,
                        Path = currentPath
                    });
                }
                else if (valueNode.Name == "dict" || valueNode.Name == "array")
                {
                    var containerNode = new PlistTreeNode
                    {
                        Name = key,
                        NodeType = valueNode.Name == "dict" ? PlistNodeType.Dict : PlistNodeType.Array,
                        Children = new List<PlistTreeNode>(),
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
                parent.Children.Add(new PlistTreeNode
                {
                    Name = $"[{i}]",
                    NodeType = GetNodeType(itemNode.Name),
                    Value = itemNode.InnerText,
                    XmlValueNode = itemNode,
                    Path = currentPath
                });
            }
            else if (itemNode.Name == "dict")
            {
                var dictNode = new PlistTreeNode
                {
                    Name = $"[{i}]",
                    NodeType = PlistNodeType.Dict,
                    Children = new List<PlistTreeNode>(),
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
        if (node.XmlValueNode != null)
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

// 节点类型枚举
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

// 树节点类
[System.Serializable]
public class PlistTreeNode
{
    public string Name;
    public PlistNodeType NodeType;
    public string Value;
    public List<PlistTreeNode> Children;
    public XmlNode XmlValueNode;
    public string Path;

    public bool IsContainer => NodeType == PlistNodeType.Dict || NodeType == PlistNodeType.Array || NodeType == PlistNodeType.Root;
}
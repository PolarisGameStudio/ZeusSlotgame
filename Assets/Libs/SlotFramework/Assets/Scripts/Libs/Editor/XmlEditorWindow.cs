using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class PlistColumnEditorWindow : EditorWindow
{
      #region Member Variables

    private string xmlPath = "";
    private XmlDocument xmlDoc;
    private PlistTreeNode rootNode;

    // UI State
    private Vector2 mainScrollPos;
    private List<Vector2> columnScrollPositions = new List<Vector2>();
    private List<List<PlistTreeNode>> columns = new List<List<PlistTreeNode>>();
    private List<int> selectedIndices = new List<int>();

    // Key Renaming State
    private PlistTreeNode renamingNode;
    private string renamingInput = "";
    private string renameControlName = "";
    private bool isRenameJustInitiated = false;

    // Value Editing State
    private PlistTreeNode editingValueNode;
    private string editingValueInput = "";
    private string valueEditorControlName = "ValueTextField";
    private bool isValueEditorJustInitiated = false;

    // --- 新增：用于复制/粘贴功能的剪贴板 ---
    private static PlistTreeNode clipboardNode;
      #endregion

      #region Window Initialization

    [MenuItem("Tools/Plist Column Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlistColumnEditorWindow>("Plist Column Editor");
        string projectRoot = Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "");
        window.xmlPath = Path.Combine(projectRoot, "Assets/AssetResources/Resources/GameConfig.plist.xml");
        window.LoadPlist();
    }

      #endregion

      #region Unity GUI Methods

    private void OnGUI()
    {
        // --- Focus Loss Detection ---
        // Must be at the top of OnGUI to work reliably
        HandleFocusLossForRename();
        HandleFocusLossForValueEdit();

        // --- Top Bar Controls ---
        DrawTopBar();

        if (string.IsNullOrEmpty(xmlPath) || xmlDoc == null)
        {
            EditorGUILayout.HelpBox("Please select a valid .plist file.", MessageType.Info);
            return;
        }

        // --- Main Content Area ---
        EditorGUILayout.BeginVertical();

        // Main scroll view for columns
        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        for (int colIndex = 0; colIndex < columns.Count; colIndex++)
        {
            DrawColumn(colIndex);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();

        // Path display bar at the bottom
        DrawSelectedPath();

        EditorGUILayout.EndVertical();

        // Handle mouse wheel scrolling over columns
        HandleMouseWheelScroll();
    }

    private void DrawTopBar()
    {
        EditorGUILayout.Space();
        xmlPath = EditorGUILayout.TextField("Plist Path:", xmlPath);
        if (GUILayout.Button("Save to Plist"))
        {
            SavePlist();
        }
        EditorGUILayout.Space();
    }

    private void DrawColumn(int colIndex)
    {
        var column = columns[colIndex];
        int selectedIndex = selectedIndices[colIndex];

        // Ensure scroll position list is large enough
        while (colIndex >= columnScrollPositions.Count)
        {
            columnScrollPositions.Add(Vector2.zero);
        }

        GUILayout.BeginVertical("box", GUILayout.Width(220), GUILayout.ExpandHeight(true));
        EditorGUILayout.LabelField($"Level {colIndex + 1}", EditorStyles.boldLabel);

        // Scroll view for the column's content
        columnScrollPositions[colIndex] = EditorGUILayout.BeginScrollView(
            columnScrollPositions[colIndex],
            GUILayout.ExpandHeight(true)
        );

        for (int i = 0; i < column.Count; i++)
        {
            var node = column[i];
            bool isSelected = (i == selectedIndex);

            Color defaultColor = GUI.color;
            if (isSelected) GUI.backgroundColor = new Color(0.24f, 0.48f, 0.9f); // A nice blue selection color

            // Determine if this item is the one being renamed
            bool isRenamingThisNode = (renamingNode == node);

            // Get a rect for the list item
            Rect itemRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

            // Draw selection background if selected but not renaming
            if (isSelected && !isRenamingThisNode)
            {
                GUI.Box(itemRect, "");
            }

            GUI.backgroundColor = Color.white;


            // --- Draw Item Content ---
            if (isRenamingThisNode)
            {
                DrawRenameEditor(itemRect);
            }
            else
            {
                DrawItemLabel(itemRect, node, isSelected);
                HandleItemEvents(colIndex, i, itemRect);
            }

            // If this item is a selected leaf node, draw its value editor below it
            if (isSelected && !node.IsContainer)
            {
                DrawValueEditor(node);
            }

            GUI.color = defaultColor;
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawItemLabel(Rect rect, PlistTreeNode node, bool isSelected)
    {
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.normal.textColor = isSelected ? Color.white : EditorStyles.label.normal.textColor;

        // Draw left-aligned name
        GUI.Label(new Rect(rect.x, rect.y, rect.width - 60, rect.height), node.Name, labelStyle);

        // Draw right-aligned type
        GUIStyle typeStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight,
            normal =
            {
                textColor = isSelected ? new Color(0.8f, 0.8f, 0.8f) : Color.gray
            }
        };
        GUI.Label(new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height), GetDisplayTypeLabel(node), typeStyle);
    }

    private void DrawRenameEditor(Rect rect)
    {
        // 获取当前UI事件
        Event e = Event.current;

        // 检查：当此重命名输入框有焦点时，是否按下了回车键
        if (GUI.GetNameOfFocusedControl() == renameControlName &&
            e.type == EventType.KeyDown &&
            (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            // 如果是，则提交重命名
            CommitRename(); 
            e.Use(); // (最关键) 消费掉此事件，输入框和其他逻辑就不会再处理它
        }
    
        // 无论如何都绘制输入框，以便用户可以持续输入
        // (在CommitRename被调用后的下一帧，因为renamingNode为null，此函数将不会被调用)
        GUI.SetNextControlName(renameControlName);
        renamingInput = EditorGUI.TextField(rect, renamingInput);
    }

    private void DrawValueEditor(PlistTreeNode node)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Value");

        if (node.NodeType == PlistNodeType.Boolean)
        {
            bool boolValue = node.Value == "true";
            bool newBoolValue = EditorGUILayout.Toggle(boolValue);
            if (newBoolValue != boolValue)
            {
                node.Value = newBoolValue ? "true" : "false";
            }
        }
        else if (editingValueNode == node)
        {
            Event e = Event.current;

            // 检查：当此控件有焦点时，是否按下了回车键
            if (GUI.GetNameOfFocusedControl() == valueEditorControlName &&
                e.type == EventType.KeyDown && 
                (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                // 如果是，执行提交并取消选中的逻辑
                CommitValueAndDeselect();
                e.Use(); // (最关键的一步) 消费掉此事件，输入框就不会再收到它了
            }
            else
            {
                // 如果不是回车，就正常绘制输入框
                GUI.SetNextControlName(valueEditorControlName);
                editingValueInput = EditorGUILayout.TextField(editingValueInput);
            }
        }
        else
        {
            if (GUILayout.Button(node.Value, EditorStyles.textField))
            {
                StartValueEdit(node);
            }
        }

        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 提交当前正在编辑的值，并取消对该节点的选中（通过重新选中其父节点实现）。
    /// 这个函数专门由回车键事件触发。
    /// </summary>
    private void CommitValueAndDeselect()
    {
        if (editingValueNode == null || editingValueNode.Parent == null)
        {
            // 如果没有正在编辑的节点或它没有父节点，则只做常规提交
            CommitValueEdit();
            return;
        }

        // 1. 先保存必要的引用
        PlistTreeNode parentNode = editingValueNode.Parent;
        string newValue = editingValueInput;

        // 2. 提交值的修改
        if (newValue != editingValueNode.Value)
        {
            editingValueNode.Value = newValue;
        }

        // 3. 清理编辑状态（这会把 editingValueNode 设为 null）
        CancelValueEdit();

        // 4. 找到父节点在UI中的位置
        for (int col = 0; col < columns.Count; col++)
        {
            int parentIdx = columns[col].IndexOf(parentNode);
            if (parentIdx != -1)
            {
                // 5. 找到了！“重新点击”父节点，刷新UI到父节点状态
                HandleColumnClick(col, parentIdx);
                break; 
            }
        }
    }

    private void DrawSelectedPath()
    {
        List<string> pathParts = new List<string>();
        PlistTreeNode lastSelectedNode = null;

        for (int i = 0; i < columns.Count; i++)
        {
            int selectedIndex = selectedIndices[i];
            if (selectedIndex >= 0 && selectedIndex < columns[i].Count)
            {
                PlistTreeNode selectedNode = columns[i][selectedIndex];
                pathParts.Add(selectedNode.Name);
                lastSelectedNode = selectedNode;
            }
            else
            {
                break;
            }
        }

        if (pathParts.Count > 0)
        {
            string fullPath = string.Join(" > ", pathParts);

            if (lastSelectedNode != null && !lastSelectedNode.IsContainer)
            {
                fullPath += $": <b>{lastSelectedNode.Value}</b>";
            }

            EditorGUILayout.Space(5);
            GUIStyle pathStyle = new GUIStyle(EditorStyles.helpBox)
            {
                richText = true
            };
            EditorGUILayout.LabelField(fullPath, pathStyle);
        }
    }

      #endregion

      #region Event Handling

    private void HandleItemEvents(int colIndex, int itemIndex, Rect itemRect)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && itemRect.Contains(e.mousePosition))
        {
            if (e.button == 0) // Left click
            {
                HandleColumnClick(colIndex, itemIndex);
                e.Use();
            }
            else if (e.button == 1) // Right click
            {
                // Select the item first, then show the context menu
                HandleColumnClick(colIndex, itemIndex);
                ShowContextMenu(colIndex, itemIndex);
                e.Use();
            }
        }
    }

    private void HandleColumnClick(int colIndex, int itemIndex)
    {
        // If clicking the same item, do nothing
        if (selectedIndices.Count > colIndex && selectedIndices[colIndex] == itemIndex)
        {
            GUI.FocusControl(null); // Deselect text fields if any
            return;
        }

        // A different item was clicked, so cancel any active editing
        CommitRename();
        CommitValueEdit();

        selectedIndices[colIndex] = itemIndex;

        // Trim columns to the right of the current one
        while (columns.Count > colIndex + 1)
        {
            columns.RemoveAt(columns.Count - 1);
            selectedIndices.RemoveAt(selectedIndices.Count - 1);
            columnScrollPositions.RemoveAt(columnScrollPositions.Count - 1);
        }

        var clickedNode = columns[colIndex][itemIndex];
        if (clickedNode.IsContainer && clickedNode.Children != null && clickedNode.Children.Count > 0)
        {
            columns.Add(new List<PlistTreeNode>(clickedNode.Children));
            selectedIndices.Add(-1);
            columnScrollPositions.Add(Vector2.zero);
        }

        GUI.FocusControl(null);
        Repaint();
    }

        private void ShowContextMenu(int colIndex, int itemIndex)
    {
        var node = columns[colIndex][itemIndex];
        GenericMenu menu = new GenericMenu();

        // --- 复制 ---
        // 任何节点都可以被复制
        menu.AddItem(new GUIContent("Copy"), false, () => CopyNode(node));

        // --- 粘贴 ---
        // 只有当剪贴板里有东西，并且当前节点是容器时，才允许粘贴
        if (clipboardNode != null && node.IsContainer)
        {
            menu.AddItem(new GUIContent("Paste"), false, () => PasteNode(node));
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Paste"));
        }

        menu.AddSeparator(""); // 添加一条分割线

        // --- 重命名 ---
        if (node.XmlKeyNode != null)
        {
            menu.AddItem(new GUIContent("Rename"), false, () => RenameNode(node));
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Rename"));
        }

        // --- 添加 ---
        if (node.IsContainer)
        {
            menu.AddItem(new GUIContent("Add Item/String"), false, () => AddChildNode(node, PlistNodeType.String, "string"));
            menu.AddItem(new GUIContent("Add Item/Integer"), false, () => AddChildNode(node, PlistNodeType.Integer, "integer"));
            menu.AddItem(new GUIContent("Add Item/Boolean"), false, () => AddChildNode(node, PlistNodeType.Boolean, "true"));
            menu.AddItem(new GUIContent("Add Item/Dictionary"), false, () => AddChildNode(node, PlistNodeType.Dict, "dict"));
            menu.AddItem(new GUIContent("Add Item/Array"), false, () => AddChildNode(node, PlistNodeType.Array, "array"));
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Add Item"));
        }
        
        // --- 删除 ---
        if (node != rootNode)
        {
            menu.AddItem(new GUIContent("Delete"), false, () => DeleteNode(node));
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Delete"));
        }

        menu.ShowAsContext();
    }

    /// <summary>
    /// 将一个节点放入剪贴板。
    /// </summary>
    private void CopyNode(PlistTreeNode node)
    {
        if (node == null) return;
        clipboardNode = node;
        Debug.Log($"Copied '{node.Name}' to clipboard.");
    }

    /// <summary>
    /// 将剪贴板中的节点深拷贝并粘贴到目标父节点下。
    /// </summary>
    private void PasteNode(PlistTreeNode destinationParent)
    {
        if (clipboardNode == null || !destinationParent.IsContainer) return;

        // 1. 深拷贝XML节点结构
        // ImportNode(node, true) 是一个非常强大的功能，可以完美地递归复制整个XML片段
        XmlElement clonedXmlValueNode = (XmlElement)xmlDoc.ImportNode(clipboardNode.XmlValueNode, true);

        // 2. 递归地创建新的PlistTreeNode数据结构来匹配新的XML结构
        PlistTreeNode pastedNode = RebuildTreeFromXml(clonedXmlValueNode, clipboardNode);
        pastedNode.Parent = destinationParent;

        // 3. 处理粘贴到不同容器类型的逻辑
        if (destinationParent.NodeType == PlistNodeType.Dict || destinationParent.NodeType == PlistNodeType.Root)
        {
            // 粘贴到字典时，需要生成一个新的、唯一的Key
            pastedNode.Name = GenerateUniqueKey(destinationParent);
            
            XmlElement keyElement = xmlDoc.CreateElement("key");
            keyElement.InnerText = pastedNode.Name;
            pastedNode.XmlKeyNode = keyElement;
            
            destinationParent.XmlValueNode.AppendChild(keyElement);
            destinationParent.XmlValueNode.AppendChild(clonedXmlValueNode);
        }
        else if (destinationParent.NodeType == PlistNodeType.Array)
        {
            // 粘贴到数组时，名称就是索引
            pastedNode.Name = $"[{destinationParent.Children.Count}]";
            destinationParent.XmlValueNode.AppendChild(clonedXmlValueNode);
        }

        // 4. 更新数据模型并刷新UI
        destinationParent.Children.Add(pastedNode);
        ForceRefreshAndSelect(destinationParent, pastedNode);
    }
    
    /// <summary>
    /// 一个辅助的递归函数，用于从一个已存在的XML片段重建PlistTreeNode的数据结构。
    /// </summary>
    /// <param name="currentXmlNode">当前正在处理的（新克隆的）XML值节点</param>
    /// <param name="templateNode">提供类型和结构参考的原始（被复制的）节点</param>
    /// <returns>一个新建的、与XML匹配的PlistTreeNode</returns>
    private PlistTreeNode RebuildTreeFromXml(XmlNode currentXmlNode, PlistTreeNode templateNode)
    {
        var newNode = new PlistTreeNode
        {
            Name = templateNode.Name, // Name将在PasteNode中被重写
            NodeType = templateNode.NodeType,
            Value = templateNode.Value,
            XmlValueNode = currentXmlNode,
        };

        if (newNode.IsContainer)
        {
            newNode.Children = new List<PlistTreeNode>();
            if (newNode.NodeType == PlistNodeType.Dict)
            {
                // 对于字典，我们需要成对地处理key和value
                for (int i = 0; i < currentXmlNode.ChildNodes.Count; i += 2)
                {
                    XmlNode keyNode = currentXmlNode.ChildNodes[i];
                    XmlNode valueNode = currentXmlNode.ChildNodes[i + 1];
                    PlistTreeNode templateChild = templateNode.Children.Find(c => c.Name == keyNode.InnerText);

                    var childNode = RebuildTreeFromXml(valueNode, templateChild);
                    childNode.Parent = newNode;
                    childNode.XmlKeyNode = keyNode;
                    newNode.Children.Add(childNode);
                }
            }
            else // Array
            {
                for (int i = 0; i < currentXmlNode.ChildNodes.Count; i++)
                {
                    XmlNode valueNode = currentXmlNode.ChildNodes[i];
                    PlistTreeNode templateChild = templateNode.Children[i];
                    
                    var childNode = RebuildTreeFromXml(valueNode, templateChild);
                    childNode.Parent = newNode;
                    newNode.Children.Add(childNode);
                }
            }
        }
        return newNode;
    }

    private void HandleFocusLossForRename()
    {
        if (renamingNode == null || isRenameJustInitiated) return;

        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            // 回车键的逻辑已经被移到 DrawRenameEditor 中了
            // 这里只处理 Escape 键
            if (e.keyCode == KeyCode.Escape)
            {
                CancelRename();
                e.Use();
            }
        }
        // 当焦点离开输入框时，提交重命名的逻辑保持不变
        else if (GUI.GetNameOfFocusedControl() != renameControlName)
        {
            CommitRename();
        }

        if (isRenameJustInitiated) isRenameJustInitiated = false;
    }

    private void HandleFocusLossForValueEdit()
    {
        if (editingValueNode == null || isValueEditorJustInitiated) return;

        Event e = Event.current;
        if (e.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == valueEditorControlName)
        {
            
            if (e.keyCode == KeyCode.Escape)
            {
                CancelValueEdit();
                e.Use();
            }
        }
        else if (GUI.GetNameOfFocusedControl() != valueEditorControlName)
        {
            CommitValueEdit();
        }

        if (isValueEditorJustInitiated) isValueEditorJustInitiated = false;
    }

    private void HandleMouseWheelScroll()
    {
        Event e = Event.current;
        if (e.type != EventType.ScrollWheel) return;

        Vector2 mousePos = e.mousePosition;
        float currentX = 5f; // Start with a small offset for borders
        const float columnWidth = 220f;

        for (int i = 0; i < columns.Count; i++)
        {
            Rect columnRect = new Rect(currentX, 0, columnWidth, position.height);
            if (columnRect.Contains(mousePos))
            {
                if (i < columnScrollPositions.Count)
                {
                    columnScrollPositions[i] += e.delta * 5f; // Adjust scroll speed
                    e.Use();
                    Repaint();
                    break;
                }
            }
            currentX += columnWidth + 4f; // Add spacing
        }
    }

      #endregion

      #region Data Logic (CRUD)

    private void AddChildNode(PlistTreeNode parentNode, PlistNodeType type, string xmlType)
    {
        // --- 分支1：为 Dict 或 Root 添加子节点 ---
        if (parentNode.NodeType == PlistNodeType.Dict || parentNode.NodeType == PlistNodeType.Root)
        {
            string name = GenerateUniqueKey(parentNode);
            string value = "";

            // 创建 XML 节点...
            XmlElement keyElement = xmlDoc.CreateElement("key");
            keyElement.InnerText = name;
            XmlElement valueElement;
            if (type == PlistNodeType.Dict || type == PlistNodeType.Array)
            {
                valueElement = xmlDoc.CreateElement(xmlType);
            }
            else
            {
                if (type == PlistNodeType.Boolean) value = "true";
                else if (type == PlistNodeType.Integer) value = "0";
                else value = "NewValue";
                valueElement = xmlDoc.CreateElement(xmlType);
                valueElement.InnerText = value;
            }
            parentNode.XmlValueNode?.AppendChild(keyElement);
            parentNode.XmlValueNode?.AppendChild(valueElement);

            // 创建 PlistTreeNode...
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
            if (newNode.IsContainer) newNode.Children = new List<PlistTreeNode>();
            parentNode.Children.Add(newNode);

            ForceRefreshAndSelect(parentNode, newNode);
            RenameNode(newNode); // 在刷新并选中后，立即进入重命名
        }
        // --- 分支2：为 Array 添加子节点 ---
        else if (parentNode.NodeType == PlistNodeType.Array)
        {
            // (省略了对添加容器类型的警告)
            string value = "";
            if (type == PlistNodeType.Boolean) value = "true";
            else if (type == PlistNodeType.Integer) value = "0";
            else value = "NewValue";

            XmlElement valueElement = xmlDoc.CreateElement(xmlType);
            valueElement.InnerText = value;
            parentNode.XmlValueNode?.AppendChild(valueElement);

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

            ForceRefreshAndSelect(parentNode, newNode);
        }
    }

    /// <summary>
    /// 强制刷新UI，确保父节点被选中，并选中新创建的子节点。
    /// </summary>
    /// <param name="parentNode">刚刚被添加了子节点的父节点</param>
    /// <param name="newlyAddedNode">新创建的子节点</param>
    private void ForceRefreshAndSelect(PlistTreeNode parentNode, PlistTreeNode newlyAddedNode)
    {
        // 1. 找到父节点所在的列
        int parentColIndex = columns.FindIndex(col => col.Contains(parentNode));
        if (parentColIndex < 0) return; // 如果找不到父列，则不执行任何操作

        // 2. 确保父节点在UI上是选中状态
        selectedIndices[parentColIndex] = columns[parentColIndex].IndexOf(parentNode);

        // 3. 移除父节点右侧所有“过时”的列
        int childColIndex = parentColIndex + 1;
        while (columns.Count > childColIndex)
        {
            columns.RemoveAt(columns.Count - 1);
            selectedIndices.RemoveAt(selectedIndices.Count - 1);
            columnScrollPositions.RemoveAt(columnScrollPositions.Count - 1);
        }

        // 4. 如果父节点有子节点，则创建或更新子节点列
        if (parentNode.Children != null && parentNode.Children.Count > 0)
        {
            List<PlistTreeNode> newChildList = new List<PlistTreeNode>(parentNode.Children);
            columns.Add(newChildList);
            // 5. 在新的子节点列中，直接选中新添加的节点
            selectedIndices.Add(newChildList.IndexOf(newlyAddedNode));
            columnScrollPositions.Add(Vector2.zero);
        }

        // 6. 请求重绘以应用所有UI状态的更改
        Repaint();
    }

    private void DeleteNode(PlistTreeNode node)
    {
        if (node == null || node.Parent == null) return;

        if (!EditorUtility.DisplayDialog("Confirm Deletion", $"Are you sure you want to delete '{node.Name}'?", "Delete", "Cancel"))
        {
            return;
        }

        
        // 1. 找到父节点所在的列索引和行索引
        int parentColIndex = -1;
        int parentItemIndex = -1;
        for (int i = 0; i < columns.Count; i++)
        {
            int foundIndex = columns[i].IndexOf(node.Parent);
            if (foundIndex != -1)
            {
                parentColIndex = i;
                parentItemIndex = foundIndex;
                break;
            }
        }

        // 如果在UI上找不到父节点（理论上不应该发生），则完全重置界面以防万一
        if (parentColIndex == -1)
        {
            InitializeColumns();
            return;
        }

        // 2. 从数据模型（树结构和XML）中移除节点
        PlistTreeNode parentNode = node.Parent;
        parentNode.Children.Remove(node);
        node.XmlKeyNode?.ParentNode?.RemoveChild(node.XmlKeyNode);
        node.XmlValueNode?.ParentNode?.RemoveChild(node.XmlValueNode);


        // 3. 强制刷新UI，从父节点那一列开始
        // 确保父节点在UI上是选中状态
        selectedIndices[parentColIndex] = parentItemIndex;

        // 4. (最关键的一步) 裁剪掉父节点右侧的所有列。
        // 这样就强制清除了那个包含了已删除节点的旧的子列。
        int childColIndex = parentColIndex + 1;
        while (columns.Count > childColIndex)
        {
            columns.RemoveAt(columns.Count - 1);
            selectedIndices.RemoveAt(selectedIndices.Count - 1);
            columnScrollPositions.RemoveAt(columnScrollPositions.Count - 1);
        }

        // 5. 如果父节点还有子节点，则根据更新后的 Children 列表重新创建下一列
        if (parentNode.IsContainer && parentNode.Children != null && parentNode.Children.Count > 0)
        {
            columns.Add(new List<PlistTreeNode>(parentNode.Children));
            selectedIndices.Add(-1); // 在新的子列中，默认不选中任何项
            columnScrollPositions.Add(Vector2.zero);
        }

        // 6. 请求重绘，让Unity根据我们刚刚更新好的UI状态来绘制界面
        Repaint();
        
    }

    private void RenameNode(PlistTreeNode node)
    {
        if (node == null || node.XmlKeyNode == null) return;

        // Find the node's position to generate a unique control name
        for (int col = 0; col < columns.Count; col++)
        {
            int idx = columns[col].IndexOf(node);
            if (idx >= 0)
            {
                renamingNode = node;
                renamingInput = node.Name;
                renameControlName = $"rename_{col}_{idx}";
                isRenameJustInitiated = true;

                EditorApplication.delayCall += () =>
                {
                    EditorGUI.FocusTextInControl(renameControlName);
                    var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (textEditor != null) textEditor.SelectAll();
                };
                break;
            }
        }
    }

    private void CommitRename()
    {
        if (renamingNode == null) return;

        if (string.IsNullOrWhiteSpace(renamingInput) || renamingInput == renamingNode.Name)
        {
            CancelRename();
            return;
        }

        // Check for duplicate keys in the same parent
        bool isDuplicate = false;
        if (renamingNode.Parent != null && renamingNode.Parent.Children != null)
        {
            foreach (var sibling in renamingNode.Parent.Children)
            {
                if (sibling != renamingNode && sibling.Name == renamingInput)
                {
                    isDuplicate = true;
                    break;
                }
            }
        }

        if (isDuplicate)
        {
            EditorUtility.DisplayDialog("Invalid Name", "Another item with the same name already exists at this level.", "OK");
            // Do not cancel rename, let the user fix it
        }
        else
        {
            renamingNode.Name = renamingInput;
            if (renamingNode.XmlKeyNode != null)
            {
                renamingNode.XmlKeyNode.InnerText = renamingInput;
            }
            CancelRename();
        }
    }

    private void CancelRename()
    {
        renamingNode = null;
        renamingInput = "";
        renameControlName = "";
        GUI.FocusControl(null);
        Repaint();
    }

    private void StartValueEdit(PlistTreeNode node)
    {
        if (node.NodeType == PlistNodeType.Boolean) return;
        editingValueNode = node;
        editingValueInput = node.Value;
        isValueEditorJustInitiated = true;

        EditorApplication.delayCall += () =>
        {
            GUI.FocusControl(valueEditorControlName);
            var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (textEditor != null) textEditor.SelectAll();
        };
    }

    private void CommitValueEdit()
    {
        if (editingValueNode != null)
        {
            if (editingValueInput != editingValueNode.Value)
            {
                editingValueNode.Value = editingValueInput;
            }
        }
        CancelValueEdit();
    }

    private void CancelValueEdit()
    {
        editingValueNode = null;
        editingValueInput = "";
        GUI.FocusControl(null);
        Repaint();
    }

      #endregion

      #region File I/O and Parsing

    private void LoadPlist()
    {
        if (string.IsNullOrEmpty(xmlPath) || !File.Exists(xmlPath))
        {
            xmlDoc = null;
            rootNode = null;
            InitializeColumns();
            return;
        }

        try
        {
            xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlNode plistDict = xmlDoc.SelectSingleNode("//plist/dict");

            if (plistDict == null) throw new System.Exception("Could not find <plist> -> <dict> root structure.");

            rootNode = new PlistTreeNode
            {
                Name = "Root",
                NodeType = PlistNodeType.Root,
                Children = new List<PlistTreeNode>(),
                XmlValueNode = plistDict
            };

            ParseDictChildren(plistDict, rootNode, "Root");
            InitializeColumns();
            Repaint();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Plist Load Error: {ex}");
            EditorUtility.DisplayDialog("Load Error", ex.Message, "OK");
            xmlDoc = null;
            rootNode = null;
        }
    }

    private void ParseDictChildren(XmlNode dictNode, PlistTreeNode parent, string parentPath)
    {
        XmlNode keyNode = dictNode.FirstChild;
        while (keyNode != null)
        {
            if (keyNode.Name == "key")
            {
                string key = keyNode.InnerText;
                XmlNode valueNode = keyNode.NextSibling;
                if (valueNode == null) break;

                ParseNode(key, valueNode, keyNode, parent, parentPath);
                keyNode = valueNode.NextSibling;
            }
            else
            {
                keyNode = keyNode.NextSibling;
            }
        }
    }

    private void ParseArrayChildren(XmlNode arrayNode, PlistTreeNode parent, string parentPath)
    {
        for (int i = 0; i < arrayNode.ChildNodes.Count; i++)
        {
            XmlNode itemNode = arrayNode.ChildNodes[i];
            string key = $"[{i}]";
            ParseNode(key, itemNode, null, parent, parentPath);
        }
    }

    private void ParseNode(string key, XmlNode valueNode, XmlNode keyNode, PlistTreeNode parent, string parentPath)
    {
        PlistNodeType type = GetNodeType(valueNode.Name);
        string currentPath = (keyNode != null) ? $"{parentPath}/{key}" : $"{parentPath}{key}";

        var node = new PlistTreeNode
        {
            Name = key,
            NodeType = type,
            XmlValueNode = valueNode,
            XmlKeyNode = keyNode,
            Parent = parent,
            Path = currentPath
        };

        if (type == PlistNodeType.Dict || type == PlistNodeType.Array)
        {
            node.Children = new List<PlistTreeNode>();
            if (type == PlistNodeType.Dict) ParseDictChildren(valueNode, node, currentPath);
            else ParseArrayChildren(valueNode, node, currentPath);
        }
        else
        {
            node.Value = (type == PlistNodeType.Boolean) ? valueNode.Name : valueNode.InnerText;
        }
        parent.Children.Add(node);
    }

    private void SavePlist()
    {
        if (xmlDoc == null)
        {
            EditorUtility.DisplayDialog("Error", "No plist loaded to save.", "OK");
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
            if (node.NodeType == PlistNodeType.Boolean)
            {
                if (node.Value != "true" && node.Value != "false") node.Value = "false";
                if (node.XmlValueNode.Name != node.Value)
                {
                    XmlElement newElement = xmlDoc.CreateElement(node.Value);
                    node.XmlValueNode.ParentNode.ReplaceChild(newElement, node.XmlValueNode);
                    node.XmlValueNode = newElement;
                }
            }
            else
            {
                node.XmlValueNode.InnerText = node.Value;
            }
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
        if (xmlDoc.DocumentType != null)
        {
            xmlDoc.RemoveChild(xmlDoc.DocumentType);
        }

        XmlDocumentType docType = xmlDoc.CreateDocumentType("plist", "-//Apple//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
        xmlDoc.InsertBefore(docType, xmlDoc.DocumentElement);

        XmlWriterSettings settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(false),
            Indent = true,
            IndentChars = "\t",
            NewLineChars = "\n"
        };

        using (XmlWriter writer = XmlWriter.Create(xmlPath, settings))
        {
            xmlDoc.Save(writer);
        }
    }

      #endregion

      #region Helpers

    private void InitializeColumns()
    {
        columns.Clear();
        selectedIndices.Clear();
        columnScrollPositions.Clear();
        if (rootNode != null)
        {
            columns.Add(new List<PlistTreeNode>
            {
                rootNode
            });
            selectedIndices.Add(-1);
            columnScrollPositions.Add(Vector2.zero);
        }
        Repaint();
    }

    private string GenerateUniqueKey(PlistTreeNode parentNode)
    {
        string baseName = "NewKey";
        string newKey = baseName;
        int counter = 1;

        var existingKeys = new HashSet<string>();
        if (parentNode.Children != null)
        {
            foreach (var child in parentNode.Children) existingKeys.Add(child.Name);
        }

        while (existingKeys.Contains(newKey))
        {
            newKey = $"{baseName} ({counter++})";
        }
        return newKey;
    }

    private string GetDisplayTypeLabel(PlistTreeNode node)
    {
        switch (node.NodeType)
        {
            case PlistNodeType.String: return "String";
            case PlistNodeType.Integer: return "Integer";
            case PlistNodeType.Real: return "Real";
            case PlistNodeType.Boolean: return "Boolean";
            case PlistNodeType.Date: return "Date";
            case PlistNodeType.Dict: return "Dictionary";
            case PlistNodeType.Array: return "Array";
            case PlistNodeType.Root: return "Root";
            default: return "Unknown";
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

      #endregion
}

  #region Data Structures

public enum PlistNodeType
{
    Root, Dict, Array, String, Integer, Real, Boolean, Date, Unknown
}

[System.Serializable]
public class PlistTreeNode
{
    public string Name;
    public PlistNodeType NodeType;
    public string Value;
    public List<PlistTreeNode> Children;
    public PlistTreeNode Parent;
    public XmlNode XmlValueNode;
    public XmlNode XmlKeyNode;
    public string Path;

    public bool IsContainer => NodeType == PlistNodeType.Dict || NodeType == PlistNodeType.Array || NodeType == PlistNodeType.Root;
}

  #endregion
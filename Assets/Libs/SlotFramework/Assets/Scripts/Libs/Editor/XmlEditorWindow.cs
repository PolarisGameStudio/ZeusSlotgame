using System;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class PlistColumnEditorWindow : EditorWindow
{
    #region Member Variables

    private string _xmlPath = "";
    private XmlDocument _xmlDoc;
    private PlistTreeNode _rootNode;
    
    // --- 新增：用于缓存GUI样式的变量，以提高性能 ---
    private GUIStyle _nameLabelStyle;
    private GUIStyle _typeLabelStyle;

    // UI State
    private Vector2 _mainScrollPos;
    private readonly List<Vector2> _columnScrollPositions = new List<Vector2>();
    private readonly List<List<PlistTreeNode>> _columns = new List<List<PlistTreeNode>>();
    private readonly List<int> _selectedIndices = new List<int>();

    // Key Renaming State
    private PlistTreeNode _renamingNode;
    private string _renamingInput = "";
    private string _renameControlName = "";
    private bool _isRenameJustInitiated = false;

    // Value Editing State
    private PlistTreeNode _editingValueNode;
    private string _editingValueInput = "";
    private readonly string _valueEditorControlName = "ValueTextField";
    private bool _isValueEditorJustInitiated = false;
    
    // 在类的成员变量区域（靠近 _clipboardNode）添加：
    private static bool _isClipboardCutMode = false;

    // --- 新增：用于复制/粘贴功能的剪贴板 ---
    private static PlistTreeNode _clipboardNode;
    
    // --- Search Functionality ---
    private string _searchQuery = "";
    private List<PlistTreeNode> _searchResults = new List<PlistTreeNode>();
    private int _currentSearchIndex = -1; // 当前选中的搜索结果索引（0-based）
    private bool _isSearchActive = false;

    #region Drag and Drop State

    private bool _isDragging = false;
    private int _dragSourceCol = -1;
    private int _dragSourceIndex = -1;
    private PlistTreeNode _draggedNode = null;
    private int _dropTargetCol = -1;
    private int _dropTargetIndex = -1;

    #endregion

    #endregion

    #region Window Initialization

    [MenuItem("Libs/GameConfig编辑器")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlistColumnEditorWindow>("Plist Column Editor");
        string projectRoot = Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "");
        window._xmlPath = Path.Combine(projectRoot, "Assets/AssetResources/Resources/GameConfig.plist.xml");
        window.LoadPlist();
    }

    #endregion

    #region Unity GUI Methods

    private void OnGUI()
    {
        Event e = Event.current;
        if (_isDragging && e.rawType == EventType.MouseUp && e.button == 0)
        {
            HandleDrop();
            e.Use();
        }

        HandleFocusLossForRename();
        HandleFocusLossForValueEdit();

        // --- Top Bar Controls ---
        DrawTopBar();

        if (string.IsNullOrEmpty(_xmlPath) || _xmlDoc == null)
        {
            EditorGUILayout.HelpBox("Please select a valid .plist file.", MessageType.Info);
            return;
        }

        // --- Main Content Area ---
        EditorGUILayout.BeginVertical();

        // Main scroll view for columns
        _mainScrollPos = EditorGUILayout.BeginScrollView(_mainScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        for (int colIndex = 0; colIndex < _columns.Count; colIndex++)
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

        if (_isDragging)
        {
            EditorGUIUtility.AddCursorRect(new Rect(e.mousePosition.x - 10, e.mousePosition.y - 10, 20, 20), MouseCursor.MoveArrow);
            Repaint();
        }
    }
    
    /// <summary>
    /// 初始化用于绘制列表项的GUI样式，避免在OnGUI中重复创建。
    /// </summary>
    private void InitializeStyles()
    {
        // 如果样式已创建，则无需任何操作
        if (_nameLabelStyle != null) return;

        // 创建名称标签的样式
        _nameLabelStyle = new GUIStyle(EditorStyles.label)
        {
            // 垂直居中对齐，让文字看起来更舒服
            alignment = TextAnchor.MiddleLeft 
        };

        // 创建类型标签的样式
        _typeLabelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight,
            // 增加一点内边距，让它不至于贴着右边缘
            padding = new RectOffset(0, 5, 0, 0) 
        };
    }

    private void DrawTopBar()
    {
        EditorGUILayout.Space();

        // Plist 路径
        EditorGUILayout.BeginHorizontal();
        _xmlPath = EditorGUILayout.TextField("Plist Path:", _xmlPath);
        if (GUILayout.Button("Save to Plist", GUILayout.Width(120)))
        {
            SavePlist();
        }
        EditorGUILayout.EndHorizontal();

        // === 搜索功能：所有控件紧凑排列在一行 ===
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Search:", GUILayout.Width(50));
    
        GUI.SetNextControlName("SearchField");
        string newQuery = EditorGUILayout.TextField(_searchQuery, GUILayout.ExpandWidth(true), GUILayout.Width(200));

        // 回车搜索
        if (GUI.GetNameOfFocusedControl() == "SearchField" && Event.current.isKey &&
            (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
        {
            _searchQuery = newQuery;
            PerformSearch();
            Event.current.Use();
        }

        _searchQuery = newQuery;

        // 搜索按钮
        if (GUILayout.Button("Search", GUILayout.Width(60)))
        {
            PerformSearch();
        }

        // 清空按钮（仅当有内容时显示）
        if (!string.IsNullOrEmpty(_searchQuery))
        {
            if (GUILayout.Button("×", GUILayout.Width(24), GUILayout.Height(18)))
            {
                ClearSearch();
            }
        }
        
        if (_isSearchActive)
        {
            if (_searchResults.Count > 0)
            {
                if (GUILayout.Button("<", GUILayout.Width(30)))
                {
                    NavigateSearchResult(-1);
                }

                GUILayout.Label($"{_currentSearchIndex + 1}/{_searchResults.Count}", GUILayout.ExpandWidth(false));

                if (GUILayout.Button(">", GUILayout.Width(30)))
                {
                    NavigateSearchResult(1);
                }
            }
            else
            {
                // 无结果时显示简短提示（可选）
                GUILayout.Label("No matches", EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }
    
    private void PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(_searchQuery) || _rootNode == null)
        {
            ClearSearch();
            return;
        }

        _searchResults.Clear();
        _currentSearchIndex = -1;
        _isSearchActive = true;

        // 递归搜索所有节点（包括容器名和叶子值）
        SearchNodeRecursive(_rootNode, _searchQuery.Trim());

        if (_searchResults.Count > 0)
        {
            _currentSearchIndex = 0;
            HighlightSearchResult(_currentSearchIndex);
        }
        else
        {
            ClearSearchHighlights();
        }

        Repaint();
    }

    private void SearchNodeRecursive(PlistTreeNode node, string query)
    {
        if (node == null) return;

        // 匹配名称（Key）或值（如果是叶子）
        bool matches = node.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        if (!node.IsContainer)
        {
            matches |= node.Value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        if (matches)
        {
            _searchResults.Add(node);
        }

        // 递归子节点
        if (node.IsContainer && node.Children != null)
        {
            foreach (var child in node.Children)
            {
                SearchNodeRecursive(child, query);
            }
        }
    }

    private void ClearSearch()
    {
        _searchQuery = "";
        _searchResults.Clear();
        _currentSearchIndex = -1;
        _isSearchActive = false;
        ClearSearchHighlights();
        Repaint();
    }

    private void ClearSearchHighlights()
    {
        _searchResults.Clear();
        _currentSearchIndex = -1;
    }

    private void DrawColumn(int colIndex)
    {
        var column = _columns[colIndex];
        int selectedIndex = _selectedIndices[colIndex];

        // Ensure scroll position list is large enough
        while (colIndex >= _columnScrollPositions.Count)
        {
            _columnScrollPositions.Add(Vector2.zero);
        }

        GUILayout.BeginVertical("box", GUILayout.Width(220), GUILayout.ExpandHeight(true));
        EditorGUILayout.LabelField($"Level {colIndex + 1}", EditorStyles.boldLabel);

        // Scroll view for the column's content
        _columnScrollPositions[colIndex] = EditorGUILayout.BeginScrollView(
            _columnScrollPositions[colIndex],
            GUILayout.ExpandHeight(true)
        );

        for (int i = 0; i < column.Count; i++)
        {
            var node = column[i];
            bool isSelected = (i == selectedIndex);

            Color defaultColor = GUI.color;
            if (isSelected) GUI.backgroundColor = new Color(0.24f, 0.48f, 0.9f); // A nice blue selection color

            // Determine if this item is the one being renamed
            bool isRenamingThisNode = (_renamingNode == node);

            // Get a rect for the list item
            Rect itemRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

            HandleItemInteractions(colIndex, i, itemRect, node);

            if (_isDragging && _dropTargetCol == colIndex && _dropTargetIndex == i)
            {
                Rect indicatorRect = new Rect(itemRect.x, itemRect.y - 1, itemRect.width, 2);
                EditorGUI.DrawRect(indicatorRect, new Color(0.24f, 0.48f, 0.9f));
            }

            if (i == column.Count - 1 && _isDragging && _dropTargetCol == colIndex && _dropTargetIndex == column.Count)
            {
                Rect indicatorRect = new Rect(itemRect.x, itemRect.yMax - 1, itemRect.width, 2);
                EditorGUI.DrawRect(indicatorRect, new Color(0.24f, 0.48f, 0.9f));
            }
            
            // 1. (新增) 在这里也计算出 isSearchHighlight 状态
            bool isSearchHighlight = _isSearchActive && 
                _currentSearchIndex >= 0 && 
                _currentSearchIndex < _searchResults.Count &&
                _searchResults[_currentSearchIndex] == node;
            // 1. 如果当前项被选中，并且不是正在重命名的那一项，就绘制自定义的背景色
            if (isSelected && !isRenamingThisNode&&!isSearchHighlight)
            {
                Color selectionColor = new Color(0.15f, 0.35f, 0.7f, 1f); // 一个比默认更深的蓝色
                EditorGUI.DrawRect(itemRect, selectionColor);
            }

            GUI.backgroundColor = Color.white;


            // --- Draw Item Content ---
            if (isRenamingThisNode)
            {
                DrawRenameEditor(itemRect);
            }
            else
            {
                bool isBeingDragged = (_isDragging && _draggedNode == node);
                if (isBeingDragged)
                {
                    var oldColor = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    DrawItemLabel(itemRect, node, isSelected);
                    GUI.color = oldColor;
                }
                else
                {
                    DrawItemLabel(itemRect, node, isSelected);
                }
            }

            // If this item is a selected leaf node, draw its value editor below it
            if (isSelected && !node.IsContainer)
            {
                DrawValueEditor(node);
            }

            GUI.color = defaultColor;
        }

        EditorGUILayout.EndScrollView();
        
        // --- 新增：检测列的空白区域点击 ---
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            // 获取整个列的区域
            Rect columnRect = GUILayoutUtility.GetLastRect();
            if (columnRect.Contains(e.mousePosition))
            {
                // 点击了列的空白区域
                if (_editingValueNode != null || _renamingNode != null)
                {
                    CommitRename();
                    CommitValueEdit();
                    e.Use();
                }
            }
        }
        
        GUILayout.EndVertical();
    }
    
    private void NavigateSearchResult(int direction)
    {
        if (_searchResults.Count == 0) return;

        _currentSearchIndex += direction;
        if (_currentSearchIndex < 0)
            _currentSearchIndex = _searchResults.Count - 1;
        else if (_currentSearchIndex >= _searchResults.Count)
            _currentSearchIndex = 0;

        HighlightSearchResult(_currentSearchIndex);
        Repaint();
    }

    private void HighlightSearchResult(int index)
    {
        if (index < 0 || index >= _searchResults.Count) return;

        var targetNode = _searchResults[index];
        NavigateToNode(targetNode);
    }

    private void NavigateToNode(PlistTreeNode node)
    {
        // 从根开始，逐级展开路径
        List<PlistTreeNode> path = new List<PlistTreeNode>();
        PlistTreeNode current = node;
        while (current != null)
        {
            path.Add(current);
            current = current.Parent;
        }
        path.Reverse(); // 现在 path[0] 是根，path[^1] 是目标节点

        // 重置列
        _columns.Clear();
        _selectedIndices.Clear();
        _columnScrollPositions.Clear();

        _columns.Add(new List<PlistTreeNode>(_rootNode.Children));
        _selectedIndices.Add(-1);
        _columnScrollPositions.Add(Vector2.zero);

        // 逐级展开
        for (int i = 1; i < path.Count; i++)
        {
            var parentNode = path[i - 1];
            var currentNode = path[i];
            int idx = parentNode.Children.IndexOf(currentNode);
            if (idx >= 0)
            {
                HandleColumnClick(i - 1, idx);
            }
            else
            {
                break; // 路径断裂
            }
        }
    }
    
    private static Texture2D _highlightTexture;


   
    /// <summary>
    /// 绘制列表中的单个项目，经过视觉和性能优化。
    /// </summary>
    private void DrawItemLabel(Rect rect, PlistTreeNode node, bool isSelected)
    {
        // 在第一次绘制时初始化所有需要的样式
        InitializeStyles();

        // --- 1. 确定当前项是否是搜索结果中正在高亮的那一项 ---
        bool isSearchHighlight = _isSearchActive && 
                                 _currentSearchIndex >= 0 && 
                                 _currentSearchIndex < _searchResults.Count &&
                                 _searchResults[_currentSearchIndex] == node;
        
        if (isSearchHighlight)
        {
            // 对于搜索高亮的项，我们绘制一个半透明的黄色背景
            // 使用 EditorGUI.DrawRect 比 GUI.DrawTexture 更高效
            EditorGUI.DrawRect(rect, new Color(1f, 0.9f, 0.4f, 0.3f)); // 更柔和的黄色
        }
        else if(isSelected)
        {
            // 对于选中的项，我们使用Unity默认的蓝色高亮背景
            // 注意：isSelected 的背景是在 DrawColumn 方法中通过 GUI.Box(itemRect, ""); 绘制的
            // 所以这里不需要再画了。
        }

        // --- 3. 确定文字颜色 ---
        // 默认颜色
        Color nameColor;
        Color typeColor;

        if (isSearchHighlight) 
        {
            // 搜索高亮时，使用更醒目的颜色
            // 在浅黄色背景上，深色文字更易读
            nameColor = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : Color.black; 
            typeColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.7f, 0.3f) : new Color(0.4f, 0.4f, 0.4f);
        }
        else
        {
            // 选中时，所有文字都变白色，以在蓝色背景上保持清晰
            nameColor = Color.white;
            typeColor = new Color(0.85f, 0.85f, 0.85f); // 类型的颜色稍微暗一点，以示区分
        }

        // --- 4. 绘制标签 ---

        // 设置名称颜色并绘制
        _nameLabelStyle.normal.textColor = nameColor;
        // 为“剪切”的节点提供视觉反馈
        bool isCut = _isClipboardCutMode && _clipboardNode == node;
        if (isCut)
        {
            var oldColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.5f); // 使其半透明
            GUI.Label(new Rect(rect.x + 3, rect.y, rect.width - 60, rect.height), node.Name, _nameLabelStyle);
            GUI.color = oldColor;
        }
        else
        {
            GUI.Label(new Rect(rect.x + 3, rect.y, rect.width - 60, rect.height), node.Name, _nameLabelStyle);
        }

        // 设置类型颜色并绘制
        _typeLabelStyle.normal.textColor = typeColor;
        GUI.Label(new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height), GetDisplayTypeLabel(node), _typeLabelStyle);
    }

    private void DrawRenameEditor(Rect rect)
    {
        // 获取当前UI事件
        Event e = Event.current;

        // 检查：当此重命名输入框有焦点时，是否按下了回车键
        if (GUI.GetNameOfFocusedControl() == _renameControlName &&
            e.type == EventType.KeyDown &&
            (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            // 如果是，则提交重命名
            CommitRename();
            e.Use(); // (最关键) 消费掉此事件，输入框和其他逻辑就不会再处理它
        }

        // 无论如何都绘制输入框，以便用户可以持续输入
        // (在CommitRename被调用后的下一帧，因为renamingNode为null，此函数将不会被调用)
        GUI.SetNextControlName(_renameControlName);
        _renamingInput = EditorGUI.TextField(rect, _renamingInput);
    }

    private void DrawValueEditor(PlistTreeNode node)
    {
        float oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 40f; // "Value" 大约占 40 像素，可根据需要调整

        EditorGUILayout.BeginHorizontal();
    
        // 使用 PropertyField 风格的标签，但不绑定 property
        EditorGUILayout.LabelField("Value", GUILayout.Width(EditorGUIUtility.labelWidth));

        if (node.NodeType == PlistNodeType.Boolean)
        {
            bool boolValue = node.Value == "true";
            bool newBoolValue = EditorGUILayout.Toggle(boolValue, GUILayout.ExpandWidth(false));
            if (newBoolValue != boolValue)
            {
                node.Value = newBoolValue ? "true" : "false";
            }
        }
        else if (_editingValueNode == node)
        {
            Event e = Event.current;

            // 检查：当此控件有焦点时，是否按下了回车键
            if (GUI.GetNameOfFocusedControl() == _valueEditorControlName &&
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
                GUI.SetNextControlName(_valueEditorControlName);
                _editingValueInput = EditorGUILayout.TextField(_editingValueInput);
            }
        }
        else
        {
            if (GUILayout.Button(node.Value, EditorStyles.textField, GUILayout.MinWidth(30)))
            {
                StartValueEdit(node);
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = oldLabelWidth; // 恢复原始值
    }
    private void DrawSelectedPath()
    {
        List<string> pathParts = new List<string>();
        PlistTreeNode lastSelectedNode = null;

        for (int i = 0; i < _columns.Count; i++)
        {
            int selectedIndex = _selectedIndices[i];
            if (selectedIndex >= 0 && selectedIndex < _columns[i].Count)
            {
                PlistTreeNode selectedNode = _columns[i][selectedIndex];
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

    private void HandleItemInteractions(int colIndex, int itemIndex, Rect itemRect, PlistTreeNode node)
    {
        Event e = Event.current;
        
        // --- 关键修改：在编辑状态下，也要处理点击事件来提交编辑 ---
        if (_renamingNode != null || _editingValueNode != null)
        {
            // 只处理鼠标点击事件
            if (e.type == EventType.MouseDown && e.button == 0 && itemRect.Contains(e.mousePosition))
            {
                // 检查点击的是否是正在编辑的节点
                bool isEditingThisNode = (_renamingNode == node) || (_editingValueNode == node);
                
                if (!isEditingThisNode)
                {
                    // 点击了其他节点，提交编辑并处理新的点击
                    CommitRename();
                    CommitValueEdit();
                    // 不要return，让下面的代码继续执行以处理新的选择
                }
                else
                {
                    // 点击的是正在编辑的节点，不做处理
                    return;
                }
            }
            else
            {
                // 不是点击事件，或不在这个item范围内，跳过
                return;
            }
        }
        
        bool canDrag = (node.Parent != null);

        if (canDrag)
        {
            if (e.type == EventType.MouseDown && e.button == 0 && itemRect.Contains(e.mousePosition))
            {
                _dragSourceCol = colIndex;
                _dragSourceIndex = itemIndex;
                _draggedNode = node;
            }

            if (e.type == EventType.MouseDrag && _draggedNode == node)
            {
                if (!_isDragging)
                {
                    _isDragging = true;
                    GUI.FocusControl(null);
                    CancelRename();
                    CancelValueEdit();
                }
                e.Use();
            }

            if (_isDragging && itemRect.Contains(e.mousePosition))
            {
                if (_dragSourceCol == colIndex)
                {
                    _dropTargetCol = colIndex;
                    if (e.mousePosition.y < itemRect.y + itemRect.height / 2)
                    {
                        _dropTargetIndex = itemIndex;
                    }
                    else
                    {
                        _dropTargetIndex = itemIndex + 1;
                    }
                }
            }
        }

        if (e.type == EventType.MouseDown && itemRect.Contains(e.mousePosition) && !_isDragging)
        {
            if (e.button == 0)
            {
                HandleColumnClick(colIndex, itemIndex);
                e.Use();
            }
            else if (e.button == 1) // Right click
            {
                HandleColumnClick(colIndex, itemIndex);
                ShowContextMenu(colIndex, itemIndex);
                e.Use();
            }
        }
    }

    private void HandleDrop()
    {
        if (!_isDragging) return;

        bool reordered = false;
        if (_dropTargetCol != -1 && _dropTargetIndex != -1 && _dragSourceCol == _dropTargetCol)
        {
            // 在拖拽过程中，目标索引可能会因为被拖拽项的移除而改变
            // 例如 [A, B, C], 拖B到C后 (index 2 -> 3), toIndex是3
            // 但B移除后列表为[A, C], toIndex应为2.
            int finalToIndex = _dropTargetIndex;
            if (_dragSourceIndex < _dropTargetIndex)
            {
                finalToIndex--;
            }

            if (_dragSourceIndex != finalToIndex)
            {
                PerformReorder(_dragSourceCol, _dragSourceIndex, finalToIndex);
                reordered = true;
            }
        }

        _isDragging = false;
        _draggedNode = null;
        _dragSourceCol = -1;
        _dragSourceIndex = -1;
        _dropTargetCol = -1;
        _dropTargetIndex = -1;

        if (reordered) Repaint();
    }

    private void PerformReorder(int colIndex, int fromIndex, int toIndex)
    {
        var listInUI = _columns[colIndex];
        if (fromIndex < 0 || fromIndex >= listInUI.Count || toIndex < 0 || toIndex > listInUI.Count)
        {
            Debug.LogError($"Reorder failed: Invalid indices. from: {fromIndex}, to: {toIndex}");
            return;
        }

        var nodeToMove = listInUI[fromIndex];
        var parentNode = nodeToMove.Parent;

        if (parentNode == null) return;

        // --- 1. PREPARE: 确定所有需要的节点，并且不修改任何东西 ---

        // 要移动的XML节点
        XmlNode parentXml = parentNode.XmlValueNode;
        XmlNode valueXml = nodeToMove.XmlValueNode;
        XmlNode keyXml = nodeToMove.XmlKeyNode;

        // 关键修复：在修改任何列表之前，先确定好参考节点（要插入的位置）
        // 如果 toIndex 是列表末尾，则 referenceNode 为 null，这会让 InsertBefore 表现为 AppendChild
        XmlNode referenceNode = null;
        if (toIndex < parentNode.Children.Count)
        {
            PlistTreeNode nodeAtTargetPosition = parentNode.Children[toIndex];
            referenceNode = nodeAtTargetPosition.XmlKeyNode ?? nodeAtTargetPosition.XmlValueNode;
        }

        // --- 2. VALIDATE ---
        // 执行健壮性检查，确保XML节点确实是预期的父节点的子节点
        if ((keyXml != null && keyXml.ParentNode != parentXml) || (valueXml.ParentNode != parentXml))
        {
            Debug.LogError("Reorder failed: XML parent mismatch. The internal data might be out of sync.");
            return;
        }

        // --- 3. EXECUTE ---
        // 现在所有引用都已安全获取，开始执行修改

        // a. 从XML文档中分离节点
        if (keyXml != null) parentXml.RemoveChild(keyXml);
        parentXml.RemoveChild(valueXml);

        // b. 修改内存中的C#列表顺序
        parentNode.Children.RemoveAt(fromIndex);
        parentNode.Children.Insert(toIndex, nodeToMove);

        // c. 将分离的XML节点重新插入到正确的位置
        // InsertBefore(newNode, null) 的行为等同于 AppendChild(newNode)
        if (keyXml != null)
        {
            parentXml.InsertBefore(keyXml, referenceNode);
        }
        parentXml.InsertBefore(valueXml, referenceNode);

        // --- 4. FINALIZE ---

        // 如果是数组，更新其显示名称（例如 "[0]", "[1]"）
        if (parentNode.NodeType == PlistNodeType.Array)
        {
            for (int i = 0; i < parentNode.Children.Count; i++)
            {
                var child = parentNode.Children[i];
                child.Name = $"[{i}]";
                // --- BUG FIX ---
                // Also update the path for the child and all its descendants
                RecursivelyUpdatePaths(child, parentNode.Path);
            }
        }

        // 更新UI列表并选中移动后的项
        _columns[colIndex] = new List<PlistTreeNode>(parentNode.Children);
        _selectedIndices[colIndex] = toIndex;
    }

    private void HandleColumnClick(int colIndex, int itemIndex)
    {
        // 如果正在编辑，不处理点击
        if (_renamingNode != null || _editingValueNode != null)
            return;
        
        // If clicking the same item, do nothing
        if (_selectedIndices.Count > colIndex && _selectedIndices[colIndex] == itemIndex)
        {
            GUI.FocusControl(null); // Deselect text fields if any
            return;
        }

        // A different item was clicked, so cancel any active editing
        CommitRename();
        CommitValueEdit();

        _selectedIndices[colIndex] = itemIndex;

        // Trim columns to the right of the current one
        while (_columns.Count > colIndex + 1)
        {
            _columns.RemoveAt(_columns.Count - 1);
            _selectedIndices.RemoveAt(_selectedIndices.Count - 1);
            _columnScrollPositions.RemoveAt(_columnScrollPositions.Count - 1);
        }

        var clickedNode = _columns[colIndex][itemIndex];
        if (clickedNode.IsContainer && clickedNode.Children != null && clickedNode.Children.Count > 0)
        {
            _columns.Add(new List<PlistTreeNode>(clickedNode.Children));
            _selectedIndices.Add(-1);
            _columnScrollPositions.Add(Vector2.zero);
        }

        GUI.FocusControl(null);
        Repaint();
    }

    // ... The rest of the file (CRUD, File I/O, Helpers) remains unchanged ...
    private void CommitValueAndDeselect()
    {
        if (_editingValueNode == null || _editingValueNode.Parent == null)
        {
            CommitValueEdit();
            return;
        }
        PlistTreeNode parentNode = _editingValueNode.Parent;
        string newValue = _editingValueInput;
        if (newValue != _editingValueNode.Value)
        {
            _editingValueNode.Value = newValue;
        }
        CancelValueEdit();
        for (int col = 0; col < _columns.Count; col++)
        {
            int parentIdx = _columns[col].IndexOf(parentNode);
            if (parentIdx != -1)
            {
                HandleColumnClick(col, parentIdx);
                break;
            }
        }
    }
    private void ShowContextMenu(int colIndex, int itemIndex)
    {
        var node = _columns[colIndex][itemIndex];
        GenericMenu menu = new GenericMenu();

        // --- 复制 ---
        // 任何节点都可以被复制
        menu.AddItem(new GUIContent("Copy"), false, () => CopyNode(node));
        
        // --- 剪切 ---
        // 不能剪切根节点
        if (node != _rootNode)
        {
            menu.AddItem(new GUIContent("Cut"), false, () => CutNode(node));
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Cut"));
        }

        // --- 粘贴 ---
        // 只有当剪贴板里有东西，并且当前节点是容器时，才允许粘贴
        if (_clipboardNode != null && node.IsContainer)
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
            // If it's a non-empty array, provide a simple "Add Item" that clones the first element
            if (node.NodeType == PlistNodeType.Array && node.Children != null && node.Children.Count > 0)
            {
                menu.AddItem(new GUIContent("Add Item"), false, () => AddClonedChildToArray(node));
            }
            // Otherwise (for Dicts, Root, and empty Arrays), show the full type list
            else
            {
                menu.AddItem(new GUIContent("Add Item/String"), false, () => AddChildNode(node, PlistNodeType.String, "string"));
                menu.AddItem(new GUIContent("Add Item/Integer"), false, () => AddChildNode(node, PlistNodeType.Integer, "integer"));
                menu.AddItem(new GUIContent("Add Item/Boolean"), false, () => AddChildNode(node, PlistNodeType.Boolean, "true"));
                menu.AddItem(new GUIContent("Add Item/Real"), false, () => AddChildNode(node, PlistNodeType.Real, "real"));
                menu.AddItem(new GUIContent("Add Item/Dictionary"), false, () => AddChildNode(node, PlistNodeType.Dict, "dict"));
                menu.AddItem(new GUIContent("Add Item/Array"), false, () => AddChildNode(node, PlistNodeType.Array, "array"));
            }
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Add Item"));
        }

        // --- 删除 ---
        if (node != _rootNode)
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
    /// 剪切一个节点：复制到剪贴板并从父节点中删除。
    /// </summary>
    private void CutNode(PlistTreeNode node)
    {
        if (node == null || node == _rootNode)
        {
            Debug.LogWarning("Cannot cut root node.");
            return;
        }

        // 先复制（用于粘贴）
        _clipboardNode = node;
        _isClipboardCutMode = true; // 标记为剪切模式（可选，当前逻辑不需要，但留作扩展）

        // 从父节点中删除
        var parent = node.Parent;
        if (parent == null)
        {
            Debug.LogError("Cut node has no parent.");
            return;
        }

        // 1. 从 XML 中移除 key 和 value 节点
        if (node.XmlKeyNode != null)
            parent.XmlValueNode.RemoveChild(node.XmlKeyNode);
        parent.XmlValueNode.RemoveChild(node.XmlValueNode);

        // 2. 从 C# 列表中移除
        parent.Children.Remove(node);

        // 3. 如果父节点是数组，更新后续子项的名称（如 [0], [1]...）
        if (parent.NodeType == PlistNodeType.Array)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                parent.Children[i].Name = $"[{i}]";
                RecursivelyUpdatePaths(parent.Children[i], parent.Path);
            }
        }

        // 4. 刷新 UI：回到父节点层级
        ForceRefreshAndSelect(parent, null);

        Debug.Log($"Cut node '{node.Name}' to clipboard.");
    }

    /// <summary>
    /// 将一个节点放入剪贴板。
    /// </summary>
    private void CopyNode(PlistTreeNode node)
    {
        if (node == null) return;
        _clipboardNode = node;
        Debug.Log($"Copied '{node.Name}' to clipboard.");
    }

    /// <summary>
    /// --- NEW ---
    /// Adds a new child to a non-empty array by cloning its first element.
    /// </summary>
    private void AddClonedChildToArray(PlistTreeNode parentArray)
    {
        if (parentArray == null || parentArray.NodeType != PlistNodeType.Array || parentArray.Children == null || parentArray.Children.Count == 0)
        {
            Debug.LogError("AddClonedChildToArray called on an invalid or empty array.");
            return;
        }

        PlistTreeNode templateNode = parentArray.Children[0];

        XmlNode clonedXmlValueNode = _xmlDoc.ImportNode(templateNode.XmlValueNode, true);

        PlistTreeNode newNode = RebuildTreeFromXml(clonedXmlValueNode, templateNode);

        if (newNode == null)
        {
            Debug.LogError("Failed to rebuild PlistTreeNode from cloned XML.");
            return;
        }

        newNode.Parent = parentArray;
        newNode.Name = $"[{parentArray.Children.Count}]";

        RecursivelyUpdatePaths(newNode, parentArray.Path);

        parentArray.XmlValueNode.AppendChild(clonedXmlValueNode);
        parentArray.Children.Add(newNode);

        ForceRefreshAndSelect(parentArray, newNode);
    }

    /// <summary>
    /// --- NEW ---
    /// Recursively updates the Path property for a node and all its children.
    /// </summary>
    private void RecursivelyUpdatePaths(PlistTreeNode node, string parentPath)
    {
        if (node == null) return;

        if (node.Parent != null && node.Parent.NodeType == PlistNodeType.Array)
        {
            node.Path = $"{parentPath}{node.Name}"; // e.g., "Root/MyArray[1]"
        }
        else
        {
            node.Path = $"{parentPath}/{node.Name}"; // e.g., "Root/MyDict/NewKey"
        }

        if (node.IsContainer && node.Children != null)
        {
            foreach (var child in node.Children)
            {
                RecursivelyUpdatePaths(child, node.Path);
            }
        }
    }

    /// <summary>
    /// 将剪贴板中的节点深拷贝并粘贴到目标父节点下。
    /// </summary>
    private void PasteNode(PlistTreeNode destinationParent)
    {
        if (_clipboardNode == null || !destinationParent.IsContainer) return;

        // 1. 深拷贝XML节点结构
        // ImportNode(node, true) 是一个非常强大的功能，可以完美地递归复制整个XML片段
        XmlElement clonedXmlValueNode = (XmlElement)_xmlDoc.ImportNode(_clipboardNode.XmlValueNode, true);

        // 2. 递归地创建新的PlistTreeNode数据结构来匹配新的XML结构
        PlistTreeNode pastedNode = RebuildTreeFromXml(clonedXmlValueNode, _clipboardNode);
        pastedNode.Parent = destinationParent;

        // 3. 处理粘贴到不同容器类型的逻辑
        if (destinationParent.NodeType == PlistNodeType.Dict || destinationParent.NodeType == PlistNodeType.Root)
        {
            // --- MODIFIED START ---
            // 粘贴到字典时，尝试保留原始名称，但如果冲突则确保其唯一性
            // pastedNode.Name 已经从 RebuildTreeFromXml 中获得了原始名称
            string uniqueName = GenerateUniqueKeyFromName(destinationParent, pastedNode.Name);
            pastedNode.Name = uniqueName; // 更新为唯一名称
            // --- MODIFIED END ---

            XmlElement keyElement = _xmlDoc.CreateElement("key");
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
        _isClipboardCutMode = false;
    }

    /// <summary>
    /// 一个辅助的递归函数，用于从一个已存在的XML片段重建PlistTreeNode的数据结构。
    /// </summary>
    /// <param name="currentXmlNode">当前正在处理的（新克隆的）XML值节点</param>
    /// <param name="templateNode">提供类型和结构参考的原始（被复制的）节点</param>
    /// <returns>一个新建的、与XML匹配的PlistTreeNode</returns>
    private PlistTreeNode RebuildTreeFromXml(XmlNode currentXmlNode, PlistTreeNode templateNode)
    {
        // 如果模板节点本身就是null，直接中断此分支的重建并报错。
        if (templateNode == null)
        {
            Debug.LogError("RebuildTreeFromXml 失败: templateNode 为 null。正在中止此分支的粘贴操作。");
            return null; // 返回null，防止上层函数处理一个无效的节点
        }

        var newNode = new PlistTreeNode
        {
            Name = templateNode.Name,
            NodeType = templateNode.NodeType,
            Value = templateNode.Value,
            XmlValueNode = currentXmlNode,
        };

        if (newNode.IsContainer)
        {
            newNode.Children = new List<PlistTreeNode>();
            if (newNode.NodeType == PlistNodeType.Dict)
            {
                for (int i = 0; i < currentXmlNode.ChildNodes.Count; i += 2)
                {
                    XmlNode keyNode = currentXmlNode.ChildNodes[i];
                    XmlNode valueNode = currentXmlNode.ChildNodes[i + 1];

                    // 查找对应的模板子节点
                    PlistTreeNode templateChild = templateNode.Children.Find(c => c.Name == keyNode.InnerText);

                    if (templateChild != null)
                    {
                        // 只有在找到模板时，才进行递归
                        var childNode = RebuildTreeFromXml(valueNode, templateChild);
                        if (childNode != null) // 确保递归调用没有失败
                        {
                            childNode.Parent = newNode;
                            childNode.XmlKeyNode = keyNode;
                            newNode.Children.Add(childNode);
                        }
                    }
                    else
                    {
                        // 如果找不到，打印一个警告，然后跳过这个子节点，而不是让整个程序崩溃
                        Debug.LogWarning($"在粘贴操作中，未能找到键为 '{keyNode.InnerText}' 的模板子节点。此子节点将被跳过。");
                    }
                }
            }
            else // Array
            {
                for (int i = 0; i < currentXmlNode.ChildNodes.Count; i++)
                {
                    XmlNode valueNode = currentXmlNode.ChildNodes[i];

                    // 对数组也增加安全检查，防止索引越界
                    if (i < templateNode.Children.Count)
                    {
                        PlistTreeNode templateChild = templateNode.Children[i];
                        var childNode = RebuildTreeFromXml(valueNode, templateChild);
                        if (childNode != null)
                        {
                            childNode.Parent = newNode;
                            newNode.Children.Add(childNode);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"克隆的XML比模板节点有更多的子节点（索引：{i}）。此子节点将被跳过。");
                    }
                }
            }
        }
        return newNode;
    }

    private void HandleFocusLossForRename()
    {
        if (_renamingNode == null || _isRenameJustInitiated) return;

        Event e = Event.current;
        // --- 新增：检测鼠标点击事件 ---
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            // 如果点击时焦点不在重命名框上，提交重命名
            if (GUI.GetNameOfFocusedControl() != _renameControlName)
            {
                CommitRename();
                e.Use();
            }
        }
        else if (e.type == EventType.KeyDown)
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
        else if (GUI.GetNameOfFocusedControl() != _renameControlName)
        {
            CommitRename();
        }

        if (_isRenameJustInitiated) _isRenameJustInitiated = false;
    }

    private void HandleFocusLossForValueEdit()
    {
        if (_editingValueNode == null || _isValueEditorJustInitiated) return;

        Event e = Event.current;
        // --- 新增：检测鼠标点击事件 ---
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            // 如果点击时焦点不在编辑框上，提交编辑
            if (GUI.GetNameOfFocusedControl() != _valueEditorControlName)
            {
                CommitValueEdit();
                e.Use();
            }
        }
        else if (e.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == _valueEditorControlName)
        {

            if (e.keyCode == KeyCode.Escape)
            {
                CancelValueEdit();
                e.Use();
            }
        }
        else if (GUI.GetNameOfFocusedControl() != _valueEditorControlName)
        {
            CommitValueEdit();
        }

        if (_isValueEditorJustInitiated) _isValueEditorJustInitiated = false;
    }

    private void HandleMouseWheelScroll()
    {
        Event e = Event.current;
        if (e.type != EventType.ScrollWheel) return;

        Vector2 mousePos = e.mousePosition;
        float currentX = 5f; // Start with a small offset for borders
        const float columnWidth = 220f;

        for (int i = 0; i < _columns.Count; i++)
        {
            Rect columnRect = new Rect(currentX, 0, columnWidth, position.height);
            if (columnRect.Contains(mousePos))
            {
                if (i < _columnScrollPositions.Count)
                {
                    _columnScrollPositions[i] += e.delta * 5f; // Adjust scroll speed
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
            XmlElement keyElement = _xmlDoc.CreateElement("key");
            keyElement.InnerText = name;
            XmlElement valueElement;
            if (type == PlistNodeType.Dict || type == PlistNodeType.Array)
            {
                valueElement = _xmlDoc.CreateElement(xmlType);
            }
            else
            {
                if (type == PlistNodeType.Boolean) value = "true";
                else if (type == PlistNodeType.Integer) value = "0";
                else if (type == PlistNodeType.Real) value = "0.0";
                else value = "NewValue";
                valueElement = _xmlDoc.CreateElement(xmlType);
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
            string value = ""; // 默认值为空字符串
            XmlElement valueElement;

            // --- 关键修复：区分容器和值类型 ---
            if (type == PlistNodeType.Dict || type == PlistNodeType.Array)
            {
                // 如果是添加容器，只创建空的XML元素
                valueElement = _xmlDoc.CreateElement(xmlType);
            }
            else
            {
                // 如果是添加值类型，才设置默认值并写入InnerText
                if (type == PlistNodeType.Boolean) value = "true";
                else if (type == PlistNodeType.Integer) value = "0";
                else if (type == PlistNodeType.Real) value = "0.0";
                else value = "NewValue";

                valueElement = _xmlDoc.CreateElement(xmlType);
                valueElement.InnerText = value;
            }

            parentNode.XmlValueNode?.AppendChild(valueElement);

            string itemName = $"[{parentNode.Children.Count}]";
            var newNode = new PlistTreeNode
            {
                Name = itemName,
                NodeType = type,
                Value = value, // 对于容器，这里的value是""，是正确的
                XmlValueNode = valueElement,
                Parent = parentNode,
                Path = $"{parentNode.Path}[{parentNode.Children.Count}]"
            };

            // 为新创建的容器初始化Children列表（这是上一个问题的修复）
            if (newNode.IsContainer)
            {
                newNode.Children = new List<PlistTreeNode>();
            }

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
        int parentColIndex = _columns.FindIndex(col => col.Contains(parentNode));
        if (parentColIndex < 0) return; // 如果找不到父列，则不执行任何操作

        // 2. 确保父节点在UI上是选中状态
        _selectedIndices[parentColIndex] = _columns[parentColIndex].IndexOf(parentNode);

        // 3. 移除父节点右侧所有“过时”的列
        int childColIndex = parentColIndex + 1;
        while (_columns.Count > childColIndex)
        {
            _columns.RemoveAt(_columns.Count - 1);
            _selectedIndices.RemoveAt(_selectedIndices.Count - 1);
            _columnScrollPositions.RemoveAt(_columnScrollPositions.Count - 1);
        }

        // 4. 如果父节点有子节点，则创建或更新子节点列
        if (parentNode.Children != null && parentNode.Children.Count > 0)
        {
            List<PlistTreeNode> newChildList = new List<PlistTreeNode>(parentNode.Children);
            _columns.Add(newChildList);
            // 5. 在新的子节点列中，直接选中新添加的节点
            _selectedIndices.Add(newChildList.IndexOf(newlyAddedNode));
            _columnScrollPositions.Add(Vector2.zero);
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
        for (int i = 0; i < _columns.Count; i++)
        {
            int foundIndex = _columns[i].IndexOf(node.Parent);
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

        // --- BUG FIX ---
        // If the parent is an array, re-index the remaining items
        if (parentNode.NodeType == PlistNodeType.Array)
        {
            for (int i = 0; i < parentNode.Children.Count; i++)
            {
                var child = parentNode.Children[i];
                child.Name = $"[{i}]";
                RecursivelyUpdatePaths(child, parentNode.Path);
            }
        }

        // 3. 强制刷新UI，从父节点那一列开始
        // 确保父节点在UI上是选中状态
        _selectedIndices[parentColIndex] = parentItemIndex;

        // 4. (最关键的一步) 裁剪掉父节点右侧的所有列。
        // 这样就强制清除了那个包含了已删除节点的旧的子列。
        int childColIndex = parentColIndex + 1;
        while (_columns.Count > childColIndex)
        {
            _columns.RemoveAt(_columns.Count - 1);
            _selectedIndices.RemoveAt(_selectedIndices.Count - 1);
            _columnScrollPositions.RemoveAt(_columnScrollPositions.Count - 1);
        }

        // 5. 如果父节点还有子节点，则根据更新后的 Children 列表重新创建下一列
        if (parentNode.IsContainer && parentNode.Children != null && parentNode.Children.Count > 0)
        {
            _columns.Add(new List<PlistTreeNode>(parentNode.Children));
            _selectedIndices.Add(-1); // 在新的子列中，默认不选中任何项
            _columnScrollPositions.Add(Vector2.zero);
        }

        // 6. 请求重绘，让Unity根据我们刚刚更新好的UI状态来绘制界面
        Repaint();

    }

    private void RenameNode(PlistTreeNode node)
    {
        if (node == null || node.XmlKeyNode == null) return;

        // Find the node's position to generate a unique control name
        for (int col = 0; col < _columns.Count; col++)
        {
            int idx = _columns[col].IndexOf(node);
            if (idx >= 0)
            {
                _renamingNode = node;
                _renamingInput = node.Name;
                _renameControlName = $"rename_{col}_{idx}";
                _isRenameJustInitiated = true;

                EditorApplication.delayCall += () =>
                {
                    EditorGUI.FocusTextInControl(_renameControlName);
                    var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (textEditor != null) textEditor.SelectAll();
                };
                break;
            }
        }
    }

    private void CommitRename()
    {
        if (_renamingNode == null) return;

        if (string.IsNullOrWhiteSpace(_renamingInput) || _renamingInput == _renamingNode.Name)
        {
            CancelRename();
            return;
        }

        // Check for duplicate keys in the same parent
        bool isDuplicate = false;
        if (_renamingNode.Parent != null && _renamingNode.Parent.Children != null)
        {
            foreach (var sibling in _renamingNode.Parent.Children)
            {
                if (sibling != _renamingNode && sibling.Name == _renamingInput)
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
            _renamingNode.Name = _renamingInput;
            if (_renamingNode.XmlKeyNode != null)
            {
                _renamingNode.XmlKeyNode.InnerText = _renamingInput;
            }
            CancelRename();
        }
    }

    private void CancelRename()
    {
        _renamingNode = null;
        _renamingInput = "";
        _renameControlName = "";
        GUI.FocusControl(null);
        Repaint();
    }

    private void StartValueEdit(PlistTreeNode node)
    {
        if (node.NodeType == PlistNodeType.Boolean) return;
        _editingValueNode = node;
        _editingValueInput = node.Value;
        _isValueEditorJustInitiated = true;

        EditorApplication.delayCall += () =>
        {
            GUI.FocusControl(_valueEditorControlName);
            var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (textEditor != null) textEditor.SelectAll();
        };
    }

    private void CommitValueEdit()
    {
        if (_editingValueNode != null)
        {
            if (_editingValueInput != _editingValueNode.Value)
            {
                _editingValueNode.Value = _editingValueInput;
            }
        }
        CancelValueEdit();
    }

    private void CancelValueEdit()
    {
        _editingValueNode = null;
        _editingValueInput = "";
        GUI.FocusControl(null);
        Repaint();
    }

    #endregion

    #region File I/O and Parsing

    private void LoadPlist()
    {
        if (string.IsNullOrEmpty(_xmlPath) || !File.Exists(_xmlPath))
        {
            _xmlDoc = null;
            _rootNode = null;
            InitializeColumns();
            return;
        }

        try
        {
            _xmlDoc = new XmlDocument();
            _xmlDoc.Load(_xmlPath);
            XmlNode plistDict = _xmlDoc.SelectSingleNode("//plist/dict");

            if (plistDict == null) throw new System.Exception("Could not find <plist> -> <dict> root structure.");

            _rootNode = new PlistTreeNode
            {
                Name = "Root",
                NodeType = PlistNodeType.Root,
                Children = new List<PlistTreeNode>(),
                XmlValueNode = plistDict
            };

            ParseDictChildren(plistDict, _rootNode, "Root");
            InitializeColumns();
            Repaint();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Plist Load Error: {ex}");
            EditorUtility.DisplayDialog("Load Error", ex.Message, "OK");
            _xmlDoc = null;
            _rootNode = null;
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
        if (_xmlDoc == null)
        {
            EditorUtility.DisplayDialog("Error", "No plist loaded to save.", "OK");
            return;
        }

        try
        {
            UpdateXmlFromTree(_rootNode);
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
                
                // 检查当前元素是否已经是正确的布尔元素
                string expectedElementName = node.Value; // "true" 或 "false"
                if (node.XmlValueNode.Name != expectedElementName)
                {
                    // 对于布尔值，需要创建自闭合元素
                    XmlElement newElement = _xmlDoc.CreateElement(expectedElementName);
                
                    // 对于布尔元素，不需要设置 InnerText，保持自闭合
                    // newElement.InnerText = node.Value; // 删除这行
                
                    if (node.XmlValueNode.ParentNode != null)
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
        if (_xmlDoc.DocumentType != null)
        {
            _xmlDoc.RemoveChild(_xmlDoc.DocumentType);
        }

        XmlDocumentType docType = _xmlDoc.CreateDocumentType("plist", "-//Apple//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
        _xmlDoc.InsertBefore(docType, _xmlDoc.DocumentElement);

        XmlWriterSettings settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(false),
            Indent = true,
            IndentChars = "\t",
            NewLineChars = "\n"
        };

        using XmlWriter writer = XmlWriter.Create(_xmlPath, settings);
        _xmlDoc.Save(writer);
    }

      #endregion

    #region Helpers

    private void InitializeColumns()
    {
        _columns.Clear();
        _selectedIndices.Clear();
        _columnScrollPositions.Clear();
        if (_rootNode != null)
        {
            _columns.Add(new List<PlistTreeNode>
            {
                _rootNode
            });
            _selectedIndices.Add(-1);
            _columnScrollPositions.Add(Vector2.zero);
        }
        Repaint();
    }

    private string GenerateUniqueKey(PlistTreeNode parentNode)
    {
        string baseName = "NewKey";
        return GenerateUniqueKeyFromName(parentNode, baseName);
    }

    /// <summary>
    /// --- NEW ---
    /// Generates a unique key for a new node within a parent, starting with a desired base name.
    /// If the base name is already taken, it appends a counter (e.g., "baseName (1)").
    /// </summary>
    /// <param name="parentNode">The parent node where the key must be unique.</param>
    /// <param name="baseName">The desired starting name for the key.</param>
    /// <returns>A unique key string.</returns>
    private string GenerateUniqueKeyFromName(PlistTreeNode parentNode, string baseName)
    {
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace MioHelper
{

    [CustomEditor(typeof(MioFetchSettings))]
    public class MioFetchSettingsEditor : Editor
    {
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        MioFetchTreeView m_TreeView;
        SearchField m_SearchField;
        Vector2 logScrollView;
        const string kSessionStateKeyPrefix = "MFT";

        MioFetchSettings asset
        {
            get { return (MioFetchSettings)target; }
        }

        void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            var treeViewState = new TreeViewState();
            var jsonState = SessionState.GetString(kSessionStateKeyPrefix + asset.GetInstanceID(), "");
            if (!string.IsNullOrEmpty(jsonState))
                JsonUtility.FromJsonOverwrite(jsonState, treeViewState);

            bool firstInit = m_MultiColumnHeaderState == null;
            var headerState = MioFetchTreeView.CreateDefaultMultiColumnHeaderState(160);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
            m_MultiColumnHeaderState = headerState;

            var multiColumnHeader = new MioFetchHeader(headerState);
            if (firstInit)
                multiColumnHeader.ResizeToFit();

            var treeModel = new TreeModel<MioFetchPath>(asset.paths);
            m_TreeView = new MioFetchTreeView(treeViewState, multiColumnHeader, treeModel);

            m_SearchField = new SearchField();

            m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
        }


        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            SessionState.SetString(kSessionStateKeyPrefix + asset.GetInstanceID(), JsonUtility.ToJson(m_TreeView.state));
        }

        void OnUndoRedoPerformed()
        {
            if (m_TreeView != null)
            {
                m_TreeView.treeModel.SetData(asset.paths);
                m_TreeView.Reload();
            }
        }

        void OnBeforeDroppingDraggedItems(IList<TreeViewItem> draggedRows)
        {
            Undo.RecordObject(asset, string.Format("Moving {0} Item{1}", draggedRows.Count, draggedRows.Count > 1 ? "s" : ""));
        }

        public override void OnInspectorGUI()
        {
            if (asset.autoRefresh)
            {
                Repaint();
            }

            GUILayout.Space(5f);
            ToolBar();
            GUILayout.Space(5f);

            const float topToolbarHeight = 20f;
            const float spacing = 2f;
            float totalHeight = m_TreeView.totalHeight + topToolbarHeight + 2 * spacing;
            Rect rect = GUILayoutUtility.GetRect(0, 10000, 0, totalHeight);
            Rect toolbarRect = new Rect(rect.x, rect.y, rect.width, topToolbarHeight);
            Rect multiColumnTreeViewRect = new Rect(rect.x, rect.y + topToolbarHeight + spacing, rect.width, rect.height - topToolbarHeight - 2 * spacing);
            SearchBar(toolbarRect);
            DoTreeView(multiColumnTreeViewRect);

            GUILayout.Space(5f);
            if (MioFetchCore.lastReport != null && MioFetchCore.lastReport.Length > 1)
            {
                float reportHeight = EditorStyles.label.CalcHeight(new GUIContent(MioFetchCore.lastReport), rect.width);
                logScrollView = EditorGUILayout.BeginScrollView(logScrollView, GUILayout.Height(reportHeight + 20f));
                EditorGUILayout.HelpBox(MioFetchCore.lastReport, MessageType.Info);
                EditorGUILayout.EndScrollView();
            }
        }

        void SearchBar(Rect rect)
        {
            m_TreeView.searchString = m_SearchField.OnGUI(rect, m_TreeView.searchString);
        }

        void DoTreeView(Rect rect)
        {
            m_TreeView.OnGUI(rect);
        }

        void ToolBar()
        {

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Refresh", "force MioFetch to scan for changed files immediately"), GUILayout.Width(80)))
                {
                    MioFetchCore.DoRefresh();
                }
                asset.autoRefresh = EditorGUILayout.ToggleLeft(new GUIContent("Auto refresh?", "if enabled, MioFetch will automatically download new files from the internet"), asset.autoRefresh, GUILayout.MaxWidth(90));

                EditorGUILayout.Space();
                GUILayout.Label("every", GUILayout.MaxWidth(36));
                asset.refreshInterval = System.Convert.ToDouble(Mathf.Clamp(EditorGUILayout.IntField(System.Convert.ToInt32(asset.refreshInterval), GUILayout.MaxWidth(30)), 5, 999));
                GUILayout.Label("sec.", GUILayout.MaxWidth(30));

                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                if (asset.autoRefresh)
                {
                    Rect rect = GUILayoutUtility.GetRect(50, 18, "TextField", GUILayout.MaxWidth(10000));
                    var progress = EditorApplication.timeSinceStartup - MioFetchCore.lastRefreshTime;
                    var progressPercent = progress / MioFetchSettings.Instance.refreshInterval;
                    EditorGUI.ProgressBar(rect, Mathf.Clamp01(System.Convert.ToSingle(progressPercent)), "Auto refresh in " + (MioFetchSettings.Instance.refreshInterval - progress).ToString("F0"));
                }
            }
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                var style = "miniButton";

                if (GUILayout.Button("Add New File", style))
                {
                    Undo.RecordObject(asset, "Add Item To Asset");

                    // Add item as child of selection
                    var selection = m_TreeView.GetSelection();
                    // TreeElement parent = (selection.Count == 1 ? m_TreeView.treeModel.Find(selection[0]) : null) ?? m_TreeView.treeModel.root;
                    TreeElement parent = m_TreeView.treeModel.root;
                    int depth = parent != null ? parent.depth + 1 : 0;
                    int id = m_TreeView.treeModel.GenerateUniqueID();
                    var element = new MioFetchPath("Item " + id, depth, id);
                    m_TreeView.treeModel.AddElement(element, parent, 0);

                    // Select newly created element
                    m_TreeView.SetSelection(new[] { id }, TreeViewSelectionOptions.RevealAndFrame);
                }

                if (GUILayout.Button("Remove Highlighted File(s)", style))
                {
                    Undo.RecordObject(asset, "Remove Item From Asset");
                    var selection = m_TreeView.GetSelection();
                    m_TreeView.treeModel.RemoveElements(selection);
                }
            }
        }

        internal class MioFetchHeader : MultiColumnHeader
        {
            Mode m_Mode;

            public enum Mode
            {
                LargeHeader,
                DefaultHeader,
                MinimumHeaderWithoutSorting
            }

            public MioFetchHeader(MultiColumnHeaderState state)
                : base(state)
            {
                mode = Mode.DefaultHeader;
            }

            public Mode mode
            {
                get
                {
                    return m_Mode;
                }
                set
                {
                    m_Mode = value;
                    switch (m_Mode)
                    {
                        case Mode.LargeHeader:
                            canSort = true;
                            height = 37f;
                            break;
                        case Mode.DefaultHeader:
                            canSort = true;
                            height = DefaultGUI.defaultHeight;
                            break;
                        case Mode.MinimumHeaderWithoutSorting:
                            canSort = false;
                            height = DefaultGUI.minimumHeight;
                            break;
                    }
                }
            }

            protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
            {
                // Default column header gui
                base.ColumnHeaderGUI(column, headerRect, columnIndex);
            }
        }

    }
}

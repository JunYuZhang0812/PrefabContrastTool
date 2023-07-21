using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PrefabContrastTool.Core;
using PrefabContrastTool.Prefab;
using _Color = System.Drawing.Color;
namespace PrefabContrastTool
{
    public partial class PrefabContrastTool:Form
    {
        ClonePrefabNode m_clonePrefab = Singleton<ClonePrefabNode>.Instance;
        DeletePrefabNode m_deletePrefab = Singleton<DeletePrefabNode>.Instance;
        private List<string> m_clientPathList = new List<string>();
        private bool m_initFinish = false;
        public PrefabContrastTool(string srcPath = null , string tarPath = null )
        {
            m_initFinish = false;
            Logic.m_isSvnCompare = false;
            if (srcPath != null && tarPath != null)
            {
                Logic.m_isSvnCompare = true;
            }
            InitializeComponent();
            InitPanel();
            if( Logic.m_isSvnCompare )
            {
                m_labelSvnClientPath.Visible = true;
                m_textClientPath.Visible = true;
                m_comClientPathList.Visible = true;
                //m_textClientPath.Text = Core.FileOP.Instance.ReadString("Path", "SVNClientPath");
                m_comSelectMode.SelectedIndex = (int)SelectFileModeEnum.SelectFiles;
                m_textSourcePath.Text = srcPath;
                m_textTargetPath.Text = tarPath;
                //m_btnBegin_Click(null,null);
                label2.Text = "旧版本路径:";
                label3.Text = "新版本路径:";
            }
            else
            {
                m_textClientPath.Visible = false;
                m_comClientPathList.Visible = false;
                m_labelSvnClientPath.Visible = false;
                label2.Text = "源文件路径:";
                label3.Text = "目标文件路径:";
            }
            m_initFinish = true;
        }
        private Dictionary<string, bool> m_extendNodeDic = new Dictionary<string, bool>();
        #region 初始化界面
        private void InitPanel()
        {
            InsertFileMode();
            FillClientPathList();
            m_textSourcePath.Text = Logic.SourceFilePath;
            m_textTargetPath.Text = Logic.TargetFilePath;
            m_togShowAll.Checked = Logic.ShowAll;
            m_togCheckActive.Checked = Logic.CheckActive;
            m_togCheckFont.Checked = Logic.CheckFont;
            m_togCheckImage.Checked = Logic.CheckImage;
            m_togCheckMaterial.Checked = Logic.CheckMaterial;
            m_togCheckScripts.Checked = Logic.CheckScript;
            m_togIgnorePos.Checked = Logic.IgnorePos;
            m_progress.Visible = false;
            m_textProgress.Visible = false;
            Logic.AsyncWorker = m_asyncWorker;
            m_timer.Interval = 1000;
            //m_timer.Start();
            PrefabContrastTool_SizeChanged(null, null);
        }
        private void InsertFileMode()
        {
            foreach (SelectFileModeEnum mode in Enum.GetValues(typeof(SelectFileModeEnum)))
            {
                InsertFileMode(mode);
            }
            m_comSelectMode.SelectedIndex = (int)Logic.SelectFileMode;
        }
        private void InsertFileMode(SelectFileModeEnum mode)
        {
            string strMode = Logic.GetEnumDescription<SelectFileModeEnum>(mode);
            m_comSelectMode.Items.Add(strMode);
        }

        #endregion
        private void SetSVNClientPath()
        {
            if (!Logic.m_isSvnCompare) return;
            Logic.SourceClientPath = m_comClientPathList.SelectedItem.ToString();
            Logic.TargetClientPath = Logic.SourceClientPath;
        }
        #region 界面逻辑
        private void m_btnBegin_Click(object sender, EventArgs e)
        {
            SetSVNClientPath();
            if (!CheckCanBegin()) return;
            m_btnBegin.Enabled = false;
            var maxPro = Logic.ReadyBegin();
            m_clonePrefab.ClearCache();
            m_deletePrefab.ClearCache();
            m_progress.Visible = true;
            m_progress.Maximum = maxPro;
            m_progress.Minimum = 0;
            m_textProgress.Visible = true;
            m_textProgress.Text = "开始对比，请稍后...";
            m_asyncWorker.RunWorkerAsync();
        }
        private void m_btnRefresh_Click(object sender, EventArgs e)
        {
            Logic.RestartContrastprefab();
            RefreshTree();
        }
        private void m_textSourcePath_TextChanged(object sender, EventArgs e)
        {
            Logic.SourceFilePath = m_textSourcePath.Text;
        }
        private void m_textSourcePath_DragEnter(object sender, DragEventArgs e)
        {
            // 对文件拖拽事件做处理 
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;
        }
        private void m_textSourcePath_MouseClick(object sender, MouseEventArgs e)
        {
            m_textSourcePath.SelectAll();
        }
        private void m_textSourcePath_DragDrop(object sender, DragEventArgs e)
        {
            var filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filePath.Length > 0)
            {
                var path = filePath[0];
                if (Logic.IsFileMode)
                {
                    if (!string.IsNullOrEmpty(Path.GetExtension(path)))
                    {
                        //Logic.SourceFilePath = path;
                        m_textSourcePath.Text = path;
                    }
                    else
                    {
                        MessageBox.Show("选择文件模式不能拖动文件夹");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(Path.GetExtension(path)))
                    {
                        //Logic.SourceFilePath = path;
                        m_textSourcePath.Text = path;
                    }
                    else
                    {
                        MessageBox.Show("选择文件夹模式不能拖动文件");
                    }
                }
            }
        }
        private void m_textTargetPath_TextChanged(object sender, EventArgs e)
        {
            Logic.TargetFilePath = m_textTargetPath.Text;
        }
        private void m_textTargetPath_DragEnter(object sender, DragEventArgs e)
        {
            // 对文件拖拽事件做处理 
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;
        }
        private void m_textTargetPath_MouseClick(object sender, MouseEventArgs e)
        {
            m_textTargetPath.SelectAll();
        }
        private void m_textTargetPath_DragDrop(object sender, DragEventArgs e)
        {
            var filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filePath.Length > 0)
            {
                var path = filePath[0];
                if (Logic.IsFileMode)
                {
                    if (!string.IsNullOrEmpty(Path.GetExtension(path)))
                    {
                        //Logic.TargetFilePath = path;
                        m_textTargetPath.Text = path;
                    }
                    else
                    {
                        MessageBox.Show("选择文件模式不能拖动文件夹");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(Path.GetExtension(path)))
                    {
                        //Logic.TargetFilePath = path;
                        m_textTargetPath.Text = path;
                    }
                    else
                    {
                        MessageBox.Show("选择文件夹模式不能拖动文件");
                    }
                }
            }
        }
        private void m_comSelectMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            Logic.SelectFileMode = (SelectFileModeEnum)m_comSelectMode.SelectedIndex;
            m_textSourcePath.Text = Logic.SourceFilePath;
            m_textTargetPath.Text = Logic.TargetFilePath;
        }
        private void m_btnSelectSourceFile_Click(object sender, EventArgs e)
        {
            if (Logic.IsFileMode)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;
                dialog.Title = "请选择源文件";
                dialog.Filter = "PREFAB文件(*.prefab)|*.prefab";
                var paths = m_textSourcePath.Text.Split(';');
                var path = m_textSourcePath.Text;
                if(paths.Length >= 1)
                {
                    path = paths[0];
                }
                if (File.Exists(path))
                {
                    dialog.FileName = path;
                    dialog.InitialDirectory = Path.GetDirectoryName(path);
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string[] filePath = dialog.FileNames;
                    bool bFirst = true;
                    StringBuilder strFileNames = new StringBuilder();
                    for (int i = 0; i < filePath.Length; i++)
                    {
                        if (!bFirst)
                            strFileNames.Append(";");
                        else
                            bFirst = false;
                        strFileNames.Append(filePath[i]);
                    }
                    string str = strFileNames.ToString();
                    Logic.SourceFilePath = str;
                    m_textSourcePath.Text = str;
                }
            }
            else
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "请选择源文件夹路径";
                dialog.SelectedPath = m_textSourcePath.Text;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string str = dialog.SelectedPath;
                    Logic.SourceFilePath = str;
                    m_textSourcePath.Text = str;
                }
            }
        }
        private void m_btnSelectTargetFile_Click(object sender, EventArgs e)
        {
            if (Logic.IsFileMode)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;
                dialog.Title = "请选择目标文件";
                dialog.Filter = "PREFAB文件(*.prefab)|*.prefab";
                var paths = m_textTargetPath.Text.Split(';');
                var path = m_textTargetPath.Text;
                if (paths.Length >= 1)
                {
                    path = paths[0];
                }
                if (File.Exists(path))
                {
                    dialog.FileName = path;
                    dialog.InitialDirectory = Path.GetDirectoryName(path);
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string[] filePath = dialog.FileNames;
                    bool bFirst = true;
                    StringBuilder strFileNames = new StringBuilder();
                    for (int i = 0; i < filePath.Length; i++)
                    {
                        if (!bFirst)
                            strFileNames.Append(";");
                        else
                            bFirst = false;
                        strFileNames.Append(filePath[i]);
                    }
                    string str = strFileNames.ToString();
                    Logic.TargetFilePath = str;
                    m_textTargetPath.Text = str;
                }
            }
            else
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "请选择目标文件夹路径";
                dialog.SelectedPath = m_textTargetPath.Text;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string str = dialog.SelectedPath;
                    Logic.TargetFilePath = str;
                    m_textTargetPath.Text = str;
                }
            }
        }
        private void m_textClientPath_TextChanged(object sender, EventArgs e)
        {
            //Core.FileOP.Instance.WriteString("Path", "SVNClientPath", m_textClientPath.Text);
        }
        private void m_textClientPath_MouseClick(object sender, MouseEventArgs e)
        {
            m_textClientPath.SelectAll();
        }
        private void m_togShowAll_CheckedChanged(object sender, EventArgs e)
        {
            Logic.ShowAll = m_togShowAll.Checked;
        }
        private void m_togCheckScripts_CheckedChanged(object sender, EventArgs e)
        {
            Logic.CheckScript = m_togCheckScripts.Checked;
        }
        private void m_togCheckMaterial_CheckedChanged(object sender, EventArgs e)
        {
            Logic.CheckMaterial = m_togCheckMaterial.Checked;
        }
        private void m_togCheckImage_CheckedChanged(object sender, EventArgs e)
        {
            Logic.CheckImage = m_togCheckImage.Checked;
        }
        private void m_togCheckFont_CheckedChanged(object sender, EventArgs e)
        {
            Logic.CheckFont = m_togCheckFont.Checked;
        }
        private void m_togCheckActive_CheckedChanged(object sender, EventArgs e)
        {
            Logic.CheckActive = m_togCheckActive.Checked;
        }
        private void m_togIgnorePos_CheckedChanged(object sender, EventArgs e)
        {
            Logic.IgnorePos = m_togIgnorePos.Checked;
        }
        private void m_treeSource_AfterSelect(object sender, TreeViewEventArgs e)
        {
            /*var node = e.Node;
            string key;
            RootDirectory rootDirectory = node.Tag as RootDirectory;
            if( rootDirectory == null )
            {
                GameObject gameObject = node.Tag as GameObject;
                key = gameObject.path;
            }
            else
            {
                key = rootDirectory.path;
            }
            var targetNodes = m_treeTarget.Nodes.Find(key, true);
            if(targetNodes.Length > 0 )
            {
                var targetNode = targetNodes[0];
                if( targetNode.Parent != null)
                {
                    targetNode.Parent.Expand();
                }
                m_treeTarget.SelectedNode = targetNode;
            }*/
        }
        private void m_treeTarget_AfterSelect(object sender, TreeViewEventArgs e)
        {
            /*var node = e.Node;
            string key;
            RootDirectory rootDirectory = node.Tag as RootDirectory;
            if (rootDirectory == null)
            {
                GameObject gameObject = node.Tag as GameObject;
                key = gameObject.path;
            }
            else
            {
                key = rootDirectory.path;
            }
            var targetNodes = m_treeSource.Nodes.Find(key, true);
            if (targetNodes.Length > 0)
            {
                var targetNode = targetNodes[0];
                if (targetNode.Parent != null)
                {
                    targetNode.Parent.Expand();
                }
                m_treeSource.SelectedNode = targetNode;
            }*/
        }
        private void m_treeSource_AfterExpand(object sender, TreeViewEventArgs e)
        {
            ResetSearchData();
            var node = e.Node;
            if( node.Nodes.Count == 1)
            {
                if( node.Nodes[0].Text.Equals("TEMP"))
                {
                    node.Nodes.Clear();
                }
            }
            string key;
            RootDirectory rootDirectory = node.Tag as RootDirectory;
            if (rootDirectory == null)
            {
                GameObject gameObject = node.Tag as GameObject;
                if(gameObject != null)
                {
                    key = gameObject.path;
                    AddExtendDic(key, true);
                    if (node.Nodes.Count == 0)
                    {
                        for (int k = 0; k < gameObject.childs.Count; k++)
                        {
                            RefreshPrefabTreeNodes(gameObject.childs[k], node.Nodes);
                        }
                    }
                }
                else
                {
                    key = node.Tag.ToString();
                }
            }
            else
            {
                key = rootDirectory.path;
                if (node.Nodes.Count == 0)
                {
                    RefreshDirTreeNodes(rootDirectory, node.Nodes);
                }
            }
            var targetNodes = m_treeTarget.Nodes.Find(key, true);
            if (targetNodes.Length > 0)
            {
                var targetNode = targetNodes[0];
                if(node.IsExpanded)
                {
                    targetNode.Expand();
                }
                else
                {
                    targetNode.Collapse();
                }
                m_treeTarget.SelectedNode = targetNode;
            }
        }
        private void m_treeSource_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            var node = e.Node;
            string key;
            RootDirectory rootDirectory = node.Tag as RootDirectory;
            if (rootDirectory == null)
            {
                GameObject gameObject = node.Tag as GameObject;
                if( gameObject != null )
                {
                    key = gameObject.path;
                    AddExtendDic(key, false);
                }
                else
                {
                    key = node.Tag.ToString();
                }
            }
            else
            {
                key = rootDirectory.path;
            }
            var targetNodes = m_treeTarget.Nodes.Find(key, true);
            if (targetNodes.Length > 0)
            {
                var targetNode = targetNodes[0];
                if (node.IsExpanded)
                {
                    targetNode.Expand();
                }
                else
                {
                    targetNode.Collapse();
                }
                m_treeTarget.SelectedNode = targetNode;
            }
        }
        private void m_treeTarget_AfterExpand(object sender, TreeViewEventArgs e)
        {
            var node = e.Node;
            if (node.Nodes.Count == 1)
            {
                if (node.Nodes[0].Text.Equals("TEMP"))
                {
                    node.Nodes.Clear();
                }
            }
            string key;
            RootDirectory rootDirectory = node.Tag as RootDirectory;
            if (rootDirectory == null)
            {
                GameObject gameObject = node.Tag as GameObject;
                if (gameObject != null)
                {
                    key = gameObject.path;
                    if (node.Nodes.Count == 0)
                    {
                        for (int k = 0; k < gameObject.childs.Count; k++)
                        {
                            RefreshPrefabTreeNodes(gameObject.childs[k], node.Nodes);
                        }
                    }
                }
                else
                {
                    key = node.Tag.ToString();
                }
            }
            else
            {
                key = rootDirectory.path;
                if (node.Nodes.Count == 0)
                {
                    RefreshDirTreeNodes(rootDirectory, node.Nodes);
                }
            }
            var targetNodes = m_treeSource.Nodes.Find(key, true);
            if (targetNodes.Length > 0)
            {
                var targetNode = targetNodes[0];
                if (node.IsExpanded)
                {
                    targetNode.Expand();
                }
                else
                {
                    targetNode.Collapse();
                }
                m_treeSource.SelectedNode = targetNode;
            }
        }
        private void m_treeTarget_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            var node = e.Node;
            string key;
            RootDirectory rootDirectory = node.Tag as RootDirectory;
            if (rootDirectory == null)
            {
                GameObject gameObject = node.Tag as GameObject;
                if (gameObject != null)
                {
                    key = gameObject.path;
                }
                else
                {
                    key = node.Tag.ToString();
                }
            }
            else
            {
                key = rootDirectory.path;
            }
            var targetNodes = m_treeSource.Nodes.Find(key, true);
            if (targetNodes.Length > 0)
            {
                var targetNode = targetNodes[0];
                if (node.IsExpanded)
                {
                    targetNode.Expand();
                }
                else
                {
                    targetNode.Collapse();
                }
                m_treeSource.SelectedNode = targetNode;
            }
        }
        #endregion
        #region 树相关
        private void RefreshTree()
        {
            m_extendNodeDic.Clear();
            m_textProgress.Visible = true;
            RefreshSourceTree();
            RefreshTargetTree();
            m_textProgress.Visible = false;
        }
        private void RefreshSourceTree()
        {
            m_treeSource.Nodes.Clear();
            var rootDirs = Logic.SourceDirList;
            if(rootDirs.Count == 1 )
            {
                var dir = rootDirs[0];
                RefreshDirTreeNodes(dir, m_treeSource.Nodes);
                return;
            }
            for (int i = 0; i < rootDirs.Count; i++)
            {
                var _i = i;
                //PostFunc(i, () =>
                //{
                    RefreshDirTree(rootDirs[_i], m_treeSource.Nodes);
                //});
            }
        }
        private void RefreshDirTree( RootDirectory dir, TreeNodeCollection nodes)
        {
            if (dir.rootDirs.Count > 0)
            {
                m_textProgress.Text = dir.path;
                var newNode = NewDirTreeNode(nodes, dir);
                if(dir.rootDirs.Count > 0 || dir.prefabs.Count > 0)
                {
                    //先挂一个临时节点
                    newNode.Nodes.Add("TEMP");
                }
                //RefreshDirTreeNodes(dir, newNode);
            }
            if (dir.prefabs.Count > 1)
            {
                m_textProgress.Text = dir.path;
                var newNode = NewDirTreeNode(nodes, dir);
                RefreshDirTreeNodes(dir, newNode.Nodes);
            }
            else if (dir.prefabs.Count > 0)
            {
                RefreshPrefabTreeNodes(dir.prefabs[0].root, nodes);
            }
        }
        private void RefreshTargetTree()
        {
            m_treeTarget.Nodes.Clear();
            var rootDirs = Logic.TargetDirList;
            if (rootDirs.Count == 1)
            {
                var dir = rootDirs[0];
                RefreshDirTreeNodes(dir, m_treeTarget.Nodes);
                return;
            }
            for (int i = 0; i < rootDirs.Count; i++)
            {
                var _i = i;
                //PostFunc(i, () =>
                //{
                    RefreshDirTree(rootDirs[_i], m_treeTarget.Nodes);
                //});
            }
        }
        private void RefreshDirTreeNodes(RootDirectory dir, TreeNodeCollection nodes)
        {
            for (int i = 0; i < dir.rootDirs.Count; i++)
            {
                var _dir = dir.rootDirs[i];
                m_textProgress.Text = _dir.path;
                var newNode = NewDirTreeNode(nodes, _dir);
                if(_dir.rootDirs.Count > 0 )
                {
                    //先挂一个临时节点
                    newNode.Nodes.Add("TEMP");
                }
                else if(_dir.prefabs.Count > 0)
                {
                    int childCount = 0;
                    for (int k = 0; k < _dir.prefabs.Count; k++)
                    {
                        if (Logic.ShowAll || _dir.prefabs[k].root.differentType != DifferentType.AllNone)
                        {
                            childCount++;
                            break;
                        }
                    }
                    if( childCount > 0)
                    {
                        //先挂一个临时节点
                        newNode.Nodes.Add("TEMP");
                    }
                    else
                    {
                        newNode.Remove();
                    }
                }
                else
                {
                    newNode.Remove();
                }
                //RefreshDirTreeNodes(_dir, newNode.Nodes);
            }
            for (int i = 0; i < dir.prefabs.Count; i++)
            {
                var prefab = dir.prefabs[i];
                if(prefab.root != null)
                {
                    RefreshPrefabTreeNodes(prefab.root, nodes);
                }
            }
        }
        private void RefreshPrefabTreeNodes( GameObject gameObject , TreeNodeCollection nodes)
        {
            var newNode = NewPrefabTreeNode(nodes, gameObject);
            if (newNode == null) return;
            int childCount = 0;
            for (int i = 0; i < gameObject.childs.Count; i++)
            {
                if ( Logic.ShowAll || gameObject.childs[i].differentType != DifferentType.AllNone)
                {
                    childCount++;
                    break;
                }
            }
            if (childCount > 0)
            {
                //先挂一个临时节点
                newNode.Nodes.Add("TEMP");
            }
            /*for (int k = 0; k < gameObject.childs.Count; k++)
            {
                RefreshPrefabTreeNodes(gameObject.childs[k], newNode.Nodes);
            }*/
        }
        private TreeNode NewDirTreeNode( TreeNodeCollection nodes , RootDirectory rootDirectory )
        {
            if (string.IsNullOrEmpty(rootDirectory.name)) return null;
            TreeNode newNode = nodes.Add(rootDirectory.path, rootDirectory.name);
            //newNode.BackColor = _Color.Orange;
            newNode.Tag = rootDirectory;
            SetTreeNode(newNode);
            return newNode;
        }
        private TreeNode NewPrefabTreeNode(TreeNodeCollection nodes, GameObject gameObject )
        {
            if (Logic.IsPlaceNode(gameObject)) return null;
            if (!Logic.ShowAll && gameObject.differentType == DifferentType.AllNone) return null;
            TreeNode newNode = nodes.Add(gameObject.path , gameObject.name);
            newNode.Tag = gameObject;
            SetTreeNode(newNode);
            return newNode;
        }
        private void SetTreeNode( TreeNode node )
        {
            RootDirectory rootDirectory = node.Tag as RootDirectory;
            if( rootDirectory == null )
            {
                GameObject go = node.Tag as GameObject;
                if (string.IsNullOrEmpty(go.name))
                {
                    node.Text = "";
                    return;
                }
                var addStr = Logic.GetStrByDifferentType(go.differentType);
                if (!string.IsNullOrEmpty(addStr))
                    node.Text = go.name + "      " + Logic.GetStrByDifferentType(go.differentType);
                else
                    node.Text = go.name;
                if ( go.differentType == DifferentType.Null || Logic.HasFlag(go.differentType, DifferentType.NodePath) || Logic.HasFlag(go.differentType, DifferentType.NodeName))
                {
                    node.BackColor = Logic.GetColorByDifferentType(go.differentType);
                }
                else
                {
                    node.ForeColor = Logic.GetColorByDifferentType(go.differentType);
                }
            }
            else
            {
                if(CheckRootDirDifferent(rootDirectory))
                {
                    node.BackColor = _Color.Red;
                }
                else
                {
                    node.BackColor = _Color.Orange;
                }
                node.Text = rootDirectory.name;
            }
        }
        private bool CheckRootDirDifferent(RootDirectory rootDirectory)
        {
            if (rootDirectory.prefabs.Count > 0)
            {
                for (int i = 0; i < rootDirectory.prefabs.Count; i++)
                {
                    var prefab = rootDirectory.prefabs[i];
                    if (prefab.root.differentType == DifferentType.Null || prefab.root.differentType == DifferentType.NodeName || prefab.root.differentType == DifferentType.NodePath)
                    {
                        return true;
                    }
                }
            }
            if( rootDirectory.rootDirs.Count > 0)
            {
                for (int i = 0; i < rootDirectory.rootDirs.Count; i++)
                {
                    if (CheckRootDirDifferent(rootDirectory.rootDirs[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #region 添加右键菜单
        private string m_scriptTag = "  [组件]";
        
        private void m_treeSource_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var snode = e.Node;
                GameObject sgo = snode.Tag as GameObject;
                if (sgo == null) return;
                snode.ContextMenuStrip = m_treeMenu;
                m_treeMenu.Close();
                m_treeMenu.Items.Clear();
                AddMenuItem("查看组件", m_treeMenu.Items, (_s, _e) =>
                {
                    //先清除原有组件节点
                    var count = snode.Nodes.Count;
                    for (int i = count-1; i >=0; i--)
                    {
                        if(snode.Nodes[i].Text.Contains(m_scriptTag) )
                        {
                            snode.Nodes.RemoveAt(i);
                        }
                    }
                    if(snode.Nodes.Count > 0 )
                    {
                        snode.Expand();
                    }
                    TreeNode tnode = null;
                    TreeNode[] tnodes = m_treeTarget.Nodes.Find(sgo.path, true);
                    GameObject tgo = null;
                    if(tnodes.Length > 0)
                    {
                        tnode = tnodes[0];
                        var tcount = tnode.Nodes.Count;
                        for (int i = tcount - 1; i >= 0; i--)
                        {
                            if (tnode.Nodes[i].Text.Contains(m_scriptTag))
                            {
                                tnode.Nodes.RemoveAt(i);
                            }
                        }
                        tgo = tnode.Tag as GameObject;
                    }
                    if(sgo.scripts != null)
                    {
                        for (int i = sgo.scripts.Count-1; i >=0 ; i--)
                        {
                            var script = sgo.scripts[i];
                            var key = sgo.path + "/" + script.name;
                            TreeNode newNode = snode.Nodes.Insert(0,key, m_scriptTag + script.name);
                            newNode.Tag = key;
                            if (script.name.Equals(Logic.MissName))
                            {
                                newNode.ForeColor = _Color.Red;
                            }
                            else
                            {
                                AddScriptDetailNode(newNode, script, sgo , tgo);
                                if(tgo != null)
                                {
                                    var tscript = CheckContainsScript(script.name, tgo.scripts);
                                    if (tscript == null)
                                    {
                                        newNode.ForeColor = _Color.Red;
                                    }
                                    else if (!script.Equals(tscript))
                                    {
                                        newNode.ForeColor = _Color.Red;
                                    }
                                }
                                else
                                {
                                    newNode.ForeColor = _Color.Red;
                                }
                            }
                        }
                    }
                    TreeNode _newNode_ = snode.Nodes.Insert(0,sgo.path + "/IsActive", m_scriptTag + "IsActive  " + sgo.isActive.ToString());
                    _newNode_.Tag = sgo.path + "/IsActive";
                    if (tgo == null || tgo.isActive != sgo.isActive)
                    {
                        _newNode_.ForeColor = _Color.Red;
                    }
                    if (tgo != null && tgo.scripts != null)
                    {
                        for (int i = tgo.scripts.Count-1; i >=0 ; i--)
                        {
                            var script = tgo.scripts[i];
                            var key = tgo.path + "/" + script.name;
                            TreeNode newNode = tnode.Nodes.Insert(0, key, m_scriptTag + script.name);
                            newNode.Tag = key;
                            if (script.name.Equals(Logic.MissName) )
                                newNode.ForeColor = _Color.Red;
                            else
                            {
                                AddScriptDetailNode(newNode, script, tgo , sgo );
                                var sscript = CheckContainsScript(script.name, sgo.scripts);
                                if(sscript == null)
                                {
                                    newNode.ForeColor = _Color.Red;
                                }
                                else if (!script.Equals(sscript))
                                {
                                    newNode.ForeColor = _Color.Red;
                                }
                            }
                            tnode.Expand();
                        }
                        TreeNode _newNode = tnode.Nodes.Insert(0, tgo.path + "/IsActive", m_scriptTag + "IsActive  " + tgo.isActive.ToString());
                        _newNode.Tag = tgo.path + "/IsActive";
                        if (tgo.isActive != sgo.isActive)
                        {
                            _newNode.ForeColor = _Color.Red;
                        }
                    }
                    snode.Expand();
                }, "此操作不影响预制");
                AddMenuItem("隐藏组件", m_treeMenu.Items, (_s, _e) =>
                {
                    var count = snode.Nodes.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (snode.Nodes[i].Text.Contains(m_scriptTag))
                        {
                            snode.Nodes.RemoveAt(i);
                        }
                    }
                    TreeNode[] tnodes = m_treeTarget.Nodes.Find(sgo.path, true); 
                    if(tnodes.Length > 0 )
                    {
                        var _node = tnodes[0];
                        var _count = _node.Nodes.Count;
                        for (int i = count - 1; i >= 0; i--)
                        {
                            if (_node.Nodes[i].Text.Contains(m_scriptTag))
                            {
                                _node.Nodes.RemoveAt(i);
                            }
                        }
                    }
                }, "此操作不影响预制");
                AddMenuItem("拷贝节点", m_treeMenu.Items, (_s, _e) =>
                {
                    var goId = sgo.fileId;
                    ShowLoadingText("预制克隆中，可能需要加载Meta文件，请稍后...");
                    var srcPath = sgo.root.fileFullPath;
                    TreeNode[] tnodes = m_treeTarget.Nodes.Find(sgo.root.path, true);
                    GameObject trootgo = null;
                    TreeNode trootNode = null;
                    if (tnodes.Length > 0)
                    {
                        trootNode = tnodes[0];
                        trootgo = trootNode.Tag as GameObject;
                    }
                    if(trootgo == null)
                    {
                        MessageBox.Show("克隆失败，未找到目标根节点");
                        return;
                    }
                    var tarPath = trootgo.fileFullPath;
                    bool success = m_clonePrefab.ClonePrefabData(goId, srcPath, tarPath, true);
                    ShowLoadingText();
                    if( success )
                    {
                        MessageBox.Show("拷贝成功!");
                        Logic.RefreshOnePrefab(sgo.root.prefab, trootgo.prefab);
                        sgo = sgo.root.prefab.root;
                        trootgo = trootgo.prefab.root;
                        var srootNode = FindParentNode(snode, sgo);
                        if(srootNode != null )
                        {
                            srootNode.Nodes.Clear();
                            for (int i = 0; i < sgo.childs.Count; i++)
                            {
                                RefreshPrefabTreeNodes(sgo.childs[i], srootNode.Nodes);
                            }
                        }
                       // var trootNode = FindParentNode(tnode, trootgo);
                        if (trootNode != null)
                        {
                            trootNode.Nodes.Clear();
                            for (int i = 0; i < trootgo.childs.Count; i++)
                            {
                                RefreshPrefabTreeNodes(trootgo.childs[i], trootNode.Nodes);
                            }
                        }
                        SetExtendState(srootNode);
                    }
                }, "此操作会影响预制");
                AddMenuItem("删除节点", m_treeMenu.Items, (_s, _e) =>
                {
                    var result = MessageBox.Show("确认删除此节点？ Name:" + sgo.name, "提醒", MessageBoxButtons.OKCancel);
                    if (result != DialogResult.OK) return;
                    var goId = sgo.fileId;
                    var srootgo = sgo.root;
                    var srcPath = srootgo.fileFullPath;
                    var success = m_deletePrefab.DeletePrefabData(goId, srcPath);
                    if( success )
                    {
                        MessageBox.Show("删除节点成功!");
                        TreeNode[] tnodes = m_treeTarget.Nodes.Find(srootgo.path, true);
                        GameObject trootgo = null;
                        TreeNode trootNode = null;
                        if (tnodes.Length > 0)
                        {
                            trootNode = tnodes[0];
                            trootgo = trootNode.Tag as GameObject;
                        }
                        if(trootgo == null)
                        {
                            Logic.RefreshOnePrefab(srootgo.prefab , null);
                        }
                        else
                        {
                            Logic.RefreshOnePrefab(srootgo.prefab, trootgo.prefab);
                        }
                        sgo = sgo.root.prefab.root;
                        var srootNode = FindParentNode(snode, sgo);
                        if (srootNode != null)
                        {
                            srootNode.Nodes.Clear();
                            for (int i = 0; i < sgo.childs.Count; i++)
                            {
                                RefreshPrefabTreeNodes(sgo.childs[i], srootNode.Nodes);
                            }
                        }
                        // var trootNode = FindParentNode(tnode, trootgo);
                        if(trootgo != null)
                        {
                            trootgo = trootgo.prefab.root;
                            trootNode.Nodes.Clear();
                            for (int i = 0; i < trootgo.childs.Count; i++)
                            {
                                RefreshPrefabTreeNodes(trootgo.childs[i], trootNode.Nodes);
                            }
                        }
                        SetExtendState(srootNode);
                    }
                }, "此操作会影响预制");
            }
        }
        private void SetExtendState( TreeNode node )
        {
            if (node == null) return;
            var go = node.Tag as GameObject;
            if (go == null) return;
            var extend = GetExtendDic(go.path);
            if( extend )
            {
                node.Expand();
                for (int i = 0; i < node.Nodes.Count; i++)
                {
                    SetExtendState(node.Nodes[i]);
                }
            }
        }
        private TreeNode FindParentNode( TreeNode node , GameObject root )
        {
            var parent = node.Parent;
            if (parent == null) return null;
            var parentGo = parent.Tag as GameObject;
            if( parentGo.path == root.path)
            {
                return parent;
            }
            return FindParentNode(parent, root);
        }
        private void AddScriptDetailNode( TreeNode node ,Script script , GameObject go1 , GameObject go2 )
        {
            if (go1 == null ) return;
            var nodes = node.Nodes;
            /*if ( script.type == ScriptType.RectTransform )
            {
                var rect1 = go1.rectTransform;
                var rect2 = go2 == null ? null : go2.rectTransform;
                if( rect1.isTransform )
                {
                    var posNode = nodes.Add("Position  " + rect1.localPosition.ToString());
                    if(rect2 == null || !rect1.localPosition.Equals(rect2.localPosition) )
                    {
                        posNode.ForeColor = _Color.Red;
                    }
                    var rotationNode = nodes.Add("Rotation  " + rect1.localRotation.ToString());
                    if (rect2 == null || !rect1.localRotation.Equals(rect2.localRotation))
                    {
                        rotationNode.ForeColor = _Color.Red;
                    }
                    var scaleNode = nodes.Add("Scale  " + rect1.localScale.ToString());
                    if (rect2 == null || !rect1.localScale.Equals(rect2.localScale))
                    {
                        scaleNode.ForeColor = _Color.Red;
                    }
                }
                else
                {
                    var posNode = nodes.Add("Position  " + rect1.anchoredPosition.ToString());
                    if (rect2 == null || !rect1.anchoredPosition.Equals(rect2.anchoredPosition))
                    {
                        posNode.ForeColor = _Color.Red;
                    }
                    var rotationNode = nodes.Add("Rotation  " + rect1.localRotation.ToString());
                    if (rect2 == null || !rect1.localRotation.Equals(rect2.localRotation))
                    {
                        rotationNode.ForeColor = _Color.Red;
                    }
                    var scaleNode = nodes.Add("Scale  " + rect1.localScale.ToString());
                    if (rect2 == null || !rect1.localScale.Equals(rect2.localScale))
                    {
                        scaleNode.ForeColor = _Color.Red;
                    }
                    var sizeNode = nodes.Add("SizeDelta  " + rect1.sizeDelta.ToString());
                    if (rect2 == null || !rect1.sizeDelta.Equals(rect2.sizeDelta))
                    {
                        sizeNode.ForeColor = _Color.Red;
                    }
                    var anchorMinNode = nodes.Add("AnchorMin  " + rect1.anchorMin.ToString());
                    if (rect2 == null || !rect1.anchorMin.Equals(rect2.anchorMin))
                    {
                        anchorMinNode.ForeColor = _Color.Red;
                    }
                    var anchorMaxNode = nodes.Add("AnchorMax  " + rect1.anchorMax.ToString());
                    if (rect2 == null || !rect1.anchorMax.Equals(rect2.anchorMax))
                    {
                        anchorMaxNode.ForeColor = _Color.Red;
                    }
                    var pivotNode = nodes.Add("Pivot  " + rect1.pivot.ToString());
                    if (rect2 == null || !rect1.pivot.Equals(rect2.pivot))
                    {
                        pivotNode.ForeColor = _Color.Red;
                    }
                }
                return;
            }
            else if( script.type == ScriptType.MonoBehaviour)
            {
                var mono1 = go1.monoBehaviour;
                var mono2 = go2 == null ? null : go2.monoBehaviour;
                if (mono1 == null) return;
                var enableNode = nodes.Add("Enabled  " + mono1.enabled.ToString());
                if (mono2 == null || !mono1.enabled.Equals(mono2.enabled))
                {
                    enableNode.ForeColor = _Color.Red;
                }
                if (mono1.sprite != null)
                {
                    var node1 = nodes.Add("Sprite  " + mono1.sprite.name);
                    if (mono2 == null || mono2.sprite == null || !mono1.sprite.name.Equals(mono2.sprite.name) || mono1.sprite.name.Equals(Logic.MissName) )
                    {
                        node1.ForeColor = _Color.Red;
                    }
                    var node2 = nodes.Add("RaycastTarget  " + ( mono1.sprite.raycastTarget.Equals("1") ).ToString() );
                    if (mono2 == null || mono2.sprite == null || !mono1.sprite.raycastTarget.Equals(mono2.sprite.raycastTarget))
                    {
                        node2.ForeColor = _Color.Red;
                    }
                    var node3 = nodes.Add("Color  " + mono1.sprite.color.ToString());
                    if (mono2 == null || mono2.sprite == null || !mono1.sprite.color.Equals(mono2.sprite.color))
                    {
                        node3.ForeColor = _Color.Red;
                    }
                }
                if (mono1.font != null)
                {
                    var node0 = nodes.Add("Text  " + mono1.font.text);
                    if (mono2 == null || mono2.font == null || !mono1.font.text.Equals(mono2.font.text))
                    {
                        node0.ForeColor = _Color.Red;
                    }
                    var node1 = nodes.Add("Font  " + mono1.font.name);
                    if (mono2 == null || !mono1.font.name.Equals(mono2.font.name) || mono1.font.name.Equals(Logic.MissName))
                    {
                        node1.ForeColor = _Color.Red;
                    }
                    var node2 = nodes.Add("Size  " + mono1.font.size);
                    if (mono2 == null || mono2.font == null || !mono1.font.size.Equals(mono2.font.size))
                    {
                        node2.ForeColor = _Color.Red;
                    }
                    var node3 = nodes.Add("Aligment  " + mono1.font.aligment);
                    if (mono2 == null || mono2.font == null || !mono1.font.aligment.Equals(mono2.font.aligment))
                    {
                        node3.ForeColor = _Color.Red;
                    }
                    var node4 = nodes.Add("LineSpacing  " + mono1.font.lineSpacing);
                    if (mono2 == null || mono2.font == null || !mono1.font.lineSpacing.Equals(mono2.font.lineSpacing))
                    {
                        node4.ForeColor = _Color.Red;
                    }
                    var node5 = nodes.Add("RaycastTarget  " + (mono1.font.raycastTarget.Equals("1")).ToString() );
                    if (mono2 == null || mono2.font == null || !mono1.font.raycastTarget.Equals(mono2.font.raycastTarget))
                    {
                        node5.ForeColor = _Color.Red;
                    }
                    var node6 = nodes.Add("Color  " + mono1.font.color.ToString());
                    if (mono2 == null || mono2.font == null || !mono1.font.color.Equals(mono2.font.color))
                    {
                        node6.ForeColor = _Color.Red;
                    }
                }
                if (mono1.material != null)
                {
                    var node1 = nodes.Add("Material  " + mono1.material.ToString());
                    if (mono2 == null || !mono1.material.Equals(mono2.material) || mono1.material.name.Equals(Logic.MissName))
                    {
                        node1.ForeColor = _Color.Red;
                    }
                }
            }
            else if( script.type == ScriptType.Other )*/
            {
                Script otherScript = null;
                if(go2 != null)
                {
                    for (int i = 0; i < go2.scripts.Count; i++)
                    {
                        if (go2.scripts[i].name.Equals(script.name))
                        {
                            otherScript = go2.scripts[i];
                            break;
                        }
                    }
                }
                if( otherScript == null )
                {
                    foreach (var item in script.members)
                    {
                        var member = item.Value;
                        if (Logic.IsIgnoreFiled(member.key)) continue;
                        if (member.valueList != null)
                        {
                            var key = node.Tag.ToString() + "/" + member.key;
                            var _node = nodes.Add(key,member.key);
                            _node.Tag = key;
                            _node.ForeColor = _Color.Red;
                            for (int i = 0; i < member.valueList.Count; i++)
                            {
                                var _key = _node.Tag.ToString() + "/" + i.ToString();
                                var cinode = _node.Nodes.Add(_key,"Index:" + i.ToString());
                                cinode.Tag = _key;
                                cinode.ForeColor = _Color.Red;
                                var cmembers = member.valueList[i];
                                foreach (var cmember in cmembers.Values)
                                {
                                    if (Logic.IsIgnoreFiled(member.key)) continue;
                                    var __key = cinode.Tag.ToString() + "/" + member.key;
                                    var cnode = cinode.Nodes.Add(__key,cmember.key + "  " + cmember.value.ToString());
                                    cnode.Tag = __key;
                                    cnode.ForeColor = _Color.Red;
                                }
                            }
                        }
                        else
                        {
                            var key = node.Tag.ToString() + "/" + member.key;
                            var _node = nodes.Add(key,member.key +"  " +member.value.ToString());
                            _node.Tag = key;
                            _node.ForeColor = _Color.Red;
                        }
                    }
                    return;
                }
                foreach (var item in script.members)
                {
                    var member = item.Value;
                    if (Logic.IsIgnoreFiled(member.key)) continue;
                    Member otherMember = null;
                    if( otherScript.members.ContainsKey(member.key))
                    {
                        otherMember = otherScript.members[member.key];
                    }
                    if (member.valueList != null)
                    {
                        bool isSame = true;
                        if(otherMember == null || otherMember.valueList == null || otherMember.valueList.Count != member.valueList.Count )
                        {
                            isSame = false;
                        }
                        var key = node.Tag.ToString() + "/" + member.key;
                        var _node = nodes.Add(key,member.key);
                        _node.Tag = key;
                        for (int i = 0; i < member.valueList.Count; i++)
                        {
                            var _key = _node.Tag.ToString() + "/" + i.ToString();
                            var cinode = _node.Nodes.Add(_key, "Index:" + i.ToString());
                            cinode.Tag = _key;
                            bool isSame2 = true;
                            var cmembers = member.valueList[i];
                            Dictionary<string,Member> otherCMembers = null;
                            if(otherMember != null && otherMember.valueList != null && i < otherMember.valueList.Count )
                            {
                                otherCMembers = otherMember.valueList[i];
                            }
                            if(otherCMembers == null )
                            {
                                isSame2 = false;
                            }
                            foreach (var cmember in cmembers.Values)
                            {
                                if (Logic.IsIgnoreFiled(cmember.key)) continue;
                                var __key = cinode.Tag.ToString() + "/" + i.ToString();
                                var cnode = cinode.Nodes.Add(__key, cmember.key + "  " + cmember.value.ToString());
                                cnode.Tag = __key;
                                if (otherCMembers == null || !otherCMembers.ContainsKey(cmember.key) || !otherCMembers[cmember.key].Equals(cmember))
                                {
                                    cnode.ForeColor = _Color.Red;
                                    isSame2 = false;
                                }
                            }
                            if( !isSame2 )
                            {
                                cinode.ForeColor = _Color.Red;
                                isSame = false;
                            }
                        }
                        if ( !isSame )
                        {
                            _node.ForeColor = _Color.Red;
                        }
                    }
                    else
                    {
                        var key = node.Tag.ToString() + "/" + member.key;
                        var _node = nodes.Add(key,member.key + "  " + member.value.ToString());
                        _node.Tag = key;
                        if (otherMember == null || !otherMember.Equals(member) )
                        {
                            _node.ForeColor = _Color.Red;
                        }
                    }
                }
            }
        }
        private void m_treeTarget_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var tnode = e.Node;
                GameObject tgo = tnode.Tag as GameObject;
                if (tgo == null) return;
                tnode.ContextMenuStrip = m_treeMenu;
                m_treeMenu.Close();
                m_treeMenu.Items.Clear();
                AddMenuItem("查看组件", m_treeMenu.Items, (_s, _e) =>
                {
                    //先清除原有组件节点
                    var count = tnode.Nodes.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (tnode.Nodes[i].Text.Contains(m_scriptTag))
                        {
                            tnode.Nodes.RemoveAt(i);
                        }
                    }
                    if (tnode.Nodes.Count > 0)
                    {
                        tnode.Expand();
                    }
                    TreeNode snode = null;
                    TreeNode[] snodes = m_treeSource.Nodes.Find(tgo.path, true);
                    GameObject sgo = null;
                    if (snodes.Length > 0)
                    {
                        snode = snodes[0];
                        var scount = snode.Nodes.Count;
                        for (int i = scount - 1; i >= 0; i--)
                        {
                            if (snode.Nodes[i].Text.Contains(m_scriptTag))
                            {
                                snode.Nodes.RemoveAt(i);
                            }
                        }
                        sgo = snode.Tag as GameObject;
                    }
                    if (tgo.scripts != null)
                    {
                        for (int i = tgo.scripts.Count-1; i >=0 ; i--)
                        {
                            var script = tgo.scripts[i];
                            TreeNode newNode = tnode.Nodes.Insert(0,tgo.path+"/"+script.name , m_scriptTag + script.name);
                            newNode.Tag = tgo.path + "/" + script.name;
                            if (script.name.Equals(Logic.MissName) )
                            {
                                newNode.ForeColor = _Color.Red;
                            }
                            else
                            {
                                AddScriptDetailNode(newNode, script, tgo, sgo);
                                if(sgo != null)
                                {
                                    var sscript = CheckContainsScript(script.name, sgo.scripts);
                                    if (sscript == null)
                                    {
                                        newNode.ForeColor = _Color.Red;
                                    }
                                    else if (!script.Equals(sscript))
                                    {
                                        newNode.ForeColor = _Color.Red;
                                    }
                                }
                                else
                                {
                                    newNode.ForeColor = _Color.Red;
                                }
                            }
                        }
                    }
                    TreeNode _newNode_ = tnode.Nodes.Insert(0,tgo.path + "/IsActive", m_scriptTag + "IsActive  " + tgo.isActive.ToString());
                    _newNode_.Tag = tgo.path + "/IsActive";
                    if (sgo == null || sgo.isActive != tgo.isActive)
                    {
                        _newNode_.ForeColor = _Color.Red;
                    }
                    if (sgo != null && sgo.scripts != null)
                    {
                        for (int i = sgo.scripts.Count-1; i >= 0 ; i--)
                        {
                            if(snode.Nodes.Count > 0)
                            {
                                snode.Expand();
                            }
                            var script = sgo.scripts[i];
                            TreeNode newNode = snode.Nodes.Insert(0,sgo.path + "/" + script.name, m_scriptTag + script.name);
                            newNode.Tag = tgo.path + "/" + script.name;
                            if (script.name.Equals(Logic.MissName) )
                            {
                                newNode.ForeColor = _Color.Red;
                            }
                            else
                            {
                                AddScriptDetailNode(newNode, script, sgo, tgo);
                                var tscript = CheckContainsScript(script.name, tgo.scripts);
                                if (tscript == null)
                                {
                                    newNode.ForeColor = _Color.Red;
                                }
                                else if (!script.Equals(tscript))
                                {
                                    newNode.ForeColor = _Color.Red;
                                }
                            }
                            snode.Expand();
                        }
                        TreeNode _newNode = snode.Nodes.Insert(0, sgo.path + "/IsActive", m_scriptTag + "IsActive  " + sgo.isActive.ToString() );
                        _newNode.Tag = sgo.path + "/IsActive";
                        if( sgo.isActive != tgo.isActive )
                        {
                            _newNode.ForeColor = _Color.Red;
                        }
                    }
                    tnode.Expand();
                });
                AddMenuItem("清除组件", m_treeMenu.Items, (_s, _e) =>
                {
                    var count = tnode.Nodes.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (tnode.Nodes[i].Text.Contains(m_scriptTag))
                        {
                            tnode.Nodes.RemoveAt(i);
                        }
                    }
                    TreeNode[] snodes = m_treeSource.Nodes.Find(tgo.path, true);
                    if (snodes.Length > 0)
                    {
                        var _node = snodes[0];
                        var _count = _node.Nodes.Count;
                        for (int i = count - 1; i >= 0; i--)
                        {
                            if (_node.Nodes[i].Text.Contains(m_scriptTag))
                            {
                                _node.Nodes.RemoveAt(i);
                            }
                        }
                    }
                });
                AddMenuItem("拷贝节点", m_treeMenu.Items, (_s, _e) =>
                {
                    var goId = tgo.fileId;
                    ShowLoadingText("预制克隆中，可能需要加载Meta文件，请稍后...");
                    var srcPath = tgo.root.fileFullPath;
                    TreeNode[] snodes = m_treeSource.Nodes.Find(tgo.root.path, true);
                    GameObject srootgo = null;
                    TreeNode srootNode = null;
                    if (snodes.Length > 0)
                    {
                        srootNode = snodes[0];
                        srootgo = srootNode.Tag as GameObject;
                    }
                    if (srootgo == null)
                    {
                        MessageBox.Show("克隆失败，未找到目标根节点");
                        return;
                    }
                    var tarPath = srootgo.fileFullPath;
                    bool success = m_clonePrefab.ClonePrefabData(goId, srcPath, tarPath, false);
                    ShowLoadingText();
                    if (success)
                    {
                        MessageBox.Show("拷贝成功!");
                        Logic.RefreshOnePrefab(tgo.root.prefab, srootgo.prefab);
                        tgo = tgo.root.prefab.root;
                        srootgo = srootgo.prefab.root;
                        var trootNode = FindParentNode(tnode, tgo);
                        if (trootNode != null)
                        {
                            trootNode.Nodes.Clear();
                            for (int i = 0; i < tgo.childs.Count; i++)
                            {
                                RefreshPrefabTreeNodes(tgo.childs[i], trootNode.Nodes);
                            }
                        }
                        // var trootNode = FindParentNode(tnode, trootgo);
                        if (srootNode != null)
                        {
                            srootNode.Nodes.Clear();
                            for (int i = 0; i < srootgo.childs.Count; i++)
                            {
                                RefreshPrefabTreeNodes(srootgo.childs[i], srootNode.Nodes);
                            }
                        }
                        SetExtendState(trootNode);
                    }
                });
                AddMenuItem("删除节点", m_treeMenu.Items, (_s, _e) =>
                {
                    var result = MessageBox.Show("确认删除此节点？ Name:" + tgo.name, "提醒", MessageBoxButtons.OKCancel);
                    if (result != DialogResult.OK) return;
                    var goId = tgo.fileId;
                    var trootgo = tgo.root;
                    var tarPath = trootgo.fileFullPath;
                    var success = m_deletePrefab.DeletePrefabData(goId, tarPath);
                    if (success)
                    {
                        MessageBox.Show("删除节点成功!");
                        TreeNode[] snodes = m_treeSource.Nodes.Find(trootgo.path, true);
                        GameObject srootgo = null;
                        TreeNode srootNode = null;
                        if (snodes.Length > 0)
                        {
                            srootNode = snodes[0];
                            srootgo = srootNode.Tag as GameObject;
                        }
                        if (srootgo == null)
                        {
                            Logic.RefreshOnePrefab( null, trootgo.prefab);
                        }
                        else
                        {
                            Logic.RefreshOnePrefab(srootgo.prefab, trootgo.prefab);
                        }
                        tgo = tgo.root.prefab.root;
                        var trootNode = FindParentNode(tnode, tgo);
                        if (trootNode != null)
                        {
                            trootNode.Nodes.Clear();
                            for (int i = 0; i < tgo.childs.Count; i++)
                            {
                                RefreshPrefabTreeNodes(tgo.childs[i], trootNode.Nodes);
                            }
                        }
                        // var trootNode = FindParentNode(tnode, trootgo);
                        if (srootgo != null)
                        {
                            srootgo = srootgo.prefab.root;
                            srootNode.Nodes.Clear();
                            for (int i = 0; i < srootgo.childs.Count; i++)
                            {
                                RefreshPrefabTreeNodes(srootgo.childs[i], srootNode.Nodes);
                            }
                        }
                        SetExtendState(trootNode);
                    }
                });
            }
        }
        private Script CheckContainsScript( string name , List<Script> scripts )
        {
            if (scripts == null) return null;
            for (int i = 0; i < scripts.Count; i++)
            {
                if (scripts[i].name.Equals(name))
                    return scripts[i];
            }
            return null;
        }
        private ToolStripMenuItem AddMenuItem(string text, ToolStripItemCollection cms, EventHandler callBack = null , string tips = null )
        {
            if (!string.IsNullOrEmpty(text))
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem(text);
                if (callBack != null) tsmi.Click += callBack;
                tsmi.ToolTipText = tips;
                cms.Add(tsmi);
                return tsmi;
            }
            return null;
        }
        #endregion
        #endregion
        #region 异步相关
        private void m_asyncWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Logic.Begin();
        }
        private void m_asyncWorker_Complete(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            m_progress.Visible = false;
            m_textProgress.Visible = false;
            m_btnBegin.Enabled = true;
            RefreshTree();
        }
        private void m_asyncWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            int currPro = e.ProgressPercentage;
            if (currPro > m_progress.Maximum)
                currPro = m_progress.Maximum;
            m_progress.Value = currPro;
            m_textProgress.Text = Math.Floor(((double)currPro / m_progress.Maximum) * 100) + "%   " + Logic.CurrStateTips;
        }
        private void m_timer_Tick(object sender, EventArgs e)
        {
            TimeUpdate();
        }
        struct DelayFunction
        {
            public float time;
            public Action action;
        }
        float m_runTime = 0;
        List<DelayFunction> m_delayList = new List<DelayFunction>();
        List<int> m_deleteList = new List<int>();
        private void TimeUpdate()
        {
            m_runTime += m_timer.Interval;
            if (m_delayList.Count <= 0) return;
            m_deleteList.Clear();
            for (int i = 0; i < m_delayList.Count; i++)
            {
                if (m_runTime >= m_delayList[i].time)
                {
                    m_delayList[i].action();
                    m_deleteList.Add(i);
                }
            }
            for (int i = m_deleteList.Count - 1; i >= 0; i--)
            {
                m_delayList.RemoveAt(m_deleteList[i]);
            }
        }
        /// <summary>
        /// 延迟调用
        /// </summary>
        /// <param name="time">毫秒</param>
        /// <param name="action"></param>
        private void DelayFunc(float _time, Action _action)
        {
            if (_time <= 0)
            {
                _action();
                return;
            }
            m_delayList.Add(new DelayFunction()
            {
                time = m_runTime + _time,
                action = _action
            });
        }
        /// <summary>
        /// 延迟调用
        /// </summary>
        /// <param name="time">毫秒</param>
        /// <param name="action"></param>
        private void PostFunc(int i, Action _action)
        {
            if (i <= 0 )
            {
                _action();
                return;
            }
            m_delayList.Add(new DelayFunction()
            {
                time = m_runTime + i * m_timer.Interval,
                action = _action
            });
        }
        #endregion
        #region 私有逻辑
        private bool CheckCanBegin()
        {
            if( Logic.IsFileMode )
            {
                for (int i = 0; i < Logic.SourceFiles.Length; i++)
                {
                    if (!File.Exists(Logic.SourceFiles[i]))
                    {
                        MessageBox.Show("源文件不存在！" + Logic.SourceFiles[i] );
                        return false;
                    }
                }
                for (int i = 0; i < Logic.TargetFiles.Length; i++)
                {
                    if (!File.Exists(Logic.TargetFiles[i]))
                    {
                        MessageBox.Show("目标文件不存在！" + Logic.TargetFiles[i]);
                        return false;
                    }
                }
                if (Logic.m_isSvnCompare)
                {
                    if (!Directory.Exists(Logic.SourceClientPath))
                    {
                        MessageBox.Show("请设置正确的SVN项目路径");
                        return false;
                    }
                    if( !FileOP.Instance.ReadBool("Config" , "SVNClientPathRemind" , false ) )
                    {
                        var result = MessageBox.Show("由于SVN对比文件需要手动设置项目路径，请确认您已经设置好正确的SVN文件所在项目路径。(以后不再提醒请点<否>)", "提示", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            return true;
                        }
                        else
                        {
                            FileOP.Instance.WriteBool("Config", "SVNClientPathRemind", true);
                            return true;
                        }
                    }
                }
                return true;
            }
            if (!Directory.Exists(Logic.SourceFilePath))
            {
                MessageBox.Show("源目录不存在！");
                return false;
            }
            if (!Directory.Exists(Logic.TargetFilePath))
            {
                MessageBox.Show("目标目录不存在！");
                return false;
            }
            return true;
        }
        private TreeNode FindNodeByTag(string tag, TreeNode rootNode)
        {
            foreach (TreeNode node in rootNode.Nodes)
            {
                if (node.Tag.Equals(tag)) return node;
                TreeNode next = FindNodeByTag(tag, node);
                if (next != null) return next;
            }
            return null;
        }
        private TreeNode FindNodeByTag( string tag , TreeView tree )
        {
            TreeNode itemNode = null;
            foreach (TreeNode node in tree.Nodes)
            {
                itemNode = FindNodeByTag(tag, node);
                if (itemNode != null) break;
            }
            return itemNode;
        }
        private void ShowLoadingText(string text = null)
        {
            if (text != null)
            {
                m_textProgress.Visible = true;
                m_textProgress.Text = text;
            }
            else
            {
                m_textProgress.Visible = false;
                m_textProgress.Text = "";
            }
        }
        private void AddExtendDic( string path , bool extend )
        {
            if(m_extendNodeDic.ContainsKey(path))
            {
                m_extendNodeDic[path] = extend;
            }
            else
            {
                m_extendNodeDic.Add(path, extend);
            }
        }
        private bool GetExtendDic( string path )
        {
            if(m_extendNodeDic.ContainsKey(path))
            {
                return m_extendNodeDic[path];
            }
            return false;
        }
        #endregion

        private void PrefabContrastTool_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                var result = MessageBox.Show("EXE运行地址:" + Application.StartupPath + "\r\n配置缓存地址：" + Logic.APPDataPath + "\r\n \'是\'打开EXE运行地址,\'否\'打开配置缓存地址", "地址信息", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.No)
                {
                    System.Diagnostics.Process.Start(Logic.APPDataPath);
                }
                else if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(Application.StartupPath);
                }
            }
        }
        private List<TreeNode> m_searchTreeNodeList = new List<TreeNode>();
        private string m_lastSearch = null;
        private int m_searchIndex = 0;
        private void ResetSearchData()
        {
            m_searchTreeNodeList.Clear();
            m_lastSearch = null;
            m_searchIndex = 0;
        }
        private void m_btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(m_textSearch.Text)) return;
            if (m_lastSearch != m_textSearch.Text)
            {
                ResetSearchData();
                m_lastSearch = m_textSearch.Text;
                Regex regex = new Regex(m_textSearch.Text, RegexOptions.IgnoreCase);
                for (int i = 0; i < m_treeSource.Nodes.Count; i++)
                {
                    FindSearchNode(regex, m_treeSource.Nodes[i]);
                }
            }
            if (m_searchIndex < m_searchTreeNodeList.Count)
            {
                var node = m_searchTreeNodeList[m_searchIndex];
                m_treeSource.SelectedNode = node;
                if (m_treeSource.SelectedNode.Parent != null)
                    m_treeSource.SelectedNode.Parent.Expand();
                //m_tree.ToDrawNode(m_tree.SelectedNode);
                m_treeSource.Focus();
            }
            if (m_searchIndex >= m_searchTreeNodeList.Count - 1)
                m_searchIndex = 0;
            else
                m_searchIndex++;
        }
        private void FindSearchNode(Regex regex, TreeNode treeNode)
        {
            if (regex.IsMatch(treeNode.Text))
            {
                m_searchTreeNodeList.Add(treeNode);
            }
            for (int i = 0; i < treeNode.Nodes.Count; i++)
            {
                FindSearchNode(regex, treeNode.Nodes[i]);
            }
        }
        private void FillClientPathList()
        {
            m_comClientPathList.Items.Clear();
            m_clientPathList.Clear();
            var eleClientPath = XmlOp.Instance.GetXmlElement("ClientPath");
            if (eleClientPath == null) return;
            m_canAddClientPath = false;
            var list = XmlOp.Instance.GetXmlAllElement(eleClientPath);
            for (int i = 0; i < list.Count; i++)
            {
                m_clientPathList.Add(list[i].Value);
                m_comClientPathList.Items.Add(list[i].Value);
            }
            m_comClientPathList.SelectedIndex = 0;//FileOP.Instance.ReadInt("Config", "ClientPathIndex");
            m_canAddClientPath = true;
        }
        private bool m_canAddClientPath = true;
        private void AddClientPath( string path , bool isInput )
        {
            if (isInput)
            {
                path = Logic.m_regClientPath.Match(path).Value;
            }
            if (string.IsNullOrEmpty(path)) return;
            var count = m_clientPathList.Count;
            for (int i = 0; i < count; i++)
            {
                if (m_clientPathList[i] == path)
                {
                    if (isInput) return;
                    m_clientPathList.RemoveAt(i);
                    break;
                }
            }
            m_clientPathList.Insert(0,path);
            var eleClientPath = XmlOp.Instance.GetXmlElement("ClientPath");
            if (eleClientPath == null)
            {
                eleClientPath = XmlOp.Instance.AddXmlElement(new System.Xml.Linq.XElement("ClientPath" , "") );
            }
            count = Math.Min(m_clientPathList.Count, 10);
            for (int i = 0; i < count; i++)
            {
                XmlOp.Instance.SetOrAddElement("Path"+i.ToString(), m_clientPathList[i], eleClientPath);
            }
            FillClientPathList();
        }
        private void m_comClientPathList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = m_comClientPathList.SelectedIndex;
            if (index >= m_clientPathList.Count) return;
            //FileOP.Instance.WriteInt("Config", "ClientPathIndex", index);
            m_textClientPath.Text = m_clientPathList[index];
            if (m_initFinish && m_canAddClientPath)
            {
                AddClientPath(m_comClientPathList.Text, false);
            }
        }
        private void m_comClientPathList_TextChanged(object sender, EventArgs e)
        {
            m_textClientPath.Text = m_comClientPathList.Text;
            if (m_initFinish && m_canAddClientPath )
            {
                AddClientPath(m_comClientPathList.Text, true);
            }
        }

        private void PrefabContrastTool_SizeChanged(object sender, EventArgs e)
        {
            var range = 14;
            var interval = 6;
            var panelWidth = (Width - (range * 3) - interval) / 2;
            m_treeSource.Width = panelWidth;
            m_treeSource.Location = new System.Drawing.Point(range, m_treeSource.Location.Y);
            m_treeTarget.Width = panelWidth;
            m_treeTarget.Location = new System.Drawing.Point(m_treeSource.Location.X + m_treeSource.Width + interval, m_treeTarget.Location.Y);
        }
    }
}

namespace PrefabContrastTool
{
    partial class PrefabContrastTool
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.m_comSelectMode = new System.Windows.Forms.ComboBox();
            this.m_togShowAll = new System.Windows.Forms.CheckBox();
            this.m_togCheckScripts = new System.Windows.Forms.CheckBox();
            this.m_togCheckMaterial = new System.Windows.Forms.CheckBox();
            this.m_togCheckImage = new System.Windows.Forms.CheckBox();
            this.m_togCheckFont = new System.Windows.Forms.CheckBox();
            this.m_togCheckActive = new System.Windows.Forms.CheckBox();
            this.m_btnBegin = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.m_textSourcePath = new System.Windows.Forms.TextBox();
            this.m_textTargetPath = new System.Windows.Forms.TextBox();
            this.m_btnSelectSourceFile = new System.Windows.Forms.Button();
            this.m_btnSelectTargetFile = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.m_textSearch = new System.Windows.Forms.TextBox();
            this.m_btnSearch = new System.Windows.Forms.Button();
            this.m_treeSource = new System.Windows.Forms.TreeView();
            this.m_treeTarget = new System.Windows.Forms.TreeView();
            this.m_asyncWorker = new System.ComponentModel.BackgroundWorker();
            this.m_progress = new System.Windows.Forms.ProgressBar();
            this.m_textProgress = new System.Windows.Forms.Label();
            this.m_btnRefresh = new System.Windows.Forms.Button();
            this.m_treeMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.m_timer = new System.Windows.Forms.Timer(this.components);
            this.m_textClientPath = new System.Windows.Forms.TextBox();
            this.m_labelSvnClientPath = new System.Windows.Forms.Label();
            this.m_comClientPathList = new System.Windows.Forms.ComboBox();
            this.m_togIgnorePos = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "选择文件模式：";
            // 
            // m_comSelectMode
            // 
            this.m_comSelectMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_comSelectMode.FormattingEnabled = true;
            this.m_comSelectMode.Location = new System.Drawing.Point(98, 6);
            this.m_comSelectMode.Name = "m_comSelectMode";
            this.m_comSelectMode.Size = new System.Drawing.Size(121, 20);
            this.m_comSelectMode.TabIndex = 1;
            this.m_comSelectMode.SelectedIndexChanged += new System.EventHandler(this.m_comSelectMode_SelectedIndexChanged);
            // 
            // m_togShowAll
            // 
            this.m_togShowAll.AutoSize = true;
            this.m_togShowAll.Location = new System.Drawing.Point(238, 9);
            this.m_togShowAll.Name = "m_togShowAll";
            this.m_togShowAll.Size = new System.Drawing.Size(72, 16);
            this.m_togShowAll.TabIndex = 2;
            this.m_togShowAll.Text = "显示全部";
            this.m_togShowAll.UseVisualStyleBackColor = true;
            this.m_togShowAll.CheckedChanged += new System.EventHandler(this.m_togShowAll_CheckedChanged);
            // 
            // m_togCheckScripts
            // 
            this.m_togCheckScripts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_togCheckScripts.AutoSize = true;
            this.m_togCheckScripts.ForeColor = System.Drawing.Color.Red;
            this.m_togCheckScripts.Location = new System.Drawing.Point(316, 9);
            this.m_togCheckScripts.Name = "m_togCheckScripts";
            this.m_togCheckScripts.Size = new System.Drawing.Size(72, 16);
            this.m_togCheckScripts.TabIndex = 3;
            this.m_togCheckScripts.Text = "对比脚本";
            this.m_togCheckScripts.UseVisualStyleBackColor = true;
            this.m_togCheckScripts.CheckedChanged += new System.EventHandler(this.m_togCheckScripts_CheckedChanged);
            // 
            // m_togCheckMaterial
            // 
            this.m_togCheckMaterial.AutoSize = true;
            this.m_togCheckMaterial.ForeColor = System.Drawing.Color.Blue;
            this.m_togCheckMaterial.Location = new System.Drawing.Point(394, 9);
            this.m_togCheckMaterial.Name = "m_togCheckMaterial";
            this.m_togCheckMaterial.Size = new System.Drawing.Size(72, 16);
            this.m_togCheckMaterial.TabIndex = 4;
            this.m_togCheckMaterial.Text = "对比材质";
            this.m_togCheckMaterial.UseVisualStyleBackColor = true;
            this.m_togCheckMaterial.CheckedChanged += new System.EventHandler(this.m_togCheckMaterial_CheckedChanged);
            // 
            // m_togCheckImage
            // 
            this.m_togCheckImage.AutoSize = true;
            this.m_togCheckImage.ForeColor = System.Drawing.Color.Green;
            this.m_togCheckImage.Location = new System.Drawing.Point(472, 9);
            this.m_togCheckImage.Name = "m_togCheckImage";
            this.m_togCheckImage.Size = new System.Drawing.Size(72, 16);
            this.m_togCheckImage.TabIndex = 5;
            this.m_togCheckImage.Text = "对比图片";
            this.m_togCheckImage.UseVisualStyleBackColor = true;
            this.m_togCheckImage.CheckedChanged += new System.EventHandler(this.m_togCheckImage_CheckedChanged);
            // 
            // m_togCheckFont
            // 
            this.m_togCheckFont.AutoSize = true;
            this.m_togCheckFont.ForeColor = System.Drawing.Color.Orange;
            this.m_togCheckFont.Location = new System.Drawing.Point(550, 9);
            this.m_togCheckFont.Name = "m_togCheckFont";
            this.m_togCheckFont.Size = new System.Drawing.Size(72, 16);
            this.m_togCheckFont.TabIndex = 6;
            this.m_togCheckFont.Text = "对比字体";
            this.m_togCheckFont.UseVisualStyleBackColor = true;
            this.m_togCheckFont.CheckedChanged += new System.EventHandler(this.m_togCheckFont_CheckedChanged);
            // 
            // m_togCheckActive
            // 
            this.m_togCheckActive.AutoSize = true;
            this.m_togCheckActive.ForeColor = System.Drawing.Color.Purple;
            this.m_togCheckActive.Location = new System.Drawing.Point(628, 9);
            this.m_togCheckActive.Name = "m_togCheckActive";
            this.m_togCheckActive.Size = new System.Drawing.Size(108, 16);
            this.m_togCheckActive.TabIndex = 7;
            this.m_togCheckActive.Text = "对比Active状态";
            this.m_togCheckActive.UseVisualStyleBackColor = true;
            this.m_togCheckActive.CheckedChanged += new System.EventHandler(this.m_togCheckActive_CheckedChanged);
            // 
            // m_btnBegin
            // 
            this.m_btnBegin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnBegin.Location = new System.Drawing.Point(931, 3);
            this.m_btnBegin.Name = "m_btnBegin";
            this.m_btnBegin.Size = new System.Drawing.Size(75, 23);
            this.m_btnBegin.TabIndex = 8;
            this.m_btnBegin.Text = "开始对比";
            this.m_btnBegin.UseVisualStyleBackColor = true;
            this.m_btnBegin.Click += new System.EventHandler(this.m_btnBegin_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "源文件路径：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "对比文件路径：";
            // 
            // m_textSourcePath
            // 
            this.m_textSourcePath.AllowDrop = true;
            this.m_textSourcePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_textSourcePath.Location = new System.Drawing.Point(98, 31);
            this.m_textSourcePath.Name = "m_textSourcePath";
            this.m_textSourcePath.Size = new System.Drawing.Size(819, 21);
            this.m_textSourcePath.TabIndex = 11;
            this.m_textSourcePath.MouseClick += new System.Windows.Forms.MouseEventHandler(this.m_textSourcePath_MouseClick);
            this.m_textSourcePath.TextChanged += new System.EventHandler(this.m_textSourcePath_TextChanged);
            this.m_textSourcePath.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_textSourcePath_DragDrop);
            this.m_textSourcePath.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_textSourcePath_DragEnter);
            // 
            // m_textTargetPath
            // 
            this.m_textTargetPath.AllowDrop = true;
            this.m_textTargetPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_textTargetPath.Location = new System.Drawing.Point(98, 58);
            this.m_textTargetPath.Name = "m_textTargetPath";
            this.m_textTargetPath.Size = new System.Drawing.Size(819, 21);
            this.m_textTargetPath.TabIndex = 12;
            this.m_textTargetPath.MouseClick += new System.Windows.Forms.MouseEventHandler(this.m_textTargetPath_MouseClick);
            this.m_textTargetPath.TextChanged += new System.EventHandler(this.m_textTargetPath_TextChanged);
            this.m_textTargetPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_textTargetPath_DragDrop);
            this.m_textTargetPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_textTargetPath_DragEnter);
            // 
            // m_btnSelectSourceFile
            // 
            this.m_btnSelectSourceFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnSelectSourceFile.Location = new System.Drawing.Point(931, 29);
            this.m_btnSelectSourceFile.Name = "m_btnSelectSourceFile";
            this.m_btnSelectSourceFile.Size = new System.Drawing.Size(75, 23);
            this.m_btnSelectSourceFile.TabIndex = 13;
            this.m_btnSelectSourceFile.Text = "选择文件";
            this.m_btnSelectSourceFile.UseVisualStyleBackColor = true;
            this.m_btnSelectSourceFile.Click += new System.EventHandler(this.m_btnSelectSourceFile_Click);
            // 
            // m_btnSelectTargetFile
            // 
            this.m_btnSelectTargetFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnSelectTargetFile.Location = new System.Drawing.Point(931, 56);
            this.m_btnSelectTargetFile.Name = "m_btnSelectTargetFile";
            this.m_btnSelectTargetFile.Size = new System.Drawing.Size(75, 23);
            this.m_btnSelectTargetFile.TabIndex = 14;
            this.m_btnSelectTargetFile.Text = "选择文件";
            this.m_btnSelectTargetFile.UseVisualStyleBackColor = true;
            this.m_btnSelectTargetFile.Click += new System.EventHandler(this.m_btnSelectTargetFile_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.SystemColors.Control;
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Location = new System.Drawing.Point(14, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(797, 12);
            this.label4.TabIndex = 15;
            this.label4.Text = "提示：红底表示预制名不一致或路径不一致。组件材质等有差异用字体颜色表示，同描述颜色。显示的路径代表一定有差异，没有差异的路径不显示。";
            // 
            // m_textSearch
            // 
            this.m_textSearch.Location = new System.Drawing.Point(14, 120);
            this.m_textSearch.Name = "m_textSearch";
            this.m_textSearch.Size = new System.Drawing.Size(118, 21);
            this.m_textSearch.TabIndex = 17;
            // 
            // m_btnSearch
            // 
            this.m_btnSearch.Location = new System.Drawing.Point(140, 119);
            this.m_btnSearch.Name = "m_btnSearch";
            this.m_btnSearch.Size = new System.Drawing.Size(75, 23);
            this.m_btnSearch.TabIndex = 18;
            this.m_btnSearch.Text = "搜索";
            this.m_btnSearch.UseVisualStyleBackColor = true;
            this.m_btnSearch.Click += new System.EventHandler(this.m_btnSearch_Click);
            // 
            // m_treeSource
            // 
            this.m_treeSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_treeSource.Location = new System.Drawing.Point(14, 147);
            this.m_treeSource.Name = "m_treeSource";
            this.m_treeSource.Size = new System.Drawing.Size(499, 298);
            this.m_treeSource.TabIndex = 19;
            this.m_treeSource.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.m_treeSource_AfterCollapse);
            this.m_treeSource.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.m_treeSource_AfterExpand);
            this.m_treeSource.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.m_treeSource_AfterSelect);
            this.m_treeSource.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.m_treeSource_NodeMouseClick);
            // 
            // m_treeTarget
            // 
            this.m_treeTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_treeTarget.Location = new System.Drawing.Point(519, 147);
            this.m_treeTarget.Name = "m_treeTarget";
            this.m_treeTarget.Size = new System.Drawing.Size(499, 298);
            this.m_treeTarget.TabIndex = 20;
            this.m_treeTarget.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.m_treeTarget_AfterCollapse);
            this.m_treeTarget.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.m_treeTarget_AfterExpand);
            this.m_treeTarget.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.m_treeTarget_AfterSelect);
            this.m_treeTarget.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.m_treeTarget_NodeMouseClick);
            // 
            // m_asyncWorker
            // 
            this.m_asyncWorker.WorkerReportsProgress = true;
            this.m_asyncWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.m_asyncWorker_DoWork);
            this.m_asyncWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.m_asyncWorker_ProgressChanged);
            this.m_asyncWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.m_asyncWorker_Complete);
            // 
            // m_progress
            // 
            this.m_progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_progress.Location = new System.Drawing.Point(14, 422);
            this.m_progress.Name = "m_progress";
            this.m_progress.Size = new System.Drawing.Size(992, 23);
            this.m_progress.TabIndex = 21;
            // 
            // m_textProgress
            // 
            this.m_textProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_textProgress.AutoSize = true;
            this.m_textProgress.Location = new System.Drawing.Point(20, 407);
            this.m_textProgress.Name = "m_textProgress";
            this.m_textProgress.Size = new System.Drawing.Size(59, 12);
            this.m_textProgress.TabIndex = 22;
            this.m_textProgress.Text = "加载中...";
            // 
            // m_btnRefresh
            // 
            this.m_btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnRefresh.Location = new System.Drawing.Point(842, 3);
            this.m_btnRefresh.Name = "m_btnRefresh";
            this.m_btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.m_btnRefresh.TabIndex = 23;
            this.m_btnRefresh.Text = "刷新";
            this.m_btnRefresh.UseVisualStyleBackColor = true;
            this.m_btnRefresh.Click += new System.EventHandler(this.m_btnRefresh_Click);
            // 
            // m_treeMenu
            // 
            this.m_treeMenu.Name = "m_treeMenu";
            this.m_treeMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // m_timer
            // 
            this.m_timer.Tick += new System.EventHandler(this.m_timer_Tick);
            // 
            // m_textClientPath
            // 
            this.m_textClientPath.AllowDrop = true;
            this.m_textClientPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_textClientPath.Location = new System.Drawing.Point(493, 121);
            this.m_textClientPath.Name = "m_textClientPath";
            this.m_textClientPath.Size = new System.Drawing.Size(512, 21);
            this.m_textClientPath.TabIndex = 24;
            this.m_textClientPath.MouseClick += new System.Windows.Forms.MouseEventHandler(this.m_textClientPath_MouseClick);
            this.m_textClientPath.TextChanged += new System.EventHandler(this.m_textClientPath_TextChanged);
            // 
            // m_labelSvnClientPath
            // 
            this.m_labelSvnClientPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_labelSvnClientPath.AutoSize = true;
            this.m_labelSvnClientPath.Location = new System.Drawing.Point(610, 106);
            this.m_labelSvnClientPath.Name = "m_labelSvnClientPath";
            this.m_labelSvnClientPath.Size = new System.Drawing.Size(395, 12);
            this.m_labelSvnClientPath.TabIndex = 25;
            this.m_labelSvnClientPath.Text = "（仅对比SVN差异时使用）项目路径，复制当前正在比较的文件夹路径即可";
            this.m_labelSvnClientPath.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_comClientPathList
            // 
            this.m_comClientPathList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_comClientPathList.BackColor = System.Drawing.SystemColors.Window;
            this.m_comClientPathList.FormattingEnabled = true;
            this.m_comClientPathList.Location = new System.Drawing.Point(493, 122);
            this.m_comClientPathList.Name = "m_comClientPathList";
            this.m_comClientPathList.Size = new System.Drawing.Size(513, 20);
            this.m_comClientPathList.TabIndex = 26;
            this.m_comClientPathList.SelectedIndexChanged += new System.EventHandler(this.m_comClientPathList_SelectedIndexChanged);
            this.m_comClientPathList.TextChanged += new System.EventHandler(this.m_comClientPathList_TextChanged);
            // 
            // m_togIgnorePos
            // 
            this.m_togIgnorePos.AutoSize = true;
            this.m_togIgnorePos.ForeColor = System.Drawing.Color.Black;
            this.m_togIgnorePos.Location = new System.Drawing.Point(742, 8);
            this.m_togIgnorePos.Name = "m_togIgnorePos";
            this.m_togIgnorePos.Size = new System.Drawing.Size(72, 16);
            this.m_togIgnorePos.TabIndex = 27;
            this.m_togIgnorePos.Text = "忽略偏移";
            this.m_togIgnorePos.UseVisualStyleBackColor = true;
            this.m_togIgnorePos.CheckedChanged += new System.EventHandler(this.m_togIgnorePos_CheckedChanged);
            // 
            // PrefabContrastTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1017, 450);
            this.Controls.Add(this.m_togIgnorePos);
            this.Controls.Add(this.m_comClientPathList);
            this.Controls.Add(this.m_labelSvnClientPath);
            this.Controls.Add(this.m_textClientPath);
            this.Controls.Add(this.m_btnRefresh);
            this.Controls.Add(this.m_textProgress);
            this.Controls.Add(this.m_progress);
            this.Controls.Add(this.m_treeTarget);
            this.Controls.Add(this.m_treeSource);
            this.Controls.Add(this.m_btnSearch);
            this.Controls.Add(this.m_textSearch);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.m_btnSelectTargetFile);
            this.Controls.Add(this.m_btnSelectSourceFile);
            this.Controls.Add(this.m_textTargetPath);
            this.Controls.Add(this.m_textSourcePath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_btnBegin);
            this.Controls.Add(this.m_togCheckActive);
            this.Controls.Add(this.m_togCheckFont);
            this.Controls.Add(this.m_togCheckImage);
            this.Controls.Add(this.m_togCheckMaterial);
            this.Controls.Add(this.m_togCheckScripts);
            this.Controls.Add(this.m_togShowAll);
            this.Controls.Add(this.m_comSelectMode);
            this.Controls.Add(this.label1);
            this.KeyPreview = true;
            this.Name = "PrefabContrastTool";
            this.Text = "预制对比工具";
            this.SizeChanged += new System.EventHandler(this.PrefabContrastTool_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PrefabContrastTool_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox m_comSelectMode;
        private System.Windows.Forms.CheckBox m_togShowAll;
        private System.Windows.Forms.CheckBox m_togCheckScripts;
        private System.Windows.Forms.CheckBox m_togCheckMaterial;
        private System.Windows.Forms.CheckBox m_togCheckImage;
        private System.Windows.Forms.CheckBox m_togCheckFont;
        private System.Windows.Forms.CheckBox m_togCheckActive;
        private System.Windows.Forms.Button m_btnBegin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_textSourcePath;
        private System.Windows.Forms.TextBox m_textTargetPath;
        private System.Windows.Forms.Button m_btnSelectSourceFile;
        private System.Windows.Forms.Button m_btnSelectTargetFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox m_textSearch;
        private System.Windows.Forms.Button m_btnSearch;
        private System.Windows.Forms.TreeView m_treeSource;
        private System.Windows.Forms.TreeView m_treeTarget;
        private System.ComponentModel.BackgroundWorker m_asyncWorker;
        private System.Windows.Forms.ProgressBar m_progress;
        private System.Windows.Forms.Label m_textProgress;
        private System.Windows.Forms.Button m_btnRefresh;
        private System.Windows.Forms.ContextMenuStrip m_treeMenu;
        private System.Windows.Forms.Timer m_timer;
        private System.Windows.Forms.TextBox m_textClientPath;
        private System.Windows.Forms.Label m_labelSvnClientPath;
        private System.Windows.Forms.ComboBox m_comClientPathList;
        private System.Windows.Forms.CheckBox m_togIgnorePos;
    }
}


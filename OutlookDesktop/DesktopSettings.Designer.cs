namespace OutlookDesktop
{
    partial class DesktopSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Instances");
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.uxSettingsTree = new System.Windows.Forms.TreeView();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.uxSettingsTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.splitContainer1.Size = new System.Drawing.Size(600, 494);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 0;
            // 
            // uxSettingsTree
            // 
            this.uxSettingsTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uxSettingsTree.Location = new System.Drawing.Point(0, 0);
            this.uxSettingsTree.Name = "uxSettingsTree";
            treeNode1.Name = "instanceRootNode";
            treeNode1.Text = "Instances";
            this.uxSettingsTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.uxSettingsTree.Size = new System.Drawing.Size(200, 494);
            this.uxSettingsTree.TabIndex = 0;
            this.uxSettingsTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.uxSettingsTree_AfterSelect);
            // 
            // DesktopSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 494);
            this.Controls.Add(this.splitContainer1);
            this.Name = "DesktopSettings";
            this.Text = "Outlook on the Desktop Settings";
            this.Load += new System.EventHandler(this.DesktopSettings_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView uxSettingsTree;
    }
}
using OutlookDesktop.Properties;

namespace OutlookDesktop.Forms
{
    partial class AboutBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.labelProductName = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.linkWebsite = new System.Windows.Forms.LinkLabel();
            this.picDonate = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picDonate)).BeginInit();
            this.SuspendLayout();
            // 
            // labelProductName
            // 
            this.labelProductName.AutoSize = true;
            this.labelProductName.Location = new System.Drawing.Point(15, 9);
            this.labelProductName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelProductName.MaximumSize = new System.Drawing.Size(0, 17);
            this.labelProductName.Name = "labelProductName";
            this.labelProductName.Size = new System.Drawing.Size(120, 13);
            this.labelProductName.TabIndex = 26;
            this.labelProductName.Text = "Outlook on the Desktop";
            this.labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(15, 36);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelVersion.MaximumSize = new System.Drawing.Size(0, 17);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(42, 13);
            this.labelVersion.TabIndex = 25;
            this.labelVersion.Text = "Version";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCopyright
            // 
            this.labelCopyright.AutoSize = true;
            this.labelCopyright.Location = new System.Drawing.Point(15, 64);
            this.labelCopyright.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelCopyright.MaximumSize = new System.Drawing.Size(0, 17);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(51, 13);
            this.labelCopyright.TabIndex = 27;
            this.labelCopyright.Text = "Copyright";
            this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(105, 170);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 22);
            this.okButton.TabIndex = 28;
            this.okButton.Text = global::OutlookDesktop.Properties.Resources.OKButton;
            // 
            // linkWebsite
            // 
            this.linkWebsite.AutoSize = true;
            this.linkWebsite.Location = new System.Drawing.Point(15, 92);
            this.linkWebsite.Name = "linkWebsite";
            this.linkWebsite.Size = new System.Drawing.Size(51, 13);
            this.linkWebsite.TabIndex = 30;
            this.linkWebsite.TabStop = true;
            this.linkWebsite.Text = "Web Site";
            this.linkWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkWebsite_LinkClicked);
            // 
            // picDonate
            // 
            this.picDonate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picDonate.Image = global::OutlookDesktop.Properties.Resources.paypal;
            this.picDonate.Location = new System.Drawing.Point(111, 120);
            this.picDonate.Name = "picDonate";
            this.picDonate.Size = new System.Drawing.Size(62, 31);
            this.picDonate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picDonate.TabIndex = 31;
            this.picDonate.TabStop = false;
            this.picDonate.Click += new System.EventHandler(this.PicDonate_Click);
            // 
            // AboutBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 204);
            this.Controls.Add(this.picDonate);
            this.Controls.Add(this.linkWebsite);
            this.Controls.Add(this.labelProductName);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelCopyright);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About Outlook on the Desktop";
            ((System.ComponentModel.ISupportInitialize)(this.picDonate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.LinkLabel linkWebsite;
        private System.Windows.Forms.PictureBox picDonate;

    }
}
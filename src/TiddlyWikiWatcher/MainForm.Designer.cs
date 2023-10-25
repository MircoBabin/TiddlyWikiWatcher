namespace TiddlyWikiWatcher
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.FilenameTextbox = new System.Windows.Forms.TextBox();
            this.FilenameLabel = new System.Windows.Forms.Label();
            this.FilenameSelect = new System.Windows.Forms.Button();
            this.FilenameOpen = new System.Windows.Forms.Button();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.SuspendLayout();
            // 
            // FilenameTextbox
            // 
            this.FilenameTextbox.Location = new System.Drawing.Point(95, 3);
            this.FilenameTextbox.Name = "FilenameTextbox";
            this.FilenameTextbox.Size = new System.Drawing.Size(932, 22);
            this.FilenameTextbox.TabIndex = 0;
            // 
            // FilenameLabel
            // 
            this.FilenameLabel.AutoSize = true;
            this.FilenameLabel.Location = new System.Drawing.Point(15, 6);
            this.FilenameLabel.Name = "FilenameLabel";
            this.FilenameLabel.Size = new System.Drawing.Size(76, 17);
            this.FilenameLabel.TabIndex = 1;
            this.FilenameLabel.Text = "Tiddly Wiki";
            // 
            // FilenameSelect
            // 
            this.FilenameSelect.Location = new System.Drawing.Point(1033, 3);
            this.FilenameSelect.Name = "FilenameSelect";
            this.FilenameSelect.Size = new System.Drawing.Size(75, 23);
            this.FilenameSelect.TabIndex = 2;
            this.FilenameSelect.Text = "Select...";
            this.FilenameSelect.UseVisualStyleBackColor = true;
            this.FilenameSelect.Click += new System.EventHandler(this.FilenameSelect_Click);
            // 
            // FilenameOpen
            // 
            this.FilenameOpen.Location = new System.Drawing.Point(95, 31);
            this.FilenameOpen.Name = "FilenameOpen";
            this.FilenameOpen.Size = new System.Drawing.Size(91, 29);
            this.FilenameOpen.TabIndex = 3;
            this.FilenameOpen.Text = "Open";
            this.FilenameOpen.UseVisualStyleBackColor = true;
            this.FilenameOpen.Click += new System.EventHandler(this.FilenameOpen_Click);
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.DefaultExt = "html";
            // 
            // webView
            // 
            this.webView.AllowExternalDrop = true;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView.Location = new System.Drawing.Point(1, 32);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(1119, 598);
            this.webView.TabIndex = 5;
            this.webView.ZoomFactor = 1D;
            this.webView.CoreWebView2InitializationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs>(this.webView_CoreWebView2InitializationCompleted);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1120, 629);
            this.Controls.Add(this.FilenameOpen);
            this.Controls.Add(this.FilenameSelect);
            this.Controls.Add(this.FilenameLabel);
            this.Controls.Add(this.FilenameTextbox);
            this.Controls.Add(this.webView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Tiddly Wiki Watcher";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox FilenameTextbox;
        private System.Windows.Forms.Label FilenameLabel;
        private System.Windows.Forms.Button FilenameSelect;
        private System.Windows.Forms.Button FilenameOpen;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
    }
}
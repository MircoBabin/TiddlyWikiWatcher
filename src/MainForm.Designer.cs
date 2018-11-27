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
            this.FilenameWatchingLabel = new System.Windows.Forms.Label();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.LogListbox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // FilenameTextbox
            // 
            this.FilenameTextbox.Location = new System.Drawing.Point(97, 22);
            this.FilenameTextbox.Name = "FilenameTextbox";
            this.FilenameTextbox.Size = new System.Drawing.Size(932, 22);
            this.FilenameTextbox.TabIndex = 0;
            // 
            // FilenameLabel
            // 
            this.FilenameLabel.AutoSize = true;
            this.FilenameLabel.Location = new System.Drawing.Point(15, 25);
            this.FilenameLabel.Name = "FilenameLabel";
            this.FilenameLabel.Size = new System.Drawing.Size(76, 17);
            this.FilenameLabel.TabIndex = 1;
            this.FilenameLabel.Text = "Tiddly Wiki";
            // 
            // FilenameSelect
            // 
            this.FilenameSelect.Location = new System.Drawing.Point(1033, 22);
            this.FilenameSelect.Name = "FilenameSelect";
            this.FilenameSelect.Size = new System.Drawing.Size(75, 23);
            this.FilenameSelect.TabIndex = 2;
            this.FilenameSelect.Text = "Select...";
            this.FilenameSelect.UseVisualStyleBackColor = true;
            this.FilenameSelect.Click += new System.EventHandler(this.FilenameSelect_Click);
            // 
            // FilenameOpen
            // 
            this.FilenameOpen.Location = new System.Drawing.Point(97, 50);
            this.FilenameOpen.Name = "FilenameOpen";
            this.FilenameOpen.Size = new System.Drawing.Size(200, 29);
            this.FilenameOpen.TabIndex = 3;
            this.FilenameOpen.Text = "Open Tiddly Wiki in browser";
            this.FilenameOpen.UseVisualStyleBackColor = true;
            this.FilenameOpen.Click += new System.EventHandler(this.FilenameOpen_Click);
            // 
            // FilenameWatchingLabel
            // 
            this.FilenameWatchingLabel.AutoSize = true;
            this.FilenameWatchingLabel.Location = new System.Drawing.Point(303, 56);
            this.FilenameWatchingLabel.Name = "FilenameWatchingLabel";
            this.FilenameWatchingLabel.Size = new System.Drawing.Size(67, 17);
            this.FilenameWatchingLabel.TabIndex = 4;
            this.FilenameWatchingLabel.Text = "Watching";
            this.FilenameWatchingLabel.Visible = false;
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.DefaultExt = "html";
            // 
            // LogListbox
            // 
            this.LogListbox.FormattingEnabled = true;
            this.LogListbox.ItemHeight = 16;
            this.LogListbox.Location = new System.Drawing.Point(97, 85);
            this.LogListbox.Name = "LogListbox";
            this.LogListbox.Size = new System.Drawing.Size(1011, 132);
            this.LogListbox.TabIndex = 5;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1120, 221);
            this.Controls.Add(this.LogListbox);
            this.Controls.Add(this.FilenameWatchingLabel);
            this.Controls.Add(this.FilenameOpen);
            this.Controls.Add(this.FilenameSelect);
            this.Controls.Add(this.FilenameLabel);
            this.Controls.Add(this.FilenameTextbox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Tiddly Wiki save Watcher";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox FilenameTextbox;
        private System.Windows.Forms.Label FilenameLabel;
        private System.Windows.Forms.Button FilenameSelect;
        private System.Windows.Forms.Button FilenameOpen;
        private System.Windows.Forms.Label FilenameWatchingLabel;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.ListBox LogListbox;
    }
}
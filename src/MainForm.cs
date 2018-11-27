using System;
using System.IO;
using System.Windows.Forms;

namespace TiddlyWikiWatcher
{
    public partial class MainForm : Form, ITiddlyWikiWatcherLogger
    {
        private bool _watching = false;
        private ITiddlyWikiWatcher _watcher = null;

        private MainForm()
        {
            InitializeComponent();

            FilenameTextbox.Text = AppSettings_LoadFilename();
        }

        public MainForm(ITiddlyWikiWatcher watcher, string filename, bool autoopen) : this()
        {
            _watcher = watcher;
            if (!String.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                FilenameTextbox.Text = filename;

                if (autoopen) Open();
            }
        }

        private void FilenameSelect_Click(object sender, EventArgs e)
        {
            //OpenFileDialog.InitialDirectory = @"C:\";

            OpenFileDialog.Title = "Select Tiddly Wiki file";

            OpenFileDialog.CheckFileExists = true;
            OpenFileDialog.CheckPathExists = true;

            OpenFileDialog.DefaultExt = "html";

            OpenFileDialog.Filter = "Tiddly Wiki file (*.html)|*.html";
            OpenFileDialog.FilterIndex = 1;

            OpenFileDialog.RestoreDirectory = true;

            OpenFileDialog.ReadOnlyChecked = false;
            OpenFileDialog.ShowReadOnly = false;

            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                FilenameTextbox.Text = OpenFileDialog.FileName;
            }
        }

        private string AppSettings_LoadFilename()
        {
            try
            {
                return global::TiddlyWikiWatcher.Properties.Settings.Default.TiddlyWikiFilename;
            }
            catch { }

            try
            {
                global::TiddlyWikiWatcher.Properties.Settings.Default.Upgrade();
                global::TiddlyWikiWatcher.Properties.Settings.Default.Save();
                global::TiddlyWikiWatcher.Properties.Settings.Default.Reload();

                return global::TiddlyWikiWatcher.Properties.Settings.Default.TiddlyWikiFilename;
            }
            catch
            {
                return String.Empty;
            }
        }

        private void AppSettings_SaveFilename(string filename)
        {
            try
            {
                global::TiddlyWikiWatcher.Properties.Settings.Default.TiddlyWikiFilename = filename;
                global::TiddlyWikiWatcher.Properties.Settings.Default.Save();
                global::TiddlyWikiWatcher.Properties.Settings.Default.Reload();
            }
            catch { }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0) return;

            FilenameTextbox.Text = files[files.Length - 1];
        }

        private void FilenameOpen_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _watcher.Dispose();
        }

        private void Open()
        {
            var filename = FilenameTextbox.Text;

            if (!_watching)
            {
                var downloadsPath = KnownFolderPaths.KnownFolders.GetPath(KnownFolderPaths.KnownFolder.Downloads);

                if (!File.Exists(filename))
                {
                    MessageBox.Show("Filename does not exist", this.Text);
                    return;
                }

                FilenameTextbox.Enabled = false;
                FilenameSelect.Enabled = false;

                FilenameWatchingLabel.Visible = true;
                FilenameWatchingLabel.Text += ": " + downloadsPath;

                AppSettings_SaveFilename(filename);

                _watcher.Start(filename, downloadsPath, this);
                _watching = true;
            }

            System.Diagnostics.Process.Start(filename);
            this.WindowState = FormWindowState.Minimized;
        }

        private delegate void TiddlyWikiWatcher_LogDelegate(string text);
        public void TiddlyWikiWatcher_Log(string text)
        {
            if (LogListbox.InvokeRequired)
            {
                TiddlyWikiWatcher_LogDelegate d = new TiddlyWikiWatcher_LogDelegate(TiddlyWikiWatcher_Log);
                LogListbox.Invoke(d, new object[] { text });
            }
            else
            {
                if (text.Length == 0)
                    LogListbox.Items.Add("");
                else
                    LogListbox.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text);

                LogListbox.SelectedIndex = LogListbox.Items.Count - 1;
            }
        }
    }
}

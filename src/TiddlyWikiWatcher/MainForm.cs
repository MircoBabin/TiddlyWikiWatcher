using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace TiddlyWikiWatcher
{
    public partial class MainForm : Form //, ITiddlyWikiWatcherLogger
    {
        private bool _watching = false;
        private DownloadedFileHandler _downloadHandler;

        private MainForm()
        {
            InitializeComponent();

            _downloadHandler = null;

            FilenameTextbox.Text = AppSettings_LoadFilename();

            WindowState = FormWindowState.Maximized;
        }

        public MainForm(string filename, bool autoopen) : this()
        {
            if (!String.IsNullOrEmpty(filename))
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
            if (_downloadHandler != null)
            {
                _downloadHandler.Dispose();
            }
        }

        private void Open()
        {
            var filename = FilenameTextbox.Text;

            if (_watching) return;

            if (!File.Exists(filename))
            {
                MessageBox.Show("Filename does not exist", this.Text);
                return;
            }

            FilenameOpen.Visible = false;
            FilenameOpen.Enabled = false;
            FilenameTextbox.Enabled = false;
            FilenameSelect.Visible = false;
            FilenameSelect.Enabled = false;
            AppSettings_SaveFilename(filename);

            // var downloadsPath = KnownFolderPaths.KnownFolders.GetPath(KnownFolderPaths.KnownFolder.Downloads);
            _watching = true;
            _downloadHandler = new DownloadedFileHandler(filename, null);
            webView.Source = new System.Uri(filename);
        }

        /*
        public void TiddlyWikiWatcher_Log(string text)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (text.Length == 0)
                    LogListbox.Items.Add("");
                else
                    LogListbox.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text);

                LogListbox.SelectedIndex = LogListbox.Items.Count - 1;
            });
        }
        */

        private void MainForm_Resize(object sender, EventArgs e)
        {
            webView.Size = this.ClientSize - new System.Drawing.Size(webView.Location);
            /*
            goButton.Left = this.ClientSize.Width - goButton.Width;
            addressBar.Width = goButton.Left - addressBar.Left;
            */
        }

        private void webView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            webView.CoreWebView2.DownloadStarting += webView_DownloadStarting;
        }

        private void webView_DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            var download = e.DownloadOperation;

            download.StateChanged += delegate (object _sender, Object ev)
            {
                webView.CoreWebView2.CloseDefaultDownloadDialog();

                if (download.State == CoreWebView2DownloadState.Completed)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        _downloadHandler.DownloadsWatcher_HandleFile(download.ResultFilePath);
                    });
                }
            }; 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace TiddlyWikiWatcher
{
    public partial class MainForm : Form //, ITiddlyWikiWatcherLogger
    {
        private Mutex singleInstanceMutex;
        private bool _watching = false;
        private DownloadedFileHandler _downloadHandler;
        private List<CoreWebView2DownloadOperation> _downloadsBusy = new List<CoreWebView2DownloadOperation>();

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

                if (autoopen)
                {
                    Open();
                    if (!_watching)
                    {
                        Application.Exit();
                        Environment.Exit(0);
                    }
                }
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (_downloadsBusy)
            {
                if (_downloadsBusy.Count > 0)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (_downloadHandler != null)
            {
                if (_downloadHandler.IsBusy())
                {
                    e.Cancel = true;
                    return;
                }
            }

            e.Cancel = false;
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

            if (!IsSingleInstance(filename))
            {
                MessageBox.Show("Filename is opened in another instance of Tiddly Wiki Watcher.");
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

        private bool IsSingleInstance(string filename)
        {
            string singleInstanceMutexName;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] hash = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(filename));

                singleInstanceMutexName = "TiddlyWikiWatcher." + BitConverter.ToString(hash).Replace("-", string.Empty);
            }

            bool firstInstance = false;
            singleInstanceMutex = new Mutex(true, singleInstanceMutexName, out firstInstance);
            if (firstInstance)
            {
                return true;
            }

            singleInstanceMutex.Dispose();
            singleInstanceMutex = null;

            return false;
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
        }

        private void webView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            webView.CoreWebView2.DownloadStarting += webView_DownloadStarting;
        }

        private void webView_DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            var download = e.DownloadOperation;

            lock (_downloadsBusy)
            {
                _downloadsBusy.Add(download);
            }

            download.StateChanged += delegate (object _sender, Object ev)
            {
                webView.CoreWebView2.CloseDefaultDownloadDialog();

                bool done = false;
                switch(download.State)
                {
                    case CoreWebView2DownloadState.Completed:
                        _downloadHandler.AddFile(download.ResultFilePath);
                        done = true;
                        break;

                    case CoreWebView2DownloadState.Interrupted:
                        done = true;
                        break;
                }

                if (done)
                {
                    lock (_downloadsBusy)
                    {
                        for (int i = 0; i < _downloadsBusy.Count; i++)
                        {
                            var busy = _downloadsBusy[i];

                            if (busy == download)
                            {
                                _downloadsBusy.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }; 
        }
    }
}

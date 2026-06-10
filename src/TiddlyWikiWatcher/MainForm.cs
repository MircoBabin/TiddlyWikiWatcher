using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace TiddlyWikiWatcher
{
    public partial class MainForm : Form, ITiddlyWikiWatcherSaveAs
    {
        private const string FormTitle = "Tiddly Wiki Watcher";

        private string singleInstanceMutexName;
        private Mutex singleInstanceMutex;
        private bool _watching = false;
        private DownloadedFileHandler _downloadHandler;
        private List<CoreWebView2DownloadOperation> _downloadsBusy = new List<CoreWebView2DownloadOperation>();

        private LogForm _logForm;

        private class NativeMethods
        {
            // P/Invoke constants
            public const int WM_SYSCOMMAND = 0x112;
            public const int MF_STRING = 0x0;
            public const int MF_SEPARATOR = 0x800;

            // P/Invoke declarations
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);
        }
        // ID for the About item on the system menu
        private int SYSMENU_SHOWLOG = 0x1;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Get a handle to a copy of this form's system (window) menu
            IntPtr hSysMenu = NativeMethods.GetSystemMenu(this.Handle, false);

            // Add a separator
            NativeMethods.AppendMenu(hSysMenu, NativeMethods.MF_SEPARATOR, 0, string.Empty);

            // Add the Show log
            NativeMethods.AppendMenu(hSysMenu, NativeMethods.MF_STRING, SYSMENU_SHOWLOG, "Show log");
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // Test if the Show log was selected from the system menu
            if ((m.Msg == NativeMethods.WM_SYSCOMMAND) && ((int)m.WParam == SYSMENU_SHOWLOG))
            {
                _logForm.Visible = true;
                _logForm.Focus();
            }
        }


        private MainForm()
        {
            InitializeComponent();

            _downloadHandler = null;

            this.Text = FormTitle;
            FilenameTextbox.Text = AppSettings_LoadFilename();

            webView.Visible = false;
            webView.Location = new System.Drawing.Point(0, 0);
            webView.Size = this.ClientSize - new System.Drawing.Size(webView.Location);

            WindowState = FormWindowState.Maximized;

            _logForm = new LogForm();

            // bugfix for System.InvalidOperationException: 'Invoke of BeginInvoke kan niet op een besturingselement worden aangeroepen tot de vensterkoppeling is gemaakt.'
            _logForm.Visible = true;
            _logForm.Visible = false;
        }

        public MainForm(string filename) : this()
        {
            // Check if WebView2 is installed
            try
            {
                CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (Exception ex)
            {
                // WebView2 runtime is not installed or there is some other issue with it
                var result = CustomDialogBox.Show(this.Text,
                    "Microsoft WebView2 is not installed.\n" +
                    "Use the install button to download the Microsoft WebView2 Evergreen installer.\n" +
                    "\n" +
                    "Details: " + ex.Message,
                    CustomDialogBox.Result.Button2,
                    "Install WebView2", "Exit");
                if (result == CustomDialogBox.Result.Button1)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "https://go.microsoft.com/fwlink/p/?LinkId=2124703",
                            UseShellExecute = true,
                        });
                    }
                    catch
                    {
                    }
                }

                Application.Exit();
                Environment.Exit(0);
            }

            if (!String.IsNullOrEmpty(filename))
            {
                FilenameTextbox.Text = filename;

                try
                {
                    Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                if (!_watching)
                {
                    Application.Exit();
                    Environment.Exit(0);
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

        private bool FormClose_TiddlyWikiIsNotDirty = false;
        private async void FormCloseIfTiddlyWikiIsNotDirty()
        {
            var result = await webView.CoreWebView2.ExecuteScriptAsync("(document.body.classList.contains('tc-dirty') ? 1 : 0)");
            if (result != "0")
            {
                this.Invoke((MethodInvoker)delegate
                {
                    var button = CustomDialogBox.Show(this.Text,
                        "The Tiddly Wiki has unsaved changes.\n" +
                        "- Cancel and then manually save the changes.\n" +
                        "- Or discard all unsaved changes.",
                        CustomDialogBox.Result.Button2,
                        "Exit - discard all unsaved changes", "Cancel"
                        );

                    if (button == CustomDialogBox.Result.Button1)
                    {
                        FormClose_TiddlyWikiIsNotDirty = true;
                        this.Close(); // calls MainForm_FormClosing again!
                    }
                });
                return;
            }

            FormClose_TiddlyWikiIsNotDirty = true;
            this.Invoke((MethodInvoker)delegate
            {
                this.Close(); // calls MainForm_FormClosing again!
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var notDirty = FormClose_TiddlyWikiIsNotDirty;
            FormClose_TiddlyWikiIsNotDirty = false;

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

            if (_watching)
            {
                if (!notDirty)
                {
                    e.Cancel = true;
                    FormCloseIfTiddlyWikiIsNotDirty();
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

        private async void Open()
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
            AppSettings_SaveFilename(filename);

            FilenameLabel.Visible = false;
            FilenameTextbox.Visible = false;
            FilenameTextbox.Enabled = false;
            FilenameSelect.Visible = false;
            FilenameSelect.Enabled = false;
            FilenameOpen.Visible = false;
            FilenameOpen.Enabled = false;

            _logForm.TiddlyWikiWatcher_Log("Open Tiddly Wiki file " + filename);
            this.Text = FormTitle + " - " + filename;
            webView.Visible = true;
            webView.Size = this.ClientSize - new System.Drawing.Size(webView.Location);

            // var downloadsPath = KnownFolderPaths.KnownFolders.GetPath(KnownFolderPaths.KnownFolder.Downloads);
            _watching = true;
            _downloadHandler = new DownloadedFileHandler(filename, this, _logForm);

            var webViewUserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), singleInstanceMutexName);
            if (Directory.Exists(webViewUserDataFolder))
            {
                try
                {
                    Directory.Delete(webViewUserDataFolder, true);
                }
                catch { }
            }
            if (!Directory.Exists(webViewUserDataFolder))
            {
                Directory.CreateDirectory(webViewUserDataFolder);
            }

            var webViewEnvironment = await CoreWebView2Environment.CreateAsync(null, webViewUserDataFolder);
            await webView.EnsureCoreWebView2Async(webViewEnvironment);

            webView.Source = new System.Uri(filename);
        }

        private bool IsSingleInstance(string filename)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] hash = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(filename.ToLowerInvariant()));

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

        public string TiddlyWikiWatcher_SaveAs(string tiddlyWikiFullpath, string fullpath)
        {
            return (string) this.Invoke((Func<string>)delegate 
            {
                var nameAndExtension = Path.GetFileName(fullpath).Trim();
                var extension = Path.GetExtension(fullpath).Trim();

                var filter = "All files (*.*)|*.*";
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    filter = 
                        extension.Substring(1) + " files (*" + extension + ")|*" + extension + "|" +
                        filter;
                }

                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Title = "Save file as";
                dialog.FileName = nameAndExtension;
                dialog.InitialDirectory = Path.GetDirectoryName(tiddlyWikiFullpath);
                dialog.Filter = filter;
                dialog.OverwritePrompt = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }

                return dialog.FileName;
            });
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            webView.Size = this.ClientSize - new System.Drawing.Size(webView.Location);
        }

        private void webView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            webView.CoreWebView2.DownloadStarting += webView_DownloadStarting;
            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = e.Uri,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            };
            Process.Start(psi);
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
                switch (download.State)
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

using System;
using System.Windows.Forms;

namespace TiddlyWikiWatcher
{
    public partial class LogForm : Form, ITiddlyWikiWatcherLogger
    {
        public LogForm()
        {
            InitializeComponent();

            loggingListbox.Location = new System.Drawing.Point(0, 0);
            loggingListbox.Size = this.ClientSize;
        }

        private void LogForm_Resize(object sender, EventArgs e)
        {
            loggingListbox.Size = this.ClientSize;
        }

        private void LogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }

        public void TiddlyWikiWatcher_Log(string text)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (text.Length == 0)
                        loggingListbox.Items.Add("");
                    else
                        loggingListbox.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text);

                    if (loggingListbox.Items.Count > 1000)
                    {
                        loggingListbox.Items.RemoveAt(0);
                    }

                    loggingListbox.SelectedIndex = loggingListbox.Items.Count - 1;
                });
            }
            catch { }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TiddlyWikiWatcher
{
    public class DownloadedFileHandler : IDisposable, ITiddlyWikiWatcherSaveAs, ITiddlyWikiWatcherLogger
    {
        private bool _terminate = false;
        private List<string> _fileQueue = new List<string>();
        private Thread _thread = null;
        private Semaphore _threadContinue = null;
        private volatile bool _threadIsBusy = false;

        private ITiddlyWikiWatcherLogger _logger;
        private ITiddlyWikiWatcherSaveAs _saveAs;

        private string _tiddlyWikiFullpath;
        private string _tiddlyWikiFilenameWithoutExtension;
        private string _tiddlyWikiExtension;

        public DownloadedFileHandler(string tiddlyWikiFullpath, ITiddlyWikiWatcherSaveAs saveAs, ITiddlyWikiWatcherLogger logger)
        {
            _tiddlyWikiFullpath = tiddlyWikiFullpath;
            _tiddlyWikiFilenameWithoutExtension = Path.GetFileNameWithoutExtension(tiddlyWikiFullpath);
            _tiddlyWikiExtension = Path.GetExtension(tiddlyWikiFullpath);

            _saveAs = (saveAs != null ? saveAs : this);
            _logger = (logger != null ? logger : this);

            _threadContinue = new Semaphore(0, int.MaxValue);
            _thread = new Thread(new ThreadStart(Main));
            _thread.Start();
        }

        public string TiddlyWikiWatcher_SaveAs(string tiddlyWikiFullpath, string fullpath)
        {
            //null saveAs, don't save anything
            return null;
        }

        public void TiddlyWikiWatcher_Log(string text)
        {
            //null logger, don't log anything
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~DownloadedFileHandler()
        {
            Dispose(false);
        }

        protected bool _isDisposed = false;
        protected void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            _isDisposed = true;

            /*
            if (disposing)
            {
            }
            */

            _terminate = true;
            _threadContinue.Release();

            _logger = null;
        }

        public bool IsBusy()
        {
            lock (_fileQueue)
            {
                if (_fileQueue.Count != 0) return true;
                if (_threadIsBusy) return true;

                return false;
            }
        }

        public void AddFile(string FullPath)
        {
            //Put in _fileQueue, and handle in thread. So watcher can continue
            lock (_fileQueue)
            {
                _fileQueue.Add(FullPath);
            }
            _threadContinue.Release(1);
        }

        private void Main()
        {
            while (true)
            {
                _threadContinue.WaitOne();
                if (_terminate) break;

                _threadIsBusy = true;
                try
                {
                    string fullpath = null;
                    lock (_fileQueue)
                    {
                        if (_fileQueue.Count > 0)
                        {
                            fullpath = _fileQueue[0];
                            _fileQueue.RemoveAt(0);
                        }
                    }

                    if (fullpath != null) HandleFile(fullpath);
                }
                catch { }
                _threadIsBusy = false;
            }
        }

        private void HandleFile(string fullpath)
        {
            string error;

            _logger.TiddlyWikiWatcher_Log("Downloaded file: " + fullpath);

            if (!File.Exists(fullpath))
            {
                _logger.TiddlyWikiWatcher_Log("    Skip, file does not exist");
            }


            error = CheckForTiddlyWikiFile(fullpath);
            if (String.IsNullOrEmpty(error))
            {
                _logger.TiddlyWikiWatcher_Log("    Handle the main TiddlyWiki file.");
                HandleTiddlyWikiFile(fullpath);
                return;
            }
            _logger.TiddlyWikiWatcher_Log("    Not the main TiddlyWiki file. " + error.Trim());

            _logger.TiddlyWikiWatcher_Log("    Handle generic file download.");
            HandleGenericFile(fullpath);
        }

        private string CheckForTiddlyWikiFile(string fullpath)
        {
            var name = Path.GetFileNameWithoutExtension(fullpath);
            var extension = Path.GetExtension(fullpath);
            if (extension != _tiddlyWikiExtension)
            {
                return "    Skip, extension \"" + extension + "\" should be \"" + _tiddlyWikiExtension + "\".";
            }
            if (name != _tiddlyWikiFilenameWithoutExtension)
            {
                if (!name.StartsWith(_tiddlyWikiFilenameWithoutExtension))
                {
                    return "    Skip, name \"" + name + "\" should start with \"" + _tiddlyWikiFilenameWithoutExtension + "\".";
                }

                // (1), (2) etc.
                var name1 = name.Substring(_tiddlyWikiFilenameWithoutExtension.Length).Trim();
                if (name1.Length > 0)
                {
                    if (name1[0] != '(')
                    {
                        return "    Skip, name suffix \"" + name1 + "\" should start with \"(\".";
                    }
                    if (name1[name1.Length - 1] != ')')
                    {
                        return "    Skip, name suffix \"" + name1 + "\" should end with \")\".";
                    }
                }
            }

            return String.Empty;
        }

        private void HandleTiddlyWikiFile(string fullpath)
        {
            try
            {
                if (File.Exists(_tiddlyWikiFullpath))
                {
                    var backupfile = _tiddlyWikiFullpath + ".bak";

                    _logger.TiddlyWikiWatcher_Log("    Make backup of original Tiddly Wiki file to " + backupfile);
                    Retried_FileDelete(backupfile, 30);
                    Retried_FileMove(_tiddlyWikiFullpath, backupfile, 30);
                }

                _logger.TiddlyWikiWatcher_Log("    Move downloaded file to original Tiddly Wiki file " + _tiddlyWikiFullpath);
                Retried_FileMove(fullpath, _tiddlyWikiFullpath, 30);

                _logger.TiddlyWikiWatcher_Log("    Done");
            }
            catch (Exception ex)
            {
                _logger.TiddlyWikiWatcher_Log("    ERROR: " + ex.Message);
            }
        }

        private void HandleGenericFile(string fullpath)
        {
            try
            {
                var saveAs = _saveAs.TiddlyWikiWatcher_SaveAs(_tiddlyWikiFullpath, fullpath);
                if (string.IsNullOrEmpty(saveAs))
                {
                    _logger.TiddlyWikiWatcher_Log("    Save-as is canceled.");
                    return;
                }

                if (File.Exists(saveAs))
                {
                    _logger.TiddlyWikiWatcher_Log("    Delete existing file " + saveAs);
                    Retried_FileDelete(saveAs, 30);
                }

                _logger.TiddlyWikiWatcher_Log("    Move download to " + saveAs);
                Retried_FileMove(fullpath, saveAs, 30);

                _logger.TiddlyWikiWatcher_Log("    Done");
            }
            catch (Exception ex)
            {
                _logger.TiddlyWikiWatcher_Log("    ERROR: " + ex.Message);
            }
        }

        private void Retried_FileDelete(string fullpath, int seconds)
        {
            //Retried, because file could be in use by virusscanner or something
            var StartTime = DateTime.Now;
            while (File.Exists(fullpath))
            {
                try
                {
                    File.Delete(fullpath);
                    break;
                }
                catch
                {
                    if (_terminate || DateTime.Now.Subtract(StartTime).Seconds > seconds) throw;
                }

                Thread.Sleep(1000);
            }
        }

        private void Retried_FileMove(string from, string to, int seconds)
        {
            //Retried, because file could be in use by virusscanner or something
            var StartTime = DateTime.Now;
            while (true)
            {
                try
                {
                    File.Move(from, to);
                    break;
                }
                catch
                {
                    if (_terminate || DateTime.Now.Subtract(StartTime).Seconds > seconds) throw;
                }

                Thread.Sleep(1000);
            }
        }
    }
}

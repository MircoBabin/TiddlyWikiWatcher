﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TiddlyWikiWatcher
{
    public class DownloadedFileHandler : IDisposable, IDownloadsWatcherHandler
    {
        private bool _terminate = false;
        private List<string> _fileQueue = new List<string>();
        private Thread _thread = null;
        private Semaphore _threadContinue = null;

        private ITiddlyWikiWatcherLogger _logger;

        private string _tiddlyWikiFullpath;
        private string _tiddlyWikiFilenameWithoutExtension;
        private string _tiddlyWikiExtension;

        public DownloadedFileHandler(string tiddlyWikiFullpath, string tiddlyWikiFilenameWithoutExtension, string tiddlyWikiExtension, ITiddlyWikiWatcherLogger logger)
        {
            _tiddlyWikiFullpath = tiddlyWikiFullpath;
            _tiddlyWikiFilenameWithoutExtension = tiddlyWikiFilenameWithoutExtension;
            _tiddlyWikiExtension = tiddlyWikiExtension;
            _logger = logger;

            _threadContinue = new Semaphore(0, int.MaxValue);
            _thread = new Thread(new ThreadStart(Main));
            _thread.Start();
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

        public void DownloadsWatcher_HandleFile(string FullPath)
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
            }
        }

        private void HandleFile(string fullpath)
        {
            try
            {
                _logger.TiddlyWikiWatcher_Log("Downloaded file: " + fullpath);
                string error = CheckForTiddlyWikiFile(fullpath);
                if (!String.IsNullOrEmpty(error))
                {
                    _logger.TiddlyWikiWatcher_Log(error);
                    return;
                }

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

        private string CheckForTiddlyWikiFile(string fullpath)
        {
            if (!File.Exists(fullpath))
            {
                return "    Skip, file does not exist";
            }

            var name = Path.GetFileNameWithoutExtension(fullpath);
            var extension = Path.GetExtension(fullpath);
            if (extension != _tiddlyWikiExtension)
            {
                return "    Skip, extension \"" + extension + "\" should be \"" + _tiddlyWikiExtension + "\"";
            }
            if (name != _tiddlyWikiFilenameWithoutExtension)
            {
                if (!name.StartsWith(_tiddlyWikiFilenameWithoutExtension))
                {
                    return "    Skip, name \"" + name + "\" should start with \"" + _tiddlyWikiFilenameWithoutExtension + "\"";
                }

                // (1), (2) etc.
                var name1 = name.Substring(_tiddlyWikiFilenameWithoutExtension.Length).Trim();
                if (name1.Length > 0)
                {
                    if (name1[0] != '(')
                    {
                        return "    Skip, name suffix \"" + name1 + "\" should start with \"(\"";
                    }
                    if (name1[name1.Length - 1] != ')')
                    {
                        return "    Skip, name suffix \"" + name1 + "\" should end with \")\"";
                    }
                }
            }

            return String.Empty;
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

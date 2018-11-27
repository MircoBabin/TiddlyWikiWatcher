using System;
using System.IO;

namespace TiddlyWikiWatcher
{
    public class DownloadsWatcher : IDisposable
    {
        private FileSystemWatcher _watcher;
        private IDownloadsWatcherHandler _handler;

        public DownloadsWatcher(string downloadsPath, string filter, IDownloadsWatcherHandler handler)
        {
            _handler = handler;

            _watcher = new FileSystemWatcher();
            _watcher.Path = downloadsPath;
            _watcher.EnableRaisingEvents = true;
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            _watcher.Filter = filter;
            _watcher.Created += new FileSystemEventHandler(Watcher_OnCreated);
            _watcher.Renamed += new RenamedEventHandler(Watcher_OnRenamed);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~DownloadsWatcher()
        {
            Dispose(false);
        }

        private bool _isDisposed = false;
        public void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (disposing)
            {
                _watcher.Dispose();
            }

            _handler = null;
        }

        private void Watcher_OnCreated(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created) return;

            _handler.DownloadsWatcher_HandleFile(e.FullPath);
        }

        private void Watcher_OnRenamed(object source, RenamedEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Renamed) return;

            _handler.DownloadsWatcher_HandleFile(e.FullPath);
        }
    }
}

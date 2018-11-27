using System;
using System.IO;

namespace TiddlyWikiWatcher
{
    public class TiddlyWikiWatcher : ITiddlyWikiWatcher
    {
        private DownloadedFileHandler _handler = null;
        private DownloadsWatcher _watcher = null;

        public void Start(string tiddlyWikiFullpath, string downloadsPath, ITiddlyWikiWatcherLogger logger)
        {
            if (_watcher != null)
                throw new Exception("Already started, can't start again");

            string tiddlyWikiFilenameWithoutExtension = Path.GetFileNameWithoutExtension(tiddlyWikiFullpath);
            string tiddlyWikiExtension = Path.GetExtension(tiddlyWikiFullpath);

            //Glue watcher and handler together. Handler on seperate thread, to make watching fast.
            _handler = new DownloadedFileHandler(tiddlyWikiFullpath, tiddlyWikiFilenameWithoutExtension, tiddlyWikiExtension,
                logger);

            _watcher = new DownloadsWatcher(downloadsPath, tiddlyWikiFilenameWithoutExtension + "*" + tiddlyWikiExtension,
                _handler);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~TiddlyWikiWatcher()
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
                _handler.Dispose();
            }
        }
    }
}

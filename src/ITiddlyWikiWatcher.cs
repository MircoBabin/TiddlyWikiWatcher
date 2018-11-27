using System;

namespace TiddlyWikiWatcher
{
    public interface ITiddlyWikiWatcher : IDisposable
    {
        void Start(string tiddlyWikiFullpath, string downloadsPath, ITiddlyWikiWatcherLogger logger);
    }
}

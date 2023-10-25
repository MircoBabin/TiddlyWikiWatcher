namespace TiddlyWikiWatcher
{
    public interface ITiddlyWikiWatcherLogger
    {
        void TiddlyWikiWatcher_Log(string text); //Should be thread safe, can be called from a background thread
    }
}

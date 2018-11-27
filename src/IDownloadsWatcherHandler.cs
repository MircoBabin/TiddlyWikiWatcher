namespace TiddlyWikiWatcher
{
    public interface IDownloadsWatcherHandler
    {
        void DownloadsWatcher_HandleFile(string FullPath);
    }
}

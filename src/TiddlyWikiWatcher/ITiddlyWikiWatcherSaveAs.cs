namespace TiddlyWikiWatcher
{
    public interface ITiddlyWikiWatcherSaveAs
    {
        string TiddlyWikiWatcher_SaveAs(string tiddlyWikiFullpath, string fullpath); //Should be thread safe, can be called from a background thread
    }
}

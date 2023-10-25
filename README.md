[![Github All Releases](https://img.shields.io/github/downloads/MircoBabin/TiddlyWikiWatcher/total)](https://github.com/MircoBabin/TiddlyWikiWatcher/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/MircoBabin/TiddlyWikiWatcher/blob/master/LICENSE.md)

# Tiddly Wiki Watcher
Opens the [Tiddly Wiki](https://tiddlywiki.com/ "Tiddly Wiki") in Microsoft WebView2 and watches it being saved. 
And then moves the download automatically to the original filename.

![Screenshot](screenshot.png)

# Download binary
For Windows (.NET framework 4.7.2), [the latest version can be found here](https://github.com/MircoBabin/TiddlyWikiWatcher/releases/latest "Lastest Version").

* The 64-bits version has "x64" in the name.
* The 32-bits version has "x86" in the name.

Download the zip and unpack it somewhere on your computer. Then run TiddlyWikiWatcher.exe (or make a shortcut on your desktop to it).

*For unattended automatic installation scripts, read the section "Automatic installation scripts" lower down the page.*

# Commandline arguments
TiddlyWikiWatcher.exe "full-path-to-tiddly-wiki-file.html" {-autoopen}

The optional **-autoopen** automatically starts the provided **file** argument when TiddlyWikiWatcher.exe is started.

e.g. TiddlyWikiWatcher.exe "D:\AaRiverside\aaaDevelopment\wiki\wiki - Mirco.html" -autoopen

# Why
Previously I used the Tiddly Wiki Chrome extension to maintain my personal wiki. 
But at some point I could not scroll anymore in the extension, because of a Google Chrome update. 
So I had to come up with something, to make the save easier. At this point Tiddly Wiki Watcher was born.

# Automatic installation scripts
For unattended installation scripts the following flow can be used for the latest version:

- For 64-bits, download https://github.com/MircoBabin/TiddlyWikiWatcher/releases/latest/download/release.x64.download.zip.url-location
- For 32-bits, download https://github.com/MircoBabin/TiddlyWikiWatcher/releases/latest/download/release.x86.download.zip.url-location
2) Read the text of this file into **latest-download-url**. The file only contains an url, so the encoding is ASCII. *The encoding UTF-8 may also be used to read the file, because ASCII is UTF-8 encoding.*
3) Download the zip from the **latest-download-url** to local file **TiddlyWikiWatcher.zip**. *Each release carries the version number in the filename. To prevent not knowing the downloaded filename, download to a fixed local filename.*
4) Unpack the downloaded **TiddlyWikiWatcher.zip** into a directory of your choice. Optionally create a Windows desktop shortcut to TiddlyWikiWatcher.exe.

# Contributions
Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md "contributing") before making any contribution!

# License
[The license is MIT.](LICENSE.md "license")

The [icon](http://wlb.wikia.com/wiki/File:Wikipedia-icon.png "icon") is licensed as Attribution-ShareAlike 3.0 Unported (CC BY-SA 3.0).






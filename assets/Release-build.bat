@echo off
setlocal
cls

cd /D "%~dp0"


set sz_exe=C:\Program Files\7-Zip\7z.exe
if exist "%sz_exe%" goto build

set sz_exe=C:\Program Files (x86)\7-Zip\7z.exe
if exist "%sz_exe%" goto build

echo !!! 7-Zip 18.06 - 7z.exe not found
pause
goto:eof

:build 
echo 7-Zip 18.06: %sz_exe%

del Release_version.txt >nul 2>&1

"%~dp0..\bin\Release\x64\TiddlyWikiWatcher.exe" "--version=Release_version.txt"
set /p TiddlyWikiWatcher64ReleaseVersion=< Release_version.txt
del Release_version.txt >nul 2>&1

"%~dp0..\bin\Debug\x64\TiddlyWikiWatcher.exe" "--version=Release_version.txt"
set /p TiddlyWikiWatcher64DebugVersion=< Release_version.txt
del Release_version.txt >nul 2>&1

"%~dp0..\bin\Release\x86\TiddlyWikiWatcher.exe" "--version=Release_version.txt"
set /p TiddlyWikiWatcher32ReleaseVersion=< Release_version.txt
del Release_version.txt >nul 2>&1

"%~dp0..\bin\Debug\x86\TiddlyWikiWatcher.exe" "--version=Release_version.txt"
set /p TiddlyWikiWatcher32DebugVersion=< Release_version.txt
del Release_version.txt >nul 2>&1

if not "%TiddlyWikiWatcher64ReleaseVersion%" == "%TiddlyWikiWatcher64DebugVersion%" goto version_error
if not "%TiddlyWikiWatcher32ReleaseVersion%" == "%TiddlyWikiWatcher32DebugVersion%" goto version_error
if not "%TiddlyWikiWatcher64ReleaseVersion%" == "%TiddlyWikiWatcher32ReleaseVersion%" goto version_error
goto build_zips

:version_error
echo.
echo x64 Release version: %TiddlyWikiWatcher64ReleaseVersion%
echo x64 Debug version..: %TiddlyWikiWatcher64DebugVersion%
echo x86 Release version: %TiddlyWikiWatcher32ReleaseVersion%
echo x86 Debug version..: %TiddlyWikiWatcher32DebugVersion%
echo.
echo !!! Versions do not match.
pause
goto :eof

:append_filename
    set filenames=%filenames% "%~dp0..\bin\Release\%~1\%~2"
goto:eof

:build_zips
del /q "Release\*" >nul 2>&1

:build_zip64
echo.
echo x64 Release version: %TiddlyWikiWatcher64ReleaseVersion%
echo.
echo.


set filenames=
rem TiddlyWikiWatcher.exe filenames x64)
del Release_filenames.txt >nul 2>&1
"%~dp0..\bin\Release\x64\TiddlyWikiWatcher.exe" "--installationFilenames=Release_filenames.txt"
for /f "tokens=* delims=" %%a in (Release_filenames.txt) do call :append_filename "x64" "%%a"
del Release_filenames.txt >nul 2>&1

"%sz_exe%" a -tzip -mx7 "Release\TiddlyWikiWatcher-x64-%TiddlyWikiWatcher64ReleaseVersion%.zip" %filenames%

echo.
echo.
echo Created "Release\TiddlyWikiWatcher-x64-%TiddlyWikiWatcher64ReleaseVersion%.zip"


rem https://github.com/MircoBabin/TiddlyWikiWatcher/releases/latest/download/release.x64.download.zip.url-location
rem Don't output trailing newline (CRLF)
<NUL >"Release\release.x64.download.zip.url-location" set /p="https://github.com/MircoBabin/TiddlyWikiWatcher/releases/download/%TiddlyWikiWatcher64ReleaseVersion%/TiddlyWikiWatcher-x64-%TiddlyWikiWatcher64ReleaseVersion%.zip"

echo.
echo Created "Release\release.x64.download.zip.url-location" 
echo.

:build_zip32
echo.
echo x86 Release version: %TiddlyWikiWatcher32ReleaseVersion%
echo.
echo.


set filenames=
rem TiddlyWikiWatcher.exe filenames x64)
del Release_filenames.txt >nul 2>&1
"%~dp0..\bin\Release\x86\TiddlyWikiWatcher.exe" "--installationFilenames=Release_filenames.txt"
for /f "tokens=* delims=" %%a in (Release_filenames.txt) do call :append_filename "x86" "%%a"
del Release_filenames.txt >nul 2>&1

"%sz_exe%" a -tzip -mx7 "Release\TiddlyWikiWatcher-x86-%TiddlyWikiWatcher32ReleaseVersion%.zip" %filenames%

echo.
echo.
echo Created "Release\TiddlyWikiWatcher-x86-%TiddlyWikiWatcher32ReleaseVersion%.zip"


rem https://github.com/MircoBabin/TiddlyWikiWatcher/releases/latest/download/release.x86.download.zip.url-location
rem Don't output trailing newline (CRLF)
<NUL >"Release\release.x86.download.zip.url-location" set /p="https://github.com/MircoBabin/TiddlyWikiWatcher/releases/download/%TiddlyWikiWatcher32ReleaseVersion%/TiddlyWikiWatcher-x86-%TiddlyWikiWatcher32ReleaseVersion%.zip"

echo.
echo Created "Release\release.x86.download.zip.url-location" 
echo.

pause

using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using IPA.Utilities.Async;
using UnityEngine;

namespace SaberFactory
{
    public class SaberFileWatcher
    {
        private const string Filter = "*.saber";

        public bool IsWatching { get; private set; }
        private readonly DirectoryInfo _dir;

        private FileSystemWatcher _watcher;

        public SaberFileWatcher(PluginDirectories dirs)
        {
            _dir = dirs.CustomSaberDir;
        }

        public event Action<string> OnSaberUpdate;

        public void Watch()
        {
            if (_watcher is { })
            {
                StopWatching();
            }

            _watcher = new FileSystemWatcher(_dir.FullName, Filter);

            _watcher.NotifyFilter = NotifyFilters.LastWrite;

            _watcher.Changed += WatcherOnCreated;

            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;

            IsWatching = true;
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                var seconds = 0f;
                while (seconds < 10)
                {
                    if (File.Exists(e.FullPath))
                    {
                        await Task.Delay(500);
                        OnSaberUpdate?.Invoke(e.FullPath);
                        return;
                    }

                    await Task.Delay(500);
                    seconds += 0.5f;
                }
            }, System.Threading.CancellationToken.None, TaskCreationOptions.None, UnityMainThreadTaskScheduler.Default);
        }

        private IEnumerator Initiate(string filename)
        {
            var seconds = 0f;
            while (seconds < 10)
            {
                if (File.Exists(filename))
                {
                    yield return new WaitForSeconds(0.5f);
                    OnSaberUpdate?.Invoke(filename);
                    yield break;
                }

                yield return new WaitForSeconds(0.5f);
                seconds += 0.5f;
            }
        }

        public void StopWatching()
        {
            if (_watcher is null)
            {
                return;
            }

            _watcher.Changed -= WatcherOnCreated;
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
            IsWatching = false;
        }
    }
}
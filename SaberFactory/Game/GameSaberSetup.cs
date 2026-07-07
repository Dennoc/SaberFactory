using System;
using System.Linq;
using System.Threading.Tasks;
using SaberFactory.Configuration;
using SaberFactory.DataStore;
using SaberFactory.Models;
using UnityEngine;
using Zenject;

namespace SaberFactory.Game
{
    internal class GameSaberSetup : IInitializable, IDisposable
    {
        public Task SetupTask { get; private set; }
        private readonly BeatmapData _beatmapData;

        private readonly PluginConfig _config;
        private readonly MainAssetStore _mainAssetStore;

        private readonly SaberModel _oldLeftSaberModel;
        private readonly SaberModel _oldRightSaberModel;
        private readonly SaberSet _saberSet;

        private GameSaberSetup(PluginConfig config, SaberSet saberSet, MainAssetStore mainAssetStore,
            [Inject(Id = "beatmapdata")] BeatmapData beatmapData)
        {
            _config = config;
            _saberSet = saberSet;
            _mainAssetStore = mainAssetStore;
            _beatmapData = beatmapData;

            _oldLeftSaberModel = _saberSet.LeftSaber;
            _oldRightSaberModel = _saberSet.RightSaber;

            Setup();
        }

        public void Dispose()
        {
            _saberSet.LeftSaber = _oldLeftSaberModel;
            _saberSet.RightSaber = _oldRightSaberModel;
        }

        public void Initialize()
        {
        }

        public async void Setup()
        {
            SetupTask = SetupInternal();
            await SetupTask;
        }

        private async Task SetupInternal()
        {
            if (_config.RandomSaber)
            {
                await _saberSet.Randomize();
                return;
            }


        }

    }
} 
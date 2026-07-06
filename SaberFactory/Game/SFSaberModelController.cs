using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using IPA.Utilities;
using SaberFactory.Configuration;
using SaberFactory.Helpers;
using SaberFactory.Instances;
using SaberFactory.Misc;
using SaberFactory.Models;
using SiraUtil.Interfaces;
using SiraUtil.Logging;
using SiraUtil.Sabers;
using UnityEngine;
using Zenject;

namespace SaberFactory.Game
{
    internal class SfSaberModelController : SaberModelController, IColorable, IPreSaberModelInit
    {
        [InjectOptional] private readonly EventPlayer _eventPlayer = null;
        [Inject] private readonly GameSaberSetup _gameSaberSetup = null;
        [Inject] private readonly SiraLog _logger = null;
        [Inject] private readonly SaberInstance.Factory _saberInstanceFactory = null;
        [Inject] private readonly SaberSet _saberSet = null;
        [Inject] private readonly PluginConfig _pluginConfig = null;
        private Color? _saberColor;

        private SaberInstance _saberInstance;

        public void SetColor(Color color)
        {
            _saberColor = color;
            _saberInstance?.SetColor(color);
        }

        public Color Color
        {
            get => _saberColor.GetValueOrDefault();
            set => SetColor(value);
        }

        public bool PreInit(Transform parent, Saber saber)
        {
            CustomInit(parent, saber);
            return false;
        }

        public async void CustomInit(Transform parent, Saber saber)
        {

            await _gameSaberSetup.SetupTask;

            transform.SetParent(parent, false);

            var saberModel = saber.saberType == SaberType.SaberA ? _saberSet.LeftSaber : _saberSet.RightSaber;

            _saberInstance = _saberInstanceFactory.Create(saberModel);

            _saberInstance.SetParent(transform);
            var saberTrail = this.GetField<SaberTrail, SaberModelController>("_saberTrail");
            var colorManager = this.GetField<ColorManager, SaberModelController>("_colorManager");
            _saberInstance.CreateTrail(false, saberTrail);
            SetColor(_saberColor ?? colorManager.ColorForSaberType(_saberInstance.Model.SaberSlot.ToSaberType()));

            _eventPlayer?.SetPartEventList(_saberInstance.Events, saber.saberType);

            _logger.Info("Instantiated Saber");
        }
    }
}
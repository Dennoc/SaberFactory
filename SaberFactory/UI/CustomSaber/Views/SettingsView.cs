using System;
using System.Diagnostics;
using System.Threading;
using BeatSaberMarkupLanguage.Attributes;
using SaberFactory.Configuration;
using SaberFactory.Editor;
using SaberFactory.ProjectComponents;
using SaberFactory.UI.CustomSaber.CustomComponents;
using SaberFactory.UI.Lib;
using UnityEngine;
using Zenject;

namespace SaberFactory.UI.CustomSaber.Views
{
    internal class SettingsView : SubView, INavigationCategoryView
    {
        private const string ProfileUrl = "https://github.com/ToniMacaroni";
        private const string DiscordUrl = "https://github.com/ToniMacaroni/SaberFactory";
        private const string ProfileTwoUrl = "https://github.com/Dennoo11";

        [UIComponent("changelog-popup")] private readonly ChangelogPopup _changelogPopup = null;
        [UIObject("github-button")] private readonly GameObject _githubButton = null;

        [UIValue("mod-enabled")]
        private bool ModEnabled
        {
            get => _pluginConfig.Enabled;
            set
            {
                _pluginConfig.Enabled = value;
                OnPropertyChanged();
            }
        }

        [UIValue("events-enabled")]
        private bool EventsEnabled
        {
            get => _pluginConfig.EnableEvents;
            set
            {
                _pluginConfig.EnableEvents = value;
                OnPropertyChanged();
            }
        }

        [UIValue("random-sabers")]
        private bool RandomSabers
        {
            get => _pluginConfig.RandomSaber;
            set
            {
                _pluginConfig.RandomSaber = value;
                OnPropertyChanged();
            }
        }


        [Inject] private readonly PluginConfig _pluginConfig = null;
        [Inject] private readonly PluginManager _pluginManager = null;
        [Inject] private readonly EditorInstanceManager _editorInstanceManager = null;

        public ENavigationCategory Category => ENavigationCategory.Settings;

        

        public override void DidClose()
        {
            _changelogPopup.Hide();
        }

       
        [UIAction("#post-parse")]
        private async void Setup()
        {
            var release = await _pluginManager.GetNewestReleaseAsync(CancellationToken.None);
            if (release is { IsLocalNewest: false })
            {
                _githubButton.SetActive(true);
            }
        }

        [UIAction("profile-clicked")]
        private void ProfileClicked()
        {
            Process.Start(ProfileUrl);
        }

        [UIAction("discord-clicked")]
        private void DiscordClicked()
        {
            Process.Start(DiscordUrl);
        }

        [UIAction("profiletwo-clicked")]
        private void ProfileTwoClicked()
        {
            Process.Start(ProfileTwoUrl);
        }

        [UIAction("github-clicked")]
        private async void GithubClicked()
        {
            var release = await _pluginManager.GetNewestReleaseAsync(CancellationToken.None);
            if (release == null)
            {
                return;
            }

            _changelogPopup.Show(release);
        }
    }
}
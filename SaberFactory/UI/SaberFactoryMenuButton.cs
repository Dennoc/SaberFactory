using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using SiraUtil.Logging;
using SiraUtil.Tools;
using Zenject;

namespace SaberFactory.UI
{
    internal class SaberFactoryMenuButton : IInitializable, IDisposable
    {
        private readonly Editor.Editor _editor;
        private readonly SiraLog _logger;

        private readonly MenuButton _menuButton;

        private SaberFactoryMenuButton(SiraLog logger, Editor.Editor editor)
        {
            _logger = logger;
            _editor = editor;
            _menuButton = new MenuButton("Saber Factory", "Good quality sabers", OnClick);
        }

        public void Dispose()
        {
            if ((MenuButtons.Instance != null) && (BSMLParser.Instance != null))
                MenuButtons.Instance.UnregisterButton(_menuButton);
        }

        public void Initialize()
        {
            MenuButtons.Instance.RegisterButton(_menuButton);
        }

        private void OnClick()
        {
            _editor.Open();
        }
    }
}
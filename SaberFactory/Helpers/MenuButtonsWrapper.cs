using BeatSaberMarkupLanguage.MenuButtons;

namespace SaberFactory.Helpers
{
    public static class MenuButtonsWrapper
    {
        #if V_1_29_1
        public static MenuButtons Instance => MenuButtons.instance;
        #else
        public static MenuButtons Instance => MenuButtons.Instance;
        #endif
    }
}
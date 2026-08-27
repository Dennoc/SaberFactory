using BeatSaberMarkupLanguage.MenuButtons;

namespace SaberFactory.Helpers
{
    public class MenuButtonsWrapper
    {
        #if V_1_29_1
        public static MenuButtons Instance  { get; } = MenuButtons.instance;
        #else
        public static MenuButtons Instance  { get; } = MenuButtons.Instance;
        #endif
    }
}
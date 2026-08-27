using BeatSaberMarkupLanguage;

namespace SaberFactory.Helpers
{
    public class BSMLParserWrapper
    {
        #if V_1_29_1
        public static BSMLParser Instance  { get; } = BSMLParser.instance;
        #else
        public static BSMLParser Instance { get; } = BSMLParser.Instance;
        #endif
    }
}
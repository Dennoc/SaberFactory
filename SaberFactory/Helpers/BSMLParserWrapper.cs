using BeatSaberMarkupLanguage;

namespace SaberFactory.Helpers
{
    public static class BSMLParserWrapper
    {
        #if V_1_29_1
        public static BSMLParser Instance => BSMLParser.instance;
        #else
        public static BSMLParser Instance => BSMLParser.Instance;
        #endif
    }
}
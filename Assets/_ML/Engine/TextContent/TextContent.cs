
namespace ML.Engine.TextContent
{
    [System.Serializable]
    public struct TextContent
    {
        public string Chinese;

        public string English;

        public string GetText()
        {
            if (Config.language == Config.Language.Chinese)
            {
                return Chinese;
            }
            else if (Config.language == Config.Language.English)
            {
                return English;
            }
            return "";
        }

        public override string ToString()
        {
            return this.GetText();
        }

        public static implicit operator string(TextContent text)
        {
            return text.GetText();
        }
    }

}

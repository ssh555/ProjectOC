
namespace ML.Engine.TextContent
{
    [System.Serializable]
    public class TextTip
    {
        public string name;
        public TextContent description;

        public string GetDescription()
        {
            return this.description.GetText();
        }
    }
}

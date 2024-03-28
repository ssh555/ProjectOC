
namespace ML.Engine.TextContent
{
    [System.Serializable]
    public struct MultiTextContent
    {
        public TextContent[] description;
        public int index;
        public string GetDescription()
        {
            return this.description[this.index].GetText();
        }
    }

}

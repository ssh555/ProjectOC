namespace ProjectOC.DataNS
{
    public interface IDataObj
    {
        public string GetDataID();
        public bool DataEquals(IDataObj other);
        public bool DataEquals(object other)
        {
            return other is IDataObj otherData && DataEquals(otherData);
        }
    }
}
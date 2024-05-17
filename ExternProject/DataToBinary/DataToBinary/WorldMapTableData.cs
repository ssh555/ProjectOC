using ExcelToJson;


[System.Serializable]
public struct WorldMapTableData : IGenData
{
    public int index;
    public int[] mapRow;

    public bool GenData(string[] row)
    {
        if (string.IsNullOrEmpty(row[0])) { return false; }
        index = Program.ParseInt(row[0]);
        mapRow = new int[101];
        for (int i = 1; i <= 101; i++)
        {
            mapRow[i-1] = Program.ParseInt(row[i]);
        }
        return true;
    }
}
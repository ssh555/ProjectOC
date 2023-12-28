using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelToJson
{
    public interface IGenData
    {
        bool GenData(string[] row);
    }
}

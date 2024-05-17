using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ExcelToJson
{
    public class DataToBinaryManager
    {
        /// <summary>
        /// 获取Excel工作表中指定行的数据
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        public string[] GetRowData(ExcelWorksheet worksheet, int rowNumber)
        {
            int columns = worksheet.Dimension.Columns;
            string[] rowData = new string[columns];

            for (int i = 1; i <= columns; i++)
            {
                rowData[i - 1] = worksheet.Cells[rowNumber, i].Text;
            }

            return rowData;
        }

        /// <summary>
        /// 读取指定Excel表文件数据
        /// </summary>
        /// <typeparam name="T">表一行数据转化的类型</typeparam>
        /// <param name="path">Excel文件路径</param>
        /// <param name="iBeginRow">实际数据开始行</param>
        /// <param name="readRowFunc">将一行</param>
        /// <param name="iWorksheet"></param>
        /// <returns></returns>
        public List<T> ReadExcel<T>(string path, int iBeginRow, int iWorksheet = 0) where T : IGenData, new()
        {
            var data = new List<T>();
            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                // 选择工作表
                var worksheet = package.Workbook.Worksheets[iWorksheet];
                int rowCount = worksheet.Dimension.Rows;
                System.Threading.Tasks.Parallel.For(iBeginRow, rowCount + 1, (row) =>
                {
                    T d = new T();
                    if (d.GenData(GetRowData(worksheet, row)))
                    {
                        lock(data)
                        {
                            data.Add(d);
                        }
                    }
                });
                //for (int row = iBeginRow; row <= rowCount; ++row)
                //{
                //    T d = new T();
                //    if (d.GenData(GetRowData(worksheet, row)))
                //    {
                //        data.Add(d);
                //    }
                //}
            }
            return data;
        }

        public void WriteJsonFromExcel<T>(List<T> data, string path)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                string jsonData = JsonConvert.SerializeObject(data);
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(jsonData);
                writer.Flush();
            }
        }
        public void WriteJsonFromExcelForSingleData<T>(T data, string path)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                string jsonData = JsonConvert.SerializeObject(data);
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(jsonData);
                writer.Flush();
            }
        }

        ///// <summary>
        ///// 将数据写进指定的JSON文件中
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="data"></param>
        ///// <param name="path"></param>
        //public void WriteJson<T>(T[] data, string path, Formatting formatting = Formatting.None)
        //{
        //    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
        //    var json = JsonConvert.SerializeObject(data, formatting);
        //    File.WriteAllText(path, json);
        //}

        public void WriteBinaryFromExcel<T>(List<T> data, string path)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                //DataContractSerializer serializer = new DataContractSerializer(typeof(List<T>));

                //serializer.WriteObject(fs, data);

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, data);
            }
        }

        public T ReadJson<T>(string path)
        {
            string text = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<T>(text);
        }

        public void WriteBinaryFromJson<T>(T data, string path)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                //DataContractSerializer serializer = new DataContractSerializer(typeof(T));

                //serializer.WriteObject(fs, data);

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, data);
            }
        }
    }
}

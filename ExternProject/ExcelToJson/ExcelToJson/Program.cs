using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ExcelToJson
{
    class Program
    {
        /// <summary>
        /// 单个转换表的配置
        /// </summary>
        struct EJConfig
        {
            public string ExcelFilePath;
            public string JsonFilePath;
            public int IWorksheet;
            public int IBeginRow;
            public Type type;
        }

        static void Main(string[] args)
        {
            ExcelJsonManager EJMgr = new ExcelJsonManager();

            List<EJConfig> configs = new List<EJConfig>();
            // 科技树
            // to-do : JSON输出路径待修改 -> 更改为unity/assets下面的正确路径
            // ../../../Assets/_ProjectOC/Resources/Json/TableData/TechTree.json
            configs.Add(new EJConfig { ExcelFilePath = "./DataTable/Config_OC_数据表.xlsx", IBeginRow = 5, IWorksheet = 8, JsonFilePath = "./JSON/TechTree.json", type = typeof(ProjectOC.TechTree.TechPoint) });



            System.Threading.Tasks.Parallel.ForEach(configs, (config) =>
            {
                Console.WriteLine($"开始处理: {System.IO.Path.GetFullPath(config.ExcelFilePath)} -> {System.IO.Path.GetFullPath(config.JsonFilePath)}");

                MethodInfo genericMethodDefinition = typeof(ExcelJsonManager).GetMethod("ReadExcel");
                MethodInfo constructedMethod = genericMethodDefinition.MakeGenericMethod(config.type);

                var datas = constructedMethod.Invoke(EJMgr, new object[] { config.ExcelFilePath, config.IBeginRow, config.IWorksheet });

                genericMethodDefinition = typeof(ExcelJsonManager).GetMethod("WriteJson");
                constructedMethod = genericMethodDefinition.MakeGenericMethod(config.type);
                constructedMethod.Invoke(EJMgr, new object[] { datas, config.JsonFilePath, Formatting.None });

                Console.WriteLine($"转换完成: {System.IO.Path.GetFullPath(config.ExcelFilePath)} -> {System.IO.Path.GetFullPath(config.JsonFilePath)}");
            });

            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("转换完成!!!");
            Console.Read();

        }

        public static List<ML.Engine.InventorySystem.CompositeSystem.Formula> ParseFormula(string data)
        {
            // 100001,3;100002,1
            List<ML.Engine.InventorySystem.CompositeSystem.Formula> formulas = new List<ML.Engine.InventorySystem.CompositeSystem.Formula>();
            string[] sfs = data.Split(';').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            foreach(var sf in sfs)
            {
                var t = sf.Split(',');
                var f = new ML.Engine.InventorySystem.CompositeSystem.Formula();
                f.id = t[0];
                f.num = int.Parse(t[1]);

                formulas.Add(f);
            }

            return formulas;
        }
    }
}

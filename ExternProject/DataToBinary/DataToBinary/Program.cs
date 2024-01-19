using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using ProjectOC.ResonanceWheelSystem.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using static ProjectOC.InventorySystem.UI.UIInfiniteInventory;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheelUI;

namespace ExcelToJson
{
    class Program
    {
        /// <summary>
        /// 单个转换表的配置
        /// </summary>
        struct EBConfig
        {
            /// <summary>
            /// 原Excel表格的文件路径
            /// </summary>
            public string ExcelFilePath;
            /// <summary>
            /// 目标二进制数据的文件路径
            /// </summary>
            public string BinaryFilePath;
            /// <summary>
            /// 使用Excel表格的第几个WorkSheet表项
            /// 0 开始计数
            /// </summary>
            public int IWorksheet;
            /// <summary>
            /// 第几行开始是实际数据
            /// 1 开始计数
            /// </summary>
            public int IBeginRow;
            /// <summary>
            /// 转换过程中存储EXCEL表格数据的数据结构类型
            /// </summary>
            public Type type;
        }

        struct JBConfig
        {
            /// <summary>
            /// 原JSON的文件路径
            /// </summary>
            public string JsonFilePath;
            /// <summary>
            /// 目标二进制数据的文件路径
            /// </summary>
            public string BinaryFilePath;
            /// <summary>
            /// 转换过程中存储数据的数据结构类型
            /// </summary>
            public Type type;
        }

        static void Main(string[] args)
        {
            DataToBinaryManager DBMgr = new DataToBinaryManager();

            #region EXCEL
            // to-do: 在处理前或处理后需要根据Recipe表、Build表需要合成表Binary文件，用于合成系统


            // 合成表二进制文件
            // 必须是.bytes后缀
            List<EBConfig> configs = new List<EBConfig>();
            // 科技树
            // to-do : 输出路径待修改 -> 更改为unity/assets下面的正确路径
            // ../../../Assets/_ProjectOC/Resources/Json/TableData/TechTree
            //configs.Add(new EBConfig { ExcelFilePath = "./DataTable/Config_OC_数据表.xlsx", IBeginRow = 5, IWorksheet = 8, BinaryFilePath = "./BINARY/TechTree", type = typeof(ProjectOC.TechTree.TechPoint.bytes) });



            System.Threading.Tasks.Parallel.ForEach(configs, (config) =>
            {
                try
                {
                    Console.WriteLine($"开始处理: {System.IO.Path.GetFullPath(config.ExcelFilePath)} -> {System.IO.Path.GetFullPath(config.BinaryFilePath)}");

                    MethodInfo genericMethodDefinition = typeof(DataToBinaryManager).GetMethod("ReadExcel");
                    MethodInfo constructedMethod = genericMethodDefinition.MakeGenericMethod(config.type);

                    var datas = constructedMethod.Invoke(DBMgr, new object[] { config.ExcelFilePath, config.IBeginRow, config.IWorksheet });

                    genericMethodDefinition = typeof(DataToBinaryManager).GetMethod("WriteBinaryFromExcel");
                    constructedMethod = genericMethodDefinition.MakeGenericMethod(config.type);
                    constructedMethod.Invoke(DBMgr, new object[] { datas, config.BinaryFilePath });

                    Console.WriteLine($"转换完成: {System.IO.Path.GetFullPath(config.ExcelFilePath)} -> {System.IO.Path.GetFullPath(config.BinaryFilePath)}");
                }
                catch(Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: 处理异常 {System.IO.Path.GetFullPath(config.ExcelFilePath)} -> {System.IO.Path.GetFullPath(config.BinaryFilePath)} => {e.Message}");
                    Console.ResetColor();
                }
            });

            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("EXCEL转换完成!!!");


            // 1. 读入需要的EXCEL表

            // 2. 分别解析表格并将数据暂存在内存中

            // 3. 将解析完成的数据存为对应的二进制文件

            // 4. 将暂存的数据进行处理生成其他数据的二进制文件(比如合成表)

            #endregion

            #region JSON

            // ../../../Assets/_ProjectOC/Resources/Binary/TableData/TechTree
            // 必须是.bytes后缀
            List<JBConfig> jBConfigs = new List<JBConfig>();
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TableData/TechTree.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TableData/TechTree.bytes", type = typeof(ProjectOC.TechTree.TechPoint[]) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TableData/CompositionTableData.json", BinaryFilePath = "../../../Assets/_ML/MLResources/Binary/TableData/CompositionTableData.bytes", type = typeof(CompositionJsonData[]) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TableData/ItemTableData.json", BinaryFilePath = "../../../Assets/_ML/MLResources/Binary/TableData/ItemTableData.bytes", type = typeof(ItemTableJsonData[]) });


            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/PlayerUIPanel/PlayerUIPanel.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/PlayerUIPanel/PlayerUIPanel.bytes", type = typeof(TextTip[]) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/BuildingSystem/UI/Category.json", BinaryFilePath = "../../../Assets/_ML/MLResources/Binary/TextContent/BuildingSystem/UI/Category.bytes", type = typeof(TextTip[]) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/BuildingSystem/UI/Type.json", BinaryFilePath = "../../../Assets/_ML/MLResources/Binary/TextContent/BuildingSystem/UI/Type.bytes", type = typeof(TextTip[]) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/BuildingSystem/UI/KeyTip.json", BinaryFilePath = "../../../Assets/_ML/MLResources/Binary/TextContent/BuildingSystem/UI/KeyTip.bytes", type = typeof(KeyTip[]) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/InteractSystem/InteractKeyTip.json", BinaryFilePath = "../../../Assets/_ML/MLResources/Binary/TextContent/InteractSystem/InteractKeyTip.bytes", type = typeof(KeyTip[]) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/Inventory/InventoryPanel.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/Inventory/InventoryPanel.bytes", type = typeof(InventoryPanel) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/TechTree/TechPointPanel.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/TechTree/TechPointPanel.bytes", type = typeof(ProjectOC.TechTree.TechTreeManager.TPPanel) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/ResonanceWheel/ResonanceWheelPanel.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/ResonanceWheel/ResonanceWheelPanel.bytes", type = typeof(ResonanceWheelPanel) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/ResonanceWheel/ResonanceWheel_sub1.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/ResonanceWheel_sub1/ResonanceWheel_sub1.bytes", type = typeof(ResonanceWheel_sub1.ResonanceWheel_sub1Struct) });
            jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/ResonanceWheel/ResonanceWheel_sub2.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/ResonanceWheel_sub2/ResonanceWheel_sub2.bytes", type = typeof(ResonanceWheel_sub2.ResonanceWheel_sub2Struct) });
            System.Threading.Tasks.Parallel.ForEach(jBConfigs, (config) =>
            {
                try
                {
                    Console.WriteLine($"开始处理: {System.IO.Path.GetFullPath(config.JsonFilePath)} -> {System.IO.Path.GetFullPath(config.BinaryFilePath)}");

                    MethodInfo genericMethodDefinition = typeof(DataToBinaryManager).GetMethod("ReadJson");
                    MethodInfo constructedMethod = genericMethodDefinition.MakeGenericMethod(config.type);

                    var datas = constructedMethod.Invoke(DBMgr, new object[] { config.JsonFilePath });

                    genericMethodDefinition = typeof(DataToBinaryManager).GetMethod("WriteBinaryFromJson");
                    constructedMethod = genericMethodDefinition.MakeGenericMethod(config.type);
                    constructedMethod.Invoke(DBMgr, new object[] { datas, config.BinaryFilePath });

                    Console.WriteLine($"转换完成: {System.IO.Path.GetFullPath(config.JsonFilePath)} -> {System.IO.Path.GetFullPath(config.BinaryFilePath)}");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: 处理异常 {System.IO.Path.GetFullPath(config.JsonFilePath)} -> {System.IO.Path.GetFullPath(config.BinaryFilePath)} => {e.Message}");
                    Console.ResetColor();
                }
            });


            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("JSON转换完成!!!");

            #endregion

            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("按任意键关闭");
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

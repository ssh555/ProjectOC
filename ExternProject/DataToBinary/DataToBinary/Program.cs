using ML.Engine.BuildingSystem;
using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ProjectOC.TechTree;
using ProjectOC.WorkerEchoNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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
            string excelFilePath = "./DataTable/Config_OC_数据表.xlsx";
            string rootPath = "../../../Assets/_ProjectOC/OCResources/Json/TableData/";

            // 在处理前或处理后需要根据Recipe表、Build表需要合成表Binary文件，用于合成系统
            List<CompositionTableData> compositionTableDatas = new List<CompositionTableData>();
            // 1. 读入需要的EXCEL表
            List<BuildingTableData> buildingTableDatas = DBMgr.ReadExcel<BuildingTableData>(path: excelFilePath, iBeginRow: 5, iWorksheet: 4);
            List<RecipeTableData> recipeTableDatas = DBMgr.ReadExcel<RecipeTableData>(path:excelFilePath, iBeginRow:5, iWorksheet:5);
            List<ItemTableData> itemTableDatas = DBMgr.ReadExcel<ItemTableData>(path: excelFilePath, iBeginRow: 5, iWorksheet: 6);
            List<WorkerEchoTableData> workerEchoTableDatas = DBMgr.ReadExcel<WorkerEchoTableData>(path: excelFilePath, iBeginRow: 5, iWorksheet: 12);
            Dictionary<string, BuildingTableData> buildDict = new Dictionary<string, BuildingTableData>();
            // 2. 分别解析表格并将数据暂存在内存中
            foreach (BuildingTableData data in buildingTableDatas)
            {
                buildDict.Add(data.GetClassificationString(), data);
                compositionTableDatas.Add(new CompositionTableData(data));
            }

            foreach (RecipeTableData data in recipeTableDatas)
            {
                compositionTableDatas.Add(new CompositionTableData(data, itemTableDatas.Find(item => item.id == data.Product.id)));
            }
            foreach (WorkerEchoTableData data in workerEchoTableDatas)
            {
                compositionTableDatas.Add(new CompositionTableData(data));
            }
            // 3. 将解析完成的数据存为对应的二进制文件
            DBMgr.WriteJsonFromExcel(buildingTableDatas, rootPath + "Building.json");
            DBMgr.WriteJsonFromExcel(recipeTableDatas, rootPath + "Recipe.json");
            DBMgr.WriteJsonFromExcel(compositionTableDatas, rootPath + "Composition.json");
            DBMgr.WriteJsonFromExcel(workerEchoTableDatas, rootPath + "WorkerEcho.json");

            // 合成表二进制文件
            // 必须是.json后缀
            List<EBConfig> configs = new List<EBConfig>();
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 1,  BinaryFilePath = rootPath + "ProNode.json", type = typeof(ProjectOC.ProNodeNS.ProNodeTableData) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 3, BinaryFilePath = rootPath + "TechPoint.json", type = typeof(TechPoint) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 6,  BinaryFilePath = rootPath + "Item.json", type = typeof(ItemTableData) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 9,  BinaryFilePath = rootPath + "Feature.json", type = typeof(ProjectOC.WorkerNS.FeatureTableData) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 10,  BinaryFilePath = rootPath + "Effect.json", type = typeof(ProjectOC.WorkerNS.EffectTableData) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 11, BinaryFilePath = rootPath + "Skill.json", type = typeof(ProjectOC.WorkerNS.SkillTableData) });

            System.Threading.Tasks.Parallel.ForEach(configs, (config) =>
            {
                try
                {
                    Console.WriteLine($"开始处理: {System.IO.Path.GetFullPath(config.ExcelFilePath)} -> {System.IO.Path.GetFullPath(config.BinaryFilePath)}");

                    MethodInfo genericMethodDefinition = typeof(DataToBinaryManager).GetMethod("ReadExcel");
                    MethodInfo constructedMethod = genericMethodDefinition.MakeGenericMethod(config.type);

                    var datas = constructedMethod.Invoke(DBMgr, new object[] { config.ExcelFilePath, config.IBeginRow, config.IWorksheet });

                    genericMethodDefinition = typeof(DataToBinaryManager).GetMethod("WriteJsonFromExcel");
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
            #endregion

            #region JSON
            //// 必须是.json后缀
            //List<JBConfig> jBConfigs = new List<JBConfig>();
            //jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/PlayerUIPanel/PlayerUIPanel.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/PlayerUIPanel/PlayerUIPanel.json", type = typeof(TextTip[]) });
            //jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/BuildingSystem/UI/Category.json", BinaryFilePath = "../../../Assets/_ML/MLResources/Binary/TextContent/BuildingSystem/UI/Category.json", type = typeof(TextTip[]) });
            //jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/BuildingSystem/UI/Type.json", BinaryFilePath = "../../../Assets/_ML/MLResources/Binary/TextContent/BuildingSystem/UI/Type.json", type = typeof(TextTip[]) });
            //jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/BuildingSystem/UI/KeyTip.json", BinaryFilePath = "../../../Assets/_ML/MLResources/Binary/TextContent/BuildingSystem/UI/KeyTip.json", type = typeof(KeyTip[]) });
            //jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/InteractSystem/InteractKeyTip.json", BinaryFilePath = "../../../Assets/_ML/MLResources/Binary/TextContent/InteractSystem/InteractKeyTip.json", type = typeof(KeyTip[]) });
            //jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/Inventory/InventoryPanel.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/Inventory/InventoryPanel.json", type = typeof(ProjectOC.InventorySystem.UI.UIInfiniteInventory.InventoryPanel) });
            //jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/TechTree/TechPointPanel.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/TechTree/TechPointPanel.json", type = typeof(ProjectOC.TechTree.TechTreeManager.TPPanel) });
            //jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/Store/StorePanel.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/Inventory/StorePanel.json", type = typeof(ProjectOC.InventorySystem.UI.UIStore.StorePanel) });
            //jBConfigs.Add(new JBConfig { JsonFilePath = "./Json/TextContent/ProNode/ProNodePanel.json", BinaryFilePath = "../../../Assets/_ProjectOC/Resources/Binary/TextContent/Inventory/ProNodePanel.json", type = typeof(ProjectOC.InventorySystem.UI.UIProNode.ProNodePanel) });

            //System.Threading.Tasks.Parallel.ForEach(jBConfigs, (config) =>
            //{
            //    try
            //    {
            //        Console.WriteLine($"开始处理: {System.IO.Path.GetFullPath(config.JsonFilePath)} -> {System.IO.Path.GetFullPath(config.BinaryFilePath)}");

            //        MethodInfo genericMethodDefinition = typeof(DataToBinaryManager).GetMethod("ReadJson");
            //        MethodInfo constructedMethod = genericMethodDefinition.MakeGenericMethod(config.type);

            //        var datas = constructedMethod.Invoke(DBMgr, new object[] { config.JsonFilePath });

            //        genericMethodDefinition = typeof(DataToBinaryManager).GetMethod("WriteBinaryFromJson");
            //        constructedMethod = genericMethodDefinition.MakeGenericMethod(config.type);
            //        constructedMethod.Invoke(DBMgr, new object[] { datas, config.BinaryFilePath });

            //        Console.WriteLine($"转换完成: {System.IO.Path.GetFullPath(config.JsonFilePath)} -> {System.IO.Path.GetFullPath(config.BinaryFilePath)}");
            //    }
            //    catch (Exception e)
            //    {
            //        Console.ForegroundColor = ConsoleColor.Red;
            //        Console.WriteLine($"Error: 处理异常 {System.IO.Path.GetFullPath(config.JsonFilePath)} -> {System.IO.Path.GetFullPath(config.BinaryFilePath)} => {e.Message}");
            //        Console.ResetColor();
            //    }
            //});
            //Console.WriteLine("\n----------------------------------------");
            //Console.WriteLine("JSON转换完成!!!");
            
            #endregion

            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("按任意键关闭");
            Console.Read();
        }

        public static List<ML.Engine.InventorySystem.CompositeSystem.Formula> ParseFormula(string data)
        {
            List<ML.Engine.InventorySystem.CompositeSystem.Formula> formulas = new List<ML.Engine.InventorySystem.CompositeSystem.Formula>();
            if (!string.IsNullOrEmpty(data))
            {
                string[] sfs = data.Split(';').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                foreach (var sf in sfs)
                {
                    var t = sf.Split( ',', '，' );
                    if (t.Length == 2)
                    {
                        var f = new ML.Engine.InventorySystem.CompositeSystem.Formula();
                        f.id = t[0];
                        f.num = int.Parse(t[1]);
                        formulas.Add(f);
                    }
                    else
                    {
                        Debug.Print($"Error {t}");
                    }
                }
            }
            return formulas;
        }
    }
}

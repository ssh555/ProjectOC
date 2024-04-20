using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Reflection;
using ML.Engine.InventorySystem.CompositeSystem;


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
            List<ProjectOC.WorkerEchoNS.WorkerEchoTableData> workerEchoTableDatas = DBMgr.ReadExcel<ProjectOC.WorkerEchoNS.WorkerEchoTableData>(path: excelFilePath, iBeginRow: 5, iWorksheet: 7);
            List<ML.Engine.BuildingSystem.BuildingTableData> buildingTableDatas = DBMgr.ReadExcel<ML.Engine.BuildingSystem.BuildingTableData>(path: excelFilePath, iBeginRow: 5, iWorksheet: 10);
            List<ML.Engine.InventorySystem.ItemTableData> itemTableDatas = DBMgr.ReadExcel<ML.Engine.InventorySystem.ItemTableData>(path: excelFilePath, iBeginRow: 5, iWorksheet: 16);
            List<ML.Engine.InventorySystem.RecipeTableData> recipeTableDatas = DBMgr.ReadExcel<ML.Engine.InventorySystem.RecipeTableData>(path: excelFilePath, iBeginRow: 5, iWorksheet: 18);
            Dictionary<string, ML.Engine.BuildingSystem.BuildingTableData> buildDict = new Dictionary<string, ML.Engine.BuildingSystem.BuildingTableData>();
            // 2. 分别解析表格并将数据暂存在内存中
            foreach (var data in buildingTableDatas)
            {
                buildDict.Add(data.GetClassificationString(), data);
                compositionTableDatas.Add(new CompositionTableData(data));
            }

            foreach (var data in recipeTableDatas)
            {
                compositionTableDatas.Add(new CompositionTableData(data, itemTableDatas.Find(item => item.id == data.Product.id)));
            }
            foreach (var data in workerEchoTableDatas)
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
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 1, BinaryFilePath = rootPath + "ProNode.json", type = typeof(ProjectOC.ProNodeNS.ProNodeTableData) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 2, BinaryFilePath = rootPath + "StoreIcon.json", type = typeof(ProjectOC.StoreNS.StoreIconTableData) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 11, BinaryFilePath = rootPath + "FurnitureTheme.json", type = typeof(ML.Engine.BuildingSystem.FurnitureThemeTableData) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 12, BinaryFilePath = rootPath + "TechPoint.json", type = typeof(ProjectOC.TechTree.TechPoint) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 15, BinaryFilePath = rootPath + "Order.json", type = typeof(ProjectOC.Order.OrderTableData) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 16, BinaryFilePath = rootPath + "Item.json", type = typeof(ML.Engine.InventorySystem.ItemTableData) });
            configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 17, BinaryFilePath = rootPath + "ItemCategory.json", type = typeof(ML.Engine.InventorySystem.ItemCategoryTableData) });

            //configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 9, BinaryFilePath = rootPath + "Feature.json", type = typeof(ProjectOC.WorkerNS.FeatureTableData) });
            //configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 17, BinaryFilePath = rootPath + "Effect.json", type = typeof(ProjectOC.WorkerNS.EffectTableData) });
            //configs.Add(new EBConfig { ExcelFilePath = excelFilePath, IBeginRow = 5, IWorksheet = 11, BinaryFilePath = rootPath + "Skill.json", type = typeof(ProjectOC.WorkerNS.SkillTableData) });

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
                catch (Exception e)
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

        #region Parse
        public static string ParseString(string data, string defaultValue = "")
        {
            if (!string.IsNullOrEmpty(data))
            {
                return data.Trim();
            }
            return defaultValue;
        }

        public static int ParseInt(string data, int defaultValue = 0)
        {
            if (!string.IsNullOrEmpty(data) && int.TryParse(data.Trim(), out int result))
            {
                return result;
            }
            return defaultValue;
        }

        public static TextContent ParseTextContent(string data, string defaultValue = "")
        {
            TextContent result = new TextContent();
            if (!string.IsNullOrEmpty(data))
            {
                result.Chinese = ParseString(data.Trim());
            }
            else
            {
                result.Chinese = defaultValue;
            }
            result.English = result.Chinese;
            return result;
        }

        public static T ParseEnum<T>(string data, string defaultValue = "None") where T : struct, Enum
        {
            if (!string.IsNullOrEmpty(data) && Enum.IsDefined(typeof(T), data.Trim()))
            {
                return (T)Enum.Parse(typeof(T), data);
            }
            // defaultValue
            if (!string.IsNullOrEmpty(data) && Enum.IsDefined(typeof(T), defaultValue))
            {
                return (T)Enum.Parse(typeof(T), defaultValue);
            }
            return default(T);
        }

        public static bool ParseBool(string data, bool defaultValue = false)
        {
            if (!string.IsNullOrEmpty(data))
            {
                if (bool.TryParse(data.Trim(), out bool resultBool))
                {
                    return resultBool;
                }
                if (int.TryParse(data.Trim(), out int resultInt))
                {
                    return resultInt != 0;
                }
            }
            return defaultValue;
        }

        public static Formula ParseFormula(string data, Formula defaultValue=new Formula())
        {
            if (!string.IsNullOrEmpty(data) && !string.IsNullOrEmpty(data.Trim()))
            {
                string[] s = data.Trim().Split(':', '：');
                if (s.Length == 2 && !string.IsNullOrEmpty(s[0].Trim()) && !string.IsNullOrEmpty(s[1].Trim()))
                {
                    Formula f = new Formula();
                    if (int.TryParse(s[1].Trim(), out f.num))
                    {
                        f.id = s[0].Trim();
                        return f;
                    }
                }
            }
            return defaultValue;
        }

        public static List<string> ParseStringList(string data)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(data))
            {
                foreach (var s in data.Trim().Split(',', '，'))
                {
                    if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(s.Trim()))
                    {
                        result.Add(s.Trim());
                    }
                }
            }
            return result;
        }

        public static List<Formula> ParseFormulaList(string data)
        {
            List<string> strings = ParseStringList(data);
            List<Formula> results = new List<Formula>();
            foreach (string s in strings)
            {
                Formula result = ParseFormula(s);
                if (!string.IsNullOrEmpty(result.id))
                {
                    results.Add(result);
                }
            }
            return results;
        }

        public static List<ProjectOC.Order.OrderMap> ParseOrderMap(string data)
        {
            List<ProjectOC.Order.OrderMap> ordermaps = new List<ProjectOC.Order.OrderMap>();
            if (!string.IsNullOrEmpty(data))
            {
                foreach (var s in data.Trim().Split(',', '，'))
                {
                    if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(s.Trim()))
                    {
                        var t = s.Split(':', '：');
                        if (t.Length == 2 && !string.IsNullOrEmpty(t[0].Trim()) && !string.IsNullOrEmpty(t[1].Trim()))
                        {
                            var f = new ProjectOC.Order.OrderMap();
                            if (int.TryParse(t[1].Trim(), out f.num))
                            {
                                f.id = t[0].Trim();
                                ordermaps.Add(f);
                            }
                        }
                    }
                }
            }
            return ordermaps;
        }

        public static List<T> ParseEnumList<T>(string data, string defaultValue = "None") where T : struct, Enum
        {
            List<string> strings = ParseStringList(data);
            List<T> result = new List<T>();
            foreach (string s in strings)
            {
                result.Add(ParseEnum<T>(s, defaultValue));
            }
            return result;
        }
        #endregion
    }
}

using Hook.Common;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Reflection;

namespace Hook.HookDB
{
    public class ColNameToPropertyConveter
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _tbPrefix;
        private readonly string _colPrefix;
        private readonly bool _addSugar;
        private readonly string _file;


        public ColNameToPropertyConveter()
        {
            string configFile = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Hook.config");
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap() { ExeConfigFilename = configFile },
                ConfigurationUserLevel.None);
            if (config.AppSettings.Settings["connStr"] == null)
            {
                throw new ConfigurationErrorsException("请配置数据库连接字符串：【connStr】");
            }
            if (config.AppSettings.Settings["tbName"] == null)
            {
                throw new ConfigurationErrorsException("请配置表名：【tbName】");
            }
            _connectionString = config.AppSettings.Settings["connStr"].Value;
            _tableName = config.AppSettings.Settings["tbName"].Value;
            _tbPrefix = config.AppSettings.Settings["tbPrefix"] != null ? config.AppSettings.Settings["tbPrefix"].Value : string.Empty;
            _colPrefix = config.AppSettings.Settings["colPrefix"] != null ? config.AppSettings.Settings["colPrefix"].Value : string.Empty;
            _addSugar = config.AppSettings.Settings["addSugar"] != null ? bool.Parse(config.AppSettings.Settings["addSugar"].Value) : false;
            _file = config.AppSettings.Settings["fileName"] != null ? config.AppSettings.Settings["fileName"].Value : string.Empty;
        }

        public void Convert()
        {
            var dictionary = Converter();
            Console.WriteLine("正在输出...");
            WriteSimpleEntity(dictionary, _file, _addSugar);
        }

        private Dictionary<string, KeyValuePair<string, string>> Converter()
        {
            // 获取列名和类型
            Dictionary<string, string> colsNameAndNetType = GetColsInfo();
            // 将列名转换为驼峰格式，并保留列信息
            var keyValuePairs = new Dictionary<string, KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, string> kvp in colsNameAndNetType)
            {
                // 预处理
                string connStr = ColNamePretxUtil.RemovePrefix(kvp.Key, _colPrefix);
                // 列名转属性名
                string propertyName = ColNamePretxUtil.ConvertUnderscoreToCamelCase(connStr);

                keyValuePairs.Add(propertyName, kvp);
            }
            return keyValuePairs;
        }

        private Dictionary<string, string> GetColsInfo()
        {
            // 1. 准备数据存储
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            // 2. 创建数据库连接
            using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
            {
                // 3. 获取信息
                string query = $"SELECT TOP 0 * FROM {_tableName}";
                using (SqlCommand command = new(query, sqlConnection))
                {
                    try
                    {
                        sqlConnection.Open();
                        Console.WriteLine("数据库连接成功");
                        Console.WriteLine("正在读取数据库信息...");
                        using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
                        {
                            DataTable schemaTable = reader.GetSchemaTable();
                            if (schemaTable != null)
                            {
                                foreach (DataRow row in schemaTable.Rows)
                                {
                                    string? columnName = row["ColumnName"].ToString();
                                    Type dataType = (Type)row["DataType"];
                                    if (columnName != null)
                                    {
                                        pairs.Add(columnName, dataType.Name);
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("数据库连接失败：" + ex.Message);
                    }

                }
            }
            Console.WriteLine("信息获取完成");
            return pairs;
        }

        private void WriteSimpleEntity(Dictionary<string, KeyValuePair<string, string>> dictionary, string file, bool addSugarAttr)
        {
            if (string.IsNullOrEmpty(file))
            {
                WriteToConsole(dictionary, addSugarAttr);
            }
            else
            {
                WriteToClassFile(dictionary, file, addSugarAttr);
            }
        }

        private void WriteToConsole(Dictionary<string, KeyValuePair<string, string>> dictionary, bool addSugarAttr)
        {
            string className = ColNamePretxUtil.ConvertUnderscoreToCamelCase(ColNamePretxUtil.RemovePrefix(_tableName, _tbPrefix));
            Console.WriteLine($"public class {className} {{");
            foreach (KeyValuePair<string, KeyValuePair<string, string>> kvp in dictionary)
            {
                // 注解
                if (addSugarAttr)
                {
                    Console.WriteLine($"[SugarColumn(ColumnName = \"{kvp.Value.Key}\")]");
                }
                // 属性
                Console.WriteLine($"public {kvp.Value.Value} {kvp.Key}  {{get;set;}}");
                Console.WriteLine();
            }
            Console.WriteLine($"}}");
            Console.WriteLine("已成功输出到控制台.");
        }

        private void WriteToClassFile(Dictionary<string, KeyValuePair<string, string>> dictionary, string fileName, bool addSugarAttr)
        {
            // 输出流
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = Path.Combine(desktopPath, fileName);
            using (StreamWriter writer = new StreamWriter(path))
            {
                // 类名
                string className = ColNamePretxUtil.ConvertUnderscoreToCamelCase(ColNamePretxUtil.RemovePrefix(_tableName, _tbPrefix));
                writer.WriteLine($"public class {className} {{");
                // 属性
                foreach (KeyValuePair<string, KeyValuePair<string, string>> kvp in dictionary)
                {
                    // 注解
                    if (addSugarAttr)
                    {
                        writer.WriteLine($"[SugarColumn(ColumnName = \"{kvp.Value.Key}\")]");
                    }
                    // 属性
                    writer.WriteLine($"public {kvp.Value.Value} {kvp.Key}  {{get;set;}}");
                    writer.WriteLine();
                }
                writer.WriteLine($"}}");
            }
            Console.WriteLine("已成功输出到文件.");
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using WangSql.Abstract.Linq;
using WangSql.Abstract.Migrate;
using WangSql.Abstract.Paged;

namespace WangSql
{
    public class DbProviderOptions
    {
        public DbProviderOptions()
        {
        }

        public DbProviderOptions(string name, string connectionString, string connectionType, bool useParameterPrefixInSql, bool useParameterPrefixInParameter, string parameterPrefix, bool useQuotationInSql, bool debug)
        {
            Name = name;
            ConnectionString = connectionString;
            ConnectionType = connectionType;
            UseParameterPrefixInSql = useParameterPrefixInSql;
            UseParameterPrefixInParameter = useParameterPrefixInParameter;
            ParameterPrefix = parameterPrefix;
            UseQuotationInSql = useQuotationInSql;
            Debug = debug;
        }

        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string ConnectionType { get; set; }
        public bool UseParameterPrefixInSql { get; set; }
        public bool UseParameterPrefixInParameter { get; set; }
        public string ParameterPrefix { get; set; }
        public bool UseQuotationInSql { get; set; }
        public bool Debug { get; set; }
    }

    public class DbProvider
    {
        public DbProvider(DbProviderOptions options)
        {
            if (options.ConnectionString.Contains(","))
            {
                var cs = options.ConnectionString.Split(',').Where(x => !string.IsNullOrEmpty(x?.Trim())).ToList();
                if (cs.Count() > 1)
                {
                    ConnectionString = cs[0];
                    cs.RemoveAt(0);
                    ConnectionReadString = cs.ToArray();
                }
                else if (cs.Count() == 1)
                {
                    ConnectionString = cs[0];
                }
            }
            else
            {
                ConnectionString = options.ConnectionString;
            }

            Name = options.Name;
            //ConnectionString = connectionString;
            ConnectionType = options.ConnectionType;
            UseParameterPrefixInSql = options.UseParameterPrefixInSql;
            UseParameterPrefixInParameter = options.UseParameterPrefixInParameter;
            ParameterPrefix = options.ParameterPrefix;
            UseQuotationInSql = options.UseQuotationInSql;
            Debug = options.Debug;
        }

        public string ConnectionString { get; }
        public string[] ConnectionReadString { get; }
        public string ConnectionType { get; }
        public string Name { get; }
        public string ParameterPrefix { get; }
        public bool UseParameterPrefixInParameter { get; }
        public bool UseParameterPrefixInSql { get; }
        public bool UseQuotationInSql { get; }
        public bool Debug { get; }

        private Type _connectionType;
        public DbConnection CreateConnection(bool isReadDb)
        {
            if (_connectionType == null)
            {
                try
                {
                    _connectionType = Type.GetType(ConnectionType);
                }
                catch (Exception ex)
                {
                    throw new SqlException(ConnectionType + "加载失败：" + ex.Message);
                }
            }
            var conn = (DbConnection)Activator.CreateInstance(_connectionType);
            string connStr = ConnectionString;
            if (isReadDb)
            {
                if (ConnectionReadString == null || ConnectionReadString.Length == 0)
                {
                    connStr = ConnectionString;
                }
                else if (ConnectionReadString.Length > 1)
                {
                    Random random = new Random();
                    var dbIndex = random.Next(ConnectionReadString.Length);
                    connStr = ConnectionReadString[dbIndex];
                }
                else if (ConnectionReadString.Length == 1)
                {
                    connStr = ConnectionReadString[0];
                }
            }
            conn.ConnectionString = connStr;
            return conn;
        }

        public string FormatNameForParameter(string parameterName)
        {
            return UseParameterPrefixInParameter ? ParameterPrefix + parameterName : parameterName;
        }

        public string FormatNameForSql(string parameterName)
        {
            return UseParameterPrefixInSql ? ParameterPrefix + parameterName : parameterName;
        }

        public string FormatQuotationForSql(string parameterName)
        {
            return UseQuotationInSql ? "\"" + parameterName + "\"" : parameterName;
        }

        #region 扩展注入服务方法
        public void AddService<TService, TImplementation>() where TImplementation : TService
        {
            ServiceUtil.AddService<TService, TImplementation>(this.Name);
        }

        public void AddService(Type serviceType, Type implementationType)
        {
            ServiceUtil.AddService(this.Name, serviceType, implementationType);
        }

        public T GetService<T>()
        {
            return ServiceUtil.GetService<T>(this.Name);
        }

        public object GetService(Type serviceType)
        {
            return ServiceUtil.GetService(this.Name, serviceType);
        }
        #endregion

        #region 扩展注入表映射
        public void SetTableMaps(IList<Type> tableMaps)
        {
            EntityUtil.SetMaps(tableMaps, Name);
        }
        #endregion
    }

    public class DbProviderManager
    {
        //默认实例
        private static DbProvider _dbProvider;
        private static readonly object _obj_lock = new object();

        /// <summary>
        /// 使用静态变量注意内存溢出
        /// </summary>
        private static readonly ConcurrentDictionary<string, DbProvider> DbProviderConfigCache = new ConcurrentDictionary<string, DbProvider>();
        private static readonly int DbProviderConfigCacheSize = 100;

        /// <summary>
        /// 通过入参初始化驱动程序
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        /// <param name="connectionString"></param>
        /// <param name="connectionType"></param>
        /// <param name="useParameterPrefixInSql"></param>
        /// <param name="useParameterPrefixInParameter"></param>
        /// <param name="parameterPrefix"></param>
        /// <param name="useQuotationInSql"></param>
        /// <param name="debug"></param>
        public static void Set(DbProviderOptions options)
        {
            var dbProvider = new DbProvider(options);
            if (DbProviderConfigCache.Count > DbProviderConfigCacheSize)
            {
                DbProviderConfigCache.Clear();
            }
            if (_dbProvider == null)
            {
                lock (_obj_lock)
                {
                    if (_dbProvider == null)
                    {
                        _dbProvider = dbProvider;
                    }
                }
            }
            DbProviderConfigCache[options.Name] = dbProvider;
            //注入默认服务
            dbProvider.AddService<IMigrateProvider, DefaultMigrateProvider>();
            dbProvider.AddService<IPageProvider, DefaultPageProvider>();
        }
        /// <summary>
        /// 通过配置文件初始化驱动程序
        /// </summary>
        /// <param name="config"></param>
        public static void Set(string config)
        {
            var dbProviders = GetConfigFile(config);
            foreach (var item in dbProviders)
            {
                Set(item);
            }
        }
        /// <summary>
        /// 通过默认配置文件(appsettings.json或者app.config或者web.config)初始化驱动程序
        /// </summary>
        public static void Set()
        {
            Set(string.Empty);
        }
        /// <summary>
        /// 根据驱动名称获取驱动对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DbProvider Get(string name)
        {
            if (string.IsNullOrEmpty(name)) return Get();

            if (_dbProvider != null && _dbProvider.Name == name)
            {
                return _dbProvider;
            }
            if (DbProviderConfigCache.ContainsKey(name))
            {
                return DbProviderConfigCache[name];
            }
            throw new SqlException($"未找到数据库{name}的配置信息");
        }
        /// <summary>
        /// 获取默认驱动对象（第一个SET的对象）
        /// </summary>
        /// <returns></returns>
        public static DbProvider Get()
        {
            if (_dbProvider != null)
            {
                return _dbProvider;
            }
            if (DbProviderConfigCache.Count > 0)
            {
                return DbProviderConfigCache.First().Value;
            }
            throw new SqlException($"未找到数据库的配置信息");
        }

        private static IList<DbProviderOptions> GetConfigFile(string config)
        {
            if (string.IsNullOrEmpty(config))
            {
                var rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
                //查找appsettings.json
                if (File.Exists(Path.Combine(rootPath, "appsettings.json")))
                {
                    config = Path.Combine(rootPath, "appsettings.json");
                }
                //查找app.config
                else if (File.Exists(Path.Combine(rootPath, "app.config")))
                {
                    config = Path.Combine(rootPath, "app.config");
                }
                else if (File.Exists(Path.Combine(rootPath, "App.config")))
                {
                    config = Path.Combine(rootPath, "App.config");
                }
                //查找web.config
                else if (File.Exists(Path.Combine(rootPath, "web.config")))
                {
                    config = Path.Combine(rootPath, "web.config");
                }
                else if (File.Exists(Path.Combine(rootPath, "Web.config")))
                {
                    config = Path.Combine(rootPath, "Web.config");
                }
            }
            if (string.IsNullOrEmpty(config))
            {
                throw new SqlException($"数据库配置文件不存在");
            }
            if (!File.Exists(config))
            {
                throw new SqlException($"数据库配置文件{config}不存在");
            }
            FileInfo fileInfo = new FileInfo(config);
            var ext = fileInfo.Extension.ToLower();
            if (ext != ".config" && ext != ".xml" && ext != ".json")
            {
                throw new SqlException($"数据库配置文件格式{ext}不支持");
            }
            if (ext == ".config" || ext == ".xml")
            {
                return GetXmlConfigFile(config);
            }
            if (ext == ".json")
            {
                return GetJsonConfigFile(config);
            }
            return new List<DbProviderOptions>();
        }
        private static IList<DbProviderOptions> GetXmlConfigFile(string config)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(config);
            var root = doc.DocumentElement;
            var db = root.Name == "database" ? root : root.SelectSingleNode("database");
            if (db == null)
            {
                throw new SqlException($"配置文件{config}无database配置信息");
            }
            var providers = db.SelectNodes("dbProvider");
            if (providers == null || providers.Count == 0)
            {
                throw new SqlException($"配置文件{config}无database/dbProvider配置信息");
            }
            IList<DbProviderOptions> result = new List<DbProviderOptions>();
            foreach (XmlNode item in providers)
            {
                string name = item.SelectSingleNode("name")?.InnerText;
                string connectionString = item.SelectSingleNode("connectionString")?.InnerText;
                string connectionType = item.SelectSingleNode("connectionType")?.InnerText;
                bool useParameterPrefixInSql = StringToBool(item.SelectSingleNode("useParameterPrefixInSql")?.InnerText);
                bool useParameterPrefixInParameter = StringToBool(item.SelectSingleNode("useParameterPrefixInParameter")?.InnerText);
                string parameterPrefix = item.SelectSingleNode("parameterPrefix")?.InnerText;
                bool useQuotationInSql = StringToBool(item.SelectSingleNode("useQuotationInSql")?.InnerText);
                bool debug = StringToBool(item.SelectSingleNode("debug")?.InnerText);

                result.Add(new DbProviderOptions(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug));
            }
            return result;
        }
        private static IList<DbProviderOptions> GetJsonConfigFile(string config)
        {
            IList<DbProviderOptions> result = new List<DbProviderOptions>();
            var json = File.ReadAllText(config, Encoding.UTF8);

            //Database
            Regex regex = new Regex("Database[\\w\\W]*\\[([\\w\\W]*?)\\]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var match = regex.Match(json);
            if (match == null || match.Groups.Count < 2)
            {
                throw new SqlException($"配置文件{config}无Database配置信息");
            }
            json = match.Groups[1].Value;
            json = json.Replace("\r\n", "").Replace("\r", "").Replace("\n", "\n");
            //{}
            regex = new Regex("\\{([\\w\\W]*?)\\}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matchs = regex.Matches(json);
            if (matchs == null || matchs.Count == 0)
            {
                throw new SqlException($"配置文件{config}无Database配置信息");
            }
            foreach (Match item in matchs)
            {
                if (item.Groups.Count > 1)
                {
                    json = item.Value;
                    var sod = new JsonReader(json).Value;
                    string name = GetDictString(sod, "name");
                    string connectionString = GetDictString(sod, "connectionString");
                    string connectionType = GetDictString(sod, "connectionType");
                    bool useParameterPrefixInSql = StringToBool(GetDictString(sod, "useParameterPrefixInSql"));
                    bool useParameterPrefixInParameter = StringToBool(GetDictString(sod, "useParameterPrefixInParameter"));
                    string parameterPrefix = GetDictString(sod, "parameterPrefix");
                    bool useQuotationInSql = StringToBool(GetDictString(sod, "useQuotationInSql"));
                    bool debug = StringToBool(GetDictString(sod, "debug"));

                    result.Add(new DbProviderOptions(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug));
                }
            }

            return result;
        }
        private static bool StringToBool(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;
            if (str.Trim().ToLower() == "true" || str.Trim() == "1") return true;
            else return false;
        }
        private static string GetDictString(Dictionary<string, object> dict, string name)
        {
            if (dict.Keys.Any(op => op.ToUpper() == name.ToUpper()))
            {
                return dict.First(x => x.Key.ToUpper() == name.ToUpper()).Value?.ToString();
            }
            return null;
        }
    }

    internal class JsonReader
    {
        private TextReader _textReader = null;
        private int _endCount = 0;
        public Dictionary<string, object> Value = new Dictionary<string, object>();
        private JsonReader(TextReader reader, int endCount = 0)
        {
            Value = new Dictionary<string, object>();
            _textReader = reader;
            _endCount = endCount;
            int intByte = ReadInt();
            while (_endCount != 0 && intByte != -1)
            {
                var key = ReadKeyName(intByte);
                var value = ReadValue();
                Value.Add(key, value);
                if (_endCount == 0) { break; }
                intByte = ReadInt();
            }
        }
        public JsonReader(TextReader reader)
            : this(reader, 0)
        { }
        public JsonReader(string value)
            : this(new StringReader(value), 0)
        { }
        private int ReadInt()
        {
            var intByte = _textReader.Read();
            while (intByte == 32)
            {
                intByte = _textReader.Read();
            }
            if (intByte == 123)
            {
                _endCount++;
            }
            else if (intByte == 125)
            {
                _endCount--;
            }
            return intByte;
        }
        private string ReadKeyName(int lastChar)
        {
            StringBuilder strBuilder = new StringBuilder();
            var intByte = lastChar;
            if (intByte == 123)
            {
                intByte = _textReader.Read();
            }
            var lastIntByte = -1;
            int endByteInt = -1;
            if (intByte == -1)
            {
                return null;
            }
            while (intByte == 32)
            {
                endByteInt = intByte;
                intByte = _textReader.Read();
            }
            if (intByte == 34 || intByte == 39)
            {
                endByteInt = intByte;
                intByte = _textReader.Read();
            }
            while (intByte != -1)
            {
                if (endByteInt != -1)
                {
                    if (intByte == endByteInt && lastIntByte != 92)
                    {
                        ReadInt();
                        break;
                    }
                }
                else if (intByte == 58)
                {
                    break;
                }

                strBuilder.Append((char)intByte);
                intByte = _textReader.Read();
            }
            return strBuilder.ToString().Trim();
        }
        private object ReadValue()
        {
            var intByte = _textReader.Read();
            while (intByte == 32)
            {
                intByte = _textReader.Read();
            }
            if (intByte == 123)
            {
                var item = new JsonReader(_textReader, 1).Value;
                ReadInt();
                return item;
            }
            else if (intByte == 91)
            {
                return ReadValueArray();
            }
            else
            {
                return ReadValueString(intByte);
            }
        }
        private string ReadValueArrayString(ref int lastChar)
        {
            StringBuilder strBuilder = new StringBuilder();
            var intByte = lastChar;
            if (intByte == 123)
            {
                intByte = _textReader.Read();
            }
            var lastIntByte = -1;
            int endByteInt = -1;
            if (intByte == -1)
            {
                return null;
            }
            while (intByte == 32 || intByte == 34 || intByte == 39)
            {
                endByteInt = intByte;
                intByte = _textReader.Read();
            }
            while (intByte != -1)
            {
                lastChar = intByte;
                if (endByteInt != -1)
                {
                    if (intByte == endByteInt && lastIntByte != 92)
                    {
                        break;
                    }
                }
                else if (intByte == 44 || (intByte == 93 && lastIntByte != 92))
                {
                    break;
                }

                strBuilder.Append((char)intByte);
                intByte = ReadInt();
            }
            return strBuilder.ToString();
        }
        private object ReadValueString(int lastChar)
        {
            StringBuilder strBuilder = new StringBuilder();
            var intByte = lastChar;
            if (intByte == 123)
            {
                intByte = _textReader.Read();
            }
            var lastIntByte = -1;
            int endByteInt = -1;
            bool isString = true;
            if (intByte == -1)
            {
                return null;
            }
            while (intByte == 32)
            {
                endByteInt = intByte;
                intByte = _textReader.Read();
            }
            if (intByte == 34 || intByte == 39)
            {
                endByteInt = intByte;
                intByte = _textReader.Read();
            }
            while (intByte != -1)
            {
                if (endByteInt != -1)
                {
                    if (intByte == endByteInt && lastIntByte != 92)
                    {
                        ReadInt();
                        break;
                    }
                }
                else if (intByte == 44 || (intByte == 125 && lastIntByte != 92))
                {
                    isString = false;
                    break;
                }
                strBuilder.Append((char)intByte);
                intByte = ReadInt();
            }
            return isString ? strBuilder.ToString() : strBuilder.ToString().Trim();
        }
        private object[] ReadValueArray()
        {
            List<object> list = new List<object>();
            var intByte = _textReader.Read();
            while (intByte != 93)
            {
                if (intByte == 123)
                {
                    var item = new JsonReader(_textReader, 1).Value;
                    list.Add(item);
                    if (ReadInt() == 93)
                    {
                        break;
                    }
                }
                else if (intByte == 91)
                {
                    list.Add(ReadValueArray());
                }
                else
                {
                    list.Add(ReadValueArrayString(ref intByte));
                    if (intByte == 93) { break; }
                }
                intByte = _textReader.Read();
            }
            return list.ToArray();
        }
    }
}
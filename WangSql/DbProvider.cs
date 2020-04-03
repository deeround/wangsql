using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace WangSql
{
    public class DbProvider
    {
        /// <summary>
        /// 使用静态变量注意内存溢出
        /// </summary>
        private static readonly ConcurrentDictionary<string, Type> CacheConnectionType = new ConcurrentDictionary<string, Type>();
        private static readonly int CacheConnectionTypeLength = 1000;

        public DbProvider(
            string name,
            string connectionString,
            string connectionType,
            bool useParameterPrefixInSql,
            bool useParameterPrefixInParameter,
            string parameterPrefix,
            bool useQuotationInSql,
            bool debug)
        {
            Name = name;
            ConnectionString = connectionString;
            ConnectionType = connectionType;
            UseParameterPrefixInSql = useParameterPrefixInSql;
            UseParameterPrefixInParameter = useParameterPrefixInParameter;
            ParameterPrefix = parameterPrefix;
            UseQuotationInSql = useQuotationInSql;
            Debug = debug;
            BuildProvider = new DefaultBuildProvider();

            //oracle
            if (connectionType.ToLower().Contains("oracle"))
            {
                BuildProvider = new OracleBuildProvider();
            }
            //oracle
            else if (connectionType.ToLower().Contains("pgsql"))
            {
                BuildProvider = new PgsqlBuildProvider();
            }
        }

        public string ConnectionString { get; }
        public string ConnectionType { get; }
        public string Name { get; }
        public string ParameterPrefix { get; }
        public bool UseParameterPrefixInParameter { get; }
        public bool UseParameterPrefixInSql { get; }
        public bool UseQuotationInSql { get; }
        public bool Debug { get; }
        public IBuildProvider BuildProvider { get; }

        public IDbConnection CreateConnection()
        {
            var type = GetCacheType();
            var conn = (IDbConnection)Activator.CreateInstance(type);
            conn.ConnectionString = ConnectionString;
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

        private Type GetCacheType()
        {
            var code = Utils.GetHashCode(this);
            if (CacheConnectionType.ContainsKey(code))
            {
                return CacheConnectionType[code];
            }

            Type type;
            try
            {
                type = Type.GetType(ConnectionType);
            }
            catch (Exception ex)
            {
                throw new SqlException(ConnectionType + "加载失败：" + ex.Message);
            }

            if (CacheConnectionType.Count > CacheConnectionTypeLength)
            {
                CacheConnectionType.Clear();
            }
            CacheConnectionType[code] = type ?? throw new SqlException(ConnectionType + "加载失败");
            return type;
        }
    }

    public class DbProviderManager
    {
        //默认实例
        private static DbProvider _dbProvider;
        private static readonly object _obj_lock = new object();
        //直接实例化的集合
        private static readonly ConcurrentDictionary<string, DbProvider> DbProviderNewCache = new ConcurrentDictionary<string, DbProvider>();
        private static readonly int DbProviderNewCacheSize = 100;
        //通过配置文件生成的集合
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
        public static void Set(string name, string connectionString, string connectionType, bool useParameterPrefixInSql, bool useParameterPrefixInParameter, string parameterPrefix, bool useQuotationInSql, bool debug)
        {
            var dbProvider = new DbProvider(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug);
            if (DbProviderNewCache.Count > DbProviderNewCacheSize)
            {
                DbProviderNewCache.Clear();
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
            DbProviderNewCache[name] = dbProvider;
        }
        /// <summary>
        /// 通过配置文件初始化驱动程序
        /// </summary>
        /// <param name="config"></param>
        public static void Set(string config)
        {
            var dbProviders = GetConfigFile(config);
            if (dbProviders.Count > 0)
            {
                if (DbProviderConfigCache.Count + dbProviders.Count > DbProviderConfigCacheSize)
                {
                    DbProviderConfigCache.Clear();
                }
                foreach (var item in dbProviders)
                {
                    if (_dbProvider == null)
                    {
                        lock (_obj_lock)
                        {
                            if (_dbProvider == null)
                            {
                                _dbProvider = item;
                            }
                        }
                    }
                    DbProviderConfigCache[item.Name] = item;
                }
            }
        }
        /// <summary>
        /// 通过默认配置文件(appsettings.json或者app.config或者web.config)初始化驱动程序
        /// </summary>
        public static void Set()
        {
            Set(null);
        }
        /// <summary>
        /// 根据驱动名称获取驱动对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DbProvider Get(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return _dbProvider;
            }
            if (DbProviderNewCache.ContainsKey(name))
            {
                return DbProviderNewCache[name];
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
            return Get(null);
        }


        private static IList<DbProvider> GetConfigFile(string config)
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

            return new List<DbProvider>();
        }
        private static IList<DbProvider> GetXmlConfigFile(string config)
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
            IList<DbProvider> result = new List<DbProvider>();
            foreach (XmlNode item in providers)
            {
                string name = item.SelectSingleNode("name")?.InnerText;
                bool enabled = StringToBool(item.SelectSingleNode("enabled")?.InnerText);
                string connectionString = item.SelectSingleNode("connectionString")?.InnerText;
                string connectionType = item.SelectSingleNode("connectionType")?.InnerText;
                bool useParameterPrefixInSql = StringToBool(item.SelectSingleNode("useParameterPrefixInSql")?.InnerText);
                bool useParameterPrefixInParameter = StringToBool(item.SelectSingleNode("useParameterPrefixInParameter")?.InnerText);
                string parameterPrefix = item.SelectSingleNode("parameterPrefix")?.InnerText;
                bool useQuotationInSql = StringToBool(item.SelectSingleNode("useQuotationInSql")?.InnerText);
                bool debug = StringToBool(item.SelectSingleNode("debug")?.InnerText);

                if (!enabled) continue;
                result.Add(new DbProvider(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug));
            }
            return result;
        }
        private static IList<DbProvider> GetJsonConfigFile(string config)
        {
            IList<DbProvider> result = new List<DbProvider>();
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
                    bool enabled = StringToBool(GetDictString(sod, "enabled"));
                    string connectionString = GetDictString(sod, "connectionString");
                    string connectionType = GetDictString(sod, "connectionType");
                    bool useParameterPrefixInSql = StringToBool(GetDictString(sod, "useParameterPrefixInSql"));
                    bool useParameterPrefixInParameter = StringToBool(GetDictString(sod, "useParameterPrefixInParameter"));
                    string parameterPrefix = GetDictString(sod, "parameterPrefix");
                    bool useQuotationInSql = StringToBool(GetDictString(sod, "useQuotationInSql"));
                    bool debug = StringToBool(GetDictString(sod, "debug"));

                    if (!enabled) continue;
                    result.Add(new DbProvider(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug));
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
}
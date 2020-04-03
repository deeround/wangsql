# 快速入门

### 配置文件

 appsettings.json

``` json
{
  "Database": [
    {
      "Name": "SQLite",
      "Enabled": true,
      "ConnectionString": "Data Source=wangsql.db;",
      "ConnectionType": "System.Data.SQLite.SQLiteConnection,System.Data.SQLite",
      "UseParameterPrefixInSql": true,
      "UseParameterPrefixInParameter": true,
      "ParameterPrefix": "@",
      "UseQuotationInSql": false,
      "Debug": false
    }
  ]
}
```

或者 

app.config

``` xml
<?xml version="1.0" encoding="utf-8" ?>
<database>
  <dbProvider>
    <name>SQLite</name>
    <enabled>true</enabled>
    <connectionString>Data Source=wangsql.db;</connectionString>
    <connectionType>System.Data.SQLite.SQLiteConnection,System.Data.SQLite</connectionType>
    <useParameterPrefixInSql>true</useParameterPrefixInSql>
    <useParameterPrefixInParameter>true</useParameterPrefixInParameter>
    <parameterPrefix>@</parameterPrefix>
    <useQuotationInSql>false</useQuotationInSql>
    <debug>false</debug>
  </dbProvider>
</database>
```

### 初始化驱动

~~~ c#
/// <summary>
/// 通过默认配置文件(appsettings.json或者app.config或者web.config)初始化驱动程序
/// </summary>
DbProviderManager.Set();
~~~

或者

~~~ c#
/// <summary>
/// 通过指定文件初始化驱动程序
/// </summary>
DbProviderManager.Set("/app_database.config");
~~~

或者

~~~ c#
/// <summary>
/// 通过代码初始化驱动程序
/// </summary>
DbProviderManager.Set(
                    "SQLite",
                    "Default",
                    "Data Source=mario.db;",
                    "System.Data.SQLite.SQLiteConnection,System.Data.SQLite",
                    true,
                    true,
                    "@",
                    false,
                    false
                    );
~~~

### 创建SqlMapper对象

~~~ c#
/// <summary>
/// 创建默认对象
/// </summary>
var _sqlMapper = new SqlMapper();
~~~

或者

~~~ c#
/// <summary>
/// 根据驱动名称创建指定对象
/// </summary>
var _sqlMapper = new SqlMapper("SQLite");
~~~

### CRUD使用

##### 查询列表数据

~~~ c#
//无参数查询列表
var sql = "select * from tb_user";
var r = _sqlMapper.Query<Dictionary<string, object>>(sql, null).ToList();
~~~

~~~ c#
//有参数查询列表

//传参方式1 直接传入简单类型的值
var sql = "select * from tb_user where userid=#userid#";
var r1 = _sqlMapper.Query<Dictionary<string, object>>(sql, userid).ToList();
Dictionary<string, object>  sod = new Dictionary<string, object>()
{
    { "userid", userid }
};

//传参方式2 传入键值对
var r2 = _sqlMapper.Query<Dictionary<string, object>>(sql, sod).ToList();

//传参方式3 传入实体
UserInfo user = new UserInfo()
{
    UserId = userid
};
var r3 = _sqlMapper.Query<Dictionary<string, object>>(sql, user).ToList();
~~~

##### 查询单条数据

~~~ c#
//无参数查询单条
var sql = "select * from tb_user";
var r = _sqlMapper.QueryFirstOrDefault<Dictionary<string, object>>(sql, null);
~~~

~~~ c#
//有参数查询单条

//传参方式1 直接传入简单类型的值
var sql = "select * from tb_user where userid=#userid#";
var r1 = _sqlMapper.QueryFirstOrDefault<Dictionary<string, object>>(sql, userid);
Dictionary<string, object>  sod = new Dictionary<string, object>()
{
    { "userid", userid }
};

//传参方式2 传入键值对
var r2 = _sqlMapper.QueryFirstOrDefault<Dictionary<string, object>>(sql, sod);

//传参方式3 传入实体
UserInfo user = new UserInfo()
{
    UserId = userid
};
var r3 = _sqlMapper.QueryFirstOrDefault<Dictionary<string, object>>(sql, user);
~~~

##### 插入数据

新增同样支持查询时的三种入参类型，下面只演示其中一种传参方式。

~~~ c#
var sql = "insert into tb_user(userid,username,age) values(#userid#,#username#,#age#)";
var sod = new Dictionary<string, object>()
{
    { "userid", userid },
    { "username", username },
    { "age", age }
};
var r = _sqlMapper.Execute(sql, sod);
~~~

##### 更新数据

新增同样支持查询时的三种入参类型，下面只演示其中一种传参方式。

~~~ c#
var sql = "update tb_user set username=#username#,age=#age# where userid=#userid#";
var sod = new Dictionary<string, object>()
{
    { "userid", userid },
    { "username", username },
    { "age", age }
};
var r = _sqlMapper.Execute(sql, sod);
~~~

##### 删除数据

新增同样支持查询时的三种入参类型，下面只演示其中一种传参方式。

~~~ c#
var sql = "delete from tb_user where userid=#userid#";
var sod = new Dictionary<string, object>()
{
    { "userid", userid }
};
var r = _sqlMapper.Execute(sql, sod);
~~~


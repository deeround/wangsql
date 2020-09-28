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

//传参方式2 传入键值对
Dictionary<string, object>  sod = new Dictionary<string, object>()
{
    { "userid", userid }
};
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

//传参方式2 传入键值对
Dictionary<string, object>  sod = new Dictionary<string, object>()
{
    { "userid", userid }
};
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

##### 实体操作

使用实体实现CRUD操作。

~~~ c#
SqlMapper.Entity<UserInfo>().FirstOrDefault();
SqlMapper.Entity<UserInfo>().Where(x => x.UserId == id).FirstOrDefault();
SqlMapper.Entity<UserInfo>().ToList();
SqlMapper.Entity<UserInfo>().Where(x => x.UserId == id).ToList();
SqlMapper.Entity<UserInfo>().ToPaged(pageInfo.PageIndex, pageInfo.PageSize, out total)
SqlMapper.Entity<UserInfo>().Insert(model);
SqlMapper.Entity<UserInfo>().Set(x => new { x.UpdateUserId, x.UpdateUserName, x.UpdateDateTime }, false).Insert(model);
SqlMapper.Entity<UserInfo>().Update(model);
SqlMapper.Entity<UserInfo>().Set(x => new
                {
                    x.UserId,
                    x.UserName,
                    x.UserCode,
                    x.Email,
                    x.Mobile,
                    x.Landline,
                    x.Avatar,
                    x.UpdateUserId,
                    x.UpdateUserName,
                    x.UpdateDateTime,
                }).Update(model);
SqlMapper.Entity<UserInfo>().Delete(id);
~~~

### 表映射

~~~ c#
public class UserDataConfig : IDataConfig
    {
        public UserDataConfig()
        {
            //Fluent API
            var map = TableMap.Entity<Models.UserInfo>().ToTable("tb_sys_user", "用户", true);
            map.HasColumn(x => x.UserId).Name("UserId").Length(50).Comment("ID").IsPrimaryKey().IsNotNull();
            map.HasColumn(x => x.UserName).Name("UserName").Length(50).Comment("用户名").IsUnique().IsNotNull();
            map.HasColumn(x => x.UserCode).Name("UserCode").Length(50).Comment("用户编码").IsUnique().IsNotNull();
            map.HasColumn(x => x.UserType).Name("UserType").Length(50).Comment("用户类型").IsNotNull();
            map.IgnoreColumn(x => x.IsAdmin);
            map.HasColumn(x => x.LoginName).Name("LoginName").Length(50).Comment("登录名").IsUnique().IsNotNull();
            map.HasColumn(x => x.LoginPwd).Name("LoginPwd").Length(200).Comment("登录密码").IsNotNull();

            map.HasColumn(x => x.Email).Name("Email").Comment("电子邮箱");
            map.HasColumn(x => x.Mobile).Name("Mobile").Comment("移动电话");
            map.HasColumn(x => x.Landline).Name("Landline").Comment("座机电话");
            map.HasColumn(x => x.Avatar).Name("Avatar").DataType(SimpleStandardType.Text).Comment("头像");

            map.HasColumn(x => x.CreateDateTime).Name("CreateDateTime").Comment("创建时间");
            map.HasColumn(x => x.CreateUserId).Name("CreateUserId").Comment("创建人");
            map.HasColumn(x => x.CreateUserName).Name("CreateUserName").Comment("创建人");
            map.HasColumn(x => x.UpdateDateTime).Name("UpdateDateTime").Comment("更新时间");
            map.HasColumn(x => x.UpdateUserId).Name("UpdateUserId").Comment("更新人");
            map.HasColumn(x => x.UpdateUserName).Name("UpdateUserName").Comment("更新人");
        }
    }
~~~



### 事务

~~~ c#
using (var trans = SqlMapper.BeginTransaction())
            {
                try
                {
                    trans.Entity<UserRoleRelInfo>().Where(x => x.UserId == dto.User.UserId).Delete();
                    foreach (var item in dto.Roles)
                    {
                        trans.Entity<UserRoleRelInfo>().Insert(new UserRoleRelInfo() { UserId = dto.User.UserId, RoleId = item.RoleId });
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
~~~

### 工作单元

整个工作单元会在一个事务执行

~~~ c#
[UnitOfWork]
        public virtual IList<StrObjDict> R1()
        {
            _evnRepository.R1();
            _evnRepository.R2();
            _evnRepository.E1();
            return _evnRepository.Q1();
        }
~~~

### $和#区别

在sql语句中所有参数由##包裹，也可以由$$包裹，##包裹的参数会进行参数化处理，而$$包裹的参数会在sql处理前直接替换拼接字符串处理。

正式环境要求使用##包裹参数，对入参进行参数化处理，防止sql注入！

处理前sql

~~~ sql
select * from users where userid=#userid#
~~~

处理后sql

~~~ sql
select * from users where userid=@userid
~~~

处理前sql(注意这种方式需要自己带上单引号)

~~~ sql
select * from users where userid='$userid$'
~~~

处理后sql

~~~ sql
select * from users where userid='8F2D7A615AE94579A7CE8058EB091E9E'
~~~

